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

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Pickup.Pickup"/> when a pickup is
    /// collected by the player (M5.3). Carries kind, amount and the collector.
    /// Level-up / thresholds are NOT evaluated here (Roguelite system later).
    /// </summary>
    public readonly struct PickupCollected
    {
        public VoidSurvivor.Pickup.PickupType Type { get; }
        public int Amount { get; }
        public GameObject Collector { get; }

        public PickupCollected(VoidSurvivor.Pickup.PickupType type, int amount, GameObject collector)
        {
            Type = type;
            Amount = amount;
            Collector = collector;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Enemy.WaveManager"/> exactly once
    /// when a wave officially starts (M8.1). Fact-only event; no UI logic here.
    /// </summary>
    public readonly struct WaveStarted
    {
        public int WaveIndex { get; }

        public WaveStarted(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Enemy.WaveManager"/> exactly once
    /// when a wave officially ends (M8.1). Fact-only event; Boss/Victory logic
    /// belongs to later M8 subtasks.
    /// </summary>
    public readonly struct WaveCompleted
    {
        public int WaveIndex { get; }

        public WaveCompleted(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Enemy.WaveManager"/> when the Wave 10
    /// boss is spawned (M8.3). Fact-only event.
    /// </summary>
    public readonly struct BossSpawned
    {
        public GameObject Boss { get; }

        public BossSpawned(GameObject boss)
        {
            Boss = boss;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Enemy.WaveManager"/> once when the
    /// Wave 10 boss is defeated by the player (M8.3), after which it enters
    /// Victory. Fact-only event; existing EnemyKilled semantics unchanged.
    /// </summary>
    public readonly struct BossDefeated
    {
        public GameObject Boss { get; }
        public GameObject Killer { get; }

        public BossDefeated(GameObject boss, GameObject killer)
        {
            Boss = boss;
            Killer = killer;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Player.PlayerProgress"/> once per
    /// level gained (M9.1). Carries the NEW level. Fact-only event; the upgrade
    /// chooser (M9.2) reacts to it. One AddXP can publish several of these.
    /// </summary>
    public readonly struct PlayerLevelUp
    {
        public int Level { get; }

        public PlayerLevelUp(int level)
        {
            Level = level;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Player.UpgradeManager"/> after an
    /// upgrade has been applied to PlayerStats (M9.2). Carries the chosen
    /// upgrade and the player's level at selection time. Fact-only event.
    /// </summary>
    public readonly struct UpgradeSelected
    {
        public VoidSurvivor.Player.UpgradeData Upgrade { get; }
        public int Level { get; }

        public UpgradeSelected(VoidSurvivor.Player.UpgradeData upgrade, int level)
        {
            Upgrade = upgrade;
            Level = level;
        }
    }

    /// <summary>
    /// Published by <see cref="VoidSurvivor.Player.UpgradeManager"/> AFTER the
    /// current candidate set has been fully written into Options (M9.3), so a
    /// UI listening for this event can always read the complete candidates.
    /// Carries the up-to-3 options (null slots when the pool is exhausted).
    /// Fact-only event; no UI logic.
    /// </summary>
    public readonly struct UpgradeOptionsGenerated
    {
        public VoidSurvivor.Player.UpgradeData Option0 { get; }
        public VoidSurvivor.Player.UpgradeData Option1 { get; }
        public VoidSurvivor.Player.UpgradeData Option2 { get; }

        public UpgradeOptionsGenerated(
            VoidSurvivor.Player.UpgradeData option0,
            VoidSurvivor.Player.UpgradeData option1,
            VoidSurvivor.Player.UpgradeData option2)
        {
            Option0 = option0;
            Option1 = option1;
            Option2 = option2;
        }
    }
}
