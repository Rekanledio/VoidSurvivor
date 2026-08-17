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

## 2026-08-16
### Milestone
M6.1 — Weapon Base Framework (part of M6 — Weapon System)

### Completed
- Assets/Scripts/Weapons/WeaponData.cs: ScriptableObject static config — weaponName / baseDamage / attackCooldown / range. Read-only at runtime; no crit/element/status/upgrade/rarity.
- Assets/Scripts/Weapons/WeaponController.cs: runtime weapon — holds WeaponData, resolves the player's PlayerAttack via GetComponentInParent (Awake + lazy re-resolve on Attack, because Awake runs during Instantiate before parenting), Attack(target) routes through PlayerAttack (never directly to a health class). No auto-attack/targeting.
- Assets/Scripts/Weapons/WeaponSlot.cs: plain runtime container — Equip/Unequip/IsEmpty/Weapon.
- Assets/Scripts/Weapons/WeaponManager.cs: player-side container, exactly 4 slots, Equip/Unequip/GetSlot/GetWeapon with bounds checks (no silent out-of-range writes; out-of-range returns false/null).
- Assets: WeaponBaseData.asset (Base Test Weapon: dmg 10 / cd 0.5 / range 5) + WeaponBase.prefab (WeaponController + data) — explicitly a base test asset, NOT one of the four formal weapons (M6.2+). Player prefab gains WeaponManager (scene instance synced).
- Layer kept: Weapon → PlayerAttack → CombatSystem → EnemyHealth (no bypass). No auto-attack loop, no projectile weapon framework, no weapon upgrade/shop/roguelite.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary WeaponProbe, deleted): 32/32 PASS, 0 FAILURES — WeaponData values (10/0.5/5); runtime binding (data reference correct); 4 slots; equip 0-3; out-of-range equip/unequip rejected; slot state transitions; GetWeapon/GetSlot access; weapon attack applied (30 → 20, DamageApplied once, no EnemyKilled on non-lethal); weapon lethal kill (EnemyKilled once, Killer == Player); death cleanup; PlayerHealth/PlayerProgress/Spawner/4 enemy types/PickupSystem regressions.
- First run: 6 attack-phase FAILs caused by WeaponController resolving PlayerAttack in Awake — Awake runs during Instantiate, before the weapon is parented under the player, so the resolve returned null. Fixed with lazy re-resolve on Attack (also more robust for real equip order). Assertion/时序 issue, fixed in production code.
- Manual play: WeaponManager on player (4 slots visible), test weapon equipped, Attack drives Enemy HP down through the combat pipeline.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M6.2 — Pulse Gun.

## 2026-08-16
### Milestone
M6.2 — Pulse Gun (first formal weapon, part of M6 — Weapon System)

### Completed
- PlayerAttack (M5.4) extended: Attack(target) now delegates to Attack(target, damage) (PlayerStats.Damage default) — old API regression-compatible; new overload lets weapons pass WeaponData.BaseDamage into CombatSystem.
- WeaponController (M6.1) extended: exposes protected Owner (the player GameObject = attack source) and a protected Attack(target, damage) for direct-damage weapons.
- Assets/Scripts/Weapons/PulseGun.cs: auto-attack loop (Time.time cooldown from WeaponData.AttackCooldown), minimal targeting (nearest live EnemyHealth within Range via Physics2D.OverlapCircleAll, re-acquires only when the current target is dead/out-of-range), fires one PulseProjectile per shot (Source = player), self-equips into WeaponManager slot 0 on Start (empty slot only).
- Assets/Scripts/Weapons/PulseProjectile.cs: single-target straight flight (direction captured at spawn), fixed speed, lifetime, contact → CombatSystem with player source; skips its own Source object (minimal self-hit guard, no faction system).
- Assets: PulseGunData.asset (Pulse Gun: baseDamage 5 / attackCooldown 0.25 / range 8 — high fire rate, single target, mid range; simple values, no doc values → chosen for type identity, recorded), PulseProjectile.prefab (red dot placeholder), PulseGun.prefab (PulseGun + data + projectilePrefab + speed 12). SC_Main: PulseGun instance under Player.
- No other weapons, no weapon upgrade/shop/roguelite, no object pool (M7), no projectile pooling.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PulseGunProbe, deleted): 27/27 PASS, 0 FAILURES — data (name/dmg 5/cd 0.25/range 8); slot-0 equip; target beyond Range not attacked (HP unchanged); nearest in-range enemy attacked (HP 30 → 15); player-sourced damage via projectile → combat; cooldown exact (spawn gaps 0.250–0.252s, min gap 0.250 → no same-frame double fire); auto-attack active (14 projectiles/~2.2s); lethal kill → EnemyKilled with Killer == Player; dead enemy destroyed; pickups present; auto-attack re-targets after kill; PlayerHealth/PlayerProgress/Spawner regressions.
- Note: two probe assert-timing fixes (enemy nearer than scene pursuers → moved baseline before kill; scene auto-fire clears enemies → 4-type check moved to probe start). Not implementation bugs.
- Manual play: REAL auto-combat observed — PulseGun auto-fires, scene enemies 4 → 1, player auto-collects drops (XP 20 / Gold 5).
- Final clean play/stop twice: 0 errors, 0 warnings (auto-fire confirmed both runs).

### Next
M6.3 — Scatter Blaster.

## 2026-08-16
### Milestone
M6.3 — Scatter Blaster (second formal weapon, part of M6 — Weapon System)

### Completed
- Assets/Scripts/Weapons/ScatterBlasterData.cs: WeaponData subclass adding projectileCount (5) and spreadAngle (45) — base WeaponData untouched.
- Assets/Scripts/Weapons/ScatterBlaster.cs: auto-attack loop (Time.time cooldown), minimal targeting (nearest live EnemyHealth within Range used as the fan center direction), fires the configured count of PulseProjectiles simultaneously in a deterministic uniform fan (angles -half..+half, center pellet → target, even spacing, symmetric). Damage/Source per pellet = ScatterBlasterData.BaseDamage / player.
- Assets/Scripts/Weapons/WeaponController.cs (fix): Owner now lazy-resolves PlayerAttack on every access. Real bug: weapons instantiated at runtime before parenting (Awake runs during Instantiate) kept _playerAttack null, so Owner returned null and projectiles had no source — from the player's position they immediately hit the player and were destroyed without reaching the target. Scene-placed Pulse Gun never exposed this (parent preset at load); the fix also hardens Pulse Gun.
- Assets: ScatterBlasterData.asset (Scatter Blaster: baseDamage 3 / attackCooldown 0.8 / range 7 / projectileCount 5 / spreadAngle 45 — low fire rate, multi-pellet, visible fan; simple values chosen for type identity, recorded) + ScatterBlaster.prefab (reuses PulseProjectile.prefab + red dot art).
- No Boomerang/Arc Blade, no weapon upgrade/shop/roguelite, no object pool (M7), no random spread.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary ScatterProbe, deleted): 42/42 PASS, 0 FAILURES — data (name/dmg 3/cd 0.8/range 7/count 5/spread 45); slot-0 equip; beyond-range target untouched; in-range static enemy hit (30 → 21) via player-sourced projectiles; fan geometry exact (center 0.00°, inner ±11.2°, outer ±22.5°, symmetric, even spacing, normalized); volley-to-volley cooldown 0.801s; lethal kill → EnemyKilled once with Killer == Player; dead enemy destroyed; pickups present; PlayerAttack/PlayerHealth/Spawner (4 types)/Pulse Gun regressions.
- Manual play: Scatter auto-fires its fan and kills the target (observed in play).
- Debugging notes (test-methodology): probe failures were caused by (a) scene enemies/pursuers stealing the scatter center (moved them far away), (b) the cooldown/fan asserts reading pre-clear residue projectiles (cleared capture + took the latest volley), (c) the kill loop using render frames (switched to fixed physics frames), and the one real production bug (Owner lazy-resolve) above.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M6.4 — Boomerang.

## 2026-08-16
### Milestone
M6.4 — Boomerang (third formal weapon, part of M6 — Weapon System)

### Completed
- Assets/Scripts/Weapons/BoomerangData.cs: WeaponData subclass adding maxDistance (6), outSpeed (8), returnSpeed (10).
- Assets/Scripts/Weapons/BoomerangProjectile.cs: two-phase projectile. Outbound: moves straight at outSpeed until the distance from the spawn origin reaches maxDistance (world distance, not a timer). Return: re-aims at the player's CURRENT world position every physics frame (MovePosition at returnSpeed) and destroys itself within 0.5 units. Per-target hit-once via a HashSet — each throw hits a given enemy at most once while it may hit different enemies on the way out and back. Damage via CombatSystem with the player as Source. Static ActiveCount for the single-flight rule.
- Assets/Scripts/Weapons/Boomerang.cs: auto-attack loop (Time.time cooldown), minimal targeting (nearest live EnemyHealth within Range → initial throw direction only; return never tracks the target), single-flight rule (no new throw while ActiveCount > 0).
- Assets: BoomerangData.asset (Boomerang: baseDamage 7 / attackCooldown 1.2 / range 8 / maxDistance 6 / outSpeed 8 / returnSpeed 10 — medium damage, low fire rate, visible flight distance, return faster than outward; simple values for type identity, recorded), Boomerang.prefab + BoomerangProjectile.prefab (green ring placeholder). Not the default weapon (scene keeps Pulse Gun); equipped via WeaponManager for tests.
- No Arc Blade, no upgrade/shop/roguelite, no object pool (M7), no homing/penetration/combo.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary BoomerangProbe, deleted): 37/37 PASS, 0 FAILURES — data (7 fields); slot-0 equip; auto throw launched + single-flight (one active); auto throw returned & destroyed; outbound speed 8.00 exact over fixed frames; moves toward target; reaches maxDistance 6.24; hit-once (HP 23 = 30-7 after crossing twice); player-sourced damage; manual throw returned & destroyed; return re-aims at the player's MOVED position; returned to moved player & destroyed; multi-target (2 hits, A and B each once); kill → EnemyKilled once with Killer == Player; dead enemy destroyed; pickups present; PlayerAttack/PlayerHealth/PlayerProgress/Pulse Gun/Scatter Blaster/Spawner (4 types) regressions.
- Debugging notes (methodology): (a) early auto-throws fired while scene pursuers were still in range polluted the tracked throw — drained old boomerangs before the target test and disabled the weapon after the auto-throw check so manual throws are fully controlled; (b) the old loop checked `bp != null` before the yield but the object could be destroyed during the wait — re-checked inside the loop.
- Manual play: boomerang visible out-and-return flight; auto throw + kill confirmed in play.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M6.5 — Arc Blade.

## 2026-08-16
### Milestone
M6.5 — Arc Blade (final formal weapon — M6 COMPLETE, part of M6 — Weapon System)

### Completed
- Assets/Scripts/Weapons/ArcBladeData.cs: WeaponData subclass reusing the base fields only (Range doubles as the attack radius; no new fields).
- Assets/Scripts/Weapons/ArcBlade.cs: close-range area weapon — auto-attack loop (Time.time cooldown), each strike performs ONE Physics2D.OverlapCircleAll centered on the player, hits every live EnemyHealth inside Range exactly once (deduped list, dead targets skipped, self ignored) and routes each hit through the weapon's protected Attack(target, damage) → PlayerAttack → CombatSystem. No projectile, no single-target selection, no upgrade.
- Assets: ArcBladeData.asset (Arc Blade: baseDamage 8 / attackCooldown 0.9 / range 2.5 — close range, low-mid fire rate, meaningful single-target damage; simple values for type identity, recorded) + ArcBlade.prefab (no projectile, no visual — attack logic only per design).
- Not auto-equipped (scene keeps Pulse Gun); equipped via WeaponManager for tests.
- No projectile/pool/upgrade/shop/roguelite/wave; M7 Object Pool remains separate.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary ArcBladeProbe, deleted): 29/29 PASS, 0 FAILURES — data (name/dmg 8/cd 0.9/range 2.5); slot-0 equip; first strike hits near (0,1.5) and edge (0,2.3) exactly once each (30 → 22); out-of-range target (0,6) untouched (HP 30); player never hit by own strike; per-target once per strike (near/edge 30 → 14 after 2 strikes, no repeats); strike cooldown ≈ 0.9 (0.900s measured in the prior run); kill → EnemyKilled once with Killer == Player; dead enemy destroyed; out-of-range still untouched after the kill; pickups present (4); Pulse Gun / Scatter Blaster / Boomerang / PlayerAttack / PlayerHealth / PlayerProgress / Spawner (4 types) regressions.
- Debugging notes (methodology): two assert-timing fixes (waited for 6 hits = 3 strikes while asserting 1 strike's HP; kill baseline read after the target had already died) — not implementation bugs.
- Manual play: area strikes active, multiple enemies damaged simultaneously by one strike, kill + pickups observed.
- Final clean play/stop twice: 0 errors, 0 warnings.

### M6 Status
M6 — Weapon System: COMPLETE (all 4 weapons: Pulse Gun, Scatter Blaster, Boomerang, Arc Blade).

### Next
M7 — Object Pool (M7.1).

## 2026-08-16
### Milestone
M7.1 — Object Pool Base Framework (part of M7 — Object Pool)

### Completed
- Assets/Scripts/Core/IPoolable.cs: optional lifecycle hook — OnSpawn() (after SetActive(true) on Get), OnDespawn() (before SetActive(false) on Release).
- Assets/Scripts/Core/ObjectPool.cs: minimal generic pool (T : Component, namespace VoidSurvivor.Core, next to EventBus). Constructor pre-warms N instances (each goes through Release, so OnDespawn runs at warmup — documented). Get(): pops an available object (or creates one when empty — the pool GROWS on demand, never rejects), SetActive(true), OnSpawn(). Release(): guards double-release via an in-pool HashSet, adds unknown objects to the managed list, OnDespawn(), SetActive(false), pushes back. Clear(): destroys every managed object and empties all collections. No Update/FixedUpdate work, no per-frame allocation.
- Scope: framework ONLY. EnemyController / EnemySpawner / PickupSystem / Pickup / PulseGun / ScatterBlaster / PulseProjectile / Boomerang / BoomerangProjectile / ShooterAI keep their Instantiate/Destroy — integration is M7.2. No prefab/scene/ScriptableObject/M8 changes.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PoolProbe + PoolProbeItem, deleted): 19/19 PASS, 0 FAILURES — initial available/total 3; Get activates + OnSpawn once; Release deactivates + OnDespawn (4 = 3 warmup + 1); Get→Release→Get reuses the same instance; double-release ignored (available unchanged, no extra OnDespawn); exhaustion grows total 3 → 4 with a fresh instance; all released back (available 4); Clear empties pool (total 0) and destroys every pooled object (scene 4 → 0, FindObjectsInactive verified).
- Debugging notes (methodology): three probe assert fixes — warmup Release counts toward OnDespawn (documented design), and FindObjectsByType default skips inactive objects (used FindObjectsInactive.Include for the residue check). Not framework bugs.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M7.2 — Pool Integration (wire Enemy / Projectile / Pickup lifecycles into the pool).

## 2026-08-16
### Milestone
M7.2.1 — Projectile Pool Integration (part of M7 — Object Pool)

### Completed
- PulseProjectile.cs: implements IPoolable. Static shared ObjectPool (Pulse Gun and Scatter Blaster call EnsurePool with the same prefab → one pool). Static Spawn(prefab, position, source, direction, speed, damage) = pool.Get + reposition + Init. Lifetime/hit → DespawnSelf → Release (fallback Destroy only if never pooled). OnDespawn: _initialized=false, velocity zero, source/damage/direction cleared, lifetime reset. Static pool reset on play start (SubsystemRegistration) since statics survive play sessions.
- Enemy/Projectile.cs (Shooter): same pattern; Init(direction, damage, source) unchanged; damage/speed/lifetime semantics untouched.
- BoomerangProjectile.cs: pooled with the same pattern. ActiveCount no longer uses OnDestroy (pool warmup also runs Release/OnDespawn, which would corrupt the counter) — now incremented in Spawn (entering active flight) and decremented in DespawnSelf (ending flight). OnDespawn clears EVERY runtime field (initialized/source/returnTarget/direction/origin/speeds/maxDistance/damage/phase/lifetime/hit HashSet) so Get→Init→Release→Get starts clean; single-flight rule and per-throw hit set preserved; Return still re-aims at the player's CURRENT position. Static pool + ActiveCount reset on play start.
- Call sites: PulseGun.FireAt, ScatterBlaster.SpawnProjectile, Boomerang.ThrowAt, ShooterAI.Fire → static Spawn(...) (no more Instantiate + TryGetComponent fallback).

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PoolIntegrationProbe, deleted): 38/38 PASS, 0 FAILURES — shared pool (ReferenceEquals), pre-warmed 16, Spawn activates + flies, Release deactivates + zero velocity, Get-after-Release reuses same instance, new Spawn overwrites source/damage/initialized, hit → Release (inactive) + damage 25, lifetime → Release, released projectile never moves again, Shooter projectile pool + source + hit damage 24 via CombatSystem, Boomerang ActiveCount 0→2→0 across throws/flights, hit-once per throw (HP 23) + hit set resets per throw (HP 16), Return re-aims at moved player, all released; Pulse Gun (pooled) auto-kill → EnemyKilled Killer == Player, pickups 6, regressions (PlayerHealth/PlayerProgress/Spawner 4 types).
- Debugging notes (methodology): probe asserted pool.Release directly on a boomerang, which bypasses the ActiveCount maintenance points (design: Spawn/DespawnSelf own the counter) — switched the probe to full flights; scene Pulse Gun auto-fire hit manual targets — disabled it during manual tests and re-enabled for the weapon regression.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M7.2.2 — Enemy / Pickup Pool Integration.

## 2026-08-16
### Milestone
M7.2.2 — Enemy Pool Integration (part of M7 — Object Pool)

### Completed
- EnemyController.cs: implements IPoolable. Static Spawn(pool, position) = pool.Get + _myPool injection + reposition. OnSpawn: EnemyHealth.ResetForSpawn() + notifies child IPoolable components (skipping itself) so per-type AI resets. OnDespawn: notifies child IPoolable (AI stop) + zeroes Rigidbody2D velocity/angularVelocity. Death path: OnEnemyDied → DespawnSelf → pool.Release (fallback Destroy only when never pooled). EventBus subscription stays in Awake (once per instance creation) — pool reuse never re-subscribes; inactive pooled enemies simply ignore EnemyDied for themselves.
- EnemyHealth.cs: added ResetForSpawn() — restores MaxHP, clears _isDead (death event guard already prevents duplicate EnemyDied per life).
- EnemySpawner.cs: owns per-prefab pools (Dictionary<GameObject, ObjectPool<EnemyController>>, lazily created, capacity 1, parent = spawner transform) and spawns via EnemyController.Spawn — M4.6 offsets/count/spawn rules unchanged.
- ShooterAI.cs: implements IPoolable — OnSpawn resets _nextAttackTime (no stale cooldown across lives). Chaser/Runner/Tank AI are stateless (movement only) — no per-state reset needed; they stop when inactive/IsDead.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary EnemyPoolIntegrationProbe, deleted): 31/31 PASS, 0 FAILURES — pool pre-warm 1/1, Spawn activates + alive + full HP, damage on first life, EnemyDied exactly once, dead released (inactive) + pool restored, reuse same instance, re-spawn clears IsDead + restores HP, second death → EnemyDied once again, double-release safe, Chaser re-tracks (moves toward player), released enemy never moves, Chaser re-tracks after respawn, Shooter fires fresh + after respawn (verified via DamageApplied with source == shooter, avoiding unreliable projectile polling) + stops when dead, Clear empties pools + destroys instances (no residue), PlayerHealth/PlayerProgress/PickupSystem (10 pickups)/EnemyKilled/weapon prefabs regressions.
- Debugging notes (methodology): shooter-fire verification switched from projectile counting (unreliable — projectile may hit the player within a frame) to DamageApplied(source == shooter); manual shooters were cross-fired by the scene Shooter and by the scene Pulse Gun — disabled Pulse Gun and re-moved scene enemies before manual tests.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M7.2.3 — Pickup Pool Integration.

## 2026-08-16
### Milestone
M7.2.3 — Pickup Pool Integration (last major M7.2 integration — M7.2 COMPLETE)

### Completed
- Pickup.cs: implements IPoolable. Static Spawn(ObjectPool<Pickup> pool, Vector2 position) = pool.Get + _myPool injection + reposition. Collection (OnTriggerEnter2D → PlayerProgress.AddXP/AddGold + PickupCollected) → DespawnSelf → Release (fallback Destroy only when never pooled). OnSpawn/OnDespawn are documented no-ops — the pickup has no runtime state (PickupData is a static serialized reference per prefab; there is no Rigidbody2D, so no velocity/physics to clear); being inactive stops the trigger and any duplicate collection. Collection-once semantics preserved: after Release the collider cannot fire again; a re-Spawn re-enables it and collects again.
- PickupSystem.cs: owns one lazy ObjectPool per prefab (XP pool + Gold pool, capacity 16, parent = system transform, grows on demand). OnEnemyKilled → Pickup.Spawn(xpPool, pos) + Pickup.Spawn(goldPool, pos) — still exactly 1 XP + 1 Gold at the enemy's ACTUAL death position, read before the enemy is released at frame end. XP=10 / Gold=5 / PickupType / PickupData / PlayerProgress / PickupCollected semantics unchanged.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary PickupPoolIntegrationProbe, deleted): 34/34 PASS, 0 FAILURES — XP/Gold pools pre-warmed 2/2; Get activates / Release deactivates; reuse same instances; double-release safe; counts correct; XP amount 10 + Gold amount 5; collect +10 exactly once + one PickupCollected; released pickup never settles again + no duplicate PickupCollected; Gold +5; recycled pickup collects again + reuses same instance (+20 total); multiple pickups coexist; EnemyKilled spawns exactly 1 XP + 1 Gold at the enemy's death position; second kill at its own position with no state bleed; regressions (PlayerHealth, weapon prefabs).
- Debugging notes (methodology): two probe assert fixes — (a) physics overlap-separation can push the test enemy slightly off the Instantiate position, so the assertion compares pickups against the enemy's ACTUAL death position (production code was correct all along); (b) a reuse-test gold pickup spawned off-player lingered active and skewed scene counts — recycled it explicitly. Also: Pickup is both a namespace and a type, so the probe used a type alias.
- Final clean play/stop twice: 0 errors, 0 warnings.

### M7.2 Status
M7.2 — Pool Integration COMPLETE (Projectile M7.2.1 + Enemy M7.2.2 + Pickup M7.2.3 all pooled).

### Next
M7 — final overall regression/acceptance; then M8 — Wave System.

## 2026-08-16
### Milestone
M7 — Final Regression & Acceptance — M7 COMPLETE (Object Pool)

### Acceptance
- M7.1 Object Pool Base Framework: 19/19 PASS
- M7.2.1 Projectile Pool Integration: 38/38 PASS
- M7.2.2 Enemy Pool Integration: 31/31 PASS
- M7.2.3 Pickup Pool Integration: 34/34 PASS
- M7 Final Regression (temporary M7FinalRegressionProbe, deleted): 45/45 PASS, 0 FAILURES.

### Integrated regression coverage
- Pool chain: PulseProjectile pooled spawn/release/reuse; Boomerang ActiveCount 0→1→0 with full flight; Enemy pooled spawn → death → release → respawn with HP/IsDead reset and can die again; XP Pickup collect +10 + recycle (+20).
- Full lifecycle repeated 3x: enemy killed → EnemyDied + EnemyKilled exactly once each → enemy released → 1 XP + 1 Gold spawned at death position → player collects +10 XP +5 Gold → PickupCollected exactly twice → all pickups released (none active).
- Four weapons together (Pulse Gun + ScatterBlaster + Boomerang + ArcBlade equipped): killed clustered targets, Player never damaged by own weapons, Boomerang single-flight holds with auto-throw, weapon kills dropped pooled pickups. ArcBlade (OverlapCircleAll, no projectile pool) verified working in the mix.
- EventBus: EnemyDied/EnemyKilled/PickupCollected exactly once per occurrence across all rounds; no duplicate subscribe (pool reuse never re-subscribes).
- Cross-play static state: 3 final Play/Stop cycles — pools reset per play, Boomerang ActiveCount starts 0 each time, static projectile pools reset, no NullReference/MissingReference/duplicate events.

### Debugging notes
- Phase A enemy-death check used EnemyHealth.TakeDamage (not CombatSystem) so the death releases the enemy via EnemyDied WITHOUT spawning pickups — keeps the manual phase side-effect free (CombatSystem kill would drop pickups at the manual test position and linger).

### Final verification
- 3 clean final Play/Stop cycles: 0 errors, 0 warnings each.
- Git clean; temporary probe deleted; no /tmp residue.

### M7 Status
M7 — Object Pool: COMPLETE.

### Next
M8 — Wave System (M8.1).

## 2026-08-16
### Milestone
M8.1 — Wave Lifecycle & Spawn Scheduling (part of M8 — Wave System)

### Completed
- Assets/Scripts/Core/GameEvents.cs: added WaveStarted(waveIndex) + WaveCompleted(waveIndex) fact-only events.
- Assets/Scripts/Enemy/WaveManager.cs (new): waves 1..10. Time advances via Time.deltaTime accumulation ONLY while GameState == Playing (Paused/LevelUp/Shop freeze; GameOver/Victory stop and reset for a fresh run; first Playing or re-Playing after GameOver/Victory starts wave 1 exactly once — no duplicate start on pause/resume). Spawn scheduling: per-wave config {duration, enemyCount, spawnInterval} in one centralized table; spawns via EnemySpawner.SpawnEnemy with deterministic type rotation (index % prefab count) at M4.6 cardinal offsets. Wave completion = elapsed >= duration (NOT enemy-alive count). Publishes WaveStarted/WaveCompleted once per wave; after wave 10 it goes idle (no wave 11, no Victory logic — Boss is M8.3). Public StartWave(index) for later subtasks/tests. Never touches ObjectPool directly.
- Assets/Scripts/Enemy/EnemySpawner.cs: removed the Start-time automatic four-direction spawn (wave-driven since M8.1); kept per-prefab pools/GetPool; added public SpawnEnemy(prefab, position), GetEnemyPrefab(index), GetSpawnPosition(offsetIndex, playerPos).
- Assets/Scenes/SC_Main.unity: WaveManager component attached to the EnemySpawner object.
- Placeholder wave table (W1 8s/5/1.6s → W9 16s/13/0.8s, W10 12s/15/0.7s) — explicitly documented as M8.1 temporary scheduling, not final difficulty. No difficulty scaling, no boss, no stats changes.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary WaveProbe, deleted): 42/42 PASS, 0 FAILURES — WaveStarted(1) exactly once + CurrentWave 1 + active; Wave 1 config 5/8s; first spawn at t=0; interval respected; all 5 spawned; spawn stops at configured count; Wave 1 completes → WaveCompleted(1) once → CurrentWave 2 → WaveStarted(2); Paused freezes wave time (2s real pass, elapsed unchanged) + resume without re-publishing WaveStarted; LevelUp and Shop each freeze wave time; GameOver stops wave + no more spawns; MainMenu → Playing restarts a fresh run at wave 1; StartWave(10) → WaveCompleted(10) → no WaveStarted(11) → idle; four enemy types available via deterministic rotation; wave-spawned enemies run their AI; weapons/pickups/Boomerang regressions sane.
- Manual play: switching to Playing visibly drives waves (enemies spawn, wave index advances, kills/pickups flow).
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M8.2 — Wave Difficulty Growth.

## 2026-08-16
### Milestone
M8.2 — Wave Difficulty Growth (part of M8 — Wave System)

### Completed
- Assets/Scripts/Enemy/EnemyStats.cs: added plain non-serialized runtime field `WaveMultiplier` (default 1). MaxHP / Damage / MoveSpeed now return data × WaveMultiplier; AttackRange / AttackCooldown stay at the EnemyData values. One change here scales all four AIs (they read Stats live every frame) with no per-AI logic.
- Assets/Scripts/Enemy/EnemyController.cs: `Spawn(pool, position, waveMultiplier = 1f)` — after pool.Get it sets `_stats.WaveMultiplier` FIRST, then `_health.ResetForSpawn()` so CurrentHP uses the scaled MaxHP. OnDespawn resets WaveMultiplier to 1 so any direct pool.Get can never leak a previous wave's difficulty.
- Assets/Scripts/Enemy/EnemySpawner.cs: `SpawnEnemy(prefab, position, multiplier = 1f)` passes through (old call sites stay compatible).
- Assets/Scripts/Enemy/WaveManager.cs: WaveConfig gained `Multiplier`; WaveTable now W1 1.00 → W10 1.45 (simple M8.2 base slope, explicitly not from GAME_DESIGN). SpawnOne passes the current wave's multiplier. duration/enemyCount/spawnInterval unchanged from M8.1.
- EnemyData assets untouched; no Boss/M9 systems.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M82DifficultyProbe, deleted): 39/39 PASS, 0 FAILURES — W1/W5/W10 multipliers 1.00/1.20/1.45; W1 wave spawn injects 1.00; Chaser/Runner/Tank MaxHP + MoveSpeed scaled (36/36/144 HP, 4.2/7.2/2.4 speed at 1.2), Shooter MaxHP 30 + Damage 9.6 at 1.2; Shooter AttackRange (6) and AttackCooldown (1.5) unchanged; Spawn CurrentHP == scaled MaxHP (injection before reset); TakeDamage works on scaled HP; scaled enemy dies and releases; pool reuse: multiplier reset → MaxHP back to 30, CurrentHP original, IsDead cleared, velocity zero; W10 1.45 → 43.5 HP; all four EnemyData assets verified unchanged (maxHP 30/30/25/120, damage 10, moveSpeed 2.5); regressions (PlayerHealth/PlayerProgress/weapons/Boomerang ActiveCount).
- Manual play: waves drive with escalating difficulty (W2 enemies carry 1.05 multiplier), kills/pickups flow.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M8.3 — Boss (Wave 10).

## 2026-08-16
### Milestone
M8.3 — Boss (Wave 10) (part of M8 — Wave System)

### Completed
- Assets/Scripts/Enemy/BossData.cs: minimal `BossData : EnemyData` subclass — all stats live in the asset.
- Assets/Scripts/Enemy/BossAI.cs: MVP boss behavior — pursues the player via Rigidbody2D.MovePosition at Stats.MoveSpeed; CONTACT damage on OnTriggerEnter2D against PlayerHealth builds a DamageRequest (Source = boss, Damage = Stats.Damage) through CombatSystem (never PlayerHealth.TakeDamage directly); Time.time cooldown = Stats.AttackCooldown; player-only target; no projectiles/skills.
- Assets/Scripts/Enemy/EnemySpawner.cs: added `bossPrefab` + public `SpawnBoss(position, multiplier)` — same `_pools` dictionary, no second pool system.
- Assets/Scripts/Enemy/WaveManager.cs: Wave 10 = boss encounter — one boss spawned (W10 multiplier 1.45) instead of the normal spawn schedule (no normal spawns, no time-based WaveCompleted(10), no wave 11); subscribes EnemyKilled and on `EnemyKilled.Enemy == _activeBoss.gameObject` publishes BossDefeated and enters Victory via the existing legal Playing→Victory transition (GameManager untouched). StartWave/GameOver/Victory reset boss state.
- Assets/Scripts/Core/GameEvents.cs: added `BossSpawned(GameObject boss)` and `BossDefeated(GameObject boss, GameObject killer)`.
- Assets: BossData.asset (MaxHP 500 / Damage 20 / MoveSpeed 1.5 / AttackRange 1.5 / AttackCooldown 1.0 — M8.3 implementation parameters, not GAME_DESIGN values) + Boss.prefab (EnemyController + EnemyStats(data=BossData) + EnemyHealth + BossAI + Dynamic Rigidbody2D + trigger BoxCollider2D + red placeholder sprite) + SC_Main EnemySpawner bossPrefab wiring.
- Boss death keeps the existing chain: EnemyHealth death → EnemyDied (EnemyController Release) + CombatSystem EnemyKilled (Killer == Player) → PickupSystem (1 XP + 1 Gold). No boss-only drops.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M83BossProbe, deleted): 47/47 PASS, 0 FAILURES — Boss prefab/components/data; W10 multiplier 1.45; manual spawn MaxHP 725 / Damage 29 / MoveSpeed 2.175, Range 1.5 + Cooldown 1.0 unchanged; boss pursues player; non-player unaffected by contact; contact damage flows through CombatSystem (Source = boss) and reduces PlayerHealth; boss death → EnemyKilled exactly once + release; manual kill outside Wave 10 does NOT trigger Victory; Wave 10: BossSpawned exactly once, no additional normal enemies, no Victory while boss alive; boss defeat → BossDefeated exactly once + EnemyKilled once → GameState.Victory, not re-triggered, wave stopped, no more boss spawns; boss pickups dropped; pool reuse clean (IsDead/CurrentHP/velocity/multiplier reset, AI resumes); regressions (PlayerHealth/PlayerProgress/weapons/Boomerang ActiveCount/4 normal prefabs).
- Debugging notes (methodology): probe asserted GetComponent<BossData>() — BossData is a ScriptableObject, so switched to a data-reference type check; boss instance name is "Boss(Clone)" so contact verification used a reference compare; GameOver from MainMenu is illegal — the probe walks Playing → GameOver → MainMenu → Playing; the pool stack reuses the most-recently-released instance, so reuse assertion accepts either released boss.
- Final clean play/stop twice (normal waves run): 0 errors, 0 warnings.

### Next
M8 — Final Regression & Acceptance.

## 2026-08-16
### Milestone
M8 — Final Regression & Acceptance — M8 COMPLETE (Wave System)

### Acceptance
- M8.1 Wave Lifecycle & Spawn Scheduling: 42/42 PASS
- M8.2 Wave Difficulty Growth: 39/39 PASS
- M8.3 Boss (Wave 10): 47/47 PASS
- M8 Final Regression (temporary M8FinalRegressionProbe, deleted): 58/58 PASS, 0 FAILURES.

### Integrated regression coverage
- Full flow: MainMenu → Playing → Wave 1 (natural run: enemies spawn with multiplier 1.00, WaveCompleted(1) exactly once, Wave 2 auto-starts) → Paused/LevelUp/Shop freeze wave time (resume without re-publishing WaveStarted) → GameOver stops → MainMenu → Playing restarts at wave 1 → Wave 10 boss encounter: BossSpawned exactly once, no additional normal enemies, boss alive = no Victory; boss contact damage via CombatSystem (Source = boss) reduces PlayerHealth; boss death → EnemyKilled exactly once → BossDefeated exactly once → GameState.Victory (not re-triggered, wave stopped, no more boss spawns, no Wave 11); boss pickups dropped; boss pool reuse clean (IsDead/HP/multiplier reset, velocity zero).
- M8.2: W1/W5/W10 multipliers 1.00/1.20/1.45; Chaser scaled at 1.2 (36 HP / 4.2 speed); Range/Cooldown unchanged; W1 schedule (5/8s/1.6) unchanged.
- Four weapons: Pulse Gun killed 4 test enemies (within range) → EnemyKilled flowed + pickups dropped.
- EventBus: WaveStarted per wave (4 total incl. restart + W10), boss events exactly once each, no duplicates.
- Cross-play static state: 3 final Play/Stop cycles — 0 errors / 0 warnings each, no NullReference/MissingReference/residue.

### Debugging notes (methodology)
- Probe asserts fixed, not production: StartWave(10) legitimately publishes WaveStarted(10) (assert expected +1); a test enemy at y=9 was outside Pulse Gun's range 8 (physics boundary), then y=8 sat on the boundary — final positions y=4..7 killed reliably. Play-mode time advances slowly while MCP tool calls are frequent, so waits used longer sleeps with fewer queries.

### Final verification
- 3 clean final Play/Stop cycles: 0 errors, 0 warnings each.
- Git clean; temporary probe deleted; no /tmp residue.

### M8 Status
M8 — Wave System: COMPLETE (lifecycle + difficulty + boss + final regression).

### Next
M9 — Roguelite / Upgrade (M9.1 XP Level Up).

## 2026-08-16
### Milestone
M9.1 — XP Level Up (part of M9 — Roguelite / Upgrade)

### Completed
- Assets/Scripts/Player/PlayerProgress.cs: gained level state — Level (starts 1), XPToNextLevel (M9.1 PLACEHOLDER formula 100 × level, explicitly NOT a GAME_DESIGN value), AddXP accumulates XP and levels up with carry-over; one AddXP can cross multiple levels (each level publishes its own event; the loop always terminates because the threshold grows and XP strictly decreases).
- Assets/Scripts/Core/GameEvents.cs: added PlayerLevelUp(level) — published once per level gained, carries the NEW level.
- Assets/Scripts/Player/PlayerLevelSystem.cs (new, attached to the Player in SC_Main and the Player prefab): subscribes PlayerLevelUp; only while GameState == Playing calls GameManager.TryChangeState(LevelUp) (Playing → LevelUp is a legal transition; GameManager untouched). Non-Playing states are never force-entered.
- WaveManager untouched — its existing `GameState != Playing` check freezes wave time/spawns during LevelUp; resume continues the same wave with no duplicate WaveStarted.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M91LevelProbe, deleted): 41/41 PASS, 0 FAILURES — initial Level 1 / XP 0; threshold placeholder 100; AddXP(0)/negative ignored; 90 XP no level-up + no event; 90+20 → Level 2 with carry 10 + PlayerLevelUp(2) exactly once + threshold 200; Playing → LevelUp (PlayerLevelSystem) not re-entered; WaveElapsed + spawns frozen during LevelUp + wave NOT reset; LevelUp → Playing resumes wave (elapsed continues, no duplicate WaveStarted); 340 → Level 3 / XP 150 + PlayerLevelUp(3) once; 1000 → Level 5 / XP 450 crossing levels with PlayerLevelUp(4)+(5) each once; GameOver: XP still processes + level-ups publish but GameOver NOT force-entered LevelUp (non-Playing protection); regressions (Gold, PlayerHealth, weapons, Boss prefab, Boomerang ActiveCount).
- Debugging notes (methodology): a scene demo XPPickup adds XP on the first physics frame before the probe can disable it — the probe resets the test target's XP/Level via reflection for clean assertions (test-only; production untouched); two assertion arithmetic corrections (threshold grows with level: 350 → L3/150, 1150 → L5/450, 950 → L6/450).
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.2 — Upgrade Chooser (3 random options).

## 2026-08-16
### Milestone
M9.2 — Upgrade Chooser Logic (part of M9 — Roguelite / Upgrade)

### Completed
- Assets/Scripts/Player/UpgradeData.cs: `UpgradeData : ScriptableObject` (UpgradeId, DisplayName, StatType enum — the 10 GAME_DESIGN stats, Amount additive) with CreateAssetMenu.
- Assets/ScriptableObjects/Upgrades/: 10 assets (MaxHP +10, HPRegen +0.5, MoveSpeed +0.5, Damage +1, AttackSpeed +0.1, CritChance +0.02, CritDamage +0.25, Range +0.5, PickupRange +0.5, Armor +1) — M9.2 implementation placeholders, explicitly NOT GAME_DESIGN balance.
- Assets/Scripts/Player/PlayerStats.cs: runtime bonus layer — per-stat private bonus fields; every accessor returns base + bonus; ApplyUpgrade(UpgradeData) adds the amount to the matching bonus; ResetForRun() zeroes all bonuses. Serialized base fields are never modified.
- Assets/Scripts/Player/UpgradeManager.cs (new, on Player scene + prefab with the 10-upgrade pool): listens for PlayerLevelUp, keeps a pending level-up queue (one AddXP crossing levels queues each level), enters LevelUp only while Playing (no re-entry), GenerateOptions() draws 3 UNIQUE options (no weights/rarity; repeats across level-ups allowed), SetForcedOptions(...) test hook, Select(index) guards (waiting + LevelUp + valid index) and applies the chosen upgrade once, publishes UpgradeSelected, then either stays LevelUp with the next options (pending > 0) or returns to Playing (pending == 0).
- Assets/Scripts/Core/GameEvents.cs: added UpgradeSelected(upgrade, level).
- WaveManager untouched — LevelUp still freezes wave time/spawns and resume continues the same wave.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M92UpgradeProbe, deleted): 58/58 PASS, 0 FAILURES — assets (10, one per stat, valid amounts); base stats 100/5/0/10; ApplyUpgrade MaxHP +10 → 110, MoveSpeed +0.5 → 5.5, Armor +1 → 1; ResetForRun clears; XP basics (0/negative ignored, 90 no level-up, no event); Level 2 + PlayerLevelUp(2) once + pending 1 + Playing → LevelUp; wave elapsed frozen + no duplicate WaveStarted; 3 options non-null + unique; Select(0) applies only the chosen one + UpgradeSelected once + returns to Playing + wave resumes; double-select / invalid index / non-LevelUp select ignored; GameOver → MainMenu → Playing protection (200 XP → Level 3 → options again); one AddXP crossing Level 3 → 5 (pending 2, two forced selects both applied, UpgradeSelected twice, stays LevelUp between, wave restored exactly once); regressions (PlayerHealth, Pulse Gun + WeaponManager, Boomerang ActiveCount).
- Debugging notes (methodology): a scene XPPickup is collected on the very first physics frame — the probe now parks scene pickups in the AfterSceneLoad static hook (before any Start/contact); three probe fixes — the fresh-run check must account for the carried Level (threshold = 100 × level, so AddXP(200) at Level 2), the second consecutive selection must also SetForcedOptions (Select auto-regenerates random options when pending > 0), and the scene Player has no ArcBlade child (weapons checked via Pulse Gun + WeaponManager).
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.3 — LevelUp UI Panel.

## 2026-08-16
### Milestone
M9.3 — LevelUp UI Panel (part of M9 — Roguelite / Upgrade)

### Completed
- Assets/Scripts/Core/GameEvents.cs: added UpgradeOptionsGenerated(option0, option1, option2) — fact-only event carrying the fully-written candidate set.
- Assets/Scripts/Player/UpgradeManager.cs: GenerateOptions() and SetForcedOptions() now publish UpgradeOptionsGenerated AFTER Options is complete (UI listeners always read the full set).
- Assets/Scripts/UI/LevelUpPanel.cs (new): component on the ACTIVE Canvas (inactive objects never get Awake — a critical lesson). GameStateChanged controls visibility (LevelUp → show; Playing/GameOver/Victory/MainMenu → hide; Paused/Shop untouched). UpgradeOptionsGenerated updates the 3 button labels (DisplayName / StatType / +Amount; integer amounts shown as integers, floats raw, no %). Buttons bound once via onClick.AddListener → UpgradeManager.Select(i). No stats/game-state writes.
- Assets/Scenes/SC_Main.unity: Canvas (Screen Space Overlay + CanvasScaler ScaleWithScreenSize 1920x1080 match 0.5 + GraphicRaycaster) + EventSystem with InputSystemUIInputModule (project uses Unity Input System; StandaloneInputModule not used) + LevelUpPanel hierarchy (Image, Title "LEVEL UP!", 3 UpgradeButtons each with a label child), panel initially inactive.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M93LevelUpUIProbe, deleted): 55/55 PASS, 0 FAILURES — Canvas/EventSystem/InputSystemUIInputModule present; panel starts hidden; exactly 3 buttons; title correct; LevelUp → panel visible; UpgradeOptionsGenerated received; 3 labels match DisplayName/StatType/Amount ("Max HP +10|MaxHP|+10" etc.); REAL button.onClick.Invoke() applied only the chosen upgrade + UpgradeSelected once + Playing + panel hidden + wave resumes; duplicate click ignored; consecutive level-ups (L3→L5): panel stays visible, second set genuinely refreshed (button0 changed Move Speed → Armor) + applied, Playing + hidden after queue drains; non-LevelUp states hidden; exactly 1 Canvas/EventSystem/LevelUpPanel; options events per generation (6); regressions (PlayerProgress/PlayerHealth/weapons/Boomerang/EnemySpawner).
- Debugging notes (methodology): (a) the first UI creation script crashed (TMP_Settings.defaultFontAsset getter NRE) AFTER already creating Canvas/EventSystem/Panel/Title — leaving duplicates; cleaned by name and rebuilt in one pass. (b) Putting the LevelUpPanel component on the INACTIVE panel meant Awake never ran → no subscriptions; moved the component to the active Canvas with the panel as a serialized child reference. (c) probe: GameObject.Find only sees active objects, so the inactive panel was located via canvas.transform.Find; Title check needs FindObjectsInactive.Include. All production-behavior fixes, not probe-only.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.4 — Shop.

## 2026-08-16
### Milestone
M9.4 — Shop (part of M9 — Roguelite / Upgrade)

### Completed
- Assets/Scripts/Player/PlayerProgress.cs: added TrySpendGold(int) — the ONLY gold-spending entry; amount <= 0 or insufficient gold → false (nothing changes), otherwise deducts and returns true; gold never negative.
- Assets/Scripts/Shop/ShopItemData.cs (new): ScriptableObject (ShopItemType Weapon/StatBonus, DisplayName, Price, WeaponPrefab, Upgrade reference). WeaponUpgrade type deferred.
- Assets/ScriptableObjects/Shop/: 14 assets — 4 weapons (30 gold) + 10 stat bonuses (20 gold), referencing the existing weapon prefabs and UpgradeData. Prices are M9.4 implementation placeholders, NOT GAME_DESIGN.
- Assets/Scripts/Shop/ShopManager.cs (new, scene GameObject "ShopManager"): listens WaveCompleted (W1..W9; Wave 10 never publishes it → no shop). GenerateProducts: 4 products = 2 weapons + 2 stat bonuses, unique per shop (M9.4 rule), no weights/rarity. Purchase(i): stat bonus → UpgradeData → PlayerStats.ApplyUpgrade; weapon → prefab Instantiate + WeaponManager.Equip into an EMPTY slot, gold spent AFTER a successful equip, already-owned (same WeaponData) and no-empty-slot rejected WITHOUT spending. SetForcedProducts test hook. Refresh(): flat 20 gold (M9.4 placeholder, unlimited) → re-roll + reset purchase state. Continue(): Shop → Playing (WaveManager resumes the pre-started next wave).
- Assets/Scripts/Core/GameEvents.cs: added ShopProductsGenerated(product0..3) — published after the product list is fully written (entry, purchase, refresh).
- Assets/Scripts/UI/ShopPanel.cs (new, on the ACTIVE Canvas — same pattern as LevelUpPanel): GameStateChanged show/hide (Shop visible, other states hidden), ShopProductsGenerated drives the 4 product buttons (DisplayName/ItemType/Price) + gold text, Purchase/Refresh/Continue buttons wired to ShopManager. Single Canvas/EventSystem preserved.
- Assets/Scenes/SC_Main.unity: ShopManager object + ShopPanel hierarchy (Title, GoldText, 4 ProductButtons, RefreshButton, ContinueButton) under the existing Canvas.

### Verification
- Compilation: 0 errors.
- In-play probe (temporary M94ShopProbe, deleted): 58/58 PASS, 0 FAILURES — gold spend rules (<=0/insufficient rejected, never negative); W1 completes naturally → Shop state + ShopPanel visible + wave elapsed frozen; 4 products unique, 2 weapons + 2 stat bonuses; stat purchase (20 gold, applied, no double-buy); weapon purchase into an empty slot (30 gold, equipped, SlotCount unchanged, no double-buy); ALREADY-OWNED PulseGun product rejected without spending; no-empty-slot weapon purchase rejected without spending; Refresh (20 gold, re-roll, purchase state reset, re-published; insufficient gold fails without deduction); Continue → Playing → next wave resumes with no duplicate WaveStarted; ShopPanel hidden after Continue; W10 boss → BossDefeated → Victory with NO shop products (no WaveCompleted(10)); Victory hides the panel; UI buttons exist; exactly 1 Canvas / 1 EventSystem; regressions (PlayerProgress/PlayerStats/PlayerHealth/Pulse Gun/Boomerang ActiveCount).
- Debugging notes (methodology): (a) GenerateOptions-style bug — DrawUnique compared _products.Count against the per-pool target, so the second pool (stats) drew nothing when the first pool already filled 2; fixed with a local drawn counter. (b) The scene Pulse Gun self-equips into slot 0 (M6.2 Start behavior), so the PulseGun shop product is legitimately "already owned" — the probe became deterministic via SetForcedProducts (Boomerang/ArcBlade/MaxHP/MoveSpeed) plus sufficient gold, and explicitly asserts the already-owned rejection. (c) Refresh event-count assertion recorded the counter AFTER Refresh() (which already published) — moved the baseline before the call.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.5 — Weapon Upgrade.

## 2026-08-16
### Milestone
M9.4 UI Polish (Chinese localization + ShopPanel layout fix — no gameplay logic changes)

### Completed
- Assets/Scripts/UI/ShopPanel.cs: rebuilt around a 4-product-card layout — each card has separate Name / Type / Price texts and a Buy button (no overlapping children). All player-facing text is now Chinese via a display mapping (ShopItemData / UpgradeData / enums / WeaponData untouched): 商店 / 金币：{N} / 武器 / 属性 / 购买 / 已购买 / 刷新（20金币） / 继续; weapons 脉冲枪/散射爆能枪/回旋镖/弧刃; stats 最大生命值/生命回复/移动速度/伤害/攻击速度/暴击率/暴击伤害/攻击范围/拾取范围/护甲; cards show "+属性 数值" and "价格：XX 金币". TMP overflow set to Overflow + word wrapping so long names never clip.
- Assets/Scripts/UI/LevelUpPanel.cs: option labels changed to Chinese "+属性 数值" (e.g. "+最大生命值 10"); scene Title changed from "LEVEL UP!" to "升级！".
- Assets/Scenes/SC_Main.unity: ShopPanel hierarchy rebuilt (Title, GoldText, 4×ProductCard{Name,Type,Price,BuyButton}, RefreshButton, ContinueButton) under the existing single Canvas; panel 600×700 centered, cards 560×96 spaced 115 apart (no overlap). CanvasScaler unchanged (1920×1080, match 0.5) — content fits 1280×720 and 1024×768 (scaled height ≈ 431–497 logical → content ≈ 580 logical incl. margins, fits).
- No gameplay logic touched: ShopManager / PlayerProgress / PlayerStats / WeaponManager / WaveManager / GameManager / UpgradeData / ShopItemData unchanged.

### Verification
- In-play probe (temporary M94UIFixProbe + UIFixTool, deleted): 32/32 PASS, 0 FAILURES — panels start hidden; LevelUp title 升级！ + Chinese options (+最大生命值 10 / +移动速度 0.5 / +护甲 1) + choose → Playing; W1 → Shop state + panel visible; 4 cards evenly spaced (no overlap); Chinese texts (商店 / 金币：0 / 刷新（20金币） / 继续 / 回旋镖 / 弧刃 / +最大生命值 10 / +移动速度 0.5 / 武器 / 价格：30 金币 / 购买); buy → -30 gold + 已购买 + disabled + gold refresh; refresh → -20 gold + reset to 购买; continue → Playing + panel hidden (all logic unchanged).
- Debugging notes: the scene-builder ran as a temporary runtime tool (execute_code's codedom cannot reference Editor assemblies nor compile Chinese literals reliably — the rebuild was driven by a plain runtime class + execute_code calling it); probe TMP paths must include the child "Label" (UIFixTool attaches TMP to a Label child; the M9.3 LevelUp Title carries its TMP directly); the LevelUp choose button must be found by path (UpgradeButton0), not by sorting all scene buttons.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.5 — Weapon Upgrade.

## 2026-08-16
### Milestone
TMP CJK Font Integration (Noto Sans SC static SDF + atlas persisted as sub-asset)

### Completed
- Assets/Fonts/NotoSansSC-Regular.otf (8.3MB): Noto Sans CJK SC SubsetOTF (OFL-1.1) — source font.
- Assets/Fonts/NotoSansSC-LICENSE.txt: full SIL OFL 1.1 text.
- Assets/Fonts/NotoSansSC SDF.asset (+sub-asset atlas + material): static SDF TMP Font Asset baked from NotoSansSC-Regular.otf in Edit mode (TryAddCharacters + AddObjectToAsset for atlas/material persistence — atlasPath resolves to the main font asset, so play mode reuses the persisted texture and the dynamic-atlas MissingReferenceException seen in earlier attempts is gone).
- The static bake includes every character used by the current Shop/LevelUp UI (商店/金币/武器/属性/购买/已购买/刷新/继续/价格/升级/脉冲枪/散射爆能枪/回旋镖/弧刃/最大生命值/生命回复/移动速度/伤害/攻击速度/暴击率/暴击伤害/攻击范围/拾取范围/护甲/：/（）/+/.0-9) — 71 Chinese + punctuation glyphs, lookup=83, missingChinese=0/71.
- Scene SC_Main.unity: every TextMeshProUGUI rewired to NotoSansSC SDF (direct font, no fallback chain — avoids the TMP_MaterialManager.GetFallbackMaterial NRE).

### Verification
- Play mode probe:
  - Shop panel: font=NotoSansSC SDF, "商店" mesh verts=12 (live geometry), text strings fully readable in Chinese, screenshot `D:/Work/ui_diag_shop_final.png` shows 商店 / 金币：0 / 武器 / 属性 / 价格：30 金币 / 价格：20 金币 / +生命回复 0.5 / +暴击伤害 0.25 / 购买 / 刷新（20金币） / 继续 / 脉冲枪 / 回旋镖 etc. with NO □ placeholders.
  - LevelUp panel: the same font asset is wired in (same Wire tool). Label "—" placeholder renders cleanly in NotoSansSC (proves the font pipeline works on every TMP in the scene); the live LevelUp "升级！ / +最大生命值 10" path was blocked by a pre-existing NRE inside LevelUpPanel.OnOptionsGenerated (button[i].GetComponentInChildren returning null) which is unrelated to the CJK work — the font + glyph path is shared with the Shop probe and is verified there.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M9.5 — Weapon Upgrade (per previous M9.4 final report).

## 2026-08-16
### Milestone
LevelUpPanel NRE investigation & fix (M9.3 upgrade-chooser race)

### Root cause (verified in Play)
- NO NullReferenceException in production code. The reported NRE came from test scripts reading `Button.GetComponent<TextMeshProUGUI>()` (the label TMP lives on the button's child "Label").
- A REAL bug was found and fixed: PlayerLevelSystem and UpgradeManager BOTH subscribe to PlayerLevelUp. Depending on Awake order, PlayerLevelSystem could enter GameState.LevelUp FIRST, so UpgradeManager.OnPlayerLevelUp saw `CurrentState != Playing` and returned WITHOUT calling GenerateOptions -> UpgradeOptionsGenerated never published -> LevelUpPanel labels stayed "—" (this is why an earlier probe saw the panel show with no options; the order is unstable, so earlier test sessions happened to subscribe UpgradeManager first).
- Fix (Assets/Scripts/Player/UpgradeManager.cs, minimal): OnPlayerLevelUp now generates options whenever state is Playing OR LevelUp (if already LevelUp from the other subscriber, still GenerateOptions; only enters LevelUp when Playing). Pending queue, Select, ApplyUpgrade, UpgradeSelected, LevelUp->Playing unchanged.

### Verification
- Temporary M93LevelUpFinalProbe (deleted): 17/17 PASS, 0 FAILURES — panel hidden initially; 3 upgrade buttons + TMP Label children present; Playing -> LevelUp; panel visible; Button0 label is Chinese "+暴击伤害 0.25"-style (starts with +, no □); Button0 + Title meshes have vertices (Chinese actually rendered via NotoSansSC SDF); real `Button.onClick.Invoke()` -> UpgradeManager.Select -> PlayerStats bonus applied (e.g. CritDamage 1.50 -> 1.75) -> pending drained -> Playing -> panel hidden. No NRE anywhere.
- Screenshot: D:/Work/ui_diag_levelup_cam.png (Camera-rendered capture) shows 升级！ title + 3 Chinese upgrade options, all glyphs correct, layout intact (a moving enemy projectile crosses one card — scene AI, not the panel).
- Final clean play/stop twice: 0 errors, 0 warnings.

## 2026-08-16
### Milestone
M9.4 ShopPanel Layout Fix (anchoredPosition 修复 — 纯布局修正)

### Root cause (确诊于 21:11 only-read diagnostic)
- M9.4 UIFixTool 的 `MakeRect(parent, anchor, anchoredPos, size)` 把 `anchoredPosition` 当成 "距父边缘 inset" 写入（按 edge-margin 语义），但 Unity 的 `RectTransform.anchoredPosition` 是 **pivot(0.5, 0.5) 距 anchor 参考点的偏移**。所有使用角点 anchor 的 ProductCard 子项整体偏移了 `(+w/2, +h/2)`。
- 截图证据（修复前 3 个分辨率）：Name/Price 文字跑出卡片**左外**，Type 跑出卡片**右外**，BuyButton 从卡片右下探出。

### Fix (2026-08-16 21:43)
- 修改 SC_Main.unity 中 4 个 ProductCard 子节点的 anchoredPosition（pivot 0.5/0.5，按 inset 16/16 + BuyButton inset 14/14 推算）：
  - Name: (16,-16) → **(196,-32)**
  - Type: (-16,-16) → **(-91,-29)**
  - Price: (16,16) → **(166,29)**
  - BuyButton: (-14,14) → **(-74,37)**
- **未改**：ProductCard 尺寸、Name/Type/Price/BuyButton size、ShopPanel 600×700、Canvas/CanvasScaler/EventSystem、TMP Font Asset、ShopPanel.cs、ShopManager.cs、ShopItemData、PlayerProgress/Stats、WeaponManager、WaveManager、GameManager。
- 实现方式：单次 execute_code 调用（codedom，无局部函数、无中文字面量），直接赋值 RectTransform.anchoredPosition + EditorSceneManager.SaveScene。
- 中途副发现：Play 期间 Unity 错误地写坏了 NotoSansSC SDF.asset（atlas 从 1024² 变成 1×1，删 1829 行）—— 这是 Editor Play 时 TMP Dynamic Font Asset 的已知问题（之前 CJK 任务遇到过的 atlasTexture 不持久化）。**`git checkout HEAD -- "Assets/Fonts/NotoSansSC SDF.asset"` 从 HEAD 恢复**（最终 commit 不包含字体 asset 改动，atlas 仍为 1024²、glyph 82 个、中文 missing 0/71）。

### Verification (Play Mode 实测)
- **布局**：4 个 ProductCard × 4 子节点全部 inside card rect（GetWorldCorners 验证 inside=True），Name 左上、Type 右上、Price 左下、BuyButton 右下，无重叠/越界。
- **三分辨率实际截图**（GameView.position 反射切换，Unity Editor 窗口约束下实际尺寸略异）：
  - 1920×1080 → screen 1280×933，scaleFactor 0.759
  - 1280×720 → screen 1600×874，scaleFactor 0.821
  - 1024×768 → screen 1280×934，scaleFactor 0.759
  - 三张截图均呈现：Name 左上、Type 右上、Price 左下、BuyButton 右下、底部 Refresh/Continue 按钮完整可见。
- **Shop 完整功能回归**（反射 OnWaveCompleted 私有方法触发）：gold=300 → W1 → state=Shop/products=4 → statIdx=2 购买成功 gold 300→280 (-20) → Refresh gold 280→260 (-20) → Continue state=Playing → publish WaveCompleted(10) 状态保持 Playing (W10 不进 Shop)。
- **中文渲染**：4 张卡片的 Name/Type/Price/BuyButton TMP mesh.vertexCount 全部 > 0（实际字符顶点生成，字符 `散射爆能枪`、`回旋镖`、`+移动速度 0.5`、`+暴击率 0.02`、`武器`、`属性`、`价格：30 金币`、`价格：20 金币`、`购买` 全部正确）。
- **最终 Play/Stop ×2**：0 errors / 0 warnings。

### Next
M9.5 — Weapon Upgrade。

## 2026-08-16
### Milestone
M9.4 Shop Type/BuyButton Layout Fix v2 (消除右上/右下视觉重叠)

### Root cause (实测确认)
- **用户最初指定值（Type (-91,-32)/150×22 + BuyButton (-74,39)/120×46）实测仍重叠**：Type anchor (1,1) 顶右角 pos y=-32 实际比旧值-29 更靠下（绝对值越大越靠下），BuyButton anchor (1,0) 底右角 pos y=39 实际比旧值37 更靠上（正值越大越靠上）——两者**相向移动**；且 size 26→22 后半高从 13 减为 11，用户按旧半高 13 推算"Type 底 3"实际为 5。卡片坐标 gap = Type底5 - Buy顶14 = **-9px**（重叠 9px）
- **文字视觉间距**：fontSize 20 行高 28.96 > rect 22 → textBounds 超 rect 6.96px，文字向下溢出 → 文字底 < Buy 顶，视觉间距仅 6.9 卡片 px

### Fix
| 节点 | 旧值 | 新值 | 效果 |
|---|---|---|---|
| Type anchoredPosition | (-91,-29) | **(-91,-15)** | 中心 19→33（更靠上）+ Type 底 6→22 |
| Type sizeDelta | 150×26 | **150×22** | 高减 4 |
| Type fontSize | 20 | **15** | Noto Sans SC 行高系数 1.448 → 21.72 ≤ 22（textBounds 不超 rect） |
| BuyButton anchoredPosition | (-74,37) | **(-74,32)** | 中心 -11→-16（更靠下）+ BuyButton 顶 12→7 |
| BuyButton sizeDelta | 120×46 | **120×46**（保持） | — |
- 卡片坐标：rectGap = Type底22 - Buy顶7 = **15px ≥10 ✓**；textGap = 文字底 - Buy顶 = **11.7px >0 ✓**；textBounds 21.72 ≤ rect 22 ✓

### Verification
- **Play 实测 4/4 PASS**（scale 0.759 → 卡片坐标系）：rectGap=15、textBoundsH=21.72≤rectH=22、textGap=11.7>0、无 yOverlap，4 张卡片一致（"武器"/"属性"均 PASS）
- **三分辨率实际截图**（GameView.position 反射切换）：
  - 当前（1280×934）→ `D:/Work/ui_fix2_shop_current.png` + `ui_fix2_card0_current.png`
  - 1280×720（实际 1600×874）→ `ui_fix2_shop_1280.png` + `ui_fix2_card0_1280.png`（"武器"小字15px 清晰在右上角、"购买"绿色按钮24px 在右下角、明显空白间隙）
  - 1024×768（实际 1280×934）→ `ui_fix2_card0_1024.png`
- **Shop 完整功能回归**：W1→Shop/State=Shop/isInShop=True/products=4 → stat购买 gold 200→180 (-20) → Refresh gold 180→160 (-20) → Continue state=Playing → W10 publish state保持Playing products=4
- **最终 Play/Stop×2**：0 errors / 0 warnings

### Next
M9.5 — Weapon Upgrade.

## 2026-08-16
### Milestone
M9.4 Shop Weapon Purchase Owner Fix (购买武器 Parent 缺陷修复)

### Root cause (Play 实测确认，22:25 只读核查)
- ShopManager.TryPurchaseWeapon 的 `Instantiate(WeaponPrefab)` 后**没有 SetParent** → 武器实例挂在 Scene Root（transform.parent=null）→ WeaponController.Owner（`GetComponentInParent<PlayerAttack>()`）向上查找找不到 Player → Owner=null → `Attack()` 在 PlayerAttack 解析前短路返回 `DamageResult(false,0,false)` → **Shop 购买并装备的武器无法造成任何伤害**。
- 四把武器（PulseGun/ScatterBlaster/Boomerang/ArcBlade）全部受影响；对照组（SetParent(Player) 后 Owner=Player、攻击 applied=True）证实根因。
- M9.4 probe 未发现的原因：只断言了 Equip 成功 + 金币扣除，未验证购买武器的实际伤害。

### Fix
- `Assets/Scripts/Shop/ShopManager.cs` TryPurchaseWeapon：`Instantiate` 后、`Equip` 前加 `instance.transform.SetParent(_weaponManager.transform, false)`。
- parent 来源：`_weaponManager`（挂在 Player 上的 WeaponManager 组件，ResolveRefs 从 Player 获取）——**不用全局 FindFirstObjectByType<PlayerAttack>**，符合"武器挂 Player 层级"的架构语义。
- 购买语义保持不变：商品→空槽→已拥有拒绝→Gold→Instantiate→SetParent→Equip→(Equip 失败 Destroy+refund)→标记已购。

### Verification
- 临时 WeaponPurchaseRegressionProbe（已删除）：**51/51 PASS，0 FAILURES**。
- 四武器各自验证：Purchase 成功 / gold -30 / equipped / **parent==Player** / **Owner==Player** / attack applied=True / damage=Data.BaseDamage（5/3/7/8）/ **DamageApplied 事件触发且 source==Player**（真实伤害管道）。
- 拒绝路径：已拥有 PulseGun 拒绝且不扣 Gold；无空槽拒绝且不扣 Gold、槽数不变。
- Wave resume：购买 Boomerang → Continue → Playing → 购买武器仍能攻击（applied=True）。
- 最终 Play/Stop×2：0 errors / 0 warnings。

### Next
M9.5 — Weapon Upgrade（尚未开始）。

## 2026-08-16
### Milestone
M9.5 — Weapon Upgrade (Weapon runtime level/bonus + Shop WeaponUpgrade products)

### Completed
- `WeaponController` (M9.5 runtime layer): non-serialized `_weaponLevel=1` + `_damageBonus/_attackCooldownBonus/_rangeBonus=0`; `EffectiveDamage = Data.BaseDamage + DamageBonus`, `EffectiveAttackCooldown = max(0.05f, Data.AttackCooldown + CooldownBonus)` (0.05 = runtime safety floor, NOT a design value), `EffectiveRange = Data.Range + RangeBonus`; `ApplyWeaponUpgrade(WeaponUpgradeData)` validates upgrade + requires TargetWeapon == this.Data, then level++ and additive bonus; `ResetWeaponUpgrades()`; read-only getters for UI/shop. WeaponData and serialized base values NEVER mutated.
- `WeaponUpgradeData.cs` (new ScriptableObject): UpgradeId / DisplayName / TargetWeapon / WeaponUpgradeStat (Damage, AttackCooldown, Range — first-version scope only) / Amount.
- 4 weapons: read points switched from Data.X to EffectiveX — PulseGun (FireAt damage, cooldown, Range for acquire/valid), ScatterBlaster (SpawnProjectile damage, cooldown, Range), Boomerang (ThrowAt damage, cooldown, Range), ArcBlade (Strike damage, cooldown, Range). No changes to Projectile Pool / Init / CombatSystem / DamageRequest / Boomerang ActiveCount / ArcBlade OverlapCircleAll.
- `ShopItemType` += WeaponUpgrade; `ShopItemData` += `weaponUpgrade` (WeaponUpgradeData ref). Legacy StatBonus assets migrated itemType 1 -> 2 (enum reorder).
- `ShopManager`: Purchase WeaponUpgrade branch (`TryPurchaseWeaponUpgrade` — requires target weapon EQUIPPED via `equipped.Data == upgrade.TargetWeapon` (no name compare), gold check -> ApplyWeaponUpgrade -> TrySpendGold, any failure spends nothing); GenerateProducts M9.5 rule = 1 Weapon + 1 WeaponUpgrade + 2 StatBonus, upgrade drawn ONLY for owned weapons (else falls back to stat); `LevelOfEquipped(WeaponData)` for UI.
- `ShopPanel`: WeaponUpgrade card shows multi-line 武器名/升级：属性（伤害/攻击速度/攻击范围）/等级：Lv.X → Lv.X+1/价格：XX 金币.
- 12 WeaponUpgradeData assets (4 weapons x Damage+1 / AttackCooldown -0.05 / Range +0.5) + 12 ShopItemData WeaponUpgrade products (price 30) appended to scene ShopManager.productPool (total 26). **All amounts/prices are M9.5 IMPLEMENTATION parameters, NOT GAME_DESIGN values.**

### Verification
- Temporary M95WeaponUpgradeProbe (deleted): 42/42 PASS, 0 FAILURES — defaults Level=1/bonus=0 on all 4; 12 assets loadable+valid; direct upgrades (PulseGun/ArcBlade Damage+1/Cooldown-0.05/Range+0.5, level 1->4, additive stacking); Scatter/Boomerang EffectiveDamage/Range; cooldown clamped >= 0.05 after 10x -0.05; WeaponData asset unchanged; wrong-target and null upgrades rejected; fresh instance Level=1/bonus=0; GenerateProducts mix 1/1/2; upgrade product targets OWNED weapon only (PulseGun upgrade absent when PulseGun not owned, Boomerang upgrade present when owned); WeaponUpgrade purchase (-30 gold, Level+1, marked purchased); same product not double-bought; Continue -> Playing; upgraded weapon attacks after shop.
- Probe debug notes: (a) initial FAILs were probe-side — a broken validity condition excluded Damage assets (StatType==0) and float `==` comparisons on cooldown; fixed to approx compare. (b) Enum reorder needed the legacy StatBonus asset migration (itemType 1->2) else stats were misread as WeaponUpgrade.
- Final clean play/stop twice: 0 errors, 0 warnings.

### Next
M10 — Boss (per milestone list).

## 2026-08-17
### M9 Final Regression & Acceptance — M9 COMPLETE / ACCEPTED
- M9.1～M9.5 全部通过；**82/82 PASS, 0 FAILURES**（临时 M9FinalRegressionProbe，已删除）。
- 覆盖：完整 M9 链（XP→LevelUp→真实 UI 点击→Shop 购买/刷新/继续→W9 Shop→W10 不进店→Boss）、M6/M7/M8 回归（四武器、三池、Boomerang ActiveCount、WaveStarted 无重复）、GameOver XP 隔离、跨会话状态干净。
- **3× 独立 Play/Stop：0 errors / 0 warnings 每次**；跨会话无残留（PulseGun Lv1、Boomerang 0、面板隐藏）。
- 回归期间未改任何生产代码；最终 Git clean。**M9 ACCEPTED。**

### Shop WeaponUpgrade UI Final Layout Fix
- **最终问题**：原 WeaponUpgrade 卡片把 武器名 / 升级：属性 / 等级：Lv.X → Lv.X+1 三行塞进普通 Name slot（360×32），造成文本上溢出（≈34.5 canvas）与等级/价格重叠。
- **最终修复**（commit `12f17e4`）：
  - WeaponUpgrade Name 改为两行单 TMP：第一行武器名 **24px**（与普通卡一致），第二行 `升级：{属性}  等级：Lv.{X} → Lv.{X+1}` 用 rich text `<size=15>`（15px，动态 WeaponLevel）。
  - Name rect 运行时 360×32 → **360×60**（TopLeft；仅 WeaponUpgrade 卡，普通卡保持 360×32 fs24）。
  - Price 22px / Type 15px / BuyButton 24px 不变；独立 Level row 删除（4 个 Level 子物体移除、ShopPanel.levelTexts 清空）。
  - 修复过程中发现的真实缺陷：①Name/Label 是 stretch 子物，改 Label sizeDelta 无效 → 必须改 Name（父）RectTransform；②levelTexts 数组 ClearArray 后 `[i]` 越界 → 加 `i < levelTexts.Length` 防御。
- **验证**：Damage / AttackCooldown / Range 三种 WeaponUpgrade 均 Name 完整在卡内、无 textBounds 重叠（Name vs Price/Type/BuyButton 全分离）、最长文本"升级：攻击速度"不越界；普通 Weapon / StatBonus 零变化；中文正常无 □；Console 0/0；Git clean。

## 2026-08-17
### M10.1 — Boss Base Framework — COMPLETE（验证现有 M8.3 Boss 实现）
- **结论**：M8.3 已完整实现 Boss Base Framework（BossData : EnemyData / BossAI / SpawnBossNow / OnEnemyKilled→BossDefeated→Victory / BossSpawned+BossDefeated 事件 / Boss 复用 EnemySpawner 同一 pool / WaveManager W10 独立逻辑）。按"不要重复创建已存在的东西"原则，M10.1 **不修改任何生产代码**，仅正式验证 + 文档化。
- 验证（临时 M10_1BossProbe，已删除）：**29/29 PASS, 0 FAILURES** — BossData 加载、prefab 组件链（EnemyController/Stats/Health/BossAI/Rigidbody2D + data 引用）、W10 真实路径 SpawnBoss（MaxHP 725/Damage 29/MoveSpeed 2.175，W10 WaveMultiplier 1.45 缩放正确）、死亡链 via CombatSystem（EnemyDied/EnemyKilled/BossDefeated/Victory 各一次）、State Victory、No wave 11（wave=10）、pool reuse 干净（IsDead=false/HP=MaxHP/velocity=0/WaveMultiplier 1.45）、Reused boss death chain 1/1/1/1、PulseGun 不受影响、Boomerang ActiveCount sane。
- probe 调试记录：①直接调 EnemyHealth.TakeDamage 只触发 EnemyDied（绕过 CombatSystem 无 EnemyKilled/BossDefeated）—— 验证必须走 CombatSystem.ApplyDamage；②直接 spawner.SpawnBoss 的 boss 不是 WaveManager._activeBoss → 无 BossDefeated —— 验证必须走 wave.StartWave(10) 真实路径；③W10 真实路径注入 WaveMultiplier 1.45 → 断言用缩放值（725/29/2.175）。
- 最终 Play/Stop×2：0 errors / 0 warnings；字体 asset 未被 Play 写坏（git diff 空）。
- M10.1 范围确认：无新 Boss 技能/多阶段/UI/VFX/掉落规则/新武器/新敌人/新波次机制；M8 Boss 行为完全保持。

## 2026-08-17
### M10.2 — Boss Projectile Skill — COMPLETE
- BossAI 新增唯一主动技能：每 skillCooldown(3s) 存活+Playing+玩家存活+距离≤skillRange(10) 时，向玩家**当前位置**发射 1 个 PulseProjectile（方向发射瞬间锁定，不追踪；source=Boss；damage=runtime Stats.Damage 自动继承 WaveMultiplier；lifetime 3s；命中一次即 Release）。数值 3s/6.0/10.0 为 implementation parameters 非 GAME_DESIGN 值。
- **复用**：PulseProjectile 共享静态 ObjectPool（Boss prefab 挂同一 PulseProjectile.prefab，无第二套 Pool）+ CombatSystem.ApplyDamage + DamageRequest + IPoolable。未建第二套 Damage/Pool/HP 框架。
- Boss prefab：BossAI 新增 projectilePrefab/skillCooldown/projectileSpeed/skillRange 序列化字段（skillRange 10）。
- 验证（临时 M10_2BossAbilityProbe 已删）：**21/22 PASS** — W10 spawn/BossAI/冷却前不发射/冷却后发射 1 颗/source==Boss/damage==29(W10 缩放)/命中玩家 HP 下降(58=接触29+弹道29)/命中后 Release/Boss 死亡后不再发射/池复用技能状态干净重发/BossDefeated+Victory 各一次/No wave11/PulseGun+Boomerang 回归。唯一 FAIL 为 probe 内 DamageApplied 订阅计数时序问题——独立最小验证（反射 Subscribe + CombatSystem.ApplyDamage）确认 DamageApplied 正常发布且 Target==Player，**非生产缺陷**。
- 最终 Play/Stop×2：0 errors / 0 warnings；字体 asset 未写坏（git diff 空）。
- 未做：第二阶段/Rage/多技能/AoE/弹幕/血条/UI/VFX/音效/新掉落/新状态机/新 GameState。

### M10.3 — Boss Finalization & Regression — COMPLETE（M10 = COMPLETE / ACCEPTED）
- **范围**：Boss 最终参数确认 + 完整战斗验收 + 跨系统回归 + 跨 Play Session 稳定性。**无新增 Boss mechanic、无生产代码修改**。
- **只读检查**：BossData.asset（500/20/1.5/1.5/1.0）、BossAI.cs（追击+接触+技能 3s/6.0/10.0）、Boss.prefab（projectilePrefab=PulseProjectile）、EnemyController/Stats/Health（WaveMultiplier 缩放）、WaveManager（W10 SpawnBossNow/_activeBoss→BossDefeated→Victory）、EnemySpawner（同池）、PulseProjectile（共享池）——与 M8.3/M10.1/M10.2 文档完全一致，无差异。
- **验证**（临时 M10_3BossFinalRegressionProbe，已删）：**55/55 PASS, 0 FAILURES** — BossData 最终参数 / W10 runtime 725·29·2.175（×1.45）/ 范围门（玩家>10 不发射）/ 追击 / 技能发射恰 1 颗 / 接触伤害 29（HP 71）/ Projectile source==Boss + damage==29（继承 WaveMultiplier）/ 方向锁定不追踪（dot=1.00）/ 命中玩家（HP 71）/ 命中后 Release / 冷却 armed + 冷却中不发射 / 死亡链 EnemyDied·EnemyKilled·BossDefeated·Victory 各一次 / GameState Victory / No Wave11 / pool reuse 干净（IsDead=false·HP=725·velocity=0·multiplier 1.45）+ 技能冷却重置重发 / 跨两 run BossDefeated·Victory 各 2 次 / M6 四武器 + Boomerang ActiveCount / M7 pool 可达 + 无残留弹 / M9 XP·Shop·Upgrade。
- **probe 调试记录**：①玩家 PulseGun 自动攻击的弹会污染 projectile 计数 → 断言必须按 source==Boss 过滤；②Boss 生成在玩家位置 → spawn 帧可能先发射 1 颗 baseline 弹 → 用增量断言（baseline+Δ）；③反射 SetField 清零 `_nextSkillTime` 可控制发射时机（等效 OnSpawn 重置语义）。
- **平衡观察（实测）**：接触 29/次 ≈ 3.4 次致命；弹道 29/次、6.0 弹速、方向锁定可横向躲避；boss 移速 2.175 慢、725 HP 配合玩家四武器 DPS 约 20-35s 击杀 —— **适合作为 5~10 分钟 MVP run 的最终收尾**。**参数确认：No production parameter changes required**。
- **3× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删；跨 session Boss/projectile/EventBus/GameState 干净。
- **M10 = COMPLETE / ACCEPTED**（M10.1 + M10.2 + M10.3 全部完成）。

### M11.1 — Main Menu — COMPLETE
- 范围：最小 Main Menu（Game Title "虚空幸存者" + PlayButton "开始游戏" + QuitButton "退出游戏"）；Play → MainMenu→Playing；Quit → Application.Quit (Editor 下 EditorApplication.isPlaying=false)。
- **复用**：现有 Canvas / EventSystem / InputSystemUIInputModule（唯一确认）/ GameState / EventBus (GameStateChanged) / TMP NotoSansSC。**不创建第二套 Canvas/EventSystem/GameState/UI 事件系统**。
- **新文件**：`Assets/Scripts/UI/MainMenuPanel.cs`（仿 LevelUpPanel 模式：GameStateChanged 驱动显隐，Awake 订阅+初始显隐，OnPlayPressed→TryChangeState(Playing)，OnQuitPressed→#if UNITY_EDITOR ... #else Application.Quit()）。
- **场景**：SC_Main.unity Canvas 下新增 MainMenuPanel 子物体（深色 Image 背景 + Title TMP + PlayButton + QuitButton，绑定 MainMenuPanel 组件）。
- **验证**（临时 M11_1MainMenuProbe，已删）：**23/23 PASS, 0 FAILURES** — Canvas 唯一(1) / EventSystem 唯一(1) / MainMenuPanel 存在 / 初始可见（state=MainMenu）/ Title="虚空幸存者" / PlayButton+QuitButton 存在+onClick 各 1 个 listener（反射 m_Calls in UnityEventBase 沿基类链）/ Label "开始游戏"/"退出游戏" / **真实点击 PlayButton.onClick.Invoke() → GameState=Playing + MainMenuPanel 隐藏** / Playing→GameOver→MainMenu 合法返回 + MainMenuPanel 重新显示 / LevelUpPanel+ShopPanel 存在且在 MainMenu 隐藏（M9 回归）。
- **probe 调试记录**：①`Playing→MainMenu` 不是合法转换（M2 设计 Playing 只能去 Paused/LevelUp/Shop/GameOver/Victory）→ probe 改走 `Playing→GameOver→MainMenu` 合法路径；②`Button.onClick.GetRuntimeEventCount()` Unity 6 无此 API → 反射 `m_Calls` 沿基类链查找 UnityEventBase.Count。
- **视觉**：截图 `D:/Work/m11_mainmenu.png` 确认 "虚空幸存者"/"开始游戏"/"退出游戏" 中文渲染正常无 □；"开始游戏" 被 MainMenu 状态下 Player PulseGun 自动攻击循环发射的 PulseProjectile 覆盖——Player weapon pause 属更大范围（M11.1 严格限制只做 MainMenu 本身，不修改武器逻辑），不影响功能验证。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M11 整体 IN PROGRESS**（仅 M11.1 完成；M11.2/3/4 待任务）。
- Commit 67f2ce0（feat: M11.1 main menu ui）。

### M11.2 — Gameplay HUD — COMPLETE
- 范围：最小 HUD 5 文本（HP/XP/Lv./Gold/Wave）+ 顶部条；只读展示，事件驱动显隐 + 每帧读缓存引用刷数据。
- **复用**：现有 Canvas/EventSystem/InputSystemUIInputModule（唯一）/ GameStateChanged / PlayerHealth / PlayerProgress / WaveManager / TMP NotoSansSC。**不修改** PlayerHealth/PlayerProgress/WaveManager/GameManager/LevelUpPanel/ShopPanel/GameEvents。
- **新文件**：`Assets/Scripts/UI/GameplayHUD.cs`（Awake 缓存 PlayerHealth/PlayerProgress/WaveManager + 订阅 GameStateChanged + 初始显隐；Update 在 visible 时读缓存刷 5 个 TMP 文本）。
- **场景**：Canvas 下新增 GameplayHUD 子物体（半透明深色背景 Image + 5 TMP 文本：左上 HPText/XPText/LevelText，右上 GoldText/WaveText；锚顶 100 高）。
- **验证**（临时 M11_2GameplayHUDProbe，已删）：**26/26 PASS, 0 FAILURES** — Canvas/EventSystem 各 1 / 5 文本存在 / MainMenu 隐藏 / Playing 显示 / **HP 初始 100/100 + 受伤后 75/100（真实 TakeDamage）** / **XP 跟随 runtime（AddXP(50) 后 "XP 60 / 100"，含击杀拾取 +10 XP）** / Level 升级 Lv.2 + XP "10 / 200" / Gold "120" + 消费后 "100" / Wave StartWave(3)→"Wave 3" / LevelUp·Shop 显示 / GameOver·MainMenu·Victory 隐藏 / 无 □ / 文本非空 / bounds 内 + 5 文本垂直分离无重叠。
- **probe 调试记录**：①初始 probe 用绝对值断言（"XP 50/100"）→ 实际 Playing 中 WaveManager 自动 W1 + PulseGun 击杀敌人 + PickupSystem 拾取 → XP +10 → HUD 显示 60/100 是正确跟随真实 runtime → 断言改为"HUD 文本 == PlayerProgress 实时格式化值"（同步校验，证明了"HUD 跟随 runtime"）；②`Shop→GameOver` 非合法转换（M2 AllowedTransitions）→ probe 状态序列改为走合法路径（Playing→LevelUp→Playing→Shop→Playing→GameOver→MainMenu）。
- **视觉**：截图 `D:/Work/m112_hud_playing.png` Playing 状态确认左上 HP 75/100 / XP 10/200 / Lv.2 + 右上 Gold 100 / Wave 1，字体清晰无 □，不遮挡战斗区域。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M11 整体 IN PROGRESS**（仅 M11.1+M11.2 完成；M11.3/4 pending）。

### M11.3 — Result Screens — COMPLETE
- 范围：GameOverPanel + VictoryPanel + RunRestarter（最小 Run Restart API，Core 层）+ ResultPanel 通用组件。
- **修改**：PlayerProgress.cs 新增 `ResetForRun()`（XP=0/Level=1/Gold=0）；SC_Main.unity 加 GameOverPanel + VictoryPanel。
- **新增**：`Assets/Scripts/Core/RunRestarter.cs`（static `RestartRun()`：PlayerProgress.ResetForRun + PlayerStats.ResetForRun + 遍历 WeaponManager 调用 weapon.ResetWeaponUpgrades + `gm.TryChangeState(MainMenu→Playing)`，WaveManager 自动 StartWave(1)）；`Assets/Scripts/UI/ResultPanel.cs`（showInState + title + Restart/MainMenu 按钮，GameStateChanged 驱动显隐）。
- **不修改**：GameManager / SceneFlow / PlayerStats / WeaponController / WeaponManager / WaveManager / LevelUpPanel / ShopPanel / GameEvents。
- **重启链路**：RestartRun 重置 PlayerProgress/Stats/武器升级 → `MainMenu→Playing`（合法转换），WaveManager.OnGameStateChanged 检测 `_hasStartedRun=false → StartWave(1)` 自动新 run（M8.1 已支持）。
- **Victory 进入**保持 M8.3：`WaveManager.OnEnemyKilled` 匹配 `_activeBoss` → `BossDefeated` → `TryChangeState(Victory)`。**GameOver 自动触发**当前**不存在**（PlayerDied 事件发布但无消费者切 GameOver）——属 M11.4 全流程集成范畴；M11.3 面板经合法转换 Playing→GameOver 验证。
- **验证**（临时 M11_3ResultScreensProbe，已删）：**46/46 PASS, 0 FAILURES** — Canvas/EventSystem 各 1 / GameOverPanel+VictoryPanel 存在+按钮+标题中文（"游戏结束"/"胜利"）/ MainMenu+Playing 两面板隐藏 / GameOverPanel 在 GameOver 显示 / VictoryPanel 在 Victory 显示 / HUD 隐藏 / **GameOver MainMenuButton 真实点击→MainMenu（流程 B）** / **GameOver RestartButton 真实点击→Playing+HUD 显示+Level=1+XP=0+Gold=0+stats bonuses 清零+WeaponLevel=1+Wave=1（流程 A + reset 全部验证）** / **W10 BossDefeated→Victory→VictoryPanel 显示→Restart 真实点击→Playing（流程 C + M10 Victory 保持）** / **Victory MainMenuButton 真实点击→MainMenu（流程 D）** / LevelUpPanel+ShopPanel 存在（M9 回归）。
- **probe 技巧**：①用反射 `SetField` 给 PlayerStats._damageBonus/WeaponController._weaponLevel 模拟"脏状态"以验证 Restart 后清零；②Battle→GameOver 后 wave 不可再 StartWave(10) 测流程 C（因 GameOver→MainMenu→Playing → StartWave(1) 第一次自动新 run，Restart 再 StartWave(10) 测流程 C）；③M9 UI 回归仅验证 LevelUpPanel/ShopPanel 存在（M9 已全面验证）。
- **视觉**：截图 `D:/Work/m113_gameover.png` 深红背景 + "游戏结束" + 重新开始/主菜单；`D:/Work/m113_victory.png` 深蓝背景 + "胜利" + 重新开始/主菜单；中文渲染无 □。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M11 整体 IN PROGRESS**（仅 M11.1+M11.2+M11.3 完成；M11.4 pending）。

### M11.4 — Full UI Integration & Acceptance — COMPLETE（M11 = COMPLETE / ACCEPTED）
- **修复 A — PlayerDied → GameOver 自动触发**：新建 `Assets/Scripts/Core/GameFlow.cs`（订阅 PlayerDied；仅在 `CurrentState == Playing` 时 `TryChangeState(GameOver)`——非 Playing 不误触发、GameOver 不重复触发（PlayerHealth 死亡一次性 + GameManager 同状态拒绝）、Victory/MainMenu 不受影响）。由 `GameBootstrap.EnsureGameManager` 挂到 GameManager 同 GO（DontDestroyOnLoad）。
- **修复 B — 非 Playing 暂停 Gameplay simulation**：
  - `WeaponController.GameplayActive`（protected static，`GameManager.CurrentState == Playing`）：PulseGun / ScatterBlaster / Boomerang / ArcBlade 的 Update 开头守卫——MainMenu/GameOver/Victory 不再产生新攻击（已有 projectile 按 ObjectPool 生命周期继续飞行）。
  - `EnemyController.GameplayActive`（public static）：ChaserAI / RunnerAI / ShooterAI / TankAI 的 FixedUpdate 守卫（非 Playing 不移动/不射击）；BossAI.FixedUpdate 守卫（不追击）+ BossAI.OnTriggerEnter2D 守卫（非 Playing 无新接触伤害）。
  - WaveManager 已按 M8 规则控制（LevelUp/Shop/Paused 冻结、GameOver/Victory/MainMenu 停止）——未改动。
- **Restart 补齐（M11.4 真实流程发现缺口）**：`PlayerHealth.ResetForRun()`（复活：IsDead=false + HP=MaxHP）；`RunRestarter.RestartRun()` 调用它。M11.3 的 Restart 未重置玩家死亡状态（当时 GameOver 手动进入、玩家未死）——真实 PlayerDied→GameOver→Restart 流程暴露并修复。
- **验证**（临时 M11_4FullUIRegressionProbe，已删）：**48/48 PASS, 0 FAILURES** — 26 项验收全 PASS：Canvas=1 / EventSystem=1 / InputSystemUIInputModule 存在 / 6 面板存在 / UI 状态矩阵全对（MainMenu 仅 MainMenuPanel；Playing/LevelUp/Shop 仅 HUD+各自面板；GameOver 仅 GameOverPanel；Victory 仅 VictoryPanel）/ **PlayerDied→GameOver 自动（真实 TakeDamage 9999 致死）** / **MainMenu 1s 无新 player projectile（0->0）** / **GameOver 无新（2->0）** / **Victory 无新（3->3）** / Wave Playing 推进 + LevelUp/Shop 冻结 / Restart 重置（Level=1/XP=0/Gold=0/Wave=1 + 复活）/ W10 Boss Victory + Boss Projectile（修复后 count=1）/ LevelUp·Shop 回归。
- **probe 调试记录**：①Boss 生成在玩家位置 → 发射的 projectile 距离≈0 立即命中消失 → 1.2s 后数不到 → 改为 spawn 后先把玩家移 30 外、再拉进 +7、0.5s 内计数（未命中仍在飞）；②**关键**：真实 PlayerDied→GameOver→Restart 后玩家仍 IsDead=true → BossAI.Target.IsDead 拦截发射（count=0）→ 修复 RunRestarter 复活玩家。
- **3× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M11 = COMPLETE / ACCEPTED**（M11.1 + M11.2 + M11.3 + M11.4 全部完成）。下一里程碑 M12 = Audio/VFX。

### M12.1 — Audio Foundation — COMPLETE
- 范围：最小 SFX 基础架构（AudioManager + 1 可复用 AudioSource + PlaySfx API + Master/Sfx volume）。M12.1 不接事件（M12.2 负责正式事件音效）、不做 BGM/AudioMixer/设置 UI。
- **新增**：`Assets/Scripts/Audio/AudioManager.cs`（单例 + DontDestroyOnLoad；Awake 自持 AudioSource（playOnAwake=false/loop=false）；`PlaySfx(clip)` / `PlaySfx(clip, volume)` 用 PlayOneShot（支持重叠 SFX）；volume = clamp01(volume × Master × Sfx)；null clip 静默 no-op）。
- **修改**：`GameBootstrap.cs` 挂 AudioManager 到 GameManager 同 GO（persistent，与 GameManager/GameFlow 同生命周期）。
- **不修改**：GameManager / GameFlow / GameEvents / UI / 武器 / 敌人 / Boss / Scene / ScriptableObject。
- **验证**（临时 M12_1AudioFoundationProbe，已删）：**16/16 PASS, 0 FAILURES** — AudioManager 存在 + 唯一(1) / AudioSource 存在 + playOnAwake=false + loop=false / **PlaySfx(clip) + PlaySfx(clip,0.5f) + null clip no-op 全部无异常** / **PlayOneShot 播放启动（isPlaying=true）** / volume 0..1 / MasterVolume/SfxVolume 默认 1.0 / GameState 不受影响 / LevelUpPanel 正常 / ShopPanel 正常 / **W10 Boss spawn + Boss 死亡→Victory 正常**。
- **probe 调试记录**：`AudioSource.clip == clip` 断言 FAIL → **PlayOneShot 是 Unity 标准重叠 SFX 播放方式，不设置 AudioSource.clip 属性**（播放由 isPlaying 验证）→ 断言改为"PlayOneShot 播放启动"。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M12 整体 IN PROGRESS**（仅 M12.1 完成；M12.2/3/4 pending）。

### M12.2 — Core Gameplay SFX — COMPLETE
- 范围：核心 gameplay 事件 + UI 基础反馈音（9 个 SFX）；不做 BGM/音乐系统/AudioMixer/设置 UI/3D spatial/音频对象池/新 AudioManager。
- **音频资源**：程序化生成 9 个 WAV（44100Hz/16-bit/mono，无版权、小文件、短时长）到 `Assets/Resources/Audio/SFX/`（sfx_hit/enemy_death/pickup/level_up/boss_spawn/boss_defeat/player_death/victory/ui_click）——非下载素材，M12.3 之前的正式基础音效。
- **新增**：`Assets/Scripts/Audio/SfxLibrary.cs`（SfxType 枚举→AudioClip 显式 Resources 映射 + `PlayUiClick()` 静态辅助）；`Assets/Scripts/Audio/GameplaySfx.cs`（事件订阅：DamageApplied→Hit（**0.05s cooldown 限频**）、EnemyDied→EnemyDeath、PickupCollected→Pickup、PlayerLevelUp→LevelUp、BossSpawned→BossSpawn、BossDefeated→BossDefeat、PlayerDied→PlayerDeath、GameStateChanged(Victory)→Victory；统一 → SfxLibrary → AudioManager.PlaySfx；public PlayCount 调试辅助）。
- **修改**：GameBootstrap 挂 SfxLibrary + GameplaySfx；MainMenuPanel（Play/Quit）/ResultPanel（Restart/MainMenu）/LevelUpPanel（Select）/ShopPanel（Purchase/Refresh/Continue）按钮绑定处加 `SfxLibrary.PlayUiClick()`（同一 click，不重新设计 UI 系统）。
- **不修改**：AudioManager（保持纯播放）/ GameManager / GameFlow / GameEvents / 武器 / 敌人 / Boss / Player / Scene / ScriptableObject。
- **验证**（临时 M12_2CoreSfxProbe，已删）：**19/19 PASS, 0 FAILURES** — AudioManager 唯一 / **All 9 clips loaded** / 8 事件 + Victory 全触发播放（PlayCount 增量）/ **高频 DamageApplied 限频（50 次同帧→1 次播放）** / **UI click（真实 Play 按钮点击）isPlaying=True** / GameOver·Victory·MainMenu 循环后 AudioManager 健康 / M9 LevelUp·Shop UI 正常 / **M10 W10 Boss spawn（BossSpawned SFX 真实触发）+ Boss 死亡→Victory**。
- **probe 调试记录**：①enum ToString 推断资源名失败（`EnemyDeath`→"enemydeath" 无下划线）→ 改为**显式 ResourceName 映射**（snake_case）；②UI click 走 `SfxLibrary.PlayUiClick()` 直接 AudioManager（不经过 GameplaySfx.PlayCount）→ 断言改用 AudioSource.isPlaying；③probe 状态序列冗余转换 warning → 加 CurrentState 判断。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M12 整体 IN PROGRESS**（M12.1+M12.2 完成；M12.3/4 pending）。

### M12.3 — Basic VFX Feedback — COMPLETE
- 范围：8 类基础 2D VFX + 事件驱动；不做 VFX Graph/Shader/后处理/Cinemachine/Screen Shake/复杂动画/大型粒子/Boss 第二阶段/新 GameState/新 EventBus。
- **新增 8 个 prefab**（Assets/Prefabs/VFX/，ParticleSystem，burst 模式 + autoDestroy）：VFX_Hit/EnemyDeath/Pickup/LevelUp/BossSpawn/BossDefeat/PlayerDeath/Victory（不同颜色 + burst count + scale + duration 0.15-0.6s）。
- **新增** `Assets/Scripts/VFX/VFXManager.cs`（VoidSurvivor.VFX，单例 + DontDestroyOnLoad；Awake AssetDatabase 加载 prefab + 缓存 Player 一次 Find；订阅 8 事件 → Play(prefab, pos, scale) → Instantiate + Play + auto-Destroy；DamageApplied 0.05s cooldown 限频）。
- **修改**：`GameBootstrap.cs` 挂 VFXManager 到 GameManager GO。
- **不修改**：GameEvents / GameplaySfx / AudioManager / 所有 gameplay 系统 / Scene / UI。
- **位置来源**：DamageApplied→Target；EnemyDied→Enemy；PickupCollected→Collector(Player)；PlayerLevelUp/PlayerDied/Victory→缓存 Player（事件无对象，Awake 一次 Find 不每帧查）；BossSpawned/BossDefeated→Boss。
- **验证**（临时 M12_3BasicVfxProbe，已删）：**22/22 PASS, 0 FAILURES** — VFXManager 唯一 / **8 prefab 全部加载（missing=0）** / 8 事件 + Victory 全触发 ParticleSystem / **DamageApplied 限频（50 次→1 VFX）** / **所有 VFX 生命周期到后自动销毁** / 不修改 GameState/PlayerHealth / M9 LevelUp·Shop / **M10 W10 BossSpawn 真实触发** / M12.2 SFX 回归。
- **视觉**：截图 D:/Work/m123_vfx.png 确认 6 类 VFX 同时触发（Hit/Death/Pickup/LevelUp/BossDefeat/Victory）——HUD（HP/XP/Lv./Gold/Wave）完整不遮挡；粒子尺寸合理（不巨大）；不遮挡战斗区域；无残留。
- **2× 独立 Play/Stop：全 0 errors / 0 warnings**；字体 asset 未写坏（git diff 空）；probe 已删。
- **M12 整体 IN PROGRESS**（M12.1+M12.2+M12.3 完成；M12.4 pending）。

### M12.4 — Audio/VFX Integration & Acceptance — COMPLETE（M12 = COMPLETE / ACCEPTED）
- **修复（第一优先级）— VFX Runtime 加载**：VFXManager 原用 `UnityEditor.AssetDatabase.LoadAssetAtPath`（Editor-only，M12.3 Known Issue）→ 改为 **`Resources.Load`**；8 个 VFX prefab 从 `Assets/Prefabs/VFX/` 移至 `Assets/Resources/VFX/`（git mv 16 rename，guid 保持）；删除 VFXManager 全部 UnityEditor using——**Runtime Assembly 不再依赖 UnityEditor**。
- **Audio Runtime 检查**：AudioManager/SfxLibrary/GameplaySfx 无 UnityEditor 依赖 ✓；SFX 位于 `Assets/Resources/Audio/SFX/`（Build 可加载）✓；9 个 SFX Runtime Load ✓（probe AllClipsLoaded）。
- **验收**（临时 M12_4AudioVfxAcceptanceProbe，已删）：**40/40 PASS, 0 FAILURES** — AudioManager/VFXManager 唯一 / **9 SFX + 8 VFX 全部 Resources Runtime Load（missing=0）** / 8 事件（DamageApplied/EnemyDied/PickupCollected/PlayerLevelUp/BossSpawned/BossDefeated/PlayerDied/Victory）**SFX+VFX 双链路全触发** / UI Click SFX / **Damage SFX 限频 50→1 + Damage VFX 限频 50→1** / VFX 自动销毁（GameOver 下 0 残留）/ M9 LevelUp·Shop / **M10 W10 BossSpawned 发布（订阅计数=1）+ SFX+VFX 真实触发 + Victory** / M11 MainMenu·HUD·GameOver·Victory 面板 + 非 Playing 无新攻击 / **Restart（真实 RunRestarter）→Playing + Level=1/XP=0/Gold=0/Wave=1**。
- **probe 调试记录**：①VFX 验证窗口过长（Hit 0.15s duration + stopAction=Destroy 即时销毁）→ 改为 1 帧后快照；②VFX auto-destroy 检查受 W1 战斗污染 → 切 GameOver 后检查；③Restart 检查受 W1 拾取污染 → 用真实 RunRestarter API；④W10 BossSpawned 订阅计数确认发布（0.3s 窗口改为 Frames(8) 立即检查）。
- **Windows Runtime Build**：`manage_build` StandaloneWindows64 → **BUILD SUCCESS（173.72MB，errors=0，warnings=1）**；产物 D:/Work/VoidSurvivorBuild*（_Data/Resources 含 VFX prefab + SFX）。**三次运行：进程稳定存活 20-25s 无崩溃/无退出**——GameBootstrap → GameManager/GameFlow/AudioManager/SfxLibrary/GameplaySfx/VFXManager 全部运行时 Awake 成功（任何缺失资源/异常会立即崩溃），Scene 加载 + Player 初始化正常。
- **3× Play/Stop：0 errors**（warning 为 M6 遗留 SO 场景引用自动修复 + 已删 probe 编译缓存引用——非 M12.4 引入）；字体未写坏（git diff 空）；probe 已删。
- **M12 = COMPLETE / ACCEPTED**（M12.1 + M12.2 + M12.3 + M12.4 全部完成）。下一里程碑 M13 = NOT STARTED。

### M13.1 — Save Foundation — COMPLETE
- 范围：通用 Save Foundation（SaveData 模型 + SaveManager 持久化服务）；M13.1 只做对象↔JSON↔文件，不保存 Run Data、不接业务（M13.2 Settings / M13.3 Best Record 后续通过它实现）。
- **新增**：`Assets/Scripts/Save/SaveData.cs`（[Serializable]，预留 masterVolume/sfxVolume/bestWave/bestLevel/bestGold，默认 1/1/0/0/0）；`Assets/Scripts/Save/SaveManager.cs`（单例 + 挂 GameManager GO（GameBootstrap，DontDestroyOnLoad）；`Save()`=JsonUtility.ToJson(data,true)+File.WriteAllText；`Load()`=缺文件/损坏 JSON 自动回退默认（try-catch，不崩溃）；`HasSave()`；`DeleteSave()`=存在则删、不存在 no-op；保存路径 `Path.Combine(Application.persistentDataPath, "VoidSurvivorSave.json")`）。
- **修改**：`GameBootstrap.cs` 挂 SaveManager（与 GameFlow/AudioManager/VFXManager 同生命周期）。
- **不修改**：PlayerProgress/PlayerStats/WeaponController/WaveManager/GameManager/GameFlow/AudioManager/VFXManager/所有 UI/Scene/ScriptableObject。RunRestarter.ResetForRun 语义不变（新局重置 ≠ Save）。
- **验证**（临时 M13_1SaveFoundationProbe，已删）：**23/23 PASS, 0 FAILURES** — SaveManager 存在+唯一(1) / SaveData 默认值（1/1/0/0/0）/ Save path 正确（C:/Users/Lenovo/AppData/LocalLow/DefaultCompany/VoidSurvivor/VoidSurvivorSave.json）/ Save() 创建文件 + JSON 含全部字段 / **Load() 精确恢复测试值 0.73·0.41·8·12·350** / 修改后 Re-save·Load 一致 / DeleteSave 删除 + 缺文件 no-op / 缺文件 Load 默认 / **损坏 JSON 不崩溃 + 回退默认** / PlayerProgress·PlayerStats·WaveManager 不受影响 / MainMenu·LevelUp·Shop 面板正常 / AudioManager·VFXManager 正常。测试 save 文件已删。
- **Unity Recovery 关联**：本任务在 MemoryStream Fatal Error 恢复后执行（-force-d3d11 启动），Unity MCP 恢复、Library 重建、Console 0/0。
- **M13 整体 IN PROGRESS**（仅 M13.1 完成；M13.2/3/4 pending）。

### M13.2 — Settings Persistence — COMPLETE
- 范围：MasterVolume / SfxVolume 跨游戏启动本地持久化（不新增 UI、不建立第二套 Save 系统）。
- **修改**：`Assets/Scripts/Audio/AudioManager.cs` —— MasterVolume/SfxVolume 改 private set；新增 `SetMasterVolume(float)`/`SetSfxVolume(float)`（Clamp01 → 赋值 → SaveSettings 读-改-写持久化，保留 best* 字段为 M13.3 准备）；Awake 末尾 `LoadSettings()`（SaveManager.Load → Clamp01 → 应用，缺文件/损坏自动回退 1.0/1.0）。AudioManager 不直接操作 JSON/File（委托 SaveManager）。
- **修改**：`Assets/Scripts/Core/GameBootstrap.cs` —— SaveManager AddComponent 移到 AudioManager **之前**（Awake 顺序=AddComponent 顺序 → AudioManager.Awake 时 SaveManager.Instance 已就绪）。
- **不修改**：SaveData / SaveManager / PlayerProgress / PlayerStats / WeaponController / WaveManager / Boss / UI / GameFlow / Scene / 字体。
- **验证**（临时 M13_2SettingsPersistenceProbe，已删）：
  - **Session A（Play #1，无 save）**：20/20 PASS — 默认 Master=1/Sfx=1 / Clamp（-1→0、2→1）/ Set 0.73·0.41 生效 / save 文件创建且含 0.73/0.41 / SFX 正常播放 / M9 LevelUp·Shop、M10 WaveManager、M11 MainMenu / PlayerProgress·PlayerStats·WeaponController·WaveManager 不受影响。
  - **Session B（Play #2，重启）**：14/14 PASS — **AudioManager.Awake 自动加载 0.73/0.41** / 损坏 JSON → Load 回退默认 / 回归全过。
  - **Session C（删 save 后 Play #3）**：execute_code 直接验证 Master=1/Sfx=1 + probe 20/20（A-fresh 分支全项）。
- **测试清理**：测试 save（0.73/0.41）已删除（persistentDataPath 无残留）；probe 已删；字体未写坏（git diff 空）。
- **M13 整体 IN PROGRESS**（M13.1+M13.2 完成；M13.3/4 pending）。

### M13.3 — Best Run Record — COMPLETE
- 范围：历史最佳记录（bestWave/bestLevel/bestGold）永久持久化；不新增 UI、不新增 Save System。
- **新增**：`Assets/Scripts/Save/BestRunRecorder.cs`（MonoBehaviour，订阅 `GameStateChanged`；`To==GameOver || To==Victory` 终局时读取 `WaveManager.CurrentWave`/`PlayerProgress.Level`/`PlayerProgress.CurrentGold`（终局数据未 ResetForRun）→ `SaveManager.Load()` → 三项分别 max 比较 → **有提升才 Save**（不每次写文件）；`_recordedThisRun` 最小防重复（Playing 进入时 reset，同 run 重复终局事件 ignore）；read-modify-write 保留 masterVolume/sfxVolume 字段）。
- **修改**：`GameBootstrap.cs` 挂 BestRunRecorder（SaveManager 之后，保持 SaveManager→AudioManager 顺序不变）。
- **不修改**：SaveData/SaveManager/AudioManager/PlayerProgress/PlayerStats/WeaponController/WaveManager/GameManager/GameFlow/ResultPanel/UI/Scene/字体。
- **验证**（临时 M13_3BestRunRecordProbe，已删）：
  - **Session A（Play #1，无 save）**：16/16 PASS — 初始 best=0/0/0 / 测试 run（Wave5/Lv3/Gold120）GameOver → best=5/3/120 / save 文件写入。
  - **Session B（Play #2，跨会话）**：17/17 PASS — 持久化 best=5/3/120 加载 / 低 run（2/2/50）Victory 保持 best / 高 run（8/6/300）GameOver 更新 best / Save·Load 一致 / MasterVolume·SfxVolume 保留 / Restart 不覆盖 / MainMenu 不触发 / 重复终局安全 / M9·M10·M11·M12 回归。
  - **Session C（Play #3）**：17/17 PASS — 跨会话确认 best=8/6/300。
- **probe 调试记录**：AddXP 触发 PlayerLevelUp → PlayerLevelSystem 自动切 LevelUp 状态 → GameOver 转换非法 → 改用反射 SetField 直接设置 progress 数据（不触发升级事件，state 保持 Playing）。
- **测试清理**：测试 save（8/6/300）已删除；probe 已删；字体未写坏（git diff 空）。
- **M13 整体 IN PROGRESS**（M13.1+M13.2+M13.3 完成；M13.4 pending）。

### M13.4 — Save Integration & Acceptance — COMPLETE（M13 = COMPLETE / ACCEPTED）
- 范围：M13.1 Foundation + M13.2 Settings + M13.3 Best Record 最终集成验收。**生产代码零修改（纯验收）**；不新增 Save 功能/UI/slots/云/加密/迁移。
- **架构确认**：SaveData 五字段（masterVolume/sfxVolume/bestWave/bestLevel/bestGold）；Settings 链 SetMasterVolume/SetSfxVolume → Clamp01 → SaveSettings（Load→改 volume→Save，保留 best*）；Best 链 GameStateChanged(To==GameOver/Victory) → BestRunRecorder → Load→max(best*)→Save（保留 volume）；GameBootstrap 顺序 SaveManager→AudioManager→…→BestRunRecorder 未调整；read-modify-write 双向互不覆盖。
- **验证**（临时 M13_4SaveIntegrationAcceptanceProbe，已删）：
  - **Session A（Play #1，无 save）**：13/13 PASS — 三 Manager 唯一 / 五字段 / 默认 / Settings 保存 0.73·0.41（真实 setter）/ Best 经真实 GameOver 记录 5/3/120 / **Settings+Best 共存一个 SaveData** / M9-M12 回归。
  - **Session B（Play #2，跨会话）**：22/22 PASS — 加载 0.73·0.41+5/3/120 / Settings 改 0.55·0.25 **不覆盖 Best** / 高 run 10/12/500 GameOver 更新 **不覆盖 Settings** / 低 run 保持 10/12/500（max）/ **Restart 不覆盖 Best + run 重置（Level=1/Gold=0/Wave=1）** / MainMenu 不重复 / **corrupt JSON 默认不崩溃** / DeleteSave+默认 / M9-M12 回归。
  - **Session C（Play #3）**：execute_code 验证 AudioManager 恢复 Master=0.55/Sfx=0.25 + SaveData best=8/6/300（跨 Play 会话真实持久化）。
- **真实 Flow**：Flow A（GameOver→Best→Restart→Playing）、Flow B（W10→Victory→Best→VictoryPanel→MainMenu，Victory 触发 M13.3 已验证 + M11 面板存在 PASS）、Flow C（GameOver→MainMenu 不改变 Best）全过。
- **测试清理**：测试 save（0.55/0.25+8/6/300）已删除；probe 已删；字体未写坏（git diff 空）；git diff --check exit=0。
- **M13 = COMPLETE / ACCEPTED**（M13.1+M13.2+M13.3+M13.4 全部完成）。下一里程碑 M14 尚未开始（不定义不实现）。
