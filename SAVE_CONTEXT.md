# Void Survivor — Save Context

## Last Updated
2026-08-14 (M1 complete)

## Current Phase
Phase 1 — Core framework development

## Current Milestone
M2 — Core Framework

## Completed
- Game concept, MVP scope, delivery strategy finalized (M0).
- Project documentation placed in repository root (M0).
- Unity project confirmed: Unity 6000.3.21f1 (6.3 LTS), project opens normally (M1).
- Git repository initialized on branch `main` with Unity-specific `.gitignore` (Library/Temp/Logs/Obj/UserSettings excluded) (M1).
- Folder structure created per ARCHITECTURE.md: Assets/{Art, Audio, Materials, Prefabs, Scenes, Scripts, ScriptableObjects, Settings}; Scripts subfolders Core/Player/Enemy/Weapons/Combat/Roguelite/Shop/Wave/UI/Audio/Save/Utilities; ScriptableObjects subfolders Weapons/Enemies/Upgrades/Waves/Characters (M1).
- Input System verified as the only active input handler (activeInputHandler=1) (M1).
- Project defaults to 2D behavior mode; URP 17.3.0 active; Linear color space (M1).
- Base scene created: Assets/Scenes/SC_Main.unity with orthographic Main Camera (size 5, z=-10); set as the only scene in Build Settings (M1).
- Minimal Core entry created: GameState enum, GameManager singleton (DontDestroyOnLoad), GameBootstrap (RuntimeInitializeOnLoadMethod). No gameplay logic (M1).
- Unity MCP (CoplayDev unity-mcp) verified: scene read/create/modify, build settings update, play/stop, console read (M1).
- Play-mode test passed: GameManager auto-created into DontDestroyOnLoad scene; console free of errors after final verification (M1).

## In Progress
- Nothing. M1 is fully complete.

## Modified / Added Files (M1)
- .gitignore (new)
- Assets/Scripts/Core/GameState.cs (new)
- Assets/Scripts/Core/GameManager.cs (new)
- Assets/Scripts/Core/GameBootstrap.cs (new)
- Assets/Scenes/SC_Main.unity (new)
- ProjectSettings/EditorBuildSettings.asset (SC_Main replaces SampleScene in build)
- Folder skeleton under Assets/ (new, with .meta files)
- PROJECT_CONTEXT.md / TASKS.md / SAVE_CONTEXT.md / DEVELOPMENT_LOG.md / KNOWN_ISSUES.md / MILESTONES.md (synced)

## Test Results (M1)
- Script validation: GameState/GameManager/GameBootstrap — 0 errors, 0 warnings.
- Play mode: entered/exited cleanly; GameManager present with GameManager component in DontDestroyOnLoad scene.
- Console after final play/stop cycle: 0 errors, 0 warnings.

## MCP Status
- Connected. Verified operations: telemetry_ping, get_active scene, get_hierarchy, find_gameobjects, manage_gameobject (create/modify), manage_scene (create/save/build settings), manage_editor (play/stop), execute_code, read_console.
- Note: mcp execute_code uses CodeDom compiler, which loads a separate copy of Assembly-CSharp — static fields of project types are not trustworthy through it; verify runtime state via UnityEngine object queries instead.

## Next Step
M2 — Core Framework: GameState transition API, core event bus, scene flow entry, shared utilities foundation. Do NOT start gameplay systems (Player/Enemy/Weapon/Wave/Shop) in M2.

## Important Constraints
- Do not expand MVP scope.
- Do not rely only on chat history.
- Update project context documents after milestone changes.
- Test Web builds early.

## Known Issues
- GitHub remote repository URL not yet configured (local-only Git for now).
- Web build not yet tested.
- One transient "referenced script missing" console noise pair observed during first script compilation; did not reproduce afterwards. See KNOWN_ISSUES.md.
