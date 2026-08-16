using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// The weapon stat an upgrade modifies (M9.5). First-version scope: the
    /// three common stats every weapon reads live (Damage / AttackCooldown /
    /// Range). Weapon-specific stats (ProjectileCount, SpreadAngle, Boomerang
    /// flight params) are intentionally NOT in the first version — they are
    /// documented for a later extension.
    /// </summary>
    public enum WeaponUpgradeStat
    {
        Damage,
        AttackCooldown,
        Range
    }

    /// <summary>
    /// Static weapon-upgrade configuration (M9.5): identity + which weapon it
    /// targets + which stat to boost and by how much (additive bonus). Assets
    /// are never mutated at runtime — WeaponController.ApplyWeaponUpgrade reads
    /// them only and applies the amount to its runtime bonus. Price lives in
    /// ShopItemData (NOT here). Amount values in the assets are M9.5
    /// IMPLEMENTATION placeholders (Damage +1, AttackCooldown -0.05, Range +0.5),
    /// not GAME_DESIGN balance.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponUpgradeData", menuName = "VoidSurvivor/Weapon Upgrade Data")]
    public class WeaponUpgradeData : ScriptableObject
    {
        [SerializeField] private string upgradeId = "weapon_upgrade";
        [SerializeField] private string displayName = "Weapon Upgrade";
        [SerializeField] private WeaponData targetWeapon;
        [SerializeField] private WeaponUpgradeStat statType;
        [SerializeField] private float amount = 0f;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public WeaponData TargetWeapon => targetWeapon;
        public WeaponUpgradeStat StatType => statType;
        public float Amount => amount;
    }
}
