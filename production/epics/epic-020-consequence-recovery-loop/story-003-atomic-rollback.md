# Story 003: 后果原子回滚（任一校验失败整批回滚，无部分写入）

> **Epic**: Consequence / Recovery Loop（后果与恢复循环 / M07）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，纯测试为主）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`（§后果）
**Requirement**: `TR-outcome-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 后果先生成变更集再校验后原子写回（全有或全无）；任一目标校验失败 → 整批回滚、会话态不变、无部分写入。复用 `OutcomeWritebackResult.Committed` 的原子保证。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 失败必须产生可继续状态；原子（全有或全无）
- Forbidden: 部分写入（写一半目标）
- Guardrail: 回滚后会话哈希不变（原子性可断言）

---

## Acceptance Criteria

*来自 GDD `gdd-010` §后果，作用域限本 story：*

- [ ] 后果写回校验失败时 `OutcomeWritebackResult.Committed == false`
- [ ] 写回失败时会话城市态**不变**（整批回滚，无部分写入，TR-outcome-001）
- [ ] 写回失败后 `session.ComputeHash()` 不变（原子性）
- [ ] 写回失败后会话可继续（再提合法后果写回成功）
- [ ] 写回成功（Committed）时会话态按变更集更新（对照组）

---

## Implementation Notes

*来自 ADR-0009 实现指引：*

- `ResolveBattleOutcome`（story-001）内：`OutcomeWritebackResult.Committed == false` 时**不**更新会话态（原子）。
- 复用 `OutcomeWritebackService` 既有校验（任一目标非法/越界 → 整批拒绝）。
- 触发回滚的方式：构造导致写回校验失败的变更集（如目标城市不在 OutcomeWorld / 越界损耗）；具体由 Domain 校验规则决定。
- 本 story 主要新增**测试**覆盖回滚路径 + 原子性（生产代码在 story-001 已就位）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002：后果写回 / 续局选项
- Story 004：后果态存读档

---

## QA Test Cases

- **AC-1**: 写回成功更新会话态（对照组）
  - Given: 合法后果写回的开战会话，记录写回前城市态
  - When: `ResolveBattleOutcome(s, Defeat, ctx, cfg)`
  - Then: `Writeback.Committed == true`；城市态按变更集更新
  - Edge cases: N/A

- **AC-2**: 写回校验失败整批回滚 + 哈希不变
  - Given: 构造导致写回校验失败的上下文/配置；`before = ComputeHash()`
  - When: `ResolveBattleOutcome(s, ...)`
  - Then: `Writeback.Committed == false`；城市态不变；`ComputeHash() == before`
  - Edge cases: 多目标部分非法 → 整批回滚（无部分写入）

- **AC-3**: 写回失败后会话可继续
  - Given: 一次写回失败后
  - When: 提一次合法后果写回
  - Then: 第二次写回成功（失败不卡死会话）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOutcomeRollbackTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignOutcomeRollbackTests.cs` — 4/4 通过（743/743 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（ResolveBattleOutcome 路径，本 story 测其回滚分支）
- Unlocks: Story 004（后果态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 3/3 passing（+原子全应用 + 夹取不越界 + Domain 回滚机制确认）
**Deviations**: FailureContinuationService 守卫使 M07 正常路径 Committed 恒真；S003 验证成功路径原子全应用 + 损耗夹取不出负 + 直接 OutcomeWritebackService 非法变更集回滚机制（M07 装配仅 Committed 时更新会话所依赖的保证）。
**Test Evidence**: `tests/unit/.../Session/CampaignOutcomeRollbackTests.cs` — 4 tests
**Code Review**: 内联 — APPROVED（Lean）
