# Epics Index

Last Updated: 2026-06-28
Engine: Unity 6.3 LTS + C#
Manifest Version: 2 (2026-06-28)

> 本文件由 `/create-epics` + `/create-stories` 生成，为可执行 epic/story 总账。
> 概念级规划表见 `epics-index.md`（EPIC_001–010 范围说明）；story 草稿溯源见 `../story-backlog.md`。

## Foundation 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [项目与 Domain 基础](epic-001-domain-foundation/EPIC.md) | Foundation | 工程底座 | 横切 | 4 stories | ✅ Complete |
| [世界基底（时间·环境·地图）](epic-002-world-substrate/EPIC.md) | Foundation | 时间/天气/地图 | gdd-001/002/003 | 5 stories | ✅ Complete |
| [存档与复现](epic-009-save-replay/EPIC.md) | Foundation | 存档 | gdd-013 | 3 stories | ✅ Complete |

## Core 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [人物与关系](epic-003-character-relationship/EPIC.md) | Core | 人物/关系 | gdd-005/006 | 3 stories | ✅ Complete |
| [城市与后勤](epic-004-city-logistics/EPIC.md) | Core | 城市/后勤/外交入口 | gdd-004/012 | 3 stories | ✅ Complete |
| [情报与军议](epic-005-intel-council/EPIC.md) | Core | 情报/军议 | gdd-007/008 | 3 stories | ✅ Complete |
| [战前准备](epic-006-battle-preparation/EPIC.md) | Core | 战前准备 | gdd-009 | 2 stories | ✅ Complete |
| [兵法沙盒结算](epic-007-tactics-sandbox/EPIC.md) | Core | 战斗/士气 | gdd-010/011 | 3 stories | ✅ Complete |
| [后果与可玩失败](epic-008-outcome-consequence/EPIC.md) | Core | 后果结算 | gdd-010 §后果 | 2 stories | ✅ Complete |

## Presentation 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [Slice UX 与可访问性](epic-010-slice-ux/EPIC.md) | Presentation | Slice UX/无障碍 | design/ux/* + accessibility | 5 stories | ✅ Complete |

## Feature 层（Meta 连接层 — 大规划）

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [战役与生涯](epic-011-campaign-career/EPIC.md) | Feature | 生涯/晋升/自立 | gdd-014 | 5 stories | ✅ Complete |
| [条件历史世界模型](epic-012-historical-world-model/EPIC.md) | Feature | 世界骨架/历史推进 | gdd-015 | 6 stories | ✅ Complete |

> 这两个 Meta epic 把单场战役连接成可持续人生（014）+ 提供历史世界骨架（015）。
> **Sprint 02（2026-06-28）完成全部 11 story（Must 5 + Should 3 + Nice 3）**；当时 smoke/team-qa 基线为 556/556 绿；当前本地回归验证为 564/564 绿；team-qa APPROVED。
> 014/015 governing ADR-0007/0008 + 0002~0005 全 Accepted；无 untraced 需求。
> 敌方 AI（gdd-016，ADR-0006）仍 Reviewed，未建 epic。

## Assembly 层（装配 — 从内核到可玩游戏）

| Epic | Layer | System | GDD/ADR | Stories | Status |
|------|-------|--------|---------|---------|--------|
| [CampaignSession 完整会话装配](epic-013-campaign-session-assembly/EPIC.md) | Assembly | 会话脊梁 | ADR-0009 + systems-index + TR-session-* | 6 stories | 🔵 In Progress |

> epic-013 = 完整游戏循环模块规划的 **M00 脊梁**（`production/full-game-loop-module-plan-2026-06-28.md`）。ADR-0009 Accepted（2026-06-28，经子代理复审）。首批范围+估算见 EPIC.md §First Sprint Scope。后续装配 epic（M01~M16）见模块规划。

## 后续层（未展开）

- **Feature（其余）**：完整 8 阶梯队、全时间线事件网络等见各 GDD §Future Scope，MVP 不含。

## 统计

- 12 epics（3 Foundation + 6 Core + 1 Presentation + 2 Feature Meta）全部 ✅ Complete。
- 44 stories 全部 ✅ Complete；Sprint 02 已完成 11/11（Must 5 + Should 3 + Nice 3）。
- 已实现并测试通过的范围为 Domain / Application / Presentation 逻辑内核与 Slice UX；**不等于完整可玩太守循环已装配完成**。
- 全部 governing ADR 为 Accepted（ADR-0001~0008）；无 Blocked story。
- GDD_016 敌方 AI 仍为 Reviewed：有 GDD + ADR-0006，尚无 epic/story/实现。

## 下一步

★ **全部 12 epics（44 stories）已 ✅ Complete（2026-06-28）** — Foundation/Core/Presentation/Meta 的 Domain 内核与可测展示逻辑已落地；当前本地回归 **564/564 全绿，`-warnaserror` 0 warning**。

**下一阶段建议**：停止继续扩内核，优先创建“太守循环装配”集成 epic，把 Career / World / Battle / CityControl / Save 段接入可运行 session，并补一个目标循环端到端测试。详见 `docs/reviews/full-game-review-2026-06-28.md` 的 BLK-1。

**并行裁决项**：GDD_016 敌方 AI（ADR-0006）需要决定是否进入 MVP 装配期；若进入，应先建 epic/story 并修复 GDD_016 设计缺口；若不进入，应在概念/控制清单中显式标注 MVP 不含敌方 AI。
