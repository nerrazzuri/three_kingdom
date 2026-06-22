# Story 002: 可玩失败延续（撤退/失城/问责分支）

> **Epic**: 后果与可玩失败
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md（§后果/失败延续）
**Requirement**: 强制设计锁「失败必须产生可继续状态」（gdd-010 §后果验收）

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: 失败为分支而非终局；至少撤退/失城/问责一条可继续路径，经 Command 路径产生。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 失败必须产生可继续状态（强制设计锁）
- Forbidden: 失败切到空白/死局
- Guardrail: 败局后至少存在一条合法可继续命令

---

## Acceptance Criteria

- [x] 胜/败/撤退/失城均为分支结算（非单一胜负开关）— `OutcomeBranch` 四分支，`FailureContinuationService.Resolve` 各生成不同变更集
- [x] 战败延续至少提供撤退 / 失城 / 问责一条可继续路径 — 通用兜底 Regroup+Accountability，分支特有 Retreat/SueForPeace
- [x] 失败后世界状态完整可继续（非空白界面，对齐 art-bible §2.5「尚有余地」）— 写回成功且 `IsPlayable`，损失上限夹取不写负
- [x] 败局后存在至少一条合法可继续命令（自动化断言）— `OutcomeContinuation` 构造断言 Options 非空 + `HasPlayableContinuation`

---

## Implementation Notes

*Derived from ADR-0002 + 强制设计锁:*
- 失败结算走与胜利相同的后果写回（Story 001），只是变更集不同。
- 败局后构造合法命令集，断言非空（撤退/求和/问责等）。
- 与 hud.md §2.5 战果/延续态 + main-menu/pause 的「继续」契约一致。

---

## Out of Scope

- 后果写回机制（Story 001）
- 失败叙事文本（writer / 后续）

---

## QA Test Cases

- **AC-1**: 失败可继续
  - Given: 一个战败结算
  - When: 结算完成
  - Then: 世界状态完整；存在 ≥1 条合法可继续命令
  - Edge cases: 失城 + 主将被俘的极端败局仍可继续（问责/重整）
- **AC-2**: 分支正确
  - Given: 撤退 vs 失城两种败因
  - When: 结算
  - Then: 写回不同变更集，均产生可继续态
  - Edge cases: 撤退途中再遭袭

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Outcome/PlayableFailureTests.cs` — 7 测全通过（归一到唯一可编译测试工程，ADVISORY 偏差）
**Status**: [x] Passed — 309/309 全绿，`-warnaserror` 0 warning

---

## Dependencies

- Depends on: Story 001（后果写回）
- Unlocks: 完整核心循环闭环（胜败均可继续）
