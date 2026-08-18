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
M14 — Optimization (Performance and stability pass) — **IN PROGRESS**（M13 — Save COMPLETE / ACCEPTED；M14.1 Performance Baseline IN PROGRESS）

## Completed Milestones
- M0 — Project Documentation Initialization
- M1 — Unity Project Initialization (2026-08-14): Git initialized, folder structure created, SC_Main scene with orthographic camera, minimal Core entry (GameManager / GameBootstrap / GameState), Unity MCP verified, clean play-mode test.
- M2 — Core Framework (2026-08-14): GameState transition API with legal-transition table, type-safe generic EventBus, GameStateChanged event, SceneFlow/SceneIds scene-flow base, lifecycle documented in GameBootstrap. Verified via in-play smoke test (33 checks, 0 failures); test code removed after verification.
- M3 — Player System (2026-08-14): PlayerController (Input System Move/WASD, 8-way, diagonal-consistent, bounds clamp via Rigidbody2D.MovePosition), PlayerStats (10 MVP stats), PlayerHealth (TakeDamage with flat armor, Heal/FullHeal, HP clamped 0..Max, single death + PlayerDied event), CameraFollow (smooth exponential orthographic follow, no Cinemachine). Player prefab + placeholder sprite in SC_Main. Verified via in-play smoke test (29 checks, 0 failures) + dynamic play checks; test code removed. Follow-up fixes: Rigidbody2D interpolation = Interpolate (jitter), Camera orthographicSize 5 → 8, PlayerController input switched to serializable InputActionAsset reference, ground reference added (empty-scene movement was visually imperceptible — the "return to center" report was a visual-reference issue; world coordinates never actually reset, verified via in-assembly probe).
- M4 — Enemy System (2026-08-14, COMPLETE): M4.1 Enemy base framework (EnemyData/EnemyStats/EnemyHealth/EnemyController/EnemyDied/EnemyBase); M4.2 Chaser AI (pursue, speed 3.5); M4.3 Runner AI (faster pursue, speed 6); M4.4 Shooter AI (approach/stay in AttackRange, fire minimal Projectile, speed 2.5/range 6/cd 1.5/dmg 8/HP 25); M4.5 Tank AI (slow high-HP pursue, speed 2/HP 120); M4.6 minimal spawn entry (EnemySpawner spawns one of each type around the player). Verified per subtask (27/15/20/27/23/22 probe checks all PASS), 0 errors/warnings; manual in-play observation confirms all four enemies spawn and run their AI.
- M5.1 — Combat Base Framework (2026-08-14): IDamageable + DamageRequest/DamageResult + CombatSystem unified ApplyDamage + DamageApplied event; Projectile migrated off direct PlayerHealth coupling. Verified 23/23 probe PASS, 0 errors/warnings.
- M5.2 — Enemy Death & Kill Attribution (2026-08-14): EnemyKilled (Enemy/Killer) published once per lethal hit on EnemyHealth with valid Source; EnemyController destroys dead enemies (pool in M7). Verified 28/28 probe PASS, 0 errors/warnings.
- M5.3 — Pickup System (2026-08-14): PlayerProgress (XP/Gold) + PickupType/PickupData + Pickup collect + PickupSystem (EnemyKilled → drop XP+Gold at death position). XPPickup/GoldPickup prefabs; Player gains PlayerProgress; SC_Main gains PickupSystem. Verified 25/25 probe PASS + manual play, 0 errors/warnings.
- M5.4 — Player Attack Path (2026-08-14): PlayerAttack.Attack(target) → DamageRequest(Source=Player, PlayerStats.Damage) → CombatSystem; no auto-attack/weapon logic (M6 owns weapons). Player prefab gains PlayerAttack. Verified 23/23 probe PASS, 0 errors/warnings.
- M6.1 — Weapon Base Framework (2026-08-16): WeaponData (SO) + WeaponController (runtime → PlayerAttack, lazy resolve) + WeaponSlot + WeaponManager (4 slots, bounds-checked). WeaponBaseData.asset + WeaponBase.prefab (test base); Player prefab gains WeaponManager. Verified 32/32 probe PASS, 0 errors/warnings.
- M6.2 — Pulse Gun (2026-08-16): first formal weapon — auto-attack loop + nearest-target + single PulseProjectile (Source = player) → CombatSystem; PlayerAttack gains damage overload. PulseGunData (dmg 5/cd 0.25/range 8) + prefabs; SC_Main PulseGun under player. Verified 27/27 probe PASS + manual auto-combat, 0 errors/warnings.
- M6.3 — Scatter Blaster (2026-08-16): multi-pellet fan weapon — ScatterBlasterData (:WeaponData, count 5/spread 45), ScatterBlaster fires N PulseProjectiles in a deterministic uniform symmetric fan; WeaponController.Owner lazy-resolve fix. ScatterBlasterData (dmg 3/cd 0.8/range 7) + prefab. Verified 42/42 probe PASS + manual play, 0 errors/warnings.
- M6.4 — Boomerang (2026-08-16): out-and-return weapon — BoomerangData (:WeaponData, maxDistance/outSpeed/returnSpeed), Boomerang single-flight auto-throw, BoomerangProjectile two-phase (out to maxDistance, return re-aims at player's current position, hit-once per enemy). BoomerangData (dmg 7/cd 1.2/range 8) + prefabs. Verified 37/37 probe PASS, 0 errors/warnings.
- M6.5 — Arc Blade (2026-08-16): close-range area weapon — ArcBladeData (:WeaponData, Range = radius), ArcBlade one OverlapCircleAll strike per cooldown, every live in-range enemy hit once via PlayerAttack → CombatSystem. ArcBladeData (dmg 8/cd 0.9/range 2.5) + prefab (no projectile). Verified 29/29 probe PASS, 0 errors/warnings. **M6 — Weapon System: COMPLETE (all 4 weapons).**
- M7 — Object Pool (COMPLETE): ObjectPool<T> + IPoolable; PulseProjectile / BoomerangProjectile / Enemy / Pickup pooled via static shared pools; ResetStatic* on SubsystemRegistration; verified pool reuse + clean state across Play/Stop.
- M8 — Wave System (COMPLETE): WaveManager (10 waves, time/section-based, WaveStarted/WaveCompleted), EnemySpawner (scaling difficulty), W10 boss wave → Victory; LevelUp/Shop/Pause freeze wave time without re-publishing WaveStarted.
- M9 — Roguelite / Upgrade (2026-08-17, COMPLETE / ACCEPTED):
  - M9.1 XP Level Up: PlayerProgress Level/XP/XPToNextLevel + AddXP carry-over/multi-level + PlayerLevelUp event + PlayerLevelSystem (Playing → LevelUp). 41/41 PASS.
  - M9.2 Upgrade Chooser: UpgradeData (10 assets) + PlayerStats runtime bonus layer (ApplyUpgrade/ResetForRun) + UpgradeManager (pending queue, 3 unique options, SetForcedOptions, Select guards, UpgradeSelected). 58/58 PASS.
  - M9.3 LevelUp UI: UpgradeOptionsGenerated + LevelUpPanel (active Canvas, 3 buttons, event-driven labels, real clicks, consecutive-level refresh, panel instance reuse). 55/55 PASS.
  - M9.4 Shop: PlayerProgress.TrySpendGold (only spend entry) + ShopItemData + ShopManager (W1..W9 → Shop, 4 products, weapon/stat purchase, refresh, continue, already-owned/no-slot rejection) + ShopPanel. 58/58 PASS + layout fixes + weapon-purchase Owner fix (51/51).
  - M9.5 Weapon Upgrade: WeaponController runtime level/bonus (EffectiveDamage/EffectiveAttackCooldown/EffectiveRange) + WeaponUpgradeData (12 assets) + ShopItemType.WeaponUpgrade + shop weapon-upgrade purchase (target must be equipped). 42/42 PASS.
  - M9 Final Regression & Acceptance: 82/82 PASS, 0 FAILURES; M6/M7/M8 regression PASS; full M9 integration PASS; 3× independent Play/Stop 0 errors/0 warnings; temporary probe deleted; final Git clean. **M9 ACCEPTED.**
  - Shop WeaponUpgrade UI Final Layout Fix: WeaponUpgrade Name = two-line single TMP (line1 weapon name 24px, line2 upgrade+level 15px via rich text), Price 22px unchanged, independent Level row removed, normal Weapon/StatBonus cards unaffected, no overlap, Chinese UI OK.
- M10.1 — Boss Base Framework (2026-08-17, COMPLETE — 验证/正式化): 现有 M8.3 Boss 实现已完整覆盖 M10.1 职责 — `BossData : EnemyData` (SO 静态配置) + `BossAI` (追击 + 接触伤害 via CombatSystem, IPoolable) + Boss runtime 复用 EnemyController/EnemyStats/EnemyHealth + W10 Boss spawn (SpawnBossNow) + pooled lifecycle (复用 EnemySpawner 同一 ObjectPool) + death attribution (EnemyKilled) → `BossDefeated` → `Victory` + BossSpawned/BossDefeated 事件。**无生产代码修改**（不重复实现 Boss）。M10_1BossProbe 29/29 PASS（数据/prefab/W10 缩放 725·29·2.175/死亡链各一次/State Victory/No wave11/pool reuse 干净/reused chain 1/1/1/1/回归），2× Play/Stop 0/0。M10 整体 IN PROGRESS。M10.2 — Boss Projectile Skill (2026-08-17, COMPLETE): BossAI 唯一主动技能（每 3s 向玩家当前位置发射 1 个 PulseProjectile，复用共享 ObjectPool+CombatSystem，source=Boss/damage=runtime Stats.Damage 继承 WaveMultiplier/lifetime 3s 命中一次 Release/方向锁定不追踪；Boss prefab 挂 projectilePrefab；21/22 PASS 唯一 FAIL 为 probe 订阅时序、独立验证确认 DamageApplied 正常；2× Play/Stop 0/0）。M10.3 — Boss Finalization & Regression (2026-08-17, COMPLETE): **Boss 最终参数确认 + 完整战斗验收 + 跨系统回归 + 跨 Play Session 稳定性** — 无新增 mechanic、无生产代码修改；M10_3BossFinalRegressionProbe 55/55 PASS（参数 500/20/1.5/1.5/1.0 + W10 ×1.45→725/29/2.175、技能 3s/6.0/3.0s/10.0、追击/接触/技能/方向锁定/命中/Release/冷却/死亡链各一次/Victory/No Wave11/pool reuse 干净/跨两 run 各 2 次/M6·M7·M9 回归）；3× Play/Stop 0/0；参数确认适合 MVP 最终值 No changes required。**M10 = COMPLETE / ACCEPTED。**

## Next Milestone
M14 — Optimization (Performance and stability pass)

## Current Task
M14.1 — Performance Baseline（IN PROGRESS；M14 — Optimization IN PROGRESS；M13 — Save COMPLETE / ACCEPTED）

## Latest Commit
817851c fix: M14 pre-baseline functional regression（43ef935 docs: update latest commit to M13.4 acceptance）

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
