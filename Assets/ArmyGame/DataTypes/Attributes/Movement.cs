using System;

namespace Logic.Attributes
{
    [Serializable]
    public struct Movement
    {
        public float Speed; /// Measured in metres/second
        public float RotationSpeed; /// Measured in metres/second
    }
}