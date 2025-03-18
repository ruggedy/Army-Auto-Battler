using Logic.Attributes;
using UnityEngine;

namespace ArmyGame.DataTypes
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "Bullets/Create BulletSo", order = 0)]
    public class BulletSo : ScriptableObject
    { 
        [SerializeField] public Attack Attack;
        [SerializeField] public Movement Movement;
    }
}