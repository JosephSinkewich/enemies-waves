using UnityEngine;

namespace EnemiesWaves.Spawning
{
    public sealed class SpawnPointReference
    {
        public SpawnPointReference(Transform transform) => Transform = transform;

        public Transform Transform { get; }
    }
}
