# Epics Index

Last Updated: 2026-06-21
Engine: Unity 6.3 LTS + C#
Manifest Version: 1 (2026-06-21)

> 本文件由 `/create-epics` + `/create-stories` 生成，为可执行 epic/story 总账。
> 概念级规划表见 `epics-index.md`（EPIC_001–010 范围说明）；story 草稿溯源见 `../story-backlog.md`。

## Foundation 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [项目与 Domain 基础](epic-001-domain-foundation/EPIC.md) | Foundation | 工程底座 | 横切 | 4 stories | Ready |
| [世界基底（时间·环境·地图）](epic-002-world-substrate/EPIC.md) | Foundation | 时间/天气/地图 | gdd-001/002/003 | 5 stories | Ready |
| [存档与复现](epic-009-save-replay/EPIC.md) | Foundation | 存档 | gdd-013 | 3 stories | Ready |

## Core 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [人物与关系](epic-003-character-relationship/EPIC.md) | Core | 人物/关系 | gdd-005/006 | 3 stories | Ready |
| [城市与后勤](epic-004-city-logistics/EPIC.md) | Core | 城市/后勤/外交入口 | gdd-004/012 | 3 stories | Ready |
| [情报与军议](epic-005-intel-council/EPIC.md) | Core | 情报/军议 | gdd-007/008 | 3 stories | Ready |
| [战前准备](epic-006-battle-preparation/EPIC.md) | Core | 战前准备 | gdd-009 | 2 stories | Ready |
| [兵法沙盒结算](epic-007-tactics-sandbox/EPIC.md) | Core | 战斗/士气 | gdd-010/011 | 3 stories | Ready |
| [后果与可玩失败](epic-008-outcome-consequence/EPIC.md) | Core | 后果结算 | gdd-010 §后果 | 2 stories | Ready |

## 后续层（未展开）

- **Presentation**：EPIC_010 Slice UX 与可访问性 — UX 规格已就绪（`design/ux/main-menu.md`/`hud.md`/`pause-menu.md`，均 Approved）；待 Core 接近完成后 `/create-epics layer:presentation`。
- **Feature**：MVP 不含；slice 后按需。

## 统计

- 9 epics（3 Foundation + 6 Core）、28 stories（12 Foundation + 16 Core）
- Story 类型分布：Logic 17 / Integration 11
- 全部 governing ADR 为 Accepted（ADR-0001~0005）；无 Blocked story

## 下一步

`/sprint-plan new`（用 slice REPORT velocity 标定 solo 容量）→ `/story-readiness [story]` → `/dev-story [story]`。
按 story 的 `Depends on:` 字段顺序推进（Foundation 先于 Core）。
