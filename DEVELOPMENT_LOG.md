# Void Survivor — Development Log

## 2026-08-14
### Milestone
M0 — Project Documentation Initialization

### Completed
- Finalized game direction.
- Finalized MVP scope.
- Finalized Unity + WorkBuddy + Unity MCP workflow.
- Finalized Web + Windows delivery target.
- Designed shared long-term context documentation strategy.
- Created initial project memory documents.

### Next
Initialize the actual Unity project and connect the tooling workflow.

## 2026-08-14
### Milestone
M1 — Unity Project Initialization

### Completed
- Verified project environment: Unity 6000.3.21f1 (6.3 LTS), URP 17.3.0, Input System 1.20.0 as sole input handler, 2D default behavior mode, Linear color space. No pre-existing console errors.
- Initialized Git (branch `main`) with Unity `.gitignore`; confirmed Library/Temp/Logs/Obj/UserSettings excluded.
- Created full Assets folder skeleton per ARCHITECTURE.md (Scripts + ScriptableObjects subfolders).
- Created `Assets/Scenes/SC_Main.unity` via Unity MCP: orthographic Main Camera (size 5), no gameplay objects; scene set as the only Build Settings entry.
- Created minimal Core entry: `GameState` enum (states per ARCHITECTURE.md), `GameManager` singleton (DontDestroyOnLoad), `GameBootstrap` (RuntimeInitializeOnLoadMethod). No gameplay logic implemented.
- Verified Unity MCP end-to-end: scene read/create/modify, build settings, play/stop, console read.
- Play-mode verification: GameManager auto-created with component in DontDestroyOnLoad scene; final console state 0 errors / 0 warnings.

### Issues Encountered
- First MCP play test happened before Unity had compiled the newly written Core scripts, so GameManager was absent. Reimport + recompile resolved it.
- A transient pair of "referenced script (Unknown) missing" console errors appeared once during the first compile cycle; did not reproduce in later compile/play/stop cycles. Recorded in KNOWN_ISSUES.md as an observation.
- Discovered mcp execute_code (CodeDom) loads its own copy of Assembly-CSharp; static fields read through it are unreliable. Runtime state must be verified through UnityEngine object queries.

### Next
M2 — Core Framework: GameState transitions, event bus, scene flow entry, utilities foundation.

## 2026-08-14
### Milestone
M2 — Core Framework

### Completed
- GameManager upgraded to the centralized state owner: `TryChangeState` single entry, legal-transition table, same-state/illegal transitions rejected with warning, `GameStateChanged` broadcast.
- EventBus added: type-safe generic static bus (Subscribe/Unsubscribe/Publish/Clear), struct events, no third-party dependency.
- GameEvents added: `GameStateChanged` (only core event in M2; gameplay events deferred to their milestones).
- SceneFlow + SceneIds added: minimal scene-load wrapper (MainMenu/Gameplay/Result constants, same-scene reload guard). No scene assets created.
- Lifecycle documented in GameBootstrap: GameBootstrap (BeforeSceneLoad) → GameManager.Awake → scene Awake/Start; EventBus lazy static.
- No gameplay systems introduced.

### Verification
- MCP validate_script (standard level): 0 errors / 0 warnings for all new/modified scripts.
- In-play smoke test (temporary `CoreSmokeTest.cs`): 33 checks, 0 failures — full state flow, illegal/redundant transition rejection, GameStateChanged event ordering + unsubscribe, EventBus subscribe/publish/unsubscribe/duplicate/Clear, SceneFlow constants + same-scene guard.
- After deleting the temp test: clean play/stop — 0 errors, 0 warnings; GameManager verified in DontDestroyOnLoad with CurrentState=MainMenu via MCP execute_code.
- Compile re-check after test removal: no errors.

### Issues Encountered
- Once, right after deleting the temp test script, the next play session still ran the old assembly (compile pending), so CurrentState was observed as Playing. Re-verified after compile completed: clean MainMenu. Lesson: always confirm compilation finished before verifying state after removing scripts.
- The transient "referenced script (Unknown) missing" console pair reappeared during the recompile cycle after test removal; stable states are clean. Updated KNOWN_ISSUES.md (it correlates with script recompile cycles, not play itself).

### Next
M3 — Player System: movement, stats, health, camera follow.

## 2026-08-14
### Milestone
M3 — Player System

### Completed
- PlayerController: reuses the existing InputSystem_Actions asset (Player/Move: WASD + arrows + gamepad) — no new input config; reads Vector2 in FixedUpdate; NormalizeMoveInput caps diagonal magnitude at 1 (consistent diagonal speed, analog partial range kept); physics movement via Rigidbody2D.MovePosition; configurable arena bounds clamp (default ±20); movement disabled after death; component refs cached in Awake (no per-frame GetComponent).
- PlayerStats: all 10 MVP stats (GAME_DESIGN.md) as base values + read accessors; modifiers explicitly deferred to M9.
- PlayerHealth: CurrentHP/MaxHP/IsDead; TakeDamage with flat armor reduction (armor from PlayerStats); Heal/FullHeal; HP clamped [0, MaxHP]; single death (guard) publishing PlayerDied via EventBus; no Game Over flow.
- CameraFollow: exponential frame-rate-independent smoothing; X/Y follow with fixed Z=-10 offset; optional bounds reserved for later arena; no Cinemachine.
- GameEvents: added PlayerDied (empty payload).
- Player prefab (Assets/Prefabs/Player.prefab) + placeholder sprite (Assets/Art/PlayerPlaceholder.png, 64px cyan circle) placed in SC_Main; Main Camera gets CameraFollow targeting Player.

### Verification
- MCP validate_script: 0 errors / 0 warnings for all four player scripts.
- In-play smoke test (temporary PlayerSmokeTest.cs): 29 checks, 0 failures — stats read, TakeDamage/Heal/FullHeal, HP never below 0 / never above MaxHP, death fires exactly once + no heal after death, PlayerDied event count == 1, diagonal normalization (magnitude 1), diagonal velocity == MoveSpeed, bounds clamping.
- Dynamic play checks: teleported player to (30,30) then (5,5); camera stayed at (30,30) right after teleport and converged to (5,5) within 2s (smooth, no snap); full component set verified on scene GO and prefab; clean session: hp=100/100, dead=False.
- After deleting the temp test: clean play/stop — 0 errors, 0 warnings.

### Issues Encountered
- MCP manage_gameobject requires fully-qualified component names for project scripts (short names like "PlayerStats" fail with "not found").
- mcp execute_code (CodeDom) lacks some UnityEditor APIs (AssetDatabase.GetAtPath); LoadAssetAtPath/Refresh/ImportAsset, SerializedObject and InputActionReference.Create all work — used to assign the Move InputActionReference to PlayerController and the sprite to SpriteRenderer.
- Placeholder PNG was auto-imported as Sprite by Unity 6 (2D project default), so no manual importer tweaks were needed.

### Next
M4 — Enemy System: enemy base framework, Chaser/Runner/Shooter/Tank AI, minimal spawn entry (wave logic stays in M8).

## 2026-08-14
### Milestone
M3 Bug Fix — player movement boundary & camera jitter

### Reported Issues (manual play test)
1. Player seemed to move only in a small range, then returned toward the center near the edge.
2. Holding WASD caused visible jitter / blur.

### Root Causes (verified, not guessed)
- **Issue 2 (jitter) — confirmed code-level defect:** `Rigidbody2D.interpolation = None`. Physics position stepped at FixedUpdate rate with no rendering interpolation, while CameraFollow smoothed every frame → relative jitter/trailing. Fixed with the standard mechanism: `RigidbodyInterpolation2D.Interpolate` (no Cinemachine).
- **Issue 1 (small range / pull-back perception):** No code anywhere pulls the player back toward the center (verified in play: position holds with no input, stops at bounds, A/D covered the full -20..20 range through the center). The perceived "small range + return to center" came from the combination of (a) the jitter (issue 2) making movement feel unstable, and (b) a small camera viewport (orthographicSize 5 → visible height 10 units) so the player quickly left the visible area; after releasing the key the camera's exponential smoothing converged to the player, reading as "pulled back to center". Fixed by widening the viewport to orthographicSize 8 and removing the jitter.
- **Latent serialization defect found while investigating:** `PlayerController` originally took an `InputActionReference` assigned via `InputActionReference.Create()`, which is a runtime (non-asset) object — the reference was NOT persisted to the scene/prefab files (verified: no `moveAction` key in YAML). It only worked within the current editor session; after reopening the project the player would not move at all. Reworked to `[SerializeField] InputActionAsset inputActions` + `FindAction("Move")` at Awake (asset reference persists; verified `inputActions:` present in Player.prefab YAML).

### Changes
- `PlayerController.cs`: InputActionReference → InputActionAsset (serializable); Move resolved by name in Awake.
- `Rigidbody2D.interpolation = Interpolate` (scene instance + Player.prefab).
- `Camera.orthographicSize` 5 → 8 (scene; Camera is not part of the prefab).
- No architecture changes; no other systems touched.

### Verification (Unity MCP + play)
- Confirmed all settings applied and persisted: rb interpolation=Interpolate, cam size 8, `inputActions` asset reference in Player.prefab YAML, `m_Interpolate: 1`, camera `orthographic size: 8`.
- Play tests (simulated WASD): W moved to +20 (north bound, stayed, no pull-back); A moved to -20; D covered -20 → +20 through center (full range, no pull-back); camera followed throughout.
- Console after play/stop: 0 errors, 0 warnings.
- Note: simulated keyboard events in a background editor have unstable event retention, so per-step distance is not representative; direction, bounds and full-range traversal are.

### Next
M4 — Enemy System.

## 2026-08-14
### Milestone
M3 Bug Fix #2 — player position "return to center" investigation (final root cause)

### User report (manual play test)
1. Player seemed to move only in a small range.
2. After releasing W/A/S/D the player seemed to return to the center.
3. Jitter was confirmed fixed.

### Investigation (deterministic, not simulated-keyboard guessing)
- Full-project search: only two scripts ever write positions — CameraFollow writes only the Camera transform; PlayerController writes only via Rigidbody2D.MovePosition. No code resets the player to the origin.
- Runtime probe inside the real game assembly (temporary AutoProbe, drove the Input System in-process, deleted after): W hold 1s -> (0,0)->(0,5); release 1.5s -> (0,5) KEPT; A hold -> (-5,5); release -> KEPT; D crossed -10 -> -2.5 through the center; release -> KEPT. Speed exact 5 u/s. Identical across 3 separate Play sessions (Stop->Play).
- Camera verified following perfectly: player (-2.5,5) <-> camera (-2.5,5,-10); CameraFollow enabled.
- Console: 0 errors/warnings throughout; InputActionAsset input reference confirmed persisted and working across Play restarts.
- ROOT CAUSE — visual reference, not position logic: SC_Main was an empty scene (only Camera + Player). With the camera following the player, the player stays centered on screen and the empty background gives no visual reference, so movement is invisible; the always-centered player reads as "small range" and "returns to center" after release. Player world coordinates never actually returned to the center.

### Fix
- Added a simple ground reference (Assets/Art/GroundPlaceholder.png grid texture, SpriteRenderer sortingOrder -10, scale 50) so movement is visually observable. No player/camera logic changed (previous fixes: interpolation=Interpolate, camera size 8, InputActionAsset reference remain).

### Verification
- Probe sequence identical across Play/Stop cycles; camera follows; console 0/0 after final clean play/stop.

### Next
M4 — Enemy System.

## 2026-08-14
### Milestone
M3 Full Review + visual scale adjustment

### Review results
- Player code (PlayerController/PlayerStats/PlayerHealth/CameraFollow): clean — no temp/debug/test code, no high-frequency GetComponent, refs cached in Awake, InputActionAsset persisted, death fires once. No required fixes.
- Core (GameManager/EventBus/GameBootstrap/GameEvents/SceneFlow): M2 architecture intact; M3 added only PlayerDied event. No regression. Note (Post-MVP): EventBus static handlers rely on domain reload for reset; revisit if domain reload is disabled.
- Scene/Prefab: no missing components (Player 7/7, Camera 4/4), no null refs, prefab instance consistent, inputActions reference present in both prefab and scene, Rigidbody2D Interpolate + no constraints, single active Player, no stray GameObjects.
- Temp residue scan: zero matches (no SmokeTest/AutoProbe/TestRunner/TEMP); all 10 project scripts are formal code.
- Assets/GUIDs: GroundPlaceholder & PlayerPlaceholder imported as Sprite; prefab/scene GUID refs intact.

### Visual scale adjustment (final values)
- Camera orthographicSize: 8 -> 7 (view height 14 units)
- Player placeholder visual scale: 1 -> 1.5 (0.64 -> 0.96 units, clearly visible)
- Ground: scale 50 kept (covers +-32), texture regenerated with 8px grid -> 4-unit grid spacing (~3.5 cells on screen at size 7)
- Movement bounds (+-20) and MoveSpeed (5) unchanged; CameraFollow algorithm unchanged.

### Regression (temporary MiniRegress probe, deleted after)
15/15 PASS, 0 FAILURES: W/S/A/D direction+speed (5 u/s), diagonal speed consistency, bounds stop at 20, release keeps position, camera follows, HP full, TakeDamage(30)->70, death once, no second event. Final clean play/stop: 0 errors / 0 warnings.

## 2026-08-14
### Milestone
M4.1 — Enemy Base Framework (part of M4 — Enemy System)

### Completed
- EnemyData (Assets/Scripts/Enemy/EnemyData.cs): ScriptableObject static configuration per enemy type (MaxHP/Damage/AttackRange/AttackCooldown/MoveSpeed), CreateAssetMenu, read-only accessors. One asset per type later (Chaser/Runner/Shooter/Tank/Boss).
- EnemyStats (EnemyStats.cs): MonoBehaviour read-only runtime view over an EnemyData asset; runtime state never mutates configuration assets (mirrors PlayerStats pattern).
- EnemyHealth (EnemyHealth.cs): CurrentHP/MaxHP/IsDead; TakeDamage clamps HP to [0, MaxHP]; death fires exactly once and publishes EnemyDied via EventBus (mirrors PlayerHealth pattern; no combat logic).
- EnemyController (EnemyController.cs): common control base — caches EnemyStats/EnemyHealth/Rigidbody2D in Awake; resolves PlayerHealth target once in Start; exposes Stats/Health/Body/Target read-only; extension point for per-type AI (M4.2+). No AI behavior implemented.
- GameEvents (Core): added EnemyDied (struct, carries the enemy GameObject) — leaves EnemyKilled/attribution to the Combat milestone (M5).
- Assets: Assets/Art/EnemyPlaceholder.png (64px red square, Python-generated); Assets/ScriptableObjects/Enemies/EnemyBase.asset (default values); Assets/Prefabs/Enemies/EnemyBase.prefab (SpriteRenderer + Rigidbody2D Dynamic/gravity 0/Interpolate + BoxCollider2D 0.64 + EnemyStats/EnemyHealth/EnemyController, data reference persisted).

### Verification
- Compilation: 0 errors.
- In-play probe (temporary EnemyProbe, deleted): 27/27 PASS, 0 FAILURES — prefab loaded; 3 components present; Stats values (30/10/1.5/1/3); HP init 30/30; TakeDamage(10) -> 20; clamp to 0; IsDead; EnemyDied published once with enemy reference; no second death; Controller refs resolved; Target resolves to PlayerHealth.
- Final clean play/stop after probe removal: 0 errors, 0 warnings.

### Next
M4.2 — Chaser AI (approach player). Wave logic stays in M8.

## 2026-08-14
### Milestone
M4.2 — Chaser AI (part of M4 — Enemy System)

### Completed
- ChaserAI (Assets/Scripts/Enemy/ChaserAI.cs): chases the player's current world position at the configured MoveSpeed using Rigidbody2D.MovePosition (physics-compatible, no Transform writes). Reuses the references resolved by EnemyController (Target/Stats/Health/Body) — no per-frame GetComponent/Find. Stops when EnemyHealth.IsDead. No attack/damage/projectile logic.
- ChaserData.asset (Assets/ScriptableObjects/Enemies/ChaserData.asset): EnemyData asset, moveSpeed 3.5 (maxHP 30, damage 10, range 1.5, cooldown 1).
- Chaser.prefab (Assets/Prefabs/Enemies/Chaser.prefab): based on EnemyBase + ChaserAI component, EnemyStats.data = ChaserData.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary ChaserProbe, deleted): 15/15 PASS, 0 FAILURES — prefab instantiation; HP 30/30; player target resolved; MoveSpeed 3.5 from data; moved 3.50 in 1s (exact); approaches player; speed stays ~3.5 across segments (no abnormal acceleration); re-targets after player teleport; continues chasing when player moves back; stops completely after death (moved 0.000); player components intact.
- Final clean play/stop after probe removal: 0 errors, 0 warnings.

### Next
M4.3 — Runner AI (fast approach).

## 2026-08-14
### Milestone
M4.3 — Runner AI (part of M4 — Enemy System)

### Completed
- RunnerAI (Assets/Scripts/Enemy/RunnerAI.cs): faster pursuer. Same pattern as ChaserAI — FixedUpdate + Rigidbody2D.MovePosition toward the player's current world position at the configured MoveSpeed; reuses EnemyController refs (no per-frame GetComponent/Find); stops when dead. No extra mechanics (no dash/turn/collision skill).
- RunnerData.asset (Assets/ScriptableObjects/Enemies/RunnerData.asset): moveSpeed 6 (clearly higher than Chaser 3.5; project docs give no explicit Runner value, so a simple fixed value was chosen and recorded here). Other values match base (maxHP 30, damage 10, range 1.5, cooldown 1).
- Runner.prefab (Assets/Prefabs/Enemies/Runner.prefab): based on EnemyBase + RunnerAI, EnemyStats.data = RunnerData.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary RunnerProbe, deleted): 20/20 PASS, 0 FAILURES — prefab load; HP 30/30; target resolve; RunnerData MoveSpeed 6; speed 6.00 over 50 fixed physics frames (both segments, exact); moves toward player; re-targets after player teleports; chases across teleports; physics holds runner near player (dist ~1.07, expected contact behavior); death stops movement completely (0.000); Chaser regression intact (3.5 exact); player components intact.
- Note: fixed-physics-frame counting (WaitForFixedUpdate) was used instead of WaitForSeconds because background-editor frame timing is unreliable; also, when a runner reaches the player, the two colliders contact and physics holds it near the player — expected M4 behavior (contact damage arrives with M5 Combat).
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M4.4 — Shooter AI (ranged attack).

## 2026-08-14
### Milestone
M4.4 — Shooter AI (part of M4 — Enemy System)

### Completed
- ShooterAI (Assets/Scripts/Enemy/ShooterAI.cs): ranged attacker. Movement: approaches only when farther than AttackRange, stops once inside it (no kiting/wall-hugging). Attack: when in range, player alive and off cooldown, fires a minimal Projectile at the player's current position; cooldown via Time.time (stable, testable clock) from AttackCooldown. Reuses EnemyController refs; stops moving/attacking when dead.
- Projectile (Assets/Scripts/Enemy/Projectile.cs): MINIMAL M4.4 projectile to prove the ranged attack — fixed direction+speed (8), lifetime (3s), OnTriggerEnter2D applies damage to PlayerHealth then self-destroys. Flagged explicitly as a temporary minimal path: M5 Combat System will replace it with the unified pipeline (no pool, no generic weapon framework).
- ShooterData.asset: moveSpeed 2.5 (slow, ranged), attackRange 6 (long), attackCooldown 1.5, damage 8, maxHP 25 (squishy). No explicit docs values → simple fixed values chosen for type differentiation, recorded here.
- ShooterProjectile.prefab (kinematic RB, trigger collider 0.2, orange placeholder) + Shooter.prefab (EnemyBase + ShooterAI, data = ShooterData, projectilePrefab = ShooterProjectile).
- Damage path: Projectile hits player and calls PlayerHealth.TakeDamage — the only existing damage API (no combat system yet). Recorded as M4.4-minimal; M5 will own damage rules.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary ShooterProbe, deleted): 27/27 PASS, 0 FAILURES — base stats (HP 25/25, speed 2.5, range 6, cd 1.5, dmg 8); approach speed 2.50 exact; stops at range edge (drift 0.00, dist 6.00); projectilePrefab resolved at runtime; fire→flight→hit→damage proven by player HP dropping (84 → 68 → 60); cooldown limits damage in short window; fires again after cooldown; no attack out of range (HP unchanged); re-approaches when player far; death stops movement and attacks (HP unchanged); Chaser (3.5) and Runner (6) regression intact; no projectile residue after lifetime.
- Note: scene sampling misses a projectile because it lives ~0.7s before hitting the player — player HP delta is the reliable end-to-end signal (test-methodology lesson).
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M4.5 — Tank AI (slow, high HP).

## 2026-08-14
### Milestone
M4.5 — Tank AI (part of M4 — Enemy System)

### Completed
- TankAI (Assets/Scripts/Enemy/TankAI.cs): slow, high-HP pursuer. Same pursuit pattern as ChaserAI — FixedUpdate + Rigidbody2D.MovePosition toward the player at the configured MoveSpeed; reuses EnemyController refs; stops when dead. No special attack / dash / knockback / area skills — type identity comes purely from data.
- TankData.asset: moveSpeed 2 (clearly below Chaser 3.5; docs give no explicit value — simple fixed value chosen for type differentiation, recorded here), maxHP 120 (clearly above the other enemies' 30), damage 10 / range 1.5 / cooldown 1 (base).
- Tank.prefab (Assets/Prefabs/Enemies/Tank.prefab): EnemyBase + TankAI, EnemyStats.data = TankData.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary TankProbe, deleted): 23/23 PASS, 0 FAILURES — HP 120/120; target resolve; MoveSpeed 2 / MaxHP 120 from data; Tank speed 2 < Chaser 3.5; Tank MaxHP 120 > others max 30; pursuit speed 2.00 exact over 50 fixed physics frames (stable); moves toward player; re-targets on player move (delta y) and after the player crosses the tank; TakeDamage(30) -> 90; clamp to 0; IsDead; EnemyDied fired once with enemy reference; stops moving after death; Chaser/Runner/Shooter prefabs intact.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M4.6 — minimal enemy spawn entry (wave logic stays in M8).

## 2026-08-14
### Milestone
M4.6 — Minimal Spawn Entry (M4 — Enemy System COMPLETE)

### Completed
- EnemySpawner (Assets/Scripts/Enemy/EnemySpawner.cs): minimal spawn entry — holds a List<GameObject> of enemy prefabs and a spawnDistance; Start() spawns one instance of each prefab at fixed cardinal offsets (left/right/up/down, 10 units) around the player. Only instantiates; enemies run their own AI via EnemyController. No wave logic, no timers, no loops, no object pool (M8 owns waves, M7 owns pooling).
- SC_Main: added EnemySpawner GameObject wired to Chaser/Runner/Shooter/Tank prefabs (spawnDistance 10).

### Verification
- Compilation: 0 errors.
- In-play probe (temporary SpawnProbe, deleted): 22/22 PASS, 0 FAILURES — EnemySpawner present + component; Player present; all 4 enemy prefabs spawned (Chaser/Runner/Shooter/Tank); total count == 4; each spawned at ~9.3-9.8 units from the player (not overlapping); base components + PlayerHealth target resolved on all; Chaser/Runner/Tank AI moving; Shooter active (fires when in range); no duplicate spawning after additional time.
- Manual observation (in-play scene query): all four enemies visible as Chaser(Clone)/Runner(Clone)/Shooter(Clone)/Tank(Clone), converging on the player; Shooter spawned projectiles (ShooterProjectile(Clone) present) — confirms the "run SC_Main and see enemies" goal.
- Final clean play/stop twice: 0 errors, 0 warnings (enemies spawned correctly on both runs).

### Next
M5 — Combat System (damage, death, pickups). Wave logic remains in M8.

## 2026-08-14
### Milestone
M5.1 — Combat Base Framework (part of M5 — Combat System)

### Completed
- Assets/Scripts/Combat/IDamageable.cs: minimal damage-target abstraction (IsDead + TakeDamage); implemented by PlayerHealth and EnemyHealth (no responsibility changes — they keep owning HP/death state).
- Assets/Scripts/Combat/DamageRequest.cs: struct — Source (may be null), Target (must expose IDamageable), Damage.
- Assets/Scripts/Combat/DamageResult.cs: struct — Applied / Damage / TargetDead (minimal outcome).
- Assets/Scripts/Combat/CombatSystem.cs: static unified entry — ApplyDamage validates target/damage, rejects dead targets, routes to IDamageable.TakeDamage, publishes DamageApplied, returns DamageResult. No Player/Enemy branches; no per-frame allocations.
- Assets/Scripts/Core/GameEvents.cs: added DamageApplied (Source/Target/Damage). Kill attribution / XP / Gold events still deferred.
- PlayerHealth / EnemyHealth: now implement IDamageable (signatures unchanged).
- Projectile (migration): no longer calls PlayerHealth.TakeDamage directly — on contact routes DamageRequest through CombatSystem to any IDamageable; carries an optional source (ShooterAI now passes its gameObject). M4.4 temporary path fully replaced; temporary comments updated.
- ShooterAI: Init(direction, damage, source) call updated (movement/attack logic untouched).

### Why this cross-cutting change (per PROJECT_RULES.md 5.6 / ARCHITECTURE.md)
- M4.4's Projectile → PlayerHealth.TakeDamage() was a minimal temporary path (documented then). M5.1 replaces it with a unified damage entry so Player/Enemy/Projectile/Weapon share one pipeline and health classes stay decoupled from "what attacked them".

### Verification
- Compilation: 0 errors.
- In-play probe (temporary CombatProbe, deleted): 23/23 PASS, 0 FAILURES — Spawner regression (4 enemies moving); player damaged via combat (HP 100→90, DamageApplied once); enemy damaged via combat with source (HP 30→20); enemy death via combat (EnemyDied once); damage to dead enemy rejected; projectile fly→hit→combat→damage (player HP 90→82) →destroy; projectile DamageApplied (probe source); player death via combat (PlayerDied once); damage to dead player rejected.
- Decoupling check: Projectile.cs has zero PlayerHealth code references.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Known behavior (recorded, not a defect)
- Projectile hits any IDamageable, including other enemies (no friendly-fire filter). This is the minimal M5.1 unified-target design; faction/team filtering is left to a later combat refinement.

### Next
M5.2 (per TASKS.md split).

## 2026-08-14
### Milestone
M5.2 — Enemy Death & Kill Attribution (part of M5 — Combat System)

### Completed
- Assets/Scripts/Core/GameEvents.cs: added EnemyKilled (Enemy + Killer). Semantics: EnemyDied = an enemy died; EnemyKilled = that death is attributed to a Source. XP/Gold/rewards still deferred.
- Assets/Scripts/Combat/CombatSystem.cs: after routing damage, if the target died this hit AND the request carried a valid Source AND the target is an EnemyHealth, publishes EnemyKilled(target, source) exactly once. Null-source lethal deaths publish EnemyDied only. (Cross-system dependency Combat → Enemy added deliberately for kill attribution; documented here per PROJECT_RULES 5.6 / ARCHITECTURE.)
- Assets/Scripts/Enemy/EnemyController.cs: now also the death/despawn layer — subscribes EnemyDied in Awake (unsubscribes in OnDestroy) and destroys its own GameObject on its own death. Plain Destroy; Object Pool is M7.

### Event order (verified)
CombatSystem.ApplyDamage → Target.TakeDamage (EnemyHealth) → EnemyDied published → CombatSystem publishes EnemyKilled (if source valid) → EnemyController destroys the GameObject (frame-end).

### Verification
- Compilation: 0 errors.
- In-play probe (temporary KillProbe, deleted): 28/28 PASS, 0 FAILURES — non-lethal: HP 20, DamageApplied once, no EnemyKilled; lethal: IsDead + TargetDead, EnemyDied once, EnemyKilled once, Enemy reference correct, Killer == source; dead enemy destroyed (cleanup); dead-target damage rejected with no duplicate death/kill events; null-source lethal: EnemyDied only (no EnemyKilled), still cleaned up; projectile → combat → EnemyKilled chain (EnemyBase static target, source = probe) with correct attribution and cleanup; PlayerHealth/DamageApplied/PlayerDied regressions; Spawner + 4 enemy AI regressions.
- Note: the first run's 3 projectile-chain FAILs were a test-methodology issue — a live Chaser target moves (AI pursues player) and dodges a straight shot; switching to the AI-free EnemyBase target made the hit deterministic. Not an implementation bug.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M5.3 (per TASKS.md split).

## 2026-08-14
### Milestone
M5.3 — Pickup System (part of M5 — Combat System)

### Completed
- Assets/Scripts/Player/PlayerProgress.cs: runtime XP/Gold resources (CurrentXP/CurrentGold, AddXP/AddGold; negative/zero amounts ignored). No level/threshold/upgrade/shop logic. Separate from PlayerStats (character attributes) by design.
- Assets/Scripts/Pickup/PickupType.cs: enum XP / Gold.
- Assets/Scripts/Pickup/PickupData.cs: ScriptableObject (Type + Amount), read-only at runtime.
- Assets/Scripts/Pickup/Pickup.cs: trigger collect — on player contact (TryGetComponent<PlayerProgress>) credits progress, publishes PickupCollected, destroys itself.
- Assets/Scripts/Pickup/PickupSystem.cs: subscribes EnemyKilled; on each killed normal enemy spawns 1 XP + 1 Gold pickup at the enemy's position immediately (before frame-end destroy). Deterministic MVP rule (XP 10 / Gold 5); no drop rates/rarity/weights.
- Assets/Scripts/Core/GameEvents.cs: added PickupCollected (Type/Amount/Collector). No level-up events.
- Assets: XPPickupData.asset (XP 10), GoldPickupData.asset (Gold 5), XPPickup.prefab + GoldPickup.prefab (sprite + trigger collider + Pickup), placeholder art; Player.prefab gains PlayerProgress; SC_Main gains PickupSystem object (wired to both prefabs).
- No level-up / upgrade / weapon / shop / wave / pool implemented.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PickupProbe, deleted): 25/25 PASS, 0 FAILURES — progress adds (relative deltas) + negative rejection; pickup prefab config (component, trigger, data); XP/Gold collection on contact with correct deltas + PickupCollected + destroy; EnemyKilled → PickupSystem spawned XP+Gold at the death position; DamageApplied/EnemyDied/EnemyKilled normal; Spawner 4 enemy types present.
- Manual play observation: scene enemies die (cross-fire), pickups drop at death positions, player auto-collects them (XP/Gold grow in real time).
- Note: initial probe asserts assumed XP/Gold start at 0, but by the time the probe reads them the scene cross-fire has already produced drops the player auto-collected — switched to relative-delta asserts. Expected composition behavior, not a defect.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M5.4+ per the official task split (to be confirmed by the next task prompt).

## 2026-08-14
### Milestone
M5.4 — Player Attack Path (part of M5 — Combat System)

### Completed
- Assets/Scripts/Player/PlayerAttack.cs: formal Player → Enemy attack entry. Attack(target) validates stats/target, builds a DamageRequest with this player as Source and PlayerStats.Damage as base damage, and routes it through CombatSystem.ApplyDamage, returning the DamageResult. No auto-attack loop, no weapon/projectile/targeting framework (M6 owns weapon cycles and target selection).
- Assets/Prefabs/Player.prefab: Player prefab gains PlayerAttack (scene instance synced).

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PlayerCombatProbe, deleted): 23/23 PASS, 0 FAILURES — component present; PlayerStats.Damage = 10 readable; PlayerHealth/PlayerProgress intact; static EnemyBase target; non-lethal attack (30 → 20, DamageApplied once, no EnemyKilled); repeated player attacks kill (EnemyKilled once, EnemyDied, Enemy reference correct, Killer == Player, attributed to player); dead enemy destroyed; attack on dead enemy rejected with no duplicate events; PickupSystem dropped XP+Gold at the killed enemy's position (0 → 2); Spawner all 4 enemy types present.
- Note: one probe assert initially counted pickups after the kill (PickupSystem spawns synchronously), so before/after were equal — moved the baseline capture before the kill. Assertion timing, not an implementation bug.
- Manual play: full Player → CombatSystem → Enemy → EnemyKilled(Killer=Player) → Pickup chain observed via the in-play probe run.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M5 completion per the official task split; weapons in M6.
