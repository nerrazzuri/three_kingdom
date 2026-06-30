# Epic: Consequence / Recovery Loop（后果与恢复循环 / M07）

> **Layer**: Feature（含 Assembly 装配——后果/恢复 Domain 接入会话）
> **GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`（§后果/失败延续）+ gdd-014（生涯）
> **Architecture Module**: M07 Consequence / Recovery Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M07）
> **Governing ADR**: ADR-0004（确定性）· ADR-0009（CampaignSession 装配）· ADR-0008（城池控制权）· ADR-0005（存档）· ADR-0002（分层/后果写回）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-outcome-writeback-into-session.md) | 战果分支后果写回会话（四分支变更集 → 城市态原子写回） | Integration | ✅ Complete | ADR-0009/0004 |
| [002](story-002-continuation-options.md) | 四分支续局选项（胜败撤退失城都有后续，败局必非空） | Integration | ✅ Complete | ADR-0002/0009 |
| [003](story-003-atomic-rollback.md) | 后果原子回滚（任一校验失败整批回滚，无部分写入） | Integration | ✅ Complete | ADR-0009/0004 |
| [004](story-004-outcome-save-determinism.md) | 后果续局态存读档 + 确定性 | Integration | ✅ Complete | ADR-0005/0004 |

## Overview

把战果写回完整世界，并保证**胜败都打开后续选择**——战争改变人生和世界，失败不是删档，而是进入新局面。M00~M06 已就绪（脊梁→城市→情报军议→战役准备→兵法战斗）；后果/恢复 Domain 内核（`FailureContinuationService`/`OutcomeWritebackService`/`ConsequenceSet`/`ContinuationOption`，epic-008 完成）已实现四分支（胜/撤退/失城/败北）变更集 + 原子写回 + 可继续命令，但**尚未接入** `CampaignSession`。本 epic 把它接入可玩会话：从 M06 战斗结果定战果分支，构造后果世界，经 `FailureContinuationService` 生成变更集 → **原子写回会话**（城市态等）→ 给出**四分支各自的续局选项**（败局至少一条合法可继续命令）；任一目标校验失败整批回滚、无部分写入；后果续局态存读档一致 + 确定性。这是把一场战斗**收口为对人生与世界的持久改变**，并确保游戏永不切死局。

## Boundary（与 M00/M02/M06 的边界）

- **已交付**：M00 `ConsequenceTransaction`（career/control/faction 原子写回）；M02 守城开局特化后果（`ResolveSiege`）；M06 战斗结果（`BattleResolution`）；后果/恢复 Domain 内核（epic-008，已测但仅接旧竖切）。
- **M07（本 epic）新增**（**含新生产代码**，同 M03~M06）：
  1. `CampaignSession` 后果命令 `ResolveBattleOutcome`：按战果分支（Victory/Retreat/CityLost/Defeat）经 `FailureContinuationService` 生成变更集 → 原子写回会话城市态。
  2. 四分支各自续局选项（`ContinuationOption`）存会话；败局至少一条合法可继续命令（失败不切死局）。
  3. 任一目标校验失败整批回滚、会话态不变、无部分写入。
  4. 后果续局态存读档一致 + 确定性。
- 复用既有 Domain 规则（epic-008），**不新增后果/续局公式**。

## 关键裁断与护栏（风险）

> module-plan §M07 风险：**写回过宽导致事务难测；写回过窄导致战斗不影响世界**。

- **写回范围裁断**：M07 写回会话**已持有的态**（城市经济态，M03）+ 续局选项；reputation/relationship/character-vitality 在 `OutcomeWorld` 内计算并在 `OutcomeContinuation` 结果暴露（可查、确定性，epic-008 已测），但**会话不新增这些独立持久字段**（会话当前无 reputation/relationship 独立态）——避免"写回过宽难测"，亦非"过窄"（城市后果 + 四分支续局是最小有意义集）。会话级 reputation/relationship 持久化留后续 epic 若需。
- **失败必须产生可继续状态**（强制设计锁）：四分支均生成后续，败局至少一条合法可继续命令（`OutcomeContinuation` 构造断言非空；`FailureContinuationService.HasPlayableContinuation`）。
- **城权经 GDD_004**（ADR-0008）：失城分支的城权变更仍经控制权唯一权威（复用 M00 `ConsequenceTransaction.StageControlChange`），后果层不独立写归属。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0004: 确定性 | 同 (world, branch, context, config) → 同变更集 → 同写回哈希 + 同命令集 | LOW |
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；后果经会话命令路径复用 FailureContinuationService，不重写后果公式 | LOW |
| ADR-0008: 城池控制权所有权契约 | 失城分支城权变更经 GDD_004 唯一权威；后果层只读/经它发起 | LOW |
| ADR-0005: 存档版本与迁移 | 后果续局态显式序列化 round-trip 一致 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-outcome-001 | 战果后果先生成变更集再校验后原子写回会话（全有或全无）；任一目标校验失败整批回滚、无部分写入 | ADR-0004/0009 ✅ |
| TR-outcome-002 | 胜/败/撤退/失城四分支均生成后续：任一败局至少一条合法可继续命令（失败不切死局） | ADR-0002/0009 ✅ |

> 注：TR-outcome-001/002 于 2026-06-30 补登（M07 后果写回接入会话；epic-008 曾注"后果结算无独立 slug，若需独立追踪运行 /architecture-review 追加 TR-outcome-NNN"，此即补登）。原子性/守恒亦受既有 TR-battle-003/city-001 约束。无 untraced requirement。

## Scope

### In Scope
- `CampaignSession` 后果命令 `ResolveBattleOutcome(session, branch, context, config)`：四分支经 `FailureContinuationService` 生成变更集 → 原子写回会话城市态。
- 四分支各自续局选项存会话（Victory→追击/巩固；Defeat/CityLost/Retreat→重整/问责/撤退/求和等）；败局至少一条合法可继续命令。
- 任一目标校验失败整批回滚、会话态不变、无部分写入（TR-outcome-001）。
- 后果续局态（变更集结果 + 续局选项）存读档一致 + 同 (输入) 同哈希。

### Out of Scope
- 会话级 reputation/relationship/character-vitality 独立持久态（在 OutcomeWorld 内计算暴露，会话不新增字段；留后续）。
- 完整外交影响写回（M10+）；历史世界 delta 传播全量（epic-012 已有，M07 不重做）。
- 失城城权变更的新机制（复用 M00 ConsequenceTransaction + GDD_004）。
- 新 Unity scene / 新 UI / 新后果或续局公式。

## Definition of Done

This epic is complete when:
- `ResolveBattleOutcome` 四分支接入会话；变更集原子写回会话城市态。
- 胜/败/撤退/失城都有续局选项；任一败局至少一条合法可继续命令（失败不切死局，module-plan §M07 测试证据）。
- 任一目标校验失败整批回滚、无部分写入（TR-outcome-001）。
- 后果续局态存读档一致 + 同输入同哈希。
- 城权变更经 GDD_004（ADR-0008）。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M06 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-020-consequence-recovery-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
