using UnityEngine;
using UnityEngine.Events;

namespace ArmyGame.ScriptableObjects.EventChannels
{
    public class EventChannelSO<T> : ScriptableObject
    {
        public UnityAction<T> OnEventRaised;
        public void RaiseEvent(T value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}