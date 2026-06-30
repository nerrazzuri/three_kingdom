# Story 003: 冲突 DAG 拒绝非法计划（失败无部分写入）

> **Epic**: War Preparation / Commitment Loop（战役准备循环 / M05）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，纯测试为主）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-009-battle-preparation.md`
**Requirement**: `TR-prep-002`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 硬冲突（占用/资源/可达/权限/循环依赖）阻止提交；命令依赖图须为 DAG。任一硬冲突 → 全单拒绝、资源池**不变**、无部分写入（TR-prep-002）。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 失败必须产生可继续状态；硬冲突阻止提交、无部分写入
- Forbidden: 部分提交（扣一半资源）；环依赖通过
- Guardrail: 拒绝后资源池哈希不变（原子性）

---

## Acceptance Criteria

*来自 GDD `gdd-009-battle-preparation.md`，作用域限本 story：*

- [ ] 资源不足（need > avail）→ `ResourceShortage` 拒绝、资源池不变、无 CommittedPlan
- [ ] 目标不可达 → `Unreachable` 拒绝、资源不变
- [ ] 执行者无权限 → `NoAuthority` 拒绝、资源不变
- [ ] 循环依赖（非 DAG）→ `CyclicDependency` 拒绝、资源不变
- [ ] 任一硬冲突拒绝后 `session.ComputeHash()` 不变（无部分写入）；会话可继续（再提合法计划成功）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `PlanValidator` / `PlanCommitService`）：*

- 复用 `PlanCommitService.Submit` 既有硬冲突校验（`PlanValidator.Validate` 产出 `PlanError` 列表，`CanCommit` false 则全单拒绝）。
- 会话 `SubmitPlan` 失败时**不**写回资源池（`SubmitPlanResult.Failure` 返回 unchangedPool；会话不更新）。
- 本 story 主要新增**测试**覆盖各硬冲突码（生产代码在 S002 已就位，本 story 验证拒绝路径 + 原子性）。
- `PlanErrorCode`：ResourceShortage=1 / TimeOverlap=2 / Unreachable=3 / NoAuthority=4 / CyclicDependency=5。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002：准备态接入 / 合法提交
- Story 004：准备态存读档
- 软风险警告（TightResource，只警告不阻断，非本 story 焦点）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 资源不足拒绝 + 资源不变
  - Given: 资源池 {粮:30}，草稿命令需 {粮:100}；`before = ComputeHash()`
  - When: `Service.SubmitPlan(s)`
  - Then: `Committed == false`；Validation 含 `ResourceShortage`；`ComputeHash() == before`；无 CommittedPlan
  - Edge cases: 多命令累计超额也拒绝

- **AC-2**: 目标不可达拒绝
  - Given: 命令目标区域不在可达集；`before = ComputeHash()`
  - When: `SubmitPlan(s)`
  - Then: `Committed == false`；含 `Unreachable`；资源不变
  - Edge cases: N/A

- **AC-3**: 执行者无权限拒绝
  - Given: 命令执行者不在授权集；`before = ComputeHash()`
  - When: `SubmitPlan(s)`
  - Then: `Committed == false`；含 `NoAuthority`；资源不变
  - Edge cases: N/A

- **AC-4**: 循环依赖（非 DAG）拒绝
  - Given: 草稿命令 A 依赖 B、B 依赖 A
  - When: `SubmitPlan(s)`
  - Then: `Committed == false`；含 `CyclicDependency`；资源不变
  - Edge cases: 自依赖（A 依赖 A）也拒绝

- **AC-5**: 拒绝后会话可继续
  - Given: 一次被拒提交后
  - When: 修正草稿为合法 → `SubmitPlan(s)`
  - Then: 第二次提交成功（失败不卡死会话）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPlanConflictTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignPlanConflictTests.cs` — 5/5 通过（703/703 全绿）

---

## Dependencies

- Depends on: Story 002 DONE（SubmitPlan 路径，本 story 测其拒绝分支）
- Unlocks: Story 004（准备态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing
**Deviations**: 无（生产代码在 S002 SubmitPlan 已含拒绝分支，本 story 验证各硬冲突码 + 原子性 + 可继续）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPlanConflictTests.cs` — 5 tests（ResourceShortage/Unreachable/NoAuthority/CyclicDependency + 可继续）
**Code Review**: 内联 — APPROVED（Lean）
