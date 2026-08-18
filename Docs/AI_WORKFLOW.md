# Void Survivor — AI Workflow

## Roles

### ChatGPT
- High-level game design
- Architecture planning
- Task decomposition
- Design review
- Code/design review
- Milestone acceptance
- Problem diagnosis

### WorkBuddy
- Local repository operations
- File creation/modification
- Code implementation
- Unity project interaction through MCP
- Local testing
- Documentation synchronization

### Unity MCP
- Bridge between AI tooling and Unity Editor
- Scene/GameObject operations
- Editor-side inspection and project interaction

## Standard Cycle
1. ChatGPT defines the next task.
2. Task is recorded in TASKS.md.
3. WorkBuddy reads the required context documents.
4. WorkBuddy performs implementation.
5. Unity MCP is used when editor-side operations are needed.
6. WorkBuddy runs tests/checks.
7. WorkBuddy updates project documents.
8. ChatGPT reviews the result.
9. SAVE_CONTEXT.md is updated at milestone boundaries.

## Context Loading Priority
1. PROJECT_CONTEXT.md
2. SAVE_CONTEXT.md
3. TASKS.md
4. PROJECT_RULES.md
5. GAME_DESIGN.md
6. ARCHITECTURE.md
7. DECISIONS.md
8. DEVELOPMENT_LOG.md / KNOWN_ISSUES.md as needed

## Important Rule
Do not keep important project state only inside an AI chat.
