using System;
using ArmyGame.Editor.ScriptableObjects.Maps;
using Logic.Units;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.RuntimeSets.Dictionary
{
    [CreateAssetMenu(menuName = "RuntimeSets/MapZoneForAgentSet", order = 0)]
    public class MapZoneForAgentSet : GenericDictionarySet<AgentsEnumLike, MapZoneSO>
    { }
}