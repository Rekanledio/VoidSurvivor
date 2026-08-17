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
- `RunnerAI` (M4.3) — faster pursuer; same pursuit pattern as ChaserAI, speed driven by RunnerData (moveSpeed 6). RunnerData.asset + Runner.prefab.
- `ShooterAI` (M4.4) — ranged attacker: approaches only outside AttackRange, stops inside it; fires a minimal `Projectile` at the player when in range and off cooldown (Time.time). ShooterData.asset (speed 2.5 / range 6 / cd 1.5 / dmg 8 / HP 25) + Shooter.prefab.
- `Projectile` (M5.1 migration) — flies at fixed velocity, expires after lifetime, on contact routes a `DamageRequest` through CombatSystem to any `IDamageable` (source carried from the firer). No direct health coupling. Known: hits any IDamageable including other enemies (no faction filter yet — later refinement).
- `TankAI` (M4.5) — slow, high-HP pursuer; same pursuit pattern as ChaserAI, identity from TankData (moveSpeed 2, maxHP 120). TankData.asset + Tank.prefab.
- `EnemySpawner` (M4.6, wave-driven since M8.1) — owns per-prefab enemy pools and the public `SpawnEnemy(prefab, position)` entry; no automatic spawn (M8.1 removed Start-time spawn).
- `WaveManager` (M8.1) — wave lifecycle 1..10: Time.deltaTime-accumulated wave time advancing only while Playing; centralized per-wave config; deterministic type rotation; spawns via EnemySpawner.SpawnEnemy at M4.6 cardinal offsets; publishes WaveStarted/WaveCompleted; after wave 10 idle (Boss M8.3). Never touches ObjectPool directly.
- Wave difficulty (M8.2) — `EnemyStats.WaveMultiplier` (runtime, non-serialized): MaxHP/Damage/MoveSpeed × per-wave multiplier (W1 1.00 → W10 1.45 in WaveTable); AttackRange/AttackCooldown unchanged. Injected per spawn; OnDespawn resets to 1 (no pool leak). EnemyData assets stay static.
- Boss (M8.3) — `BossData : EnemyData` (Minimal subclass; asset stats). `BossAI` pursues the player and deals CONTACT damage via CombatSystem (player-only, AttackCooldown timer; no projectile/skills). `EnemySpawner.SpawnBoss` uses the same pool dictionary. Wave 10 is the boss encounter (one boss, W10 multiplier, no normal spawns) → `BossSpawned` → boss defeat matches EnemyKilled → `BossDefeated` → Victory (existing legal transition). Boss death keeps the EnemyDied/EnemyKilled/Pickup chain.
- M8 COMPLETE — wave lifecycle (M8.1), difficulty growth (M8.2), boss + Victory (M8.3), final regression 58/58 PASS.

## Roguelite / Upgrade (M9, COMPLETE / ACCEPTED)
- M9.1 — XP Level Up: `PlayerProgress` Level state (start 1) + `XPToNextLevel` (PLACEHOLDER 100 × level, non-design) + AddXP carry-over + multi-level; `PlayerLevelUp(level)` event once per level. `PlayerLevelSystem` (on Player) transitions Playing → LevelUp only while Playing (non-Playing protected); WaveManager untouched (LevelUp freezes/resumes the wave).
- M9.2 — Upgrade chooser logic: `UpgradeData : ScriptableObject` (StatType enum + additive Amount; 10 assets with placeholder values). `PlayerStats` gained a runtime bonus layer (accessors = base + bonus; ApplyUpgrade / ResetForRun; serialized base never modified). `UpgradeManager` (on Player): pending level-up queue, 3 unique random options (SetForcedOptions test hook), Select guards + applies chosen once → `UpgradeSelected(upgrade, level)` → Playing when queue drains (stays LevelUp for consecutive level-ups). WaveManager untouched.
- M9.3 — LevelUp UI: `UpgradeOptionsGenerated(option0..2)` published after Options is fully written. `LevelUpPanel` (UI ns, component on the ACTIVE Canvas — inactive objects never Awake): GameStateChanged show/hide (LevelUp visible; Playing/GameOver/Victory/MainMenu hidden), event-driven 3-button labels (DisplayName / StatType / +Amount), onClick → UpgradeManager.Select(i). Scene: Canvas (Overlay + CanvasScaler 1920x1080 + GraphicRaycaster) + EventSystem with InputSystemUIInputModule (Unity Input System) + LevelUpPanel hierarchy.
- M9.4 — Shop: `PlayerProgress.TrySpendGold` (only spend entry). `ShopItemData : ScriptableObject` (Weapon/StatBonus + price + weaponPrefab | UpgradeData ref; 14 placeholder assets — weapon 30 / stat 20). `ShopManager` (scene object): WaveCompleted W1..W9 → 4 products (2 weapon + 2 stat, unique) → Shop; Purchase (stat via UpgradeData → PlayerStats.ApplyUpgrade; weapon via Instantiate + WeaponManager.Equip into empty slot, gold AFTER equip, weapon parented under WeaponManager (Owner fix); already-owned / no-slot rejected without spending); Refresh (flat 20, placeholder); Continue → Playing (wave resumes). `ShopPanel` (on active Canvas): GameStateChanged show/hide + `ShopProductsGenerated` event drives 4 product buttons + gold text; Purchase/Refresh/Continue. Single Canvas/EventSystem.
- M9.5 — Weapon Upgrade: `WeaponController` runtime layer (WeaponLevel=1 + additive bonuses; EffectiveDamage/EffectiveAttackCooldown (min 0.05 safety floor)/EffectiveRange; ApplyWeaponUpgrade requires TargetWeapon == Data; ResetWeaponUpgrades). `WeaponUpgradeData : ScriptableObject` (TargetWeapon + WeaponUpgradeStat {Damage,AttackCooldown,Range} + Amount; 12 assets, IMPLEMENTATION values). 4 weapons read Effective* at every attack (Damage/Cooldown/Range only — Scatter ProjectileCount/SpreadAngle and Boomerang flight params deferred). Shop: ShopItemType.WeaponUpgrade + ShopItemData.weaponUpgrade; ShopManager.TryPurchaseWeaponUpgrade (target must be equipped via Data match; gold only after apply; fail = no spend) + GenerateProducts 1 Weapon + 1 WeaponUpgrade (owned-only, stat fallback) + 2 StatBonus; ShopPanel upgrade card.
- Shop WeaponUpgrade UI final implementation: WeaponUpgrade card Name = ONE two-line TMP — line1 weapon name 24px (same as normal cards), line2 `升级：{属性}  等级：Lv.{X} → Lv.{X+1}` at 15px (rich text `<size=15>`, dynamic WeaponLevel via ShopManager.LevelOfEquipped); Name rect 360×60 at runtime (WeaponUpgrade only); Price 22px / Type 15px / BuyButton 24px unchanged; independent Level row removed (levelTexts cleared); normal Weapon/StatBonus cards keep Name 360×32 fs24.
- M9 Final Regression: 82/82 PASS, 0 FAILURES; M6/M7/M8 regression PASS; 3× Play/Stop 0/0; M9 ACCEPTED.
- Wave logic stays in M8.

## Combat System (M5, in progress)
- `IDamageable` (M5.1) — minimal damage-target abstraction (IsDead + TakeDamage); implemented by PlayerHealth and EnemyHealth.
- `DamageRequest` / `DamageResult` (M5.1) — structs carrying Source/Target/Damage and the minimal outcome (Applied / Damage / TargetDead).
- `CombatSystem.ApplyDamage` (M5.1) — static unified damage entry: validates target/damage, rejects dead targets, routes to the target's IDamageable, publishes `DamageApplied`, returns DamageResult. No Player/Enemy branches; health classes keep owning HP/death state.
- `DamageApplied` (M5.1) — event (Source/Target/Damage). 
- `EnemyKilled` (M5.2) — event (Enemy/Killer); published by CombatSystem exactly once when a lethal hit on EnemyHealth carries a valid Source. EnemyDied (from EnemyHealth) says an enemy died; EnemyKilled attributes it to a Source. Null-source deaths publish EnemyDied only.
- `EnemyController` (M4.1 + M5.2) — common control base; since M5.2 also the death/despawn layer: destroys its GameObject on its own EnemyDied (plain Destroy; Object Pool is M7).
- Kill attribution events done (M5.2); weapons (M6), pool (M7), waves (M8).

## Pickup System (M5.3)
- `PlayerProgress` — runtime XP/Gold resources (AddXP/AddGold, negative-safe); no level/threshold/upgrade logic (Roguelite later). Separate from PlayerStats (character attributes).
- `PickupType` (XP/Gold) + `PickupData` (ScriptableObject, read-only at runtime).
- `Pickup` — trigger collect: on player contact credits PlayerProgress, publishes PickupCollected, destroys itself (Instantiate/Destroy; pool in M7).
- `PickupSystem` — event subscriber to EnemyKilled: spawns 1 XP + 1 Gold at the enemy's death position immediately (before frame-end destroy). Deterministic MVP drop rule (XP 10 / Gold 5).
- `PickupCollected` — event (Type/Amount/Collector).

## Player Attack (M5.4)
- `PlayerAttack` — formal Player → Enemy entry: Attack(target) builds a DamageRequest (Source = Player, damage from PlayerStats.Damage) and routes it through CombatSystem.ApplyDamage, returning DamageResult. No auto-attack loop, no weapon/projectile/targeting framework (M6 owns weapon cycles and target selection).

## Weapon System (M6, in progress)
- `WeaponData` (M6.1) — ScriptableObject static config (weaponName / baseDamage / attackCooldown / range); read-only at runtime.
- `WeaponController` (M6.1) — runtime weapon: holds WeaponData, resolves the player's PlayerAttack via GetComponentInParent (Awake + lazy re-resolve on Attack), routes Attack(target) through PlayerAttack. Layering: Weapon → PlayerAttack → CombatSystem → EnemyHealth (never bypasses).
- `WeaponSlot` (M6.1) — plain runtime container (Equip/Unequip/IsEmpty).
- `WeaponManager` (M6.1) — player-side container: exactly 4 slots, Equip/Unequip/GetSlot/GetWeapon with bounds checks.
- WeaponBaseData.asset + WeaponBase.prefab — base test weapon (NOT one of the four formal weapons).
- `PulseGun` (M6.2) — first formal weapon: auto-attack loop (Time.time cooldown), minimal targeting (nearest live EnemyHealth within Range), fires one `PulseProjectile` (Source = player) per shot; self-equips into slot 0. PulseGunData (dmg 5 / cd 0.25 / range 8) + PulseGun.prefab + PulseProjectile.prefab.
- `PulseProjectile` (M6.2) — straight flight, lifetime, contact → CombatSystem with player source; skips its own Source.
- `PlayerAttack` (M5.4 + M6.2) — added Attack(target, damage) overload; Attack(target) keeps PlayerStats.Damage default (regression-compatible).
- `ScatterBlaster` (M6.3) — multi-pellet fan weapon: auto-attack loop, nearest-target gives the fan center, fires `PulseProjectile` × N simultaneously in a deterministic uniform symmetric fan. `ScatterBlasterData : WeaponData` (+projectileCount 5 / spreadAngle 45; dmg 3 / cd 0.8 / range 7).
- `WeaponController.Owner` (M6.3 fix) — lazy-resolves PlayerAttack on every access; weapons instantiated before parenting keep a valid player source.
- `Boomerang` (M6.4) — out-and-return weapon: auto-throw with single-flight rule (no new throw while one is active); `BoomerangProjectile` two-phase (Outbound to maxDistance from spawn, Return re-aims at the player's CURRENT position, hit-once per enemy per throw via HashSet). `BoomerangData : WeaponData` (maxDistance 6 / outSpeed 8 / returnSpeed 10; dmg 7 / cd 1.2 / range 8).
- `ArcBlade` (M6.5) — close-range area weapon: auto-attack loop; each strike does one `Physics2D.OverlapCircleAll` centered on the player (Range = radius) and hits every live EnemyHealth inside exactly once (deduped, dead skipped, self ignored) via PlayerAttack → CombatSystem. `ArcBladeData : WeaponData` (dmg 8 / cd 0.9 / range 2.5).
- M6 — Weapon System COMPLETE (Pulse Gun / Scatter Blaster / Boomerang / Arc Blade). No weapon upgrade/shop/roguelite yet; Object Pool is M7.

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
