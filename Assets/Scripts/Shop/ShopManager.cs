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
        /// Draws 2 weapons + 2 stat bonuses, unique within this shop.
        /// M9.4 implementation rule (not GAME_DESIGN): fixed 2/2 mix, no
        /// weights/rarity; different shops may repeat products.
        /// </summary>
        public void GenerateProducts()
        {
            _products.Clear();
            _purchased.Clear();

            var weapons = new List<ShopItemData>();
            var stats = new List<ShopItemData>();
            if (productPool != null)
            {
                foreach (var item in productPool)
                {
                    if (item == null) continue;
                    if (item.ItemType == ShopItemType.Weapon) weapons.Add(item);
                    else stats.Add(item);
                }
            }

            DrawUnique(weapons, 2);
            DrawUnique(stats, 2);

            for (int i = _products.Count; i < ProductCount; i++) _products.Add(null); // pad
            _purchased.Clear();
            for (int i = 0; i < _products.Count; i++) _purchased.Add(false);

            PublishProducts();
        }

        private void DrawUnique(List<ShopItemData> pool, int count)
        {
            int drawn = 0;
            while (drawn < count && pool.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                _products.Add(pool[idx]);
                pool.RemoveAt(idx);
                drawn++;
            }
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
