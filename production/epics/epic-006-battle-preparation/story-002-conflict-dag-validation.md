# Story 002: 硬冲突校验与 DAG 依赖图

> **Epic**: 战前准备
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-009-battle-preparation.md
**Requirement**: TR-prep-002

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 硬冲突阻止提交；命令依赖图须为 DAG，校验确定性。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 校验确定性；区分错误（阻断）与风险（P7）
- Forbidden: 循环依赖通过校验
- Guardrail: 同计划 → 同校验结论

---

## Acceptance Criteria

- [ ] 硬冲突检测：占用 / 资源 / 可达 / 权限 / 循环依赖
- [ ] 命令依赖图须为 DAG；检出环则拒绝
- [ ] 区分「错误」（硬冲突阻断）与「风险」（可提交有代价，P7）
- [ ] 校验确定性：同计划同结论（平局/遍历序稳定）

---

## Implementation Notes

*Derived from ADR-0004:*
- 依赖图拓扑排序检环；平局按稳定 ID。
- 错误聚合返回（一次列全部硬冲突，非首个即停）；风险单列不阻断（P7 UI 契约）。

---

## Out of Scope

- 风险 UI 呈现（hud.md P7）
- 提交事务（Story 001）

---

## QA Test Cases

- **AC-1**: 循环依赖拒绝
  - Given: 命令 A→B→A 依赖
  - When: 校验
  - Then: 检出环并拒绝
  - Edge cases: 自依赖、长环
- **AC-2**: 错误 vs 风险
  - Given: 含硬冲突 + 软风险的计划
  - When: 校验
  - Then: 硬冲突阻断；风险列出但不阻断
  - Edge cases: 资源恰好够（风险非错误）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/prep/conflict_dag_validation_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-002 Story 004（可达）；epic-003 Story 002（权限）
- Unlocks: Story 001（提交调用校验）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（无 deferred）
**Deviations**: ADVISORY — 测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/Preparation/`；按依赖方向先于 Story 001 实现（S2 校验器被 S1 提交消费）
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/Preparation/ConflictDagValidationTests.cs`（12 测全绿，268/268 总）
**Code Review**: Complete（APPROVED）
