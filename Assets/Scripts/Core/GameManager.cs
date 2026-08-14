using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Minimal project entry singleton holding the centralized GameState.
    /// M1 scope: skeleton only. Event-driven state broadcasting, scene flow
    /// and system wiring are implemented in M2 (Core Framework).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
