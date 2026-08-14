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
