using UnityEditor.Tilemaps;
using UnityEngine;

namespace ArmyGame.Editor.ScriptableObjects.Maps
{
    [CreateAssetMenu(menuName = "maps/create_zone", order = 0)]
    public class MapZoneSO : ScriptableObject
    {
        [SerializeField]
        private BoundsInt _internal;
        public BoundsInt value => _internal;

        private void Initialize(BoundsInt value)
        {
            _internal = value;
        }

        public static MapZoneSO Create(BoundsInt value)
        {
            var zone = ScriptableObject.CreateInstance<MapZoneSO>();
            zone.Initialize(value); 
            return zone;
        }
    }
}