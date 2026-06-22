# Story 003: 士气/疲劳/军纪三维与阈值检查

> **Epic**: 兵法沙盒结算
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-011-morale-fatigue.md
**Requirement**: TR-cohesion-001、TR-cohesion-002

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 士气/疲劳/军纪三维独立不合并；阈值综合军纪/指挥/退路（非单值）；拆分/合并按人数加权。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Core)**:
- Required: 多维独立（P6）；011 唯一施加 morale·fatigue（断粮传导单一权威）
- Forbidden: 三维合并成单一数值；阈值由单值决定
- Guardrail: 同一士气事件幂等

---

## Acceptance Criteria

- [ ] 士气、疲劳、军纪三维独立保存，不合并为单一数值（P6）
- [ ] 士气事件按受众权重聚合，同一事件幂等
- [ ] 阈值检查综合军纪 + 指挥 + 退路（非单一数值决定）
- [ ] 拆分/合并状态按人数加权（非取最大）
- [ ] 011 为 morale/fatigue 唯一施加点（消费 012 supply 事件，010 只读）

---

## Implementation Notes

*Derived from ADR-0004 + systems-index S-W1/C-W1:*
- 三维各为独立定点量；事件带稳定 ID 幂等去重。
- 阈值函数 = f(军纪, 指挥, 退路)，多输入；崩溃非单一士气触发。
- civ_morale（城市民心）与 unit_morale（部队士气）命名消歧（C-W1 修复）。

---

## Out of Scope

- 断粮 supply 事件产出（epic-004 Story 002）
- 城市民心（epic-004）

---

## QA Test Cases

- **AC-1**: 三维独立 + 幂等
  - Given: 同一士气事件应用两次
  - When: 聚合
  - Then: 仅生效一次；疲劳/军纪不被士气改写
  - Edge cases: 同事件不同受众权重
- **AC-2**: 阈值多输入
  - Given: 高士气但军纪崩/无退路
  - When: 阈值检查
  - Then: 仍可触发动摇（非单一士气决定）
  - Edge cases: 拆分按人数加权（非取最大）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/cohesion/cohesion_three_dim_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001；epic-004 Story 002（断粮事件）
- Unlocks: Story 002（链消费士气）；epic-008（士气后果）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 5/5 passing（无 deferred）
**Deviations**: ADVISORY — 测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/Cohesion/`；阈值结果用 Steady/Wavering/Routed 三态表达多输入判定
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/Cohesion/CohesionThreeDimensionTests.cs`（9 测全绿，294/294 总）
**Code Review**: Complete（APPROVED）
