using System;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.Units.Base;
using Logic.Units.Interfaces;
using UnityEngine;

namespace ArmyGame.Units.Actions
{
    public class RocketHitDetection : MonoBehaviour
    {
        // private  
        [SerializeField] private SimulationActionsEventChannel _eventChannel;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Bullets.Bullets>(out var bullet)) return;

            if (!TryGetComponent<Unit>(out var unit)) return;


            if (unit.IsAlly(bullet.owner)) return;

            // new defend simulation launched
            _eventChannel?.RaiseEvent(new SimulationActionsEventParams(bullet, unit, SimulationActionsEnum.Defend));
        }
    }
}