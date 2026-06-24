# Epic: 战役与生涯

> **Layer**: Feature（Meta 连接层）
> **GDD**: design/gdd/gdd-014-campaign-and-career.md
> **Architecture Module**: Career/Meta Domain（生涯状态 + 晋升/自立结算）
> **Status**: Ready
> **Stories**: 5 stories（见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | CareerState 权威状态与确定性结算骨架 | Logic | Ready | ADR-0002/0004 |
| 002 | 忠臣晋升逐级门槛与功绩/名望累积 | Logic | Ready | ADR-0003/0004 |
| 003 | 自立触发判定与三分支结局 | Logic | Ready | ADR-0004/0003 |
| 004 | 太守开局 + 守城事件胜败后果接入 | Integration | Ready | ADR-0008/0002 |
| 005 | 生涯状态存档 round-trip | Integration | Ready | ADR-0005 |

## Overview

把单场战役连接成一段可持续人生。实现玩家从「城池太守」开局后，通过功绩、名望、君主好感与实战，沿**忠臣晋升**或**自立反叛**两条线延续游戏的生涯结算层。本 epic 是「可玩性/连接性/持续性」的来源——它让战斗胜负、城池经营、关系积累有处可写回、有目标可追求。作为 Meta 连接层，依赖既有 Core 层（人物/关系/城市/战斗）已 Complete 的权威状态，自身不重复持有这些状态，只在其上做生涯结算。城池归属由 GDD_004 唯一权威，本层只读（ADR-0008）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0008: 城池控制权跨系统所有权契约 | GDD_004 唯一权威 + CityControlChanged 事件；生涯层只读归属、夺城经事件发起 | LOW |
| ADR-0002: 四层架构 | 生涯状态落 Domain；玩家操作经 Command/Application 唯一写路径 | LOW |
| ADR-0003: 数据驱动配置 | 晋升门槛/自立阈值/名望权重为版本化配置，构建时转不可变 | LOW |
| ADR-0004: 确定性模拟 | 晋升/自立结算定点 + 注入随机 + 入状态哈希 | LOW |
| ADR-0005: 存档版本与迁移 | CareerState/RetinueState/RebellionState 入版本化 DTO，round-trip | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-career-001 | CareerState（merit/renown/lord_standing/rank）权威 Domain 状态；晋升/自立结算确定性、入状态哈希 | ADR-0002/0004 ✅ |
| TR-career-002 | 晋升逐级门槛 + 自立三分支由配置阈值 + 好感快照确定性判定，无隐式随机；非战斗功绩源须速率有竞争力 | ADR-0003/0004 ✅ |
| TR-career-003 | 生涯状态（Career/Retinue/Rebellion/LordMission）存档 round-trip 一致 | ADR-0005 ✅ |
| TR-career-004 | 城池归属只读；夺城/易主经 GDD_004 控制权变更事件，本层不独立写 | ADR-0007 §5 + ADR-0008 ✅ |
| TR-career-005 | 非法操作（门槛未达却申请晋升/自立）返回稳定错误码，无部分写入 | ADR-0002 ✅ |

## MVP Scope（本 epic 聚焦）

太守开局 + 守城开局事件胜败后果 + 功绩/名望两数值 + 忠臣线前 2–3 阶晋升 + 自立**触发判定与三分支结局**（结局后玩法可简化）。暂不做完整 7 阶全部权限差异、复杂任务库、继承基业后君主玩法（见 GDD_014 §Future Scope）。

## Definition of Done

本 epic 完成当：
- 所有 story 经 `/dev-story` 实现、`/code-review`、`/story-done` 关闭
- `design/gdd/gdd-014-campaign-and-career.md` 全部 §MVP Scope 验收准则被验证
- 所有 Logic/Integration story 在 `tests/` 有通过的测试文件
- 所有 Visual/Feel/UI story 在 `production/qa/evidence/` 有签核证据

## Next Step

Run `/create-stories epic-011-campaign-career` 把本 epic 拆为可实现 story。
