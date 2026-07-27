using UnityEngine;

namespace EnemiesWaves.Pooling
{
    public sealed class PoolRootReference
    {
        public PoolRootReference(Transform transform) => Transform = transform;

        public Transform Transform { get; }
    }
}
