# Cross-GDD Review Report — 焦点审查 GDD_019 出征攻城

> **Date**: 2026-07-04
> **Mode**: 焦点审查（Focus）——GDD_001–016 已于 2026-06-24 全量交叉审查（见 gdd-cross-review-2026-06-24.md），本轮只审 2026-07-04 新增的**进攻侧循环契约边界**。
> **Scope**: GDD_019 出征攻城（Draft）对照 GDD_004 城池控制权 / GDD_009 备战 / GDD_010 兵法沙盒 / GDD_014 战役与生涯 / GDD_015 世界模型 / GDD_007 情报 + ADR-0008 城池控制权契约 + ADR-0010 占城归属契约。
> **Verdict**: **CONCERNS**（无 Blocking；5 Warning 均为小幅 GDD 编辑可闭）

---

## 通过项（契约核对无误）

- **GDD_009 `CommittedPlan`** — 存在（gdd-009 L21/118）。GDD_019 R3 / Data Model 引用正确。
- **GDD_004 控制权变更事件** — 唯一权威（gdd-004 L146）。占城复用它，与 GDD_014 既有"夺城/易主经 004"用法（gdd-014 L114）方向一致，进攻方向无冲突。
- **反全知（AC-7）** — 与 GDD_007 `FactionKnowledge` + 编译级反全知锁一致；连继承基业后君主玩法都守（gdd-014 L131）。
- **失败可继续（AC-6/§11）· 无胜率 · 兵法条件涌现（AC-3）· 确定性** — 设计锁完好，未引入计策按钮或胜率数字。
- **占城 C 判定（ADR-0010）** — 复用 ADR-0008 控制权 + ADR-0006 种子流 + ADR-0009 装配层不拥规则，自洽。

---

## Consistency Issues

### Blocking
无。

### Warnings

**W1 —〔契约未接线，最重要〕`rebellion_lean` 无消费方**
GDD_019 R4/F4 + ADR-0010 D3 新造累加量 `rebellion_lean`，声称"喂 GDD_014 自立线触发"。但 GDD_014 自立触发（L40）三条件（①≥3 自主城+兵粮 ②名望+好感 ③主君昏庸/贬谪）**无一读 `rebellion_lean`**——"喂自立线"断头。
→ **修**：GDD_014 §自立触发条件③扩为可由"屡被夺战果（rebellion_lean）"累积推动；registry 登记 `rebellion_lean`（source=ADR-0010/GDD_019，referenced_by=GDD_014）。

**W2 —〔陈旧引用〕`TacticCondition` 在 GDD_010 无此命名 + 与事后识别器混淆**
GDD_019 F2 引 `TacticCondition`（GDD_010）"条件全→兵法成型→结算加成"。GDD_010 只有 `TacticRecognizer`，且它**事后只读、不参与结算**（gdd-010 L42/202）；结算优势来自 §7 条件涌现，非识别器。
→ **修**：F2 改引 GDD_010 §7 条件涌现结算路径，分述"事后打标签（TacticRecognizer）"与"结算加成（条件涌现）"；`TacticCondition` 若保留须在 GDD_010 正式命名（本轮采用前者，不新增命名）。

**W3 —〔缺口〕GDD_014 无"出征授权/朝廷政令"子类型**
GDD_019 R1 依赖"出征=朝廷政令任务（GDD_014 忠臣线）"。GDD_014 有"君主任务/LordMissionLog"与"授权"概念，但未定义**出征授权**这一任务子类型。`CampaignAuthorization` 为 019 新引入、014 未承认。
→ **修**：GDD_014 把"出征授权"列为君主任务子类型，授权额度/战区随 Rank（014 已有 Rank/授权钩子）。

**W4 —〔依赖不对称〕反向引用缺失**
GDD_004/009/010/014/015 的 Dependencies 均未把 GDD_019 列为 dependent。
→ **修**：5 份依赖段各加一行反向引用（轻量 Edit）。

### Advisory
- **A1** 新跨系统名（`conquestIndex` / `rebellion_lean` / `OwnershipVerdict` / `CampaignAuthorization`）ADR 落定后跑 `/consistency-check` 登记 registry。

---

## Game Design Issues

### Warnings

**W5 —〔反支柱〕出征作为强功绩源，风险"只刷出征"**
出征引入强战功→功绩/名望通道。若速率压过治理/招揽等非战斗源，踩中 game-pillars 反支柱"最优玩法只需刷战斗"（同 2026-06-24 W5 / GDD_014 N10）。
→ **修**：GDD_019 §8 Tuning + 平衡验证显式护栏——出征功绩速率不得使非战斗源失竞争力。

### Advisory
- **A2** 前向 epic 依赖（epic-020 失败可继续 / epic-022 升官 / epic-025 多城委任）须在引用它们的 story 实现前存在。
- **A3** `OwnershipVerdict=GrantToPlayer` 令玩家真直辖多城 → 强依赖 epic-025（ADR-0010 已知代价，M12 前置）。

---

## Cross-System Scenario Issues

Scenarios walked: 2

**场景 A「攻下第三座 → 君主种子取舍收走 → 自立累积」** — GDD_019/ADR-0010 + GDD_004 + GDD_014
触发 F3 → `LordKeeps` → D3 `rebellion_lean +=` → **在 W1 处断链**（GDD_014 自立触发不读它）。实证 W1。

**场景 B「裸战 vs 备好条件攻同城」** — GDD_019 + GDD_009 + GDD_010
准备态派生初始条件（R3）→ GDD_010 §7 确定性结算 → 不同胜负。链通；唯 W2 命名/职责需厘清（不影响链正确性）。

无 Blocker / 无未定义行为。

---

## GDDs Flagged for Revision

| GDD | 原因 | 类型 | 优先级 |
|-----|------|------|--------|
| gdd-019 | W2 `TacticCondition` 陈旧引用 / W5 平衡护栏 | Consistency + Design | Warning |
| gdd-014 | W1 自立触发未接 rebellion_lean / W3 缺出征授权子类型 | Consistency | Warning |
| gdd-004 / 009 / 010 / 015 | W4 反向依赖缺失 | Consistency | Warning |

---

## Verdict: CONCERNS

无 Blocking——设计锁完好、确定性/反全知/失败可继续全守、契约方向无冲突。5 项 Warning 均为小幅 GDD 编辑即可闭合。

**建议处置序**：W1–W5 就地 Edit 闭合 → GDD_019 Draft→Reviewed → ADR-0010 文件头 Proposed→Accepted（与 technical-preferences 日志对齐）→ `/create-stories epic-029`（W1/W2 分别在 story-004/003 实现前已闭）→ ADR 落定后 `/consistency-check` 登记 A1 新名。
