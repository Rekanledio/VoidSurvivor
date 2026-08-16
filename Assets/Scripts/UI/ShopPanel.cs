using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Core;
using VoidSurvivor.Player;
using VoidSurvivor.Shop;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Shop UI (M9.4, polished M9.4): component on the ACTIVE Canvas; the
    /// visible ShopPanel is an initially-inactive child. GameStateChanged shows/
    /// hides the panel (Shop → visible, other states → hidden); ShopProductsGenerated
    /// drives 4 product cards (Name / Type / Price / Buy button) + gold text.
    /// All player-facing text is Chinese (display mapping only — ShopItemData/
    /// UpgradeData fields are untouched). Owns no gold/stats/weapons writes.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private PlayerProgress progress;
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[4];
        [SerializeField] private TextMeshProUGUI[] typeTexts = new TextMeshProUGUI[4];
        [SerializeField] private TextMeshProUGUI[] priceTexts = new TextMeshProUGUI[4];
        [SerializeField] private Button[] buyButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] buyLabels = new TextMeshProUGUI[4];
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
            if (progress == null) progress = FindFirstObjectByType<PlayerProgress>();
            if (panel == null) panel = gameObject;

            for (int i = 0; i < buyButtons.Length; i++)
            {
                int index = i;
                if (buyButtons[i] != null)
                {
                    buyButtons[i].onClick.AddListener(() =>
                    {
                        if (shopManager != null) shopManager.Purchase(index);
                    });
                }
            }
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(() =>
                {
                    if (shopManager != null) shopManager.Refresh();
                });
            }
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() =>
                {
                    if (shopManager != null) shopManager.Continue();
                });
            }

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Subscribe<ShopProductsGenerated>(OnProductsGenerated);

            if (panel != null) panel.SetActive(false); // start hidden
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Unsubscribe<ShopProductsGenerated>(OnProductsGenerated);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (panel == null) return;

            switch (e.To)
            {
                case GameState.Shop:
                    panel.SetActive(true);
                    break;
                case GameState.Playing:
                case GameState.GameOver:
                case GameState.Victory:
                case GameState.MainMenu:
                    panel.SetActive(false);
                    break;
                // Paused / LevelUp: keep current state.
            }
        }

        private void OnProductsGenerated(ShopProductsGenerated e)
        {
            RefreshGoldText();
            var products = new[] { e.Product0, e.Product1, e.Product2, e.Product3 };
            for (int i = 0; i < buyButtons.Length; i++)
            {
                bool valid = i < products.Length && products[i] != null;
                if (nameTexts[i] != null) nameTexts[i].text = valid ? ProductName(products[i]) : "—";
                if (typeTexts[i] != null) typeTexts[i].text = valid ? ProductTypeName(products[i]) : "";
                if (priceTexts[i] != null) priceTexts[i].text = valid ? $"价格：{products[i].Price} 金币" : "";

                if (buyButtons[i] != null)
                {
                    bool bought = shopManager != null && shopManager.IsPurchased(i);
                    buyButtons[i].interactable = valid && !bought;
                    if (buyLabels[i] != null) buyLabels[i].text = valid ? (bought ? "已购买" : "购买") : "—";
                }
            }
        }

        private void RefreshGoldText()
        {
            if (goldText != null)
            {
                goldText.text = progress != null ? $"金币：{progress.CurrentGold}" : "金币：—";
            }
        }

        // ---- Player-facing display mapping (中文显示映射；不改数据结构) ----

        private static readonly Dictionary<string, string> WeaponNames = new()
        {
            ["PulseGun"] = "脉冲枪",
            ["ScatterBlaster"] = "散射爆能枪",
            ["Boomerang"] = "回旋镖",
            ["ArcBlade"] = "弧刃",
        };

        /// <summary>Maps a WeaponData asset name (e.g. "PulseGunData") to Chinese (UI display only).</summary>
        private static readonly Dictionary<string, string> WeaponDataNames = new()
        {
            ["PulseGunData"] = "脉冲枪",
            ["ScatterBlasterData"] = "散射爆能枪",
            ["BoomerangData"] = "回旋镖",
            ["ArcBladeData"] = "弧刃",
        };

        private static string ProductName(ShopItemData item)
        {
            if (item.ItemType == ShopItemType.Weapon)
            {
                string key = item.DisplayName;
                if (WeaponNames.TryGetValue(key, out string zh)) return zh;
                if (item.WeaponPrefab != null && WeaponNames.TryGetValue(item.WeaponPrefab.name, out zh)) return zh;
                return key;
            }

            if (item.ItemType == ShopItemType.WeaponUpgrade)
            {
                return WeaponUpgradeName(item);
            }

            // Stat bonus: "+属性名 数值" using the mapped stat name.
            if (item.Upgrade != null)
            {
                return $"+{StatName(item.Upgrade.StatType)} {FormatAmount(item.Upgrade.Amount)}";
            }
            return item.DisplayName;
        }

        /// <summary>
        /// Multi-line label for a WeaponUpgrade product (M9.5):
        /// weapon name / 升级：stat / 等级：Lv.X → Lv.X+1 (X from the equipped weapon).
        /// </summary>
        private static string WeaponUpgradeName(ShopItemData item)
        {
            var upgrade = item.WeaponUpgrade;
            if (upgrade == null || upgrade.TargetWeapon == null) return item.DisplayName;

            string weaponZh = upgrade.TargetWeapon.name;
            if (WeaponDataNames.TryGetValue(upgrade.TargetWeapon.name, out string zn)) weaponZh = zn;
            else if (WeaponNames.TryGetValue(upgrade.TargetWeapon.name, out zn)) weaponZh = zn;
            string statZh = WeaponUpgradeStatName(upgrade.StatType);
            int level = FindFirstObjectByType<ShopManager>() is { } sm
                ? sm.LevelOfEquipped(upgrade.TargetWeapon)
                : 1;
            return $"{weaponZh}\n升级：{statZh}\n等级：Lv.{level} → Lv.{level + 1}";
        }

        private static string ProductTypeName(ShopItemData item)
        {
            return item.ItemType switch
            {
                ShopItemType.Weapon => "武器",
                ShopItemType.WeaponUpgrade => "武器升级",
                _ => "属性",
            };
        }

        private static string WeaponUpgradeStatName(Weapons.WeaponUpgradeStat stat)
        {
            return stat switch
            {
                Weapons.WeaponUpgradeStat.Damage => "伤害",
                Weapons.WeaponUpgradeStat.AttackCooldown => "攻击速度",
                Weapons.WeaponUpgradeStat.Range => "攻击范围",
                _ => stat.ToString(),
            };
        }

        public static string StatName(UpgradeStat stat)
        {
            return stat switch
            {
                UpgradeStat.MaxHP => "最大生命值",
                UpgradeStat.HPRegen => "生命回复",
                UpgradeStat.MoveSpeed => "移动速度",
                UpgradeStat.Damage => "伤害",
                UpgradeStat.AttackSpeed => "攻击速度",
                UpgradeStat.CritChance => "暴击率",
                UpgradeStat.CritDamage => "暴击伤害",
                UpgradeStat.Range => "攻击范围",
                UpgradeStat.PickupRange => "拾取范围",
                UpgradeStat.Armor => "护甲",
                _ => stat.ToString(),
            };
        }

        public static string FormatAmount(float amount)
        {
            return Mathf.Approximately(amount, Mathf.Round(amount))
                ? ((int)Mathf.Round(amount)).ToString()
                : amount.ToString("0.##");
        }
    }
}
