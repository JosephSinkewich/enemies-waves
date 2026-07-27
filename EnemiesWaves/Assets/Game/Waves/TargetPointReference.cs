using UnityEngine;

namespace EnemiesWaves.Spawning
{
    public sealed class TargetPointReference : ITargetPointReference
    {
        public TargetPointReference(Transform transform) => Transform = transform;

        public Transform Transform { get; }
    }
}
