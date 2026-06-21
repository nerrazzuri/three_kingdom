# 会话状态 — Vertical Slice 实现中（汜水小城守御）

> **最后更新**：2026-06-21
> **语言**：全程中文（见用户偏好 memory）
> **审查模式**：lean

<!-- STATUS -->
Epic: Vertical Slice
Feature: 汜水小城守御
Task: ✅ Slice 完成（26 测试绿）— 裁定 PROCEED；下一步 /gate-check pre-production
<!-- /STATUS -->

## ▶ VERTICAL SLICE 检查点（2026-06-21 启动）

- **概念名**：three-kingdom-siege（汜水小城守御战）
- **验证问题**：新玩家从守城开局出发，无引导体验到「我赢因我创造了条件，而非点了技能」，且完整循环能在 2 周内以接近量产品质实现于纯 C# 四层架构？
- **构建形态**：headless C# 控制台（.NET 10），驱动真实 Domain；文本 UI。自检 `dotnet run` / `dotnet test`。
- **范围内系统**：GDD_001/002/003/004/005/006/007+008/009/010/011/012（外交受控入口 §8）
- **3 条破局链**：假退伏击 / 断粮疲敌 / 守城待变（玩家选其一或组合）
- **品质**：占位符文本 UI，无美术资产；数值与因果可读性优先
- **目录**：`prototypes/three-kingdom-siege-vertical-slice/`
- **铁律**：production 从头重写、永不 import prototypes；每文件带 slice 头注释
- **时限**：2 周；Day 3 检查点（完整循环不可演示则停）
- **当前阶段**：Phase 4 — Implement（搭骨架 → 模块 0/1）
- **Velocity Log**：
  - Day 1（2026-06-21）：✅ 项目骨架（.NET 10 控制台 + NUnit 测试项目）；模块 0（Fixed Q16.16 / DetRng 确定性流）；模块 1（Time）；Config（数据驱动）；Forces（断粮唯一施加点 + 溃逃）；BattleResolver（≤5 决定性因素、确定性）；SiegeState 聚合 + Application Service（Command 路径）；SiegeScenario 工厂。**断粮疲敌链端到端跑通**：对照组正面硬守战力比 1.43→城破；实验组断粮疲敌 0.59→击退守城成功。**13/13 测试绿**（Fixed/DetRng 确定性/断粮单点/时间/翻盘不变量/状态哈希复现/命令校验）。`dotnet run` + `dotnet test` 均自检通过。
  - Day 2（2026-06-21 同会话续）：✅ 链 2 假退伏击（Commander 性格 + AmbushResolver 三分支：弄假成真/敌不追/伏击成立；突然性压制守方 ×0.55 + 伏兵 ×1.6）；链 3 守城待变（Weather 确定性天气 + DiplomaticPledge/DiplomacyEvaluator 外交受控入口 GDD_012 §8：grant_score 判定/延迟交付/可背约/代价；援军→兵力、补给→后勤、时限→压力）；**交互式 harness**（InteractiveSession：状态面板 + 命令菜单 + 因果流 + 胜负判定，`dotnet run play`）。三条链脚本演示 + 交互模式均跑通。**22/22 测试绿**（新增伏击三分支、外交 grant/拒绝/延迟/必结算/援军击退）。
  - 三链结果：对照组正面硬守 1.43→城破；链1 断粮 0.59→击退；链2 伏击 1.42→追击支队重创(敌1200→826)；链3 守城待援 0.74→击退。
  - Day 2 续（用户反馈）：✅ **军师建议层（GDD_008）** Advisor — 观察 + 候选路线(所需/风险/操作) + 缺失情报 + 置信 + 「不替你定计/不保证」免责；harness 前期 3 回合自动显示（新手上手），之后 `?` 随时调出。守住设计锁：建议不排优劣、不选最优、不暴露真值。22/22 测试仍绿。
  - Day 2 收尾：✅ **存档 round-trip**（SiegeState.Capture/Restore memento 含 RNG 状态 + Infrastructure SiegeSaveSerializer 版本化信封/原子写/迁移链占位，ADR-0005）；4 项存档测试（哈希保持/读档后续推进确定性/在途外援存活/未来版本拒绝）。**26/26 测试绿**。✅ **REPORT.md 写就 — 裁定 PROCEED**（三条链体验成立 + 架构可行性验证 + 速度远超预算；限定：未验证 Unity 表现层/序列化适配）。prototypes/index.md 建立。
  - Day 2 playtest 迭代（用户反馈「断粮为何单边必胜？敌军也该有补给/援军，且须靠侦察判断」）：✅ **断粮改双边博弈**（RaidStrengthPerUnit vs EnemyEscortStrength 拉锯 + ApplyResupplyPush 敌补给车队回补；投入不足则徒劳）；✅ **敌军自身援军**（EnemyReinforceSegment=14 定时抵达，消耗赛时间压力）；✅ **情报雾 EnemyIntel（GDD_007）**——玩家只见估计值/置信/时效，新增侦察命令(7)刷新、随时间衰减；军师改读知识投影；面板敌军行改显探报。memento 扩展含 intel/scout-rng/reinforced。**33/33 测试绿**（新增双边博弈/敌援军/情报雾 7 测）。REPORT 更新含此关键设计修正。
  - **设计者试玩签核（2026-06-21）**：用户亲手 `dotnet run play` 后确认「不确定性会给玩家更好的一线体验」→ PROCEED 由「实现者自测」升级为「设计者验证」。REPORT playtest 段已记。
- **★ Vertical Slice 完成（lean 模式，CD-PLAYTEST 跳过）。裁定 PROCEED（设计者签核）。**
- **下一步（Phase 8 PROCEED 路径）**：`/gate-check pre-production` 正式推进 Production → `/create-epics layer:foundation` / `layer:core` → `/sprint-plan`（用 REPORT velocity）。注意 G6 门禁，control-manifest 待 slice 完成回填。
- **未提交 git**：本 slice 全部改动待用户指示 commit/push。
- **关键结构**：`src/Domain/{Numerics,Time,Config,Forces,Battle,Siege}` + `src/Application` + `src/Program.cs` + `tests/`
- **构建命令**：`cd prototypes/three-kingdom-siege-vertical-slice/src && dotnet run`；测试 `cd ../tests && dotnet test`

---


## 当前任务

★ 阶段已推进：**Pre-Production**（production/stage.txt 已更新）★
`/gate-check pre-production` 重跑（2026-06-21）→ **裁定 CONCERNS**（首跑 FAIL，补 art-bible 后升级），用户接受 CONCERNS 推进。

**当前正在做：G3 跨系统审查（/review-all-gdds）** — vertical slice 已暂停，待 G3 完成后恢复。
理由：control-manifest（v1）明文「下一门禁为 G3」，且 G3 真实未跑（无 cross-GDD review 报告）；
vertical slice 属 G6，不应抢在 G3 前搭在未对账的跨系统契约上（PR 总监建议一致）。用户 2026-06-21 拍板先跑 G3。

**Pre-Production 推荐顺序（修正）**：
1. ★进行中★ `/review-all-gdds`（G3 跨系统矛盾审查）→ 回写阻断项 → 刷新 control-manifest 门禁状态。
2. `/vertical-slice` — 恢复，前置验证乐趣，再写大量 story（勿先连跑两个地基 sprint）。
3. 首段 Domain 代码（EPIC_001，如 ADR-0004 FixedPoint/状态哈希）落地时**同步补示例测试 + 配 UNITY_LICENSE 跑通 CI**。

**进入 Production 的硬前置（务必在首个 Logic story 标 Done 前关闭）**：
- 示例测试文件（.cs）跑通 + CI 至少一次绿灯（需先配 `UNITY_LICENSE` secret，许可激活有外部延迟风险，宜并行处理）。

**Pre-Production 内管理项（CD/TD/PR 的 CONCERNS，非阻断）**：
- CD：支柱4「外交/天下局势」无 GDD 归属 → story 拆分前指派单一受控外交接口（落点 GDD_003 或 GDD_009）；010/008/007 补 CD-GDD-ALIGN 签核。
- PR：无时间线/容量基线 → 出粗里程碑日历 + EPIC_001 容量探针标定 solo 速度；control-manifest 门禁状态刷新。
- 次要清理项：主架构文档命名 architecture-overview.md（闸门期望 architecture.md）；可追溯索引命名 architecture-traceability.md（期望 requirements-traceability.md）。实质存在、TD 已审稳健，可日后改名。

注意：用户偏好——公式/草稿提炼完直接写入，不再逐份询问是否可写入。

## 项目背景

- 项目：《三国演义：兵法沙盒》— 离线单机三国沙盒战略 RPG
- 引擎：Unity 6.3 LTS + C#（ADR-0001 锁定，已配置）
- 阶段：Technical Setup（`production/stage.txt`）
- 原始文档来源：`D:\Projects\三国演义\docs`（外部，已复制迁移，原件保留作备份）

## 采纳计划进度

### ✅ 已完成
- **Step 1a** — 全部外部文档迁移至模板路径（design/gdd/、design/concept/、docs/architecture/、production/）
- **Step 1b** — 引擎配置：CLAUDE.md + technical-preferences.md 写入 Unity 6.3 LTS + C#，@import 改为 unity
- **Step 1c** — ADR-0001 添加 `## Status` = Accepted
- **Step 2a** — 全部 13 份 GDD 移除节标题编号前缀（`## 1. System Purpose` → `## System Purpose`）
- **Step 2c** — ADR-0001 添加 `## Engine Compatibility`（HIGH 风险标注）+ `## ADR Dependencies`
- **Step 4c** — ADR-0001 添加 `## GDD Requirements Addressed`

### ✅ Step 2b — GDD Formulas 节（13/13 全部完成）
全部 13 份 GDD 已写入 `## Formulas` 节，位置在 Main Rules 之后、Data Model 之前。
已用脚本验证 13 份均含 `## Formulas` 标题。每份均遵循 design-docs.md 强制要求
（变量定义表 + 编号公式 + 约束 + 数值示例），并贯彻设计锁（无条件计策按钮禁止、
确定性可复盘、资源守恒、三维状态不合并、不完全信息四层分离）。

### ✅ 已完成（续）
- **Step 3b** — control-manifest.md 添加 `Manifest Version: 1`
- **Step 4b** — 全部 13 份 GDD 添加 `**Status**: Draft`（保留中文行双语并存）
- **Step 4e** — systems-index.md 无括号状态值（概念地图）；附带修复 gdd-index.md 13 个断链

### ⏳ 待办（计划剩余项，均为技能驱动或设计驱动）
- **Step 2d / 3a** — 运行 `/architecture-review` 初始化 tr-registry.yaml（重头戏，读全部 GDD+ADR）
- **Step 3c** — 运行 `/sprint-plan update` 创建 sprint-status.yaml
- **Step 3d** — 运行 `/gate-check Technical Setup`（验证能否进入 Pre-Production）
- **Step 4a** — GDD 节名调整：用户已选"最小改动"保留英文节名（System Purpose 等），
  故**不**重命名；接受部分技能可能用内容匹配而非精确标题匹配的折中。可日后按需调整。
- **Step 4d** — 创建 ADR-0002~0005：
  - [x] ADR-0002 架构分层 — Accepted（注册表 4 项 stance）
  - [x] ADR-0004 确定性战斗模拟 — Accepted（注册表 3 项 stance：定点策略/状态哈希/float 禁止）
  - [x] ADR-0005 存档版本与迁移 — Accepted（注册表 3 项 stance：save_format/save_repository/Unity 序列化禁止）
  - [x] ADR-0003 数据驱动配置 — Accepted（注册表 2 项 stance：config_pipeline/config_loader）
  ★ 4 份必需 ADR 全部完成（含 ADR-0001 共 5 份 Accepted）★

### 架构审查后的覆盖率变化（供下次 /architecture-review 参考）
ADR-0002 + ADR-0004 已覆盖原 17 缺口中的约 19 条 TR（含原部分覆盖项的正式锁定）。
剩余主要缺口指向 ADR-0005（存档序列化：TR-time-003/intel-003/save-003）与
ADR-0003（数据驱动配置的正式锁定）。

### 注册表状态（docs/registry/architecture.yaml，version 5）
- state_ownership: all_authoritative_gameplay_state → domain-layer
- interfaces: player_command_path、battle_state_hash、save_repository、config_loader
- api_decisions: battle_numeric_strategy（整数/定点）、save_format（DTO+JSON）、config_pipeline（SO→不可变）
- forbidden_patterns: domain_depends_on_unity、direct_cross_system_state_write、
  scriptableobject_as_runtime_authority、implicit_global_random、
  float_in_domain_authoritative_path、unity_serialization_of_domain

### 下一步（下次会话）
1. ★开新会话★ 跑 `/architecture-review` 验证覆盖率（5 份 ADR 后应接近全覆盖）——
   不可在本撰写会话内跑，审查 agent 须独立。
2. gate-check 前置仍缺：/test-setup（tests 目录+CI）、/ux-design（无障碍+交互模式）。
3. 之后 /gate-check Technical Setup 验证能否进入 Pre-Production。

## Formulas 节起草方法（保持一致风格）

每份 Formulas 节遵循 `.claude/rules/design-docs.md` 强制要求：
1. 顶部引言：声明数值来自版本化配置、不硬编码、确定性
2. 「变量定义」表格：变量 | 含义 | 范围/单位 | 来源
3. 编号公式块：每个含约束说明 + 具体数值示例计算
4. 回应该 GDD 的 §Test Requirements（尤其确定性/重放要求）
5. 插入位置：`## Main Rules` 之后、`## Data Model` 之前（模板 Detailed Rules → Formulas 排序）

## 下一步

继续 Step 2b：从 gdd-004-city-economy.md 起草 Formulas 节，逐份给用户审阅后写入。
读取该 GDD 全文 → 提炼公式草稿 → 展示 → 批准后 Edit 插入 → 更新本状态文件。

## 恢复指引

新会话开始时：
1. 读本文件
2. 读 `docs/adoption-plan-2026-06-21.md` 查看完整计划与勾选状态
3. 从「进行中：Step 2b」的下一个未勾选 GDD 继续

## Session Extract — /architecture-review 2026-06-21（初版）
- Verdict: CONCERNS
- Requirements: 30 total — 1 covered, 12 partial, 17 gaps
- New TR-IDs registered: 30
- GDD revision flags: None
- Top ADR gaps: ADR-0002 架构分层, ADR-0004 确定性战斗模拟, ADR-0005 存档版本与迁移
- Report: docs/architecture/architecture-review-2026-06-21.md

## Session Extract — /test-setup 2026-06-21
- Verdict: COMPLETE — Unity Test Framework 脚手架 + CI 接通
- 创建: tests/{README,unit,integration,smoke,evidence,EditMode,PlayMode} + .github/workflows/tests.yml
- 待办: 配置 UNITY_LICENSE secret；随首段 Domain 代码补 1 个示例测试（gate 要求）

## Session Extract — accessibility-requirements 2026-06-21
- Verdict: COMPLETE（In Review）— accessibility-specialist 起草，本会话写入
- File: design/accessibility-requirements.md（路径选 design/ 根，匹配 gate 检查；非 design/ux/）
- 基线: WCAG 2.1 AA；MVP 15 项必达 / 应达 6 / Future 7；10 条验收；6 开放问题
- 与模式库交叉引用 P1/P3/P5/P7/P9/P12，无矛盾
- 开放问题重点: OQ-03 须开 Spike 实测 Unity 6.3 UI Toolkit AT API 能力
- Next: /ux-review 验证两份 UX 文档

## Session Extract — /ux-design patterns 2026-06-21
- Verdict: COMPLETE（In Review）— 交互模式库 12 条，从 13 份 GDD UI Requirements 种子化
- File: design/ux/interaction-patterns.md
- 模式: P1-P12（数据显示/反馈模态/HUD/全局约束）；P3/P11/P12 为横切约束
- 缺口记录: 无 player-journey、无 accessibility-requirements.md（建议 accessibility-specialist 补）
- 平台确认: PC（键鼠为主、无触控）— 用户 /btw 确认
- /ux-review interaction-patterns: APPROVED（补 Standard Controls/Animation/Sound 三节后）；Status=Approved
- 全部 4 项 architecture-review pre-gate 缺口已补齐

## Session Extract — /architecture-review 2026-06-21（复审）
- Verdict: CONCERNS（极轻微，0 阻断项）
- Requirements: 31 total — 28 covered, 3 partial, 0 gaps（初版「30」系少计 1 条，已更正）
- New TR-IDs registered: None（31 条已全部登记，注册表无变更）
- GDD revision flags: None
- Cross-ADR conflicts: None（依赖图无环，5 份 ADR 全 Accepted）
- 部分覆盖项（仅需可选可追踪性列名补充）: TR-map-001, TR-council-002, TR-supply-001
- Pre-gate 缺失: /test-setup（tests+CI）、/ux-design（无障碍+交互模式）→ 阻断 /gate-check pre-production
- Report: docs/architecture/architecture-review-2026-06-21.md（已覆盖初版）

## Session Extract — /gate-check pre-production 2026-06-21
- 闸门: Technical Setup → Pre-Production；审查模式 lean（四总监 PHASE-GATE 全跑）
- **裁定: FAIL**（Chain-of-Verification 5 问已核，含 2 项工具复核，裁定不变）
- 必备制品 11/13；硬阻断 = 美术圣经缺失
- Director Panel: CD=CONCERNS / TD=CONCERNS / PR=CONCERNS / **AD=NOT READY**（升级规则→整体最低 FAIL）
  - CD: 支柱4外交无 GDD 归属（影响"守城待变"条件链）；010/008/007 未留 CD-GDD-ALIGN 签核
  - TD: 架构本体 READY；无示例测试+CI 未跑通=进 Production 硬前置（非进 Pre-Production 阻断）
  - PR: 无时间线/容量基线 → solo 范围现实性无法证伪；建议 vertical slice 前置验证乐趣；
        control-manifest(v1) 门禁状态脱节，需澄清 G3 是否补跑 /review-gdds
  - AD: art-bible 缺失；art-direction.md 覆盖 60-70%，缺色彩量化锚/字体排版/资产技术规格（越过最晚责任时刻）
- 用户决定: ①现在运行 /art-bible ②本闸门报告只更新 active.md，不单独存档
- 最小路径转 PASS: /art-bible（Section 1-4）+ 补一示例测试（或显式签字后置）→ 重跑 gate
- 落盘: 仅本 active.md（用户选项）；未写 production/gate-checks/

## Session Extract — /gate-check pre-production（重跑）2026-06-21
- 触发变化: art-bible 已补 → 美术总监三项具名硬阻断（色彩量化锚/字体排版/资产规格）全部解除
- **裁定: CONCERNS（首跑 FAIL → 升级）**；无阻断残留
- Director Panel: CD/TD/PR=CONCERNS（不变，沿用）/ **AD=READY**（art-bible Section 4+8 客观满足）
- 成本审慎: 未重复冷启动 CD/TD/PR 子代理（结论无变化变量）；AD 阻断按文件客观核验解除
- 唯一剩余必备制品缺口: 示例测试文件 — TD 背书后置（进 Production 硬前置，非进 Pre-Production 阻断）
- **用户决定: 推进阶段 → production/stage.txt = Pre-Production**
- Chain-of-Verification: 5 问已核（含 grep art-bible 章节头 / find *.cs 两项工具复核）— 裁定不变 CONCERNS

## Session Extract — 状态盘点 + 全量入库 GitHub 2026-06-21

- **触发**：用户「check latest status … record everything before stop … 所有内容先 git 到 github 上」
- **当前阶段**：Pre-Production（`production/stage.txt`）；审查模式 lean（`production/review-mode.txt`）
- **git 起始状态**：分支 main（与 origin/main 同步、无未推送提交）；5 个已跟踪文件被修改 + 30 项未跟踪 = 35 项
  - 修改：`.claude/docs/technical-preferences.md`、`CLAUDE.md`、`docs/CLAUDE.md`、`docs/architecture/tr-registry.yaml`、`docs/registry/architecture.yaml`
  - 新增（未跟踪重点）：`design/`（gdd/art/concept/ux + accessibility-requirements.md）、`docs/architecture/`（ADR-0001~0005 + index + overview + traceability + control-manifest + review）、`docs/`（project-brief/test-strategy/balance-strategy/ux-accessibility/adoption-plan）、`production/`（epics/sprints/story-backlog/stage/review-mode/project-stage-report/session-state）、`tests/`、`.github/workflows/`、`.claude/agent-memory/`（art-director + lead-programmer）
- **入库结果**：
  - `origin`（Donchitos/Claude-Code-Game-Studios，上游模板）→ **推送被拒 403**，nerrazzuri 无写权限
  - 用户指定目标 `nerrazzuri/three_kingdom`（私有，default=main）→ 新增 remote `tk`
  - 发现 `three_kingdom` 是**原始源文档仓库**（旧结构 docs/01_concept… 共 32 文件，即 D:\三国演义\docs 备份），与本地模板化工作仓**历史无关**
  - 未覆盖其 main；将本地提交以独立分支 `chore/pre-production-content-snapshot-2026-06-21` 推送到 `tk` ✓
  - 提交 SHA：见 `git log -1`；全部 62 文件已入 GitHub
- **已决定并执行**：用户选「提升为新 main」→ force-push `chore/...` → `tk:main`（覆盖旧的原始文档备份）
  - 旧 `three_kingdom/main` SHA = `d5973d1`（原始源文档备份，**可经此 SHA 恢复**，未删除只是不再被 main 引用）
  - 新 `tk/main` = `32b7174`（含 README 重写 + 全量快照）
  - 本地 `main` 已快进至 `32b7174` 并改跟踪 `tk/main`（以后直接 `git push` 走 three_kingdom）
- **README**：已从模板框架介绍重写为《三国演义：兵法沙盒》项目说明（commit 32b7174）
- **备注**：`.gitignore` 未忽略 `production/session-state/active.md`（与 directory-structure.md 注释「gitignored」不符）；本次按用户「记录全部」意图一并入库，作为会话记录留痕
- **下一步（恢复后）**：跑 `/review-all-gdds`（G3 跨系统审查）→ 回写阻断项 → 刷新 control-manifest 门禁状态

## Session Extract — /art-bible 2026-06-21

- **状态: COMPLETE** — design/art/art-bible.md v0.1 写入
- 覆盖范围: Section 1–4（视觉身份基础）+ Section 8（资产标准）
- Section 5–7（动效/音频/无障碍视觉）骨架已占位，待后续阶段补充
- 关键量化锚:
  - 核心视觉规则: 军府案上的活图卷（三层：地貌底/批注中/情绪顶）
  - 三支柱原则: 信息即美术 / 笔迹分层语气 / 历史质感承载演义气势
  - 五种游戏状态（生活观察/判断布局/行动承诺/战争应变/战果延续）逐一定义光照+情绪+视觉元素
  - 色彩系统: 宣纸白 H35S12L90 / 山水墨 H25S8L18 / 朱批朱红 H12S72L40 / 推测蓝灰 H215S18L55
  - WCAG-AA 对比度: 山水墨~11∶1 ✓；朱批朱红~4.7∶1 ✓（须实测）；推测蓝灰~4.1∶1（仅图形元素用）
  - 三势力旗色: 曹魏铁苍蓝 H210S28L32 / 蜀汉赤金朱 H18S58L40 / 东吴碧江青 H188S42L34
  - 色盲安全: 4 组高风险色对 + 纹样/形状冗余方案 + 强制验证流程
  - 资产分辨率层级: 地图底图 2048→4096 / 人物肖像 256×340 @1x / UI 图标 32 @1x
  - 字号体系: T1-T3 / B1-B2 / C1 / N1-N2，1080p→1440p→4K 三档，最小 11px
  - 命名规范: 12 个 category 前缀（map/mapnode/maproute/char/charfx/ui/uiicon/faction/overlay/event/vfx/bg）
  - 内存预算: @1x ~154MB 压缩后；@2x 全启 ~616MB，总占 ~2.9GB（8GB 内无冲突，真实瓶颈=制作成本）
- 4K vs 内存冲突：**无实质冲突**（技术约束不是瓶颈，制作成本才是）；MVP 阶段 @1x 优先
- 下一步: 重跑 /gate-check pre-production（art-bible 硬阻断已解除；示例测试仍待处理）

## Session Extract — /review-all-gdds 2026-06-21（G3 跨系统审查）

- **执行方式**: inline 重做综合（首轮 3 路子代理产出未落盘，恢复会话后主审查全量重读 13 份 GDD 重综合）
- **裁定: FAIL（1 项阻断）** — 支柱4「外交与天下局势」零 GDD 归属，且「守城待变」核心 slice 条件链依赖未规约外交输入
  - 阻断狭窄可廉价闭合：仅需「一个受控外交入口」附录，非新系统；另两条链（假退伏击/断粮疲敌）已完整规约
- **GDDs reviewed**: 13（+ concept/pillars/systems-index）
- **Flagged for revision**: GDD_012(指派外交入口-Blocking)、010/011/012(断粮传导三处重复-W)、003/007(知识TTL重复-W)、004/011(morale变量名碰撞-W)、001/003(mod_weather范围不一致-W)
- **Blocking issues**: 1 — 外交受控入口缺失（C-BLOCK-1 = D-BLOCK-1 = S-BLOCK-1，三面同根）
- **用户决定（2026-06-21）**: ①写独立报告 + 更新 active.md ②外交受控入口**指派进 GDD_012 后勤**
- **正向强项**: 资源守恒(004↔012)全corpus最强一致点；无竞争进程循环；无反支柱违反；玩家幻想高度一致
- **Report**: design/gdd/gdd-cross-review-2026-06-21.md
- **实体注册表为空**: 建议补跑 /consistency-check 填充

## Session Extract — G3 阻断闭合 + 重判 2026-06-21

- **阻断已闭合**: 外交受控入口写入 **GDD_012 §8**（求援/求粮/求时限三选一作用于 slice，外势力静态背景、非完整天下外交）
  - GDD_012 同步补：Main Rules 条目、§8 公式块（响应判定/交付时间/兑现背约/slice作用/代价兑付）、DiplomaticPledge 数据模型、Dependencies(加 006)、Player Inputs、Failure Cases、Balancing、Test Requirements(支柱4 验收)、MVP Scope
  - 设计锁守住：延迟交付 + 条件判定 + 代价 + 可背约失败 + 确定性可重放 + 守恒（绝非即到保证按钮）
  - systems-index §GDD责任分派 加「外交受控入口：GDD_012 §8」
- **G3 重判: FAIL → CONCERNS**（剩余 5 项 Warning 皆非阻断）
- **control-manifest 刷新**: G3 行 → Passed（CONCERNS）；当前阻断项段更新；下一门禁标注为 G6（恢复 /vertical-slice）
- **5 项 Warning 已全部回写修复（2026-06-21）**:
  - S-W1 断粮传导单点: 012持supply_state发事件 / 011唯一施加morale·fatigue / 010只读（GDD_010§8、011§2、012§5）
  - C-W2 知识TTL: 时效权威归007，003只记observed_time复用007降级（GDD_003§6）
  - C-W1 morale命名: 城市民心 civ_morale / 部队士气 unit_morale 消歧（GDD_004§4·§5、011）
  - C-W3 mod_weather: 天气只减速≥1.0对齐003，地形保留0.5–2.0（GDD_001变量表）
  - C-W4 破环/对称消费者: 补005/003消费者 + 新增 systems-index §跨系统结算顺序（含日界顺序，收S-I1）
- **G3 实质问题全部清零**（阻断闭合 + 5 Warning 全修）；control-manifest G3=Passed(CONCERNS)，下一门禁 G6
- **Recommended next**: 恢复 `/vertical-slice` 前置验证乐趣（active.md 既定）；或先补跑 /consistency-check 填充实体注册表
