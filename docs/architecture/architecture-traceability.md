# 架构可追溯索引

> **最后更新**：2026-06-21（复审）
> **引擎**：Unity 6.3 LTS

## 覆盖摘要

- 总需求：31
- ✅ 覆盖：28（90%）
- ⚠️ 部分：3（10%）
- ❌ 缺口：0（0%）

> 5 份 ADR（ADR-0001~0005）全部 Accepted，原 17 缺口全部清零。
> 剩余 3 处「部分」为可追踪性列名收尾，已由 ADR-0002/0004 通用原则实质覆盖。

## 完整矩阵

| TR-ID | 系统 | ADR 覆盖 | 状态 |
|---|---|---|---|
| TR-time-001 | 时间 | ADR-0004 | ✅ |
| TR-time-002 | 时间 | ADR-0004 | ✅ |
| TR-time-003 | 时间 | ADR-0005 | ✅ |
| TR-weather-001 | 天气 | ADR-0004 | ✅ |
| TR-weather-002 | 天气 | ADR-0002 | ✅ |
| TR-map-001 | 地图 | ADR-0002 + ADR-0004 | ⚠️ |
| TR-map-002 | 地图 | ADR-0004 | ✅ |
| TR-map-003 | 地图 | ADR-0002 + ADR-0005 | ✅ |
| TR-city-001 | 城市 | ADR-0002 | ✅ |
| TR-city-002 | 城市 | ADR-0002 | ✅ |
| TR-character-001 | 人物 | ADR-0002 | ✅ |
| TR-character-002 | 人物 | ADR-0002 | ✅ |
| TR-relationship-001 | 关系 | ADR-0002 | ✅ |
| TR-relationship-002 | 关系 | ADR-0002 | ✅ |
| TR-intel-001 | 情报 | ADR-0002 | ✅ |
| TR-intel-002 | 情报 | ADR-0004 | ✅ |
| TR-intel-003 | 情报 | ADR-0005 | ✅ |
| TR-council-001 | 军议 | ADR-0002 | ✅ |
| TR-council-002 | 军议 | ADR-0002 | ⚠️ |
| TR-prep-001 | 战前准备 | ADR-0002 | ✅ |
| TR-prep-002 | 战前准备 | ADR-0002 | ✅ |
| TR-battle-001 | 战斗 | ADR-0004 + ADR-0003 | ✅ |
| TR-battle-002 | 战斗 | ADR-0004 | ✅ |
| TR-battle-003 | 战斗 | ADR-0004 | ✅ |
| TR-cohesion-001 | 士气疲劳 | ADR-0002 | ✅ |
| TR-cohesion-002 | 士气疲劳 | ADR-0004 | ✅ |
| TR-supply-001 | 后勤 | ADR-0002 | ⚠️ |
| TR-supply-002 | 后勤 | ADR-0004 | ✅ |
| TR-save-001 | 存档 | ADR-0005 | ✅ |
| TR-save-002 | 存档 | ADR-0005 | ✅ |
| TR-save-003 | 存档 | ADR-0005 + ADR-0003 | ✅ |

## 已知缺口

**无。** 0 缺口。

## 可选可追踪性补强（非阻断）

| TR-ID | 实质覆盖来源 | 建议动作 |
|---|---|---|
| TR-map-001 | ADR-0002 层隔离 + ADR-0004 禁 float | ADR-0004 GDD 表补列 TR-map-001 |
| TR-council-002 | ADR-0002 唯一写路径 | ADR-0002 GDD 表补列 TR-council-002 |
| TR-supply-001 | ADR-0002 单一权威 + 原子命令 | ADR-0002 GDD 表补列 TR-supply-001 |

## 被取代的需求

无。

## 历史

| 日期 | 完整覆盖率 | 备注 |
|---|---|---|
| 2026-06-21（初版） | 3%（1/30） | 仅 ADR-0001 存在；ADR-0002~0005 待撰写 |
| 2026-06-21（复审） | 90%（28/31）+ 10% 部分，0 缺口 | 5 份 ADR 全 Accepted |
