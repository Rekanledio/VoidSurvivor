using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Enemy;
using VoidSurvivor.Player;

namespace VoidSurvivor.Save
{
    /// <summary>
    /// M13.3 — Best Run Record persistence.
    ///
    /// Records the historical best run (bestWave / bestLevel / bestGold) exactly
    /// once per run, at the moment the run ends:
    ///   GameStateChanged with To == GameOver or To == Victory.
    ///
    /// At that instant the final values are still readable (PlayerProgress and
    /// WaveManager are only reset by RunRestarter on the NEXT run's Playing),
    /// the transition is unique per run (TryChangeState guards), and neither
    /// Restart nor Main Menu re-fires it — so no duplicate saves.
    ///
    /// Persistence goes through SaveManager (read-modify-write on the existing
    /// SaveData), so masterVolume/sfxVolume (M13.2) are never overwritten and
    /// the five fields stay intact. No gameplay state is modified; no UI.
    /// </summary>
    [DisallowMultipleComponent]
    public class BestRunRecorder : MonoBehaviour
    {
        /// <summary>True after a run's first terminal state; reset when Playing starts.</summary>
        private bool _recordedThisRun;

        private void Awake()
        {
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            // A fresh run begins when Playing is entered (same trigger the
            // WaveManager uses to start wave 1).
            if (e.To == GameState.Playing)
            {
                _recordedThisRun = false;
                return;
            }

            // Only terminal states record the best run.
            if (e.To != GameState.GameOver && e.To != GameState.Victory)
                return;

            // Guard: record at most once per run even if an unexpected
            // duplicate terminal event arrives.
            if (_recordedThisRun) return;
            _recordedThisRun = true;

            // Final values are still intact at this point (no ResetForRun yet).
            int currentWave = 0;
            int currentLevel = 0;
            int currentGold = 0;

            var wave = FindFirstObjectByType<WaveManager>();
            if (wave != null) currentWave = wave.CurrentWave;

            var progress = FindFirstObjectByType<PlayerProgress>();
            if (progress != null)
            {
                currentLevel = progress.Level;
                currentGold = progress.CurrentGold;
            }

            if (SaveManager.Instance == null) return;

            // Read-modify-write: preserve masterVolume/sfxVolume (M13.2).
            var data = SaveManager.Instance.Load();
            bool newBest = false;

            if (currentWave > data.bestWave) { data.bestWave = currentWave; newBest = true; }
            if (currentLevel > data.bestLevel) { data.bestLevel = currentLevel; newBest = true; }
            if (currentGold > data.bestGold) { data.bestGold = currentGold; newBest = true; }

            // Only write when something actually improved.
            if (newBest)
                SaveManager.Instance.Save(data);
        }
    }
}
