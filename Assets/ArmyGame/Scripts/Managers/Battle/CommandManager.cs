using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base;
using UnityEngine;

namespace ArmyGame.Managers.Battle
{
    public class CommandManager : MonoBehaviour
    {
        [SerializeField] private NearbyUnitsEventChannel nearbyUnitsEventChannel;
        [SerializeField] private SimulationActionsEventChannel simulationChannel;

        [SerializeField] private GameObjectEventChannel gameObjectStateChangeChannel;

        private readonly Dictionary<int, GameObject> _unitsMap = new Dictionary<int, GameObject>();

        // this dictionary tries to map the unit being attacked to a list of its attackers
        // this will make it easy to handle when a unit dies so we can tell its attackers 
        // to stop attacking/change targets.
        private Dictionary<int, HashSet<int>> _unitAttackingMap = new Dictionary<int, HashSet<int>>();

        // This is used to keep the attacking coroutines mapped to their attackers 
        private Dictionary<int, Coroutine> _coroutines = new Dictionary<int, Coroutine>();

        private void OnEnable()
        {
            _unitsMap.Clear();
            nearbyUnitsEventChannel.OnEventRaised += ProcessAttack;
            gameObjectStateChangeChannel.OnEventRaised += HandleGameObjectStateChange;
        }

        private void OnDisable()
        {
            nearbyUnitsEventChannel.OnEventRaised -= ProcessAttack;
            gameObjectStateChangeChannel.OnEventRaised -= HandleGameObjectStateChange;
        }

        private void HandleGameObjectStateChange(GameObject go)
        {
            if (!go.TryGetComponent<Unit>(out var unit)) return;

            var unitId = unit.GameObject.GetInstanceID();
            if (unit.GetState() == UnitState.Active)
            {
                _unitsMap.TryAdd(unitId, unit.GameObject);
            }
            else if (unit.GetState() == UnitState.InActive)
            {
                _unitsMap.Remove(unitId);

                //stop tracking attacking units
                if (!_unitAttackingMap.Remove(unitId, out var attacking)) return;

                // this is safe to do as units should only be attacking one unit at a time
                foreach (var attacker in attacking)
                {
                    StopUnitAttack(attacker);
                }
            }
        }

        private void ProcessAttack(NearbyUnitsParam param)
        {
            var attackingUnitId = param.UnitInstanceId;
            if (!_unitsMap.TryGetValue(attackingUnitId, out var attackingUnitGo) || IsAttacking(attackingUnitId))
            {
                return;
            }

            // implement filter by owner
            if (!attackingUnitGo.TryGetComponent(out Unit attackingUnit)) return;

            var defendingUnitId = param.NearbyUnitIdList
                .Select(unitId => _unitsMap.GetValueOrDefault(unitId))
                .Where(unitGo => unitGo is not null && !unitGo.GetComponent<Unit>().IsAlly(attackingUnit))
                .Select(unitGo => unitGo.GetInstanceID())
                .FirstOrDefault();

            if (defendingUnitId == 0 || !_unitsMap.TryGetValue(defendingUnitId, out var defendingUnitGo))
            {
                Debug.Log("the unit to be attacked is not present");
                return;
            }

            if (!defendingUnitGo.TryGetComponent(out Unit defendingUnit)) return;

            StartUnitAttack(attackingUnit, defendingUnit);
        }

        private void StartUnitAttack(Unit attackingUnit, Unit defendingUnit)
        {
            var attackingUnitId = attackingUnit.GameObject.GetInstanceID();
            if (_coroutines.ContainsKey(attackingUnitId))
            {
                Debug.LogError($"unit is already attacking  {attackingUnitId}");
                return;
            }

            var defendingUnitId = defendingUnit.GameObject.GetInstanceID();
            // add to unitAttackingMap
            AddToAttackingMap(attackingUnitId, defendingUnitId);
            Debug.Log($"attack coroutine is created for {attackingUnitId}");
            // create attack coroutine
            _coroutines.Add(attackingUnitId, StartCoroutine(AttackCoroutine(attackingUnit, defendingUnit)));
        }


        IEnumerator AttackCoroutine(Unit attackingUnit, Unit defendingUnit)
        {
            var attackCooldown = attackingUnit.GetWeapon().GetComponent<Bullets.Bullets>().Settings.Attack.Cooldown;
            while (true)
            {
                simulationChannel.RaiseEvent(new SimulationActionsEventParams(attackingUnit, defendingUnit,
                    SimulationActionsEnum.Attack));
                yield return new WaitForSeconds(attackCooldown);
            }
        }

        private void StopUnitAttack(int attackingUnitId)
        {
            if (!_coroutines.TryGetValue(attackingUnitId, out var routineId)) return;
            StopCoroutine(routineId);
            _coroutines.Remove(attackingUnitId);
        }

        private void AddToAttackingMap(int attackerId, int defenderId)
        {
            if (!_unitAttackingMap.TryGetValue(defenderId, out var attackers))
            {
                _unitAttackingMap[defenderId] = new HashSet<int>() { attackerId };
            }
            else
            {
                attackers.Add(attackerId);
            }
        }

        private bool IsAttacking(int attacker) => _coroutines.ContainsKey(attacker);
    }
}