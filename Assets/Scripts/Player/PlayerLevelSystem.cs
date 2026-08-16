using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Minimal level-up state linkage (M9.1): listens for
    /// <see cref="PlayerLevelUp"/> and — only while the game is Playing — moves
    /// to <see cref="GameState.LevelUp"/> via the GameManager's single state
    /// entry (Playing → LevelUp is a legal transition). Non-Playing states are
    /// never force-entered. It owns NO XP math, no upgrade/UI logic — the
    /// upgrade chooser (M9.2) will handle LevelUp → Playing afterwards.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerLevelSystem : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<PlayerLevelUp>(OnPlayerLevelUp);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerLevelUp>(OnPlayerLevelUp);
        }

        private void OnPlayerLevelUp(PlayerLevelUp e)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentState == GameState.Playing)
            {
                gm.TryChangeState(GameState.LevelUp);
            }
        }
    }
}
