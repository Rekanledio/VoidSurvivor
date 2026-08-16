using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Enemy;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Pulse Gun (M6.2): single-target ranged weapon with a high rate of fire.
    /// Auto-attack loop (Time.time-based cooldown from WeaponData.AttackCooldown),
    /// minimal targeting (nearest live Enemy within WeaponData.Range), fires one
    /// <see cref="PulseProjectile"/> at a time with Source = the player. No
    /// scatter, no return flight, no area attack, no projectile pooling (M7).
    /// </summary>
    [DisallowMultipleComponent]
    public class PulseGun : WeaponController
    {
        [SerializeField, Tooltip("Projectile prefab fired at the current target.")]
        private GameObject projectilePrefab;

        [SerializeField, Tooltip("Projectile flight speed.")]
        private float projectileSpeed = 12f;

        private Transform _target;
        private float _nextAttackTime;

        private void Start()
        {
            // M6.2: the default Pulse Gun equips itself into slot 0 of the
            // player's WeaponManager (empty slots only). The attack loop below
            // is self-driven; weapon switching logic arrives with later tasks.
            var manager = GetComponentInParent<WeaponManager>();
            if (manager != null && manager.GetWeapon(0) == null)
            {
                manager.Equip(0, this);
            }
        }

        private void Update()
        {
            if (Data == null) return;

            // Re-acquire only when we have no valid in-range target (not every frame).
            if (!HasValidTarget())
            {
                AcquireTarget();
            }

            if (_target == null || Time.time < _nextAttackTime) return;

            FireAt(_target);
            _nextAttackTime = Time.time + EffectiveAttackCooldown;
        }

        private bool HasValidTarget()
        {
            if (_target == null) return false;
            if (_target.GetComponent<EnemyHealth>() == null || _target.GetComponent<EnemyHealth>().IsDead) return false;
            if (Vector2.Distance(transform.position, _target.position) > EffectiveRange) return false;
            return true;
        }

        /// <summary>Picks the nearest live EnemyHealth within Range (minimal targeting).</summary>
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

        private void FireAt(Transform target)
        {
            // M7.2.1: pooled spawn — Pulse Gun and Scatter Blaster share the
            // same static PulseProjectile pool.
            Vector2 direction = (Vector2)(target.position - transform.position);
            PulseProjectile.Spawn(projectilePrefab, transform.position, Owner, direction, projectileSpeed, EffectiveDamage);
        }
    }
}
