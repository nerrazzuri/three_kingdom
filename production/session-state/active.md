# 会话状态 — 大方向锁定（游戏整体定位）

> **最后更新**：2026-06-27
> **语言**：全程中文（见用户偏好 memory）
> **审查模式**：lean

> **▶ 批次完成（2026-06-27）**：用户授权「把 Must 完成后继续所有 Should」——**全部 8 story（5 Must + 3 Should）已实现+审查+收尾+push**。每 story：实现+测试→inline lean review→story-done→commit+push。**全套 dotnet 536/536 绿，-warnaserror 0。**
> **提交链（push tk/main）**：11-1 `cf00edb` · 12-1 `6414b32` · 11-2 `51039f6` · 11-3 `dfed215` · 12-2 `1003b4e` · 11-4 `ff3b284` · 12-3 `43dd9af` · **11-5 生涯存档**（CareerSaveCodec 版本化 DTO+版本/指纹校验+LordMissionLog；536/536）。
> **★ Sprint 02 Must+Should 全清（8/8）。epic-011 = 5/5 Complete。** 仅剩 Nice(3)：12-4 归属投影（已被 11-4 的 ICityControlAuthority 解锁）/12-5 抽象结算/12-6 世界存档——**未做（用户未要求）**。
> **✅ Sprint 02 全部收尾完成（2026-06-28）**：11/11 story（Must 5 + Should 3 + Nice 3）实现+审查+收尾+push（556/556 绿）。`/smoke-check` PASS（smoke-2026-06-28.md）· `/team-qa` **APPROVED**（qa-signoff-sprint-02-2026-06-28.md，0 缺陷）· `/retrospective`（retro-sprint-02-2026-06-28.md）· **epic-011 5/5 + epic-012 6/6 判 Complete**（EPIC.md + epics/index.md 已标 ✅）。
> **HEAD 待推**：close-out 文档批次（smoke/qa-signoff/retro/epic 状态/index/active）。提交链见下方提交序段。
> **下一步候选**：① 敌方 AI（gdd-016/ADR-0006，仍 Reviewed）建 epic 进实现；② Presentation 把 Meta 层接 UI；③ 存档三段统一信封整合（retro 行动项 #2）；④ 统一测试路径约定（retro 行动项 #1）。
<!-- QA RUN: 2026-06-28 | Sprint: sprint-02 | Verdict: PASS（APPROVED）| Report: production/qa/qa-signoff-sprint-02-2026-06-28.md -->

## Session Extract — 全游戏 review 2026-06-28（6 层）
- 报告：`docs/reviews/full-game-review-2026-06-28.md`（未 commit）
- 核心结论：**内核质量极高、装配极薄**。代码零禁则违反/零确定性泄漏/556 绿/8 ADR 全 Accepted；但 Meta 层(011/012)+~13 Domain 系统是**孤岛**，可玩装配≈竖切守城那一局。
- 🔴 Blocking：① BLK-1 Meta 层+多数系统未装配成可玩太守循环（GameSession 只驱动竖切系统）；② BLK-2 敌方 AI（gdd-016/ADR-0006）整块未建（无 epic/无代码）。
- 🟠 Concern：CON-1 治理状态全面漂移（gdd-index/epics-index/**control-manifest 卡在 Foundation**）；CON-2 Meta 层未纳入全局结算顺序（装配后即确定性 bug 源）；CON-3 gdd-016 三处设计缺陷（悬空引用 IntelProjection/负 softmax/lerp 未 clamp）；CON-4 自立新势力创建权威 014↔015 未裁；CON-5 SliceScenario 硬编码(ADR-0003)。
- 🟡 Advisory：雪球/粮源汇/刷战斗护栏未验证/民心恢复薄；依赖双向缺口；015 抽象结算随机源代码已对但 GDD 未声明；无目标循环端到端测试；存档三段未统一。
- **建议处置序**：①廉价校准治理状态 →②编辑级补结算顺序+裁自立权威 →③**裁决方向：先装配可玩太守循环（建议）vs 继续造内核** →④裁决敌方 AI 去留。
- **下一步等用户裁决**：是否（a）先做廉价治理校准；（b）开"装配集成 epic"；（c）建敌方 AI epic。

---

## ▶▶▶ 新会话从这里读起（2026-06-27）— Sprint 02 开工：11-1 已实现，待 review/done

> 用户授权直接开工 A（不逐步确认）。已用 `/dev-story` 实现 **epic-011 story-001 CareerState 骨架**。**全套 dotnet 465/465 绿（451+14 新），-warnaserror 0**。**尚未 commit**（按惯例待用户指示）。

### 本轮产出（11-1，Status: Ready → In Progress）
- **Domain**（`src/Domain/Career/`，10 文件）：`Rank`(0–7 官阶梯队) · `OfficeRole`(城守/副将/内政主事/军师) · `CareerErrorCode`(稳定码) · `CareerState`(merit/renown int≥0 + lord_standing Q16.16∈[0,1] + rank + FactionId? + 在野标志；不可变+不变量校验+AppendTo 哈希) · `RetinueMember` · `RetinueState`(僚属+好感+官职任免，规范序哈希) · `CareerSnapshot`(Career+Retinue 组合哈希) · `CareerCommand`(4 命令：GainMerit/AdjustLordStanding/PromoteRank/AssignOffice) · `CareerCommandResult`(成功/失败+稳定码，失败=原态不变) · `CareerStateService`(Domain 解析，纯函数确定性)。
- **Application**（`src/Application/Career/CareerCommandService.cs`）：唯一写路径，前置校验(空命令→NullCommand)后委派 Domain。
- **Test**（`tests/unit/ThreeKingdom.Domain.Tests/Career/CareerStateTests.cs`，14 测）：覆盖 AC-1/5 字段类型+越界拒、AC-3 非法操作稳定码+无部分写入(负功绩/越级/空命令/非成员任免/在野晋升)、AC-4 同前态同命令流哈希一致 + 顺序敏感(同职位先后任免→哈希异)。
- **路径偏差**：测试落统一工程 `ThreeKingdom.Domain.Tests/Career/`（沿 epic-001 约定），非 story 原拟 `tests/unit/career/`（不在编译工程内）。
- **结构性边界**：晋升只校验逐级结构（不做 merit/renown/standing 门槛——属 story-002）；lord_standing 钳制非报错（N10 有源有汇）。
- **已改追踪**：story header(Status/Last Updated/Estimate/Test Evidence✓) + sprint-status.yaml(11-1 in_progress, updated 2026-06-27)。

### ▶ 下一步
1. ✅ `/code-review` APPROVED + `/story-done` **COMPLETE WITH NOTES** — 11-1 已关闭（Status: Complete；sprint-status done；EPIC.md ✅）。
2. **接着做（sprint-02 Must 链）**：12-1 WorldState 骨架（epic-012 DAG 根）或 11-2 忠臣晋升。建议先 12-1（另一 DAG 根，并行解锁 12 链）。
3. **commit 时机由用户定**（本轮全部未 commit）。建议命令：`git add src/Domain/Career src/Application/Career tests/unit/ThreeKingdom.Domain.Tests/Career production/epics/epic-011-campaign-career production/sprint-status.yaml production/session-state/active.md && git commit -m "feat: CareerState 权威状态与确定性结算骨架 (TR-career-001/005)"`

## Session Extract — /story-done 2026-06-27
- Verdict: COMPLETE WITH NOTES
- Story: epic-011 story-001 — CareerState 权威状态与确定性结算骨架
- Tech debt logged: None
- Next recommended: epic-012 story-001 WorldState 骨架（DAG 根）或 epic-011 story-002 忠臣晋升

## Session Extract — 12-1 WorldState 骨架 实现+审查+收尾 2026-06-27
- **HEAD 进度**：11-1 已 commit+push `cf00edb`（tk/main）。12-1 实现完成，待 commit+push。
- **产出**（epic-012 story-001，Status: Ready → Complete）：
  - Domain（`src/Domain/World/`，6 文件）：`SurvivalStatus`/`RelationToPlayer` 枚举 · `CityOwnership`(城归属**只读投影**，无写 API，TR-world-003/ADR-0008) · `FactionRecord`(势力 id/君主/存续/领有城池/对玩家关系，规范序+不变量) · `WorldState`(时间引用+势力+城投影+已触发/已分叉事件集合；不可变+规范序哈希+只读访问器) · `WorldProgressionService`(纯函数确定性时间推进，story-002 事件触发的单一注入点)。
  - Test（`tests/unit/ThreeKingdom.Domain.Tests/World/WorldStateTests.cs`，12 测）：AC-1/2 字段+空集合/单势力合法+存续须有君主、AC-3/4 同序列哈希一致+不同序列哈希异+输入序无关+0段恒等、AC-5 归属只读（反射断言无 setter + WorldState 无 public 变更方法）。
  - 验证：全套 **dotnet 477/477 绿**（465+12），-warnaserror 0。
- **Deviations（ADVISORY）**：① 测试路径统一工程（非 story 拟 tests/unit/world/）；② 线性时间可交换，严格非交换序敏感随 story-002 到来（已以不同序列+输入序无关证确定性）；③ Cities 城侧投影与 FactionRecord.OwnedCities 势力侧两视图未交叉校验，权威同步随 story-004 接入。
- **追踪已更**：story header + Completion Notes + sprint-status.yaml(12-1 done) + EPIC.md(✅) + active.md。
- **▶ 下一步**：commit+push 12-1 → 续 sprint-02 Must 链最后一项 **11-2 忠臣晋升**（merit/renown/standing_req 配置化门槛，接 11-1）。之后 Should 层 11-4/12-3/11-5。

---

## ▶▶▶ 新会话从这里读起（2026-06-25）— Meta 层（014/015）已走完文档→实现管线，待开工

> 上个会话从「重启 A」一路推进到 Sprint 02 + QA plan，全部已 commit+push。**HEAD = tk/main = `fddf4c3`，工作树干净。**

### 已完成的整条管线（按提交序，全部 push tk/main）
1. `fcb8e90` 重启 A — 跨系统审查（CONCERNS，5 Warning 全落，W1 城池归属裁定 004 唯一权威）+ 014/015/016 转 Reviewed + registry 首填
2. `761a4fe` 补 014/015 架构 — **ADR-0007**（条件历史世界模型）+ **ADR-0008**（城池控制权契约）均 Accepted + 审查报告 + TR v3
3. `2fe240e` 架构审查验证重跑 — 缺口全闭，裁定 **PASS**
4. `4bc397c` 014/015 → **Locked for Slice** + **epic-011**（生涯，5 story）+ **epic-012**（世界模型，6 story）= 11 story
5. `3168fc0` **Sprint 02** 排期（sprint-02.md + sprint-status.yaml）
6. `fddf4c3` **QA plan**（qa-plan-sprint-02-2026-06-24.md）

### 当前可立即做的下一步（二选一）
- **A. 直接开工**：`/dev-story production/epics/epic-011-campaign-career/story-001-career-state-skeleton.md`
  实现 11-1 CareerState 骨架（DAG 根，解锁 11-2/3/4/5）。已 /story-readiness=**READY**。
- **B. 先清理**：把估算（M/4h）回写进全部 11 个 story header（现为占位符 `[待 sprint 规划填]`，估算实际已在 sprint-02 + sprint-status.yaml）。10 分钟杂活，然后再开工。

### Sprint 02 关键事实（实现时须知）
- **Must(5)**：11-1 CareerState骨架 · 12-1 WorldState骨架 · 11-2 忠臣晋升 · 11-3 自立三分支 · 12-2 历史事件触发门。**Should(3)**：11-4 太守开局守城 · 12-3 分叉传播 · 11-5 生涯存档。**Nice(3)**：12-4 归属投影 · 12-5 抽象结算 · 12-6 世界存档。
- 全 0 Blocked，governing ADR 全 Accepted（8 份）。纯 C# Domain Logic/Integration，NUnit + dotnet test 旁路 Unity 许可。
- **两处跨 epic 接缝**（非阻断）：① 11-4 用最小 CitySeed 配置占位（CitySeed 权威在 epic-012）；② 12-4/11-4 需确认 epic-004（已 Complete）的 `CityControlChanged`/`ICityControlAuthority` 接口（ADR-0008）已落地——缺则先补最小实现。
- **确定性专项回归门**（QA plan）：状态哈希一致 / 存档 round-trip 矩阵 / 自立好感快照隔离 / 历史够不着短路 / 无旁路随机（System.Random/UnityEngine.Random/float 权威路径）。
- 既有：dotnet 451/451 绿（竖切+epic-001~010 全 Complete）；复用 epic-001 Numerics + epic-009 存档信封。

### 状态机要点
- gdd-index：014/015 = **Locked for Slice**；016 = **Reviewed**（未建 epic）。
- ADR：0001~0008 全 **Accepted**（technical-preferences 日志同步）。
- registry：entities.yaml v2（14 跨系统事实）；tr-registry v3（含 TR-career-001~005 + TR-world-001~006）。
- 验证命令：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/...csproj -warnaserror`。

---

## ▶▶ 大方向已锁定并写入文档（2026-06-24）— 历史背景

> 用户休息后重想了整个游戏完整品，经长讨论澄清了一直以来"竖切 vs 大规划"的混淆根源。**结论已落文档，防后续跑偏。**

### 锁定的游戏定位（权威见 `design/gdd/game-concept.md`）
- **固定开局身份：城池太守**。`大势随历史（演义时间线）+ 个人靠抉择`。
- **游戏三层结构**：① 历史世界模型（GDD_015）× ② 太守生涯（GDD_014）× ③ 兵法战斗（GDD_010）。前两层是此前缺失、最易跑偏处，现已补成 GDD。
- **双线**：忠臣晋升（功绩/名望/君主好感→7阶官职→继承基业）/ 自立反叛（实力+好感→自立，结局由关系网三分支：拥立/部分跟随/众叛亲离）。
- **战斗 = 条件涌现兵法沙盒，非自由摆阵 SLG**。澄清："自由布阵 = 布局条件"（位置/隐蔽/时机），**非**阵型坐标/兵种克制/实时微操。已把术语锁写进 GDD_010。
- **玩法靠"加杠杆"而非"加按钮"**：六杠杆（天气/地形/时间/补给/人心/工程）相乘涌现招式库（火攻招牌 + 水攻/隘口/分进合击/诱敌/长围/强攻 + 人心杠杆离间/策反/攻心=护城河独门）。数据驱动加条件链。
- **条件历史解矛盾**（GDD_015）：历史事件=`{时间窗+前置条件+正常结局+分叉结局}`；**玩家够不着则前置恒成立**（早期历史在轨上跑=便宜），强到改变前置才分叉（如提前灭孙权→赤壁不成立），晚期才付分叉成本，且只玩家势力圈脱稿。
- **敌方 AI = 让自由布阵有深度的必需件**（GDD_016 + ADR-0006）：战术层敌将，效用评分+种子softmax（不可预测却可复现）+反全知锁(AiWorldView不接受真值)+渐进记忆+LLM仅装饰不决胜负。"便宜80%"先做。
- **与三国志关系**：参考存在性、不抄全套深度。砍scope尺子：每加功能须答"喂给战斗/生涯抉择什么"，喂不到即砍。

### 本轮写入/改动的文档（防跑偏全套，已落盘）
- 改：`design/gdd/game-concept.md`（太守开局+三层结构+双线+三国志边界+砍scope尺子）。
- 改：`design/gdd/gdd-010-battle-tactics-sandbox.md`（战区+六杠杆+招式库+术语锁；MVP/Future/AI 章节更新）。
- 新：`design/gdd/gdd-014-campaign-and-career.md`（战役与生涯，Draft）。
- 新：`design/gdd/gdd-015-historical-world-model.md`（条件历史世界模型，Draft）。
- 新：`design/gdd/gdd-016-enemy-ai.md`（敌方 AI，Draft）。
- 新：`docs/architecture/adr-0006-deterministic-enemy-ai.md`（**Proposed** — 待 /architecture-decision 或 review 转 Accepted 后 story 方可引用）。
- 改：`design/gdd/gdd-index.md`（登记 Meta GDD 014/015/016）、`.claude/docs/technical-preferences.md`（ADR 日志加 0006）。
- **尚未 commit**：以上均在工作树，待用户指示再提交。

### ▶ 下一步（用户原话："文档更新好了，我们再来谈要做哪些事情"）
- 文档已更新完。**待与用户讨论：先做哪些事**。候选：① 把 014/015/016 三篇新 GDD 逐节细化/过 design-review；② ADR-0006 转 Accepted；③ 直接进 GDD_016 §MVP"便宜80%"敌方AI最小切片；④ 先做 GDD_010 §MVP 的"可玩战区"（火攻+三招+人心杠杆）。
- 注：014/015/016 为 Draft，**未过跨系统审查**，按 gdd-index 控制规则不得直接进实现。

### ✅ A（`/review-all-gdds`）已完成（2026-06-24 重启后跑完）
> **重启 A 已走完 Phase 1-6**。裁定 **CONCERNS**（如预期）。报告已落盘 `design/gdd/gdd-cross-review-2026-06-24.md`。
> **W1 城池归属已由用户裁定**：GDD_004 城级控制权唯一权威 + 唯一变更事件；GDD_015 战略尺度只反映、订阅 004 事件、不独立写；GDD_014 只读。
> **5 项 Warning 已全部落盘编辑（2026-06-24）**：W1 改 004（声明城级控制权唯一权威+变更事件）/015（city.owner 改只读投影、订阅 004 事件）/014（System Outputs 归属改"经 004 触发"）· W2 **反向依赖已彻底补齐**：004←014/015 · 010←016/014 · 015→004 · 007←015/016 · 006←014 · 005←014/016 · 001←015 · 003←015 · 011←016 · 012←016 · 013 显式含 014/015/016 存档边界· W3 GDD_010 命名 TacticRecognizer（§招式库 + Data Model）· W4 010 §7 + 016 StrategicAction 各加追击边界一句 · W5 014 Balancing 加反支柱护栏（非战斗源速率竞争力）+ N10 sink 说明。
> **①②③ 已全部完成（2026-06-24）**：① registry 填充（design/registry/entities.yaml v2：7 entities + 3 formulas + 4 constants——city_control/TacticRecognizer/AiWorldView/StrategicAction/OpponentModel/HistoricalEvent/FactionKnowledge + combat_power/pursue_decision/can_promote + merit/renown/lord_standing/supply_state）。② ADR-0006 → **Accepted**（§Decision 1 补 N-#5 随机源契合 ADR-0004；technical-preferences ADR 日志同步 Accepted）。③ GDD_014/015/016 → **Reviewed**（gdd-index + 各文件状态行）。
> **下一步**：④ commit 本批（进行中）；之后可 push tk/main、或进 014/015/016 §MVP 实现切片。

<!-- CONSISTENCY-CHECK: 2026-06-24 | registry 由空首次填充（v2，14 条跨系统命名事实）| Conflicts: 0（填充模式，非检查）| 来源 gdd-cross-review-2026-06-24 -->
<!-- consistency-check 说明：registry 原为空，按技能 Phase 1 应停（检查工具需已填 registry）；改走 Phase 6 新增路径直接填充真正跨系统事实 -->
> 以下为重启时的原始中断记录与已确认发现，保留备查。

### ⏸ A（`/review-all-gdds`）中断记录 + 重启指引（2026-06-24）
> 用户选择做 A（验证新文档），跑到一半被中断，**未出最终报告/未定 verdict**。新会话**重启 A**：重跑 `/review-all-gdds`，参数聚焦 014/015/016 + 改动的 game-concept/gdd-010，对照 GDD_001-013。下列**已确认发现**可直接带入加速，不必重新发现。

**审查已完成的步骤**：Phase 1（载入范围文档）+ Phase 1b（读 registry）。**未做**：Phase 2 完整一致性、Phase 3 设计理论、Phase 4 场景走查、Phase 5 报告、Phase 6 落盘、Phase 7 handoff。

**已确认发现（带入新会话）**：
1. **Registry 为空**：`design/registry/entities.yaml` 存在但 entities/items/formulas/constants 全为 `[]`。一致性只能靠全文读。**建议审查后跑 `/consistency-check` 填充**（尤其新引入的 功绩/名望/城池归属/StrategicAction 等跨系统名）。
2. **依赖单向（Warning）**：GDD_014/015/016 依赖 004/005/006/007/010/011/012/013，但**现有 001-013 无一把 014/015/016 列为 dependents**（新文档必然如此）。修：在相关旧 GDD 的 Dependencies 加反向引用；并把新跨系统名登记进 registry。
3. **城池归属权威边界（Warning，需裁定）**：GDD_004 拥有「控制权」（"失城改变控制权"、"控制权变更事件"——见 gdd-004 L27/138/147）；GDD_015 写 `city.owner ← outcome.owner_change`。**两者都在动城池归属，须明确权威**。建议：GDD_004 拥有城级控制权 + 变更事件；GDD_015 世界模型在战略尺度**反映**归属、**只经 GDD_004 的控制权变更事件**写入，不独立改。**（裁定权在用户，审查只标选项）**
4. **反支柱「最优玩法只需刷战斗」（Design Warning）**：game-pillars 反支柱之一。GDD_014 的 功绩/名望 须保住**非战斗来源**（治理/招揽/外交/平叛）有意义，否则生涯退化为刷战斗。现状 014 已含非战斗来源——**作为护栏保留并在审查中确认权重**。
5. **AI 随机与 ADR-0004 的契合（INFO）**：ADR-0006 用 `seed=Hash(worldTick,factionId,planId)`；须确认它**消费 ADR-0004 的注入式确定性流、在声明的 AI 决策检查点取值**，而非旁路随机源。建议在 ADR-0006 补一句明确。
6. **设计锁 PASS**：014/015/016 **未**引入任何「无条件计策按钮」，GDD_010 设计锁完好；016 维持条件涌现（效用评分→基础命令，非按钮）。
7. **自洽 PASS**：015「玩家触及边界」与 016 战略层（够得着才跑 AI、够不着抽象推进）、014 自立线三者一致。

**重启 A 的下一步动作**：重跑 `/review-all-gdds` → 走完 Phase 2-4 → 出报告（verdict 预期为 **CONCERNS**：无 Blocking，上述 2/3/4 为 Warning）→ 经批准写 `design/gdd/gdd-cross-review-2026-06-24.md` → 按 handoff 决定是否把 ADR-0006 转 Accepted / 进实现。

<!-- QA-PLAN: 2026-06-24 | System: sprint-02 | Plan written: production/qa/qa-plan-sprint-02-2026-06-24.md -->

## Session Extract — /sprint-plan new 2026-06-24（Sprint 02）
- 新建 `production/sprints/sprint-02.md` + `production/sprint-status.yaml`（首个 yaml；2026-06-24~07-07）
- Goal：epic-011 生涯 + epic-012 世界模型的 Domain 内核与标志性机制
- Must(5)：11-1 CareerState骨架 · 12-1 WorldState骨架 · 11-2 晋升 · 11-3 自立 · 12-2 事件触发门（~20h）
- Should(3)：11-4 太守开局守城 · 12-3 分叉传播 · 11-5 生涯存档
- Nice(3)：12-4 归属投影(需epic-004接口) · 12-5 抽象结算 · 12-6 世界存档
- lean → PR-SPRINT 跳过。**QA plan 缺**（qa-plan-sprint-02.md 未建）——Production→Polish 门需要，建议 /qa-plan sprint
- sprint-01（Foundation）随竖切已 Complete，无 carryover
- **下一步**：/qa-plan sprint（实现前）→ /story-readiness 11-1 → /dev-story。本批 sprint 文件未 commit。

## Session Extract — /create-epics + /create-stories 2026-06-24（014/015 Meta 层进实现管线）
- 014/015 → **Locked for Slice**（gdd-index + 各文件状态行；016 仍 Reviewed）
- 新建 2 epic：**epic-011-campaign-career**（生涯，5 story：3 Logic+2 Integration）+ **epic-012-historical-world-model**（世界模型，6 story：4 Logic+2 Integration）
- 全部 11 story 嵌 TR-ID + governing ADR（全 Accepted，0 Blocked）+ 控制清单规则 + inline QA 用例（lean，未派 qa-lead）
- epic-011 story：001 CareerState 骨架 · 002 忠臣晋升 · 003 自立三分支 · 004 太守开局+守城后果(跨epic软依赖 CitySeed/epic-012) · 005 生涯存档
- epic-012 story：001 WorldState 骨架 · 002 事件四元组+reachability门+配置校验 · 003 分叉传播 · 004 归属订阅004 · 005 抽象结算器 · 006 世界存档
- 跨 epic 注意：epic-011 story-004 用最小 CitySeed 配置占位（待 epic-012）；epic-012 story-004 需确认 epic-004 的 CityControlChanged 接口落地
- EPIC.md + epics index 已更新（11/12 epic 有 stories；016 epic 未建）
- **下一步**：/sprint-plan 排期 或 /story-readiness→/dev-story 从 epic-011 story-001 起实现。本批未 commit。

## Session Extract — /architecture-review 2026-06-24（验证重跑 coverage）
- Verdict: **CONCERNS → PASS**（014/015 Meta 层范围）
- 4 缺口 TR（world-002/004、career-004、world-003）全部闭合（grep 确认 ADR-0007/0008 GDD Requirements Addressed 实际命名）
- Cross-ADR 无依赖环（0007/0008 仅 Related/Ordering 互引，未互列 Depends On）；依赖链全 Accepted；W1 状态所有权冲突由 ADR-0008 消解
- 014/015/016 全覆盖；遗留 3 历史 cosmetic partial（map-001/council-002/supply-001，非阻断）
- 报告更新：docs/architecture/architecture-review-2026-06-24.md（加「验证重跑」节 + 顶部裁定上调）
- 014/015 仍为 Reviewed（未推进 Locked for Slice，留用户定）；本次仅报告 + active.md 改动，未 commit

## Session Extract — /architecture-review 2026-06-24（聚焦 014/015 Meta 层）
- Verdict: CONCERNS（3 缺口全 Meta 层，不阻断竖切 MVP）
- Requirements: 11 total — 7 covered, 0 partial, 3 gaps
- New TR-IDs registered: TR-career-001~005 + TR-world-001~006（tr-registry version→3）
- 缺口 → 已补 ADR：缺口1 条件历史触发模型 → **ADR-0007 Accepted**；缺口2 城池控制权契约（W1 裁定固化）→ **ADR-0008 Accepted**
- GDD revision flags: None；引擎专家咨询跳过（014/015 纯 Domain 无引擎面）
- Report: docs/architecture/architecture-review-2026-06-24.md
- ADR 日志：technical-preferences 已加 0007/0008（现共 8 份全 Accepted）
- **下一步**：014/015 可 Reviewed→Locked for Slice（缺口已补）；可重跑 /architecture-review 验证全覆盖；或进 016 §MVP 敌方 AI 切片。本批未 commit。

## Session Extract — /review-all-gdds 2026-06-24（重启 A 完成）
- Verdict: CONCERNS
- GDDs reviewed: 5 焦点（014/015/016 + game-concept + 010）对照 001-013 + pillars + systems-index
- Flagged for revision: gdd-004, gdd-010, gdd-014, gdd-015, gdd-016（均 Warning，无 Blocking）
- Blocking issues: None
- Key Warnings: W1 城池归属（已裁定 004 唯一权威/015 订阅/014 只读）· W2 反向依赖缺失 · W3 TacticRecognizer 未命名 · W4 追击决策边界 · W5 非战斗功绩源速率
- Recommended next: 落 W1-W5 轻量 Edit → /consistency-check 填 registry → ADR-0006 转 Accepted（确认 N-#5）
- Report: design/gdd/gdd-cross-review-2026-06-24.md

---

## ▶ 休息后接续（2026-06-24）— 新会话从这里读起

> 用户表示"有点混淆，先休息整理思路再继续"。本段是恢复入口，读完即可接续。

### 当前状态（事实）
- **HEAD = tk/main = `a1b10a3`**，工作树干净。**dotnet 451/451 全绿**（-warnaserror 0）。
- 本轮提交链（新→旧）：`a1b10a3` GDD_008边界 · `b22d0d6` 假退伏击第三胜路 · `ff4f89a` 侦察/袭扰改延迟 · `4ffaaa9` HUD双列布局修复 · `cf58dd5` MVP验收测试 · `bc4f8fb` 断粮第二胜路 · `4aadbc8` B7花名册 · `c02ffd6` B1军议 · `4d4cb66` B6外交 · `542982a` A一局闭环 · `1ec3759` 回顾+路线图。

### 一个关键澄清（用户问"竖切内容后续正式场景会用吗"）——已厘清
- **现在做的"竖切" ≠ 早先 throwaway 原型**（`prototypes/...` 那个铁律永不 import）。**现在这个是 production 代码**（src/Domain·Application·Presentation + 真 Unity 工程，复用 9 epic Domain），是"正式游戏的垂直切片"=第一战/教学战雏形。
- **会保留进正式版**：四层架构 + 全部代码；场景**内容**（汜水关/旋门关/敖仓地名、四人物、三条链）——故用原著地名、守原创红线。
- **会重构（内容迁移、承载方式换）**：`SliceScenario` 硬编码工厂 → 按 ADR-0003 改数据驱动配置（ScriptableObject→不可变配置）；占位数值 → 待平衡；占位 UXML/USS → 正式美术。

### 本轮与用户对齐的设计决定（重要，未来都要守）
1. **策略默认全开，只由硬条件（天气/地形/兵种/资源）禁用**；禁用须给 in-world 理由。我之前给假退伏击加的"一局一次"是**人为限制，应改为 diegetic**（如诈败被识破→敌将警觉再诱难）——**待办，尚未改**。
2. **袭击/伏击不能只点按钮**（用户最看重）：要"派谁 + 投多少 + 借势(地形/敌将性格) + 时机"的真实决策，投入不足则徒劳。当前是单按钮（demo 欠做，非设定）。复用 epic-006 准备 + epic-007 战役，但**竖切只做满足 mvp-scope §条件链验收 的最小版**，不做全套准备子系统（那是全局 B2/B3）——**待办，尚未做**。
3. **军师边界（已写入 GDD_008，commit a1b10a3）**：建议以「缘由（含原著地名 + 敌将性格等前提事实 + 依据情报 + 缺失情报）→ 条件 → 风险 → 免责」呈现；**可给**地点/敌将性格/条件/风险/定性置信；**不可给**派谁·多少·何时的完整组合、胜率数字；"地点已知≠此战已解"（可用性仍须侦察证实）；军师能力体现为完整度/准确度，不显胜率。
   - 与用户达成的折中："越线版+守线版综合"——军师可点名"附近某隘口适于伏击"（缘由），但不替玩家组完整计划。

### 地名落点（我已查三国演义地理 + 推荐，用户已认可这组）
- **假退伏击 → 旋门关**（成皋与汜水关之间窄关，敌东来必经；备选 广武涧）
- **断粮疲敌 → 敖仓**（黄河南岸著名粮仓 + 其往前线粮道）
- **守城待变 → 汜水关**（即虎牢关一带，切片现名正确；后备纵深 成皋）
- 火攻/水攻/暗度陈仓：地名已备（汜水谷·荥泽/鸿沟·小平津渡/轘辕关），但属 mvp-scope 之外，**切片不做**，记全局扩展。
- 注：地名为《三国演义》/汉末实有地理（非杜撰），"何处宜某计"含我的推断，用户可改。

### ▶ 下一步（休息后三选一，待用户定）
1. **把"汜水关守御战"场景落成正式设计文档**（建议 `design/levels/` 或 `design/gdd/`）：地点落点+四人物+三条链+当前数值 → 成权威来源，后续数据驱动配置照此生成（我已建议，用户未答）。
2. **做决定 1（去人为限制）+ 决定 2（袭击/伏击最小版真实决策）+ 把地名接进军师"缘由"**——即把本轮对齐的设计真正落到切片代码。
3. **用户继续 Play 实测**本轮新增（三条路线 + 延迟动作 + 双列布局都未经用户 Play），攒反馈再统一推进。

### 验证命令（恢复后自检）
- 测试：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`（451/451）。
- 改 src 后重建桥 DLL：`dotnet build src/Presentation/ThreeKingdom.Presentation.csproj -c Release` → 复制 Domain/Application/Presentation 三 DLL 到 `Assets/Plugins/`。
- Unity 校验：关闭 Editor 后 batchmode `-quit`，看无 error CS。

---

<!-- STATUS -->
Epic: Sprint 02 — epic-011 战役与生涯 + epic-012 条件历史世界模型（Meta 层 Domain 内核）
Feature: 11 story 已就绪（7 Logic + 4 Integration），QA plan 齐备；进 /dev-story 实现
Task: HEAD=fddf4c3，全部 push tk/main，工作树干净。Sprint 02 已排 + sprint-status.yaml + QA plan。11-1 已 /story-readiness=READY（仅 estimate 占位琐碎项）。★用户在新会话续接，下一步：把估算回写 11 个 story header，或直接 /dev-story 实现 11-1（CareerState 骨架，DAG 根）★。详见顶部「▶▶ 新会话从这里读起（2026-06-25）」
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

### ▶ EPIC_010 Unity 视觉壳（2026-06-23）
- **repo 根 Unity 6000.3.18f1 项目**（batchmode 建，matches CI 默认 projectPath）。`Assets/Plugins` 经 ThreeKingdom.{Domain,Presentation}.dll 桥引用 src/ 权威逻辑（权威源仍 src/，dotnet 测试对源编译；重建步骤见 Assets/Plugins/README.md；tech-debt：未来可改 UPM 包 asmdef 或 CI dotnet build 注入）。
- **三屏 UXML/USS/Controller**（MainMenu/Hud/PauseMenu，Assets/UI）：薄壳绑定只读 ViewModel + 按钮意图经 IntentTranslator → 命令载荷。**batchmode 编译通过**（Assembly-CSharp.dll 产出，无 error CS）= 视觉壳正确引用 Presentation DLL（compile-verified BLOCKING 级）。
- **Editor 预览窗** `Assets/Editor/UxmlPreviewWindow.cs`（菜单「三国/UXML 视觉壳预览」）：无需 Play/Scene 即可加载三屏 UXML+USS 供截图签核（编译通过）。
- commit `69f57a8`（Unity 项目+三屏壳）+ 本次（Editor 预览+文档）。push tk/main。

### 📌 EPIC_010 收尾状态
- **全部 BLOCKING 完成**：5 story 可测逻辑 dotnet 370/370 绿；3 屏 UXML 壳 batchmode 编译通过。
- **剩余皆 ADVISORY（须 graphics 模式 Editor，用户侧）**：三屏视觉/无障碍截图签核（对比度实测/文本150%/键鼠焦点/色盲冗余）；可选 Scene+PanelSettings 进 Play；S5 无障碍设置面板 + 各屏挂接。
- **挂账**：`tools/_unity_probe/` 物理文件夹待用户删（rm 被权限拒）；Assets/Plugins DLL 改 src/ 后须重建。

### ▶ EPIC_010 可 Play 场景搭建（2026-06-23，选项 a）
- **三屏 Scene 已建**：`Assets/Scenes/{MainMenu,Hud,PauseMenu}.unity`，各含 UIDocument（→ 对应 UXML + 共享 `Assets/UI/SlicePanelSettings.asset`，主题 `Assets/UI/SliceTheme.tss` = `@import unity-theme://default`）+ 对应 Controller + EventSystem（StandaloneInputModule；activeInputHandler=0 旧输入，已加 `com.unity.ugui@2.0.0`）。
- **生成器** `Assets/Editor/SliceSceneBuilder.cs`：菜单「三国/构建 Slice 场景」或 batchmode `-executeMethod ThreeKingdom.Unity.EditorTools.SliceSceneBuilder.BuildAll`，程序化建场景/PanelSettings/BuildSettings（避免手写 YAML/GUID）。
- batchmode 已跑通：ugui 解析、生成器编译、三场景生成（含 EventSystem）、无 error CS、退出码 0。MainMenu 为 BuildSettings 首场景。
- **用户侧（ADVISORY，graphics Editor）**：打开任一场景进 Play → 看渲染 + 点击 → 截图签核。
- commit 见下；push tk/main。

## ⏹ 会话收尾 — 2026-06-23（用户确认 Play OK，新会话接续）

- **用户已确认**：Unity Hub 6000.3.18f1 打开项目根 → `Assets/Scenes/MainMenu.unity` → Play **可正常运行**（视觉签核第一步通过）。
- **当前 HEAD = tk/main = `baa77d3`**，工作区干净。测试 **370/370 全绿**（dotnet，BLOCKING）。

### ▶ 新会话入口（EPIC_010 续）
按优先级三选一（lean 节奏，沿用红线）：
1. **(b) Story 005 无障碍设置面板 + 挂接**：建一个无障碍设置屏（UXML/USS/Controller）绑定 `AccessibilitySettings`（已测：缩放/色盲/减动/HUD 可见性 + 序列化 round-trip），并把这些设置挂到 MainMenu/Hud/PauseMenu 三屏（文本缩放应用、reduceMotion 关动效、HUD 元素可见性切换、色盲冗余通道）。+ 可加 `ISettingsStore` 端口持久（复用 epic-009 原子写模式）。逻辑走 dotnet BLOCKING，UXML 壳走 batchmode 编译 + 可选场景。
2. **三屏视觉/无障碍截图签核（ADVISORY，须 graphics Editor）**：打开三场景进 Play，逐项核 hud §12 / 三屏 §12（对比度实测、文本 150% 无溢出、键鼠焦点环可见、色盲去色可辨、点击交互）。证据落 `production/qa/evidence/{main-menu,hud,pause-menu,accessibility}-evidence.md` + 截图。通过后各 story 由 In Progress → Complete。
3. **EPIC_010 收尾判定**：5 story BLOCKING 全绿后，视 ADVISORY 签核进度，决定 epic 关闭或挂 ADVISORY 尾。

### 关键路径与命令
- **跑测试（BLOCKING）**：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`（370/370）。
- **改 src/ 后重建 Unity 桥 DLL**：`dotnet build src/Presentation/ThreeKingdom.Presentation.csproj -c Release` → 复制两 DLL 到 `Assets/Plugins/`（见该目录 README）。
- **重建/新增 Unity 场景**：菜单「三国/构建 Slice 场景」或 batchmode `-executeMethod ThreeKingdom.Unity.EditorTools.SliceSceneBuilder.BuildAll`。
- **batchmode 编译校验**：`Unity.exe -batchmode -nographics -quit -projectPath . -logFile -`，看无 `error CS` + `Library/ScriptAssemblies/Assembly-CSharp*.dll` 产出。
- **Presentation 源**：`src/Presentation/`（Projections/Intents/Screens/Accessibility/Display）；测试 `tests/.../Presentation/`。Unity 壳：`Assets/UI`（UXML/USS/Controller）+ `Assets/Scenes` + `Assets/Editor`。

### 挂账（非阻断）
- `tools/_unity_probe/` 物理文件夹待用户删（`rm -rf` 被权限策略拒）。
- Assets/Plugins 两 DLL 是 src/ 构建产物桥，改 src/ 须重建（tech-debt：未来可改 UPM 包 asmdef 或 CI dotnet build 注入）。
- GitHub Actions 首次绿待确认（Unity job license-gated；domain-tests job 无许可应可绿）。

## ▶ EPIC_010 S5 无障碍面板 + 三屏挂接完成（2026-06-23 本会话）

**可测逻辑（BLOCKING，dotnet 379/379 绿，+9 新测）—— 已落 src/Presentation/Accessibility/**：
- `ISettingsMedium`（命名键读写 + 原子改名原语端口，与存档 ISaveMedium 分离）。
- `ISettingsStore` + `SettingsStore`（临时键写→原子改名编排，镜像 epic-009 SaveRepository；加载时损坏文本回落默认不砸档，区别于存档拒绝语义）。
- `AccessibilitySettingsViewModel`（不可变 with 变换：文本缩放循环档位 100/125/150/175/200、色盲设定、减少动态翻转、HUD 元素可见性翻转；persist/load 经 store）。
- 测试：`AccessibilitySettingsStoreTests`（4：round-trip / 缺失回落 / 损坏回落 / 写失败保留旧 + 临时键清理）+ `AccessibilitySettingsViewModelTests`（5：缩放循环回环 / 变换不可变 / 色盲 / HUD 翻转 / persist-load）。

**Unity 视觉壳（ADVISORY，batchmode 编译干净，227 节点，Assembly-CSharp 产出，无 error CS）—— Assets/UI/**：
- `AccessibilityRuntime`（进程内单一来源；首访从 store 加载，面板提交即写回刷新 Current；默认 PlayerPrefs 介质）。
- `AccessibilityApplier`（把设置应用到任一屏 root：text-scale-*/cb-*/reduce-motion 经 USS class + HUD 元素显隐；**与情境可见性复合——只额外隐藏用户关闭的元素，绝不强制显示**）。
- `AccessibilitySettingsController` + `AccessibilitySettings.uxml/.uss`（自我演示面板：改设置即时应用到本屏）。
- `PlayerPrefsSettingsMedium`（ISettingsMedium 的 Unity 侧 PlayerPrefs 实现；原子改名经键值搬移）。
- `SliceTheme.tss` 增全局 class（text-scale 百分比 / reduce-motion 关过渡 / cb-* slice 阶段仅留钩子不改色）。
- **三屏挂接**：MainMenu/Hud/PauseMenu 的 Controller `OnEnable` 调 `AccessibilityApplier.Apply(root, AccessibilityRuntime.Current)`（HUD 额外含元素可见性复合）。
- **场景**：`SliceSceneBuilder` 增 AccessibilitySettings 屏 → `Assets/Scenes/AccessibilitySettings.unity` 已建（共 4 屏可 Play，batchmode 生成干净）。

**验证**：dotnet test 379/379 -warnaserror 0 warning；batchmode 编译 exit 0 无 error CS；6 个新 Assets/UI .meta + 1 新场景 + .meta 已生成。

### 🐞 修复：SliceSceneBuilder PanelSettings 引用失效（2026-06-23）
- **症状**（用户截图 `D:\Projects\三国演义\UI Test\AccessibilitySettings.png`）：AccessibilitySettings 屏进 Editor 只见空天空、UIDocument 报错。
- **根因**：`BuildAll` 在循环**外**只捕获一次 `PanelSettings panel`；`NewScene(Single)` 每次卸载场景域使该引用在第 2+ 迭代失效 → **只有首屏 MainMenu 序列化到 PanelSettings，其余三屏 m_PanelSettings=None**（不渲染）。上一轮重生连带把 Hud/PauseMenu 也改坏（之前只 Play 过首屏故未暴露）。
- **修复**：PanelSettings 改为**循环内逐场景 `LoadAssetAtPath` 重新加载** + 非空守卫（`Assets/Editor/SliceSceneBuilder.cs`）。
- **验证**（Unity 关闭后 batchmode 重生，exit 0 无 error CS）：四屏 m_PanelSettings 均指向 `guid e8806f1c…`；各屏 sourceAsset 指向各自 UXML；Build Settings 4 屏。Hud/PauseMenu/MainMenu.unity 一并修回（git 显示 M）。

**状态**：S5 BLOCKING（可测逻辑 + 编译级壳）完成 + 四屏场景 PanelSettings 修复并验证。
- **✅ 用户 Play 签核通过（2026-06-23）**：四屏渲染正常 + 按钮可点击 + 功能全测 OK（文本缩放/色盲/减少动态/HUD 可见性即时生效；三屏挂接生效）。视觉+交互 ADVISORY 签核第一步通过。
- **✅ 已入库 push tk/main**（HEAD=`ffad0fc`，工作树干净）：两提交 `0e986d5`（feat S5 面板+挂接）+ `ffad0fc`（fix PanelSettings 引用失效）。
- **▶ 下一步：EPIC_010 收尾判定**。5 story BLOCKING 全绿（dotnet 379/379 + batchmode 编译干净 + 四屏 Play 可交互签核）。可选 ADVISORY 尾：把四屏截图/逐项核对（对比度实测/文本150%无溢出/色盲去色可辨）落 `production/qa/evidence/` → 各 story In Progress→Complete → 决定 epic 关闭。
**剩余 ADVISORY（用户侧 graphics Editor）**：打开 4 屏进 Play 截图签核（文本 150% 无溢出 / 色盲冗余 / 减少动态生效 / HUD 可见性切换）→ 证据落 production/qa/evidence/ → 各 story In Progress→Complete → EPIC_010 收尾判定。
**改动文件清单（待 commit）**：M Assets/Plugins/{Domain,Presentation}.dll、Assets/UI/{Hud,MainMenu,PauseMenu}Controller.cs、Assets/UI/SliceTheme.tss、Assets/Editor/SliceSceneBuilder.cs；?? Assets/UI/Accessibility*{.cs,.uss,.uxml}、Assets/UI/PlayerPrefsSettingsMedium.cs、各 .meta、Assets/Scenes/AccessibilitySettings.unity(.meta)、src/Presentation/Accessibility/{ISettingsMedium,SettingsStore,AccessibilitySettingsViewModel}.cs、tests/.../Presentation/AccessibilitySettings{Store,ViewModel}Tests.cs。
