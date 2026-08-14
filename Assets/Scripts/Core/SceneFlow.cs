using UnityEngine;
using UnityEngine.SceneManagement;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Scene identifiers for the planned flow: MainMenu -> Gameplay -> Result.
    /// The actual scene assets are created by their owning milestones (M3/M12),
    /// not by M2. Keep these constants as the single source of scene names.
    /// </summary>
    public static class SceneIds
    {
        public const string MainMenu = "SC_MainMenu";
        public const string Gameplay = "SC_Gameplay";
        public const string Result = "SC_Result";
    }

    /// <summary>
    /// Minimal scene-flow API. Encapsulates SceneManager so callers never touch
    /// UnityEngine.SceneManagement directly. Does NOT change GameState — state
    /// transitions belong to GameManager; scene loads are initiated by UI/flow owners.
    /// </summary>
    public static class SceneFlow
    {
        public static string CurrentSceneName => SceneManager.GetActiveScene().name;

        /// <summary>
        /// Loads the named scene. Skips the call (with a warning) when the scene
        /// is already active, to prevent accidental reloads.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneFlow] LoadScene called with an empty scene name.");
                return;
            }

            if (sceneName == CurrentSceneName)
            {
                Debug.LogWarning($"[SceneFlow] Scene '{sceneName}' is already active; skipping reload.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
