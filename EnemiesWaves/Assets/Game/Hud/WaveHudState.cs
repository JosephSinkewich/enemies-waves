namespace EnemiesWaves.UI
{
    public readonly struct WaveHudState
    {
        public WaveHudState(int currentWave, int activeEnemies)
        {
            CurrentWave = currentWave;
            ActiveEnemies = activeEnemies;
        }

        public int CurrentWave { get; }
        public int ActiveEnemies { get; }
    }
}
