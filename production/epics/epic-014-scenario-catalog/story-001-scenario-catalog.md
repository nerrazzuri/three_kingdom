# Story 001: ScenarioCatalog 多场景注册 + 校验 + 按 id 开局

> **Epic**: Scenario / Campaign 配置目录（M01）
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: S / 0.5d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes
**Completed**: 2026-06-28 · 全套 598/598 绿
**Test**: ScenarioCatalogTests（5 测：按id开局正确场景/未知id稳定码/列已注册id/重复id加载期拒/空目录拒）
**Code Review**: inline lean — ADR-0003（数据驱动+校验，非硬编码唯一源）/ADR-0009 COMPLIANT
**实现**: ScenarioCatalog（注册+去重校验+Find/Ids）+ CampaignSessionService.StartCampaign(catalog, scenarioId) 重载

## Context

**GDD**: 横切 `systems-index.md` + `gdd-014/015`
**Requirement**: `TR-session-003`（配置驱动开局）

**ADR Governing Implementation**: ADR-0003（数据驱动配置，primary）· ADR-0009
**ADR Decision Summary**: 多命名场景配置经校验目录，按 id 装配 CampaignSession；非法/重复被拒，不部分加载。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (this layer)**:
- Required: 平衡/场景值数据驱动；配置校验拒非法
- Forbidden: 硬编码场景为唯一源
- Guardrail: 回合/时段制

---

## Acceptance Criteria

- [ ] `ScenarioCatalog` 注册多个命名 `CampaignStartConfig`，按 scenario id 查询
- [ ] **配置校验**：重复 scenario id / 空目录 被拒（加载期，不部分加载）
- [ ] `CampaignSessionService.StartCampaign(catalog, scenarioId)` 按 id 开局；未知 id → 稳定错误码
- [ ] 切换场景仅换 id，无代码改动（≥2 场景可载入并各自开局）

---

## Implementation Notes

- `ScenarioCatalog`（Application.Session）：构造校验非空 + 无重复 id；`Find(id)`、`Ids`。
- 各 `CampaignStartConfig` 自身已在 S1（epic-013）ctor 校验范围；目录只做跨条目（去重）校验。
- 服务新增 `StartCampaign(ScenarioCatalog, string scenarioId)` 重载：解析 → 委派既有 StartCampaign(config)；未知 id 返回 `CampaignErrorCode.SessionNotFound`。

---

## Out of Scope

- Story 002：SliceScenario 数据驱动 · 多城治理（M03）· Unity SO 管线（M07）

---

## QA Test Cases

- **AC-1/3/4**: 多场景 + 按 id 开局
  - Given: 含 ≥2 场景的 ScenarioCatalog
  - When: StartCampaign(catalog, "scenario-A") / "scenario-B"
  - Then: 各自开局成功、生涯/世界按该场景配置；未知 id → SessionNotFound、无会话
- **AC-2**: 校验
  - Given: 重复 id / 空目录
  - When: 构造 ScenarioCatalog
  - Then: 抛（加载期拒绝，不部分加载）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/ScenarioCatalogTests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-013 story-001（CampaignStartConfig + StartCampaign）DONE
- Unlocks: Story 002；M03 治理循环
