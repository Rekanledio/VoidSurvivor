# Void Survivor — Save Context

## Last Updated
2026-08-14 (M3 complete + M3 bug fix)

## Current Phase
Phase 1 — Core framework development

## Current Milestone
M4 — Enemy System

## Completed
- M0: concept, MVP scope, delivery strategy, documentation strategy.
- M1: Unity project (6000.3.21f1) verified; Git initialized; Assets skeleton; SC_Main scene; minimal Core entry; Unity MCP verified.
- M2: Core Framework — GameManager state machine (TryChangeState + legal-transition table + GameStateChanged), EventBus (type-safe generic static), GameEvents, SceneFlow/SceneIds, lifecycle docs. 33/33 smoke checks passed.
- M3 (this session): Player System.
  - PlayerController: reads Move from the existing InputSystem_Actions asset (Player/Move, WASD + arrows + gamepad, reused — no new input config). Movement math via public static helpers (NormalizeMoveInput caps diagonal magnitude at 1 → consistent diagonal speed; analog sticks keep partial range). Physics movement via Rigidbody2D.MovePosition (no Transform writes). Configurable arena bounds clamp (default ±20,0,20,0). Movement stops when dead. No GetComponent/Find in FixedUpdate (all cached in Awake).
  - PlayerStats: all 10 MVP stats from GAME_DESIGN.md (MaxHP/HPRegen/MoveSpeed/Damage/AttackSpeed/CritChance/CritDamage/Range/PickupRange/Armor), base values + read accessors only (modifiers deferred to M9).
  - PlayerHealth: CurrentHP/MaxHP/IsDead; TakeDamage (flat reduction: max(0, damage - Armor), armor from PlayerStats); Heal/FullHeal; HP clamped to [0, MaxHP]; death fires once (IsDead guard) and publishes PlayerDied via EventBus. No Game Over flow.
  - CameraFollow: exponential smoothing (frame-rate independent, followSpeed=8), X/Y follow with Z kept at offset -10, optional bounds reserved for the real arena. No Cinemachine.
  - GameEvents: added PlayerDied (empty payload).
- Assets: Assets/Prefabs/Player.prefab (7 components), Assets/Art/PlayerPlaceholder.png (64x64 cyan circle, Python-generated, auto-imported as Sprite). Player instance placed in SC_Main; Main Camera has CameraFollow targeting Player.
- M3 verification: in-play smoke test (temporary PlayerSmokeTest.cs) — 29 checks, 0 failures (stats read, TakeDamage/Heal/FullHeal, HP clamps, single death + event count, movement math, bounds math). Dynamic play checks via MCP: camera converged smoothly to a teleported target (30,30 → target (5,5) → cam (5,5) after 2s, no teleport snap), full component set verified on Player GO and prefab, clean session shows hp=100/100, dead=False, moveInput=(0,0). Temp test code deleted.

## In Progress
- Nothing. M3 (incl. bug fix) is fully complete.

## M3 Bug Fix (2026-08-14)
- Reported: (1) small movement range + apparent pull-back near edges; (2) jitter/blur when holding WASD.
- Root causes (verified): (2) Rigidbody2D.interpolation was None → physics-stepped positions vs per-frame camera smoothing. (1) no pull-back code exists (verified in play); perception came from jitter + small viewport (orthographicSize 5). Latent defect found: InputActionReference created at runtime was NOT persisted to scene/prefab (would break movement after editor restart).
- Fixed: Rigidbody2D.interpolation = Interpolate; Camera orthographicSize 5 → 8; PlayerController switched to serializable `InputActionAsset` + `FindAction("Move")`.
- Verified: W/A/D covered -20..+20 through center (no pull-back), bounds hold, camera follows; 0 console errors/warnings. Files: PlayerController.cs, Player.prefab, SC_Main.unity.

## Modified / Added Files (M3 bug fix)
- Assets/Scripts/Player/PlayerController.cs (InputActionAsset reference)
- Assets/Prefabs/Player.prefab (interpolation + inputActions persisted)
- Assets/Scenes/SC_Main.unity (camera size 8)
- PROJECT_CONTEXT.md / TASKS.md / SAVE_CONTEXT.md / DEVELOPMENT_LOG.md / KNOWN_ISSUES.md / DECISIONS.md (D009) (synced)

## Modified / Added Files (M3)
- Assets/Scripts/Player/PlayerController.cs (new)
- Assets/Scripts/Player/PlayerStats.cs (new)
- Assets/Scripts/Player/PlayerHealth.cs (new)
- Assets/Scripts/Player/CameraFollow.cs (new)
- Assets/Scripts/Core/GameEvents.cs (added PlayerDied)
- Assets/Prefabs/Player.prefab (new)
- Assets/Art/PlayerPlaceholder.png (new, placeholder art)
- Assets/Scenes/SC_Main.unity (Player instance + CameraFollow on Main Camera)
- ProjectSettings/EditorBuildSettings.asset (build scene now correctly points to SC_Main; M1 MCP change flushed by editor)
- PROJECT_CONTEXT.md / TASKS.md / SAVE_CONTEXT.md / DEVELOPMENT_LOG.md / KNOWN_ISSUES.md / MILESTONES.md / ARCHITECTURE.md / DECISIONS.md (synced)

## Test Results (M3)
- MCP script validation: PlayerController/PlayerStats/PlayerHealth/CameraFollow — 0 errors, 0 warnings.
- In-play smoke test: 29/29 PASS, 0 FAILURES.
- Dynamic play checks: camera smooth convergence verified (no snap); component set verified on scene GO and prefab (Transform/SpriteRenderer/Rigidbody2D/CircleCollider2D/PlayerStats/PlayerHealth/PlayerController).
- Clean play/stop after test removal: 0 errors, 0 warnings; player hp=100/100, dead=False.

## MCP Status
- Connected. Used in M3: manage_gameobject (create/modify with namespaced component types), manage_prefabs (create_from_gameobject, get_info), manage_scene (save, get_hierarchy), manage_asset (import), manage_editor (play/stop), read_console, execute_code, validate_script.
- Learned: MCP component resolution requires fully-qualified names for project scripts (e.g. "VoidSurvivor.Player.PlayerStats"); short names fail.
- Learned: mcp execute_code (CodeDom) has a restricted API surface — AssetDatabase.GetAtPath is unavailable; AssetDatabase.LoadAssetAtPath/Refresh/ImportAsset, SerializedObject, InputActionReference.Create work.

## Next Step
M4 — Enemy System: enemy base framework, 4 enemy types with simple AI (Chaser/Runner/Shooter/Tank), minimal spawn entry. Full wave logic stays in M8.

## Important Constraints
- Do not expand MVP scope.
- Do not rely only on chat history.
- Update project context documents after milestone changes.
- Test Web builds early.

## Known Issues
- GitHub remote repository URL not yet configured (local-only Git for now).
- Web build not yet tested.
- "Referenced script (Unknown) missing" console pairs appear transiently during script recompile cycles; stable states are clean. See KNOWN_ISSUES.md.

## M3 Bug Fix #2 (2026-08-14) — final root cause
- Reported: "small movement range + return to center on release" (jitter already fixed).
- Verified (deterministic, in-assembly probe): player world coordinates never returned to center — W/A/D exact 5 u/s, position kept after release, crossing center OK, camera follows perfectly (player (-2.5,5) ↔ cam (-2.5,5,-10)).
- Root cause: SC_Main was an empty scene (no visual reference). With camera-follow, movement is invisible on screen → perceived as small range + return to center.
- Fix: added Ground reference (Assets/Art/GroundPlaceholder.png, grid, sortingOrder -10, scale 50). No player/camera logic changed.
- Files: SC_Main.unity (+Ground), Assets/Art/GroundPlaceholder.png (+meta). Temp AutoProbe deleted.
- Verification: probe identical across 3 Play/Stop cycles; final clean play/stop 0 errors / 0 warnings.

## M3 Full Review + Visual Scale (2026-08-14)
- Review: Player/Core/Scene/Prefab/Assets all clean; no temp residue; 0 required fixes (Post-MVP note: EventBus static reset relies on domain reload).
- Visual scale: Camera orthographicSize 7, Player scale 1.5, Ground scale 50 with 8px grid (4-unit spacing); bounds +-20 & MoveSpeed 5 unchanged.
- Regression: 15/15 PASS (movement/speed/diagonal/bounds/release-keep/camera/health/damage/death); final play/stop 0 errors / 0 warnings.
