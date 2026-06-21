# Epic: 城市与后勤

> **Layer**: Core
> **GDD**: design/gdd/gdd-004-city-economy.md · gdd-012-logistics-supply.md（含 §8 外交受控入口）
> **Architecture Module**: Domain 城市政治 + 军队后勤权威模型（资源守恒）
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

让小城粮食、民心、工事与补给约束战争。民用与军用粮食来自同一权威库存，拨给军队后转由后勤持有不重复计算（守恒——全 corpus 最强一致点 004↔012）。日界按稳定顺序结算，资源不低于合法下限。粮食由城市/运输/携行三类持有者保存、转移守恒（同批粮不能同时留城与在途）。断粮通过实际路线切断 + 时段累积传导给士气/疲劳——无断粮成功按钮，不立即全军崩溃。含外交受控入口（gdd-012 §8：求援/求粮/求时限）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | 城市/后勤为 Domain 权威；外交单一受控入口经 Command | HIGH |
| ADR-0004: 确定性战斗模拟 | 资源结算用整数/定点，守恒可验证 | HIGH |
| ADR-0003: 数据驱动配置 | 产耗率/补给消耗/外交代价来自版本化配置 | MEDIUM |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-city-001 | 民用与军用粮食同一权威库存；拨给后转后勤持有不重复（守恒） | ADR-0002/0004 ✅ |
| TR-city-002 | 日界按稳定顺序结算（承诺→产入→消耗→短缺后果→工事/治安）；不低于合法下限 | ADR-0004 ✅ |
| TR-supply-001 | 粮食由城市/运输/携行三类持有者守恒；同批不能同时留城与在途 | ADR-0002/0004 ✅ |
| TR-supply-002 | 断粮经路线切断 + 时段累积传导士气/疲劳；无成功按钮，不立即崩溃 | ADR-0004 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-004/012 验收标准验证
- 资源守恒恒等式有测试（产入−消耗−转移=库存差）
- 断粮传导单一权威（012 持 supply_state 发事件 / 011 唯一施加 morale·fatigue / 010 只读）
- 日界全局顺序遵守（时间→环境→补给→城市→状态事件）
- 全部 Logic story 在 `tests/unit/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 城市日界产耗结算与资源守恒 | Logic | Ready | ADR-0004 |
| 002 | 三持有者补给守恒与路线断粮传导 | Logic | Ready | ADR-0004 |
| 003 | 外交受控入口（求援/求粮/求时限，§8） | Integration | Ready | ADR-0002 |

## Next Step

`/story-readiness production/epics/epic-004-city-logistics/story-001-city-daily-settlement.md` → `/dev-story`
