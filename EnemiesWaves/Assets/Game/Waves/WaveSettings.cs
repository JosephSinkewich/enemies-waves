using System.Collections.Generic;
using UnityEngine;

namespace EnemiesWaves.Configuration
{
    [CreateAssetMenu(menuName = "Enemies Waves/Wave Settings", fileName = "WaveSettings")]
    public sealed class WaveSettings : ScriptableObject
    {
        [SerializeField] private List<WaveDefinition> _waves = new();

        public IReadOnlyList<WaveDefinition> Waves => _waves;

        public bool TryValidate(out string error)
        {
            if (_waves == null)
            {
                error = "Waves must not be null.";
                return false;
            }

            for (var waveIndex = 0; waveIndex < _waves.Count; waveIndex++)
            {
                if (!_waves[waveIndex].TryValidate(waveIndex, out error))
                {
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            TryValidate(out _);
        }
    }

}
