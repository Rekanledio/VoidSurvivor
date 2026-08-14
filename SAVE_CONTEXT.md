# Void Survivor — Save Context

## Last Updated
2026-08-14 (M2 complete)

## Current Phase
Phase 1 — Core framework development

## Current Milestone
M3 — Player System

## Completed
- M0: Game concept, MVP scope, delivery strategy, documentation strategy finalized.
- M1: Unity project (6000.3.21f1) verified; Git initialized with Unity .gitignore; Assets folder skeleton per ARCHITECTURE.md; SC_Main scene with orthographic camera; minimal Core entry; Unity MCP verified; clean play-mode test.
- M2 (this session): Core Framework implemented and verified.
  - GameManager: centralized state owner. `TryChangeState` is the only public entry; legal-transition table (MainMenu→Playing; Playing→Paused/LevelUp/Shop/GameOver/Victory; Paused→Playing; LevelUp→Playing; Shop→Playing; GameOver→MainMenu; Victory→MainMenu); redundant/illegal transitions rejected with warning; `GameStateChanged` published via EventBus.
  - EventBus: type-safe generic static bus (Subscribe/Unsubscribe/Publish/Clear), struct events (no boxing), main-thread only.
  - GameEvents: `GameStateChanged` — the only core event in M2 (gameplay events belong to their milestones).
  - SceneFlow + SceneIds: minimal scene-load API wrapping SceneManager (MainMenu/Gameplay/Result constants, same-scene reload guard). No scene assets created.
  - Lifecycle: GameBootstrap (BeforeSceneLoad) → GameManager.Awake (singleton + DontDestroyOnLoad) → scene Awake/Start. EventBus is lazy static, no init required.
  - No gameplay systems introduced. No new third-party dependencies.
- M2 verification: in-play smoke test (temporary CoreSmokeTest.cs) ran 33 checks — 0 failures. Covered GameState flow (MainMenu→Playing→Paused→Playing→GameOver→MainMenu→Playing→Victory→MainMenu), illegal/redundant transition rejection, GameStateChanged event order + unsubscribe, EventBus subscribe/publish/unsubscribe/duplicate/Clear, SceneFlow constants + same-scene guard. Temp test code deleted after verification.

## In Progress
- Nothing. M2 is fully complete.

## Modified / Added Files (M2)
- Assets/Scripts/Core/EventBus.cs (new)
- Assets/Scripts/Core/GameEvents.cs (new)
- Assets/Scripts/Core/SceneFlow.cs (new)
- Assets/Scripts/Core/GameManager.cs (state machine + transition table + GameStateChanged broadcast)
- Assets/Scripts/Core/GameState.cs (docs updated; enum unchanged)
- Assets/Scripts/Core/GameBootstrap.cs (lifecycle docs updated)
- .gitignore (+ .workbuddy/ local tool data)
- PROJECT_CONTEXT.md / TASKS.md / SAVE_CONTEXT.md / DEVELOPMENT_LOG.md / KNOWN_ISSUES.md / MILESTONES.md / ARCHITECTURE.md / DECISIONS.md (synced)

## Test Results (M2)
- Script validation (MCP validate_script, standard): EventBus/GameEvents/SceneFlow/GameManager — 0 errors, 0 warnings each.
- In-play smoke test (temporary script): 33/33 PASS, 0 FAILURES.
- Clean play/stop after test removal: 0 errors, 0 warnings; GameManager present in DontDestroyOnLoad with CurrentState=MainMenu (verified via MCP execute_code).
- Compile check after temp-test deletion: no errors.

## MCP Status
- Connected. Used in M2: manage_asset (reimport), manage_editor (play/stop), read_console, execute_code, validate_script, find_gameobjects.
- Known limitation: find_gameobjects does not return objects in the DontDestroyOnLoad scene during play mode; use execute_code + UnityEngine queries instead.
- Note: mcp execute_code (CodeDom) loads a separate copy of Assembly-CSharp — project-type statics read through it are unreliable. Reflection over a real instance obtained via UnityEngine queries works correctly.

## Next Step
M3 — Player System: player movement (Input System), stats foundation, health, camera follow. Core framework from M2 is the stable base.

## Important Constraints
- Do not expand MVP scope.
- Do not rely only on chat history.
- Update project context documents after milestone changes.
- Test Web builds early.

## Known Issues
- GitHub remote repository URL not yet configured (local-only Git for now).
- Web build not yet tested.
- "Referenced script (Unknown) missing" console pairs appear transiently during script recompile cycles (observed M1 and M2); stable states are clean. See KNOWN_ISSUES.md.
