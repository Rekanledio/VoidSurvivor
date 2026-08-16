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
