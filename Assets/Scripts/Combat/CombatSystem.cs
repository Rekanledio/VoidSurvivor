using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Enemy;

namespace VoidSurvivor.Combat
{
    /// <summary>
    /// Unified combat entry point (M5.1). ApplyDamage routes a
    /// <see cref="DamageRequest"/> to whatever <see cref="IDamageable"/> the
    /// target exposes, publishes <see cref="DamageApplied"/> and returns a
    /// minimal <see cref="DamageResult"/>.
    ///
    /// M5.2 adds kill attribution: when the lethal hit lands on an
    /// <see cref="EnemyHealth"/> and the request carried a valid Source, an
    /// <see cref="EnemyKilled"/> event is published exactly once with that
    /// Source as Killer. Null-source deaths publish EnemyDied only.
    ///
    /// Static service (mirrors EventBus); no per-frame allocations. Health
    /// classes keep owning their own HP/death state.
    /// </summary>
    public static class CombatSystem
    {
        /// <summary>Applies a damage request to the target's IDamageable.</summary>
        public static DamageResult ApplyDamage(in DamageRequest request)
        {
            if (request.Target == null || request.Damage <= 0f)
            {
                return new DamageResult(false, 0f, false);
            }

            // Resolved once per damage event (not a hot path).
            if (!request.Target.TryGetComponent(out IDamageable damageable))
            {
                return new DamageResult(false, 0f, false);
            }

            if (damageable.IsDead)
            {
                return new DamageResult(false, request.Damage, true);
            }

            damageable.TakeDamage(request.Damage);
            EventBus.Publish(new DamageApplied(request.Source, request.Target, request.Damage));

            var result = new DamageResult(true, request.Damage, damageable.IsDead);

            // M5.2 kill attribution: enemy died this hit AND we know the killer.
            // Cross-system dependency Combat → Enemy is deliberate and necessary
            // (kill attribution only applies to enemies); documented in DEVELOPMENT_LOG.
            if (result.TargetDead && request.Source != null && damageable is EnemyHealth)
            {
                EventBus.Publish(new EnemyKilled(request.Target, request.Source));
            }

            return result;
        }
    }
}
