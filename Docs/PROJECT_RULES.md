# Void Survivor — Project Rules

## 1. General
- Do not expand scope beyond the approved MVP without explicit planning.
- Prefer simple, maintainable solutions over clever solutions.
- Every system must have a clear responsibility.
- Avoid unnecessary third-party dependencies.

## 2. Unity
- Target Unity 6.3 LTS.
- Use the Unity Input System.
- Keep scenes small and purposeful.
- Prefer prefabs for reusable gameplay objects.

## 3. Code
- Use clear C# naming and small focused classes.
- Avoid large God classes.
- Avoid hidden coupling between systems.
- Avoid unnecessary Find()/GetComponent() calls in hot paths.
- Avoid allocation-heavy code in Update loops.

## 4. Data
- Use ScriptableObject for static configuration data such as weapons, enemies, upgrades and waves.
- Runtime state must not mutate the original configuration assets.

## 5. Architecture
- Use centralized Game State management.
- Use events for decoupled cross-system communication where appropriate.
- Use Object Pooling for frequently spawned/despawned objects.

## 6. AI-assisted development
- WorkBuddy may execute implementation tasks, but must follow repository documents.
- AI must not silently change approved design decisions.
- AI-generated code must be inspected and tested.
- Any architecture-changing AI suggestion must be documented before adoption.

## 7. Documentation
- Important decisions belong in DECISIONS.md.
- Current work belongs in TASKS.md.
- Historical work belongs in DEVELOPMENT_LOG.md.
- Handoff/current-state information belongs in SAVE_CONTEXT.md.
- Known unresolved issues belong in KNOWN_ISSUES.md.

## 8. Git
- Use meaningful commits.
- Preferred prefixes: feat, fix, refactor, perf, docs, test, chore.
- Do not commit generated caches, temporary files, secrets, or local-only configuration.

## 9. Web
- Test Web builds early, not only at the end.
- Avoid platform-specific dependencies that cannot run in the browser.
- Keep asset sizes reasonable.
