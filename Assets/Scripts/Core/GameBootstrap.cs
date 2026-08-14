using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Scene-level bootstrap. Guarantees a GameManager exists when a scene
    /// starts directly from the editor. Extended in M2 (Core Framework).
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
            }
        }
    }
}
