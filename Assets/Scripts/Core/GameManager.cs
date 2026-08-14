using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Centralized game state owner. Responsible for:
    /// - holding the current GameState
    /// - validating and applying state transitions (single entry point)
    /// - broadcasting <see cref="GameStateChanged"/> through the EventBus
    /// - its own lifecycle (singleton, DontDestroyOnLoad)
    /// Scene loading and gameplay systems are deliberately NOT part of this class.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        /// <summary>
        /// Legal transitions for the MVP flow.
        /// MainMenu -> Playing -> (Paused/LevelUp/Shop/GameOver/Victory) -> MainMenu.
        /// Extend here when new flows are designed; keep GameManager as the only writer.
        /// </summary>
        private static readonly Dictionary<GameState, GameState[]> AllowedTransitions = new()
        {
            [GameState.MainMenu] = new[] { GameState.Playing },
            [GameState.Playing] = new[] { GameState.Paused, GameState.LevelUp, GameState.Shop, GameState.GameOver, GameState.Victory },
            [GameState.Paused] = new[] { GameState.Playing },
            [GameState.LevelUp] = new[] { GameState.Playing },
            [GameState.Shop] = new[] { GameState.Playing },
            [GameState.GameOver] = new[] { GameState.MainMenu },
            [GameState.Victory] = new[] { GameState.MainMenu },
        };

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

        /// <summary>
        /// The ONLY public entry to change the game state.
        /// Rejects same-state and illegal transitions (logs a warning), then
        /// publishes a <see cref="GameStateChanged"/> event for listeners.
        /// </summary>
        public bool TryChangeState(GameState newState)
        {
            if (newState == CurrentState)
            {
                Debug.LogWarning($"[GameManager] Ignored redundant state change to {newState} (already current).");
                return false;
            }

            if (!IsTransitionAllowed(CurrentState, newState))
            {
                Debug.LogWarning($"[GameManager] Illegal state transition: {CurrentState} -> {newState}.");
                return false;
            }

            GameState from = CurrentState;
            CurrentState = newState;
            EventBus.Publish(new GameStateChanged(from, newState));
            return true;
        }

        private static bool IsTransitionAllowed(GameState from, GameState to)
        {
            if (!AllowedTransitions.TryGetValue(from, out GameState[] targets)) return false;
            foreach (GameState target in targets)
            {
                if (target == to) return true;
            }
            return false;
        }
    }
}
