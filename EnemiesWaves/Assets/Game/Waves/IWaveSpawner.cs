using System;

namespace EnemiesWaves.Spawning
{
    public interface IWaveSpawner
    {
        event Action<int> OnCurrentWaveChanged;
        event Action<int> OnEnemyActiveCountChanged;
        event Action OnCompleted;

        int CurrentWave { get; }
        int EnemyActiveCount { get; }
    }
}
