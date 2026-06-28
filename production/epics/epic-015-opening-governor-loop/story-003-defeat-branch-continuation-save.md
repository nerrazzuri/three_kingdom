# Story 003: 败支后果——在野延续存读档 + 部曲保留验证

> **Epic**: Opening Governor Loop（太守开局循环 / M02）
> **Status**: Complete
> **Layer**: Assembly（Integration 装配层）
> **Type**: Integration
> **Estimate**: S（2–3 h，纯测试，无新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-29

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirements**: `TR-session-004`、`TR-career-003`、`TR-career-004`、`TR-career-005`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0008: 城池控制权所有权契约（secondary）；ADR-0005: 存档版本与迁移（secondary）
**ADR Decision Summary**:
- ADR-0009：败支生成合法可继续状态（六种续局之一）；`RetinueState` 经 `ResolveFallen` 保留。
- ADR-0008：失城经 GDD_004 控制权变更路径，session 不独立写 `city.owner`。
- ADR-0005：存档必须在 defeat 后 Advance 的状态下仍能 round-trip 一致（补现有 SaveTests 缺口）。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# NUnit 测试，不调用 UnityEngine。

**Control Manifest Rules (Assembly 层)**:
- Required: 失败产生合法可继续状态（control-manifest §失败必须产生可继续状态）
- Forbidden: 测试不绕过 `CampaignSessionService` 直接写 career 或城权字段
- Guardrail: `ResolveSiege` 是唯一写入城控路径——测试须验证城权经 GDD_004 路径变更

---

## Acceptance Criteria

*来自 GDD `gdd-014-campaign-and-career.md`，作用域限本 story：*

- [ ] `ResolveSiege(Fallen)` → `Advance(1)` 后，`CaptureSnapshot → Restore` round-trip 哈希一致（补现有 SaveTests：现有测试只有 Advance-before-siege 的 round-trip，缺 defeat-then-advance 存读档）
- [ ] 败支存读档后 `loaded.Career.Career.IsUnaffiliated == true`（在野态持久化）
- [ ] 败支存读档后 `loaded.Career.Career.Faction == null`（无归属持久化）
- [ ] 败支存读档后部曲成员保留：`loaded.Career.Retinue.IsMember(Aide) == true`（TR-career-003 / RetinueState round-trip）
- [ ] 败支城池归属经 GDD_004 路径变更（`loaded.World.OwnershipOf(city).Owner == enemyFaction`，ADR-0008 合规）

---

## Implementation Notes

*来自 ADR-0009 / ADR-0005 实现指引：*

- `Service.ResolveSiege(session, Fallen, ...)` 内部调用 `GovernorOutcomeService.ResolveFallen`（保留 retinue）+ `ConsequenceTransaction.StageControlChange`（经 GDD_004 写城权）。
- **本 story 关键 gap**：`CampaignSessionSaveTests` 中的 `test_roundtrip_after_advance_and_siege_loss` 在 Advance **之前**存读档；本 story 补充 defeat → Advance → 存读档序列。
- `Service.CaptureSnapshot` / `Service.Restore` 已实装，直接复用。
- **本 story 零新生产代码**：只新增测试文件 `CampaignOpeningDefeatBranchTests.cs`。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：败支 Advance 可用性（先验证能推进，本 story 再验证推进后能存读档）
- Story 002：胜支存读档
- Story 004：两支确定性哈希重放

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 败支 Advance 后存读档哈希一致（核心 gap 补充）
  - Given: `NewSession()` → `ResolveSiege(Fallen, Win(), Fall())` → `Service.Advance(s, 1)` → `beforeHash = s.ComputeHash()`
  - When: `loaded = Service.Restore(Service.CaptureSnapshot(s), Fp)`
  - Then: `loaded.ComputeHash() == beforeHash`
  - Edge cases: 与 Story 001 AC-4 基线对比：时间已推进 1 segment

- **AC-2**: 败支存读档后在野态持久化
  - Given: `ResolveSiege(Fallen)` → `CaptureSnapshot` → `Restore`
  - When: 查询 `loaded.Career.Career.IsUnaffiliated` 与 `loaded.Career.Career.Faction`
  - Then: `IsUnaffiliated == true`；`Faction == null`
  - Edge cases: N/A

- **AC-3**: 败支存读档后部曲保留（TR-career-003 RetinueState round-trip）
  - Given: `Config()` 含 `RetinueMember(Aide, Frac(6, 10))`；`ResolveSiege(Fallen)` → `CaptureSnapshot` → `Restore`
  - When: `loaded.Career.Retinue.IsMember(Aide)`
  - Then: `true`
  - Edge cases: 部曲数量与失陷前相同

- **AC-4**: 败支城池归属经 GDD_004 路径（ADR-0008 合规验证）
  - Given: `ResolveSiege(Fallen, Win(), new SiegeContext(Fanshui, Enemy, new Garrison(500)))` → `CaptureSnapshot` → `Restore`
  - When: `loaded.World.OwnershipOf(Fanshui)`
  - Then: `Owner == Enemy`（经控制权变更路径，非 session 直写）
  - Edge cases: 多次存读档后归属不改变

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningDefeatBranchTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignOpeningDefeatBranchTests.cs` — 5/5 通过（618/618 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（确认败支 Advance 路径通是本 story 的前置基线）
- Unlocks: Story 004（败支哈希重放基线）

---

## Completion Notes
**Completed**: 2026-06-29
**Criteria**: 4/4 passing（+1 二次 round-trip 稳定性副验证）
**Deviations**: None
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningDefeatBranchTests.cs` — 5 tests, 5/5 pass（618/618 全绿）
**Code Review**: Complete — APPROVED（内联 review 2026-06-29，Lean 模式）
