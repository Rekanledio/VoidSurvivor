namespace VoidSurvivor.Core
{
    /// <summary>
    /// Centralized game states, aligned with ARCHITECTURE.md.
    /// State transition logic and events are implemented in M2 (Core Framework).
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
