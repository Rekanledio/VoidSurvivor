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

## Current Milestone: M3 — Player System
- [ ] Player movement (Input System driven, 2D top-down)
- [ ] Player stats foundation (per GAME_DESIGN.md stat list)
- [ ] Player health
- [ ] Camera follow

## Rules
- Only mark a task complete after actual verification.
- When tasks change, keep this document synchronized with the current project state.
