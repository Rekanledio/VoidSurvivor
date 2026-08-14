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
