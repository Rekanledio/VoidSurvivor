using UnityEngine;

namespace VoidSurvivor.Shop
{
    /// <summary>The kind of product a shop item represents (M9.4; WeaponUpgrade added M9.5).</summary>
    public enum ShopItemType
    {
        Weapon,
        WeaponUpgrade,
        StatBonus
    }

    /// <summary>
    /// Static shop product configuration (M9.4, extended M9.5): identity + price
    /// + a reference to one of: a weapon prefab (Weapon), a WeaponUpgradeData
    /// (WeaponUpgrade), or an UpgradeData (StatBonus). Assets are never mutated
    /// at runtime — purchase state lives in ShopManager.
    /// Prices are M9.4/M9.5 implementation placeholders, NOT GAME_DESIGN balance.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItemData", menuName = "VoidSurvivor/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        [SerializeField] private ShopItemType itemType;
        [SerializeField] private string displayName = "Shop Item";
        [SerializeField] private int price = 20;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Weapons.WeaponUpgradeData weaponUpgrade;
        [SerializeField] private Player.UpgradeData upgrade;

        public ShopItemType ItemType => itemType;
        public string DisplayName => displayName;
        public int Price => price;
        public GameObject WeaponPrefab => weaponPrefab;
        public Weapons.WeaponUpgradeData WeaponUpgrade => weaponUpgrade;
        public Player.UpgradeData Upgrade => upgrade;
    }
}
