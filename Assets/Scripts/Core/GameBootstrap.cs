using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Scene-level bootstrap (M1). Guarantees a GameManager exists before any scene loads.
    ///
    /// Initialization order (M2):
    /// 1. GameBootstrap.EnsureGameManager  (BeforeSceneLoad)
    /// 2. GameManager.Awake                (singleton + DontDestroyOnLoad)
    /// 3. Scene objects' Awake/Start
    /// The EventBus is stateless-initialized (lazy static) and needs no setup.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
                // M11.4: game-flow handler (PlayerDied -> GameOver) lives on the
                // same persistent object as the GameManager.
                go.AddComponent<GameFlow>();
                // M12.1: minimal persistent SFX foundation (one AudioSource).
                go.AddComponent<VoidSurvivor.Audio.AudioManager>();
                // M12.2: SFX library (clip mapping) + event wiring (gameplay + UI).
                go.AddComponent<VoidSurvivor.Audio.SfxLibrary>();
                go.AddComponent<VoidSurvivor.Audio.GameplaySfx>();
            }
        }
    }
}
