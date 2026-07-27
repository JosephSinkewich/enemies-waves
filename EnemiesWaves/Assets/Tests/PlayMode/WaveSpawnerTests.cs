using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EnemiesWaves.Configuration;
using EnemiesWaves.Enemies;
using EnemiesWaves.Pooling;
using EnemiesWaves.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EnemiesWaves.Tests.PlayMode
{
    public sealed class WaveSpawnerTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in _createdObjects)
            {
                UnityEngine.Object.Destroy(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void Start_WithInvalidSettings_PublishesInitialStateWithoutSpawning()
        {
            var settings = CreateSettings();
            SetField<List<WaveDefinition>>(settings, "_waves", null);
            var poolManager = new FakePoolManager(_createdObjects);
            var spawner = CreateSpawner(settings, poolManager);
            var waveStates = new List<int>();
            var enemyCounts = new List<int>();
            spawner.OnCurrentWaveChanged += waveStates.Add;
            spawner.OnEnemyActiveCountChanged += enemyCounts.Add;

            LogAssert.Expect(LogType.Error, "Wave spawner did not start: Waves must not be null.");
            spawner.Start();

            Assert.That(poolManager.SpawnCount, Is.Zero);
            Assert.That(waveStates, Is.EqualTo(new[] { 0 }));
            Assert.That(enemyCounts, Is.EqualTo(new[] { 0 }));
            spawner.Dispose();
        }

        [UnityTest]
        public IEnumerator Start_ValidWave_ReturnsEnemyAndCompletesScenario()
        {
            var prefab = CreateEnemyPrefab();
            var settings = CreateSettings(CreateWave(CreateStep(prefab, 1, 0f)));
            var poolManager = new FakePoolManager(_createdObjects);
            var spawner = CreateSpawner(settings, poolManager);
            var waveStates = new List<int>();
            var enemyCounts = new List<int>();
            var completed = false;
            spawner.OnCurrentWaveChanged += waveStates.Add;
            spawner.OnEnemyActiveCountChanged += enemyCounts.Add;
            spawner.OnCompleted += () => completed = true;

            spawner.Start();
            yield return null;
            yield return null;

            Assert.That(poolManager.SpawnCount, Is.EqualTo(1));
            Assert.That(poolManager.DespawnedInstances, Has.Count.EqualTo(1));
            Assert.That(waveStates, Does.Contain(1));
            Assert.That(enemyCounts, Does.Contain(1));
            Assert.That(enemyCounts[^1], Is.Zero);
            Assert.That(completed, Is.True);
            spawner.Dispose();
        }

        [UnityTest]
        public IEnumerator Dispose_StopsScenarioBeforeRemainingSpawns()
        {
            var prefab = CreateEnemyPrefab();
            var settings = CreateSettings(CreateWave(CreateStep(prefab, 2, 10f)));
            var poolManager = new FakePoolManager(_createdObjects);
            var spawner = CreateSpawner(settings, poolManager);
            var completed = false;
            spawner.OnCompleted += () => completed = true;

            spawner.Start();
            spawner.Dispose();
            yield return null;

            Assert.That(poolManager.SpawnCount, Is.EqualTo(1));
            Assert.That(completed, Is.False);
        }

        private WaveSpawner CreateSpawner(WaveSettings settings, IPoolManager poolManager)
        {
            var spawnPoint = CreateGameObject("Spawn point");
            var targetPoint = CreateGameObject("Target point");
            targetPoint.transform.position = spawnPoint.transform.position;
            return new WaveSpawner(
                settings,
                poolManager,
                new TransformReference(spawnPoint.transform),
                new TransformReference(targetPoint.transform));
        }

        private WaveSettings CreateSettings(params WaveDefinition[] waves)
        {
            var settings = ScriptableObject.CreateInstance<WaveSettings>();
            _createdObjects.Add(settings);
            SetField(settings, "_waves", new List<WaveDefinition>(waves));
            return settings;
        }

        private static WaveDefinition CreateWave(params SpawnStep[] steps)
        {
            var wave = new WaveDefinition();
            SetField(wave, "_spawnSteps", new List<SpawnStep>(steps));
            return wave;
        }

        private static SpawnStep CreateStep(GameObject prefab, int count, float interval)
        {
            var step = new SpawnStep();
            SetField(step, "_enemyPrefab", prefab);
            SetField(step, "_count", count);
            SetField(step, "_spawnInterval", interval);
            return step;
        }

        private GameObject CreateEnemyPrefab()
        {
            var prefab = CreateGameObject("Enemy prefab");
            prefab.AddComponent<EnemyMovement>();
            prefab.AddComponent<ReturnToPoolOnTargetReached>();
            return prefab;
        }

        private GameObject CreateGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        private sealed class TransformReference : ISpawnPointReference, ITargetPointReference
        {
            public TransformReference(Transform transform)
            {
                Transform = transform;
            }

            public Transform Transform { get; }
        }

        private sealed class FakePoolManager : IPoolManager
        {
            private readonly List<UnityEngine.Object> _createdObjects;

            public FakePoolManager(List<UnityEngine.Object> createdObjects)
            {
                _createdObjects = createdObjects;
            }

            public int SpawnCount { get; private set; }
            public List<GameObject> DespawnedInstances { get; } = new();

            public void Prewarm(GameObject prefab, int count) { }

            public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
            {
                SpawnCount++;
                var instance = new GameObject($"Test enemy {SpawnCount}");
                _createdObjects.Add(instance);
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.AddComponent<EnemyMovement>();
                var returnToPool = instance.AddComponent<ReturnToPoolOnTargetReached>();
                returnToPool.Construct(this);
                return instance;
            }

            public void Despawn(GameObject instance)
            {
                DespawnedInstances.Add(instance);
                instance.SetActive(false);
            }
        }
    }
}
