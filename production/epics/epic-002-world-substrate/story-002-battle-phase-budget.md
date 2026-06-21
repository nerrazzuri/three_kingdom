# Story 002: 嵌套战斗时段预算与跨时段触发

> **Epic**: 世界基底（时间·环境·地图拓扑）
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-001-game-time.md
**Requirement**: TR-time-002（+ TR-time-003 数据模型部分）

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟（secondary ADR-0003 配置）
**ADR Decision Summary**: 时段预算由配置驱动；耗尽确定性跨入下一世界时段并触发结算。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Foundation)**:
- Required: 平衡值（时段预算）数据驱动
- Forbidden: 硬编码预算；float 计时
- Guardrail: 预算耗尽跨段确定，触发天气/补给/疲劳结算

---

## Acceptance Criteria

- [ ] 嵌套 BattlePhase 按配置预算消耗
- [ ] 预算耗尽 → 跨入下一世界时段，触发天气/补给/疲劳结算
- [ ] 行动耗时/期限/取消的数据模型定义完整且可被存档（衔接 TR-time-003）

---

## Implementation Notes

*Derived from ADR-0004:*
- BattlePhase 预算为整数（来自 epic-001 配置管线）。
- 跨段触发复用 Story 001 的日界钩子编排。
- 期限/取消模型用稳定 ID，确保 round-trip 后事件序一致（epic-009 验证）。

---

## Out of Scope

- 战斗内部管线（epic-007）— 本 story 只管时段预算与跨段触发
- 存档 round-trip 验证 → epic-009 Story 002

---

## QA Test Cases

- **AC-1**: 预算耗尽跨段
  - Given: 配置预算 N 的战斗时段
  - When: 消耗至耗尽
  - Then: 确定性跨入下一世界时段并触发结算钩子
  - Edge cases: 预算恰好为 0、单次消耗超额（夹取至边界）
- **AC-2**: 行动耗时/期限模型可序列化
  - Given: 含在途行动与期限的状态
  - When: 序列化再反序列化（占位测试，完整 round-trip 在 epic-009）
  - Then: 字段无损

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/time/battle_phase_budget_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（时间权威与日界顺序）
- Unlocks: epic-007（战斗解析消费时段预算）
