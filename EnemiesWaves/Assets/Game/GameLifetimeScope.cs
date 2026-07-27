using EnemiesWaves.Configuration;
using EnemiesWaves.Pooling;
using EnemiesWaves.Spawning;
using EnemiesWaves.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace EnemiesWaves
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private WaveSettings _waveSettings;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private WaveHudView _waveHudView;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_waveSettings == null || _spawnPoint == null || _targetPoint == null || _poolRoot == null || _waveHudView == null)
            {
                Debug.LogError("GameLifetimeScope is missing one or more required scene references.", this);
                return;
            }

            builder.RegisterInstance(_waveSettings);
            builder.RegisterInstance<ISpawnPointReference>(new SpawnPointReference(_spawnPoint));
            builder.RegisterInstance<ITargetPointReference>(new TargetPointReference(_targetPoint));
            builder.RegisterInstance<IPoolRootReference>(new PoolRootReference(_poolRoot));
            builder.RegisterComponent(_waveHudView);

            builder.Register<PoolManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<PoolPreloader>().AsSelf();
            builder.RegisterEntryPoint<WaveSpawner>().AsSelf().AsImplementedInterfaces();
            builder.Register<WaveHudModel>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<WaveHudPresenter>();
        }
    }
}
