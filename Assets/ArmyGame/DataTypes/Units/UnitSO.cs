using System;
using UnityEngine;

namespace Logic.Units
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Units/CreateUnitSO", order = 0)]
    [Serializable]
    public class UnitSO : ScriptableObject
    {
        public UnitSO Id; 
        public Attributes.Info Info;
        public Attributes.Attack Attack;
        public Attributes.Movement Movement;
        public Attributes.Vitality Vitality;
    }
}