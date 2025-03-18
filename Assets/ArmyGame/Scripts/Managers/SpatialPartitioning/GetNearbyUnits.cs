using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base;
using Logic.Attributes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ArmyGame.Managers.SpatialPartitioning
{
    public class GetNearbyUnits : MonoBehaviour
    {
        [SerializeField] private float intervalS = 2f;
        [SerializeField] private GameObjectEventChannel gameObjectStateChangeChannel;
        [SerializeField] private NearbyUnitsEventChannel nearbyUnitsEventChannel;

        private NativeArray<NativeList<int>> _results;
        private NativeParallelMultiHashMap<Vector2Int, int> _readOnlyCopyPartitions;

        [SerializeField] private int maxCapacity = 10;
        private Coroutine _publishNearbyUnitsCoroutine;

        private readonly Dictionary<int, GameObject> _gameObjectToWatch = new();

        private static readonly HashSet<UnitState> WatchableStates = new()
            { UnitState.Active, UnitState.Idle, UnitState.Moving };

        private static readonly HashSet<UnitState> RemovableStates = new() { UnitState.Attacking, UnitState.InActive };


        private void OnEnable()
        {
            gameObjectStateChangeChannel.OnEventRaised += HandleGameObjectStateChange;
        }

        private void OnDisable()
        {
            gameObjectStateChangeChannel.OnEventRaised -= HandleGameObjectStateChange;
        }

        private void HandleGameObjectStateChange(GameObject go)
        {
            if (!TryGetUnitState(go, out var unitState)) return;

            var unitGoId = go.GetInstanceID();

            Debug.Log($"value of game object state being passed to handle game object {unitState}");

            if (WatchableStates.Contains(unitState))
            {
                _gameObjectToWatch.TryAdd(unitGoId, go);

                Manager.Instance.Register(unitGoId, go.transform.position);
                EnsurePublishingCoroutineIsRunning();
            }
            else if (RemovableStates.Contains(unitState) && _gameObjectToWatch.Remove(unitGoId))
            {
                Manager.Instance.Unregister(unitGoId);
                EnsurePublishingCoroutineIsStopped();
            }
        }

        private bool TryGetUnitState(GameObject go, out UnitState state)
        {
            state = default;
            if (!go.TryGetComponent<Unit>(out var unitComponent)) return false;
            state = unitComponent.GetState();
            return true;
        }

        private void EnsurePublishingCoroutineIsRunning()
        {
            _publishNearbyUnitsCoroutine ??= StartCoroutine(PublishNearbyUnits());
        }

        private void EnsurePublishingCoroutineIsStopped()
        {
            if (_gameObjectToWatch.Count == 0 && _publishNearbyUnitsCoroutine != null)
            {
                StopCoroutine(_publishNearbyUnitsCoroutine);
                _publishNearbyUnitsCoroutine = null;
            }
        }

        private IEnumerator PublishNearbyUnits()
        {
            while (true)
            {
                if (_gameObjectToWatch.Count == 0)
                {
                    yield return new WaitForSeconds(intervalS);
                    continue;
                }

                InstantiateResult();
                var gameObjectToWatchCount = _gameObjectToWatch.Count;
                using var gameIdToPositions =
                    new NativeHashMap<int, Vector3>(gameObjectToWatchCount, Allocator.TempJob);

                foreach (var (unitInstanceId, unitGo) in _gameObjectToWatch)
                {
                    gameIdToPositions.TryAdd(unitInstanceId, unitGo.transform.position);
                }

                var jobHandles = new NativeArray<JobHandle>(_gameObjectToWatch.Count, Allocator.TempJob);
                var writeCounters = new List<NativeArray<int>>();
                var cancelFlags = new List<NativeArray<int>>();
                var resultsToUnitInstanceMap = new Dictionary<int, int>();

                PrepareCopy();

                var writeCounter = new NativeArray<int>(1, Allocator.Persistent) { [0] = 0 };
                var cancelFlag = new NativeArray<int>(1, Allocator.Persistent) { [0] = 0 };
                
                var index = 0;
                foreach (var (unitGoInstanceId, unitGo) in _gameObjectToWatch)
                {

                    var unit = unitGo.GetComponent<Units.Base.Unit>();
                    var radius = unit.GetWeapon().GetComponent<Bullets.Bullets>().Attack.Range;
                    var iterations = Manager.CalculateCellIterationByRadius(radius);

                    resultsToUnitInstanceMap.TryAdd(unitGoInstanceId, index);

                    var job = new Manager.NearbyUnitsJob
                    {
                        WriteCounter = writeCounter,
                        CancelFlag = cancelFlag,
                        GameIdToPositions = gameIdToPositions,
                        Result = _results[index].AsParallelWriter(),
                        Radius = radius,
                        CurrentCell = Manager.Instance.GetCell(unitGoInstanceId),
                        CurrentDirection = unit.IsPlayerUnit() ? Manager.Dir.Ltr : Manager.Dir.Rtl,
                        CurrGameObjectId = unitGoInstanceId,
                        PartitionsWithGameId = _readOnlyCopyPartitions.AsReadOnly(),
                        MaxCapacity = 10
                    };

                    var handle = job.Schedule(iterations, 1);
                    jobHandles[index] = handle;
                    // writeCounters.Add(writeCounter);
                    // cancelFlags.Add(cancelFlag);

                    index++;
                }

                if (jobHandles.Length > 0)
                {
                    var combineHandles = JobHandle.CombineDependencies(jobHandles);
                    yield return new WaitUntil(() => combineHandles.IsCompleted);

                    combineHandles.Complete();

                    foreach (var (unitInstanceId, resultIndex) in resultsToUnitInstanceMap)
                    {
                        ProcessNearbyUnits(unitInstanceId, _results[resultIndex]);
                    }
                }

                DisposeJobResources(jobHandles, writeCounters, cancelFlags);
                yield return new WaitForSeconds(intervalS);
            }
        }

        private void ProcessNearbyUnits(int unitInstanceId, NativeList<int> resultsArray)
        {
            var nearbyUnitIdList = new List<int>(resultsArray.Length - 1);

            foreach (var nearbyUnit in resultsArray)
            {
                if (nearbyUnit == unitInstanceId) continue;
                nearbyUnitIdList.Add(nearbyUnit);
            }

            if (nearbyUnitIdList.Count == 0) return;

            nearbyUnitsEventChannel.RaiseEvent(new()
                { UnitInstanceId = unitInstanceId, NearbyUnitIdList = nearbyUnitIdList });
        }

        private void DisposeJobResources(NativeArray<JobHandle> jobHandles, List<NativeArray<int>> writeCounters,
            List<NativeArray<int>> cancelFlags)
        {
            jobHandles.Dispose();
            _readOnlyCopyPartitions.Dispose();
            DisposeResults();

            // writeCounters and cancelFlags are always the same length
            for (var i = 0; i < writeCounters.Count; i++)
            {
                writeCounters[i].Dispose();
                cancelFlags[i].Dispose();
            }
        }

        private void PrepareCopy()
        {
            var partitions = Manager.Instance.Partitions;
            var copyCapacity = partitions.Aggregate(0, (acc, kv) => acc + kv.Value.Count);
            _readOnlyCopyPartitions =
                new NativeParallelMultiHashMap<Vector2Int, int>(copyCapacity, Allocator.TempJob);

            foreach (var cell in partitions)
            {
                foreach (var gameObjectId in cell.Value)
                {
                    _readOnlyCopyPartitions.Add(cell.Key, gameObjectId);
                }
            }
        }

        private void DisposeResults()
        {
            if (!_results.IsCreated) return;
            for (var i = 0; i < _results.Length; i++)
            {
                if (_results[i].IsCreated) _results[i].Dispose();
            }

            _results.Dispose();
        }

        private void InstantiateResult()
        {
            var count = _gameObjectToWatch.Count;
            _results = new NativeArray<NativeList<int>>(count, Allocator.TempJob);
            for (var i = 0; i < count; i++)
            {
                _results[i] = new NativeList<int>(maxCapacity, Allocator.TempJob);
            }
        }
    }
}