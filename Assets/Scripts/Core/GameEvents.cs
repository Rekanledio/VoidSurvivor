using UnityEngine;

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

    /// <summary>
    /// Published once when the player dies (M3). Payload intentionally empty:
    /// the singleton player is implied. Extended by later milestones if needed.
    /// </summary>
    public readonly struct PlayerDied
    {
    }

    /// <summary>
    /// Published once when an enemy dies (M4.1). Carries the enemy GameObject so
    /// multiple enemies can be distinguished. Combat/XP/Gold (M5/M9) will listen;
    /// the kill attribution event is added by the Combat milestone.
    /// </summary>
    public readonly struct EnemyDied
    {
        public GameObject Enemy { get; }

        public EnemyDied(GameObject enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Combat.CombatSystem"/> for every
    /// applied damage request (M5.1). Carries source (may be null), target and
    /// nominal damage. Kill attribution / XP / Gold events come later.
    /// </summary>
    public readonly struct DamageApplied
    {
        public GameObject Source { get; }
        public GameObject Target { get; }
        public float Damage { get; }

        public DamageApplied(GameObject source, GameObject target, float damage)
        {
            Source = source;
            Target = target;
            Damage = damage;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Combat.CombatSystem"/> exactly once
    /// when an enemy dies and the lethal DamageRequest carried a valid Killer
    /// (M5.2). Distinct from EnemyDied: EnemyDied says an enemy died; EnemyKilled
    /// attributes that death to a source. Null-source deaths publish EnemyDied
    /// only. XP / Gold / rewards come with later milestones.
    /// </summary>
    public readonly struct EnemyKilled
    {
        public GameObject Enemy { get; }
        public GameObject Killer { get; }

        public EnemyKilled(GameObject enemy, GameObject killer)
        {
            Enemy = enemy;
            Killer = killer;
        }
    }
}
