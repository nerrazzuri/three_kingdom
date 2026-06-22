# 会话状态 — Foundation 实现中（epic-001）

> **最后更新**：2026-06-22
> **语言**：全程中文（见用户偏好 memory）
> **审查模式**：lean

<!-- STATUS -->
Epic: ★ 全部 9 epics（28 stories）✅ Complete（2026-06-22）
Feature: Domain 四层内核 + 后果结算 + 存档/复现底座
Task: 329/329 全绿 0 warning，全推 tk/main（HEAD=c470fd1）｜下一阶段 ▶ Presentation EPIC_010 或 Unity 表现层切片重验幻想
<!-- /STATUS -->

## ▶ Pre-Production→Production 闸门补完（2026-06-21 续）

首跑 /gate-check pre-production（Pre-Production→Production）裁定 **FAIL**：slice 已 PROCEED（乐趣已验证），但缺 art-bible 完整+签核、三关键屏 UX、epics→stories。用户拍板按 1-2-3-4 顺序补完再回报。
- **✅ Step 1** — art-bible 升 v1.0：补 §5 动效规范 / §6 音频视觉协同 / §7 无障碍视觉标准 / §9 制作管线+**AD-ART-BIBLE 签核 APPROVED**（2026-06-21）。`design/art/art-bible.md`。
- **✅ Step 2** — 三关键屏 UX 写就并 /ux-review **全 APPROVED**（Status=Approved）：`design/ux/main-menu.md`、`hud.md`、`pause-menu.md`。核心设计锁（P10 无真值/P11 无最优解/P6 不合并/P4 双态）全落入验收；无障碍对齐 WCAG 2.1 AA（Comprehensive）。
- **✅ Step 3** — 9 epics（3 Foundation: epic-001/002/009；6 Core: epic-003~008）+ **28 stories**（12 Foundation + 16 Core，17 Logic/11 Integration，0 Blocked）写入 `production/epics/*/`，含 `index.md` 总账。每 story 嵌 TR-ID + governing ADR + AC + QA Test Cases + 测试证据路径。lean 模式跳过 PR-EPIC/QL-STORY-READY 子代理门。
- **▶ Step 4 — 闸门重判：artifact+quality 全绿 → PASS-eligible**。首跑三项硬阻断全闭合（art-bible 完整+签核 / 三屏 UX APPROVED / epics→stories）。剩余皆 CONCERNS 非阻断：①entity-inventory 缺（art-bible §9.2 列为 Production 早期，非硬前置）②主架构/可追溯命名 architecture/-traceability（实质在，待改名 architecture.md/requirements-traceability.md）③playtest 报告在 prototypes/REPORT.md 非 production/playtests/ ④sprint-01.md 仍引旧 STORY_NNN id，待 /sprint-plan 刷新指向新 story 文件。
  - 进 Production 硬前置仍在（首个 Logic story Done 前）：示例测试 .cs 跑通 + CI 至少一次绿。**TD/PR 建议**：Logic story 可用纯 `dotnet test`（NUnit）旁路 Unity 许可跑 CI 绿，UNITY_LICENSE 仅 EditMode 集成测试需要 → 把「dotnet test CI 绿」设为 epic-001 S1 硬验收。

## ▶ 四总监 Panel 裁定（Pre-Production→Production，lean，2026-06-21）

- **CD=CONCERNS / TD=CONCERNS / PR=CONCERNS / AD=READY** → 升级规则取最严 → **整体闸门裁定：CONCERNS**（首跑三硬阻断全闭合；AD 由 NOT READY 升 READY；无 NOT READY 残留）。
- **用户决定（更新）：先做 fix-forward ①②③ 再推进**。三项已完成 → **stage.txt = Production**（2026-06-21）。

## ✅ Fix-forward ①②③ 完成 + 进入 Production（2026-06-21）

**① 低成本 CONCERNS 消化**
- 文档改名为闸门规范名：`architecture-overview.md`→`architecture.md`、`architecture-traceability.md`→`requirements-traceability.md`；全仓引用同步更新（含 .claude/skills adopt/propagate-design-change/quick-start，消除模板命名冲突）。
- `design/accessibility-requirements.md` 跑 /ux-review → **Approved**（与 art-bible §7 + interaction-patterns + 三屏 UX §12 双向一致）。
- `production/sprints/sprint-01.md` 重生：指向真实 story 文件路径（Foundation 5 story，依赖序）+ 容量基线（slice velocity，标定 sprint）+ CI dotnet test 旁路许可为 S1 硬验收。

**② 进 Production 硬前置（CI + 示例测试）**
- 建纯 C# Domain 工程：`src/Domain/ThreeKingdom.Domain.csproj`（netstandard2.1，Unity 兼容，禁 UnityEngine）+ `BuildInfo.cs`（框架确认占位）；测试 `tests/unit/ThreeKingdom.Domain.Tests/`（net10.0 + NUnit）+ `BuildInfoTests.cs`；`ThreeKingdom.slnx`。
- **`dotnet test` 本地绿：2/2 passed**（restore 成功，netstandard2.1 ↔ net10.0 引用通）。
- CI `tests.yml` 重写：`domain-tests` job（dotnet，无许可，gating 绿）+ `check-unity-license`/`unity-tests`（license-guarded，未配则跳过不报红）。**首次 GitHub 绿待 push 验证**（workflow 已结构性无许可依赖）。

**③ 阶段推进**
- `production/stage.txt` = **Production**；control-manifest 当前阶段→PRODUCTION、G6=Passed(CONCERNS)、G7=Unblocked(Foundation)、当前阻断项=无（CONCERNS 列为 guardrail）。

**已入库**：上述全部改动已 commit（`de86317`）+ push 至 `tk/main`（nerrazzuri/three_kingdom，`f9158cf..de86317`）。

## Session Extract — /dev-story 2026-06-21（epic-001 S1 实现）

- **Story**: `production/epics/epic-001-domain-foundation/story-001-domain-test-boundary.md` — 建立纯 C# Domain 与测试边界（Status: In Progress）
- **/story-readiness 裁定**: READY（lean，QL-STORY-READY 跳过）
- **实现方式**: inline（小型反射断言测试，省冷启动 engine-programmer/unity-specialist 子代理；框架骨架闸门②已建）
- **文件**: 新增 `tests/unit/ThreeKingdom.Domain.Tests/DomainBoundaryTests.cs`（3 测）；既有 `src/Domain/{ThreeKingdom.Domain.csproj,BuildInfo.cs}` + `BuildInfoTests.cs`（2 测）+ `ThreeKingdom.slnx`
- **AC**: 全 4 条满足（Domain 无 UnityEngine 引用[反射实证]/独立 NUnit 测试程序集/dotnet test 无 Unity 运行时/示例类型+测试）
- **测试**: `dotnet test ThreeKingdom.slnx` → **5/5 绿**（本地）
- **偏差**: 测试证据路径由 `tests/integration/foundation/...` 调整为统一测试工程 `ThreeKingdom.Domain.Tests`（CI 装配更简）；实现 inline 非子代理（已记）
- **Blockers**: None
- **Next**: `/code-review` 新增文件 → `/story-done` 关闭 S1

## Session Extract — 设计锁红线 + /code-review + /story-done 2026-06-21

- **两条红线正式写入**（约束全部后续产出）：
  - control-manifest §强制设计锁 + art-bible §9.4：①【红线】零复制现有三国游戏/作品资产（原创性，含 AI 仿制/二改/同 IP 全禁；史料+原著可作题材、表达须自创；存疑不入库、违者拒收不 commit；`/asset-spec` 须填「参考来源」字段）②所有文件中文撰写（「中文叙述 + 英文标识符」，代码命名/引擎 API/编号保留英文）。
- **/code-review（epic-001 S1 文件）**: APPROVED（inline；ADR-0002 COMPLIANT、Standards 6/6、新红线合规）。
- **/story-done（epic-001 S1）**: **裁定 COMPLETE WITH NOTES**。Status→Complete；AC 4/4 COVERED；`dotnet test` 5/5 绿；ADVISORY 2 项（测试路径统一到 ThreeKingdom.Domain.Tests / inline 实现）。EPIC.md S1→✅Complete。lean：QL-TEST-COVERAGE 跳过、LP-CODE-REVIEW 由本会话 /code-review APPROVED 充当。
- **DomainBoundaryTests 设为永久回归门**（后续 Domain 程序集禁 UnityEngine）。
- **Next**: commit+push 本批 → 解锁 epic-001 S2（定点/随机流，Depends on S1=Done）。`/story-readiness epic-001 S2` → `/dev-story`。
- **CONCERNS 汇总（均非阻断，Production 早期 guardrail）**：
  - **CD-C1** 非战斗决策空间（人物/关系/城市）仅经设计未经体验验证 → 首个非切片里程碑须体验验证「有意义机会成本」，防漂移成「薄皮战斗沙盒」。
  - **CD-C2** 降输入摩擦 vs P11 无自动化捷径张力，尤其军师层 → 所有降摩擦设计须过 P11 审查；「军师不越界」列专门验收。
  - **CD-C3 / TD** 核心幻想在 Unity 表现层未实证 + 无 player-journey → Production 早期 UI 垂直切片重验幻想；UI Toolkit + 存档平台适配打样测帧/draw-call/内存。
  - **TD/PR** CI 从未跑绿（UNITY_LICENSE 未配/无示例测试）→ 设为 Production 第一动作 + Logic story Done 硬前置；可 dotnet test 旁路许可。
  - **PR** 无里程碑日历/容量基线 → sprint 1 内回填（仅 1 个 slice velocity 数据点）。
  - **PR** sprint-01.md 引旧 story ID → fix-forward 跑 `/sprint-plan new` 重生指向新路径。
  - **PR** epic-009 S3（存档校验）实质依赖 epic-005 S1（情报四层）→ 排程归 Core 期，勿当 Foundation 早期任务（唯一跨层回指，非阻断）。
  - **AD-C1** §4 对比度估算须工具实测（朱批朱红 4.7 / 蜀汉赤金朱 4.6 余量薄）→ 首批资产入库前。
  - **AD-C2** hud.md 引用「art-bible 高对比变体」但 §7 未定义该变体配色 → 高对比功能实现前补 §7。
  - **AD-C3** accessibility-requirements.md 仍 In Review，与 art-bible §7 已对齐 → 跑 /ux-review 更新状态。
  - 次要：ADR 可追溯性列名收尾（TR-supply-001/council-002/map-001 + ADR-0005→0003 Depends On）；架构文档改名 architecture.md/requirements-traceability.md。
- **子代理 ID**（可 SendMessage 续问）：CD=ac814ade4353a5e0e / TD=a1fd5c9cb45a9006d / PR=a0e88328a77c9ff07 / AD=a7e1c42b8c75c452c

## ⏹ 本会话结束（2026-06-21）

- **已完成**：G3 跨系统审查闭环 → Vertical Slice「汜水小城守御」全程（三条兵法条件链 + 军师 GDD_008 + 双边断粮博弈 + 敌军援军 + 情报雾 GDD_007 + 存档 round-trip ADR-0005）。**33/33 测试绿**。裁定 **PROCEED（设计者亲手试玩签核）**。
- **已入库远程**：`tk/main`（nerrazzuri/three_kingdom）至 `f20da44`。工作树干净。
- **未做（有意推迟，省 token，留待干净上下文）**：`/gate-check pre-production`（opus 档，多文档综合裁定）。
- **下次入口**：新会话直接说「跑 /gate-check pre-production」。通过后再 `/create-epics layer:foundation`/`layer:core` → `/sprint-plan`（用 REPORT velocity）。
- **关键参照**：slice 代码 `prototypes/three-kingdom-siege-vertical-slice/`（运行 `cd src && dotnet run` 演示 / `dotnet run play` 交互 / `cd tests && dotnet test`）；裁定与 Lessons 见同目录 REPORT.md。铁律：production 从头重写、永不 import prototypes。

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
- 次要清理项：主架构文档命名 architecture.md（闸门期望 architecture.md）；可追溯索引命名 requirements-traceability.md（期望 requirements-traceability.md）。实质存在、TD 已审稳健，可日后改名。

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

## Session Extract — /story-done 2026-06-21
- Verdict: COMPLETE WITH NOTES
- Story: production/epics/epic-001-domain-foundation/story-002-fixedpoint-rng.md — 定点数值与确定性随机流底座
- Test Evidence: 30/30 passed, 0 warnings（dotnet test ThreeKingdom.slnx）
- Code Review: APPROVED（ADR-0004 COMPLIANT / Standards 6/6 / SOLID / 红线合规）
- Tech debt logged: None
- Next recommended: epic-001 S3 版本化配置加载与校验（ADR-0003）

## ✅ 今日收尾 — 2026-06-22（休息存档）

**本次完成**：epic-001 S2 全链路收口并 push。
- S2 实现 ADR-0004 确定性数值三件套（Domain 权威路径禁 float）：
  - `FixedPoint`（Q16.16，checked 溢出 / decimal 显示 / 向零截断）
  - `IDeterministicRandom` + `DeterministicRandom`（SplitMix64 注入流，position 为权威状态可重建续抽）
  - `StateHasher`（FNV-1a 64 位，显式小端 / 顺序敏感）
- 测试 25 测（FixedPoint 10 / DetRng 8 / StateHasher 6）+ S1 共 **30/30 全绿，0 warning**。
- `/code-review` = **APPROVED**；`/story-done` = **COMPLETE WITH NOTES**（story-002 Status=Complete，EPIC.md S2=✅）。
- **push 成功**：`tk/main` = 本地 `HEAD` = commit **ed1ba8a**（工作区干净，已同步）。
- **顺手修隐患**：手写权威 `.csproj` 此前被 .gitignore 的 Unity 模板规则忽略且从未入库 → slnx 指向未跟踪工程会让干净克隆上 CI 构建失败。已加反忽略例外并纳入 `ThreeKingdom.Domain.csproj` + `ThreeKingdom.Domain.Tests.csproj`（prototype 的 csproj 维持忽略，守铁律）。

### ▶ 明天入口（epic-001 S3）
- **目标**：S3 版本化配置加载与校验（ADR-0003 数据驱动配置；SO 编辑期 → 不可变 Domain 配置 + 配置指纹）。
- **story 文件**：`production/epics/epic-001-domain-foundation/story-003-config-loading.md`
- **建议命令链**（沿用 S1/S2 节奏，lean 模式）：
  `/story-readiness production/epics/epic-001-domain-foundation/story-003-config-loading.md` → `/dev-story` → `/code-review` → `/story-done` → commit+push tk/main
- **注意**：ADR-0003 验收要点 — 非法范围 / 缺失引用被明确拒绝且**无部分写入**；配置指纹确定性。新增 .cs 落 `src/Domain/`，测试落 `tests/unit/ThreeKingdom.Domain.Tests/`（同 S2 结构）。
- **待办（非阻断 guardrail，源自闸门 CONCERNS）**：CI 首次 GitHub 绿仍待验证（push 已多次，可顺带去 Actions 页确认 domain-tests job 跑绿）；entity-inventory、sprint-01 旧 id 刷新等仍挂账。

---

## 🌙 远程自主执行 — 2026-06-22 06:00 起（用户授权离线自动跑）

> 背景：3:05AM 定时器未触发（机器睡眠，ScheduleWakeup 需会话存活）。用户 06:00 回来后改为「现在直接开干」，授权连续跑到 epic-002 完成。

### ✅ epic-001 已完全关闭
- **S3 版本化配置加载与校验**（ADR-0003）：`src/Domain/Configuration/`（ConfigIds/ConfigResult/ConfigSchema/ConfigDraft/ValidatedConfig/ConfigValidator/IConfigLoader）+ 18 测。两阶段校验、错误聚合、**无部分写入**、配置指纹（复用 StateHasher，规范化排序顺序无关）。`/code-review`=APPROVED，`/story-done`=COMPLETE WITH NOTES。commit `cc4605d` → push tk/main。
- **S4 SaveVersion 值对象**（ADR-0005）：`src/Domain/Persistence/SaveVersion.cs`（SaveVersion + SaveCompatibility 三类）+ 26 测。解析/比较/兼容（同主版本可迁移、存档高于当前不兼容不静默降级）、非法版本拒绝、不可变值相等。`/code-review`=APPROVED，`/story-done`=COMPLETE WITH NOTES。commit `ae34330` → push tk/main。
- **全套测试 74/74 全绿，`-warnaserror` 0 warning**。EPIC.md 已标全部 ✅ 并核对 DoD。
- **偏差（ADVISORY）**：两个 story 的测试路径从 `tests/unit/foundation/*.cs` 归一到真实可编译测试工程 `tests/unit/ThreeKingdom.Domain.Tests/`（foundation/ 不在任何 csproj）。

### ✅ epic-002 世界基底 — 全部 5 story 完成（2026-06-22）
- **S1 确定性时间推进**（GDD_001/ADR-0004）：`src/Domain/Time/`（WorldTime+DaySegment、WorldClock+AdvanceTimeCommand、DayBoundaryStage、ScheduledEventOrder）。commit `c595be5`。
- **S2 嵌套战斗时段预算**（ADR-0004+0003）：BattleClock floor(phases/budget) 跨段+天气/补给/疲劳结算、TimedAction/ActionCost、Deadline、CancellationPolicy。commit `a1cb999`。
- **S3 天气/风向确定性解析**（GDD_002/ADR-0004）：`src/Domain/Environment/`（WeatherTransitionTable、WeatherResolver 注入流加权选择、EnvironmentModifierSet、Wind）。commit `027bcf4`。
- **S4 拓扑与确定性寻路**（GDD_003/ADR-0004）：`src/Domain/Map/`（WorldMap、Pathfinder 整数代价+RouteId 字典序平局、RouteCost、Region 容量门控、RouteContact）。commit `98f259f`。
- **S5 真值/知识分离**（ADR-0002，Integration）：MapTruth、FactionKnowledge+只读投影、ScoutingService。commit 见下。
- **测试累计 148/148 全绿，`-warnaserror` 0 warning**。每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。
- **共性偏差（ADVISORY）**：各 story 测试路径从故事原写的 `tests/unit/<sys>/*.cs`、`tests/integration/...` 归一到唯一可编译测试工程 `tests/unit/ThreeKingdom.Domain.Tests/`。

### ✅ epic-003 人物与关系 — 全部 3 story 完成（2026-06-22）
- **S1 人物核心状态**（GDD_005/ADR-0002）：`src/Domain/Characters/`（CharacterState、CapabilitySet、PersonalityProfile、HealthState、TaskCapabilityWeights）。能力→质量系数非解锁。commit `36f0e72`。
- **S2 职责权限与意愿**（GDD_005 §2/§4/ADR-0004）：AuthorityRegistry（能力不绕过授权）、TaskConflictPolicy、WillingnessCalculator（读已结算 coop_score）。commit `17f7adc`。
- **S3 方向性多维关系**（GDD_006/ADR-0004）：`src/Domain/Relationships/`（RelationshipState 四维方向性+事件幂等、CooperationEvaluator coop_score、AuthorityGrant 有效性）。commit 见下。
- **测试累计 181/181 全绿，0 warning**。

### 📌 会话收尾交接（2026-06-22）— 新会话从此处接续

**已完成并全部 push tk/main（本地 HEAD = tk/main = `14f60ce`，工作区干净）**：
- epic-001（S1–S4）✅ 关闭 · epic-002（S1–S5）✅ 关闭 · epic-003（S1–S3）✅ 关闭
- 全套测试 **181/181 全绿，`-warnaserror` 0 warning**
- 自主任务 10 提交：`cc4605d ae34330`（E1 S3/S4）· `c595be5 a1cb999 027bcf4 98f259f 0f79e6b`（E2 S1–S5）· `36f0e72 17f7adc 14f60ce`（E3 S1–S3）

**▶ 下一模块：epic-004-city-logistics（城市与后勤）** — 尚未动工（仅读过 GDD_004，无代码）
- 入口：`/story-readiness production/epics/epic-004-city-logistics/story-001-city-daily-settlement.md` → `/dev-story`
- 3 个 story：
  1. S1 城市日界产耗结算与资源守恒（Logic, ADR-0004；GDD_004：守恒恒等、日界顺序 承诺→产入→消耗→短缺后果→工事/治安、stock≥FLOOR、军粮移交后勤不双计）
  2. S2 三持有者补给守恒与路线断粮传导（Logic, ADR-0004；GDD_012）
  3. S3 外交受控入口 求援/求粮/求时限（Integration, ADR-0002；GDD_012 §8）

**接续约定（沿用本轮节奏，lean 模式）**：
- 每 story 链路：`/story-readiness → /dev-story → /code-review(须 APPROVED) → /story-done → commit+push tk/main`
- 新增 .cs 落 `src/Domain/<模块>/`；测试落 `tests/unit/ThreeKingdom.Domain.Tests/<模块>/`（故事里写的 `tests/unit/<sys>/` 等路径需归一到此唯一可编译测试工程 —— 共性 ADVISORY 偏差）
- 红线：Domain 纯 C# 无 UnityEngine；权威路径禁 float（用 `FixedPoint` Q16.16 / 整数）；确定性同输入同结果；平衡值数据驱动不硬编码；构造校验不变量、失败无部分写入
- 复用底座：`Numerics`（FixedPoint/DeterministicRandom/StateHasher）、`Time`（WorldTime/日界编排）、`Configuration`、`Map`、`Characters`、`Relationships`
- 提交信息体含 `Story: EPIC-004-S0X` + CLAUDE.md 要求的 Co-Authored-By / Claude-Session 尾注；push 目标 remote `tk`（nerrazzuri/three_kingdom），非 origin
- 验证命令：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`

**挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## Session Extract — /dev-story 2026-06-22（epic-004 S1）
- Story: production/epics/epic-004-city-logistics/story-001-city-daily-settlement.md — 城市日界产耗结算与资源守恒
- 实现方式: inline（lean，沿用本轮节奏）
- 新增 `src/Domain/City/`：CityId、CityEconomyState（不可变聚合+不变量）、CitySettlementConfig（数据驱动+范围校验）、CityDaySettlementStage(+CanonicalOrder)、CitySettlementResult(+LedgerEntry+ConservationHolds)、CityDaySettlementService（纯函数确定性五阶段结算）
- 测试 `tests/unit/ThreeKingdom.Domain.Tests/City/CityDaySettlementTests.cs`（13 测）
- AC: 4/4 覆盖（AC-1 同源库存+移交无双计 / AC-2 稳定顺序 / AC-3 下限夹取不出负 / AC-4 守恒恒等）
- 测试: 194/194 全绿，0 warning（-warnaserror）
- 偏差(ADVISORY): 测试路径 tests/unit/city/*.cs → 归一到 ThreeKingdom.Domain.Tests/City/；消耗下限夹取改用 min(demand, max(0, stock−FLOOR)) 修正 GDD 字面 max() 在 stock<FLOOR 时会凭空补齐的边界 bug（更严守「不凭空补齐」）
- Blockers: None
- Next: /code-review src/Domain/City/*.cs → /story-done

## ✅ epic-004 城市与后勤 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 城市日界产耗结算与资源守恒**（GDD_004/ADR-0004+0003）：`src/Domain/City/`（CityEconomyState、CitySettlementConfig、CityDaySettlementStage 五阶段固定顺序、CitySettlementResult+ConservationHolds、CityDaySettlementService 纯函数）。13 测。commit `96b90eb`。
- **S2 三持有者补给守恒与路线断粮传导**（GDD_012/ADR-0004）：`src/Domain/Supply/`（SupplyChainState GrandTotal 守恒、SupplyConfig、RouteSupplyLink 拓扑切断派生非按钮、SupplyCutoffEvent 单一权威只发事件、SupplySettlementService 逐时段先携行后交付）。13 测。commit `e9c8d37`。
- **S3 外交受控入口（求援/求粮/求时限 §8）**（GDD_012 §8/ADR-0002+0004，Integration）：`src/Domain/Diplomacy/`（DiplomaticRequest/Pledge/Outcome、DiplomacyConfig、DiplomacyService Evaluate+Resolve+ApplyFulfilledSupply）。延迟交付非即到、可背约失败、代价不返还、交付守恒、随机流仅兑现检查点消费。14 测。commit 见下。
- **测试累计 221/221 全绿，`-warnaserror` 0 warning**（181 基线 + 40 新增）。
- 每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。
- **共性偏差（ADVISORY）**：测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/<模块>/`；S1 消耗夹取 `min(demand, max(0, stock−FLOOR))` 修正 GDD 字面式边界 bug。

### 📌 epic-004 收尾交接 — 新会话从此处接续
- **已完成并 push tk/main**：epic-001/002/003/004 全关闭；本会话 epic-004 三提交 `96b90eb e9c8d37` + S3（待本次 commit）。
- **▶ 下一模块**：见 `production/epics/index.md`（epic-005~009 Core/Foundation 余项）。沿用 lean 链路与红线（Domain 纯 C# 禁 UnityEngine、权威路径禁 float、确定性、数据驱动、构造校验无部分写入）。
- **复用底座新增**：`City`（日界结算）、`Supply`（三持有者守恒+断粮事件）、`Diplomacy`（受控外交入口）。
- **挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## ✅ epic-005 情报与军议 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 情报四层分离与只读投影**（GDD_007/ADR-0002，Integration）：`src/Domain/Intel/`（IntelSubjectId、IntelSource、WorldTruthLedger/TruthRecord、Observation、IntelReport、FactionIntel+IntelKnowledgeEntry+IntelProjection、IntelService）。四层分离、投影不含真值、单向流转。8 测。commit `0487100`。
- **S2 报告置信/时效/区间与确定性暴露**（GDD_007/ADR-0004，Logic）：IntelConfig、ConfidenceSignals（多信号非单一百分比）、EstimateInterval、IntelAssessment/Service、ScoutingExposureService（注入随机流）。11 测。commit `e36d0b2`。
- **S3 军师条件化建议**（GDD_008/ADR-0002，Logic）：`src/Domain/Council/`（AdvisorPerspective、AdviceTemplate、CouncilConfig、AdviceStatement、CouncilAdviceSet、WarCouncilService）。读只读投影、过时标记、置信=最弱依据×能力、结构性+反射负向断言（无成功率/最优解/命令）。10 测。commit 见下。
- **测试累计 250/250 全绿，0 warning**（181 基线 + 69 新增：epic-004 40 + epic-005 29）。
- 每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。

### 📌 接续交接 — 新会话从此处接续
- **已完成并 push tk/main**：epic-001/002/003/004/005 全关闭。
- **▶ 下一模块候选**（见 `production/epics/index.md`）：epic-006 战前准备（gdd-009，2 story，依赖 epic-005）/ epic-007 兵法沙盒结算（gdd-010/011，依赖 epic-004 supply 事件 + epic-006）/ epic-008 后果 / epic-009 存档（Foundation，S3 现已解锁）。
- **复用底座新增**：`City`、`Supply`、`Diplomacy`、`Intel`（四层+评估+暴露）、`Council`（条件化建议）。
- 红线与 lean 链路同前。验证：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`。

## ✅ epic-006 战前准备 — 全部 2 story 完成（2026-06-22 连续会话）
- **S2 硬冲突校验与 DAG 依赖图**（GDD_009/ADR-0004，Logic）：`src/Domain/Preparation/`（OrderId/ResourceKey、TimeWindow、PreparedOrder、PreparationContext、PreparationConfig、PlanValidationResult、PlanValidator）。五类硬冲突聚合 + Kahn 拓扑检环 + 错误/风险区分。12 测。commit 见下（先于 S1，依赖方向）。
- **S1 PlanDraft 零副作用与原子提交**（GDD_009/ADR-0002，Integration）：ResourcePool（StateHasher 哈希）、PlanDraft、CommittedPlan、SubmitPlanResult、PlanCommitService。草稿零副作用（哈希不变）、提交全有或全无、失败稳定错误码零部分写入。6 测。commit 见下。
- **测试累计 268/268 全绿，0 warning**（181 基线 + 87 新增：E4 40 + E5 29 + E6 18）。

### 📌 接续交接
- **已完成并 push tk/main**：epic-001/002/003/004/005/006 全关闭。
- **▶ 下一模块候选**：epic-007 兵法沙盒结算（gdd-010/011，3 story，依赖 epic-004 supply 事件 + epic-006 CommittedPlan）/ epic-008 后果 / epic-009 存档（Foundation，S3 已解锁）。
- **复用底座新增**：`Preparation`（计划草稿/校验/原子提交）。

## ✅ epic-007 兵法沙盒结算 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 确定性战役解析管线与状态哈希**（GDD_010/ADR-0004，Logic）：`src/Domain/Battle/`（BattleUnitState、八步管线 BattleResolver、CombatMath 有效战斗力/突然性/伤亡、DetectionState、状态哈希、原子回滚）。7 测。commit `cb...`（见 git log）。
- **S2 条件链涌现与复盘标签**（GDD_010/ADR-0004，Logic）：TacticTag/TacticCondition、TacticChainConfig（slice 三链+夜袭）、RetrospectiveContext、TacticRecognizer（事后打标签、反射断言无执行按钮）。10 测。
- **S3 士气/疲劳/军纪三维与阈值检查**（GDD_011/ADR-0004，Logic）：`src/Domain/Cohesion/`（CohesionState 三维独立、MoraleEvent 幂等聚合、多输入阈值 Steady/Wavering/Routed、人数加权 Merge、ApplySupplyCutoff 消费 epic-004 SupplyCutoffEvent 单一施加）。9 测。
- **测试累计 294/294 全绿，0 warning**（181 基线 + 113 新增：E4 40 + E5 29 + E6 18 + E7 26）。

### 📌 接续交接
- **已完成并 push tk/main**：epic-001~007 全关闭。
- **▶ 下一模块候选**：epic-008 后果与可玩失败（gdd-010 §后果，2 story，依赖 epic-007）/ epic-009 存档与复现（Foundation，3 story，S3 已由 epic-005 解锁）。
- **复用底座新增**：`Battle`（确定性管线+复盘标签）、`Cohesion`（士气三维）。

## ✅ epic-008 后果与可玩失败 — 全部 2 story 完成（2026-06-22 连续会话）
- **S1 跨系统变更集校验与原子写回**（gdd-010 §后果/ADR-0004+0002，Integration）：`src/Domain/Outcome/`（OutcomeChange/ConsequenceSet 意图、OutcomeWorld 四类权威状态不可变快照+确定性 ComputeHash、OutcomeWritebackService 聚合校验→任一失败整批回滚零部分写入→全通过原子构造新快照）。守恒分组净额=0、各权威系统独占写（城市 With/关系刻度 clamp/名声带符号/人物非负）。8 测。commit `965ecb7`。
- **S2 可玩失败延续分支**（ADR-0002+强制设计锁，Integration）：OutcomeBranch 胜/撤退/失城/败北四分支、FailureContinuationService 各生成不同变更集共用 S1 写回、OutcomeConsequenceConfig 数据驱动损失（按当前值上限夹取不写负）、OutcomeContinuation 构造断言 Options 非空（败局不切死局，极端失城+主将被俘仍可问责/重整）。7 测。commit `d161f43`。

## ✅ epic-009 存档与复现 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 版本化 DTO + 原子写 + 迁移链**（ADR-0005，Integration）：`src/Domain/Persistence/`（SaveSnapshot 版本+指纹+随机流位置+真值/知识分段、RngStreamState Capture/Rebuild、ISaveSerializer+CanonicalSaveSerializer 纯 BCL 确定性文本编解码禁 Unity、ISaveMedium+SaveRepository 临时写→原子改名失败保留旧档、ISaveMigration+SaveMigrator 逐版迁移只操作不可变副本失败保留原档）。7 测。commit `b1307c1`。
- **S2 Round-trip 一致性与随机流位置**（ADR-0005+0004，Integration）：load(save(s)) 经介质往返哈希≡s、(seed,position) 读档续抽与未存档逐项一致不重抽、事件序一致、在途外援/空集合存活。5 测。commit `0256947`。
- **S3 加载校验与不兼容拒绝**（ADR-0005+TR-intel-003，Logic）：SaveLoadService 顺序校验（结构→版本→指纹→迁移），不兼容/指纹不符/损坏拒绝零部分载入、纯函数零副作用、LoadResult.Reason 可行动原因、真值/知识不交叉污染（知识段缺失拒绝非真值回填）。8 测。commit `c470fd1`（关闭 epic-009）。
- **测试累计 329/329 全绿，`-warnaserror` 0 warning**（294 基线 + 35 新增：E8 15 + E9 20）。

### 📌 全部 epic 收尾
- **已完成并 push tk/main**（HEAD=`c470fd1`）：epic-001~009 全 9 epics（28 stories）✅ 关闭。
- **复用底座新增**：`Outcome`（跨系统原子写回 + 可玩失败延续）、`Persistence`（版本化存档 + 原子写 + 迁移链 + round-trip + 加载校验）。
- **▶ 下一阶段候选**：Presentation 层 EPIC_010（Slice UX，规格已 Approved）→ `/create-epics layer:presentation`；或 Unity 表现层垂直切片重验核心幻想（CD-C3/TD CONCERNS 未实证）。
- **挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## ▶ EPIC_010 Presentation 启动（2026-06-22 连续会话）
- **Unity 验证路径确认**：本机 `C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe`（=Unity 6.3 LTS，匹配项目 pin）batchmode 可用，有效 license（LicenseClient-Liang Kai Feng），`return code 0`。→ UI 故事可测表现逻辑走 dotnet/EditMode BLOCKING，视觉走 Editor 截图 ADVISORY。
- **一次性探针残留**：`tools/_unity_probe/`（已 .gitignore + untrack，但物理文件夹仍在磁盘；`rm -rf` 被权限策略拒，待用户手动删）。
- **/create-epics + /create-stories（lean）**：`epic-010-slice-ux`（EPIC.md + 5 stories）。commit `f03bdb5`，push tk/main `82f818f`。
- **✅ S1 投影→展示模型 + 意图→Command 底座**（ADR-0002，Logic）：新 `src/Presentation/`（netstandard2.1，禁 MonoBehaviour/UnityEngine）——`Projections/`（EnemyIntelPanelView 仅探报无真值 / CohesionView 三维分列 / RelationshipView 四维方向性 / CouncilView 并列+过时+定性置信）、`Intents/`（IntentTranslator 意图→命令载荷纯映射）、`Display`（定点→展示 decimal）。设计锁反射固化 P10/P6/P11 + 不依赖 UnityEngine 边界回归。16 测。commit `7b9de4a`，push tk/main。
- **测试累计 345/345 全绿，`-warnaserror` 0 warning**（329 基线 + 16 Presentation）。
- **工程接线**：src/Presentation 入 `ThreeKingdom.slnx` + `.gitignore` csproj 例外 + 测试工程 ProjectReference。

### 📌 EPIC_010 接续交接
- **▶ 剩余 4 story（002 主菜单 / 003 HUD 五态 / 004 暂停 / 005 无障碍）均为 UI 型**：各含「可测 ViewModel 逻辑（dotnet/EditMode BLOCKING）」+「UXML/USS/Scene 视觉外壳（需 Unity Editor，ADVISORY 截图签核）」。
- **待决结构**：UI 视觉外壳需在 repo 根建真实 Unity 6.3 项目（Assets/ProjectSettings/Packages）引用 Domain DLL + asmdef 引用 Presentation。建议先把 002–005 的可测 ViewModel 逻辑在 src/Presentation 落地（dotnet 验证），再一次性建 Unity 项目搭全部 UXML 外壳并 batchmode 跑 EditMode 验证。
- 复用底座：`Presentation`（Projections/Intents/Display + 设计锁反射回归）。

### ▶ EPIC_010 S2–S5 可测 ViewModel 逻辑完成（2026-06-23）
- **S2 主菜单**：`MainMenuViewModel`（5 态）+ `SaveSlotView`；读档错误态消费 LoadResult。6 测。
- **S3 HUD**：`HudContextView`（情境→元素集 + 模态隐去）+ `CausalChainView`（跳过终值不变）+ `NotificationFeed`（500ms 合并/临界绕队/并发≤3）。8 测。
- **S4 暂停**：`PauseMenuViewModel`（5 态 + 保存失败错误态 + 草稿 P9 门控）+ `ContinuationPromptView`（消费 epic-008 OutcomeContinuation，败局仍可继续）。6 测。
- **S5 无障碍**：`AccessibilitySettings`（校验 + 序列化 round-trip）+ `StatusChannels`（去色冗余）。5 测。
- **测试累计 370/370 全绿，0 warning**（345 + 25 新；其中含 S1 16）。commit `d51c413`，push tk/main。
- **各 UI 故事状态**：可测逻辑 BLOCKING 完成；**UXML/USS/Scene 视觉壳待 Unity 项目**（ADVISORY 截图签核）。

### 📌 待决：EPIC_010 视觉壳的 Unity 项目搭建
- 剩余只差 4 屏的 UXML/USS/Scene + asmdef 引用 Presentation。需在 repo 根建真实 Unity 6.3 项目（Assets/ProjectSettings/Packages）。
- 验证：batchmode 可编译 asmdef + 跑 EditMode（BLOCKING 编译级）；视觉截图需 graphics 模式（较重）。
- 结构性改动（reshape repo 根 + CI），待用户拍板再搭。
