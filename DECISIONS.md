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
