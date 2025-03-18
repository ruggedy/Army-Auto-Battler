using System.Collections.Generic;
using System.Threading;
using ArmyGame.ScriptableObjects.EventChannels;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ArmyGame.Managers.SpatialPartitioning
{
    public class Manager : MonoBehaviour
    {
        public enum Dir
        {
            Ltr,
            Rtl
        }

        // data structure to hold unit within their relative grids 
        // update the unit cell indices, depending on how close / far someone is to your unit. 
        // Grids should be Directionally aware,
        public static Manager Instance { get; private set; }

        // you should only be able to attack forward.
        // let's implement this first. 
        [SerializeField] private Vector2 cellSize = Vector2.one * 2;
        [SerializeField] private GameObjectEventChannel updateUnitInPartition;

        // handle resizing as part of the pub sub model.
        private readonly Dictionary<int, Vector2Int> _cellsById = new();

        public Dictionary<Vector2Int, HashSet<int>> Partitions { get; private set; } = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            updateUnitInPartition.OnEventRaised += UpdateCell;
        }

        private void OnDisable()
        {
            updateUnitInPartition.OnEventRaised -= UpdateCell;
        }

        private static Vector2Int CalculateCell(Vector3 position, Vector2 size) =>
            new(Mathf.FloorToInt(position.x / size.x), Mathf.FloorToInt(position.z / size.y));

        // the ideas is this, radius in "game world units", "cell is based on unity units"
        // so first you want to convert from game world to unity unit
        // then convert to partition identity units
        // then calculate the number of iterations
        public static int CalculateCellIterationByRadius(int radius)
        {
            // you would use the world scale manager here. 
            var radiusToCellUnit = Mathf.FloorToInt(radius / Instance.cellSize.x);
            return (radiusToCellUnit + 1) * (radiusToCellUnit + 1);
        }


        public void Register(int gameObjectId, Vector3 position)
        {
            var cell = CalculateCell(position, cellSize);

            _cellsById[gameObjectId] = cell;

            Partitions ??= new Dictionary<Vector2Int, HashSet<int>>();

            if (!Partitions.TryGetValue(cell, out HashSet<int> unitSet))
            {
                unitSet = new HashSet<int>();

                // there would need to be checks to reseize this, or else it might break; 
                // right now, unlikely to as it has been assigned a ton of memory space.
                Partitions[cell] = unitSet;
            }

            unitSet.Add(gameObjectId);
            _cellsById.TryAdd(gameObjectId, cell);
        }

        public Vector2Int GetCell(int gameObjectId) => _cellsById[gameObjectId];

        // keep a reference to the vectors from the positions, when registering, updating and removing
        // so cells dont have to be recalculated all the time and the accuracy will be high.
        public void Unregister(int gameObjectId)
        {
            if (_cellsById.TryGetValue(gameObjectId, out var cell) && Partitions.ContainsKey(cell))
            {
                Partitions[cell].Remove(gameObjectId);
            }

            _cellsById.Remove(gameObjectId);
        }

        private void UpdateCell(GameObject go)
        {
            var gameObjectId = go.GetInstanceID();
            var position = go.transform.position;

            // cell on move information
            var newCell = CalculateCell(position, cellSize);

            if (!_cellsById.TryGetValue(gameObjectId, out var oldCell) || oldCell == newCell) return;
            Unregister(gameObjectId);
            Register(gameObjectId, position);
        }

        [BurstCompile]
        public struct NearbyUnitsJob : IJobParallelFor
        {
            public Vector2Int CurrentCell;
            public int CurrGameObjectId;
            public int Radius;
            public Dir CurrentDirection;


            [ReadOnly] public NativeParallelMultiHashMap<Vector2Int, int>.ReadOnly PartitionsWithGameId;
            [ReadOnly] public NativeHashMap<int, Vector3> GameIdToPositions;
            public NativeList<int>.ParallelWriter Result;

            //atomic reference counting variables
            public int MaxCapacity;
            public NativeArray<int> WriteCounter;

            // cancellation flag
            public NativeArray<int> CancelFlag;


            public void Execute(int index)
            {
                unsafe
                {
                    var counterPtr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                        WriteCounter);
                    var cancelPtr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                        CancelFlag);

                    ref var cancelFlag =
                        ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.ArrayElementAsRef<int>(cancelPtr, 0);

                    ref var counterFlag =
                        ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.ArrayElementAsRef<int>(counterPtr, 0);

                    if (cancelFlag != 0 || counterFlag >= MaxCapacity) return;
                }

                if (!GameIdToPositions.TryGetValue(CurrGameObjectId, out var centerPosition))
                {
                    unsafe
                    {
                        // this should be an exception as there should not be a state where
                        // get nearby is called but we cant access its position
                        var cancelPtr =
                            Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                                CancelFlag);
                        ref var cancelFlag =
                            ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.ArrayElementAsRef<int>(cancelPtr, 0);
                        Interlocked.CompareExchange(ref cancelFlag, 1, 0);
                        return;
                    }
                }

                var size = Radius + 1;

                var offsetX = index / size;
                var offsetY = index % size;

                var x = CurrentCell.x + (CurrentDirection == Dir.Ltr ? offsetX : -offsetX);
                var y = CurrentCell.y + (CurrentDirection == Dir.Ltr ? offsetY : -offsetY);

                if (!PartitionsWithGameId.TryGetFirstValue(new Vector2Int(x, y), out var gameId, out var iterator) ||
                    !GameIdToPositions.TryGetValue(gameId, out var position)) return;
                do
                {
                    if (Vector3.SqrMagnitude(position - centerPosition) <= (Radius * Radius))
                    {
                        unsafe
                        {
                            var ptr =
                                Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                                    WriteCounter);
                            ref var counter =
                                ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.ArrayElementAsRef<int>(ptr, 0);


                            var oldCount = Interlocked.Increment(ref counter) - 1;
                            if (oldCount < MaxCapacity)
                            {
                                Result.AddNoResize(gameId);
                            }
                            else
                            {
                                Interlocked.CompareExchange(ref counter, MaxCapacity, oldCount + 1);
                                // Interlocked.CompareExchange(ref cance, counter, oldCount);
                            }
                        }
                    }
                } while (PartitionsWithGameId.TryGetNextValue(out gameId, ref iterator));
            }
        }
    }
}