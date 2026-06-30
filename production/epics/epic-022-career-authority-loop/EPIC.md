# Epic: Career / Authority Loop（生涯与权限循环 / M09）

> **Layer**: Feature（含 Assembly 装配——生涯/权限 Domain 接入会话）
> **GDD**: `design/gdd/gdd-014-campaign-and-career.md`
> **Architecture Module**: M09 Career / Authority Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M09）
> **Governing ADR**: ADR-0004（确定性）· ADR-0009（CampaignSession 装配）· ADR-0008（城权只读）· ADR-0005（存档）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-merit-accrual.md) | 功绩累积接入会话（含非战斗功绩源） | Integration | ✅ Complete | ADR-0009/0004 |
| [002](story-002-promotion-request.md) | 晋升申请命令（门槛达成晋级，未达稳定错误码无写入） | Integration | ✅ Complete | ADR-0009/0004 |
| [003](story-003-rebellion.md) | 自立资格三分支 + 反叛发起（转新势力/在野路径） | Integration | ✅ Complete | ADR-0009/0004 |
| [004](story-004-career-save-determinism.md) | 生涯权限态存读档 + 确定性（晋升/自立后 round-trip） | Integration | ✅ Complete | ADR-0005/0004 |

## Overview

让玩家从太守走向高阶官职、自立或流亡——不让战斗成为唯一成长来源。M00~M08 已就绪；生涯/权限 Domain 内核（`CareerProgressionService`/`PromotionGate`/`RebellionService`/`CareerState`，epic-011 完成）已实现功绩累积、晋升门槛、自立三分支资格，但**晋升申请/功绩累积/自立反叛命令尚未接入** `CampaignSession`（M00/M02 只接了守城后果开生涯）。本 epic 把它接入可玩会话：功绩经会话命令累积（含**非战斗功绩源**，速率有竞争力）、晋升申请经门槛判定（未达稳定错误码无写入）、自立资格三分支 + 反叛发起（转新势力/在野路径），全程确定性、纳入状态哈希、存读档一致。

## Boundary（与 M00/M02/M13 的边界）

- **已交付**：M00 `ConsequenceTransaction.StageCareer`（守城后果写生涯）；M02 `GovernorOutcomeService`（守城开局开生涯）；生涯 Domain 内核（epic-011，已测但晋升/功绩/反叛命令仅接旧路径）。
- **M09（本 epic）新增**（**含新生产代码**，同 M03~M07 装配模式）：
  1. 功绩累积命令 `ApplyCareerGain`（`CareerGainSource` → CareerState merit/renown；非战斗源 LordMissionComplete/CityGovernance/TalentRecruited 也能成长）。
  2. 晋升申请命令 `RequestPromotion`（门槛达成晋级；未达 `PromotionThresholdNotMet` 稳定错误码、无写入）。
  3. 自立资格 `CheckRebellionEligibility` + 反叛发起 `LaunchRebellion`（三分支资格；成功转新势力/在野，失败稳定错误码）。
  4. 生涯权限态（晋升后官阶 / 自立后归属）存读档一致 + 确定性（复用 career 段）。
- **裁断**：君主任命/招揽外交属 M10+；君主/争霸/统一属 M13/M14（红线，须先补 GDD_017/018）。M09 限 GDD_014 范围（太守→高阶官职/自立/流亡）。`RebellionState`/`LordMissionLog` 独立态 MVP 不单独持久化（career 的 Rank/Faction/IsUnaffiliated 已反映晋升/自立结果）。

## 关键护栏（风险）

> module-plan §M09 风险：**官阶只是数值门槛；必须带来不同玩法边界**。
- **非战斗功绩源竞争力**（TR-career-002）：LordMissionComplete/CityGovernance/TalentRecruited 等非战斗源速率须可与战斗源竞争——战斗不是唯一成长路径。
- **城权只读**（ADR-0008 / TR-career-004）：生涯层不独立写城池归属；夺城经 GDD_004。
- **失败可继续**（TR-career-005）：门槛未达/资格不足 → 稳定错误码、无部分写入、会话可继续。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0004: 确定性 | 晋升/自立由配置阈值 + 好感快照确定性判定，无隐式随机；纳入状态哈希 | LOW |
| ADR-0009: CampaignSession 装配边界 | 生涯命令经会话路径复用 CareerProgressionService/RebellionService，不重写公式 | LOW |
| ADR-0008: 城池控制权所有权契约 | 生涯层城权只读；夺城/易主经 GDD_004 | LOW |
| ADR-0005: 存档版本与迁移 | 生涯权限态 round-trip 一致（复用 career 段） | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-career-001 | CareerState（merit/renown/lord_standing/rank）为权威 Domain 状态；晋升/自立结算确定性、纳入状态哈希 | ADR-0004 ✅ |
| TR-career-002 | 晋升逐级门槛与自立三分支由配置阈值 + 好感快照确定性判定，无隐式随机；非战斗功绩源须速率有竞争力 | ADR-0004 ✅ |
| TR-career-003 | 生涯状态（CareerState/RetinueState/RebellionState/LordMissionLog）存档 round-trip 一致 | ADR-0005 ✅ |
| TR-career-004 | 城池归属只读；夺城/易主经 GDD_004 控制权变更事件，本层不独立写归属 | ADR-0008 ✅ |
| TR-career-005 | 非法操作（门槛未达却申请晋升/自立）返回稳定错误码，无部分写入 | ADR-0009 ✅ |

> 注：M09 为装配 epic，复用 epic-011 的 TR-career-*（像 epic-015/017/018 复用既有 TR），无新 TR。无 untraced requirement。

## Scope

### In Scope
- 功绩累积命令 `ApplyCareerGain`（含非战斗源；merit/renown 增长，确定性）。
- 晋升申请命令 `RequestPromotion`（门槛达成晋级；未达稳定错误码无写入）。
- 自立资格 `CheckRebellionEligibility`（三分支：军事/政治/压迫）+ 反叛发起 `LaunchRebellion`（成功转新势力/在野，失败稳定错误码）。
- 生涯权限态存读档一致（晋升后 Rank / 自立后 Faction/IsUnaffiliated）+ 同输入同哈希。

### Out of Scope
- 君主任命/招揽/外交战略约束（M10/M11）；君主/争霸/统一（M13/M14 红线，须先补 GDD_017/018）。
- `RebellionState`/`LordMissionLog` 独立态完整持久化（MVP career 态反映结果；留后续）。
- 新势力实体完整接入世界（经 ConsequenceTransaction R-3 / 015；M09 自立 MVP 改 career 归属）。
- 新 Unity scene / 新 UI / 新生涯公式。

## Definition of Done

This epic is complete when:
- 功绩累积/晋升申请/自立反叛命令接入会话；非战斗功绩源可成长。
- 晋升门槛达成晋级、未达稳定错误码无写入（TR-career-005）；自立三分支资格 + 发起。
- 城权只读经 GDD_004（ADR-0008）；生涯态确定性纳入哈希。
- 生涯权限态存读档一致 + 同输入同哈希（TR-career-003）。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M08 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-022-career-authority-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
