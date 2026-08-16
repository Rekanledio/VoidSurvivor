using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Shop UI (M9.4, same pattern as LevelUpPanel): component on the ACTIVE
    /// Canvas; the visible ShopPanel is an initially-inactive child. Shows the
    /// panel from GameStateChanged (Shop → visible, other states → hidden),
    /// refreshes the 4 product buttons + gold text from
    /// <see cref="ShopProductsGenerated"/>, and forwards clicks to ShopManager
    /// (Purchase / Refresh / Continue). Owns no gold/stats/weapons writes.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private Shop.ShopManager shopManager;
        [SerializeField] private PlayerProgress progress;
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button[] productButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] productLabels = new TextMeshProUGUI[4];
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            if (shopManager == null) shopManager = FindFirstObjectByType<Shop.ShopManager>();
            if (progress == null) progress = FindFirstObjectByType<PlayerProgress>();
            if (panel == null) panel = gameObject;

            for (int i = 0; i < productButtons.Length; i++)
            {
                int index = i;
                if (productButtons[i] != null)
                {
                    productButtons[i].onClick.AddListener(() =>
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
            for (int i = 0; i < productButtons.Length; i++)
            {
                if (productLabels[i] == null) continue;
                if (i < products.Length && products[i] != null)
                {
                    productLabels[i].text = BuildLabel(products[i]);
                    if (productButtons[i] != null)
                    {
                        bool bought = shopManager != null && shopManager.IsPurchased(i);
                        productButtons[i].interactable = !bought;
                    }
                }
                else
                {
                    productLabels[i].text = "—";
                    if (productButtons[i] != null) productButtons[i].interactable = false;
                }
            }
        }

        private void RefreshGoldText()
        {
            if (goldText != null)
            {
                goldText.text = progress != null ? $"Gold: {progress.CurrentGold}" : "Gold: —";
            }
        }

        private static string BuildLabel(Shop.ShopItemData item)
        {
            if (item.ItemType == Shop.ShopItemType.Weapon)
            {
                return $"{item.DisplayName}\n{item.ItemType}\n{item.Price} Gold";
            }

            string bonus = item.Upgrade != null ? $" {item.Upgrade.DisplayName}" : "";
            return $"{item.DisplayName}{bonus}\n{item.ItemType}\n{item.Price} Gold";
        }
    }
}
