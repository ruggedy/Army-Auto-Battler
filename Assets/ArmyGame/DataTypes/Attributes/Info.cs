using System;
using UnityEngine;

namespace Logic.Attributes
{
    [Serializable]
    public struct Info
    {
        public string Name;
        [SerializeField]
        private Sprite _icon;
        [SerializeField]
        private Sprite _sprite;
        
        public Sprite Icon => _icon;
        public Sprite Sprite => _sprite;
    }
}