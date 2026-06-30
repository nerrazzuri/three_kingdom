# Story 003: 治理选择改变战役条件（≥3 条差异化派生 + 可解释代价）

> **Epic**: City Governance Loop（城市治理循环 / M03）
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-004-city-economy.md`
**Requirement**: `TR-city-004`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0003: 数据驱动配置（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 治理态 → 战役条件输入的派生为确定性纯函数，系数来自版本化配置；派生产生可解释账本（每条战役条件标注其治理来源与代价），是"喂给战争的筛选尺子"——治理深度只在能改变战役/守城/生涯条件时才有意义。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature 层)**:
- Required: 战斗结果可确定性复现；所有平衡值数据驱动
- Forbidden: 方法体内硬编码派生系数；Domain 用 float 影响状态哈希（用整数/定点）
- Guardrail: 派生为纯函数（同输入→同输出）

---

## Acceptance Criteria

*来自 GDD `gdd-004-city-economy.md` + module-plan §M03 验收，作用域限本 story：*

- [ ] **条件①守城强度**：工事状态 → 守城强度派生值（fort 越高守城强度越高），有可解释账本条目
- [ ] **条件②补给能力**：征用军粮 → 后勤可用补给量↑（但民心↓，代价可解释）
- [ ] **条件③民心风险**：民心/治安 → 骚乱/守城意愿风险派生值（民心越低风险越高）
- [ ] 三条派生均为确定性纯函数：同治理态 → 同战役条件输入
- [ ] 不同治理选择产生**可区分**的战役条件输入（高工事 vs 低工事 → 不同守城强度值）
- [ ] 每条战役条件附可解释代价账本（治理来源 + 代价说明，非黑箱数值）

---

## Implementation Notes

*来自 ADR-0003 / ADR-0004 实现指引：*

- **边界**：完整战斗胜负在战斗系统（epic-007），其会话装配是 M05（epic-018）。本 story 只实现**治理态 → 战役条件输入的派生投影**（守城强度/补给/民心风险），不接完整战斗胜负。
- 派生为 Domain 纯函数（输入城市治理态 + 配置，输出战役条件输入 DTO + 账本）；系数来自配置。
- 使用整数/定点（Q16.16），不用 float（ADR-0004 禁则）。
- "可解释代价"= 每条派生输出附 ledger 条目（参考 `CitySettlementLedgerEntry` 模式）。
- 这是 M05 战前准备装配的输入接口预留：M05 将消费本 story 的派生值喂给战斗。

---

## Out of Scope

*由邻近 story 处理 / 后续模块——本 story 不实现：*

- 完整战斗胜负结算（epic-007 / M05 epic-018 装配）
- Story 001/002：城市态接入 + 治理命令
- Story 004：存读档确定性完整验证
- 征募（移出本 epic）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 工事 → 守城强度派生（条件①）
  - Given: 两城市态除工事外相同（fort=高 vs fort=低）
  - When: 派生守城强度
  - Then: 高工事守城强度 > 低工事；两者均附账本条目说明来源
  - Edge cases: fort=0 → 守城强度为配置下限（非负）

- **AC-2**: 征用 → 补给能力派生 + 民心代价（条件②）
  - Given: 征用 vs 不征用的两治理态
  - When: 派生补给能力 + 读民心
  - Then: 征用方补给↑、民心↓；账本说明"补给来自征用，代价=民心 −X"
  - Edge cases: 征用量 0 → 补给与民心均无变化

- **AC-3**: 民心/治安 → 风险派生（条件③）
  - Given: 高民心 vs 低民心两治理态
  - When: 派生骚乱/守城意愿风险
  - Then: 低民心风险 > 高民心；附账本
  - Edge cases: 民心达上限 → 风险为配置下限

- **AC-4**: 派生确定性
  - Given: 同一治理态
  - When: 派生两次
  - Then: 两次输出完全相同（纯函数，无随机/float 漂移）
  - Edge cases: N/A

- **AC-5**: 治理选择可区分性（综合）
  - Given: 三组不同治理选择（重工事 / 重征用 / 重安抚）从同一开局
  - When: 各自派生战役条件输入
  - Then: 三组战役条件输入两两可区分（证明治理选择确实改变战役条件）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/City/GovernanceWarConditionTests.cs` — 必须存在且全绿

**Status**: [x] `City/GovernanceWarConditionTests.cs` — 7/7 通过（657/657 全绿）

---

## Dependencies

- Depends on: Story 002 DONE（治理命令产生差异态是派生的输入来源）
- Unlocks: Story 004；M05（epic-018 战前准备装配将消费本派生）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 6/6 passing（三条战役条件 + 确定性 + 可区分 + 可解释账本）
**Deviations**: 按计划只派生战役条件输入（守城强度/补给/民心风险），不接完整战斗胜负（留 M05 epic-018 消费）。纯 Domain 函数，不碰会话。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/City/GovernanceWarConditionTests.cs` — 7 tests, 7/7 pass
**新生产代码**: WarCondition.cs（WarConditionKind/WarConditionLedgerEntry/WarConditionInputs/WarConditionConfig）；WarConditionProjection.cs（纯函数派生 + 可解释账本）
**Code Review**: 内联 — APPROVED（Lean 模式）
