# Story 001: 战果分支后果写回会话（四分支变更集 → 城市态原子写回）

> **Epic**: Consequence / Recovery Loop（后果与恢复循环 / M07）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`（§后果）
**Requirement**: `TR-outcome-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 装配层只编排——后果命令从会话态构造 `OutcomeWorld`，经既有 `FailureContinuationService` 生成变更集 + 写回，再映射回会话城市态。复用 Domain 后果逻辑，不重写公式。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 后果经 Command 路径复用 FailureContinuationService；确定性
- Forbidden: 会话内重写后果公式；写回过宽（reputation/relationship 不入会话独立态）
- Guardrail: 同 (branch, context, config, 会话态) → 同写回结果（确定性）

---

## Acceptance Criteria

*来自 GDD `gdd-010` §后果，作用域限本 story：*

- [ ] `ResolveBattleOutcome(session, branch, context, config)` 从会话城市态构造 `OutcomeWorld` → 经 `FailureContinuationService` 写回
- [ ] 败/失城/撤退分支：城市态按变更集受损（民心/治安/工事/兵力损耗，夹至下限不出负）写回会话
- [ ] 胜利分支：城市态按胜利变更集写回（损耗较小或无）
- [ ] 写回确定性：同 (branch, context, config, 会话态) → 同写回后城市态 + 同哈希
- [ ] 写回结果（`OutcomeContinuation`）可读：含分支、变更集、写回结果、续局选项

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `FailureContinuationService.Resolve`）：*

- `CampaignSessionService` 新增 `ResolveBattleOutcome(session, OutcomeBranch, OutcomeContext, OutcomeConsequenceConfig)` 返回 `OutcomeContinuation`。
- 流程：`OutcomeWorld.Empty.WithCity(session.CityEconomy)` → `FailureContinuationService.Resolve(world, branch, context, config)` → 映射写回后城市态回会话（`session.SetCityEconomy`）。
- reputation/relationship/vitality 在 OutcomeWorld 内计算（结果可查），但**不**写回会话独立态（裁断，见 EPIC）。
- 损耗夹至下限（不出负）由 `OutcomeConsequenceConfig` + Domain 既有逻辑保证。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：四分支续局选项保证
- Story 003：原子回滚（校验失败整批回滚）
- Story 004：后果续局态存读档
- 会话级 reputation/relationship 独立态（裁断留后续）

---

## QA Test Cases

- **AC-1**: 败北分支写回城市损耗
  - Given: 启用城市治理且开战的 session（城市态已知）
  - When: `ResolveBattleOutcome(s, OutcomeBranch.Defeat, ctx, cfg)`
  - Then: 城市态民心/治安等按变更集下降（夹至 ≥0）；返回 `OutcomeContinuation`
  - Edge cases: 损耗超当前值 → 夹至下限不出负

- **AC-2**: 失城分支写回
  - Given: 同上
  - When: `ResolveBattleOutcome(s, OutcomeBranch.CityLost, ctx, cfg)`
  - Then: 城市态按失城变更集写回；`OutcomeContinuation.Branch == CityLost`
  - Edge cases: N/A

- **AC-3**: 胜利分支写回
  - Given: 同上
  - When: `ResolveBattleOutcome(s, OutcomeBranch.Victory, ctx, cfg)`
  - Then: 城市态按胜利变更集写回（损耗较小/无）
  - Edge cases: N/A

- **AC-4**: 写回确定性
  - Given: 两 session 同城市态
  - When: 各 `ResolveBattleOutcome(同 branch/ctx/cfg)`
  - Then: 两者写回后 `ComputeHash()` 相同
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOutcomeWritebackTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignOutcomeWritebackTests.cs` — 5/5 通过（743/743 全绿）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-019（M06）Complete（已满足）；epic-008 后果 Domain 内核（已 Complete）
- Unlocks: Story 002（续局选项）、Story 004（后果态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing
**Deviations**: reputation/relationship/vitality 在 OutcomeWorld 内计算暴露，不写回会话独立态（裁断）；写回经 OutcomeWorld.WithCity(会话城市态) → 映射回会话。
**Test Evidence**: `tests/unit/.../Session/CampaignOutcomeWritebackTests.cs` — 5 tests
**新生产代码**: CampaignSession outcome 字段 + 哈希；CampaignSessionService.ResolveBattleOutcome（经 FailureContinuationService）
**Code Review**: 内联 — APPROVED（Lean）
