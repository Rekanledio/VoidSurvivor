# Void Survivor — Known Issues

## Current
1. GitHub remote repository URL not yet configured.
   - Impact: No remote backup / showcase yet.
   - Status: Open. Plan: add remote when GitHub repo is created (before M17 at the latest).
2. Web build has not yet been tested.
   - Impact: Web is a first-class deliverable; late discovery of Web-only issues would be costly.
   - Status: Open. Plan: first Web build smoke test during M2 or M3, earlier than M16.

## Resolved (M1)
- Unity project not yet created → Created (Unity 6000.3.21f1).
- Actual repository path not recorded → Recorded in PROJECT_CONTEXT.md (D:\Work\UnityProject\VoidSurvivor).
- Unity MCP connection unverified → Verified 2026-08-14 (CoplayDev unity-mcp via WorkBuddy).

## Observations (no action needed now)
1. Transient "The referenced script (Unknown) on this Behaviour is missing!" console pair, seen once during the first compile of the new Core scripts.
   - Impact: None observed; scenes/assets verified clean, all script GUIDs resolve.
   - Status: Not reproduced in subsequent compile/play/stop cycles. Revisit only if it reappears.
2. mcp execute_code (CodeDom backend) loads its own copy of Assembly-CSharp, so static fields of project types read through it are unreliable.
   - Impact: Verification scripts must use UnityEngine object queries instead of project-type statics.
   - Status: Documented; workaround in place.

## Policy
Record unresolved implementation issues here rather than leaving them only in chat messages.
