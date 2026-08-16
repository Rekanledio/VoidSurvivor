using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Player;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Runtime weapon (M6.1): holds its <see cref="WeaponData"/> and routes
    /// attacks through the player's <see cref="PlayerAttack"/> — never straight
    /// to a health class. Layering: Weapon → PlayerAttack → CombatSystem → Enemy.
    /// No auto-attack loop or targeting (specific weapons add those in M6.2+).
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData data;

        private PlayerAttack _playerAttack;

        public WeaponData Data => data;
        public bool IsValid => data != null;

        /// <summary>
        /// The player GameObject this weapon belongs to (attack source).
        /// Re-resolves lazily on every access so weapons instantiated before
        /// parenting (Awake runs during Instantiate) still resolve the owner
        /// once they are parented under the player.
        /// </summary>
        protected GameObject Owner
        {
            get
            {
                ResolvePlayerAttack();
                return _playerAttack != null ? _playerAttack.gameObject : null;
            }
        }

        private void Awake()
        {
            // The weapon lives under the player (parent hierarchy); resolve once
            // here and re-resolve lazily on attack in case the weapon is
            // instantiated before it is parented (Awake runs during Instantiate).
            ResolvePlayerAttack();
        }

        private void ResolvePlayerAttack()
        {
            if (_playerAttack == null)
            {
                _playerAttack = GetComponentInParent<PlayerAttack>();
            }
        }

        /// <summary>Routes a direct weapon attack through the player attack entry (player base damage).</summary>
        public DamageResult Attack(GameObject target)
        {
            ResolvePlayerAttack();

            if (_playerAttack == null || target == null)
            {
                return new DamageResult(false, 0f, false);
            }

            return _playerAttack.Attack(target);
        }

        /// <summary>Routes a direct weapon attack with an explicit damage value (weapon damage).</summary>
        protected DamageResult Attack(GameObject target, float damage)
        {
            ResolvePlayerAttack();

            if (_playerAttack == null || target == null)
            {
                return new DamageResult(false, 0f, false);
            }

            return _playerAttack.Attack(target, damage);
        }
    }
}
