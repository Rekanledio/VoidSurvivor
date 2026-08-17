using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Minimal game-flow handler (M11.4): bridges the PlayerDied event to the
    /// GameOver state through the single GameManager state entry.
    /// Owns no UI, no state machine, no gameplay logic. Guards:
    /// - only transitions while Playing (PlayerDied elsewhere — e.g. a stale
    ///   event in MainMenu/Victory — never forces GameOver);
    /// - PlayerHealth publishes PlayerDied exactly once per death, and
    ///   GameManager rejects same-state changes, so GameOver cannot re-fire.
    /// Attached by GameBootstrap on the GameManager GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameFlow : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<PlayerDied>(OnPlayerDied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDied>(OnPlayerDied);
        }

        private void OnPlayerDied(PlayerDied e)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentState == GameState.Playing)
                gm.TryChangeState(GameState.GameOver);
        }
    }
}
