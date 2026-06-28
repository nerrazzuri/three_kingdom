# Epic: Scenario / Campaign 配置目录（M01）

> **Layer**: Feature（Assembly 连接层）
> **GDD**: 横切 `gdd-003/004/014/015` + `systems-index.md`
> **Architecture Module**: M01（`production/full-game-loop-module-plan-2026-06-28.md`）
> **Governing ADR**: ADR-0003（数据驱动配置，primary）· ADR-0009（CampaignSession 装配）
> **Status**: In Progress（2026-06-28）
> **Stories**: 见下表

## Overview

把硬编码场景替换为**可验证、可扩展、可存档兼容**的开局与战役目录（ADR-0003）。CampaignSession（M00/epic-013）已能从单个 `CampaignStartConfig` 配置驱动开局；本 epic 把它推广为 **ScenarioCatalog**（多命名场景）+ 多城 `CitySeed` + 配置校验，并收尾 CON-5（竖切 `SliceScenario` 数据驱动）。目标：不同城池/势力/历史时间点开局各不相同，且**切换无代码改动**。

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | ScenarioCatalog 多场景注册 + 校验 + 按 id 开局 | Integration | ✅ Complete | ADR-0003/0009 |
| 002 | SliceScenario 数据驱动（SliceScenarioData，收尾 CON-5） | Integration | Ready | ADR-0003 |

## Scope

### In Scope
- `ScenarioCatalog`：多命名 `CampaignStartConfig` 注册、去重校验、按 scenario id 开局。
- 配置校验：缺字段/重复 id/非法值被拒，不部分加载（ADR-0003）。
- CON-5：`SliceScenario` 硬编码工厂 → 不可变 `SliceScenarioData` 数据源（数据/逻辑分离）。

### Out of Scope
- 完整 Unity ScriptableObject 编辑器管线（留 M07 / 后续；本 epic 到"不可变配置 + 数据源接缝"即可）。
- 全量历史场景内容（M06 内容）；多城治理玩法（M03）。

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-session-003 | 配置指纹进快照、载入校验 | ADR-0003/0009 ✅ |
| TR-city-001 | 城市初态合法（多城 CitySeed） | ADR-0003 ✅ |

> 注：M01 横切装配，复用 epic-013 的 TR-session-* 与 ADR-0003；无新 TR。

## Definition of Done
- ScenarioCatalog 可载入 ≥2 场景、按 id 开局、切换无代码改动。
- 非法场景配置被拒、不部分加载。
- SliceScenario 数值来自 `SliceScenarioData`（逻辑无魔法数）。
- 既有竖切回归全绿。

## Next Step
`/create-stories` 已完成（本文件含 stories 表）。逐 story `/dev-story`→`/code-review`→`/story-done`→commit。
