using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// MVP boss behavior (M8.3): pursues the player via Rigidbody2D.MovePosition
    /// at Stats.MoveSpeed and deals CONTACT damage — OnTriggerEnter2D against the
    /// PlayerHealth builds a <see cref="DamageRequest"/> (Source = the boss,
    /// Damage = Stats.Damage) through <see cref="CombatSystem"/>. A minimal
    /// Time.time cooldown (Stats.AttackCooldown) prevents repeated hits from one
    /// continuous contact. No projectiles, no skills, no boss-only frameworks.
    /// Reuses EnemyController references; no direct PlayerHealth.TakeDamage call.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class BossAI : MonoBehaviour, IPoolable
    {
        private EnemyController _controller;
        private float _nextContactAttackTime;

        public void OnSpawn()
        {
            _nextContactAttackTime = 0f; // no stale cooldown across lives
        }

        public void OnDespawn()
        {
            // Movement/contact stop automatically when inactive; nothing to clear.
        }

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
        }

        private void FixedUpdate()
        {
            if (_controller == null || _controller.Health == null || _controller.Health.IsDead) return;
            if (_controller.Body == null || _controller.Stats == null || _controller.Target == null) return;

            Vector2 toTarget = (Vector2)_controller.Target.transform.position - _controller.Body.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Vector2 next = _controller.Body.position + toTarget.normalized * (_controller.Stats.MoveSpeed * Time.fixedDeltaTime);
            _controller.Body.MovePosition(next);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Contact attack: only the player is a valid target.
            if (!other.TryGetComponent(out PlayerHealth _)) return;
            if (Time.time < _nextContactAttackTime) return;
            _nextContactAttackTime = Time.time + _controller.Stats.AttackCooldown;

            CombatSystem.ApplyDamage(new DamageRequest(gameObject, other.gameObject, _controller.Stats.Damage));
        }
    }
}
