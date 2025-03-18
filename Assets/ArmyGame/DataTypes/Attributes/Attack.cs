using System;
using UnityEngine;

namespace Logic.Attributes
{
    [Serializable]
    public struct Attack
    {
        public int Damage;
        public int Range;
        public float Accuracy;
        public float Cooldown;
    }
}