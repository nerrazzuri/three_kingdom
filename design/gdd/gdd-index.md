# GDD 索引

## 状态定义

- `Planned`：已确定需要，但尚未进入 GDD 编写阶段。
- `Draft`：已创建，尚未通过跨系统审查。
- `Reviewed`：已完成跨系统审查并处理阻断项。
- `Locked for Slice`：允许据此编写 method specs 与 vertical slice story。

## 首批 Vertical Slice GDD

| ID | 系统 | 路径 | 状态 |
|---|---|---|---|
| GDD_001 | 游戏时间 | [GDD_001_GAME_TIME](gdd-001-game-time.md) | Draft |
| GDD_002 | 季节与天气 | [GDD_002_SEASON_WEATHER](gdd-002-season-weather.md) | Draft |
| GDD_003 | 世界地图 | [GDD_003_WORLD_MAP](gdd-003-world-map.md) | Draft |
| GDD_004 | 城市经济 | [GDD_004_CITY_ECONOMY](gdd-004-city-economy.md) | Draft |
| GDD_005 | 人物 | [GDD_005_CHARACTER](gdd-005-character.md) | Draft |
| GDD_006 | 关系与派系 | [GDD_006_RELATIONSHIP_FACTION](gdd-006-relationship-faction.md) | Draft |
| GDD_007 | 情报与侦察 | [GDD_007_INTELLIGENCE_RECON](gdd-007-intelligence-recon.md) | Draft |
| GDD_008 | 军议 | [GDD_008_WAR_COUNCIL](gdd-008-war-council.md) | Draft |
| GDD_009 | 战前准备 | [GDD_009_BATTLE_PREPARATION](gdd-009-battle-preparation.md) | Draft |
| GDD_010 | 兵法沙盒战斗 | [GDD_010_BATTLE_TACTICS_SANDBOX](gdd-010-battle-tactics-sandbox.md) | Draft |
| GDD_011 | 士气与疲劳 | [GDD_011_MORALE_FATIGUE](gdd-011-morale-fatigue.md) | Draft |
| GDD_012 | 后勤与补给 | [GDD_012_LOGISTICS_SUPPLY](gdd-012-logistics-supply.md) | Draft |
| GDD_013 | 存档与读档 | [GDD_013_SAVE_LOAD](gdd-013-save-load.md) | Draft |

## Meta / 大规划 GDD（连接战役为可持续游戏）

| ID | 系统 | 路径 | 状态 |
|---|---|---|---|
| GDD_014 | 战役与生涯 | [GDD_014_CAMPAIGN_AND_CAREER](gdd-014-campaign-and-career.md) | Locked for Slice |
| GDD_015 | 条件历史世界模型 | [GDD_015_HISTORICAL_WORLD_MODEL](gdd-015-historical-world-model.md) | Locked for Slice |
| GDD_016 | 敌方 AI | [GDD_016_ENEMY_AI](gdd-016-enemy-ai.md) | Reviewed |

这三篇定义游戏三层结构的上两层（历史世界模型 + 太守生涯）与让“自由布阵”有深度的敌方 AI，锁定整体大方向（见 [game-concept.md](game-concept.md)），防止竖切与大规划脱节。GDD_016 关联 [ADR-0006](../../docs/architecture/adr-0006-deterministic-enemy-ai.md)。

## 编写与审查顺序

1. 时间、天气、地图定义共同尺度。
2. 人物、关系、城市定义非战斗权威状态。
3. 情报、军议、战前准备定义玩家如何理解并改变条件。
4. 士气、疲劳、补给与战斗定义可确定性解析。
5. 存档定义完整状态边界和版本策略。
6. 运行 `/review-gdds`，解决阻断矛盾后才可锁定。

## 当前批次结论

13 份 vertical slice GDD 已创建并完成结构检查，当前统一为 `Draft`。它们定义了系统目的、状态边界、输入输出、AI、存档、测试和 MVP 范围，但尚未通过跨系统矛盾审查，不得用于 gameplay 实现。

## 每份 GDD 的强制章节

System Purpose、Player Fantasy、Core Loop、Main Rules、Data Model、Player Inputs、System Outputs、Dependencies、Edge Cases、Failure Cases、Balancing Parameters、UI Requirements、AI Requirements、Save / Load Requirements、Test Requirements、MVP Scope、Future Scope、Open Questions。

## 控制规则

- 未列入索引的 GDD 不能绕过范围审查直接进入实现。
- GDD 只定义系统行为，不在其中硬编码平衡数值。
- 所有跨系统状态必须标明权威来源和消费者。
- GDD_010 若出现无条件计策按钮，即不得通过审查。
