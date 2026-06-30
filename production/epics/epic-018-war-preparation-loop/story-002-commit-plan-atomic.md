# Story 002: 合法计划原子提交（CommittedPlan + 资源锁定）

> **Epic**: War Preparation / Commitment Loop（战役准备循环 / M05）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-009-battle-preparation.md`
**Requirement**: `TR-prep-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 提交命令经会话路径调既有 `PlanCommitService.Submit`——全部通过验证才**原子**生成 `CommittedPlan` + 一次性扣减资源池（全有或全无）。装配层不重写校验/扣减公式。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 提交经 Command 路径复用 PlanCommitService；原子（全有或全无）
- Forbidden: 自动布阵；会话内重写资源扣减
- Guardrail: 同草稿+同资源+同命令流 → 同提交结果（确定性）

---

## Acceptance Criteria

*来自 GDD `gdd-009-battle-preparation.md`，作用域限本 story：*

- [ ] 提交命令经会话路径调 `PlanCommitService.Submit`：合法草稿 → 生成 `CommittedPlan` 存会话
- [ ] 合法提交后资源池按总需求**原子扣减**（资源锁定）
- [ ] 提交成功返回承诺计划（含 orders + committedResources）
- [ ] 提交确定性：同草稿+同资源+同上下文 → 同提交结果与哈希
- [ ] 提交后会话已承诺计划可读（`CommittedPlan` 存于会话）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `PlanCommitService.Submit`）：*

- `CampaignSessionService` 新增 `SubmitPlan(session)` 返回 `SubmitPlanResult`（或 `CampaignCommandResult` 包装）。
- 流程：`Submit(session.Draft, session.Pool, session.ReachableRegions, session.AuthorizedOrders, session.PrepConfig)` → 成功则 `session` 写回 `CommittedPlan` + 更新资源池为 `ResultingPool`。
- 成功后是否清空草稿？MVP 保留草稿（可重提交覆盖）；承诺计划存 `session.CommittedPlan`。
- 资源扣减由 `ResourcePool.Deduct` 既有逻辑，不在会话重写。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：准备态接入 + 草稿编辑（本 story 依赖其完成）
- Story 003：冲突 DAG 拒绝（本 story 只测合法路径）
- Story 004：准备态存读档
- 战斗结算（GDD_010 / M06）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 合法计划原子提交生成 CommittedPlan
  - Given: 资源充足、可达、已授权的合法草稿
  - When: `Service.SubmitPlan(s)`
  - Then: `Committed == true`；`session.CommittedPlan` 含草稿命令
  - Edge cases: 单命令草稿 / 多命令草稿均成立

- **AC-2**: 合法提交原子扣减资源
  - Given: 资源池 {粮:100}，草稿命令需 {粮:40}
  - When: `SubmitPlan(s)`
  - Then: 提交后资源池 {粮:60}（扣减锁定）
  - Edge cases: 恰好够（需=可用）→ 提交成功，余 0

- **AC-3**: 提交返回承诺计划内容
  - Given: 合法草稿
  - When: `SubmitPlan(s)`
  - Then: `CommittedPlan.Orders` == 草稿命令；`CommittedResources` == 总需求
  - Edge cases: N/A

- **AC-4**: 提交确定性
  - Given: 两 session 同开局准备态 + 同草稿
  - When: 各 `SubmitPlan`
  - Then: 两者 `ComputeHash()` 相同
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPlanCommitTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignPlanCommitTests.cs` — 5/5 通过（703/703 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（准备态 + 草稿编辑是提交的基底）
- Unlocks: Story 003（冲突拒绝对比合法提交）、Story 004（承诺态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 提交成功保留草稿（可重提交覆盖）；承诺存 session.CommittedPlan，资源池更新为扣减后池。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPlanCommitTests.cs` — 5 tests
**新生产代码**: CampaignSessionService.SubmitPlan（经 PlanCommitService.Submit；成功才写回承诺+扣减池）
**Code Review**: 内联 — APPROVED（Lean）
