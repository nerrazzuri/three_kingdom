# Story 002: 四分支续局选项（胜败撤退失城都有后续，败局必非空）

> **Epic**: Consequence / Recovery Loop（后果与恢复循环 / M07）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`（§失败延续）
**Requirement**: `TR-outcome-002`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0002: 分层（失败必须产生可继续状态）（primary）；ADR-0009: 装配（secondary）
**ADR Decision Summary**: 胜/撤退/失城/败北四分支均生成续局选项；任一败局至少一条合法可继续命令——失败不切死局（强制设计锁）。复用 `FailureContinuationService.HasPlayableContinuation` 的可断言保证。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature 层)**:
- Required: 失败必须产生可继续状态（强制设计锁）
- Forbidden: 败局切死局（零续局选项）
- Guardrail: 续局选项确定性（同分支同上下文 → 同选项集）

---

## Acceptance Criteria

*来自 GDD `gdd-010` §失败延续 + module-plan §M07 测试证据，作用域限本 story：*

- [ ] 胜利分支：续局选项含追击/巩固类（Pursue/Consolidate）
- [ ] 撤退分支：续局选项含撤退类（Retreat）+ 通用兜底（Regroup/Accountability）
- [ ] 失城分支：续局选项含求和（SueForPeace）+ 通用兜底
- [ ] 败北分支：续局选项含通用兜底（Regroup/Accountability）
- [ ] **任一败局（Defeat/CityLost/Retreat）至少一条合法可继续命令**（失败不切死局，TR-outcome-002）；`OutcomeContinuation.IsPlayable == true`

---

## Implementation Notes

*来自 ADR-0002 实现指引（参考 `FailureContinuationService.LegalContinuations` / `HasPlayableContinuation`）：*

- `ResolveBattleOutcome`（story-001）返回的 `OutcomeContinuation.Options` 即续局选项；本 story 验证四分支选项保证。
- 续局选项由 `FailureContinuationService` 既有逻辑生成（按分支 + 上下文）；装配层不重写。
- `OutcomeContinuation` 构造已断言败局选项非空（Domain 保证）；本 story 验证装配后仍成立。
- 会话可暴露最近一次 `OutcomeContinuation`（含 Options）供 UI「继续」契约读取。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：后果写回（本 story 依赖其 OutcomeContinuation 产出）
- Story 003：原子回滚
- Story 004：后果态存读档
- 续局选项的实际执行（选项落地为下一步行动属后续 epic）

---

## QA Test Cases

- **AC-1**: 胜利分支续局选项
  - Given: 开战会话
  - When: `ResolveBattleOutcome(s, Victory, ctx, cfg)`
  - Then: `Options` 含 Pursue 或 Consolidate
  - Edge cases: N/A

- **AC-2**: 失城分支续局含求和 + 兜底
  - Given: 同上
  - When: `ResolveBattleOutcome(s, CityLost, ctx, cfg)`
  - Then: `Options` 含 SueForPeace；含 Regroup/Accountability 兜底
  - Edge cases: N/A

- **AC-3**: 败北分支续局兜底
  - Given: 同上
  - When: `ResolveBattleOutcome(s, Defeat, ctx, cfg)`
  - Then: `Options` 含 Regroup 与 Accountability
  - Edge cases: N/A

- **AC-4**: 任一败局必非空可继续（失败不切死局，TR-outcome-002 核心）
  - Given: 三败局分支 {Defeat, CityLost, Retreat}
  - When: 各 `ResolveBattleOutcome`
  - Then: 各 `OutcomeContinuation.Options.Count > 0` 且 `IsPlayable == true`
  - Edge cases: 主将被俘极端败局（CommanderCaptured）仍有续局

- **AC-5**: 续局选项确定性
  - Given: 两 session 同分支同上下文
  - When: 各 `ResolveBattleOutcome`
  - Then: 两者 Options 的 Kind 集合相同
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignContinuationOptionsTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignContinuationOptionsTests.cs` — 6/6 通过（743/743 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（ResolveBattleOutcome 产出 OutcomeContinuation）
- Unlocks: Story 004（后果续局态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing（+主将被俘极端败局副验证）
**Deviations**: 无（复用 FailureContinuationService 续局保证；会话存最近续局供 UI「继续」契约）。
**Test Evidence**: `tests/unit/.../Session/CampaignContinuationOptionsTests.cs` — 6 tests
**新生产代码**: CampaignSession.SetLastOutcome + LastOutcomeBranch/LastContinuationOptions
**Code Review**: 内联 — APPROVED（Lean）
