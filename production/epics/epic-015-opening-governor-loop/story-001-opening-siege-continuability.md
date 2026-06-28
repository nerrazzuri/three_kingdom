# Story 001: 开局围城续局可用性——胜败两支 Advance 均可执行

> **Epic**: Opening Governor Loop（太守开局循环 / M02）
> **Status**: Ready
> **Layer**: Assembly（Integration 装配层）
> **Type**: Integration
> **Estimate**: S（1–2 h，纯测试，无新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: —

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-session-004`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性战斗模拟（secondary）
**ADR Decision Summary**: 装配层（CampaignSessionService）只编排不拥规则；任一结局后玩家须能继续（Advance），时间推进由 WorldState 驱动，在野态不阻止推进。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 测试为纯 C# Domain + NUnit，不调用 UnityEngine API，无引擎版本风险。

**Control Manifest Rules (Assembly 层)**:
- Required: 状态写入经 Command / ApplicationService 路径；败局产生合法可继续状态（control-manifest §强制设计锁）
- Forbidden: 测试中不绕过 CampaignSessionService 直接写 session 内部字段
- Guardrail: 测试确定性（无随机、无时间依赖断言）

---

## Acceptance Criteria

*来自 GDD `gdd-014-campaign-and-career.md`，作用域限本 story：*

- [ ] `ResolveSiege(SiegeOutcome.Defended, ...)` 执行后，调用 `Advance(1)` 不抛异常，返回 Applied=true
- [ ] `ResolveSiege(SiegeOutcome.Fallen, ...)` 执行后，调用 `Advance(1)` 不抛异常，返回 Applied=true（**败局可继续，非读档**——control-manifest §失败必须产生可继续状态）
- [ ] 胜支 `Advance(1)` 后 `session.CurrentTime` 等于开局时间向前推进 1 segment
- [ ] 败支 `Advance(1)` 后 `session.CurrentTime` 等于开局时间向前推进 1 segment（在野态下世界推进不受阻）
- [ ] 败支 Advance 后 `career.IsUnaffiliated` 仍为 true（推进不自动复职）

---

## Implementation Notes

*来自 ADR-0009 实现指引：*

- `CampaignSessionService.Advance(session, segments)` 已实装于 M00，直接复用。
- `ResolveSiege` 已实装两支（Defended / Fallen），均通过 `ConsequenceTransaction` 原子提交。
- **本 story 零新生产代码**：只新增测试文件 `CampaignOpeningContinuabilityTests.cs`，验证两支 Advance 可用性。
- `ConsequenceTransactionTests` 已验证 career/城权内容；本 story 聚焦「后果提交后继续推进」。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：胜支配置驱动生涯初值 + 胜支存读档
- Story 003：败支存读档 + 部曲保留验证
- Story 004：两支 E2E 确定性哈希

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: `ResolveSiege(Defended)` 后 Advance 可执行
  - Given: `NewSession()` 返回已初始化的 `CampaignSession`
  - When: `Service.ResolveSiege(s, SiegeOutcome.Defended, Win(), Fall())` 后调用 `Service.Advance(s, 1)`
  - Then: Advance 的 `CampaignCommandResult.Applied == true`；无任何异常
  - Edge cases: 验证 `s.CurrentTime` 不再等于 `initialTime`（时间已推进）

- **AC-2**: `ResolveSiege(Fallen)` 后 Advance 可执行（败局可继续核心验证）
  - Given: `NewSession()` 返回已初始化的 `CampaignSession`
  - When: `Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall())` 后调用 `Service.Advance(s, 1)`
  - Then: Advance 的 `CampaignCommandResult.Applied == true`；无任何异常
  - Edge cases: `career.IsUnaffiliated` 推进前后均为 true（Advance 不改变在野态）

- **AC-3**: 胜支推进后世界时间正确递增
  - Given: `ResolveSiege(Defended)` 已执行，记录 `beforeTime = s.CurrentTime`
  - When: `Service.Advance(s, 1)`
  - Then: `s.CurrentTime == beforeTime.Advance(1)`

- **AC-4**: 败支推进后世界时间正确递增（在野不阻推进）
  - Given: `ResolveSiege(Fallen)` 已执行，记录 `beforeTime = s.CurrentTime`；`career.IsUnaffiliated == true`
  - When: `Service.Advance(s, 1)`
  - Then: `s.CurrentTime == beforeTime.Advance(1)`；`career.IsUnaffiliated` 仍为 true

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningContinuabilityTests.cs` — 必须存在且全绿

**Status**: [ ] 尚未创建

---

## Dependencies

- Depends on: `production/epics/epic-013-campaign-session-assembly/` 全部 Complete（M00 脊梁，已满足）
- Unlocks: Story 002（胜支存读档）、Story 003（败支存读档）
