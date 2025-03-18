using UnityEngine;

namespace ArmyGame.Managers
{
    public class WorldScaleManager : MonoBehaviour
    {
        
        public static WorldScaleManager Instance { get; private set; }
    
        [Header("World Scale Settings")]
        [Tooltip("How many meters equal one Unity unit. For example, 1 means 1 Unity unit = 1 meter.")]
        [SerializeField] float unityUnitToMeter = 3f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Optionally, persist this manager across scenes.
            // DontDestroyOnLoad(gameObject);
        }

        public float UnityToWorldUnits(float unityValue)
        {
            return unityValue * unityUnitToMeter;
        }

        public float WorldToUnityUnits(float worldValue)
        {
            return worldValue / unityUnitToMeter;
        }

        public Vector3 UnityToWorldUnits(Vector3 unityVector)
        {
            return unityVector * unityUnitToMeter;
        }
        
        public Vector3 WorldToUnityUnits(Vector3 worldVector)
        {
            return worldVector / unityUnitToMeter;
        }
    }
}