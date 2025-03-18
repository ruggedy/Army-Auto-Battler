using UnityEngine;

namespace Logic.Units.Interfaces
{
    public interface IPooledComponent
    {
        GameObject Prefab { get; set; }
        GameObject GameObject { get; }
    }
}