using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Combat
{
    /// <summary>
    /// Unified combat entry point (M5.1). ApplyDamage routes a
    /// <see cref="DamageRequest"/> to whatever <see cref="IDamageable"/> the
    /// target exposes, publishes <see cref="DamageApplied"/> and returns a
    /// minimal <see cref="DamageResult"/>. Static service (mirrors EventBus);
    /// no per-frame allocations, no Player/Enemy branches. Health classes keep
    /// owning their own HP/death state.
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

            return new DamageResult(true, request.Damage, damageable.IsDead);
        }
    }
}
