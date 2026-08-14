using UnityEngine;
using VoidSurvivor.Combat;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Formal Player → Enemy attack entry (M5.4). Attack(target) routes a
    /// <see cref="DamageRequest"/> with this player as Source through
    /// <see cref="CombatSystem"/>; base damage comes from PlayerStats.Damage.
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

        /// <summary>Attacks the given target through the unified combat entry.</summary>
        public DamageResult Attack(GameObject target)
        {
            if (_stats == null || target == null)
            {
                return new DamageResult(false, 0f, false);
            }

            // Target validity (IDamageable, dead, damage > 0) is handled inside
            // CombatSystem; EnemyKilled/Pickup flow from there as usual.
            return CombatSystem.ApplyDamage(new DamageRequest(gameObject, target, _stats.Damage));
        }
    }
}
