# Epic: 条件历史世界模型

> **Layer**: Feature（Meta 连接层）
> **GDD**: design/gdd/gdd-015-historical-world-model.md
> **Architecture Module**: World Model Domain（WorldState + 历史事件触发 + 抽象结算）
> **Status**: Ready
> **Stories**: 6 stories（见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | WorldState 权威状态与确定性推进骨架 | Logic | ✅ Complete | ADR-0002/0004 |
| 002 | 历史事件四元组 + reachability 触发门 + 配置校验 | Logic | ✅ Complete | ADR-0007/0003 |
| 003 | 分叉传播（下游按 EventId 稳定序重评估） | Logic | ✅ Complete | ADR-0007/0004 |
| 004 | 城池归属只读投影（订阅 GDD_004 控制权变更） | Integration | ✅ Complete | ADR-0008/0007 |
| 005 | 抽象结算器（不在场势力混战） | Logic | ✅ Complete | ADR-0007/0004 |
| 006 | WorldState 存档 round-trip | Integration | Ready | ADR-0005 |

## Overview

提供玩家所处世界的结构与方向：谁占哪城、谁与谁敌对、现在什么局势、下一步会发生什么大事。世界大势严格沿《三国演义》时间线推进（大势随历史），同时定义历史事件的**条件化触发**——条件还在则历史继续，玩家强大到改变了某事件前置条件则该事件分叉（个人靠抉择）。本层用「触发条件 + 玩家触及边界」统一「严格演义」与「可自立改写」这对矛盾，为生涯（epic-011）提供态势、为敌方 AI（GDD_016）提供势力级输入。城池归属为只读投影，订阅 GDD_004 控制权变更事件（ADR-0008），不独立写。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0007: 条件历史世界模型架构 | 事件四元组 + reachability 触发门 + 分叉传播 + 抽象结算器 | LOW |
| ADR-0008: 城池控制权跨系统所有权契约 | 世界模型订阅 GDD_004 CityControlChanged 同步归属，只读不写 | LOW |
| ADR-0002: 四层架构 | 世界模型落 Domain；唯一写路径 | LOW |
| ADR-0003: 数据驱动配置 | 历史事件定义/时间窗/前置/脱稿深度为版本化配置 + 校验 | LOW |
| ADR-0004: 确定性模拟 | 历史推进定点 + 注入随机（抽象结算）+ 入状态哈希 | LOW |
| ADR-0005: 存档版本与迁移 | WorldState/diverged 标志入版本化 DTO，round-trip | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-world-001 | WorldState（势力存续/疆域、各城归属反映、已触发/已分叉事件集合）权威；历史推进确定性 | ADR-0002/0004 ✅ |
| TR-world-002 | HistoricalEvent 四元组 + reachability 门触发 + 分叉下游重评估；同一行动序列同一历史走向 | ADR-0007 ✅ |
| TR-world-003 | 城池归属为只读投影、订阅 GDD_004 控制权变更事件，世界模型不独立写 | ADR-0008 ✅ |
| TR-world-004 | 玩家不在场势力混战用抽象结算（非完整战役）、确定性；脱稿深度可配置 | ADR-0007 ✅ |
| TR-world-005 | 配置校验拒绝缺前置或缺分叉分支的历史事件 | ADR-0003 ✅ |
| TR-world-006 | WorldState + HistoricalEvent diverged 标志存档 round-trip 一致 | ADR-0005 ✅ |

## MVP Scope（本 epic 聚焦）

单一历史战役框（讨董/汜水关一带，对应当前竖切舞台）的世界快照——少数势力、若干城池归属、1–2 个带条件历史事件、玩家够不着则照常触发；城池归属订阅 GDD_004。验证「条件触发 + 触及边界」机制成立。暂不做全 96 年时间线与全图分叉（见 GDD_015 §Future Scope）。

## Definition of Done

本 epic 完成当：
- 所有 story 经 `/dev-story` 实现、`/code-review`、`/story-done` 关闭
- `design/gdd/gdd-015-historical-world-model.md` 全部 §MVP Scope 验收准则被验证
- 所有 Logic/Integration story 在 `tests/` 有通过的测试文件
- 所有 Visual/Feel/UI story 在 `production/qa/evidence/` 有签核证据

## Next Step

Run `/create-stories epic-012-historical-world-model` 把本 epic 拆为可实现 story。
