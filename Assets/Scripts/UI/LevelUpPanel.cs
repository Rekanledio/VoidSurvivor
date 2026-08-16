using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Level-up chooser UI (M9.3): shows/hides the panel from
    /// <see cref="GameStateChanged"/> (LevelUp → visible, Playing/GameOver/
    /// Victory/MainMenu → hidden), refreshes its 3 buttons from
    /// <see cref="UpgradeOptionsGenerated"/> (NEVER reads Options directly on
    /// state change — generation happens after the state event), and forwards
    /// button clicks to <see cref="UpgradeManager.Select"/>. Owns no stats,
    /// no game-state writes.
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] buttons = new Button[3];
        [SerializeField] private TextMeshProUGUI[] labels = new TextMeshProUGUI[3];

        private void Awake()
        {
            if (upgradeManager == null)
            {
                upgradeManager = GetComponentInParent<UpgradeManager>();
                if (upgradeManager == null) upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }
            if (panel == null) panel = gameObject;

            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                if (buttons[i] != null)
                {
                    buttons[i].onClick.AddListener(() =>
                    {
                        if (upgradeManager != null) upgradeManager.Select(index);
                    });
                }
            }

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Subscribe<UpgradeOptionsGenerated>(OnOptionsGenerated);

            if (panel != null) panel.SetActive(false); // start hidden
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Unsubscribe<UpgradeOptionsGenerated>(OnOptionsGenerated);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (panel == null) return;

            switch (e.To)
            {
                case GameState.LevelUp:
                    panel.SetActive(true);
                    break;
                case GameState.Playing:
                case GameState.GameOver:
                case GameState.Victory:
                case GameState.MainMenu:
                    panel.SetActive(false);
                    break;
                // Paused / Shop: keep current state (do not touch).
            }
        }

        private void OnOptionsGenerated(UpgradeOptionsGenerated e)
        {
            var options = new[] { e.Option0, e.Option1, e.Option2 };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (labels[i] == null) continue;
                if (i < options.Length && options[i] != null)
                {
                    labels[i].text = BuildLabel(options[i]);
                    if (buttons[i] != null) buttons[i].interactable = true;
                }
                else
                {
                    labels[i].text = "—";
                    if (buttons[i] != null) buttons[i].interactable = false;
                }
            }
        }

        private static string BuildLabel(UpgradeData upgrade)
        {
            // DisplayName / StatType / +Amount — no description field yet.
            return $"{upgrade.DisplayName}\n{upgrade.StatType}\n+{FormatAmount(upgrade.Amount)}";
        }

        private static string FormatAmount(float amount)
        {
            // Integers show as integers; floats keep their raw value (no %).
            return Mathf.Approximately(amount, Mathf.Round(amount))
                ? ((int)Mathf.Round(amount)).ToString()
                : amount.ToString("0.##");
        }
    }
}
