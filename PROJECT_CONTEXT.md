# Void Survivor — Project Context

## Project
- Name: Void Survivor
- Type: 2D Top-down Arena Roguelite
- Status: In development / M1 complete
- Target: Job-seeking portfolio + GitHub open source + Web playable + Windows build
- Local Repository: D:\Work\UnityProject\VoidSurvivor
- Unity Version: 6000.3.21f1 (Unity 6.3 LTS, verified 2026-08-14)
- Unity MCP: Connected and verified (CoplayDev unity-mcp, verified 2026-08-14)

## Tech Stack
- Unity 6.3 LTS
- C#
- Unity Input System
- Unity 2D
- TextMeshPro
- ScriptableObject
- Git / GitHub / GitHub Pages
- WorkBuddy
- Unity MCP

## Core Gameplay
- 1 player character
- 1 arena
- 4 normal enemy types
- 1 boss
- 4 weapons / 4 weapon slots
- 10 upgrade types
- 10 waves
- 5–10 minute run
- Wave combat -> XP/Gold -> Level Up/Shop -> Build -> Boss -> Victory/Defeat

## Engineering Goals
- Data-driven design
- Event-driven architecture
- Object Pooling
- Centralized Game State
- Clear system boundaries
- Web compatibility from early development

## Current Phase
Phase 1 — Core framework development

## Current Milestone
M4 — Enemy System (4 enemy types and AI)

## Completed Milestones
- M0 — Project Documentation Initialization
- M1 — Unity Project Initialization (2026-08-14): Git initialized, folder structure created, SC_Main scene with orthographic camera, minimal Core entry (GameManager / GameBootstrap / GameState), Unity MCP verified, clean play-mode test.
- M2 — Core Framework (2026-08-14): GameState transition API with legal-transition table, type-safe generic EventBus, GameStateChanged event, SceneFlow/SceneIds scene-flow base, lifecycle documented in GameBootstrap. Verified via in-play smoke test (33 checks, 0 failures); test code removed after verification.
- M3 — Player System (2026-08-14): PlayerController (Input System Move/WASD, 8-way, diagonal-consistent, bounds clamp via Rigidbody2D.MovePosition), PlayerStats (10 MVP stats), PlayerHealth (TakeDamage with flat armor, Heal/FullHeal, HP clamped 0..Max, single death + PlayerDied event), CameraFollow (smooth exponential orthographic follow, no Cinemachine). Player prefab + placeholder sprite in SC_Main. Verified via in-play smoke test (29 checks, 0 failures) + dynamic play checks; test code removed.

## Next Milestone
M4 — Enemy System

## Current Task
Begin M4: enemy types and AI per ARCHITECTURE.md. Player foundation from M3 is stable.

## Important Decisions
- Do not add special differentiation mechanics for MVP.
- Use Brotato + Vampire Survivors as gameplay references, without copying content/art/assets.
- Prioritize completeness and engineering quality over content quantity.
- Web playable version is a first-class deliverable.
- Do not rely on chat history as the sole source of project memory.

## Context Rules
1. Repository documents are the shared long-term memory.
2. Code and test results are the source of truth for actual implementation state.
3. Major design/architecture changes must be recorded in DECISIONS.md.
4. Milestone completion must update PROJECT_CONTEXT.md, TASKS.md and SAVE_CONTEXT.md.
