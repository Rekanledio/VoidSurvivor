using UnityEngine;
using VoidSurvivor.Combat;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Formal Player → Enemy attack entry (M5.4). Attack(target) routes a
    /// <see cref="DamageRequest"/> with this player as Source through
    /// <see cref="CombatSystem"/>; base damage comes from PlayerStats.Damage.
    /// M6.2 adds Attack(target, damage) so weapons can supply their own damage
    /// (e.g. WeaponData.BaseDamage) while the old API stays regression-compatible.
    /// No auto-attack loop, no weapon/projectile logic (M6 owns weapon cycles) —
    /// this is the single explicit attack API weapons will call.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerAttack : MonoBehaviour
    {
        private PlayerStats _stats;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        /// <summary>Attacks with the player's base damage (PlayerStats.Damage).</summary>
        public DamageResult Attack(GameObject target)
        {
            if (_stats == null || target == null)
            {
                return new DamageResult(false, 0f, false);
            }

            return Attack(target, _stats.Damage);
        }

        /// <summary>Attacks with an explicit damage value (e.g. a weapon's base damage).</summary>
        public DamageResult Attack(GameObject target, float damage)
        {
            if (_stats == null || target == null || damage <= 0f)
            {
                return new DamageResult(false, 0f, false);
            }

            // Target validity (IDamageable, dead) is handled inside CombatSystem;
            // EnemyKilled/Pickup flow from there as usual.
            return CombatSystem.ApplyDamage(new DamageRequest(gameObject, target, damage));
        }
    }
}
