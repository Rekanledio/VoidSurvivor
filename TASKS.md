# Void Survivor — Tasks

## Completed: M0 — Project Documentation Initialization (2026-08-14)

### M0.1 Documentation
- [x] Define project scope
- [x] Define MVP boundaries
- [x] Define long-term context strategy
- [x] Add all project documents to the real repository
- [x] Review documents after first Unity project creation

### M0.2 Tooling
- [x] Verify WorkBuddy project workflow
- [x] Verify Unity MCP connection for this project
- [x] Verify Git workflow
- [x] Verify Unity 6.3 LTS version

## Completed: M1 — Unity Project Initialization (2026-08-14)
- [x] Create Unity project (created manually by user, Unity 6000.3.21f1)
- [x] Initialize Git repository (main branch, Unity .gitignore)
- [x] Create folder structure (per ARCHITECTURE.md)
- [x] Create initial scene (Assets/Scenes/SC_Main.unity, orthographic camera)
- [x] Configure Input System (activeInputHandler=1, Input System only)
- [x] Configure basic project settings (2D default behavior mode, URP, Linear color space)
- [x] Connect Unity MCP (CoplayDev unity-mcp, verified via WorkBuddy)
- [x] Verify WorkBuddy can read and modify project (scene created and modified via MCP)
- [x] Run first successful build/test (play mode test passed, no console errors)

## Completed: M2 — Core Framework (2026-08-14)
- [x] GameState transition API on GameManager (TryChangeState, legal-transition table, GameStateChanged broadcast)
- [x] Core event bus for cross-system communication (EventBus: Subscribe/Unsubscribe/Publish/Clear, struct events)
- [x] Scene flow entry for MainMenu / Gameplay / Result (SceneFlow + SceneIds, same-scene reload guard)
- [x] Lifecycle documented (GameBootstrap → GameManager.Awake → scene Awake/Start; EventBus lazy static)
- [x] No gameplay systems introduced (Player/Enemy/Weapon/Wave/Shop untouched)
- [x] Verified: in-play smoke test 33 checks / 0 failures; GameManager, EventBus, SceneFlow covered; temp test code removed

## Completed: M3 — Player System (2026-08-14)
- [x] Player movement via Input System (reused existing InputSystem_Actions Player/Move, WASD + arrows + gamepad)
- [x] 8-way movement with consistent diagonal speed (input normalized, magnitude capped at 1)
- [x] Rigidbody2D.MovePosition physics-based movement (no Transform hack), configurable MoveSpeed
- [x] Configurable arena bounds clamp (placeholder, no real map)
- [x] PlayerStats with all 10 MVP stats (GAME_DESIGN.md), plain base values + read accessors
- [x] PlayerHealth: TakeDamage (flat armor reduction) / Heal / FullHeal / IsDead, HP clamped to [0, MaxHP]
- [x] Death: single trigger, IsDead flag, PlayerDied event via EventBus (no Game Over flow)
- [x] CameraFollow: smooth exponential orthographic follow (no Cinemachine), offset, optional bounds
- [x] Player prefab + placeholder sprite placed in SC_Main
- [x] Verified: in-play smoke test 29 checks / 0 failures; dynamic play checks (camera convergence, bounds, component state); temp test code removed
- [x] Bug fix (2026-08-14): Rigidbody2D interpolation = Interpolate (removes movement jitter); Camera orthographicSize 5 → 8 (viewport matches arena); PlayerController input switched to serializable InputActionAsset reference (fixes non-persisted InputActionReference). Verified: full -20..+20 traversal, bounds hold, no pull-back, camera follows, 0 console errors.

## Current Milestone: M4 — Enemy System
- [ ] Enemy base framework (stats/health shared with player pattern)
- [ ] Chaser AI (approach player)
- [ ] Runner AI (fast approach)
- [ ] Shooter AI (ranged attack)
- [ ] Tank AI (slow, high HP)
- [ ] Enemy spawning entry (minimal, full wave logic stays in M8)

## Rules
- Only mark a task complete after actual verification.
- When tasks change, keep this document synchronized with the current project state.

## M3 Bug Fix #2 (2026-08-14) — "return to center" investigation
- [x] Full-project audit of all position writes (only CameraFollow→Camera, PlayerController→MovePosition; no reset code)
- [x] In-assembly runtime probe (temporary AutoProbe): W/A/D movement exact (5 u/s), position kept after release across 3 Play/Stop cycles
- [x] Camera follow verified (player ↔ camera aligned)
- [x] Root cause: empty scene = no visual reference; camera-follow movement invisible → perceived "small range + return to center" (world coordinates never reset)
- [x] Fix: added Ground reference (GroundPlaceholder grid, sortingOrder -10); no player/camera logic changed
- [x] Final clean play/stop: 0 errors / 0 warnings; temp probe removed
