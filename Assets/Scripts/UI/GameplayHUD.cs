using TMPro;
using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Enemy;
using VoidSurvivor.Player;

namespace VoidSurvivor.UI
{
    /// <summary>
    /// Gameplay HUD (M11.2): a minimal always-during-battle display for
    /// HP / XP / Level / Gold / Wave. Visibility is event-driven
    /// (<see cref="GameStateChanged"/>: shown during Playing/LevelUp/Shop/Paused,
    /// hidden during GameOver/Victory/MainMenu). Values are READ-ONLY — the HUD
    /// refreshes cached references each frame and never writes gameplay state
    /// (no AddXP/AddGold/TakeDamage/StartWave/TryChangeState). No second
    /// EventBus / Canvas / EventSystem; references are resolved once.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;

        private PlayerHealth _health;
        private PlayerProgress _progress;
        private WaveManager _wave;
        private bool _visible;

        private void Awake()
        {
            if (panel == null) panel = gameObject;

            _health = FindFirstObjectByType<PlayerHealth>();
            _progress = FindFirstObjectByType<PlayerProgress>();
            _wave = FindFirstObjectByType<WaveManager>();

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);

            bool startVisible = GameManager.Instance != null
                && IsHudState(GameManager.Instance.CurrentState);
            _visible = startVisible;
            if (panel != null) panel.SetActive(startVisible);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private static bool IsHudState(GameState s)
        {
            switch (s)
            {
                case GameState.Playing:
                case GameState.LevelUp:
                case GameState.Shop:
                case GameState.Paused:
                    return true;
                default:
                    return false;
            }
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (panel == null) return;
            _visible = IsHudState(e.To);
            panel.SetActive(_visible);
        }

        private void Update()
        {
            if (!_visible || panel == null || !panel.activeSelf) return;

            if (hpText != null && _health != null)
                hpText.text = $"HP {Mathf.CeilToInt(_health.CurrentHP)} / {Mathf.CeilToInt(_health.MaxHP)}";

            if (xpText != null && _progress != null)
                xpText.text = $"XP {_progress.CurrentXP} / {_progress.XPToNextLevel}";

            if (levelText != null && _progress != null)
                levelText.text = $"Lv. {_progress.Level}";

            if (goldText != null && _progress != null)
                goldText.text = $"Gold {_progress.CurrentGold}";

            if (waveText != null && _wave != null)
                waveText.text = $"Wave {_wave.CurrentWave}";
        }
    }
}
