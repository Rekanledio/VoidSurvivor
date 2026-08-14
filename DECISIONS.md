# Void Survivor — Architecture & Design Decisions

## D001 — Use Unity
**Decision:** Unity 6.3 LTS.
**Reason:** Existing user experience, C# ecosystem, strong 2D support, suitable MCP workflow, and good fit for Web + Windows output.

## D002 — Gameplay Direction
**Decision:** Brotato-inspired wave arena + Vampire Survivors-style automatic combat.
**Reason:** Small scope with high system density and a complete game loop.

## D003 — No Deliberate Differentiation Mechanic in MVP
**Decision:** Do not add a custom innovation mechanic just for uniqueness.
**Reason:** Project goal is portfolio/job demonstration, not commercial product differentiation. Completeness and engineering quality have higher priority.

## D004 — Single Arena
**Decision:** One fixed arena for MVP.
**Reason:** Reduces production cost and keeps focus on gameplay/system architecture.

## D005 — Web as First-Class Deliverable
**Decision:** Web playable version is part of MVP delivery.
**Reason:** Low-friction recruiter/interviewer access and strong portfolio value.

## D006 — Repository as Shared Long-Term Memory
**Decision:** Markdown documents in the repository are the persistent project memory shared by ChatGPT and WorkBuddy.
**Reason:** Chat sessions are not a reliable single source of long-term project state.

## D007 — Core Communication Mechanism (M2)
**Decision:** Cross-system communication uses a type-safe generic static EventBus (struct events), and game state changes are centralized in GameManager with a validated legal-transition table broadcast as `GameStateChanged`.
**Reason:** Keeps systems decoupled without a DI framework or third-party dependency; struct events avoid boxing allocations; centralized state prevents systems from mutating global state arbitrarily.
**Scope:** Framework only — gameplay events are defined by their owning milestones, not pre-created in M2.

## D008 — Player Physics, Input and Camera (M3)
**Decision:** Player movement uses Rigidbody2D.MovePosition with normalized input (diagonal speed == cardinal speed) and a configurable bounds clamp; the existing InputSystem_Actions asset is reused for the Move action; the camera uses a custom frame-rate-independent exponential follow instead of Cinemachine.
**Reason:** MovePosition preserves physics compatibility for later collisions/enemies/weapons without Transform hacks; reusing the default actions asset avoids duplicated input config; a small custom follow script keeps dependencies minimal for a 2D orthographic camera.
**Scope:** M3 only. Damage uses simple flat armor reduction (`max(0, damage - Armor)`); crits/elemental/status effects are deferred to their owning milestones.
