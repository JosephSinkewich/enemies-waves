using EnemiesWaves.Pooling;
using System;
using UnityEngine;
using VContainer;

namespace EnemiesWaves.Enemies
{
    public sealed class ReturnToPoolOnTargetReached : MonoBehaviour
    {
        private const float TARGET_DISTANCE = 0.01f;

        private PoolManager _poolManager;
        private Transform _target;
        private Action<GameObject> _returnCallback;
        private bool _isInitialized;

        [Inject]
        public void Construct(PoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        public void Initialize(Transform targetPoint, Action<GameObject> returnCallback)
        {
            _target = targetPoint;
            _returnCallback = returnCallback;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || _target == null) return;

            var offset = (Vector2)(transform.position - _target.position);
            if (offset.sqrMagnitude > TARGET_DISTANCE * TARGET_DISTANCE) return;

            _isInitialized = false;
            _returnCallback?.Invoke(gameObject);
            _returnCallback = null;
            _poolManager.Despawn(gameObject);
        }

        private void OnDisable()
        {
            _target = null;
            _isInitialized = false;
            _returnCallback = null;
        }
    }
}
