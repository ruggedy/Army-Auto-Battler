using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Units;
using Unity.Mathematics;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.Units
{
    using Units;
    [CreateAssetMenu(menuName = "RuntimeSets/UnitSets", order = 0)]
    public class UnitSetSO : ScriptableObject
    {
        [Serializable]
        public struct UnitSoPrefabPair
        {
            public UnitSO unit;
            public GameObject prefab;
        }
        
        private List<UnitSoPrefabPair> _internal_value = new List<UnitSoPrefabPair>();

        public List<UnitSoPrefabPair> Units => _internal_value;

        public void AddUnit(UnitSO unit, GameObject go )
        {
            var isPresent = _internal_value.Any(pair => pair.unit.Equals(unit));
            
            if (isPresent){
                Debug.Log($"{unit} already exists");
                return;
            }
            
            _internal_value.Add(new UnitSoPrefabPair{unit = unit, prefab = go});
            Debug.Log($"new count after add {_internal_value.Count}");
        }
        
        
        public void UpdateUnit(UnitSO unit, GameObject go)
        {
            var currVal = _internal_value.FirstOrDefault(pair => pair.unit.Equals(unit));
            currVal.prefab = go;

            Debug.Log($"{unit.name} has been updated to {go.name}");
        }
        
        public void RemoveUnit(UnitSO unit) => _internal_value.RemoveAll(pair => pair.unit.Equals(unit));
    }
}