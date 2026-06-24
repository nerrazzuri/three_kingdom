# Epics Index

Last Updated: 2026-06-24
Engine: Unity 6.3 LTS + C#
Manifest Version: 1 (2026-06-21)

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
| [战役与生涯](epic-011-campaign-career/EPIC.md) | Feature | 生涯/晋升/自立 | gdd-014 | 5 stories | Ready |
| [条件历史世界模型](epic-012-historical-world-model/EPIC.md) | Feature | 世界骨架/历史推进 | gdd-015 | 6 stories | Ready |

> 这两个 Meta epic 把单场战役连接成可持续人生（014）+ 提供历史世界骨架（015）。
> 014/015 已 Locked for Slice；governing ADR-0007/0008 + 0002~0005 全 Accepted；无 untraced 需求。
> 敌方 AI（gdd-016，ADR-0006）仍 Reviewed，未建 epic。

## 后续层（未展开）

- **Feature（其余）**：完整 8 阶梯队、全时间线事件网络等见各 GDD §Future Scope，MVP 不含。

## 统计

- 10 epics（3 Foundation + 6 Core + 1 Presentation）；前 9 epics（28 stories）✅ Complete
- epic-010 Presentation：预期 5 stories（1 Logic 可测表现逻辑 + 4 UI），待 `/create-stories epic-010-slice-ux`
- 全部 governing ADR 为 Accepted（ADR-0001~0005）；无 Blocked story

## 下一步

★ **全部 9 epics（28 stories）已 ✅ Complete（2026-06-22）** — Domain 四层内核 + 存档/复现底座全部落地，
测试累计 **329/329 全绿，`-warnaserror` 0 warning**。
- **下一阶段候选**：Presentation 层 EPIC_010（Slice UX 与可访问性，UX 规格已 Approved）→ `/create-epics layer:presentation`；
  或 Unity 表现层垂直切片重验核心幻想（CD-C3/TD CONCERNS：核心幻想未在 Unity 表现层实证）。
- 挂账 guardrail（非阻断）：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。
