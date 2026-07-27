using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnemiesWaves.Configuration
{
    [Serializable]
    public sealed class WaveDefinition
    {
        [SerializeField] private List<SpawnStep> _spawnSteps = new();
        [Min(0f)][SerializeField] private float _delayAfterClear;

        public IReadOnlyList<SpawnStep> SpawnSteps => _spawnSteps;
        public float DelayAfterClear => _delayAfterClear;

        public bool TryValidate(int waveIndex, out string error)
        {
            if (_spawnSteps == null)
            {
                error = $"Wave {waveIndex + 1}: SpawnSteps must not be null.";
                return false;
            }

            if (_delayAfterClear < 0f)
            {
                error = $"Wave {waveIndex + 1}: DelayAfterClear must be non-negative.";
                return false;
            }

            for (var stepIndex = 0; stepIndex < _spawnSteps.Count; stepIndex++)
            {
                if (!_spawnSteps[stepIndex].TryValidate(waveIndex, stepIndex, out error))
                {
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}
