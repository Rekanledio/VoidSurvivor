# Void Survivor — Save Context

## Last Updated
2026-08-18（M16 — GitHub Showcase IN PROGRESS；当前阶段 Final GitHub Showcase Acceptance）

## Current Phase
Phase 2 — Release & GitHub Showcase

## Current Milestone
M16 — GitHub Showcase — **IN PROGRESS**（M16 Scope Precheck = COMPLETE / ACCEPTED；GitHub baseline synchronization = COMPLETE / ACCEPTED；M16 README = COMPLETE / ACCEPTED；Screenshot Showcase Review / Packaging = COMPLETE / ACCEPTED；M16 WebGL Online Demo / Deployment = COMPLETE / ACCEPTED；当前阶段 = Final GitHub Showcase Acceptance；M14 = STOPPED / CLOSED WITHOUT FURTHER OPTIMIZATION；M15 = COMPLETE / ACCEPTED；M13 = Save COMPLETE / ACCEPTED）

## Release / M14-M16 State（2026-08-18）
- **Release Baseline**：`2dd971c`（feat: replace placeholder assets and prepare release）。GitHub baseline synchronization = **COMPLETE / ACCEPTED**（2026-08-18 正常 FF push，无 force；GitHub main 已与本地 main 同步）。
- **M14**：Pre-Baseline Functional Regression COMPLETE / ACCEPTED（commit `817851c`）；Profiler baseline 尝试后无可用的优化 baseline；M14.2/3/4 NOT EXECUTED；**STOPPED / CLOSED WITHOUT FURTHER OPTIMIZATION**。
- **M15**：Windows Release COMPLETE（`D:\Work\UnityBuilds\VoidSurvivor\Windows\`，2026-08-18）；WebGL Release Build COMPLETE（`D:\Work\UnityBuilds\VoidSurvivor\WebGL\`，Brotli，browser smoke PASS；完整浏览器 Gameplay 人工验收待复核）。
- **Runner**：moveSpeed = **5.5**（RunnerData.asset），最终 KEEP。
- **Final Art**：Player/Ground/Chaser/Runner/Shooter/Tank/Boss/弹丸全部替换为正式美术（Release baseline `2dd971c`）。
- **M16 产出状态**：README = 已创建（`README.md`，中文专业风格，COMPLETE / ACCEPTED，README 含真实 Online Demo URL）；Showcase screenshots = 已提供 + ACCEPTED（`Screenshots/` 5 张正式截图：Main Menu / Gameplay / Boss / Shop / Game Over；hero=Normal_Gameplay；Review / Packaging COMPLETE）；**Online demo = 已部署**（Cloudflare Pages Direct Upload，URL: **https://void-survivor-9vg.pages.dev/**；HTTP 头验证 PASS；Unity 启动链 PASS；0 page error；ORIGINAL M15 Build byte-identical 保护）；GitHub baseline sync = COMPLETE（main 已同步）。**Full Browser Gameplay Acceptance = PARTIAL**（headless 沙箱 GPU 限制；real-browser 复核 outstanding）。

## Completed (overview)
- M0: concept, MVP scope, delivery strategy, documentation strategy.
- M1: Unity project (6000.3.21f1) verified; Git initialized; Assets skeleton; SC_Main scene; minimal Core entry; Unity MCP verified.
- M2: Core Framework — GameManager state machine (TryChangeState + legal-transition table + GameStateChanged), EventBus (type-safe generic static), GameEvents, SceneFlow/SceneIds, lifecycle docs. 33/33 smoke checks passed.
- M3 (2026-08-14): Player System — PlayerController (Input System Move, 8-way diagonal-consistent, MovePosition, bounds clamp), PlayerStats (10 MVP stats), PlayerHealth (TakeDamage flat armor / Heal / single death + PlayerDied), CameraFollow (exponential, no Cinemachine). Player prefab + placeholder; SC_Main camera size 8. Verified 29/29 + dynamic checks + M3 bug fixes (interpolation, camera size, persisted InputActionAsset, Ground reference).
- M4 (2026-08-14): Enemy System — Chaser/Runner/Shooter/Tank AI + EnemySpawner minimal spawn. Verified 27/15/20/27/23/22 PASS.
- M5 (2026-08-14): Combat System — IDamageable/DamageRequest/DamageResult/CombatSystem + EnemyKilled attribution + PickupSystem (XP/Gold) + PlayerAttack path. Verified 23/28/25/23 PASS.
- M6 (2026-08-16): Weapon System — 4 weapons (Pulse Gun / Scatter Blaster / Boomerang / Arc Blade) + 4 slots + WeaponManager. Verified 32/27/42/37/29 PASS.
- M7 (2026-08-16): Object Pool — ObjectPool<T> + IPoolable; Projectile/Enemy/Pickup pooled. Verified 19/38/31/34/45 PASS.
- M8 (2026-08-16): Wave System — WaveManager 10 waves + difficulty growth + W10 Boss → Victory. Verified 42/39/47/58 PASS.
- M9.1 (2026-08-16): XP Level Up — PlayerProgress Level/XP/XPToNextLevel + AddXP carry-over/multi-level + PlayerLevelUp + PlayerLevelSystem. 41/41 PASS.
- M9.2 (2026-08-16): Upgrade Chooser — UpgradeData (10 assets) + PlayerStats bonus layer + UpgradeManager (pending queue, 3 unique options, Select guards). 58/58 PASS.
- M9.3 (2026-08-16): LevelUp UI — UpgradeOptionsGenerated + LevelUpPanel (active Canvas, 3 buttons, event labels, real clicks). 55/55 PASS.
- M9.4 (2026-08-16): Shop — TrySpendGold + ShopItemData + ShopManager (W1..W9 → Shop, 4 products, purchase/refresh/continue, rejections) + ShopPanel. 58/58 PASS + layout fixes + weapon-purchase Owner fix 51/51.
- M9.5 (2026-08-16): Weapon Upgrade — WeaponController runtime level/bonus + Effective* + WeaponUpgradeData (12 assets) + Shop WeaponUpgrade products. 42/42 PASS.
- M9 Final Regression & Acceptance (2026-08-17): 82/82 PASS, 0 FAILURES; M6/M7/M8 regression PASS; full M9 integration PASS; 3× Play/Stop 0/0; temp probe deleted; Git clean. **M9 = COMPLETE / ACCEPTED.**
- Shop WeaponUpgrade UI Final Layout Fix (2026-08-17): WeaponUpgrade Name = two-line single TMP (24px name + 15px upgrade/level via rich text), Price 22px unchanged, independent Level row removed, normal cards untouched, no overlap, Chinese OK. Commit 12f17e4.
- M10.1 (2026-08-17): Boss Base Framework — **验证/正式化**。现有 M8.3 Boss 实现（BossData : EnemyData / BossAI / W10 SpawnBossNow / EnemyKilled→BossDefeated→Victory / BossSpawned+BossDefeated 事件 / 复用 EnemySpawner 同一 ObjectPool / IPoolable）已完整覆盖 M10.1 职责，**无生产代码修改**。M10_1BossProbe 29/29 PASS（数据/prefab 组件链/W10 缩放 725·29·2.175/死亡链各一次/State Victory/No wave11/pool reuse 干净/reused chain 1/1/1/1/回归），2× Play/Stop 0/0。HEAD 1f5369e。**M10.1 = COMPLETE**（M10 整体 IN PROGRESS）。
- M10.2 (2026-08-17): Boss Projectile Skill — BossAI 唯一主动技能：每 3.0s（skillCooldown）存活+Playing+玩家存活+距离≤10.0 时向玩家**当前位置**发射 1 个 PulseProjectile（speed 6.0，lifetime 3.0s，方向锁定不追踪，Source=Boss，Damage=Boss runtime Stats.Damage 继承 WaveMultiplier，命中一次即 Release）。**复用共享 PulseProjectile ObjectPool + CombatSystem.ApplyDamage + DamageRequest + IPoolable**——无第二套 Damage/Pool/HP 框架。Boss prefab 挂 projectilePrefab=PulseProjectile.prefab。数值 3.0s/6.0/3.0s/10.0 为 implementation parameters 非 GAME_DESIGN balance。M10_2BossAbilityProbe 21/22 PASS（唯一 FAIL 为 probe DamageApplied 订阅时序，独立验证确认 CombatSystem 正常发布 DamageApplied；玩家 HP 实际正确下降），2× Play/Stop 0/0，M8 回归 PASS。HEAD 1fc45fc。**M10.2 = COMPLETE**。
- M10.3 (2026-08-17): Boss Finalization & Regression — **Boss 最终参数确认（500/20/1.5/1.5/1.0 + W10 ×1.45 → 725/29/2.175，技能 3s/6.0/3.0s/10.0）确认适合作为 MVP 最终值，No production parameter changes required；无新增 mechanic、无生产代码修改**。完整战斗验收（W10 spawn/追击/接触伤害 29/技能发射/方向锁定不追踪/命中/Release/冷却/死亡链各一次/Victory/No Wave11/pool reuse 干净+技能重置/跨两 run 各 2 次）+ M6/M7/M9 回归。M10_3BossFinalRegressionProbe **55/55 PASS**；3× Play/Stop 全 0/0；probe 已删；字体未写坏。**M10 = COMPLETE / ACCEPTED。** HEAD f815d71（docs: M10.3 boss finalization and regression）。
- M11.1 (2026-08-17): Main Menu — 最小 Main Menu（Game Title "虚空幸存者" + PlayButton "开始游戏" + QuitButton "退出游戏"）；Play → MainMenu→Playing；Quit → Application.Quit (Editor 下 EditorApplication.isPlaying=false)；复用现有 Canvas/EventSystem/InputSystemUIInputModule（唯一确认）/ GameState / EventBus (GameStateChanged) / TMP NotoSansSC，不创建第二套。M11_1MainMenuProbe 23/23 PASS；2× Play/Stop 0/0；字体未写坏；probe 已删。**M11 整体 IN PROGRESS**（仅 M11.1 完成；M11.2/3/4 pending）。HEAD 67f2ce0（feat: M11.1 main menu ui）。## In Progress
- M11.2 (2026-08-17): Gameplay HUD — 最小 HUD 5 文本（HPText/XPText/LevelText 左上 + GoldText/WaveText 右上）锚顶 100 高条；显隐 GameStateChanged 驱动（Playing/LevelUp/Shop/Paused 显示，GameOver/Victory/MainMenu 隐藏）；数据 Update 每帧读缓存引用（PlayerHealth/PlayerProgress/WaveManager），不修改任何 gameplay、不建第二套事件系统、不每帧 Find。M11_2GameplayHUDProbe 26/26 PASS；2× Play/Stop 0/0；字体未写坏；probe 已删。**M11 整体 IN PROGRESS**（仅 M11.1+M11.2 完成；M11.3/4 pending）。- Nothing. M3 (incl. bug fix) is fully complete.
- M11.3 (2026-08-17): Result Screens — GameOverPanel（深红 + "游戏结束" + 重新开始/主菜单）+ VictoryPanel（深蓝 + "胜利" + 重新开始/主菜单）；ResultPanel 通用组件（showInState + GameStateChanged 显隐）；Restart → RunRestarter.RestartRun()（Core 层：PlayerProgress.ResetForRun + PlayerStats.ResetForRun + 全武器 ResetWeaponUpgrades + GameManager MainMenu→Playing 让 WaveManager 自动 StartWave(1)）。**新增 PlayerProgress.ResetForRun + RunRestarter.cs + ResultPanel.cs**；不修改 GameManager/PlayerStats/WeaponController/WeaponManager/WaveManager/SceneFlow。M11_3ResultScreensProbe 46/46 PASS（25 项验证 + 4 条流程 A/B/C/D 全 PASS）；2× Play/Stop 0/0；字体未写坏；probe 已删。**M11 整体 IN PROGRESS**（仅 M11.1+M11.2+M11.3 完成；M11.4 pending）。
- M11.4 (2026-08-17): Full UI Integration & Acceptance — **两个核心修复**：①`GameFlow.cs`（新建，Core 层）PlayerDied→仅在 Playing 时 TryChangeState(GameOver) 自动触发；②非 Playing 暂停 simulation（WeaponController/EnemyController 加 `GameplayActive` 守卫：4 武器 Update + 4 Enemy AI + BossAI FixedUpdate/OnTriggerEnter2D——MainMenu/GameOver/Victory 无新攻击）+ Restart 补齐（PlayerHealth.ResetForRun 复活 + RunRestarter 调用）。M11_4FullUIRegressionProbe **48/48 PASS**（26 项验收 + UI 状态矩阵 + PlayerDied→GameOver 自动 + 非 Playing 无新武器攻击量化 0->0/2->0/3->3 + Restart 重置 + W10 Boss Victory + Boss Projectile + M9 回归）；3× Play/Stop 0/0；字体未写坏；probe 已删。**M11 = COMPLETE / ACCEPTED**（M12 Audio/VFX pending）。HEAD a30bde3（docs: update latest commit to M11.4 acceptance）。## M3 Bug Fix (2026-08-14)
- M12.1 (2026-08-17): Audio Foundation — 最小 SFX 基础架构（AudioManager 单例 + 1 可复用 AudioSource + PlaySfx(clip)/(clip,volume) + Master/Sfx volume 默认 1.0；PlayOneShot 支持重叠；null clip no-op）。GameBootstrap 挂 GameManager 同 GO（persistent）。不修改 GameManager/GameFlow/GameEvents/UI/武器/敌人/Boss/Scene。M12_1AudioFoundationProbe 16/16 PASS；2× Play/Stop 0/0；字体未写坏；probe 已删。**M12 整体 IN PROGRESS**（仅 M12.1 完成；M12.2/3/4 pending）。- Reported: (1) small movement range + apparent pull-back near edges; (2) jitter/blur when holding WASD.
- M12.2 (2026-08-17): Core Gameplay SFX — 程序化生成 9 个 WAV（Assets/Resources/Audio/SFX/，无版权）；新增 SfxLibrary（SfxType→AudioClip 显式映射）+ GameplaySfx（8 事件 + Victory 接入，DamageApplied 0.05s 限频）；4 个 UI Panel 按钮加 PlayUiClick（同一 click）。M12_2CoreSfxProbe 19/19 PASS；2× Play/Stop 0/0；字体未写坏；probe 已删。**M12 整体 IN PROGRESS**（M12.1+M12.2 完成；M12.3/4 pending）。
- Root causes (verified): (2) Rigidbody2D.interpolation was None → physics-stepped positions vs per-frame camera smoothing. (1) no pull-back code exists (verified in play); perception came from jitter + small viewport (orthographicSize 5). Latent defect found: InputActionReference created at runtime was NOT persisted to scene/prefab (would break movement after editor restart).
- M12.3 (2026-08-17): Basic VFX Feedback — 8 个 ParticleSystem prefab（Assets/Prefabs/VFX/，短生命周期 0.15-0.6s，autoDestroy）+ `VFXManager.cs`（事件订阅 DamageApplied/EnemyDied/PickupCollected/PlayerLevelUp/BossSpawned/BossDefeated/PlayerDied/GameStateChanged(Victory) → prefab Instantiate + Play；DamageApplied 0.05s 限频；缓存 Player 一次 Find）。M12_3BasicVfxProbe 22/22 PASS；2× Play/Stop 0/0；字体未写坏；probe 已删。**M12 整体 IN PROGRESS**（M12.1+M12.2+M12.3 完成；M12.4 pending）。
- Fixed: Rigidbody2D.interpolation = Interpolate; Camera orthographicSize 5 → 8; PlayerController switched to serializable `InputActionAsset` + `FindAction("Move")`.
- M12.4 (2026-08-17): Audio/VFX Integration & Acceptance — **VFX Runtime 修复**（AssetDatabase→Resources.Load，8 prefab 移至 Assets/Resources/VFX/，删 UnityEditor 依赖）+ 全链路验收（M12_4AudioVfxAcceptanceProbe **40/40 PASS**：9 SFX+8 VFX Runtime Load/8 事件 SFX+VFX 双链路/双限频 50→1/VFX 自动销毁/M9·M10·M11 回归/Restart clean）+ **Windows Standalone64 Build SUCCESS**（173.72MB，errors=0，三次运行进程稳定 20-25s 无崩溃）。3× Play/Stop 0 errors；字体未写坏；probe 已删。**M12 = COMPLETE / ACCEPTED**（M13 NOT STARTED）。HEAD d2f435c（docs: M12.4 audio vfx acceptance）。
- M13.1 (2026-08-17): Save Foundation — SaveData（masterVolume/sfxVolume/bestWave/bestLevel/bestGold 预留字段）+ SaveManager（单例挂 GameManager GO；Save/Load/HasSave/DeleteSave；JsonUtility+persistentDataPath/VoidSurvivorSave.json；缺文件/损坏 JSON 回退默认不崩溃；不保存 Run Data）。M13_1SaveFoundationProbe **23/23 PASS**（含精确恢复 0.73·0.41·8·12·350、损坏 JSON 容错、M9/M10/M11/M12 回归）；测试 save 文件已清理；字体未写坏。**M13 整体 IN PROGRESS**（仅 M13.1 完成；M13.2/3/4 pending）。production 3819433（feat: M13.1 save foundation）+ docs 9ca61c3（docs: M13.1 save foundation documentation）。
- M13.2 (2026-08-17): Settings Persistence — AudioManager 加 SetMasterVolume/SetSfxVolume（Clamp01 + 读-改-写 SaveSettings）+ Awake LoadSettings；GameBootstrap 调整 SaveManager 先于 AudioManager 挂载。M13_2SettingsPersistenceProbe Session A 20/20 + B 14/14 + C 20/20（默认 1/1、重启自动加载 0.73/0.41、损坏 JSON/删 save 回退、M9/M10/M11 回归）；测试 save 已清理；字体未写坏。**M13 整体 IN PROGRESS**（M13.1+M13.2 完成；M13.3/4 pending）。production 08064c9（feat: M13.2 settings persistence）+ docs 376ca21（docs: M13.2 settings persistence documentation）。
- M13.3 (2026-08-17): Best Run Record — BestRunRecorder（订阅 GameStateChanged To==GameOver/Victory 终局记录 Wave/Level/Gold → SaveManager.Load → max 比较 → 有提升才 Save；_recordedThisRun 防重复）。M13_3BestRunRecordProbe Session A 16/16 + B 17/17 + C 17/17（初始 0/0/0、记录 5/3/120、低 run 保持、高 run 更新 8/6/300、跨会话持久化、volume 保留、Restart 不覆盖、MainMenu 不触发、重复终局安全、M9-M12 回归）；测试 save 已清理；字体未写坏。**M13 整体 IN PROGRESS**（M13.1+M13.2+M13.3 完成；M13.4 pending）。production 0ac516e（feat: M13.3 best run record）+ docs fb576c8（docs: M13.3 best run record documentation）。
- M13.4 (2026-08-17): Save Integration & Acceptance — 全链路最终集成验收（**生产代码零修改**）。M13_4SaveIntegrationAcceptanceProbe Session A 13/13 + B 22/22 + C PASS（Settings+Best 共存/双向不覆盖/max/Restart 边界/MainMenu 不重复/corrupt/Delete/跨会话 0.55·0.25+8/6/300/M9-M12 回归）；测试 save 已清理；字体未写坏。**M13 = COMPLETE / ACCEPTED**（M14 NOT STARTED）。docs 276eb9e（docs: M13.4 save integration and acceptance）+ 43ef935（docs: update latest commit to M13.4 acceptance）。
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
- GitHub remote repository URL not yet configured (local-only Git for now). → **Resolved (M16)**: origin configured to https://github.com/Rekanledio/VoidSurvivor.git; GitHub baseline synchronization COMPLETE (2026-08-18, fast-forward push, no force).
- Web build not yet tested. → **Resolved (M15.2)**: WebGL Release Build verified (Brotli, browser smoke PASS). Full browser gameplay acceptance pending (see KNOWN_ISSUES.md Current #5).
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

## M4.1 — Enemy Base Framework (2026-08-14)
- EnemyData (SO static config: MaxHP/Damage/AttackRange/AttackCooldown/MoveSpeed), EnemyStats (runtime read-only view), EnemyHealth (HP/TakeDamage/clamp/single death + EnemyDied), EnemyController (cached refs + PlayerHealth target, AI extension point). No AI behavior yet.
- GameEvents: added EnemyDied (carries enemy GameObject). EnemyKilled/attribution deferred to M5 Combat.
- Assets: EnemyPlaceholder.png (red square), ScriptableObjects/Enemies/EnemyBase.asset, Prefabs/Enemies/EnemyBase.prefab (7 components, data ref persisted).
- Verified: 27/27 probe PASS; final play/stop 0 errors / 0 warnings. Temp probe deleted.
- Next: M4.2 Chaser AI. Wave logic in M8.

## M4.2 — Chaser AI (2026-08-14)
- ChaserAI.cs: pursues player at EnemyData MoveSpeed via Rigidbody2D.MovePosition; reuses EnemyController refs; stops when dead; no attack logic.
- ChaserData.asset (moveSpeed 3.5) + Chaser.prefab (EnemyBase + ChaserAI, data = ChaserData).
- Verified: 15/15 probe PASS (speed exact 3.5, approach/re-target/chase, death stop, player intact); final play/stop 0 errors / 0 warnings. Temp probe deleted.
- Next: M4.3 Runner AI.

## M4.3 — Runner AI (2026-08-14)
- RunnerAI.cs: faster pursuer (same pattern as ChaserAI; MovePosition + EnemyController refs; stops when dead; no extra mechanics).
- RunnerData.asset moveSpeed 6 (vs Chaser 3.5; docs give no explicit value — recorded choice). Runner.prefab (base + RunnerAI, data = RunnerData).
- Verified: 20/20 probe PASS (speed 6.00 exact via fixed-physics-frame measure; re-target; chase; physics hold near player ~1.07; death stop 0.000; Chaser regression intact; player intact). Final play/stop twice: 0 errors / 0 warnings. Temp probe deleted.
- Next: M4.4 Shooter AI.

## M4.4 — Shooter AI (2026-08-14)
- ShooterAI.cs: approach only outside AttackRange, stop inside; fire minimal Projectile at player when in range + off cooldown (Time.time); reuses EnemyController refs; death stops move+attack.
- Projectile.cs: MINIMAL M4.4 projectile (kinematic velocity, lifetime, OnTriggerEnter2D -> PlayerHealth.TakeDamage) — temporary until M5 Combat unifies damage.
- ShooterData.asset (speed 2.5 / range 6 / cd 1.5 / dmg 8 / HP 25) + ShooterProjectile.prefab + Shooter.prefab (data + projectilePrefab wired).
- Verified: 27/27 probe PASS (fire->hit->damage via player HP 84->68->60, cooldown window, out-of-range silence, death stop, Chaser/Runner regression, no residue). Final play/stop twice: 0 errors / 0 warnings.
- Next: M4.5 Tank AI.

## M4.5 — Tank AI (2026-08-14)
- TankAI.cs: slow high-HP pursuer (ChaserAI pattern; MovePosition + EnemyController refs; stops when dead; no special attack).
- TankData.asset: moveSpeed 2 (below Chaser 3.5), maxHP 120 (above others 30). Tank.prefab (base + TankAI, data = TankData).
- Verified: 23/23 probe PASS (speed 2.00 exact; re-target; TakeDamage->90; clamp 0; single EnemyDied; death stop; Chaser/Runner/Shooter regression). Final play/stop twice: 0 errors / 0 warnings.
- Next: M4.6 minimal spawn entry.

## M4.6 — Minimal Spawn Entry (2026-08-14) — M4 COMPLETE
- EnemySpawner.cs: Start-time single spawn of one instance per configured prefab at fixed cardinal offsets (10) around the player; no wave/timer/loop; M8 owns waves.
- SC_Main: EnemySpawner GameObject wired to Chaser/Runner/Shooter/Tank prefabs.
- Verified: 22/22 probe PASS + manual in-play observation (all 4 enemies visible, Chaser/Runner/Tank pursue, Shooter fires projectiles). Final play/stop twice: 0 errors / 0 warnings.
- **M4 — Enemy System: COMPLETE.** Next: M5 — Combat System (wave logic stays in M8).

## M5.1 — Combat Base Framework (2026-08-14)
- Combat pipeline: IDamageable (PlayerHealth/EnemyHealth implement), DamageRequest/DamageResult, CombatSystem.ApplyDamage static unified entry, DamageApplied event.
- Projectile migrated: contact routes damage through CombatSystem to any IDamageable (carries source from ShooterAI); no PlayerHealth coupling.
- Verified: 23/23 probe PASS; final play/stop twice: 0 errors / 0 warnings.
- Known behavior: projectile hits any IDamageable (no friendly-fire filter yet) — recorded for a later combat refinement.
- Next: M5.2 (per TASKS.md split).

## M5.2 — Enemy Death & Kill Attribution (2026-08-14)
- EnemyKilled (Enemy/Killer) published by CombatSystem once per lethal hit on EnemyHealth with valid Source; null-source deaths → EnemyDied only.
- EnemyController = death/despawn layer: destroys its GameObject on its own EnemyDied (plain Destroy; pool in M7).
- Event order: TakeDamage → EnemyDied → EnemyKilled → cleanup.
- Verified: 28/28 probe PASS; final play/stop twice: 0 errors / 0 warnings.
- Next: M5.3 (per TASKS.md split).

## M5.3 — Pickup System (2026-08-14)
- PlayerProgress (XP/Gold runtime resources, negative-safe); PickupType (XP/Gold); PickupData (SO, read-only); Pickup (trigger collect → progress + PickupCollected → destroy); PickupSystem (EnemyKilled → spawn XP+Gold at death position).
- XPPickup/GoldPickup prefabs + data (XP 10 / Gold 5); Player prefab gains PlayerProgress; SC_Main gains PickupSystem.
- Verified: 25/25 probe PASS + manual play observation (drop → collect live). Final play/stop twice: 0 errors / 0 warnings.
- Next: M5.4+ per the official task split (to be confirmed by the next task prompt).

## M5.4 — Player Attack Path (2026-08-14)
- PlayerAttack.cs: Attack(target) → DamageRequest(Source=Player, PlayerStats.Damage) → CombatSystem.ApplyDamage; no auto-attack/weapon logic (M6 owns weapons). Player prefab gains PlayerAttack.
- Verified: 23/23 probe PASS (non-lethal/lethal, EnemyKilled once Killer==Player, cleanup, dead rejection, Pickup drops at death position, regressions). Final play/stop twice: 0 errors / 0 warnings.
- Next: M5 completion per official split; M6 weapons.

## M6.1 — Weapon Base Framework (2026-08-16)
- WeaponData (SO), WeaponController (runtime → PlayerAttack), WeaponSlot, WeaponManager (4 slots, bounds-checked). WeaponBaseData.asset + WeaponBase.prefab (test base, not a formal weapon); Player prefab gains WeaponManager.
- Verified: 32/32 probe PASS (lazy player resolve fixed Awake-before-parent timing). Final play/stop twice: 0 errors / 0 warnings.
- Next: M6.2 Pulse Gun.

## M6.2 — Pulse Gun (2026-08-16)
- PulseGun (auto-attack, Time.time cooldown, nearest-target in Range, one PulseProjectile per shot, Source = player, self-equips slot 0) + PulseProjectile (straight flight → CombatSystem, skips own source).
- PulseGunData (dmg 5 / cd 0.25 / range 8); PlayerAttack extended with Attack(target, damage) (old API compatible).
- Verified: 27/27 probe PASS (cooldown gaps 0.250–0.252s exact, beyond-range silent, nearest targeted, kill → Killer == Player, re-target). Manual play: auto-combat live (enemies 4 → 1, player XP 20 / Gold 5). Final play/stop twice: 0 errors / 0 warnings.
- Next: M6.3 Scatter Blaster.

## M6.3 — Scatter Blaster (2026-08-16)
- ScatterBlasterData : WeaponData (+count 5/spread 45); ScatterBlaster auto-fires N PulseProjectiles in a uniform symmetric fan (center → target); reuses PulseProjectile. ScatterBlasterData (dmg 3/cd 0.8/range 7) + prefab.
- WeaponController.Owner lazy-resolve fix (runtime-instantiated weapons kept null source → projectiles hit the player and died; now resolves on access).
- Verified: 42/42 probe PASS (fan geometry exact, cooldown 0.801s, kill → Killer == Player, regressions) + manual play. Final play/stop twice: 0 errors / 0 warnings.
- Next: M6.4 Boomerang.

## M6.4 — Boomerang (2026-08-16)
- BoomerangData : WeaponData (+maxDistance 6/outSpeed 8/returnSpeed 10); Boomerang auto-throws with single-flight rule; BoomerangProjectile two-phase (Outbound to maxDistance, Return re-aims at player's current position, hit-once per enemy). BoomerangData (dmg 7/cd 1.2/range 8) + prefabs (green ring).
- Verified: 37/37 probe PASS (speeds exact, hit-once, multi-target, return to moved player, kill → Killer == Player, regressions). Final play/stop twice: 0 errors / 0 warnings.
- Next: M6.5 Arc Blade.

## M6.5 — Arc Blade (2026-08-16) — M6 COMPLETE
- ArcBladeData : WeaponData (Range = attack radius); ArcBlade close-range area strike — one OverlapCircleAll per cooldown, every live in-range enemy hit once (deduped, dead skipped, self ignored), each via PlayerAttack → CombatSystem. ArcBladeData (dmg 8/cd 0.9/range 2.5) + prefab (no projectile).
- Verified: 29/29 probe PASS (multi-target one strike, out-of-range untouched, player unharmed, per-target once, kill → Killer == Player, regressions). Final play/stop twice: 0 errors / 0 warnings.
- M6 — Weapon System: COMPLETE (all 4 weapons).
- Next: M7.1 Object Pool.

## M7.1 — Object Pool Base Framework (2026-08-16)
- IPoolable (OnSpawn/OnDespawn) + generic ObjectPool<T> in Core (VoidSurvivor.Core). Pre-warm, Get (activate+OnSpawn, grow-on-exhaust), Release (OnDespawn+deactivate, double-release guarded), Clear (destroy all). Standalone — not wired into game lifecycles (M7.2).
- Verified: 19/19 probe PASS. Final play/stop twice: 0 errors / 0 warnings.
- Next: M7.2 Pool Integration.

## M7.2.1 — Projectile Pool Integration (2026-08-16)
- PulseProjectile / BoomerangProjectile / Shooter Projectile → ObjectPool (static shared per-type pool; PulseGun+ScatterBlaster share PulseProjectile pool). Spawn = Get+Init; lifetime/hit → Release. IPoolable OnDespawn clears runtime state. Boomerang ActiveCount via Spawn/DespawnSelf (not OnDestroy). Static reset on play start.
- Verified: 38/38 probe PASS + regressions. Final play/stop twice: 0 errors / 0 warnings.
- Next: M7.2.2 Enemy / Pickup Pool Integration.

## M7.2.2 — Enemy Pool Integration (2026-08-16)
- EnemyController implements IPoolable (Spawn(pool,pos); OnSpawn resets health + notifies AI; OnDespawn stops AI + zeroes velocity; death → Release). EnemyHealth.ResetForSpawn. EnemySpawner owns per-prefab lazy pools (M4.6 rules unchanged). ShooterAI OnSpawn resets cooldown.
- Verified: 31/31 probe PASS (HP/IsDead reset, EnemyDied once per life, Chaser re-track, Shooter fires+reset+stops, double-release, Clear, regressions). Final play/stop twice: 0 errors / 0 warnings.
- Next: M7.2.3 Pickup Pool Integration.

## M7.2.3 — Pickup Pool Integration (2026-08-16) — M7.2 COMPLETE
- Pickup implements IPoolable (Spawn(pool,pos); collect → Release; no runtime state to reset). PickupSystem owns XP/Gold pools (lazy, capacity 16). EnemyKilled → 1 XP + 1 Gold at actual death position unchanged.
- Verified: 34/34 probe PASS (amounts 10/5, collect-once, recycle, death-position, no state bleed, regressions). Final play/stop twice: 0 errors / 0 warnings.
- M7.2 complete: Projectile (M7.2.1) + Enemy (M7.2.2) + Pickup (M7.2.3) all pooled.
- Next: M7 final regression/acceptance; then M8 Wave System.

## M7 — Object Pool COMPLETE (2026-08-16)
- M7.1 Framework 19/19, M7.2.1 Projectile 38/38, M7.2.2 Enemy 31/31, M7.2.3 Pickup 34/34, M7 Final Regression 45/45 — all PASS.
- Full kill→drop→collect→release lifecycle verified 3x; four weapons together; EventBus once-per-event; cross-play static state clean (3 final Play/Stops, 0/0 each).
- Git clean at 0723224; temp probes deleted.
- Next: M8.1 Wave System.

## M8.1 — Wave Lifecycle & Spawn Scheduling (2026-08-16)
- WaveManager (Enemy ns, on EnemySpawner in scene): waves 1..10, deltaTime-accumulated time (Playing only; pause states freeze; GameOver/Victory stop + fresh-run reset), centralized per-wave config (duration/count/interval), deterministic type rotation, cardinal spawn points via EnemySpawner.SpawnEnemy (pooled). WaveStarted/WaveCompleted events. Wave 10 = normal enemies (boss M8.3); after wave 10 idle (no victory logic). No difficulty scaling.
- Verified: 42/42 probe PASS (events once, progression 1→2, spawn count/interval/stop, pause freeze + resume no dup start, GameOver stop + restart W1, wave-10 boundary, 4-type rotation, regressions). Manual play: waves drive visibly. Final play/stop twice: 0/0.
- Next: M8.2 Wave Difficulty Growth.

## M8.2 — Wave Difficulty Growth (2026-08-16)
- EnemyStats.WaveMultiplier (runtime, non-serialized): MaxHP/Damage/MoveSpeed × multiplier; AttackRange/AttackCooldown unchanged. Chain: WaveManager (per-wave multiplier W1 1.00→W10 1.45 in WaveTable) → SpawnEnemy(prefab,pos,mult) → EnemyController.Spawn sets stats then ResetForSpawn (scaled HP). OnDespawn resets multiplier → no pool leak. EnemyData assets untouched.
- Verified: 39/39 probe PASS (table, scaling per type, Range/Cooldown unchanged, injection order, pool reuse reset, W10 1.45, assets unchanged, regressions). Manual play: escalating difficulty visible. Final play/stop twice: 0/0.
- Next: M8.3 Boss (Wave 10).

## M8.3 — Boss (Wave 10) (2026-08-16)
- BossData : EnemyData (MaxHP 500/Dmg 20/Speed 1.5/Range 1.5/Cd 1.0 — impl params); BossAI pursuit + contact damage via CombatSystem (player-only, cooldown); EnemySpawner.SpawnBoss (same pool); Wave 10 = boss encounter (1 boss, W10 mult 1.45, no normal spawns) → BossSpawned → BossDefeated (via EnemyKilled match) → Victory. Boss death keeps EnemyDied/EnemyKilled/Pickup chain.
- Verified: 47/47 probe PASS. Final play/stop twice: 0/0.
- M8 tasks complete; Next: M8 Final Regression & Acceptance.

## M8 — Wave System COMPLETE (2026-08-16)
- M8.1 42/42, M8.2 39/39, M8.3 47/47, M8 Final 58/58 — all PASS. Full flow MainMenu→Playing→W1..W9→W10 Boss→BossDefeated→Victory verified; pause freeze; restart W1; boss contact damage; no wave 11; pools/weapons/EventBus regressions. 3 final Play/Stops 0/0.
- Git clean at 8d20ede; temp probes deleted.
- Next: M9.1 XP Level Up.

## M9.1 — XP Level Up (2026-08-16)
- PlayerProgress: Level (1) + XPToNextLevel placeholder (100×level, non-design) + AddXP carry-over + multi-level + PlayerLevelUp event (once per level). PlayerLevelSystem (on Player): Playing → LevelUp on level-up; non-Playing protected. WaveManager untouched (LevelUp freezes/resumes wave automatically).
- Verified: 41/41 probe PASS. Final play/stop twice: 0/0.
- Next: M9.2 Upgrade Chooser (3 random options).

## M9.2 — Upgrade Chooser Logic (2026-08-16)
- UpgradeData : SO (UpgradeId/DisplayName/StatType(10)/Amount additive) + 10 assets (placeholders: MaxHP+10, HPRegen+0.5, MoveSpeed+0.5, Damage+1, AttackSpeed+0.1, CritChance+0.02, CritDamage+0.25, Range+0.5, PickupRange+0.5, Armor+1 — non-design). PlayerStats runtime bonus layer (base + bonus; ApplyUpgrade; ResetForRun; base untouched). UpgradeManager (on Player, pool 10): pending queue, 3 unique random options, SetForcedOptions hook, Select guards + apply once + UpgradeSelected(upgrade,level) + Playing when drained; LevelUp freezes/resumes wave (WaveManager untouched).
- Verified: 58/58 probe PASS. Final play/stop twice: 0/0.
- Next: M9.3 LevelUp UI Panel.

## M9.3 — LevelUp UI Panel (2026-08-16)
- UpgradeOptionsGenerated(option0..2) event published by UpgradeManager after Options fully written. LevelUpPanel (UI ns, component on ACTIVE Canvas — inactive never Awakes): GameStateChanged show/hide, event-driven 3-button labels (DisplayName/StatType/+Amount), onClick → Select(i). Scene: Canvas (Overlay+Scaler 1920x1080+Raycaster) + EventSystem(InputSystemUIInputModule) + LevelUpPanel (Title+3 buttons, inactive). Consecutive pending level-ups refresh in place; hides on Playing.
- Verified: 55/55 probe PASS (real button clicks, refresh across sets, no duplicates, guards). Final play/stop twice: 0/0.
- Next: M9.4 Shop.

## M9.4 — Shop (2026-08-16)
- TrySpendGold (only spend entry). ShopItemData : SO (Weapon/StatBonus, price, weaponPrefab | Upgrade ref) + 14 assets (weapon 30 / stat 20 — placeholders). ShopManager (scene): WaveCompleted W1..W9 → 4 products (2 weapon + 2 stat, unique) → Shop; Purchase (stat via ApplyUpgrade; weapon via Instantiate+Equip empty slot, gold AFTER equip, already-owned/no-slot rejected no-spend); Refresh 20 flat (placeholder); Continue → Playing (wave resumes). ShopPanel on Canvas (LevelUpPanel pattern) + ShopProductsGenerated event. Single Canvas/EventSystem.
- Verified: 58/58 probe PASS (gold rules, W1→Shop freeze, purchases, already-owned PulseGun, no-slot, refresh, continue, W10→Victory no shop, UI single instance, regressions). Final play/stop twice: 0/0.
- Next: M9.5 Weapon Upgrade.

## TMP CJK Font Integration (2026-08-16)
- Noto Sans SC static SDF font asset (Assets/Fonts/NotoSansSC SDF.asset + sub-asset atlas/material) baked from Assets/Fonts/NotoSansSC-Regular.otf (OFL-1.1) covering all 71 UI chars + punctuation (lookup=83, missing=0). All scene TMPs wired to NotoSansSC SDF directly (no fallback chain). Shop play-mode screenshot proves full Chinese rendering with NO □ placeholders. LevelUp NRE is a pre-existing UI bug, not font-related.

## M9.4 ShopPanel Layout Fix (2026-08-16 21:43)
- SC_Main.unity 4×ProductCard 子节点 anchoredPosition: Name (196,-32) / Type (-91,-29) / Price (166,29) / BuyButton (-74,37)。修复 anchoredPosition 当成 edge-margin 写入但 Unity 是 pivot 中心偏移的语义误用。
- Verified Play Mode: 16 子节点全 inside card rect；3 分辨率（1920×1080/1280×720/1024×768）实际截图 Name 左上/Type 右上/Price 左下/BuyButton 右下；Shop 功能完整回归（W1→Shop/购买-20/Refresh-20/Continue/W10 不进 Shop）；中文渲染 mesh.verts>0。最终 Play/Stop×2 0/0。
- 字体 asset 在 Play 期间被写坏（atlas 1×1），`git checkout HEAD` 从 HEAD 恢复。
- 未改任何 Shop 玩法/逻辑/字体代码。

## M9.4 Shop Type/BuyButton Layout Fix v2 (2026-08-16 22:04)
- SC_Main.unity 4×ProductCard 内 Type/BuyButton 间距消除重叠：
  - Type: pos (-91,-29)→(-91,-15), size 150×26→150×22, fontSize 20→15
  - BuyButton: pos (-74,37)→(-74,32), size 120×46 (保持)
- 用户原指定值 Type(-91,-32)/Buy(-74,39) 实测仍重叠（卡片坐标 -9px），因 anchor(1,1) pos y 越负越靠下 + anchor(1,0) pos y 越正越靠上（相向移动）+ size 26→22 半高变 11 而非 13。已按 ≥10px 验收修正为 -15/32。
- fontSize 20→15 是为满足"Type textBounds 不超出 rect"验收（Noto Sans SC 行高 1.448×fontSize）。
- Play 验证 4/4 PASS：rectGap=15 卡片 px、textBounds 21.72≤rect 22、textGap=11.7>0、无重叠。三分辨率实际截图确认 Type/BuyButton 明显分离。Shop 功能回归全通过。最终 Play/Stop×2 0/0。
- 未改 Shop 玩法/逻辑/字体资源代码。

## M9.4 Shop Weapon Purchase Owner Fix (2026-08-16 22:27)
- ShopManager.TryPurchaseWeapon：Instantiate 后加 `instance.transform.SetParent(_weaponManager.transform, false)` —— 修复武器挂 Scene Root → Owner=null → 无法攻击的缺陷。parent 用 WeaponManager（Player 组件）transform，无全局查找。
- Verified Play：51/51 PASS（四武器 parent==Player / Owner==Player / DamageApplied source==Player / 已拥有+无空槽拒绝 / wave-resume 攻击）。
- M9.5 尚未开始。

## M9.5 — Weapon Upgrade (2026-08-16)
- WeaponController runtime layer: WeaponLevel(1) + Damage/AttackCooldown/Range bonus(0); EffectiveDamage/EffectiveAttackCooldown(max 0.05 floor)/EffectiveRange; ApplyWeaponUpgrade (TargetWeapon match + additive + level++); ResetWeaponUpgrades. WeaponData never mutated.
- WeaponUpgradeData SO (UpgradeId/DisplayName/TargetWeapon/StatType{Damage,AttackCooldown,Range}/Amount) + 12 assets (PulseGun/ScatterBlaster/Boomerang/ArcBlade x Damage+1/Cooldown-0.05/Range+0.5 — IMPLEMENTATION values).
- ShopItemType += WeaponUpgrade; ShopItemData += weaponUpgrade ref; 10 legacy StatBonus assets migrated itemType 1->2 (enum reorder).
- ShopManager: TryPurchaseWeaponUpgrade (equipped.Data == TargetWeapon, gold check->apply->spend, fail spends nothing); GenerateProducts 1 Weapon + 1 WeaponUpgrade (owned-only, else stat fallback) + 2 StatBonus; LevelOfEquipped.
- ShopPanel: WeaponUpgrade multi-line 武器名/升级：属性/等级：Lv.X→Lv.X+1/价格.
- 12 ShopItemData WeaponUpgrade products (price 30) added to pool (26 total). No level cap (design未规定).
- Verified: M95WeaponUpgradeProbe 42/42 PASS; final play/stop x2 0/0.

## M9 Final Regression & Acceptance (2026-08-17) — M9 COMPLETE / ACCEPTED
- Full M9 chain verified end-to-end: XP→LevelUp→UI real clicks→Shop (stat/weapon/upgrade purchases)→Refresh→Continue→W9 Shop→W10 no-shop→Boss; M6/M7/M8 regression PASS (4 weapons, pools, Boomerang ActiveCount, no duplicate WaveStarted, W10 no shop).
- **82/82 PASS, 0 FAILURES** (temporary M9FinalRegressionProbe, deleted).
- 3× independent Play/Stop: 0 errors / 0 warnings each; cross-session state clean (PulseGun Lv1, Boomerang ActiveCount 0, panels hidden, MainMenu).
- Final Git clean; no production code changed during regression.
- **M9 = COMPLETE / ACCEPTED.** No new blocking issues.

## Shop WeaponUpgrade UI Final Layout Fix (2026-08-17)
- WeaponUpgrade card Name = single two-line TMP: line1 weapon name **24px** (identical to normal cards), line2 `升级：{属性}  等级：Lv.{X} → Lv.{X+1}` at **15px** via rich text `<size=15>` (dynamic WeaponLevel, never hardcoded). Name rect 360×32→360×60 at runtime (WeaponUpgrade only; normal cards stay 360×32 fs24 MiddleLeft).
- Price 22px / Type 15px / BuyButton 24px unchanged; independent Level row removed (4 Level children deleted, ShopPanel.levelTexts cleared).
- Verified Damage / AttackCooldown / Range variants: Name fully in-card, no textBounds overlap (Name vs Price/Type/BuyButton separated), longest text "升级：攻击速度" fits, Chinese renders without □, Console 0/0, Git clean.
- Commit `12f17e4`. Normal Weapon / StatBonus cards visually unchanged.

## Next Step
M16 — Final GitHub Showcase Acceptance（IN PROGRESS；M16 WebGL Online Demo / Deployment COMPLETE / ACCEPTED，URL https://void-survivor-9vg.pages.dev/；Screenshot Showcase Review / Packaging COMPLETE / ACCEPTED；M16 README COMPLETE / ACCEPTED；GitHub baseline synchronization COMPLETE / ACCEPTED；M15 — Release COMPLETE / ACCEPTED；M14 — STOPPED / CLOSED WITHOUT FURTHER OPTIMIZATION；M13 COMPLETE / ACCEPTED）。后续：Full browser gameplay acceptance（real-browser 复核 outstanding）/ Showcase visual cleanup / final showcase acceptance（pending）。

## M10.1 — Boss Base Framework (2026-08-17)
- 现有 M8.3 Boss 实现已完整覆盖 M10.1 职责（BossData/BossAI/W10 流程/事件/pool），M10.1 不修改任何生产代码，仅正式验证。
- M10_1BossProbe 29/29 PASS（数据/prefab/缩放/死亡链/State Victory/No wave11/pool reuse 干净/reused chain 1/1/1/1/回归），最终 Play/Stop×2 0/0，字体未写坏。
- M10 = IN PROGRESS（仅 M10.1 完成，M10 整体未 COMPLETE）。Next: M10.2（待任务）。

## M10.2 — Boss Projectile Skill (2026-08-17)
- BossAI 唯一主动技能：每 3s 向玩家当前位置发射 1 个 PulseProjectile（复用共享池+CombatSystem，source=Boss，damage=runtime Stats.Damage 继承 WaveMultiplier，lifetime 3s 命中一次 Release，方向锁定不追踪）。Boss prefab 挂 projectilePrefab。
- M10_2BossAbilityProbe 21/22 PASS（唯一 FAIL 为 probe 订阅时序，独立验证确认 DamageApplied 正常）；2× Play/Stop 0/0；字体未写坏。
- M10.1 + M10.2 + M10.3 COMPLETE；**M10 = COMPLETE / ACCEPTED**。
- M11.1+M11.2+M11.3+M11.4 COMPLETE；**M11 = COMPLETE / ACCEPTED**。
- M12.1+M12.2+M12.3+M12.4 COMPLETE；**M12 = COMPLETE / ACCEPTED**。
- M13.1+M13.2+M13.3+M13.4 COMPLETE；**M13 = COMPLETE / ACCEPTED**（M14 NOT STARTED）。

## M10.2 — Boss Projectile Skill 参数（implementation parameters，非 GAME_DESIGN balance）
- BossSkillCooldown = 3.0 s；BossProjectileSpeed = 6.0；BossProjectileLifetime = 3.0 s；BossSkillRange = 10.0；BossProjectileDamage = Boss 当前 runtime Stats.Damage（自动继承 WaveMultiplier）。
- 触发：Boss 存活 + GameState==Playing + Player 存活 + 距离≤10 + 冷却完成 → 发射 1 颗，方向锁定发射瞬间玩家位置，不追踪；命中 Player 经 CombatSystem（Source=Boss）一次伤害后 Release；lifetime 到期 Release；Boss OnSpawn reset skill cooldown/state。
