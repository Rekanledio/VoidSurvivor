using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Player hit points. Owns CurrentHP, clamps it to [0, MaxHP] and
    /// publishes <see cref="PlayerDied"/> exactly once via the EventBus.
    /// Armor comes from PlayerStats and is applied as simple flat reduction (M3 minimal form).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealth : MonoBehaviour
    {
        private PlayerStats _stats;
        private float _currentHP;
        private bool _isDead;

        public float CurrentHP => _currentHP;
        public float MaxHP => _stats != null ? _stats.MaxHP : 0f;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            if (_stats == null)
            {
                Debug.LogError($"[PlayerHealth] Missing PlayerStats on '{gameObject.name}'.");
                return;
            }

            _currentHP = _stats.MaxHP;
        }

        /// <summary>Applies flat-reduced damage; clamps HP to >= 0; triggers death once.</summary>
        public void TakeDamage(float damage)
        {
            if (_isDead || damage <= 0f) return;

            float reduced = Mathf.Max(0f, damage - _stats.Armor);
            _currentHP = Mathf.Max(0f, _currentHP - reduced);

            if (_currentHP <= 0f)
            {
                Die();
            }
        }

        /// <summary>Restores HP; clamps to MaxHP. No effect after death.</summary>
        public void Heal(float amount)
        {
            if (_isDead || amount <= 0f) return;

            _currentHP = Mathf.Min(MaxHP, _currentHP + amount);
        }

        /// <summary>Restores HP to MaxHP. No effect after death.</summary>
        public void FullHeal()
        {
            if (_isDead) return;

            _currentHP = MaxHP;
        }

        private void Die()
        {
            if (_isDead) return; // guard against duplicate death

            _isDead = true;
            _currentHP = 0f;
            EventBus.Publish(new PlayerDied());
        }
    }
}
