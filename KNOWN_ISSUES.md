# Void Survivor — Known Issues

## Current
1. GitHub remote repository URL not yet configured.
   - Impact: No remote backup / showcase yet.
   - Status: Open. Plan: add remote when GitHub repo is created (before M17 at the latest).
2. Web build has not yet been tested.
   - Impact: Web is a first-class deliverable; late discovery of Web-only issues would be costly.
   - Status: Open. Plan: first Web build smoke test during M2 or M3, earlier than M16.

## Resolved (M1 / M2 / M3 / M3-bugfix)
- Unity project not yet created → Created (Unity 6000.3.21f1).
- Actual repository path not recorded → Recorded in PROJECT_CONTEXT.md (D:\Work\UnityProject\VoidSurvivor).
- Unity MCP connection unverified → Verified 2026-08-14 (CoplayDev unity-mcp via WorkBuddy).
- Core framework missing → Implemented in M2 (GameManager state machine, EventBus, GameEvents, SceneFlow).
- Player system missing → Implemented in M3 (PlayerController/PlayerStats/PlayerHealth/CameraFollow, Player prefab in SC_Main).
- Player jitter while moving → Fixed (Rigidbody2D interpolation = Interpolate; see DEVELOPMENT_LOG).
- InputActionReference not persisted in scene/prefab (would break movement after reopening the project) → Fixed by switching PlayerController to a serializable InputActionAsset reference.
- Small movement-range perception / apparent pull-back → No code pulled the player back; fixed by removing jitter and widening the camera viewport (orthographicSize 5 → 8).

## Observations (no action needed now)
1. Transient "The referenced script (Unknown) on this Behaviour is missing!" console pairs appear during script recompile cycles (observed M1–M3 right after script changes/removals).
   - Impact: None observed; scenes/assets verified clean, all script GUIDs resolve, stable states are 0-error.
   - Status: Documented as recompile-cycle noise. Revisit only if it appears outside compile cycles.
2. mcp execute_code (CodeDom backend) loads its own copy of Assembly-CSharp, so static fields of project types read through it are unreliable.
   - Impact: Verification scripts must use UnityEngine object queries instead of project-type statics.
   - Status: Documented; workaround in place (reflection over real instances obtained via UnityEngine queries works).
3. mcp find_gameobjects does not return objects in the DontDestroyOnLoad scene during play mode.
   - Impact: Use execute_code + UnityEngine queries to verify DontDestroyOnLoad objects.
   - Status: Documented; workaround in place.
4. mcp manage_gameobject resolves project component types only by fully-qualified name (e.g. "VoidSurvivor.Player.PlayerStats").
   - Impact: Short names fail with "not found" — always pass full names.
   - Status: Documented; workaround in place.
5. mcp execute_code (CodeDom) has a restricted UnityEditor API surface (e.g. AssetDatabase.GetAtPath unavailable); LoadAssetAtPath/Refresh/ImportAsset, SerializedObject, InputActionReference.Create are available.
   - Impact: Asset/importer tweaks via execute_code are limited; use file-level or MCP manage_asset operations instead.
   - Status: Documented; workaround in place.

## Policy
Record unresolved implementation issues here rather than leaving them only in chat messages.
