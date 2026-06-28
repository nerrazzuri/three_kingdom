# Epic: Opening Governor Loop（太守开局循环 / M02）

> **Layer**: Feature（Assembly 连接层 / Integration）
> **GDD**: `design/gdd/gdd-014-campaign-and-career.md`（primary）+ gdd-010/011/008
> **Architecture Module**: M02（`production/full-game-loop-module-plan-2026-06-28.md` §M02）
> **Governing ADR**: ADR-0009（CampaignSession 装配）· ADR-0008（城池控制权契约）· ADR-0005（存档）· ADR-0004（确定性）
> **Status**: ✅ Complete（4/4 stories，2026-06-29）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Overview

把单场守城战连接成一段**可持续人生的开端**：完整串起「太守开局 → 强制守城 → 胜败后果 → 可继续游戏」（GDD_014 Core Loop 第一环）。M00（epic-013）已建装配脊梁、M01（epic-014）已建场景目录与数据驱动开局；本 epic **不再造管道**，而是把「开局这一小时」做成**一等可玩内容**：开局强制围城的编排、胜/败两支后果都完整落地、两支都能进入下一天、两支都能存读档。这是「人生延续感」的起点——它让战斗胜负、城池得失第一次有处可写回、有目标可追求，而非一次性战斗 demo。

## Boundary（与 M00 的边界）

- **M00（epic-013）已交付**：通用装配脊梁 + 一条最小目标循环 E2E（守城败→004→015→014→存档 round-trip），证明「管道通」（TR-session-001~005）。
- **M02（本 epic）新增**：开局太守弧的**内容编排与两支完整化**——强制开局围城作为一等 campaign-start 事件；胜支开生涯+城权、败支在野延续（六种合法续局之一）；两支都可继续（非读档）且可存读档。复用既有 Domain 规则（epic-008 后果 / epic-011 生涯 / epic-012 世界），不新增 gameplay 公式。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；不算 FixedPoint 公式、不直接写 city.owner / 势力存续、不引用 *Service 内部 | LOW |
| ADR-0008: 城池控制权所有权契约 | 夺城/失城经 GDD_004 唯一权威 + 控制权变更事件；生涯/世界只读归属 | LOW |
| ADR-0005: 存档版本与迁移 | 显式版本化 DTO/JSON + 原子写入；胜败两支续局点 round-trip 一致 | LOW |
| ADR-0004: 确定性战斗模拟 | 整数/定点 + 注入随机流 + 状态哈希；同种子→同结果 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-session-004 | 守城胜败后果链：胜→生涯/权限；败→在野延续；任一结局玩家可继续（非读档），失败可继续状态合法 | ADR-0009 ✅ |
| TR-session-005 | 目标循环端到端确定性：同种子+同配置+同命令流→同状态哈希；守城败→004→015→014→存档 round-trip 全一致 | ADR-0004/0009 ✅ |
| TR-career-001 | CareerState（merit/renown/lord_standing/rank）为权威 Domain 状态；晋升/自立结算确定性、纳入状态哈希 | ADR-0004 ✅ |
| TR-career-003 | 生涯状态（CareerState/RetinueState/RebellionState/LordMissionLog）存档 round-trip 一致 | ADR-0005 ✅ |
| TR-career-004 | 城池归属只读；夺城/易主经 GDD_004 控制权变更事件，本层不独立写归属 | ADR-0008 ✅ |
| TR-career-005 | 非法操作返回稳定错误码，无部分写入 | ADR-0009 ✅ |

> 注：M02 为 Integration 装配 epic，复用 epic-013 的 TR-session-* 与 epic-011 的 TR-career-*，无新 TR。无 untraced requirement。

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-opening-siege-continuability.md) | 开局围城续局可用性——胜败两支 Advance 均可执行（败局可继续核心验证） | Integration | ✅ Complete | ADR-0009/0004 |
| [002](story-002-victory-branch-config-driven-save.md) | 胜支后果——配置驱动生涯初值 + 胜支存读档 | Integration | ✅ Complete | ADR-0008/0009/0005 |
| [003](story-003-defeat-branch-continuation-save.md) | 败支后果——在野延续存读档 + 部曲保留验证 | Integration | ✅ Complete | ADR-0009/0008/0005 |
| [004](story-004-two-branch-e2e-determinism.md) | 两支 E2E 确定性——同种子同 hash + 两结果不同 hash | Integration | ✅ Complete | ADR-0004/0005 |

## Scope

### In Scope
- 开局强制围城作为一等 campaign-start 事件的编排（装配 BattleCatalog 开局战；玩家部署；出 BattleOutcome）。
- 胜支：经 GDD_004 控制权变更确认城归玩家；开 CareerState（merit/renown/lord_standing 初值，rank=城池太守）；君主初始信任/功绩。
- 败支：经 GDD_004 易主失城；保留少量核心部曲（RetinueState）；生成六种合法续局之一（撤退/求和/失职/流亡/投效/东山再起）。
- 两支都暴露可继续状态（进入下一天，非读档）。
- 两支续局点存档 round-trip 一致 + 同种子同哈希。

### Out of Scope
- 完整晋升梯队（资深太守→大都督）与自立反叛发动（M07/M10 / epic-022 career-authority-loop）。
- 城市治理玩法（M03 / epic-016）。
- 君主任务系统全量、招揽/外交战略约束（M10+ / epic-024）。
- 敌方战略 AI（epic-021 战术层）；不自动布阵（GDD 护栏）。
- 新 Unity scene / 新 UI / 新 gameplay 公式。

## Definition of Done

This epic is complete when:
- 开局强制围城在 campaign start 必触发；胜败两路都能进入下一天且可存/读档。
- 败局必生成合法可继续状态（六种之一），非读档可继续。
- 城池得失经 GDD_004 控制权变更事件，生涯/世界只读归属（ADR-0008）。
- 两路目标循环确定性：同种子+同配置+同命令流→同状态哈希。
- All Integration stories 有通过的测试文件于 `tests/unit/ThreeKingdom.Domain.Tests/Session/`。
- 既有竖切 + M00/M01 回归全绿。

## Next Step

Run `/create-stories epic-015-opening-governor-loop` 把本 epic 拆成可实现 stories，再逐 story `/dev-story` → `/code-review` → `/story-done` → commit。
