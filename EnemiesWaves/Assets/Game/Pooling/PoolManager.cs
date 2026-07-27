using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace EnemiesWaves.Pooling
{
    public sealed class PoolManager
    {
        private sealed class Pool
        {
            public readonly Queue<GameObject> Available = new();
        }

        private readonly IObjectResolver _resolver;
        private readonly Transform _poolRoot;
        private readonly Dictionary<GameObject, Pool> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instancePrefabs = new();
        private readonly HashSet<GameObject> _warnedPrefabs = new();

        public PoolManager(IObjectResolver resolver, PoolRootReference poolRootReference)
        {
            _resolver = resolver;
            _poolRoot = poolRootReference.Transform;
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;

            var pool = GetOrCreatePool(prefab);
            for (var i = 0; i < count; i++)
            {
                pool.Available.Enqueue(CreateInstance(prefab, pool));
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            var instance = pool.Available.Count > 0 ? pool.Available.Dequeue() : CreateFallback(prefab, pool);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null || !_instancePrefabs.TryGetValue(instance, out var prefab))
            {
                Debug.LogWarning("Pool despawn ignored: object was not created by this pool.");
                return;
            }

            if (!instance.activeSelf) return;

            instance.SetActive(false);
            _pools[prefab].Available.Enqueue(instance);
        }

        private Pool GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool)) return pool;

            pool = new Pool();
            _pools.Add(prefab, pool);
            return pool;
        }

        private GameObject CreateFallback(GameObject prefab, Pool pool)
        {
            if (_warnedPrefabs.Add(prefab))
            {
                Debug.LogWarning($"Pool exhausted for '{prefab.name}'. Creating a fallback instance.");
            }

            return CreateInstance(prefab, pool);
        }

        private GameObject CreateInstance(GameObject prefab, Pool pool)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, _poolRoot);
            instance.SetActive(false);
            _resolver.InjectGameObject(instance);
            _instancePrefabs.Add(instance, prefab);
            return instance;
        }
    }

}
