using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Runtime weapon (M6.1, upgraded M9.5): holds its <see cref="WeaponData"/>
    /// plus a runtime upgrade layer (level + additive stat bonuses, M9.5) and
    /// routes attacks through the player's <see cref="PlayerAttack"/> — never
    /// straight to a health class. Layering: Weapon → PlayerAttack → CombatSystem
    /// → Enemy. No auto-attack loop or targeting (specific weapons add those).
    ///
    /// M9.5 upgrade layer: WeaponData is NEVER mutated — Effective* accessors
    /// return Data base + runtime bonus (additive, clamped cooldown to a safe
    /// minimum). WeaponLevel starts at 1, bonuses at 0; every instance is fresh
    /// (new shop-purchased weapons default to Level 1 / 0 bonus).
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData data;

        // ---- M9.5 runtime upgrade layer (not serialized; reset per run) ----
        private int _weaponLevel = 1;
        private float _damageBonus;
        private float _attackCooldownBonus;
        private float _rangeBonus;

        private const float MinAttackCooldown = 0.05f; // runtime safety floor (M9.5, not a design value)

        private PlayerAttack _playerAttack;

        public WeaponData Data => data;
        public bool IsValid => data != null;

        /// <summary>
        /// M11.4: weapons only act while the game is Playing. Non-gameplay states
        /// (MainMenu / GameOver / Victory / Paused / LevelUp / Shop) never start
        /// a new attack. In-flight projectiles keep their own pooled lifecycle.
        /// </summary>
        protected static bool GameplayActive =>
            GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;

        // ---- M9.5 upgrade state (read-only for UI / shop) ----
        public int WeaponLevel => _weaponLevel;
        public float DamageBonus => _damageBonus;
        public float AttackCooldownBonus => _attackCooldownBonus;
        public float RangeBonus => _rangeBonus;

        // ---- M9.5 effective stats (base + bonus, add-only semantics) ----
        public float EffectiveDamage => data != null ? data.BaseDamage + _damageBonus : 0f;
        public float EffectiveAttackCooldown => data != null
            ? Mathf.Max(MinAttackCooldown, data.AttackCooldown + _attackCooldownBonus)
            : MinAttackCooldown;
        public float EffectiveRange => data != null ? data.Range + _rangeBonus : 0f;

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

        /// <summary>
        /// Applies a weapon upgrade (M9.5): validates the upgrade, requires its
        /// TargetWeapon to match THIS weapon's Data, then increments the level
        /// and adds the amount to the matching runtime bonus. WeaponData and the
        /// serialized base values are never touched. Returns false (no change)
        /// on any invalid input or a target mismatch.
        /// </summary>
        public bool ApplyWeaponUpgrade(WeaponUpgradeData upgrade)
        {
            if (upgrade == null || data == null) return false;
            if (upgrade.TargetWeapon != data) return false; // only for the matching weapon

            switch (upgrade.StatType)
            {
                case WeaponUpgradeStat.Damage: _damageBonus += upgrade.Amount; break;
                case WeaponUpgradeStat.AttackCooldown: _attackCooldownBonus += upgrade.Amount; break;
                case WeaponUpgradeStat.Range: _rangeBonus += upgrade.Amount; break;
                default: return false;
            }

            _weaponLevel++;
            return true;
        }

        /// <summary>Resets the M9.5 runtime upgrade layer (used at run start; no save).</summary>
        public void ResetWeaponUpgrades()
        {
            _weaponLevel = 1;
            _damageBonus = 0f;
            _attackCooldownBonus = 0f;
            _rangeBonus = 0f;
        }
    }
}
