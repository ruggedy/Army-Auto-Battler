using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.RuntimeSets.Dictionary
{
    public class GenericDictionarySet<KEY, VALUE> : ScriptableObject
    {
        [Serializable]
        public struct SetPair
        {
            public KEY key;
            public VALUE value;
        }
        
        private List<SetPair> _internal_items = new List<SetPair>();

        public List<SetPair> Items => _internal_items;

        public void Add(KEY key, VALUE value)
        {
            var isPresent = _internal_items.Any(pair => pair.key.Equals(key));
            
            if (isPresent)
            {
                Debug.Log($"{key} already exists");
                return;
            }

            _internal_items.Add(new SetPair{ key = key, value = value });
            Debug.Log($"new count after add {_internal_items.Count}");
        }


        public void Update(KEY key, VALUE value)
        {
            var idx = _internal_items.FindIndex(pair => pair.key.Equals(key));
            
            if (idx == -1)
            {
                return;
            }
            var newValue = new SetPair{ key = key, value = value };
            
            _internal_items[idx] = newValue;

            Debug.Log($"{key.ToString()} has been updated to {value.ToString()}");
        }

        public void Remove(KEY key) => _internal_items.RemoveAll(pair => pair.key.Equals(key));
    }
}