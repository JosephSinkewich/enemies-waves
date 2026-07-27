using System.Collections.Generic;
using EnemiesWaves.Configuration;
using VContainer.Unity;

namespace EnemiesWaves.Pooling
{
    public sealed class PoolPreloader : IInitializable
    {
        private readonly WaveSettings _settings;
        private readonly IPoolManager _poolManager;

        public PoolPreloader(WaveSettings settings, IPoolManager poolManager)
        {
            _settings = settings;
            _poolManager = poolManager;
        }

        public void Initialize()
        {
            var requirements = new Dictionary<UnityEngine.GameObject, int>();
            foreach (var wave in _settings.Waves)
            {
                foreach (var step in wave.SpawnSteps)
                {
                    if (step.Count == 0) continue;
                    requirements.TryGetValue(step.EnemyPrefab, out var current);
                    requirements[step.EnemyPrefab] = current + step.Count;
                }
            }

            foreach (var requirement in requirements)
            {
                _poolManager.Prewarm(requirement.Key, requirement.Value);
            }
        }
    }
}
