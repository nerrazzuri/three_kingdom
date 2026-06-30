# Epic: Historical World / Faction Loop（历史世界与势力循环 / M10）

> **Layer**: Feature（含 Assembly 装配——历史世界 Domain 接入会话）
> **GDD**: `design/gdd/gdd-015-historical-world-model.md`
> **Architecture Module**: M10 Historical World / Faction Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M10）
> **Governing ADR**: ADR-0007（条件历史世界模型）· ADR-0008（城权只读）· ADR-0004（确定性）· ADR-0005（存档）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-history-state-into-session.md) | 历史世界态接入会话（目录 + 触及 + 分叉配置） | Integration | ✅ Complete | ADR-0009/0007 |
| [002](story-002-event-trigger-reachability.md) | 历史事件按时间窗触发（够不着前置短路恒成立） | Integration | ✅ Complete | ADR-0007/0004 |
| [003](story-003-divergence-propagation.md) | 玩家触及分叉 + 下游传播重评估 | Integration | ✅ Complete | ADR-0007/0004 |
| [004](story-004-history-save-determinism.md) | 历史世界态存读档 + 同序列同走向 | Integration | ✅ Complete | ADR-0005/0004 |

## Overview

让天下大势沿《三国演义》推进，同时允许玩家触及范围内改写历史——玩家活在真实推进的三国世界里，**够不着的历史继续，够得着的历史分叉**。M00~M09 已就绪；历史世界 Domain 内核（`HistoryAdvancer`/`WorldProgressionService`/`DivergencePropagationService`/`HistoricalEventCatalog`，epic-012 完成）已实现事件四元组 + reachability 触发门 + 分叉下游传播，会话已接世界**时间推进**，但**历史事件触发/分叉尚未接入** `Advance`。本 epic 把它接入可玩会话：会话持历史事件目录 + 玩家触及 + 分叉配置，推进时按时间窗触发到期事件——够不着（reachability 外）短路前置直接正常结局（早期历史便宜），够得着且前置成立正常，够得着但前置被玩家破坏则分叉 + 向下游传播；历史态（已触发/已分叉）随 world 段存读档，同一行动序列产生同一历史走向。

## Boundary（与 M00 的边界）

- **已交付**：M00 会话世界时间推进（`AdvanceWorld`）；历史世界 Domain 内核（epic-012，已测但事件触发/分叉仅接旧路径）。
- **M10（本 epic）新增**（**含新生产代码**，同 M03~M09 装配）：
  1. 会话持 `HistoricalEventCatalog` + `PlayerReach` + `DivergencePropagationConfig`（数据驱动，开局注入）。
  2. `AdvanceHistory` 命令：按时间窗触发到期未触发事件（稳定序），够不着前置短路、够得着评估前置/分叉。
  3. 分叉时 `DivergencePropagationService.Propagate` 向下游重评估（脱稿深度由配置定）。
  4. 历史态（triggered/diverged）写入 `WorldState`（已在存档 world 段，epic-012）。
- **裁断（module-plan 风险「全量历史数据压垮，按时代/区域包分批」）**：MVP 小型目录；`PlayerReach` 开局固定（玩家扩张动态更新 reach 留后续）；catalog/reach/config 数据驱动按指纹由 Restore 提供，不入存档体（历史**态**在 world 段）。

## 关键护栏（风险）

> module-plan §M10 风险：**全量历史数据会压垮生产；需按时代/区域包分批加载**。
- **reachability 门**（ADR-0007 / TR-world-002）：够不着的历史事件前置**恒成立**（短路不评估）——"早期历史便宜"，玩家够不着的天下大势照常推进。
- **城权只读**（ADR-0008 / TR-world-003）：历史/世界模型不独立写城池归属，订阅 GDD_004 控制权变更。
- **确定性**（ADR-0004 / TR-world-002）：同一行动序列产生同一历史走向；分叉传播确定性。

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-world-001 | WorldState（势力存续/疆域、各城归属反映、已触发/已分叉事件集合）为权威；历史推进确定性 | ADR-0007/0004 ✅ |
| TR-world-002 | HistoricalEvent 四元组 + reachability 门触发 + 分叉下游重评估；同一行动序列产生同一历史走向 | ADR-0007 ✅ |
| TR-world-003 | 城池归属为只读投影、订阅 GDD_004 控制权变更事件，世界模型不独立写归属 | ADR-0008 ✅ |
| TR-world-004 | 玩家不在场势力混战用抽象结算（非完整战役）、确定性；脱稿深度可配置 | ADR-0007 ✅ |
| TR-world-005 | 配置校验拒绝缺前置或缺分叉分支的历史事件 | ADR-0007 ✅ |
| TR-world-006 | WorldState + HistoricalEvent diverged 标志存档 round-trip 一致 | ADR-0005 ✅ |

> 注：M10 为装配 epic，复用 epic-012 的 TR-world-*（像 epic-015/017/018/022 复用既有 TR），无新 TR。无 untraced requirement。

## Scope

### In Scope
- 会话持历史事件目录 + 玩家触及 + 分叉配置（开局注入，数据驱动）。
- `AdvanceHistory`：按时间窗触发到期未触发事件（稳定序）；够不着前置短路恒成立、够得着评估。
- 玩家触及分叉 + 下游传播重评估（`DivergencePropagationService`）。
- 历史态（triggered/diverged）存读档一致 + 同一行动序列同一历史走向（确定性哈希）。

### Out of Scope
- 全量历史事件网络（按时代/区域包分批；MVP 小目录）。
- `PlayerReach` 动态更新（玩家扩张 → 触及变化；MVP 开局固定，留后续）。
- 抽象结算混战完整化（TR-world-004 Domain 已有，M10 不深化）。
- 新 Unity scene / 新 UI。

## Definition of Done

This epic is complete when:
- 历史事件按时间窗触发接入会话；够不着前置短路恒成立（TR-world-002 reachability）。
- 玩家触及 + 前置被破坏 → 分叉 + 下游传播（ADR-0007）。
- 城权只读经 GDD_004（ADR-0008）；同一行动序列同一历史走向（确定性）。
- 历史态（triggered/diverged）存读档一致（TR-world-006）。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M09 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-023-historical-world-faction-loop`（已拆）→ 逐 story `/dev-story` → `/code-review` → `/story-done`。
