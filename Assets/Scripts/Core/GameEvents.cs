namespace VoidSurvivor.Core
{
    /// <summary>
    /// Published by <see cref="GameManager"/> whenever the current game state changes.
    /// The only core event defined in M2; gameplay events are added by their own milestones.
    /// </summary>
    public readonly struct GameStateChanged
    {
        public GameState From { get; }
        public GameState To { get; }

        public GameStateChanged(GameState from, GameState to)
        {
            From = from;
            To = to;
        }

        public override string ToString() => $"{From} -> {To}";
    }
}
