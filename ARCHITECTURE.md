# Void Survivor — Architecture

## Architectural Goals
- Clear responsibilities
- Low coupling
- Data-driven configuration
- Event-driven cross-system communication
- Object pooling for frequently spawned objects
- Web-friendly implementation

## Main Systems
- Core / GameManager / GameState
- Player
- Enemy
- Combat
- Weapons
- Wave
- Roguelite
- Shop
- UI
- Audio
- Save
- Utilities / Pooling

## Player System (M3, implemented)
- `PlayerController` — Input System driven movement (reuses InputSystem_Actions Player/Move: WASD + arrows + gamepad). `Rigidbody2D.MovePosition` for physics-correct movement; normalized input keeps diagonal speed equal to cardinal; configurable arena bounds clamp. Exposes static math helpers (`NormalizeMoveInput`, `ClampToBounds`) shared with CameraFollow.
- `PlayerStats` — base values + read accessors for the 10 MVP stats (GAME_DESIGN.md). Modifiers arrive with the Roguelite milestone (M9).
- `PlayerHealth` — CurrentHP/MaxHP/IsDead; TakeDamage (flat armor reduction from PlayerStats.Armor), Heal/FullHeal; HP clamped [0, MaxHP]; death fires once and publishes `PlayerDied`.
- `CameraFollow` — smooth exponential orthographic follow on the Main Camera; no Cinemachine; optional bounds reserved for the real arena.
- Visual placeholder: Assets/Art/PlayerPlaceholder.png; prefab: Assets/Prefabs/Player.prefab.
- Event: `PlayerDied` added to GameEvents (M3).

## Enemy System (M4, in progress)
- `EnemyData` (M4.1) — ScriptableObject static configuration per enemy type (MaxHP/Damage/AttackRange/AttackCooldown/MoveSpeed); one asset per type (Chaser/Runner/Shooter/Tank/Boss).
- `EnemyStats` (M4.1) — MonoBehaviour read-only runtime view over an EnemyData asset; configuration assets are never mutated at runtime (mirrors PlayerStats).
- `EnemyHealth` (M4.1) — CurrentHP/MaxHP/IsDead; TakeDamage clamps HP to [0, MaxHP]; death fires once and publishes `EnemyDied` (carries the enemy GameObject). Combat attribution (EnemyKilled) is added by M5.
- `EnemyController` (M4.1) — common control base: caches EnemyStats/EnemyHealth/Rigidbody2D in Awake, resolves the PlayerHealth target once in Start; read-only Stats/Health/Body/Target; per-type AI (M4.2+) composes or derives from it. No AI behavior in the base.
- Placeholder: Assets/Art/EnemyPlaceholder.png; base prefab: Assets/Prefabs/Enemies/EnemyBase.prefab.
- `ChaserAI` (M4.2) — pursues the player's current position at the configured MoveSpeed via Rigidbody2D.MovePosition; reuses EnemyController's resolved refs (Target/Stats/Health/Body); stops when dead. ChaserData.asset + Chaser.prefab.
- Per-type prefabs and AI (Runner/Shooter/Tank) land in M4.3–M4.5; minimal spawn entry in M4.6; full wave logic stays in M8.

## Data Layer
Use ScriptableObject for static configuration:
- CharacterData
- WeaponData
- EnemyData
- UpgradeData
- WaveData
- ShopItemData
- GameConfig

## Runtime Layer
Runtime objects consume configuration assets and maintain mutable state at runtime.

## Core Event Examples
- GameStarted
- WaveStarted
- WaveCompleted
- EnemyKilled
- PlayerDamaged
- PlayerLevelUp
- ItemPurchased
- BossSpawned
- BossDefeated
- PlayerDied
- GameWon
- GameOver

## Game State
- MainMenu
- Playing
- LevelUp
- Shop
- Paused
- GameOver
- Victory

## Core Framework (M2, implemented)
- `GameManager` — the only owner of the current GameState. `TryChangeState` validates transitions against a legal-transition table and broadcasts `GameStateChanged`. Not a God class: it does not load scenes or host gameplay systems.
- `EventBus` — type-safe generic static bus (Subscribe/Unsubscribe/Publish/Clear) for decoupled cross-system communication. Events are structs to avoid boxing.
- `GameEvents` — core event definitions. M2 defines only `GameStateChanged`; gameplay events (WaveStarted, EnemyKilled, PlayerDamaged, ItemPurchased, BossSpawned, etc.) are added by their owning milestones.
- `SceneFlow` / `SceneIds` — minimal scene-load wrapper (MainMenu / Gameplay / Result identifiers, same-scene reload guard). Scene assets are created by their owning milestones.
- Legal state transitions (MVP flow):
  MainMenu → Playing; Playing → Paused / LevelUp / Shop / GameOver / Victory; Paused → Playing; LevelUp → Playing; Shop → Playing; GameOver → MainMenu; Victory → MainMenu.

## Pooling Targets
- Enemies
- Projectiles
- Pickups
- Hit/death effects

## Architecture Rule
Do not introduce a new cross-system dependency without documenting the reason.
