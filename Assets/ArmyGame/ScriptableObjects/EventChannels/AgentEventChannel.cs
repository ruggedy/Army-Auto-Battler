using System;
using Logic.Units;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.EventChannels
{
    public struct AgentChannelEventParams
    {
        public UnitSO Unit { get; }
        public AgentsEnumLike Agent { get; }
        public AgentChannelEventParams(UnitSO unit, AgentsEnumLike agent)
        {
            Unit = unit;
            Agent = agent;
        }
        public void Deconstruct(out UnitSO unit, out AgentsEnumLike agent)
        {
            unit = Unit;
            agent = Agent;
        }
    }
    
    [CreateAssetMenu(fileName = "SpawnUnits", menuName = "EventChannels/SpawnUnits", order = 0)]
    public class AgentEventChannel : EventChannelSO<AgentChannelEventParams>
    {
    }
}