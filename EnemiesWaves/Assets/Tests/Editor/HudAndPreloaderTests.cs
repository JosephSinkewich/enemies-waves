using System;
using System.Collections.Generic;
using System.Reflection;
using EnemiesWaves.Configuration;
using EnemiesWaves.Pooling;
using EnemiesWaves.Spawning;
using EnemiesWaves.UI;
using NUnit.Framework;
using UnityEngine;

namespace EnemiesWaves.Tests.Editor
{
    public sealed class HudAndPreloaderTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in _createdObjects)
            {
                UnityEngine.Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void WaveHudState_StoresProvidedValues()
        {
            var state = new WaveHudState(3, 7);

            Assert.That(state.CurrentWave, Is.EqualTo(3));
            Assert.That(state.ActiveEnemies, Is.EqualTo(7));
        }

        [Test]
        public void WaveHudModel_UpdatesStateAndNotifiesSubscribers()
        {
            var spawner = new FakeWaveSpawner(currentWave: 1, activeEnemyCount: 2);
            using var model = new WaveHudModel(spawner);
            var states = new List<WaveHudState>();
            model.OnChanged += states.Add;

            spawner.PublishWave(2);
            spawner.PublishEnemyCount(5);

            Assert.That(model.State.CurrentWave, Is.EqualTo(2));
            Assert.That(model.State.ActiveEnemies, Is.EqualTo(5));
            Assert.That(states, Has.Count.EqualTo(2));
            Assert.That(states[0].CurrentWave, Is.EqualTo(2));
            Assert.That(states[0].ActiveEnemies, Is.EqualTo(2));
            Assert.That(states[1].CurrentWave, Is.EqualTo(2));
            Assert.That(states[1].ActiveEnemies, Is.EqualTo(5));
        }

        [Test]
        public void WaveHudModelDispose_UnsubscribesFromSpawner()
        {
            var spawner = new FakeWaveSpawner(currentWave: 1, activeEnemyCount: 1);
            var model = new WaveHudModel(spawner);
            model.Dispose();

            spawner.PublishWave(4);
            spawner.PublishEnemyCount(9);

            Assert.That(model.State.CurrentWave, Is.EqualTo(1));
            Assert.That(model.State.ActiveEnemies, Is.EqualTo(1));
        }

        [Test]
        public void PoolPreloader_AggregatesRequirementsByPrefabAndSkipsZeroCountSteps()
        {
            var enemyA = CreateGameObject("Enemy A");
            var enemyB = CreateGameObject("Enemy B");
            var settings = CreateSettings(
                CreateWave(CreateStep(enemyA, 2), CreateStep(enemyB, 3)),
                CreateWave(CreateStep(enemyA, 4), CreateStep(enemyB, 0)));
            var poolManager = new FakePoolManager();
            var preloader = new PoolPreloader(settings, poolManager);

            preloader.Initialize();

            Assert.That(poolManager.PrewarmRequests, Has.Count.EqualTo(2));
            Assert.That(poolManager.PrewarmRequests[enemyA], Is.EqualTo(6));
            Assert.That(poolManager.PrewarmRequests[enemyB], Is.EqualTo(3));
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

        private static SpawnStep CreateStep(GameObject prefab, int count)
        {
            var step = new SpawnStep();
            SetField(step, "_enemyPrefab", prefab);
            SetField(step, "_count", count);
            return step;
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

        private sealed class FakeWaveSpawner : IWaveSpawner
        {
            public FakeWaveSpawner(int currentWave, int activeEnemyCount)
            {
                CurrentWave = currentWave;
                EnemyActiveCount = activeEnemyCount;
            }

            public event Action<int> OnCurrentWaveChanged;
            public event Action<int> OnEnemyActiveCountChanged;
            public event Action OnCompleted;
            public int CurrentWave { get; private set; }
            public int EnemyActiveCount { get; private set; }

            public void PublishWave(int wave)
            {
                CurrentWave = wave;
                OnCurrentWaveChanged?.Invoke(wave);
            }

            public void PublishEnemyCount(int count)
            {
                EnemyActiveCount = count;
                OnEnemyActiveCountChanged?.Invoke(count);
            }
        }

        private sealed class FakePoolManager : IPoolManager
        {
            public Dictionary<GameObject, int> PrewarmRequests { get; } = new();

            public void Prewarm(GameObject prefab, int count)
            {
                PrewarmRequests[prefab] = count;
            }

            public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) => null;
            public void Despawn(GameObject instance) { }
        }
    }
}
