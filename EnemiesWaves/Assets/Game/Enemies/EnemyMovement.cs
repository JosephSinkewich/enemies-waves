using UnityEngine;

namespace EnemiesWaves.Enemies
{
    public sealed class EnemyMovement : MonoBehaviour
    {
        [Min(0.01f)][SerializeField] private float _speed = 1f;

        private Transform _target;

        public void Initialize(Transform targetPoint)
        {
            _target = targetPoint;
        }

        private void Update()
        {
            if (_target == null) return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                _target.position,
                _speed * Time.deltaTime);
        }

        private void OnValidate()
        {
            _speed = Mathf.Max(0.01f, _speed);
        }
    }
}
