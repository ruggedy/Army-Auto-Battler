using Logic.Units.Interfaces;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.EventChannels
{

    public enum SimulationActionsEnum
    {
        Attack,
        Defend,
    }
    public struct SimulationActionsEventParams
    {
        public ISimulatableEntitiy Unit;
        public ISimulatableEntitiy Target;
        public SimulationActionsEnum Action;

        public SimulationActionsEventParams(ISimulatableEntitiy unit, ISimulatableEntitiy target, SimulationActionsEnum action)
        {
            Unit = unit;
            Target = target;
            Action = action;
        }
    }
    
    [CreateAssetMenu(menuName = "EventChannels/SimulationActionsEventChannel", order = 0)]
    public class SimulationActionsEventChannel : EventChannelSO<SimulationActionsEventParams>
    { }
}