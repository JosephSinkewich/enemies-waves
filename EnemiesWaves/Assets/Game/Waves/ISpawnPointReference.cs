using UnityEngine;

namespace EnemiesWaves.Spawning
{
    public interface ISpawnPointReference
    {
        Transform Transform { get; }
    }
}
