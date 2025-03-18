using System.Collections.Generic;
using System.Linq;
using ArmyGame.Editor.ScriptableObjects.Maps;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base;
using Logic.Units;
using Logic.World;
using UnityEngine;
using UnityEngine.AI;

namespace ArmyGame.Managers
{
    public class AgentManager : MonoBehaviour
    {
        [SerializeField] private AgentTypeEventChannel channel;
        [SerializeField] private GameObjectEventChannel gameObjectStateChangedChannel;
        [SerializeField] private MapZoneSetSo zones;
        [SerializeField] private AgentsEnumLike playerAgentType;
        [SerializeField] private AgentsEnumLike enemyAgentType;

        [SerializeField] private GameObject playerAgents;
        [SerializeField] private GameObject enemyAgents;

        private Grid _grid;

        private static readonly HashSet<UnitState> MovableStates = new() { UnitState.Idle, UnitState.Active };

        /// <summary>
        /// //////////
        /// </summary>
        private void OnEnable()
        {
            _grid = FindFirstObjectByType<Grid>();
            channel.OnEventRaised += ProcessAgentEvent;
            gameObjectStateChangedChannel.OnEventRaised += ProcessNewAgentSpawnedEvent;
        }

        private void OnDisable()
        {
            channel.OnEventRaised -= ProcessAgentEvent;
            gameObjectStateChangedChannel.OnEventRaised -= ProcessNewAgentSpawnedEvent;
        }

        void ProcessNewAgentSpawnedEvent(GameObject go)
        {
            if (!go.TryGetComponent(out Unit unit) || unit.GetState() != UnitState.Active) return;
            HandleAgentMovement(go);
        }

        void ProcessAgentEvent(AgentsEnumLike agentType)
        {
            var agentParents = agentType == playerAgentType ? playerAgents : enemyAgents;

            foreach (Transform child in agentParents.transform)
            {
                if (!child.gameObject.TryGetComponent<Unit>(out var unit)) continue;
                if (MovableStates.Contains(unit.GetState()))
                {
                    HandleAgentMovement(child.gameObject);
                }
            }
        }

        // this is a method exposed by the player input component
        void OnMoveAgents() => ProcessAgentEvent(playerAgentType);

        void HandleAgentMovement(GameObject go)
        {
            if (!go.TryGetComponent(out Unit baseUnit)) return;
            if (!go.TryGetComponent(out NavMeshAgent agentNavMeshAgent)) return;

            var agentTransform = go.transform;

            var zonesList = zones.Zones;
            var currAgentZoneIndex = baseUnit.Owner == enemyAgentType
                ? zonesList.FindIndex(zone => Utils.IsWithinZone(_grid, zone.value, agentTransform))
                : zonesList.FindLastIndex(zone => Utils.IsWithinZone(_grid, zone.value, agentTransform));

            if (currAgentZoneIndex == -1)
            {
                return;
            }

            var currZone = zonesList[currAgentZoneIndex];
            var isGameOver = baseUnit.Owner == playerAgentType && currZone == zonesList.Last() ||
                             baseUnit.Owner == enemyAgentType && currZone == zonesList.First();

            if (isGameOver)
            {
                // calculate some sort of win condition event here
                return;
            }


            var nextIndex = baseUnit.Owner == playerAgentType ? currAgentZoneIndex + 1 : currAgentZoneIndex - 1;

            var nextZone = zonesList[nextIndex];
            var newPosition = Utils.GetRandomWorldPositionInZone(_grid, nextZone.value);

            agentNavMeshAgent.SetDestination(newPosition);
        }
    }
}