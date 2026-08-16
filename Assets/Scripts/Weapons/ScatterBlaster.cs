using UnityEngine;
using VoidSurvivor.Enemy;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Scatter Blaster (M6.3): multi-projectile spread weapon. Auto-attack loop
    /// (Time.time cooldown from WeaponData.AttackCooldown), minimal targeting
    /// (nearest live EnemyHealth within Range — used only as the spread center
    /// direction), fires the configured count of <see cref="PulseProjectile"/>s
    /// simultaneously in a deterministic, uniform fan. Each projectile has
    /// Source = player and damage = ScatterBlasterData.BaseDamage. Lower fire
    /// rate, more pellets than Pulse Gun. No random spread, no upgrade, no pool.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScatterBlaster : WeaponController
    {
        [SerializeField, Tooltip("Projectile prefab (reuses the Pulse projectile).")]
        private GameObject projectilePrefab;

        [SerializeField, Tooltip("Projectile flight speed.")]
        private float projectileSpeed = 12f;

        private Transform _target;
        private float _nextAttackTime;

        private void Update()
        {
            if (Data is not ScatterBlasterData scatter) return;

            // Re-acquire only when the current target is gone/invalid (not every frame).
            if (!HasValidTarget())
            {
                AcquireTarget();
            }

            if (_target == null || Time.time < _nextAttackTime) return;

            FireFanAt(_target, scatter);
            _nextAttackTime = Time.time + EffectiveAttackCooldown;
        }

        private bool HasValidTarget()
        {
            if (_target == null) return false;
            if (_target.GetComponent<EnemyHealth>() == null || _target.GetComponent<EnemyHealth>().IsDead) return false;
            if (Vector2.Distance(transform.position, _target.position) > EffectiveRange) return false;
            return true;
        }

        /// <summary>Nearest live EnemyHealth within Range — center direction for the fan.</summary>
        private void AcquireTarget()
        {
            _target = null;

            var hits = Physics2D.OverlapCircleAll(transform.position, EffectiveRange);
            float bestDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent(out EnemyHealth enemy) && !enemy.IsDead)
                {
                    float distance = Vector2.Distance(transform.position, hits[i].transform.position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        _target = hits[i].transform;
                    }
                }
            }
        }

        /// <summary>Fires the configured count of projectiles in a uniform fan.</summary>
        private void FireFanAt(Transform target, ScatterBlasterData scatter)
        {
            Vector2 baseDirection = (Vector2)(target.position - transform.position);

            int count = scatter.ProjectileCount;
            if (count <= 0) return;

            if (count == 1)
            {
                SpawnProjectile(baseDirection, scatter);
                return;
            }

            float halfSpread = scatter.SpreadAngle * 0.5f;
            float step = scatter.SpreadAngle / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float angle = -halfSpread + step * i; // -half ... +half, symmetric, even spacing
                SpawnProjectile(Rotate(baseDirection, angle), scatter);
            }
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
        }

        private void SpawnProjectile(Vector2 direction, ScatterBlasterData scatter)
        {
            // M7.2.1: pooled spawn — shares the same static PulseProjectile pool
            // as Pulse Gun.
            PulseProjectile.Spawn(projectilePrefab, transform.position, Owner, direction, projectileSpeed, EffectiveDamage);
        }
    }
}
