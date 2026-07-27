using System.Collections.Generic;
using System.Reflection;
using EnemiesWaves.Configuration;
using EnemiesWaves.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace EnemiesWaves.Tests.Editor
{
    public sealed class WaveConfigurationTests
    {
        private readonly List<Object> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in _createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void SpawnStepTryValidate_RejectsNegativeCountAndInterval()
        {
            var step = CreateStep(count: -1, spawnInterval: -0.1f);

            var isValid = step.TryValidate(0, 0, out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Wave 1, step 1: Count and SpawnInterval must be non-negative."));
        }

        [Test]
        public void SpawnStepTryValidate_RequiresPrefabForPositiveCount()
        {
            var step = CreateStep(count: 1);

            var isValid = step.TryValidate(1, 2, out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Wave 2, step 3: EnemyPrefab is required when Count is positive."));
        }

        [Test]
        public void SpawnStepTryValidate_RequiresEnemyComponents()
        {
            var prefab = CreateGameObject("Incomplete enemy");
            var step = CreateStep(prefab, count: 1);

            var isValid = step.TryValidate(0, 0, out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Wave 1, step 1: EnemyPrefab must contain EnemyMovement and ReturnToPoolOnTargetReached."));
        }

        [Test]
        public void SpawnStepTryValidate_AcceptsZeroCountWithoutPrefab()
        {
            var step = CreateStep(count: 0);

            var isValid = step.TryValidate(0, 0, out var error);

            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void WaveDefinitionTryValidate_RejectsNegativeDelay()
        {
            var wave = new WaveDefinition();
            SetField(wave, "_delayAfterClear", -0.1f);

            var isValid = wave.TryValidate(2, out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Wave 3: DelayAfterClear must be non-negative."));
        }

        [Test]
        public void WaveDefinitionTryValidate_ReturnsNestedStepError()
        {
            var wave = new WaveDefinition();
            SetField(wave, "_spawnSteps", new List<SpawnStep> { CreateStep(count: 1) });

            var isValid = wave.TryValidate(0, out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Wave 1, step 1: EnemyPrefab is required when Count is positive."));
        }

        [Test]
        public void WaveSettingsTryValidate_RejectsNullWaves()
        {
            var settings = ScriptableObject.CreateInstance<WaveSettings>();
            _createdObjects.Add(settings);
            SetField<List<WaveDefinition>>(settings, "_waves", null);

            var isValid = settings.TryValidate(out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Waves must not be null."));
        }

        [Test]
        public void WaveSettingsTryValidate_AcceptsValidConfiguration()
        {
            var prefab = CreateGameObject("Enemy");
            prefab.AddComponent<EnemyMovement>();
            prefab.AddComponent<ReturnToPoolOnTargetReached>();

            var wave = new WaveDefinition();
            SetField(wave, "_spawnSteps", new List<SpawnStep> { CreateStep(prefab, 1, 0f) });
            var settings = ScriptableObject.CreateInstance<WaveSettings>();
            _createdObjects.Add(settings);
            SetField(settings, "_waves", new List<WaveDefinition> { wave });

            var isValid = settings.TryValidate(out var error);

            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }

        private SpawnStep CreateStep(GameObject prefab = null, int count = 0, float spawnInterval = 0f)
        {
            var step = new SpawnStep();
            SetField(step, "_enemyPrefab", prefab);
            SetField(step, "_count", count);
            SetField(step, "_spawnInterval", spawnInterval);
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
    }
}
