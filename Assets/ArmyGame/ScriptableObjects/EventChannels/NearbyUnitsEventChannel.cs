using System.Collections.Generic;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.EventChannels
{
    public struct NearbyUnitsParam
    {
        public int UnitInstanceId;
        public List<int> NearbyUnitIdList;
    }
    
    [CreateAssetMenu(menuName = "EventChannels/NearbyUnitsEventChannel", order = 0)]
    public class NearbyUnitsEventChannel : EventChannelSO<NearbyUnitsParam>
    {
    }
}