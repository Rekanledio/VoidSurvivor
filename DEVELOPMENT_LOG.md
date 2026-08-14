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
