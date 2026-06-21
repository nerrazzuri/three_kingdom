# Epic: 世界基底（时间·环境·地图拓扑）

> **Layer**: Foundation
> **GDD**: design/gdd/gdd-001-game-time.md · gdd-002-season-weather.md · gdd-003-world-map.md
> **Architecture Module**: Domain 世界模拟内核（时间推进 + 环境派生修正 + 区域/路线拓扑）
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

提供确定性的世界基底：权威时间（WorldDay + DaySegment，嵌套 BattlePhase）、由配置+确定性随机流驱动的天气/风向、以及区域/路线拓扑图与确定性寻路。这是其余所有时序系统的根——时间只能由推进命令前进、环境只产出具名修正供消费者自取、地图坐标不参与 Domain 结算。全部确定性可复盘（同种子→同结果）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0004: 确定性战斗模拟 | 定点 Q16.16 + 注入随机流 + 状态哈希；天气/寻路平局按稳定 ID 序 | HIGH |
| ADR-0003: 数据驱动配置 | 天气转移权重、时段预算、地形修正来自版本化配置 | MEDIUM |
| ADR-0002: 架构分层 | 时间/环境/地图为 Domain 权威，UI 只读投影 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-time-001 | WorldDay+DaySegment 权威时间；同点事件按稳定优先级/ID 全序解析，不依赖帧率 | ADR-0004 ✅ |
| TR-time-002 | 嵌套 BattlePhase 按配置预算消耗，耗尽跨入下一时段并触发天气/补给/疲劳结算 | ADR-0004/0003 ✅ |
| TR-time-003 | 行动耗时/期限/取消数据模型可存档且 round-trip 后产生相同事件序列 | ADR-0005/0004 ✅ |
| TR-weather-001 | 天气转移由配置权重 + 显式确定性随机流；同流位置+同前态→同结果 | ADR-0004/0003 ✅ |
| TR-weather-002 | 环境只产出具名修正，各消费者自取，不直接触发计策成功 | ADR-0002 ✅ |
| TR-map-001 | 区域/路线拓扑图，视觉坐标不参与结算；单位无权威像素坐标 | ADR-0002 ✅ |
| TR-map-002 | 确定性寻路（平局按稳定 ID 序）+ 区域容量门控 + 相向移动接触判定 | ADR-0004 ✅ |
| TR-map-003 | 地图真值与阵营知识分离；敌区须由侦察更新，控制权变更不自动揭示 | ADR-0002 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭
- gdd-001/002/003 验收标准全部验证
- 时间推进、天气解析、寻路结果确定性可复现（同种子→同事件/哈希）
- 全部 Logic/Integration story 在 `tests/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 确定性时间推进与时段/日界结算 | Logic | ✅ Complete | ADR-0004 |
| 002 | 嵌套战斗时段预算与跨时段触发 | Logic | ✅ Complete | ADR-0004 |
| 003 | 配置驱动天气/风向确定性解析 | Logic | ✅ Complete | ADR-0004 |
| 004 | 区域/路线拓扑与确定性寻路 | Logic | Ready | ADR-0004 |
| 005 | 地图真值与阵营知识分离 | Integration | Ready | ADR-0002 |

## Next Step

S1、S2、S3 ✅ Complete（2026-06-22）。下一步：`/story-readiness production/epics/epic-002-world-substrate/story-004-topology-pathfinding.md` → `/dev-story`
