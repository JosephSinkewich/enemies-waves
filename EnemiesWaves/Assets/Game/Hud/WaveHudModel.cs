using System;
using EnemiesWaves.Spawning;

namespace EnemiesWaves.UI
{
    public sealed class WaveHudModel : IDisposable
    {
        private readonly WaveSpawner _spawner;

        public WaveHudModel(WaveSpawner spawner)
        {
            _spawner = spawner;
            State = new WaveHudState(spawner.CurrentWave, spawner.EnemyActiveCount);
            _spawner.OnCurrentWaveChanged += OnWaveChanged;
            _spawner.OnEnemyActiveCountChanged += OnEnemyCountChanged;
        }

        public WaveHudState State { get; private set; }
        public event Action<WaveHudState> OnChanged;

        public void Dispose()
        {
            _spawner.OnCurrentWaveChanged -= OnWaveChanged;
            _spawner.OnEnemyActiveCountChanged -= OnEnemyCountChanged;
        }

        private void OnWaveChanged(int wave) => SetState(new WaveHudState(wave, State.ActiveEnemies));
        private void OnEnemyCountChanged(int count) => SetState(new WaveHudState(State.CurrentWave, count));

        private void SetState(WaveHudState newState)
        {
            State = newState;
            OnChanged?.Invoke(State);
        }
    }
}
