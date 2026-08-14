using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Enemy hit points. Owns CurrentHP, clamps it to [0, MaxHP] and publishes
    /// <see cref="EnemyDied"/> exactly once via the EventBus (mirrors PlayerHealth).
    /// Implements <see cref="IDamageable"/> so combat can address it uniformly (M5.1);
    /// no combat logic here.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        private EnemyStats _stats;
        private float _currentHP;
        private bool _isDead;

        public float CurrentHP => _currentHP;
        public float MaxHP => _stats != null ? _stats.MaxHP : 0f;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            if (_stats == null)
            {
                Debug.LogError($"[EnemyHealth] Missing EnemyStats on '{gameObject.name}'.");
                return;
            }

            _currentHP = _stats.MaxHP;
        }

        /// <summary>Applies damage; clamps HP to >= 0; triggers death exactly once.</summary>
        public void TakeDamage(float damage)
        {
            if (_isDead || damage <= 0f) return;

            _currentHP = Mathf.Max(0f, _currentHP - damage);

            if (_currentHP <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (_isDead) return; // guard against duplicate death

            _isDead = true;
            _currentHP = 0f;
            EventBus.Publish(new EnemyDied(gameObject));
        }
    }
}
