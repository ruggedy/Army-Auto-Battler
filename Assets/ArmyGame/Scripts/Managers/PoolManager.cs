using System.Collections.Generic;
using ArmyGame.ScriptableObjects.RuntimeSets.Dictionary;
using Logic.Units.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArmyGame.Managers
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private int initialPrefabPoolCount = 10;
        [SerializeField] private List<SoToGoMap> pooledPrefabs;

        private readonly Dictionary<GameObject, Queue<IPooledComponent>> _pools =
            new Dictionary<GameObject, Queue<IPooledComponent>>();

        private readonly Dictionary<int, GameObject> activeGameObjectsById = new Dictionary<int, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            pooledPrefabs.ForEach(runtimeSet =>
            {
                runtimeSet.Items.ForEach(pair => CreatePool(pair.value, initialPrefabPoolCount));
            });
        }

        private void CreatePool(GameObject prefab, int count)
        {
            Queue<IPooledComponent> pool = new Queue<IPooledComponent>();

            for (int i = 0; i < count; ++i)
            {
                GameObject obj = Instantiate(prefab, transform.GetChild(0));

                if (obj.TryGetComponent<IPooledComponent>(out var pooledComponent))
                {
                    // set pooledComponent prefab reference
                    pooledComponent.Prefab = prefab;

                    // ensure object is not active 
                    obj.SetActive(false);

                    pool.Enqueue(pooledComponent);
                }
            }

            _pools.TryAdd(prefab, pool);
        }

        public GameObject GetFromPool(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                Debug.LogError(prefab.name + " is not in pool");

                CreatePool(prefab, 1);
            }

            var pool = _pools[prefab];

            if (pool.Count == 0)
            {
                var isPooledComponent = Instantiate(prefab).TryGetComponent<IPooledComponent>(out var newObject);

                // add prefab reference
                newObject.Prefab = prefab;

                // ensure gameobject is inactive.
                newObject.GameObject.SetActive(false);
                if (isPooledComponent)
                {
                    pool.Enqueue(newObject);
                }
            }

            var obj = pool.Dequeue();

            activeGameObjectsById.TryAdd(obj.GameObject.GetInstanceID(), obj.GameObject);

            return obj.GameObject;
        }

        public void ReturnToPool(GameObject prefab, GameObject obj)
        {
            if (!obj.TryGetComponent<IPooledComponent>(out var pooledComponent)) return;

            pooledComponent.GameObject.SetActive(false);
            pooledComponent.GameObject.transform.SetParent(transform.GetChild(0));

            if (!_pools.ContainsKey(prefab))
            {
                Debug.LogError(prefab.name + " is not in pool");
                CreatePool(prefab, 0);
            }

            _pools[prefab].Enqueue(pooledComponent);
            activeGameObjectsById.Remove(obj.GetInstanceID());
        }
    }
}