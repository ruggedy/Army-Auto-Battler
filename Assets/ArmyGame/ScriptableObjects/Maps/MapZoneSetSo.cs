using System.Collections.Generic;
using UnityEngine;

namespace ArmyGame.Editor.ScriptableObjects.Maps
{
    [CreateAssetMenu(menuName = "maps/create zone list", order = 0)]
    public class MapZoneSetSo : ScriptableObject
    {
        [SerializeField]
        private List<MapZoneSO> _internal = new List<MapZoneSO>();
        public List<MapZoneSO> Zones => _internal;
    }
}