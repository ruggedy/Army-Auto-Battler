using System;
using ArmyGame.DataTypes;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base;
using Logic.Attributes;
using Logic.Units.Interfaces;
using UnityEngine;

namespace ArmyGame.Bullets
{
    public enum BulletState
    {
        Idle,
        Active,
        InActive,
    }

    public class Bullets : MonoBehaviour, ISimulatableEntitiy, IDamager, IPooledComponent
    {
        [SerializeField] private GameObjectEventChannel gameObjectStateChangeChannel;

        // bullets have a .15 unity units between them. 
        // so +/-  0.075 unity units. 
        [Header("Bullet Settings")] [SerializeField]
        private BulletSo settings;

        public BulletSo Settings => settings;
        public Transform Target;
        public Unit owner;

        public GameObject GameObject => gameObject;

        public GameObject Prefab { set; get; }

        public Attack Attack
        {
            get => settings.Attack;
        }
        
        private BulletState _state = BulletState.InActive;

        private void Update()
        {
            if (transform.position == Target.position)
            {
                gameObject.SetActive(false);
            }

            var direction = (Target.position - transform.position).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
            transform.position = Vector3.MoveTowards(transform.position, Target.position,
                settings.Movement.Speed * Time.deltaTime);
        }

        void SetStete(BulletState state)
        {
            _state = state;
            gameObjectStateChangeChannel.RaiseEvent(gameObject);
        }

        private void OnEnable()
        {
            SetStete(BulletState.Idle);
        }

        private void OnDisable()
        {
            SetStete(BulletState.InActive);
        }
    }
}