namespace VoidSurvivor.Core
{
    /// <summary>
    /// Centralized game states, aligned with ARCHITECTURE.md.
    /// Transitions are validated and broadcast by <see cref="GameManager"/>.
    /// Only GameManager may change the current state.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        LevelUp,
        Shop,
        Paused,
        GameOver,
        Victory
    }
}
