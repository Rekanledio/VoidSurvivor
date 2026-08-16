# Void Survivor — Project Context

## Project
- Name: Void Survivor
- Type: 2D Top-down Arena Roguelite
- Status: In development / M1 complete
- Target: Job-seeking portfolio + GitHub open source + Web playable + Windows build
- Local Repository: D:\Work\UnityProject\VoidSurvivor
- Unity Version: 6000.3.21f1 (Unity 6.3 LTS, verified 2026-08-14)
- Unity MCP: Connected and verified (CoplayDev unity-mcp, verified 2026-08-14)

## Tech Stack
- Unity 6.3 LTS
- C#
- Unity Input System
- Unity 2D
- TextMeshPro
- ScriptableObject
- Git / GitHub / GitHub Pages
- WorkBuddy
- Unity MCP

## Core Gameplay
- 1 player character
- 1 arena
- 4 normal enemy types
- 1 boss
- 4 weapons / 4 weapon slots
- 10 upgrade types
- 10 waves
- 5–10 minute run
- Wave combat -> XP/Gold -> Level Up/Shop -> Build -> Boss -> Victory/Defeat

## Engineering Goals
- Data-driven design
- Event-driven architecture
- Object Pooling
- Centralized Game State
- Clear system boundaries
- Web compatibility from early development

## Current Phase
Phase 1 — Core framework development

## Current Milestone
M9 — Roguelite / Upgrade (XP Level Up, upgrades, shop)

## Completed Milestones
- M0 — Project Documentation Initialization
- M1 — Unity Project Initialization (2026-08-14): Git initialized, folder structure created, SC_Main scene with orthographic camera, minimal Core entry (GameManager / GameBootstrap / GameState), Unity MCP verified, clean play-mode test.
- M2 — Core Framework (2026-08-14): GameState transition API with legal-transition table, type-safe generic EventBus, GameStateChanged event, SceneFlow/SceneIds scene-flow base, lifecycle documented in GameBootstrap. Verified via in-play smoke test (33 checks, 0 failures); test code removed after verification.
- M3 — Player System (2026-08-14): PlayerController (Input System Move/WASD, 8-way, diagonal-consistent, bounds clamp via Rigidbody2D.MovePosition), PlayerStats (10 MVP stats), PlayerHealth (TakeDamage with flat armor, Heal/FullHeal, HP clamped 0..Max, single death + PlayerDied event), CameraFollow (smooth exponential orthographic follow, no Cinemachine). Player prefab + placeholder sprite in SC_Main. Verified via in-play smoke test (29 checks, 0 failures) + dynamic play checks; test code removed. Follow-up fixes: Rigidbody2D interpolation = Interpolate (jitter), Camera orthographicSize 5 → 8, PlayerController input switched to serializable InputActionAsset reference, ground reference added (empty-scene movement was visually imperceptible — the "return to center" report was a visual-reference issue; world coordinates never actually reset, verified via in-assembly probe).
- M4 — Enemy System (2026-08-14, COMPLETE): M4.1 Enemy base framework (EnemyData/EnemyStats/EnemyHealth/EnemyController/EnemyDied/EnemyBase); M4.2 Chaser AI (pursue, speed 3.5); M4.3 Runner AI (faster pursue, speed 6); M4.4 Shooter AI (approach/stay in AttackRange, fire minimal Projectile, speed 2.5/range 6/cd 1.5/dmg 8/HP 25); M4.5 Tank AI (slow high-HP pursue, speed 2/HP 120); M4.6 minimal spawn entry (EnemySpawner spawns one of each type around the player). Verified per subtask (27/15/20/27/23/22 probe checks all PASS), 0 errors/warnings; manual in-play observation confirms all four enemies spawn and run their AI.
- M5.1 — Combat Base Framework (2026-08-14): IDamageable + DamageRequest/DamageResult + CombatSystem unified ApplyDamage + DamageApplied event; Projectile migrated off direct PlayerHealth coupling. Verified 23/23 probe PASS, 0 errors/warnings.
- M5.2 — Enemy Death & Kill Attribution (2026-08-14): EnemyKilled (Enemy/Killer) published once per lethal hit on EnemyHealth with valid Source; EnemyController destroys dead enemies (pool in M7). Verified 28/28 probe PASS, 0 errors/warnings.
- M5.3 — Pickup System (2026-08-14): PlayerProgress (XP/Gold) + PickupType/PickupData + Pickup collect + PickupSystem (EnemyKilled → drop XP+Gold at death position). XPPickup/GoldPickup prefabs; Player gains PlayerProgress; SC_Main gains PickupSystem. Verified 25/25 probe PASS + manual play, 0 errors/warnings.
- M5.4 — Player Attack Path (2026-08-14): PlayerAttack.Attack(target) → DamageRequest(Source=Player, PlayerStats.Damage) → CombatSystem; no auto-attack/weapon logic (M6 owns weapons). Player prefab gains PlayerAttack. Verified 23/23 probe PASS, 0 errors/warnings.
- M6.1 — Weapon Base Framework (2026-08-16): WeaponData (SO) + WeaponController (runtime → PlayerAttack, lazy resolve) + WeaponSlot + WeaponManager (4 slots, bounds-checked). WeaponBaseData.asset + WeaponBase.prefab (test base); Player prefab gains WeaponManager. Verified 32/32 probe PASS, 0 errors/warnings.
- M6.2 — Pulse Gun (2026-08-16): first formal weapon — auto-attack loop + nearest-target + single PulseProjectile (Source = player) → CombatSystem; PlayerAttack gains damage overload. PulseGunData (dmg 5/cd 0.25/range 8) + prefabs; SC_Main PulseGun under player. Verified 27/27 probe PASS + manual auto-combat, 0 errors/warnings.
- M6.3 — Scatter Blaster (2026-08-16): multi-pellet fan weapon — ScatterBlasterData (:WeaponData, count 5/spread 45), ScatterBlaster fires N PulseProjectiles in a deterministic uniform symmetric fan; WeaponController.Owner lazy-resolve fix. ScatterBlasterData (dmg 3/cd 0.8/range 7) + prefab. Verified 42/42 probe PASS + manual play, 0 errors/warnings.
- M6.4 — Boomerang (2026-08-16): out-and-return weapon — BoomerangData (:WeaponData, maxDistance/outSpeed/returnSpeed), Boomerang single-flight auto-throw, BoomerangProjectile two-phase (out to maxDistance, return re-aims at player's current position, hit-once per enemy). BoomerangData (dmg 7/cd 1.2/range 8) + prefabs. Verified 37/37 probe PASS, 0 errors/warnings.
- M6.5 — Arc Blade (2026-08-16): close-range area weapon — ArcBladeData (:WeaponData, Range = radius), ArcBlade one OverlapCircleAll strike per cooldown, every live in-range enemy hit once via PlayerAttack → CombatSystem. ArcBladeData (dmg 8/cd 0.9/range 2.5) + prefab (no projectile). Verified 29/29 probe PASS, 0 errors/warnings. **M6 — Weapon System: COMPLETE (all 4 weapons).**

## Next Milestone
M9 — Roguelite / Upgrade (XP Level Up, upgrades, shop)

## Current Task
M9.2 — Upgrade Chooser (3 random options; M4–M9.1 complete).

## Important Decisions
- Do not add special differentiation mechanics for MVP.
- Use Brotato + Vampire Survivors as gameplay references, without copying content/art/assets.
- Prioritize completeness and engineering quality over content quantity.
- Web playable version is a first-class deliverable.
- Do not rely on chat history as the sole source of project memory.

## Context Rules
1. Repository documents are the shared long-term memory.
2. Code and test results are the source of truth for actual implementation state.
3. Major design/architecture changes must be recorded in DECISIONS.md.
4. Milestone completion must update PROJECT_CONTEXT.md, TASKS.md and SAVE_CONTEXT.md.
