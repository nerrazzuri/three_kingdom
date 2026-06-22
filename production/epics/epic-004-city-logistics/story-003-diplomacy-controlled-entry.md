# Story 003: 外交受控入口（求援/求粮/求时限，§8）

> **Epic**: 城市与后勤
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-012-logistics-supply.md §8（外交受控入口）
**Requirement**: 支柱4 外交在 slice 的唯一落点（G3 阻断闭合项）— 经 TR-supply 守恒约束 + 时间期限模型

**ADR Governing Implementation**: ADR-0002（secondary ADR-0004）
**ADR Decision Summary**: 外交为单一受控入口经 Command；延迟交付 + 条件判定 + 代价 + 可背约，确定性可重放。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 外交只暴露一个受控入口，不运行完整天下外交模拟
- Forbidden: 即到保证按钮；外势力完整 AI
- Guardrail: 延迟交付 + 可背约失败 + 守恒 + 确定性

---

## Acceptance Criteria

- [ ] 三选一受控入口：求援（援军）/ 求粮（补给）/ 求时限（压力），作用于 slice
- [ ] 响应判定（grant_score）+ 延迟交付时间（非即到）确定性计算
- [ ] 兑现/背约：可背约失败路径存在（非保证按钮）
- [ ] 代价兑付：成功有代价（条约信誉/资源）
- [ ] 守恒：交付的援军/补给计入对应权威库存，不凭空增加
- [ ] 外势力为静态背景，不运行完整外交模拟

---

## Implementation Notes

*Derived from gdd-012 §8 + G3 闭合记录:*
- DiplomaticPledge 数据模型（请求类型/grant_score/交付时段/代价/可背约标记）。
- 在途交付随时间推进结算（衔接 epic-002 时间）；交付落入城市/后勤库存（守恒）。
- slice 的 DiplomacyEvaluator（grant 判定/延迟/可背约/援军→兵力）可作参考（重写）。

---

## Out of Scope

- 完整天下外交（明确非目标）
- 援军参战解析（epic-007）

---

## QA Test Cases

- **AC-1**: 延迟交付 + 可背约
  - Given: 提交求援承诺
  - When: 推进时段
  - Then: 按 grant_score 延迟交付或背约失败；非即到
  - Edge cases: grant 拒绝、交付前敌军已到、背约触发
- **AC-2**: 交付守恒
  - Given: 求粮成功交付
  - When: 结算
  - Then: 补给计入后勤库存，总量守恒，记代价
  - Edge cases: 部分交付、代价不足

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/supply/diplomacy_controlled_entry_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 002（补给库存）；epic-002 Story 001（时间）
- Unlocks: epic-007（守城待变链消费外交输入）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 6/6 passing（无 deferred）
**Deviations**: ADVISORY — 测试路径 `tests/integration/supply/*.cs` → 归一到唯一可编译工程 `tests/unit/ThreeKingdom.Domain.Tests/Diplomacy/`（自动化集成风格测试）
**Test Evidence**: Integration — `tests/unit/ThreeKingdom.Domain.Tests/Diplomacy/DiplomacyControlledEntryTests.cs`（14 测全绿，221/221 总）
**Code Review**: Complete（APPROVED）
