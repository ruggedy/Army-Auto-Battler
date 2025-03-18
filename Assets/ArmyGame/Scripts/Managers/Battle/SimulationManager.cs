using System;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Actions;
using ArmyGame.Units.Base;
using Logic.Units.Interfaces;
using Logic.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArmyGame.Managers.Battle
{
    public class SimulationManager : MonoBehaviour
    {
        [FormerlySerializedAs("_channel")] [SerializeField]
        private SimulationActionsEventChannel channel;


        private void OnEnable() => channel.OnEventRaised += HandleSimulation;
        private void OnDisable() => channel.OnEventRaised -= HandleSimulation;

        public void HandleSimulation(SimulationActionsEventParams eventParams)
        {
            switch (eventParams.Action)
            {
                case SimulationActionsEnum.Attack:
                    AttackSimulation(eventParams.Unit, eventParams.Target);
                    break;
                case SimulationActionsEnum.Defend:
                    DefendSimulation(eventParams.Unit, eventParams.Target);
                    break;
            }
        }

        public void AttackSimulation(ISimulatableEntitiy unit, ISimulatableEntitiy target)
        {
            if (!unit.TryGetComponent<Unit>(out var unitComponent) ||
                !target.TryGetComponent<Unit>(out var targetUnitComponent) ||
                unitComponent.GetState() == UnitState.InActive)
            {
                Debug.Log("Either the unit does not have Unit component or target does not have Target Component");
                return;
            }

            unitComponent.IsAttacking();

            //unit rotate towards attacker
            unitComponent.GetComponent<Rotate>()?.RotateTo(targetUnitComponent.transform);

            var unitWeaponPrefab = unitComponent.GetWeapon();
            var unitWeapon = PoolManager.Instance.GetFromPool(unitWeaponPrefab);

            if (!unitWeapon.TryGetComponent(out Bullets.Bullets unitBullet))
            {
                Debug.Log("weapon instance is missing bullet component");
                return;
            }

            unitBullet.owner = unitComponent;

            // set the bullet to the units position and rotation
            unitBullet.transform.SetPositionAndRotation(unitComponent.transform.position,
                unitComponent.transform.rotation);

            unitBullet.Target = targetUnitComponent.transform;
            unitBullet.GameObject.SetActive(true);
        }

        public void DefendSimulation(ISimulatableEntitiy weapon, ISimulatableEntitiy target)
        {
            if (!weapon.TryGetComponent<Bullets.Bullets>(out var bulletComponent) ||
                !target.TryGetComponent<Unit>(out var targetUnitComponent))
            {
                Debug.LogError(
                    "Either the weapon does not have bullet component or target does not have unit component");
                return;
            }

            var weaponAttack = bulletComponent.Attack;
            var targetVitality = targetUnitComponent.vitality;

            var weaponHits = (Utils.Randomizer.GenerateRandom() * 100) <= weaponAttack.Accuracy;
            var targetDodges = (Utils.Randomizer.GenerateRandom() * 100) <= targetVitality.Dogde;

            Debug.Log($"Weapon hits {weaponHits} and target dodges {targetDodges}");

            if (weaponHits && !targetDodges)
            {
                // return Bullet back to the pool. 
                var targetDefence = targetVitality.Defence;
                var damageTaken = Mathf.Max(weaponAttack.Damage - targetDefence, 0);

                Debug.Log($"calls damage {damageTaken}");

                targetUnitComponent.OnDamage(damageTaken);
            }

            PoolManager.Instance.ReturnToPool(bulletComponent.Prefab, bulletComponent.gameObject);
        }
    }
}