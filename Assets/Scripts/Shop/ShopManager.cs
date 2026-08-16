using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Enemy;
using VoidSurvivor.Player;
using VoidSurvivor.Weapons;

namespace VoidSurvivor.Shop
{
    /// <summary>
    /// Shop system logic (M9.4): listens for <see cref="WaveCompleted"/> and —
    /// for non-boss waves (index &lt; 10; Wave 10 never publishes WaveCompleted) —
    /// generates 4 products (2 weapons + 2 stat bonuses, unique per shop) and
    /// enters GameState.Shop (legal Playing → Shop transition; GameManager
    /// untouched). WaveManager already pre-started the next wave, which freezes
    /// while not Playing and resumes on Continue. Purchase applies stat bonuses
    /// via UpgradeData → PlayerStats.ApplyUpgrade and weapons via prefab
    /// Instantiate + WeaponManager.Equip into an EMPTY slot (only spend gold
    /// AFTER a successful equip). Refresh re-rolls the products for a flat gold
    /// cost. Product rules and prices are M9.4 implementation rules, not
    /// GAME_DESIGN balance.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopManager : MonoBehaviour
    {
        [SerializeField, Tooltip("All purchasable products (M9.4).")]
        private List<ShopItemData> productPool = new();

        [SerializeField, Tooltip("Refresh cost in gold (M9.4 placeholder).")]
        private int refreshPrice = 20;

        private PlayerProgress _progress;
        private PlayerStats _playerStats;
        private WeaponManager _weaponManager;

        private readonly List<ShopItemData> _products = new();
        private readonly List<bool> _purchased = new();
        private bool _inShop;

        public const int ProductCount = 4;

        public IReadOnlyList<ShopItemData> Products => _products;
        public bool IsInShop => _inShop;
        public int RefreshPrice => refreshPrice;

        private void Awake()
        {
            EventBus.Subscribe<WaveCompleted>(OnWaveCompleted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<WaveCompleted>(OnWaveCompleted);
        }

        private void OnWaveCompleted(WaveCompleted e)
        {
            // Wave 10 (boss) never publishes WaveCompleted, so only W1..W9 arrive.
            if (e.WaveIndex >= WaveManager.TotalWaves) return; // defensive only
            EnterShop();
        }

        private void EnterShop()
        {
            ResolveRefs();
            GenerateProducts();
            if (GameManager.Instance != null &&
                GameManager.Instance.TryChangeState(GameState.Shop))
            {
                _inShop = true;
            }
        }

        private void ResolveRefs()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;
            if (_progress == null) _progress = player.GetComponent<PlayerProgress>();
            if (_playerStats == null) _playerStats = player.GetComponent<PlayerStats>();
            if (_weaponManager == null) _weaponManager = player.GetComponent<WeaponManager>();
        }

        /// <summary>
        /// Generates this shop's products (M9.4 → M9.5 rule): 1 Weapon + 1
        /// WeaponUpgrade + 2 StatBonus, unique within this shop. M9.5
        /// implementation rule (not GAME_DESIGN): WeaponUpgrade products are
        /// drawn ONLY for weapons the player already owns; if no owned weapon
        /// has an upgrade available, that slot falls back to a StatBonus. No
        /// weights/rarity; different shops may repeat products.
        /// </summary>
        public void GenerateProducts()
        {
            ResolveRefs();
            _products.Clear();
            _purchased.Clear();

            var weapons = new List<ShopItemData>();
            var upgrades = new List<ShopItemData>();
            var stats = new List<ShopItemData>();
            if (productPool != null)
            {
                foreach (var item in productPool)
                {
                    if (item == null) continue;
                    if (item.ItemType == ShopItemType.Weapon)
                    {
                        weapons.Add(item);
                    }
                    else if (item.ItemType == ShopItemType.WeaponUpgrade)
                    {
                        // Only include upgrades whose target weapon is owned.
                        if (item.WeaponUpgrade != null && IsWeaponEquipped(item.WeaponUpgrade.TargetWeapon))
                        {
                            upgrades.Add(item);
                        }
                    }
                    else
                    {
                        stats.Add(item);
                    }
                }
            }

            DrawUnique(weapons, 1);
            int upgradesDrawn = DrawUnique(upgrades, 1);
            DrawUnique(stats, 2 + (1 - upgradesDrawn)); // fallback: no upgrade → extra stat

            for (int i = _products.Count; i < ProductCount; i++) _products.Add(null); // pad
            _purchased.Clear();
            for (int i = 0; i < _products.Count; i++) _purchased.Add(false);

            PublishProducts();
        }

        /// <summary>True when a weapon whose Data matches target is equipped.</summary>
        private bool IsWeaponEquipped(WeaponData target)
        {
            if (_weaponManager == null || target == null) return false;
            for (int i = 0; i < _weaponManager.SlotCount; i++)
            {
                var equipped = _weaponManager.GetWeapon(i);
                if (equipped != null && equipped.Data == target) return true;
            }
            return false;
        }

        /// <summary>
        /// Current upgrade level of the equipped weapon matching target (M9.5;
        /// UI display only). Returns 1 when not found / not equipped.
        /// </summary>
        public int LevelOfEquipped(WeaponData target)
        {
            if (_weaponManager == null || target == null) return 1;
            for (int i = 0; i < _weaponManager.SlotCount; i++)
            {
                var w = _weaponManager.GetWeapon(i);
                if (w != null && w.Data == target) return w.WeaponLevel;
            }
            return 1;
        }

        private int DrawUnique(List<ShopItemData> pool, int count)
        {
            int drawn = 0;
            while (drawn < count && pool.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                _products.Add(pool[idx]);
                pool.RemoveAt(idx);
                drawn++;
            }
            return drawn;
        }

        public bool IsPurchased(int index)
        {
            return index >= 0 && index < _purchased.Count && _purchased[index];
        }

        /// <summary>Test hook: forces the next product list (ignores the pool).</summary>
        public void SetForcedProducts(params ShopItemData[] forced)
        {
            _products.Clear();
            _purchased.Clear();
            if (forced != null) _products.AddRange(forced);
            while (_products.Count < ProductCount) _products.Add(null);
            for (int i = 0; i < _products.Count; i++) _purchased.Add(false);
            PublishProducts();
        }

        /// <summary>
        /// Buys Products[index]. Success path: validate → (weapon: equip into an
        /// empty slot AFTER spawn) → spend gold LAST → mark purchased → publish.
        /// Any failure leaves gold and state untouched.
        /// </summary>
        public bool Purchase(int index)
        {
            if (!_inShop) return false;
            if (index < 0 || index >= _products.Count) return false;
            if (_purchased.Count <= index || _purchased[index]) return false; // already bought

            var item = _products[index];
            if (item == null) return false;
            if (_progress == null || _playerStats == null) return false;

            if (item.ItemType == ShopItemType.Weapon)
            {
                if (!TryPurchaseWeapon(item)) return false;
            }
            else if (item.ItemType == ShopItemType.WeaponUpgrade)
            {
                if (!TryPurchaseWeaponUpgrade(item)) return false;
            }
            else
            {
                if (item.Upgrade == null) return false;
                if (!_progress.TrySpendGold(item.Price)) return false;
                _playerStats.ApplyUpgrade(item.Upgrade);
            }

            _purchased[index] = true;
            PublishProducts();
            return true;
        }

        /// <summary>
        /// Buys a weapon-upgrade product (M9.5): requires the target weapon to be
        /// EQUIPPED (found via Data reference match, not name). Order: gold check
        /// → apply upgrade → spend gold. Gold is spent ONLY after the upgrade was
        /// successfully applied; any failure (no target weapon / not enough gold /
        /// apply failed) leaves gold and level untouched.
        /// </summary>
        private bool TryPurchaseWeaponUpgrade(ShopItemData item)
        {
            if (item.WeaponUpgrade == null) return false;
            if (_weaponManager == null || _progress == null) return false;

            var target = item.WeaponUpgrade.TargetWeapon;
            if (target == null) return false;

            // Find the equipped weapon with matching Data.
            WeaponController equipped = null;
            for (int i = 0; i < _weaponManager.SlotCount; i++)
            {
                var w = _weaponManager.GetWeapon(i);
                if (w != null && w.Data == target)
                {
                    equipped = w;
                    break;
                }
            }
            if (equipped == null) return false; // target weapon not owned → fail, no gold

            // Gold check (spend happens after the upgrade is applied).
            if (_progress.CurrentGold < item.Price) return false;

            // Apply upgrade; only on success spend the gold.
            if (!equipped.ApplyWeaponUpgrade(item.WeaponUpgrade)) return false;
            if (!_progress.TrySpendGold(item.Price))
            {
                // Unreachable in practice (gold was checked above); keep the
                // spent-nothing invariant by leaving the level applied — the
                // check prevents entering this branch.
                return false;
            }

            return true;
        }

        private bool TryPurchaseWeapon(ShopItemData item)
        {
            if (item.WeaponPrefab == null) return false;
            if (_weaponManager == null) return false;

            // Find an empty slot first (no auto-replacement).
            int emptySlot = -1;
            for (int i = 0; i < _weaponManager.SlotCount; i++)
            {
                var slot = _weaponManager.GetSlot(i);
                if (slot != null && slot.IsEmpty)
                {
                    emptySlot = i;
                    break;
                }
            }
            if (emptySlot < 0) return false; // no empty slot → fail (no gold spent)

            // Refuse if the same weapon is already equipped (Data compare).
            var newController = item.WeaponPrefab.GetComponent<WeaponController>();
            if (newController == null || newController.Data == null) return false;
            for (int i = 0; i < _weaponManager.SlotCount; i++)
            {
                var equipped = _weaponManager.GetWeapon(i);
                if (equipped != null && equipped.Data == newController.Data) return false; // already owned
            }

            // Only proceed when gold is sufficient.
            if (!_progress.TrySpendGold(item.Price)) return false;

            // Instantiate + parent to the player's WeaponManager (fix: without
            // SetParent the weapon lands at Scene Root and WeaponController.Owner
            // (GetComponentInParent<PlayerAttack>) resolves null → attacks deal 0
            // damage). Then equip; on equip failure destroy and refund the
            // pre-spent gold.
            var instance = Instantiate(item.WeaponPrefab);
            var weapon = instance != null ? instance.GetComponent<WeaponController>() : null;
            if (weapon == null)
            {
                if (instance != null) Destroy(instance);
                // Gold was NOT spent (spend happens after equip? no — we spent before). Refund.
                _progress.AddGold(item.Price);
                return false;
            }

            instance.transform.SetParent(_weaponManager.transform, false);

            if (!_weaponManager.Equip(emptySlot, weapon))
            {
                Destroy(instance);
                _progress.AddGold(item.Price); // refund the pre-spent gold
                return false;
            }

            return true;
        }

        /// <summary>
        /// Re-rolls the 4 products for a flat gold cost (M9.4 placeholder:
        /// refreshPrice, unlimited uses). Resets this shop's purchase state.
        /// </summary>
        public bool Refresh()
        {
            if (!_inShop) return false;
            if (_progress == null) return false;
            if (!_progress.TrySpendGold(refreshPrice)) return false;

            GenerateProducts(); // also clears purchase state + publishes
            return true;
        }

        /// <summary>Leaves the shop; WaveManager resumes the pre-started next wave.</summary>
        public void Continue()
        {
            if (!_inShop) return;
            _inShop = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TryChangeState(GameState.Playing);
            }
        }

        private void PublishProducts()
        {
            EventBus.Publish(new ShopProductsGenerated(
                _products.Count > 0 ? _products[0] : null,
                _products.Count > 1 ? _products[1] : null,
                _products.Count > 2 ? _products[2] : null,
                _products.Count > 3 ? _products[3] : null));
        }
    }
}
