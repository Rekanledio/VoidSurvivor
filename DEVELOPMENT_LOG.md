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
