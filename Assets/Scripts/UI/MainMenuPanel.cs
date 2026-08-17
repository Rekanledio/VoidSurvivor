using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidSurvivor.Audio;
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
            SfxLibrary.PlayUiClick(); // M12.2: shared UI click
            // M14 P10 fix: start a new run through the unified Core entry
            // (same reset chain as RestartRun), so MainMenu -> Playing never
            // resumes a dead/HP=0 player after GameOver -> MainMenu.
            RunRestarter.StartNewRun();
        }

        private void OnQuitPressed()
        {
            SfxLibrary.PlayUiClick(); // M12.2: shared UI click
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
