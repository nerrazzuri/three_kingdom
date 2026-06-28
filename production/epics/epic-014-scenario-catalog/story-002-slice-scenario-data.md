# Story 002: SliceScenario 数据驱动（SliceScenarioData，收尾 CON-5）

> **Epic**: Scenario / Campaign 配置目录（M01）
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: S / 0.5d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes
**Completed**: 2026-06-28 · 全套 602/602 绿（-warnaserror 0）
**Test**: SliceScenarioDataTests（4 测：Default 单一实例 / 场景标量+聚合取自数据 / 集合数量+主题注入 / Default() 经数据源）
**Code Review**: inline lean — ADR-0003（数据/逻辑分离，字面值集中于不可变数据源）COMPLIANT；行为保持（字面值逐字搬移，确定性不变，既有 598 全绿）
**实现**: 新增 `SliceScenarioData`（不可变数据源 + 嵌套 AdviceSpec/CharacterSpec）；`SliceScenario` 改为读数据源组装 Domain 聚合，方法体无魔法数；`Default()` → `new SliceScenario(SliceScenarioData.Default)`

## Context

**GDD**: 横切 `systems-index.md` + `gdd-005/008`（人物/军议字面值）
**Requirement**: CON-5（full-game-review-2026-06-28：274 行硬编码违反 ADR-0003）
**ADR Governing Implementation**: ADR-0003（数据驱动配置，primary）
**ADR Decision Summary**: 平衡数值集中于不可变数据源（SO 编辑期→不可变配置的接缝）；逻辑只读数据，不内联字面值。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW（纯重构，行为保持）

**Control Manifest Rules (this layer)**:
- Required: 平衡/场景值数据驱动；逻辑无魔法数字
- Forbidden: 方法体内硬编码平衡数值（technical-preferences Forbidden Patterns）

---

## Acceptance Criteria

- [x] 新增不可变 `SliceScenarioData`，持有 slice 全部开局禀赋/链条/人物字面值（单一来源）
- [x] `SliceScenario` 仅做数据→Domain 聚合映射组装，构造方法体内无平衡数字（魔法数）
- [x] `SliceScenario.Default()` 从 `SliceScenarioData.Default` 组装，非内联工厂
- [x] 既有竖切回归全绿（行为逐字保持，确定性不变）

---

## Implementation Notes

- `SliceScenarioData`（Application.Session）：纯数据，私有 ctor 设全部字面值 + `static Default` 单一不可变实例；嵌套 `AdviceSpec`（敌情主题由组装器注入）+ `CharacterSpec`（能力五域 + 十分制性格 + 健康）。
- `SliceScenario(SliceScenarioData data)`：构造仅映射数据为 `CityEconomyState`/`CitySettlementConfig`/`DiplomacyConfig`/`CouncilConfig`/`AdviceTemplate`/`CharacterState`；公共 API 不变 → 消费方/既有测试零改动。
- 健康→战力因子映射（Healthy/Injured/Fallen）保留为组装器内的领域映射规则（非每场景平衡值），不属 CON-5 范畴。

---

## Out of Scope

- 多城 CitySeed 扩展、Unity SO 编辑器管线（M07）；全量历史场景内容（M06）。

---

## QA Test Cases

- **AC-1/2/3**: 数据驱动
  - Given: `SliceScenarioData.Default`
  - When: `new SliceScenario(data)` / `SliceScenario.Default()`
  - Then: 场景标量、城池、外交、花名册、建议均取自数据；建议主题统一注入 EnemySubject；两路径一致
- **AC-4**: 回归
  - Given: 既有 598 竖切测试（Raid/Ambush/Diplomacy/Council/MVP/Save…）
  - When: 重构后运行
  - Then: 全绿（确定性与数值逐字不变）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Application/SliceScenarioDataTests.cs` — exists and passes
**Status**: [x] Created · 602/602 绿

---

## Dependencies

- Depends on: Story 001（ScenarioCatalog）DONE · epic-013（CampaignSession 脊梁）DONE
- Unlocks: M03 治理循环（epic-015）多城/多场景内容扩展
