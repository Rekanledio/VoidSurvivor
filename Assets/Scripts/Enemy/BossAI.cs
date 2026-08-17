using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;
using VoidSurvivor.Player;
using VoidSurvivor.Weapons;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Boss behavior (M8.3 + M10.2): pursues the player via Rigidbody2D.MovePosition
    /// at Stats.MoveSpeed and deals CONTACT damage (OnTriggerEnter2D → CombatSystem,
    /// Source = boss, Damage = Stats.Damage, cooldown = Stats.AttackCooldown).
    ///
    /// M10.2 adds ONE active skill — Boss Projectile: every SkillCooldown seconds,
    /// while alive, while Playing, with a live non-dead player within SkillRange,
    /// fires a single PulseProjectile at the player's CURRENT position (direction
    /// captured at spawn; no homing). The projectile reuses the shared
    /// PulseProjectile ObjectPool and routes damage through CombatSystem
    /// (Source = boss, Damage = the boss's runtime Stats.Damage, which inherits the
    /// WaveMultiplier). Skill runtime state (cooldown timer) resets on pool reuse.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class BossAI : MonoBehaviour, IPoolable
    {
        [Header("Boss Projectile Skill (M10.2 implementation parameters, not design values)")]
        [SerializeField, Tooltip("Projectile prefab fired by the skill (reuses the Pulse projectile).")]
        private GameObject projectilePrefab;

        [SerializeField, Tooltip("Seconds between skill activations.")]
        private float skillCooldown = 3f;

        [SerializeField, Tooltip("Projectile flight speed.")]
        private float projectileSpeed = 6f;

        [SerializeField, Tooltip("Max range at which the skill will fire.")]
        private float skillRange = 10f;

        private EnemyController _controller;
        private float _nextContactAttackTime;
        private float _nextSkillTime;

        public void OnSpawn()
        {
            _nextContactAttackTime = 0f; // no stale cooldown across lives
            _nextSkillTime = 0f;         // M10.2: skill ready on spawn
        }

        public void OnDespawn()
        {
            // Movement/contact/skill stop automatically when inactive; nothing to clear.
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

            TryFireSkill(toTarget);
        }

        /// <summary>
        /// Fires one projectile at the player's current position when the skill
        /// cooldown is ready, the game is Playing, and the player is in range.
        /// </summary>
        private void TryFireSkill(Vector2 toTarget)
        {
            if (projectilePrefab == null) return;
            if (Time.time < _nextSkillTime) return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_controller.Target == null || _controller.Target.IsDead) return;

            float distance = toTarget.magnitude;
            if (distance > skillRange) return;

            // Direction captured at spawn; the projectile does NOT home.
            PulseProjectile.Spawn(projectilePrefab, _controller.Body.position, gameObject,
                toTarget.normalized, projectileSpeed, _controller.Stats.Damage);
            _nextSkillTime = Time.time + skillCooldown;
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
