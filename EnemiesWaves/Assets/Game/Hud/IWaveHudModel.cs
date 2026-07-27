using System;

namespace EnemiesWaves.UI
{
    public interface IWaveHudModel
    {
        event Action<WaveHudState> OnChanged;
        WaveHudState State { get; }
    }
}
