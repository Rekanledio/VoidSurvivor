using UnityEngine;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// The 10 player stats that an upgrade may modify, matching GAME_DESIGN
    /// section 6 (order preserved). See PlayerStats for the runtime bonus layer.
    /// </summary>
    public enum UpgradeStat
    {
        MaxHP,
        HPRegen,
        MoveSpeed,
        Damage,
        AttackSpeed,
        CritChance,
        CritDamage,
        Range,
        PickupRange,
        Armor
    }

    /// <summary>
    /// Static upgrade configuration (M9.2): identity + which stat to boost and
    /// by how much (additive bonus). Assets are never mutated at runtime —
    /// PlayerStats.ApplyUpgrade reads them only. No rarity/weight/tier/price.
    /// Amount values in the assets are M9.2 IMPLEMENTATION placeholders, not
    /// GAME_DESIGN balance values.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "VoidSurvivor/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [SerializeField] private string upgradeId = "upgrade";
        [SerializeField] private string displayName = "Upgrade";
        [SerializeField] private UpgradeStat statType;
        [SerializeField] private float amount = 1f;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public UpgradeStat StatType => statType;
        public float Amount => amount;
    }
}
