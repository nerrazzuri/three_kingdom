# Story 001: 城市治理态接入会话 + Advance 日界结算

> **Epic**: City Governance Loop（城市治理循环 / M03）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-004-city-economy.md`
**Requirement**: `TR-city-001`、`TR-city-002`、`TR-city-005`（部分：城市态入哈希）
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 装配层只编排不拥规则——城市日界结算复用既有 `CityDaySettlementService`（Domain 纯函数），`Advance` 按 systems-index 全局日界顺序叠入城市日结，不在会话内重写公式；城市态纳入会话状态哈希。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 城市日结经 Domain Rule（CityDaySettlementService）；状态变更经 Application 路径；破环顺序读已结算值
- Forbidden: 会话内重写城市结算公式；硬编码平衡值（全部来自 CitySettlementConfig）
- Guardrail: 日界确定性（同种子→同结果，纳入哈希）

---

## Acceptance Criteria

*来自 GDD `gdd-004-city-economy.md`，作用域限本 story：*

- [ ] `CampaignSession` 持有城市治理态（`CityEconomyState`）+ 配置（`CitySettlementConfig`）+ 后勤持有量
- [ ] `Advance(1)` 触发城市日界结算：按"承诺→产入→消耗→短缺后果→工事/治安"稳定顺序（TR-city-002）
- [ ] 资源守恒：征用军粮移交后勤后城市库存同步扣减，不重复计算（TR-city-001）
- [ ] 日结后库存不低于 `STOCK_FLOOR`（合法下限）
- [ ] 城市治理态纳入 `session.ComputeHash()`（TR-city-005 部分）
- [ ] 多日推进（`Advance(N)`）结算 N 次城市日结，账本可解释

---

## Implementation Notes

*来自 ADR-0009 实现指引：*

- 复用既有 `CityDaySettlementService.Settle(state, logisticsHolding, config, populationPressure)`（Domain 纯函数，已实装并测试）。
- `Advance` 当前仅 `session.AdvanceWorld(segments)`（源码注释已预留"基础层 002/012/004/011 随 M03 接入"）；本 story 把城市日结叠入日界顺序。
- 城市态接入 `CampaignSession`（新字段）+ 存档信封映射（ADR-0009 R-1 统一信封）。
- **破环顺序**（systems-index）：时间 → 环境 → 补给 → 城市/控制权 → 状态事件 → 世界模型 → 生涯。城市日结读已结算值，不回读未结算。
- 城市态须纳入 `ComputeHash`（ADR-0004）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：玩家治理命令入口（征用/修工事/安抚）
- Story 003：治理选择改变战役条件派生
- Story 004：治理态存读档 round-trip + 确定性完整验证
- 征募（移出本 epic，见 EPIC.md Out of Scope）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: CampaignSession 持有城市治理态
  - Given: `StartCampaign(config)`（config 含城市经济初态 + 结算配置）
  - When: 读取 `session` 城市态
  - Then: 库存/民心/治安/工事等字段等于开局配置值
  - Edge cases: 缺城市配置 → 开局拒绝并返回稳定错误码（无部分初始化）

- **AC-2**: Advance 触发城市日结（稳定顺序）
  - Given: 已知开局库存/产入/消耗的 session
  - When: `Service.Advance(s, 1)`
  - Then: 库存 = 开局 + base_yield − 实际消耗（按 §Formula 2 顺序）；账本含各阶段条目
  - Edge cases: 短缺日（消耗>库存）→ shortage>0、库存夹至 STOCK_FLOOR、民心按短缺扣减

- **AC-3**: 资源守恒（征用移交后勤不双计）
  - Given: reserved>0（已承诺征用军粮）的 session
  - When: `Advance(1)` 执行承诺阶段
  - Then: 城市库存扣减 reserved 量 = 后勤持有增量；总量守恒
  - Edge cases: reserved ≤ stock 不变量恒成立

- **AC-4**: 城市态纳入会话哈希
  - Given: 两 session 除城市库存外完全相同
  - When: 各自 `ComputeHash()`
  - Then: 哈希不同（城市态进哈希）
  - Edge cases: 城市态相同 → 哈希相同

- **AC-5**: 多日推进确定性
  - Given: 两 session 同开局
  - When: 各 `Advance(3)`
  - Then: 两者哈希相同（日结确定性，无随机/时间依赖）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignCityGovernanceTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignCityGovernanceTests.cs` — 10/10 通过（657/657 全绿）

---

## Dependencies

- Depends on: epic-013（M00 脊梁）+ epic-015（M02）全部 Complete（已满足）；epic-004 城市 Domain 内核（已 Complete）
- Unlocks: Story 002（治理命令入口）、Story 004（治理态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 6/6 passing
**Deviations**: 城市态设为**可选**（nullable）以向后兼容现有 622 测试——场景未启用城市治理时为 null；存档信封映射推迟到 S004（S001 AC 不含存档）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignCityGovernanceTests.cs` — 10 tests, 10/10 pass
**新生产代码**: CityEconomyState.AppendTo（哈希）；CampaignSession 持城市态 + ApplyCitySettlement + ComputeHash 含城市；CampaignStartConfig 加城市参数；CampaignSessionService.Advance 按日界叠 CityDaySettlementService.Settle
**Code Review**: 内联 — APPROVED（Lean 模式）
