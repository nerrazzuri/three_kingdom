# Story 001: 确定性时间推进与时段/日界结算

> **Epic**: 世界基底（时间·环境·地图拓扑）
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-001-game-time.md
**Requirement**: TR-time-001

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 整数/定点 + 确定性；同点事件按稳定优先级与稳定 ID 全序解析，不依赖帧率。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: 时间不得依赖 Unity Time.deltaTime；模拟引擎无关。

**Control Manifest Rules (Foundation)**:
- Required: 只有时间推进命令能前进；时间确定性
- Forbidden: UI 或单系统私自跳时；Domain 依赖帧率/Unity 时间
- Guardrail: 同输入 → 同时段变化与事件序列

---

## Acceptance Criteria

- [ ] 权威时间以 WorldDay + DaySegment 表示，仅由推进命令前进
- [ ] 同一时间点的多事件按稳定优先级 + 稳定 ID 全序解析（确定性）
- [ ] 日界按全局顺序触发：时间推进 → 环境 → 补给 → 城市 → 状态事件（systems-index §跨系统结算顺序）
- [ ] 同输入序列 → 同时段变化与事件序列（可复现）

---

## Implementation Notes

*Derived from ADR-0004 + systems-index §日界顺序:*
- WorldDay/DaySegment 为 Domain 值对象；推进经 Command。
- 同点事件排序键：(priority, stableId)；禁用字典遍历序等非确定来源。
- 日界顺序由时间系统编排，调用各系统的「日界结算」钩子，固定顺序。

---

## Out of Scope

- Story 002: 嵌套战斗时段预算
- 天气/补给/城市各自结算逻辑（各 epic）— 本 story 只编排顺序与时间权威

---

## QA Test Cases

- **AC-1**: 同点事件全序确定
  - Given: 同一时段注入多个事件（含同优先级）
  - When: 解析
  - Then: 顺序由 (priority, stableId) 唯一确定，多次运行一致
  - Edge cases: 同优先级同 ID（非法，须拒绝/断言）
- **AC-2**: 日界顺序正确
  - Given: 跨日界推进
  - When: 结算
  - Then: 钩子调用序为 环境→补给→城市→状态事件
  - Edge cases: 补给不消费同边界城市新产粮（有意约束）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Time/DeterministicTimeTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（18 测，全套 92/92 绿）
**Note**: 路径由故事原写的 `tests/unit/time/deterministic_time_test.cs` 归一到真实可编译测试工程。

---

## Dependencies

- Depends on: epic-001 Story 002（定点/哈希）
- Unlocks: Story 002；epic-004（日界城市/补给）、epic-007（战斗时段）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（WorldTime 权威时间仅命令前进、同点事件 (priority,stableId) 全序、日界顺序 环境→补给→城市→状态事件、同输入→同结果可复现）
**Files**: `src/Domain/Time/`（DaySegment、WorldTime、DayBoundaryStage、ScheduledEvent+ScheduledEventOrder、WorldClock+AdvanceTimeCommand+AdvanceResult+DayBoundarySettlement）+ `tests/unit/ThreeKingdom.Domain.Tests/Time/DeterministicTimeTests.cs`（18 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程（见 Test Evidence Note）
**Test Evidence**: Logic — 测试文件存在且通过（全套 92/92 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（ADR-0004 确定性全序 + 引擎无关；SegmentsPerDay 由枚举派生）
