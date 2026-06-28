# 全游戏细节 Review — 规划一致性 / 完整性 / 断联 / Bug

**日期**：2026-06-28
**范围**：16 GDD · 8 ADR · 12 epic · 44 story · 556 测试 · src/Domain·Application·Presentation + Assets
**方法**：6 层 review（设计一致性 → 架构可追溯 → 规划→实现完整性 → 集成断联 → 代码缺陷 → 测试盲点）
**审查模式**：lean

---

## 执行摘要（一句话）

> **项目质量极高，但"内核"与"可玩整体"严重失配。**
> 12 epic / 556 测试全绿、零禁则违反、零确定性泄漏、强 ADR 纪律——**作为一堆经过验证的 Domain 内核，它非常干净**。但**被装配成"能玩的游戏"的部分，仍≈竖切那一局守城**：整个 Meta 层（生涯 011 + 世界模型 012）、敌方 AI（016），以及约 13 个 Domain 系统，是**有单元测试、却没接进任何可玩循环的孤岛**。设计文档描述的"太守人生"三层循环（历史×生涯×战斗）**目前不存在为一个可运行的东西**。

这不是质量缺陷，是**排序/装配问题**：内核投入远超装配投入。下面按严重度给出可执行结论。

---

## 🔴 Blocking（决定"这是不是一个游戏"的根本问题）

### BLK-1 — Meta 层与多数 Domain 系统是孤岛，未装配成可玩循环
- **证据（Layer 4）**：`src/Application/Session/GameSession.cs` 实际只驱动 `SliceScenario`(硬编码)、`WorldClock`、`CityDaySettlementService`、`IntelService`、`WarCouncilService`、`DiplomacyService`。`SessionService` 暴露的玩家操作仅：Advance / Project / City / Intel / Objective / Raid / Ambush / RequestAid / Council / Roster / Diplomacy / Scout——**即竖切"汜水小城守御"那一局**。
- **未接进装配的系统（grep 证实 GameSession 零引用）**：`CareerStateService`/晋升/自立/`GovernorCampaign`、`WorldState`/`HistoryAdvancer`/分叉/抽象结算、`BattleResolver`(GDD_010)、`PlanCommit`(准备 009)、`SupplySettlement`(012)、`CohesionService`(011)、`OutcomeWriteback`(008)、`Relationships`(006)、`Pathfinder/Map`(003)。
- **影响**：epic-011/012 的 105 个测试验证的是**内核正确性**，不是"游戏里能用"。玩家现在能玩到的，没有生涯、没有晋升/自立、没有历史推进、没有完整 GDD_010 战役解算。
- **裁决需求**：这是项目当前**最重要的一件事**——需要一个明确决定：下一步是"**把已建内核装配成可玩的太守循环**"（建集成 epic + Presentation 接线），还是继续往下造更多内核。本 review 强烈建议**先装配再扩**。

### BLK-2 — 敌方 AI（gdd-016 / ADR-0006）整块未建
- **证据（Layer 1/2/3/5）**：gdd-016 有 GDD（Reviewed）、ADR-0006 有架构决策（Accepted），但**无 epic、无 story、`src/` 零代码**（grep `EnemyAi/OpponentModel/AiWorldView/softmax` 全空）。
- **影响**：game-concept 明确把敌方 AI 定位为"**让自由布阵有深度的必需件**"。没有它，兵法沙盒（GDD_010）的"对手"是空的——这是核心战斗体验的结构性缺口，不是可选润色。
- **裁决需求**：要么排期建 epic-013 敌方 AI（"便宜 80%"先做，见 gdd-016 §MVP），要么显式接受 MVP 不含敌方 AI 并在 game-concept/控制清单标注。

---

## 🟠 Concern（应在继续前解决，多为治理漂移 + 016 进实现前的设计修复）

### CON-1 — 文档治理状态全面漂移（gdd-index / epics-index / control-manifest）
- **gdd-index.md**：GDD_001–013 仍标 `Draft`「不得用于 gameplay 实现」，但已全部实现 Complete；且与 014/015 `Locked for Slice` 内部矛盾（上层解锁、下层冻结）。
- **epics-index.md**：全部 epic 标 `Not Ready`，与 src 已实现脱节。
- **control-manifest.md**：`Manifest Version: 1`，当前阶段仍写 **"PRODUCTION — FOUNDATION 实现开始"**，明文只"允许实现 Foundation 层（epic-001/002/009）"——**整个 Core/Feature/Meta 实现期都没更新它**。
- **影响**：这些是范围/门禁的权威文档。它们与现实冲突会持续误导追溯、门禁、新 story 规划。**廉价可修**：批量校准状态字段 + 推进 control-manifest 阶段到实际位置。

### CON-2 — Meta 层未纳入全局结算顺序（确定性硬锁的潜在缺口）
- **证据（Layer 1 设计 pass B1）**：`systems-index.md` 的跨系统结算顺序只到战斗内管线 + 日界基础层；**014/015/016 完全不在任何顺序声明里**，而它们互读状态（014↔015↔004↔016 双向依赖未破环）。
- **现状为何还没爆**：正因为 BLK-1（这些系统没被接进一个循环），stale-read 竞态尚未发生。**一旦装配（修 BLK-1），此缺口立即变成确定性 bug 来源**——会让 QA 的"确定性专项回归门"无法立基。
- **修复**：编辑级——在 systems-index 把"014 生涯结算 / 015 reachable 重算 / 016 决策取值"插入全局结算顺序的确定位置。**必须在装配前定**。

### CON-3 — gdd-016 进实现前的三处设计缺陷
- **悬空引用**：gdd-016 §Data Model 引用 GDD_007 不存在的 `IntelProjection`/`IntelAssessment`（007 实为 `FactionKnowledge`/`IntelClaim.effective_conf`），且该悬空名被固化进 `entities.yaml`。
- **softmax 温度可为负**：`temperature = BaseTemp·(1+Deception+Adaptability)`，性格 [-1,1] 下可 <0 → softmax 反转（偏选最差动作）。须 clamp 或改式。
- **believedEnemy lerp 未 clamp**：插值 t 在极端性格下可 >1 或 <0，与"believedEnemy ≥ 0"冲突。
- **影响**：016 一旦建 epic 即触发；现在改最便宜。

### CON-4 — 自立线"城池倒戈 / 新势力创建"权威落点未连线
- **证据（Layer 1 设计 pass W-e）**：gdd-014 自立结局直接描述"城池倒戈""成立新势力"，但 ADR-0008 要求一切城级控制权变更经 GDD_004 `ControlChanged`。① 倒戈分支未显式声明走 004 事件；② **玩家自立新势力的实体创建权威是 014 `RebellionState` 还是 015 `FactionRecord`，边界未裁**。
- **与代码交叉印证**：我在 11-3 实现里 `RebellionState` 持 `NewFaction`、`CareerState.IntoOwnFaction`，但**没有把它接到 WorldState 去创建 FactionRecord / 经 004 改归属**——正是这条未裁的接缝。装配（BLK-1）时必须先裁。

### CON-5 — SliceScenario 硬编码（ADR-0003 违反，已知技术债）
- `src/Application/Session/SliceScenario.cs` 274 行硬编码开局禀赋/人物/链条；`NewGame()` 永远只造 `SliceScenario.Default()`。违反 ADR-0003（数据驱动：SO 编辑期 → 不可变配置）。装配真实游戏时须改为配置驱动。

---

## 🟡 Advisory（设计平衡 + 文档完整性 + 测试结构）

### 设计平衡（Layer 1 设计 pass）
- **ADV-1 雪球未封顶**：城→兵权上限→战力→夺城→更多城，乘性自增，玩家侧无追赶/橡皮筋机制（软刹车=俸禄/敌对面，加性，难抵乘性）。
- **ADV-2 粮源>>汇**：非战时已稳城池 `base_yield` 无条件日产、`stock` 无上限 → 晚期"断粮杠杆"对自有城池失效。
- **ADV-3 刷战斗护栏仅"待验证"**：反支柱"最优玩法只需刷战斗"的护栏（非战斗源速率竞争力）目前是待验证平衡参数，无机制保证。
- **ADV-4 民心恢复源薄**：gdd-004 民心只给下降项，回升仅靠"安抚"动作，长围/连续征用易死亡螺旋。
- **ADV-5 君主终局 vs 非全知幻想张力**：继承基业→君主玩法须确认仍受 GDD_007 四层信息分离约束。

### 文档完整性（Layer 1 一致性 pass）
- **ADV-6 依赖双向性缺口**：gdd-001/002/006 的"被消费"列表缺 003/005/006/008/010 等对称项（违反 design-docs 规则，低风险）。
- **ADV-7 015 抽象结算 GDD 文本未声明随机源**——注：**代码已对**（我 12-5 用注入 `IDeterministicRandom`），仅 GDD 文本缺一句声明。
- **ADV-8 散落命名**：003 `fresh_enough()` vs 007 `freshness()`；006 `coop_score`→005 `relation_term` 归一化映射未定义。

### 测试结构（Layer 6）
- **ADV-9 无目标循环端到端测试**：556 测试 = 强单元 + 竖切 session 集成（MvpAcceptanceTests 等）。但**没有**一个端到端测试串起目标"太守循环"（守城战→失城经004→世界归属投影→生涯转在野），因为该循环未装配。Meta 层跨系统链只被分片测（11-4 测 career+authority、12-4 测 authority+world，但没有 career→authority→world 整链）。属**结构性盲点**（不能测一个没装配的循环），非测试疏漏。

### 存档（retro 承接）
- **ADV-10 存档三段未物理统一**：生涯段（11-5）、世界段（12-6）各自 codec，未与 epic-009 信封 + 原子写整合为单一 GDD_013 存档。

---

## 各层一句话结论

| 层 | 结论 |
|---|---|
| L1 设计一致性 | CONCERNS — 核心契约自洽；问题=状态漂移 + 016 三处缺陷 + 依赖双向 |
| L2 架构可追溯 | CONCERNS — 42 TR 全覆盖、8 ADR 全 Accepted；control-manifest 严重过期；ADR-0006 孤儿 |
| L3 规划→实现 | 44/44 story Complete；唯一缺口=gdd-016 无 epic |
| L4 集成断联 | **核心问题**——Meta 层 + ~13 Domain 系统孤岛，装配≈竖切 |
| L5 代码缺陷 | 极干净——零禁则违反/零确定性泄漏/零吞异常；唯一项=SliceScenario 硬编码 |
| L6 测试盲点 | 556 绿，强单元+竖切集成；缺目标循环端到端测试（因未装配） |

---

## 建议的处置顺序（先廉价治理，再裁决方向，再装配）

1. **立即（廉价、零风险）**：批量校准状态漂移 — gdd-index（001–013→实际态）、epics-index、**control-manifest 推进阶段**（CON-1）。
2. **立即（编辑级，但挡装配）**：systems-index 补 Meta 层全局结算顺序（CON-2）；裁定自立新势力创建权威边界 014↔015（CON-4）。
3. **方向裁决（BLK-1）**：决定"先装配可玩太守循环"还是"继续造内核"。**建议先装配**——建一个集成 epic（GameSession 接 Career/World/Battle + Presentation 接线 + 配置驱动替换 SliceScenario），并补一个目标循环端到端测试（ADV-9）。
4. **方向裁决（BLK-2）**：敌方 AI——排期建 epic（gdd-016 §MVP"便宜 80%"先做，且先修 CON-3 三处缺陷），或显式接受 MVP 不含并标注。
5. **后续平衡（Advisory）**：雪球/粮源汇/刷战斗护栏须进体验验证（ADV-1~4）；存档三段整合（ADV-10）；文档完整性扫尾（ADV-6~8）。

---

## 结论

代码与确定性纪律**优秀**，单系统设计**自洽**。真正的风险只有一个、但很大：**已经造了一座非常结实的发动机和变速箱，但还没装上车**。建议下一阶段把重心从"造更多内核"切到"装配出第一个可玩的太守人生循环"，并同时决定敌方 AI 的去留——这两个决定，比任何单点 bug 都更决定这个项目能不能成为一个游戏。
