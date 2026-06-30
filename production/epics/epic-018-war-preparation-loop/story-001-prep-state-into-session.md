# Story 001: 准备态接入会话 + 计划草稿编辑

> **Epic**: War Preparation / Commitment Loop（战役准备循环 / M05）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-009-battle-preparation.md`
**Requirement**: `TR-prep-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 装配层只编排——会话持资源池 + 准备配置 + 可达区域 + 授权命令 + 当前计划草稿；草稿编辑只改草稿、**不改权威 state**（TR-prep-001）。复用既有 `PlanDraft`，不重写准备逻辑。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 状态变更经 Application 路径；草稿不改权威态
- Forbidden: 自动布阵（系统替玩家组计划）；硬编码准备配置
- Guardrail: 准备态确定性（纳入会话哈希）

---

## Acceptance Criteria

*来自 GDD `gdd-009-battle-preparation.md`，作用域限本 story：*

- [ ] `CampaignSession` 持有资源池（`ResourcePool`）+ 准备配置（`PreparationConfig`）+ 可达区域 + 授权命令 + 当前 `PlanDraft`
- [ ] 计划编辑命令（增/删 `PreparedOrder`）经会话路径修改草稿
- [ ] 草稿编辑**不改权威 state**：资源池/承诺计划不变（TR-prep-001）
- [ ] 准备态（资源池 + 草稿 + 已承诺计划）纳入 `session.ComputeHash()`
- [ ] 准备态可选（场景未启用准备时为 null，向后兼容现有测试）

---

## Implementation Notes

*来自 ADR-0009 实现指引：*

- 复用 `PlanDraft`/`ResourcePool`/`PreparationConfig`（epic-006 已实装）。
- 会话新增可选字段：`ResourcePool? _pool`、`PlanDraft _draft`、`PreparationConfig?`、可达区域集、授权命令集、`CommittedPlan? _committed`。
- 编辑命令 `AddPlanOrder(session, order)` / `RemovePlanOrder(session, orderId)` 委派 `PlanDraft.AddOrder/RemoveOrder`。
- 草稿编辑只改 `_draft`，绝不动 `_pool`/`_committed`（TR-prep-001）。
- 准备态进 `CampaignStartConfig` 可选参数（同 M03/M04 模式）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：合法计划原子提交
- Story 003：冲突 DAG 拒绝
- Story 004：准备态存读档
- 战斗结算（GDD_010 / M06）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 会话持有准备态
  - Given: `StartCampaign(config)`（config 含资源池 + 准备配置）
  - When: 读会话准备态
  - Then: `HasPreparation == true`；资源池/草稿可读
  - Edge cases: 缺准备配置但有资源池 → 开局拒绝（无部分初始化）

- **AC-2**: 编辑命令修改草稿
  - Given: 启用准备的 session、空草稿
  - When: `AddPlanOrder(s, order)`
  - Then: 草稿含该命令；`RemovePlanOrder(s, order.Id)` 后草稿不含
  - Edge cases: 删不存在的命令返回 false（不报错）

- **AC-3**: 草稿编辑不改权威态（TR-prep-001）
  - Given: 启用准备的 session，记录资源池
  - When: 增/删若干草稿命令
  - Then: 资源池不变、已承诺计划不变（草稿是计划意图非承诺）
  - Edge cases: N/A

- **AC-4**: 准备态纳入会话哈希
  - Given: 两 session 除资源池某项外相同
  - When: 各 `ComputeHash()`
  - Then: 哈希不同
  - Edge cases: 准备态相同 → 哈希相同

- **AC-5**: 无准备配置向后兼容
  - Given: 旧式 config（不传准备态）
  - When: `StartCampaign`
  - Then: `HasPreparation == false`；现有测试不受影响

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPreparationStateTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignPreparationStateTests.cs` — 8/8 通过（703/703 全绿）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-017（M04）Complete（已满足）；epic-006 准备 Domain 内核（已 Complete）
- Unlocks: Story 002（提交命令）、Story 004（准备存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing
**Deviations**: 准备态可选（nullable）向后兼容；Pool 设 public 只读（资源池非机密，可读，区别于情报真值）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPreparationStateTests.cs` — 8 tests
**新生产代码**: CampaignSession 持 Pool/Draft/PrepConfig/可达/授权/CommittedPlan + AppendPreparation 哈希 + ApplyCommittedPlan；CampaignStartConfig 加准备参数；CampaignSessionService.AddPlanOrder/RemovePlanOrder；CampaignErrorCode +PreparationDisabled
**Code Review**: 内联 — APPROVED（Lean）
