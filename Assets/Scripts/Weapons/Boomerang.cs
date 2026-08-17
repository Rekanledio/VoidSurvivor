using UnityEngine;
using VoidSurvivor.Enemy;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Boomerang (M6.4): out-and-return weapon. Auto-attack loop (Time.time
    /// cooldown), minimal targeting (nearest live EnemyHealth within Range used
    /// only for the initial throw direction), fires a single
    /// <see cref="BoomerangProjectile"/> at a time (single-flight rule: no new
    /// throw while one is still active). The projectile returns to the player.
    /// No homing, no multi-target, no combo/upgrade.
    /// </summary>
    [DisallowMultipleComponent]
    public class Boomerang : WeaponController
    {
        [SerializeField, Tooltip("Boomerang projectile prefab.")]
        private GameObject projectilePrefab;

        private Transform _target;
        private float _nextAttackTime;

        private void Update()
        {
            if (!GameplayActive || Data is not BoomerangData boom) return; // M11.4: weapons act only while Playing

            // Single-flight rule: do not throw while one is still out.
            if (BoomerangProjectile.ActiveCount > 0) return;

            if (!HasValidTarget())
            {
                AcquireTarget();
            }

            if (_target == null || Time.time < _nextAttackTime) return;

            ThrowAt(_target, boom);
            _nextAttackTime = Time.time + EffectiveAttackCooldown;
        }

        private bool HasValidTarget()
        {
            if (_target == null) return false;
            if (_target.GetComponent<EnemyHealth>() == null || _target.GetComponent<EnemyHealth>().IsDead) return false;
            if (Vector2.Distance(transform.position, _target.position) > EffectiveRange) return false;
            return true;
        }

        /// <summary>Nearest live EnemyHealth within Range — initial throw direction.</summary>
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

        private void ThrowAt(Transform target, BoomerangData boom)
        {
            // M7.2.1: pooled spawn — ActiveCount is maintained inside Spawn/DespawnSelf.
            Vector2 direction = (Vector2)(target.position - transform.position);
            BoomerangProjectile.Spawn(projectilePrefab, transform.position, Owner,
                Owner != null ? Owner.transform : transform, direction,
                boom.OutSpeed, boom.ReturnSpeed, boom.MaxDistance, EffectiveDamage);
        }
    }
}
