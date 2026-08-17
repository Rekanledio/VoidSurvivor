using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Core;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Main Menu UI (M11.1): shows/hides the panel from
    /// <see cref="GameStateChanged"/> (MainMenu → visible, every other state →
    /// hidden), wires Play (MainMenu → Playing via GameManager) and Quit
    /// (standalone application exit). Owns no game-state writes beyond the
    /// Play transition; reuses the single Canvas / EventSystem /
    /// InputSystemUIInputModule already in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (panel == null) panel = gameObject;

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayPressed);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitPressed);

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);

            // Initial visibility follows the current state (the game starts in
            // MainMenu, so the panel is visible on boot).
            if (GameManager.Instance != null)
                panel.SetActive(GameManager.Instance.CurrentState == GameState.MainMenu);
            else
                panel.SetActive(true);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (panel == null) return;
            panel.SetActive(e.To == GameState.MainMenu);
        }

        private void OnPlayPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TryChangeState(GameState.Playing);
        }

        private void OnQuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
