using System.Collections;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base.Appearance;
using ArmyGame.Units.Base.DataTypes;
using Logic.Attributes;
using Logic.Units;
using Logic.Units.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace ArmyGame.Units.Base
{

   public enum UnitState
    {
        Idle, 
        Moving,
        Attacking,
        InActive, // also can be considered the death state
        Active
    }
    public class Unit : MonoBehaviour, ISimulatableEntitiy, IDamageable, IPooledComponent
    {
        [SerializeField] private UnitAppearance[] unitAppearanceParts;
        [SerializeField] private UnitHealthState unitHealthState = UnitHealthState.FUll;
        [FormerlySerializedAs("unit")] [SerializeField] private UnitSO unitSo;
        [SerializeField] private GameObjectEventChannel updateUnitInPartition;
        [SerializeField] private GameObjectEventChannel gameObjectStateChangeChannel;
        [SerializeField] private AgentsEnumLike playerAgent;
        [SerializeField] private AgentsEnumLike enemyAgent;
        private UnitState _state = UnitState.InActive;
        
        public GameObject Prefab { set; get; }
        public GameObject GameObject => gameObject;

        public Vitality vitality => unitSo.Vitality;

        private Coroutine _onUnitMoveCoroutine;

        [SerializeField] private float checkMoveInterval = 0.5f;
        [SerializeField] private GameObject rocketBulletSo;
        private Vector3 _lastPosition;
        private int _currentHealth;
        
        public AgentsEnumLike Owner { get; set; }

        public UnitSO UnitSo
        {
            get => unitSo;
        }

        NavMeshAgent _agent;


        public virtual UnitState GetState()
        {
            return _state;
        }

        public virtual void SetState(UnitState state)
        {
            _state = state;
            gameObjectStateChangeChannel?.RaiseEvent(gameObject);
        }
        private void OnEnable()
        {
            if (_onUnitMoveCoroutine != null)
            {
                StopCoroutine(_onUnitMoveCoroutine);
            }

            SetState(UnitState.Active);
            _lastPosition = transform.position;
            _onUnitMoveCoroutine = StartCoroutine(CheckAndFireUnitMovedEvent());
            _currentHealth = (int)UnitSo.Vitality.Health;
            
        }

        private void OnDisable()
        {
            StopCoroutine(_onUnitMoveCoroutine);
            SetState(UnitState.InActive);
        }

        private void UpdateAppearance()
        {
            // calculate UnitHealthState here or something
            unitHealthState = UnitHealthState.FUll;
            {
                foreach (var unitAppearancePart in unitAppearanceParts)
                {
                    unitAppearancePart.SetState(unitHealthState);
                }
            }
        }

        public void IsAttacking()
        {
            SetState(UnitState.Attacking);
            StopMovement();
        }

        public void StopMovement()
        {
            _agent.isStopped = true;
            SetState(UnitState.Idle);
        }

        public GameObject GetWeapon()
        {
            return rocketBulletSo;
        }

        public bool IsAlly(Unit unit)
        {
            return Owner == unit.Owner;
        }

        public bool IsPlayerUnit()
        {
            return Owner == playerAgent;
        }

        IEnumerator CheckAndFireUnitMovedEvent()
        {
            while (true)
            {
                if (_lastPosition != transform.position)
                {
                    _lastPosition = transform.position;
                    SetState(UnitState.Moving);
                    updateUnitInPartition?.RaiseEvent(gameObject);
                }

                if (_lastPosition == transform.position && _state == UnitState.Moving)
                {
                    SetState(UnitState.Idle);
                }

                yield return new WaitForSeconds(checkMoveInterval);
            }
        }

        protected virtual void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            // _agent.speed = WorldScaleManager.Instance.WorldToUnityUnits(UnitSo.Movement.Speed);
        }

        public virtual void OnDamage(float damage)
        {
            _currentHealth -= (int)damage;
            if (_currentHealth <= 0)
            {
                gameObject.SetActive(false);
            }

            UpdateAppearance();
        }
        
    }
}