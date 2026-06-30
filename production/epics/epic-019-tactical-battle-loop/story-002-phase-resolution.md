# Story 002: 阶段命令解析（稳定管线 + 原子回滚 + 确定性）

> **Epic**: Tactical Battle Loop（兵法沙盒战役循环 / M06）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`
**Requirement**: `TR-battle-001`、`TR-battle-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0004: 确定性战斗模拟（primary）；ADR-0009: CampaignSession 装配边界（secondary）
**ADR Decision Summary**: 阶段命令经会话路径调既有 `BattleResolver.ResolvePhase`——稳定管线（验证→移动→侦测→交战→损耗），异常原子回滚（返回原快照）；同快照+配置+种子+命令流 → 同事件与 hash。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 战斗结果可确定性复现；阶段稳定管线
- Forbidden: 会话内重写战斗管线；非确定性随机源
- Guardrail: 解析异常回滚整个原子阶段（无部分写入）

---

## Acceptance Criteria

*来自 GDD `gdd-010-battle-tactics-sandbox.md`，作用域限本 story：*

- [ ] 阶段命令经会话路径调 `BattleResolver.ResolvePhase`：稳定管线解析
- [ ] 解析成功更新会话战斗快照 + 产生 `BattleEvent` 列表
- [ ] 解析确定性：同快照+配置+种子+命令流 → 同 hash + 同事件（TR-battle-001）
- [ ] 解析异常原子回滚：返回原快照、会话战斗态不变（TR-battle-003）
- [ ] 多阶段连续解析推进战斗态

---

## Implementation Notes

*来自 ADR-0004 实现指引（参考 `BattleResolver.ResolvePhase`）：*

- `CampaignSessionService` 新增 `ResolveBattlePhase(session, orders)` 返回 `BattleResolution`。
- 流程：`ResolvePhase(session.Battle, orders, session.BattleSeed, session.BattleConfig)` → `Committed` 则更新会话战斗快照。
- 战斗种子确定性推进（每阶段用确定种子，或派生）；同种子同结果。
- `BattleResolution.Rollback`（异常）时不更新会话态（原子）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：战斗态接入 + 开战（本 story 依赖其完成）
- Story 003：兵法事后识别
- Story 004：战斗态存读档

---

## QA Test Cases

- **AC-1**: 阶段命令解析更新战斗态
  - Given: 已开战的 session
  - When: `ResolveBattlePhase(s, orders)`（含 Move/Engage 命令）
  - Then: `Committed == true`；战斗快照更新；产生事件
  - Edge cases: 空命令列表 → 解析成功（无事件或维持）

- **AC-2**: 解析确定性
  - Given: 两 session 同开战态 + 同命令
  - When: 各 `ResolveBattlePhase`
  - Then: 两者 `BattleResolution.Hash` 相同 + 会话 `ComputeHash()` 相同
  - Edge cases: N/A

- **AC-3**: 异常原子回滚
  - Given: 触发解析异常的非法命令（如非法 actor）
  - When: `ResolveBattlePhase(s, badOrders)`
  - Then: 返回 Rollback；会话战斗态不变（哈希不变）
  - Edge cases: 部分合法部分非法 → 整阶段回滚

- **AC-4**: 多阶段连续推进
  - Given: 已开战
  - When: 连续两阶段 `ResolveBattlePhase`
  - Then: 战斗态累积推进；两次直推 == 同操作另一 session（确定性）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattlePhaseTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignBattlePhaseTests.cs` — 4/4 通过（723/723 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（战斗态接入是阶段解析的基底）
- Unlocks: Story 003（兵法识别读阶段事件）、Story 004（战斗态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 无（复用 BattleResolver.ResolvePhase；成功才更新会话态，Rollback 保持原态）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattlePhaseTests.cs` — 4 tests
**新生产代码**: CampaignSessionService.ResolveBattlePhase（经 BattleResolver.ResolvePhase；原子回滚）
**Code Review**: 内联 — APPROVED（Lean）
