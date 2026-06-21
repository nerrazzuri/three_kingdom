# Story 001: PlanDraft 零副作用与原子提交

> **Epic**: 战前准备
> **Status**: Ready
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-009-battle-preparation.md
**Requirement**: TR-prep-001

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: PlanDraft 不改权威 state；提交全通过验证才原子生成 CommittedPlan（全有或全无）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: gameplay state 只由 Domain 经 Command 修改（P4）
- Forbidden: UI 直接改状态；草稿产生副作用
- Guardrail: 提交全有或全无，无部分写入

---

## Acceptance Criteria

- [ ] PlanDraft 构建/编辑期不修改任何权威 state（草稿后哈希不变）
- [ ] 提交经 Command，全部验证通过才原子生成 CommittedPlan
- [ ] 任一失败 → 零部分写入，返回稳定错误码
- [ ] CommittedPlan 占用同步反映到资源/人物（不可静默回退）

---

## Implementation Notes

*Derived from ADR-0002:*
- PlanDraft 为 Presentation/Application 侧可变结构；Domain 只接收 SubmitDeploymentPlanCommand。
- 提交事务：校验（Story 002）→ 通过 → 原子写；失败回滚。
- 与 P4（Draft/Committed 双态）UI 契约对齐。

---

## Out of Scope

- Story 002: 硬冲突/DAG 校验细节
- 战役执行（epic-007）

---

## QA Test Cases

- **AC-1**: 草稿零副作用
  - Given: 任意草稿编辑序列
  - When: 不提交
  - Then: 权威状态哈希不变
  - Edge cases: 大量草稿操作后放弃
- **AC-2**: 原子提交
  - Given: 含一处硬冲突的计划
  - When: 提交
  - Then: 零部分写入，返回稳定错误；无冲突则原子生效
  - Edge cases: 边界资源恰好够/差一

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/prep/draft_atomic_commit_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-003 Story 002（权限）；epic-004（资源）
- Unlocks: Story 002；epic-007（消费 CommittedPlan）
