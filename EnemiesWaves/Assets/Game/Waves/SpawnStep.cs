using System;
using EnemiesWaves.Enemies;
using UnityEngine;

namespace EnemiesWaves.Configuration
{
    [Serializable]
    public sealed class SpawnStep
    {
        [SerializeField] private GameObject _enemyPrefab;
        [Min(0)][SerializeField] private int _count;
        [Min(0f)][SerializeField] private float _spawnInterval;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int Count => _count;
        public float SpawnInterval => _spawnInterval;

        public bool TryValidate(int waveIndex, int stepIndex, out string error)
        {
            var prefix = $"Wave {waveIndex + 1}, step {stepIndex + 1}";
            if (_count < 0 || _spawnInterval < 0f)
            {
                error = $"{prefix}: Count and SpawnInterval must be non-negative.";
                return false;
            }

            if (_count > 0 && _enemyPrefab == null)
            {
                error = $"{prefix}: EnemyPrefab is required when Count is positive.";
                return false;
            }

            if (_count > 0
                && (_enemyPrefab.GetComponent<EnemyMovement>() == null
                    || _enemyPrefab.GetComponent<ReturnToPoolOnTargetReached>() == null))
            {
                error = $"{prefix}: EnemyPrefab must contain EnemyMovement and ReturnToPoolOnTargetReached.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
