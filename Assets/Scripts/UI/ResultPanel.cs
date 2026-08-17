using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Core;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Result screen (M11.3): one instance per end state — GameOverPanel
    /// (showInState = GameOver) and VictoryPanel (showInState = Victory).
    /// Visibility follows <see cref="GameStateChanged"/> exactly
    /// (panel active only when the current state == showInState). Restart
    /// delegates to <see cref="RunRestarter.RestartRun"/>; Main Menu goes
    /// through the single GameManager state entry. No reset logic lives here.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private GameState showInState;
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (panel == null) panel = gameObject;

            if (restartButton != null)
                restartButton.onClick.AddListener(() => RunRestarter.RestartRun());
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuPressed);

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);

            bool startVisible = GameManager.Instance != null
                && GameManager.Instance.CurrentState == showInState;
            if (panel != null) panel.SetActive(startVisible);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (panel == null) return;
            panel.SetActive(e.To == showInState);
        }

        private void OnMainMenuPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TryChangeState(GameState.MainMenu);
        }
    }
}
