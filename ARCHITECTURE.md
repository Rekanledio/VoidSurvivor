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

## Pooling Targets
- Enemies
- Projectiles
- Pickups
- Hit/death effects

## Architecture Rule
Do not introduce a new cross-system dependency without documenting the reason.
