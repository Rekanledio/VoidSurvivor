using UnityEngine;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Base player stats as defined in GAME_DESIGN.md (section 6).
    /// M3 scope: plain base values + read accessors only.
    ///
    /// M9.2: a runtime bonus layer was added for upgrades. The serialized base
    /// fields are NEVER modified — each accessor returns base + runtime bonus.
    /// <see cref="ApplyUpgrade"/> adds an <see cref="UpgradeData"/> amount to the
    /// matching bonus; <see cref="ResetForRun"/> zeroes every bonus (no save).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats (GAME_DESIGN.md)")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float hpRegen = 0f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float critChance = 0.05f;
        [SerializeField] private float critDamage = 1.5f;
        [SerializeField] private float range = 1f;
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private float armor = 0f;

        // Runtime upgrade bonuses (M9.2) — not serialized, reset per run.
        private float _maxHPBonus;
        private float _hpRegenBonus;
        private float _moveSpeedBonus;
        private float _damageBonus;
        private float _attackSpeedBonus;
        private float _critChanceBonus;
        private float _critDamageBonus;
        private float _rangeBonus;
        private float _pickupRangeBonus;
        private float _armorBonus;

        public float MaxHP => maxHP + _maxHPBonus;
        public float HPRegen => hpRegen + _hpRegenBonus;
        public float MoveSpeed => moveSpeed + _moveSpeedBonus;
        public float Damage => damage + _damageBonus;
        public float AttackSpeed => attackSpeed + _attackSpeedBonus;
        public float CritChance => critChance + _critChanceBonus;
        public float CritDamage => critDamage + _critDamageBonus;
        public float Range => range + _rangeBonus;
        public float PickupRange => pickupRange + _pickupRangeBonus;
        public float Armor => armor + _armorBonus;

        /// <summary>
        /// Applies an upgrade's additive amount to the matching runtime bonus.
        /// The ScriptableObject and the serialized base fields are never touched.
        /// </summary>
        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            switch (upgrade.StatType)
            {
                case UpgradeStat.MaxHP: _maxHPBonus += upgrade.Amount; break;
                case UpgradeStat.HPRegen: _hpRegenBonus += upgrade.Amount; break;
                case UpgradeStat.MoveSpeed: _moveSpeedBonus += upgrade.Amount; break;
                case UpgradeStat.Damage: _damageBonus += upgrade.Amount; break;
                case UpgradeStat.AttackSpeed: _attackSpeedBonus += upgrade.Amount; break;
                case UpgradeStat.CritChance: _critChanceBonus += upgrade.Amount; break;
                case UpgradeStat.CritDamage: _critDamageBonus += upgrade.Amount; break;
                case UpgradeStat.Range: _rangeBonus += upgrade.Amount; break;
                case UpgradeStat.PickupRange: _pickupRangeBonus += upgrade.Amount; break;
                case UpgradeStat.Armor: _armorBonus += upgrade.Amount; break;
            }
        }

        /// <summary>Clears every runtime upgrade bonus (used at run start; no save).</summary>
        public void ResetForRun()
        {
            _maxHPBonus = 0f;
            _hpRegenBonus = 0f;
            _moveSpeedBonus = 0f;
            _damageBonus = 0f;
            _attackSpeedBonus = 0f;
            _critChanceBonus = 0f;
            _critDamageBonus = 0f;
            _rangeBonus = 0f;
            _pickupRangeBonus = 0f;
            _armorBonus = 0f;
        }
    }
}
