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
