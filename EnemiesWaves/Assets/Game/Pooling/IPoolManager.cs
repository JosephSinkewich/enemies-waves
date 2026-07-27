using UnityEngine;

namespace EnemiesWaves.Pooling
{
    public interface IPoolManager
    {
        void Prewarm(GameObject prefab, int count);
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Despawn(GameObject instance);
    }
}
