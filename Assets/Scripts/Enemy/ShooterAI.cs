using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Shooter behavior (M4.4): keeps a reasonable distance and fires a minimal
    /// <see cref="Projectile"/> at the player when in range and off cooldown.
    /// Movement: approach only when farther than AttackRange, stop once in range
    /// (no kiting/wall-hugging logic). Reuses EnemyController refs — no per-frame
    /// GetComponent/Find. Stops moving/attacking when dead.
    ///
    /// M7.2.2: implements IPoolable so the attack cooldown timer resets on
    /// respawn (no stale cooldown from a previous life).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class ShooterAI : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Minimal M4.4 projectile prefab. M5 replaces this with the unified combat pipeline.")]
        private GameObject projectilePrefab;

        private EnemyController _controller;
        private float _nextAttackTime;

        public void OnSpawn()
        {
            _nextAttackTime = 0f; // no stale cooldown from a previous life
        }

        public void OnDespawn()
        {
            // Movement/attack stop automatically when inactive; nothing to clear.
        }

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
        }

        private void FixedUpdate()
        {
            if (!EnemyController.GameplayActive) return; // M11.4
            if (_controller == null || _controller.Health == null || _controller.Health.IsDead) return;
            if (_controller.Body == null || _controller.Stats == null || _controller.Target == null) return;
            if (_controller.Target.IsDead) return; // player dead → no movement/attack

            Vector2 toTarget = (Vector2)_controller.Target.transform.position - _controller.Body.position;
            float distance = toTarget.magnitude;
            float range = _controller.Stats.AttackRange;

            // Movement: approach only when outside AttackRange; stop once in range.
            if (distance > range)
            {
                Vector2 next = _controller.Body.position + toTarget.normalized * (_controller.Stats.MoveSpeed * Time.fixedDeltaTime);
                _controller.Body.MovePosition(next);
            }

            // Attack: in range, player alive, and cooldown elapsed (Time.time is a
            // stable, testable clock independent of frame timing).
            if (distance <= range && Time.time >= _nextAttackTime && projectilePrefab != null)
            {
                Fire((Vector2)_controller.Target.transform.position - _controller.Body.position);
                _nextAttackTime = Time.time + _controller.Stats.AttackCooldown;
            }
        }

        private void Fire(Vector2 direction)
        {
            // M7.2.1: pooled spawn — behavior (damage/speed/source) unchanged.
            Projectile.Spawn(projectilePrefab, _controller.Body.position, direction, _controller.Stats.Damage, gameObject);
        }
    }
}
