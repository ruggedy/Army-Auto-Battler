using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.ScriptableObjects.RuntimeSets.Dictionary;
using Logic.Units;
using UnityEngine;

namespace ArmyGame.Managers
{
    // simple strategy, for now, make sure you are spawining

    public class EnemyManager : MonoBehaviour
    {
        enum StrategyType
        {
            Simple,
            Advanced,
        }

        struct Strategy
        {
            public StrategyType Type;
            public Coroutine Routine;
        }

        [SerializeField] private AgentEventChannel channel;
        [SerializeField] private SoToGoMap prefabs;
        [SerializeField] private AgentsEnumLike enemyAgentEnum;
        [SerializeField] private AgentsEnumLike playerAgentEnum;
        [SerializeField] private GameObject playerAgents;
        [SerializeField] private GameObject enemyAgents;

        [SerializeField] private int maxSpawned = 3;
        private int _currentSpawned;
        private Strategy? _currentStrategy;
        // [SerializeField] private 


        private void OnEnable()
        {
            // adding this want the player to always start first.
            channel.OnEventRaised += SpawnUnitHandler;
        }

        private void OnDisable()
        {
            // channel.OnEventRaised -= SpawnUnitHandler;
            // Stop the current strategy coroutine, if any.
            if (_currentStrategy.HasValue && _currentStrategy.Value.Routine != null)
            {
                StopCoroutine(_currentStrategy.Value.Routine);
                _currentStrategy = null;
            }
        }

        private void SpawnUnitHandler(AgentChannelEventParams @params)
        {
            channel.OnEventRaised -= SpawnUnitHandler;
            SelectStrategy(StrategyType.Simple);
        }

        private void SelectStrategy(StrategyType newStrategy)
        {
            if (_currentStrategy?.Type == newStrategy) return;


            var routine = _currentStrategy?.Routine;

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            switch (newStrategy)
            {
                case StrategyType.Simple:
                    routine = StartCoroutine(SimpleStrategy());
                    break;
                case StrategyType.Advanced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newStrategy), newStrategy, null);
            }

            _currentStrategy = new Strategy { Type = newStrategy, Routine = routine };
        }

        // Simple strategy 
        IEnumerator SimpleStrategy()
        {
            while (true)
            {
                if (_currentSpawned >= maxSpawned)
                {
                    break;
                }

                var agents = enemyAgents.transform.Cast<Transform>();

                var initialCount = prefabs.Items
                    .Select(unit => unit.key)
                    .OfType<UnitSO>()
                    .ToDictionary(unitSo => unitSo, _ => 0);

                // Todo ---- optimize this, it can be cached somewehere, perhaps the game object itself can keep track of this itself

                var toSpawn = agents
                    .Aggregate(initialCount, (acc, child) =>
                    {
                        var baseUnit = child.GetComponent<Units.Base.Unit>();
                        acc[baseUnit.UnitSo]++;
                        return acc;
                    })
                    .OrderBy(pair => pair.Value)
                    .DefaultIfEmpty(new KeyValuePair<UnitSO, int>(prefabs.Items.First().key as UnitSO, 1))
                    .First();


                channel.RaiseEvent(new AgentChannelEventParams(toSpawn.Key, enemyAgentEnum));
                _currentSpawned += 1;
                yield return new WaitForSeconds(5f);
            }
        }
    }
}