using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnemiesWaves.Configuration;
using EnemiesWaves.Enemies;
using EnemiesWaves.Pooling;
using UnityEngine;
using VContainer.Unity;

namespace EnemiesWaves.Spawning
{
    public sealed class WaveSpawner : IStartable, IDisposable
    {
        private readonly WaveSettings _settings;
        private readonly PoolManager _poolManager;
        private readonly SpawnPointReference _spawnPoint;
        private readonly TargetPointReference _targetPoint;
        private readonly HashSet<GameObject> _activeEnemies = new();
        private readonly CancellationTokenSource _cts = new();

        private UniTaskCompletionSource _waveClearCompletionSource;
        private bool _disposed;

        public WaveSpawner(
            WaveSettings settings,
            PoolManager poolManager,
            SpawnPointReference spawnPoint,
            TargetPointReference targetPoint)
        {
            _settings = settings;
            _poolManager = poolManager;
            _spawnPoint = spawnPoint;
            _targetPoint = targetPoint;
        }

        public event Action<int> OnCurrentWaveChanged;
        public event Action<int> OnEnemyActiveCountChanged;
        public event Action OnCompleted;

        public int CurrentWave { get; private set; }
        public int EnemyActiveCount => _activeEnemies.Count;

        public void Start()
        {
            if (!_settings.TryValidate(out var error))
            {
                Debug.LogError($"Wave spawner did not start: {error}");
                PublishState();
                return;
            }

            CurrentWave = 0;
            PublishState();
            RunScenarioAsync(_cts.Token).Forget();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cts.Cancel();
            _cts.Dispose();
            _activeEnemies.Clear();
        }

        private async UniTask RunScenarioAsync(CancellationToken ct)
        {
            try
            {
                for (var waveIndex = 0; waveIndex < _settings.Waves.Count; waveIndex++)
                {
                    ct.ThrowIfCancellationRequested();
                    var wave = _settings.Waves[waveIndex];
                    CurrentWave = waveIndex + 1;
                    OnCurrentWaveChanged?.Invoke(CurrentWave);
                    _waveClearCompletionSource = new UniTaskCompletionSource();

                    foreach (var step in wave.SpawnSteps)
                    {
                        for (var spawnIndex = 0; spawnIndex < step.Count; spawnIndex++)
                        {
                            ct.ThrowIfCancellationRequested();
                            Spawn(step);

                            if (spawnIndex < step.Count - 1 && step.SpawnInterval > 0f)
                            {
                                await UniTask.WaitForSeconds(
                                    step.SpawnInterval,
                                    delayTiming: PlayerLoopTiming.Update,
                                    cancellationToken: ct);
                            }
                        }
                    }

                    if (_activeEnemies.Count > 0)
                    {
                        await _waveClearCompletionSource.Task.AttachExternalCancellation(ct);
                    }

                    if (wave.DelayAfterClear > 0f)
                    {
                        await UniTask.WaitForSeconds(
                            wave.DelayAfterClear,
                            delayTiming: PlayerLoopTiming.Update,
                            cancellationToken: ct);
                    }
                }

                CurrentWave = _settings.Waves.Count;
                OnCurrentWaveChanged?.Invoke(CurrentWave);
                OnCompleted?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Scope disposal intentionally stops the scenario without an error.
            }
        }

        private void Spawn(SpawnStep step)
        {
            var instance = _poolManager.Spawn(
                step.EnemyPrefab,
                _spawnPoint.Transform.position,
                Quaternion.identity);

            var movement = instance.GetComponent<EnemyMovement>();
            var returnToPool = instance.GetComponent<ReturnToPoolOnTargetReached>();
            if (movement == null || returnToPool == null)
            {
                Debug.LogError($"Enemy prefab '{step.EnemyPrefab.name}' has incomplete enemy components.");
                _poolManager.Despawn(instance);
                return;
            }

            _activeEnemies.Add(instance);
            OnEnemyActiveCountChanged?.Invoke(_activeEnemies.Count);
            movement.Initialize(_targetPoint.Transform);
            returnToPool.Initialize(_targetPoint.Transform, OnEnemyReturned);
        }

        private void OnEnemyReturned(GameObject enemy)
        {
            if (!_activeEnemies.Remove(enemy)) return;

            OnEnemyActiveCountChanged?.Invoke(_activeEnemies.Count);
            if (_activeEnemies.Count == 0)
            {
                _waveClearCompletionSource?.TrySetResult();
            }
        }

        private void PublishState()
        {
            OnCurrentWaveChanged?.Invoke(CurrentWave);
            OnEnemyActiveCountChanged?.Invoke(_activeEnemies.Count);
        }
    }
}
