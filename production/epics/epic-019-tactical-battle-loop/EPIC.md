# Epic: Tactical Battle Loop（兵法沙盒战役循环 / M06）

> **Layer**: Feature（含 Assembly 装配——战斗/兵法 Domain 接入会话）
> **GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md` + `design/gdd/gdd-011-morale-fatigue.md`
> **Architecture Module**: M06 Tactical Battle Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M06）
> **Governing ADR**: ADR-0004（确定性战斗模拟）· ADR-0009（CampaignSession 装配）· ADR-0005（存档）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-battle-state-into-session.md) | 战斗态接入会话 + 从 CommittedPlan 开战 | Integration | ✅ Complete | ADR-0009/0004 |
| [002](story-002-phase-resolution.md) | 阶段命令解析（稳定管线 + 原子回滚 + 确定性） | Integration | ✅ Complete | ADR-0004/0009 |
| [003](story-003-tactic-recognition-feint-ambush.md) | 兵法事后识别（FeintAmbush 机动招式 + 涌现无按钮）— CD 硬退出门 | Integration | ✅ Complete | ADR-0004/0009 |
| [004](story-004-battle-save-determinism.md) | 战斗态存读档 + 确定性 | Integration | ✅ Complete | ADR-0005/0004 |

## Overview

解析战区中的命令、条件链与战役后果——玩家通过地形、时间、补给、人心、天气、纪律等杠杆**创造胜机**，兵法是多条件全部成立时的**涌现结果**，系统只在事件后打复盘标签，绝无同名无条件执行按钮。M00~M05 已就绪（脊梁→城市→情报军议→战役准备）；战斗/兵法 Domain 内核（`BattleResolver`/`TacticRecognizer`/`BattleSnapshot`/`RecognizedTactic`，epic-007 完成）已实现确定性阶段管线与兵法事后识别，但**尚未接入** `CampaignSession`。本 epic 把它接入可玩会话：会话从 M05 的 `CommittedPlan`（+ M03 城市派生战役条件）构造开战快照，阶段命令经 `BattleResolver.ResolvePhase` 确定性解析（原子回滚），兵法（含**假退伏击 FeintAmbush** 机动招式）经 `TacticRecognizer` 事后打标，同快照+配置+种子+命令流产生同一状态哈希。这是把前五个循环的产出**收口为一场真正可打、可复现的战役**。

## Boundary（与 M05/M08 的边界）

- **已交付**：M05 战役准备（`CommittedPlan` 作战役初始条件）；M03 城市派生战役条件（`WarConditionProjection`）；战斗/兵法 Domain 内核（epic-007，已测但仅接旧竖切）。
- **M06（本 epic）新增**（**含新生产代码**，同 M03/M04/M05）：
  1. `CampaignSession` 持有战斗态（`BattleSnapshot` + `BattleConfig` + 战斗种子）。
  2. 开战命令：从 `CommittedPlan`（+ 战役条件）构造初始 `BattleSnapshot`。
  3. 阶段命令解析经 `BattleResolver.ResolvePhase`（稳定管线，原子回滚）。
  4. 兵法识别经 `TacticRecognizer` 事后打标（含 FeintAmbush 机动招式；多条件涌现，无无条件按钮）。
  5. 战斗确定性（同快照+种子+命令流→同 hash）+ 战斗态存读档。
- 复用既有 Domain 规则（epic-007/008），**不新增战斗/兵法公式**。

## 关键裁断与护栏（风险）

> module-plan §M06 风险：**无敌方 AI 时战斗只是脚本靶子；有 AI 后必须防全知与不可复现**。

- **依赖裁断（敌方 AI）**：M06 module-plan 列依赖 M08（敌方 AI），但敌方 AI Domain **尚未实装**（仅 GDD_016 + ADR-0006 设计，无 Domain 代码/epic）。module-plan 注 M08「可与 M06 并行」。本 epic **裁断**：M06 装配战斗循环本身（确定性结算 + 兵法标签 + 机动招式），敌方命令用**确定性预设命令**（场景/CommittedPlan 提供，非智能决策）；**智能敌方 AI 决策（EnemyAiDecision）留 M08 / epic-021**。M06 测试证据（同种子同 hash + 兵法事后标签）不要求智能 AI，故此裁断不阻塞 M06。
- **CD 硬退出门**（强制设计锁）：本 epic 宣称"兵法沙盒 MVP 完成"**必接 ≥1 机动招式**——由 `TacticTag.FeintAmbush`（假退伏击：受控撤退保形 + 敌追击 + 伏兵突然性）接入满足（story-003）。非薄皮战斗沙盒。
- **兵法只作事后标签**（强制设计锁）：兵法是多条件全部成立的涌现结果，系统只在事件后打复盘标签，绝无同名无条件执行按钮（TR-battle-002）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0004: 确定性战斗模拟 | 整数/定点 + 注入随机流 + 状态哈希；同快照+配置+种子+命令流 → 同事件与 hash | LOW |
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；战斗经会话命令路径复用 BattleResolver/TacticRecognizer，不在会话重写战斗公式 | LOW |
| ADR-0005: 存档版本与迁移 | 战斗态显式序列化 round-trip 一致 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-battle-001 | 确定性 Domain 模拟：相同初始快照+配置指纹+随机种子+有序命令流产生相同事件与状态哈希 | ADR-0004 ✅ |
| TR-battle-002 | 兵法为多条件全部成立时的涌现结果，系统只在事件后打复盘标签，绝无同名无条件执行按钮 | ADR-0004 ✅ |
| TR-battle-003 | 阶段按稳定管线解析（验证→移动→侦测→交战→损耗→士气→触发→发布）；解析异常回滚整个原子阶段 | ADR-0004 ✅ |

> 注：M06 为装配 epic，复用 epic-007 的 TR-battle-*（像 epic-015/017/018 复用既有 TR），无新 TR。无 untraced requirement。敌方智能 AI（gdd-016）属 M08/epic-021，本 epic 不含。

## Scope

### In Scope
- `CampaignSession` 持有战斗态（`BattleSnapshot` + `BattleConfig` + 战斗种子）。
- 开战命令：从 `CommittedPlan`（M05）构造初始 `BattleSnapshot`（玩家 + 确定性预设敌方单位）。
- 阶段命令解析经 `BattleResolver.ResolvePhase`：稳定管线（验证→移动→侦测→交战→损耗），原子回滚（异常返回原快照）。
- 兵法识别经 `TacticRecognizer` 事后打标：含 **FeintAmbush 假退伏击**（CD 硬退出门）；多条件涌现，无无条件按钮。
- 战斗确定性：同快照+配置+种子+命令流 → 同 hash + 同事件。
- 战斗态存读档一致。

### Out of Scope
- 智能敌方 AI 决策（EnemyAiDecision）→ M08 / epic-021；M06 用确定性预设敌方命令。
- 后果写回完整世界（ConsequenceSet/CareerDelta/WorldDelta）→ M07 / epic-020；M06 只到 OutcomeCandidates/战斗结束态。
- 完整兵法谱（火攻/水攻等全量）→ 仅接 ≥1 机动招式（FeintAmbush）满足硬退出门，其余渐进。
- 新 Unity scene / 新 UI / 新战斗或兵法公式。

## Definition of Done

This epic is complete when:
- 战斗态接入 `CampaignSession`；从 `CommittedPlan` 开战；阶段命令经 `BattleResolver.ResolvePhase` 解析。
- 兵法事后识别接入（含 **FeintAmbush 机动招式**，CD 硬退出门）；兵法只作事后标签、无无条件按钮（TR-battle-002）。
- 阶段稳定管线 + 异常原子回滚（TR-battle-003）。
- 战斗确定性：同快照+配置+种子+命令流 → 同 hash（TR-battle-001）+ 战斗态存读档一致。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M05 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-019-tactical-battle-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
