using System;

using System.Linq;
using ArmyGame.Editor.ScriptableObjects.Maps;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.ScriptableObjects.RuntimeSets.Dictionary;
using Logic.Units;
using Logic.World;
using UnityEngine;

namespace ArmyGame.Managers
{
    public class SpawnUnitManager : MonoBehaviour
    {
        [SerializeField] private AgentEventChannel spawnUnitManagerChannel;
        [SerializeField] private GameObjectEventChannel gameObjectStateChangeChannel;
        [SerializeField] private SoToGoMap unitSetSo;
        [SerializeField] private SoToGoMap bulletSetSo;
        [SerializeField] private MapZoneSetSo mapZones;
        [SerializeField] private AgentsEnumLike enemyAgentEnum;
        [SerializeField] private AgentsEnumLike playerAgentEnum;
        [SerializeField] private GameObject playerAgents;
        [SerializeField] private GameObject enemyAgents;

        [SerializeField] private ScriptableObjectEventChannel spawnBulletChannel;

        private Grid _grid;
        public static SpawnUnitManager Instance { get; private set; }

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
            _grid = GameObject.FindFirstObjectByType<Grid>();

            if (spawnUnitManagerChannel != null)
            {
                spawnUnitManagerChannel.OnEventRaised += SpawnUnit;
            }
        }

        private void OnDisable()
        {
            spawnUnitManagerChannel.OnEventRaised -= SpawnUnit;
        }

        private void SetSpawnedUnitProps(GameObject spawnedUnit, Vector3 position, Quaternion rotation,
            AgentsEnumLike owner)
        {
            spawnedUnit.transform.position = position;
            spawnedUnit.transform.rotation = rotation;

            var unit = spawnedUnit.GetComponent<Units.Base.Unit>();
            unit.Owner = owner;

            spawnedUnit.transform.SetParent(
                unit.Owner == playerAgentEnum ? playerAgents.transform : enemyAgents.transform, true);

            spawnedUnit.SetActive(true);
        }

        private void SpawnUnit(AgentChannelEventParams param)
        {
            var (unit, agent) = param;

            try
            {
                var prefabToSpawn = unitSetSo.Items.First(pair => pair.key == unit);
                var startZone = agent == playerAgentEnum ? mapZones.Zones.First() : mapZones.Zones.Last();

                var spawnPosition = Utils.GetRandomWorldPositionInZone(_grid, startZone.value);

                var spawnedUnit = PoolManager.Instance.GetFromPool(prefabToSpawn.value);

                SetSpawnedUnitProps(spawnedUnit, spawnPosition, spawnedUnit.transform.rotation, agent);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}