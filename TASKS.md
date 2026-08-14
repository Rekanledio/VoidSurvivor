# Void Survivor — Tasks

## Completed: M0 — Project Documentation Initialization (2026-08-14)

### M0.1 Documentation
- [x] Define project scope
- [x] Define MVP boundaries
- [x] Define long-term context strategy
- [x] Add all project documents to the real repository
- [x] Review documents after first Unity project creation

### M0.2 Tooling
- [x] Verify WorkBuddy project workflow
- [x] Verify Unity MCP connection for this project
- [x] Verify Git workflow
- [x] Verify Unity 6.3 LTS version

## Completed: M1 — Unity Project Initialization (2026-08-14)
- [x] Create Unity project (created manually by user, Unity 6000.3.21f1)
- [x] Initialize Git repository (main branch, Unity .gitignore)
- [x] Create folder structure (per ARCHITECTURE.md)
- [x] Create initial scene (Assets/Scenes/SC_Main.unity, orthographic camera)
- [x] Configure Input System (activeInputHandler=1, Input System only)
- [x] Configure basic project settings (2D default behavior mode, URP, Linear color space)
- [x] Connect Unity MCP (CoplayDev unity-mcp, verified via WorkBuddy)
- [x] Verify WorkBuddy can read and modify project (scene created and modified via MCP)
- [x] Run first successful build/test (play mode test passed, no console errors)

## Completed: M2 — Core Framework (2026-08-14)
- [x] GameState transition API on GameManager (TryChangeState, legal-transition table, GameStateChanged broadcast)
- [x] Core event bus for cross-system communication (EventBus: Subscribe/Unsubscribe/Publish/Clear, struct events)
- [x] Scene flow entry for MainMenu / Gameplay / Result (SceneFlow + SceneIds, same-scene reload guard)
- [x] Lifecycle documented (GameBootstrap → GameManager.Awake → scene Awake/Start; EventBus lazy static)
- [x] No gameplay systems introduced (Player/Enemy/Weapon/Wave/Shop untouched)
- [x] Verified: in-play smoke test 33 checks / 0 failures; GameManager, EventBus, SceneFlow covered; temp test code removed

## Completed: M3 — Player System (2026-08-14)
- [x] Player movement via Input System (reused existing InputSystem_Actions Player/Move, WASD + arrows + gamepad)
- [x] 8-way movement with consistent diagonal speed (input normalized, magnitude capped at 1)
- [x] Rigidbody2D.MovePosition physics-based movement (no Transform hack), configurable MoveSpeed
- [x] Configurable arena bounds clamp (placeholder, no real map)
- [x] PlayerStats with all 10 MVP stats (GAME_DESIGN.md), plain base values + read accessors
- [x] PlayerHealth: TakeDamage (flat armor reduction) / Heal / FullHeal / IsDead, HP clamped to [0, MaxHP]
- [x] Death: single trigger, IsDead flag, PlayerDied event via EventBus (no Game Over flow)
- [x] CameraFollow: smooth exponential orthographic follow (no Cinemachine), offset, optional bounds
- [x] Player prefab + placeholder sprite placed in SC_Main
- [x] Verified: in-play smoke test 29 checks / 0 failures; dynamic play checks (camera convergence, bounds, component state); temp test code removed
- [x] Bug fix (2026-08-14): Rigidbody2D interpolation = Interpolate (removes movement jitter); Camera orthographicSize 5 → 8 (viewport matches arena); PlayerController input switched to serializable InputActionAsset reference (fixes non-persisted InputActionReference). Verified: full -20..+20 traversal, bounds hold, no pull-back, camera follows, 0 console errors.

## Current Milestone: M4 — Enemy System
- [x] M4.1 Enemy base framework (2026-08-14): EnemyData (ScriptableObject static config), EnemyStats (read-only runtime view), EnemyHealth (CurrentHP/TakeDamage/clamp/single death + EnemyDied event), EnemyController (common refs + Player target), EnemyBase prefab + EnemyPlaceholder sprite + EnemyBase.asset. Verified: 27/27 probe checks PASS, 0 errors/warnings.
- [x] M4.2 Chaser AI (2026-08-14): ChaserAI pursues the player at EnemyData MoveSpeed via Rigidbody2D.MovePosition, reuses EnemyController references (no per-frame GetComponent/Find), stops when dead. ChaserData.asset (moveSpeed 3.5) + Chaser.prefab (base + ChaserAI). Verified: 15/15 probe checks PASS (instantiation, HP, target resolve, speed 3.5 exact, approach, re-target, continuous chase, death stop, player intact), 0 errors/warnings.
- [x] M4.3 Runner AI (2026-08-14): RunnerAI — faster pursuer (RunnerData moveSpeed 6 vs Chaser 3.5), same pursuit pattern as ChaserAI (MovePosition, EnemyController refs, stops when dead). RunnerData.asset + Runner.prefab (base + RunnerAI). Verified: 20/20 probe checks PASS (speed 6.00 exact over fixed physics frames, re-target, chase, physics hold near player, death stop 0.000, Chaser unaffected, player intact), 0 errors/warnings.
- [x] M4.4 Shooter AI (2026-08-14): ShooterAI — approaches only outside AttackRange, stops inside it, fires a minimal Projectile at the player when in range and off cooldown (Time.time-based). Minimal M4.4 Projectile (kinematic, velocity, lifetime, contact damage via PlayerHealth.TakeDamage — flagged as temporary until M5). ShooterData.asset (speed 2.5 / range 6 / cooldown 1.5 / dmg 8 / HP 25) + ShooterProjectile.prefab + Shooter.prefab. Verified: 27/27 probe checks PASS (approach/stops at range edge, fire→hit→damage via player HP, cooldown window, fires after cooldown, out-of-range silence, death stops move+attack, Chaser/Runner regression, no projectile residue), 0 errors/warnings.
- [x] M4.5 Tank AI (2026-08-14): TankAI — slow, high-HP pursuer (same pursuit pattern as ChaserAI; MovePosition + EnemyController refs; stops when dead; no special attack). TankData.asset (moveSpeed 2 < Chaser 3.5, maxHP 120 > others 30) + Tank.prefab (base + TankAI). Verified: 23/23 probe checks PASS (speed 2.00 exact over fixed physics frames, re-target on player move/crossing, TakeDamage→90, clamp 0, single EnemyDied, death stop, Chaser/Runner/Shooter regression), 0 errors/warnings.
- [x] M4.6 Minimal spawn entry (2026-08-14): EnemySpawner — Start-time single spawn of one instance per configured prefab at fixed cardinal offsets (10 units) around the player; no wave/timer/loop logic (M8 owns waves). EnemySpawner object in SC_Main wired to Chaser/Runner/Shooter/Tank prefabs. Verified: 22/22 probe checks PASS (4 enemies spawned at ~10, count == 4, components + target resolved, Chaser/Runner/Tank AI moving, no duplicate spawning) + manual observation of all 4 enemies in-play (Chaser/Runner/Tank pursue, Shooter fires projectiles), 0 errors/warnings.

## M4 — Enemy System: COMPLETE (2026-08-14)

## Current Milestone: M5 — Combat System
- [x] M5.1 Combat Base Framework (2026-08-14): IDamageable (PlayerHealth/EnemyHealth implement), DamageRequest/DamageResult structs, CombatSystem static unified ApplyDamage entry, DamageApplied event; Projectile migrated off direct PlayerHealth coupling to the combat entry (with source). Verified: 23/23 probe checks PASS (player/enemy damage via combat, death + rejection of dead targets, projectile fly→hit→combat→damage→destroy, DamageApplied events, Chaser/Runner/Tank/Shooter/Spawner regression), 0 errors/warnings.

## Rules
- Only mark a task complete after actual verification.
- When tasks change, keep this document synchronized with the current project state.

## M3 Bug Fix #2 (2026-08-14) — "return to center" investigation
- [x] Full-project audit of all position writes (only CameraFollow→Camera, PlayerController→MovePosition; no reset code)
- [x] In-assembly runtime probe (temporary AutoProbe): W/A/D movement exact (5 u/s), position kept after release across 3 Play/Stop cycles
- [x] Camera follow verified (player ↔ camera aligned)
- [x] Root cause: empty scene = no visual reference; camera-follow movement invisible → perceived "small range + return to center" (world coordinates never reset)
- [x] Fix: added Ground reference (GroundPlaceholder grid, sortingOrder -10); no player/camera logic changed
- [x] Final clean play/stop: 0 errors / 0 warnings; temp probe removed

## M3 Full Review + Visual Scale (2026-08-14)
- [x] Full code review (Player 4 scripts + Core 6 scripts): clean, no temp/debug residue, no required fixes
- [x] Scene/Prefab review: no missing components, inputActions persisted, single Player, Rigidbody2D Interpolate
- [x] Temp residue scan: zero matches; all 10 scripts are formal
- [x] Regression 15/15 PASS (temporary probe, removed)
- [x] Visual scale: Camera size 7, Player scale 1.5, Ground 8px grid (4-unit spacing); bounds/speed unchanged
- [x] Final play/stop: 0 errors / 0 warnings
