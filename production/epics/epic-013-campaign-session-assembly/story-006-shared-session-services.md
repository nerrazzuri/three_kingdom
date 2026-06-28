# Story 006: 共享会话服务抽取（新旧会话复用）

> **Epic**: CampaignSession 完整会话装配
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: S / 0.5d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes（COMPLETE WITH NOTES）
**Completed**: 2026-06-28 · 全套 593/593 绿
**工程裁定（YAGNI）**：S1-S5 CampaignSession 用生涯+世界各自存档，与竖切 GameSession 的 RngStreamState/SaveMapper 捕获**尚无实质重叠**——共享服务抽取属过早抽象。本 story 验证①两会话独立共存互不干扰②竖切回归不破；**实质共享抽取延后到 CampaignSession 纳入竖切 RNG/情报时（M03+）**，届时才有真可复用件。
**Test**: SessionCoexistenceTests（2 测：两会话独立共存/竖切回归未破）

## Context

**GDD**: 横切 `systems-index.md`
**Requirement**: `TR-session-001`（支撑）、`TR-session-003`（捕获模式）

**ADR Governing Implementation**: ADR-0002（四层，primary）· ADR-0009
**ADR Decision Summary**: 保留 GameSession 为 slice fixture、新建 CampaignSession；把可复用件（WorldClock 接线、RngStreamState 捕获、SaveMapper 模式）抽成共享 Application 服务，两会话复用。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Application 重构；无引擎面。

**Control Manifest Rules (this layer)**:
- Required: 不破坏既有 slice 回归；DRY
- Forbidden: 把 slice 专有假设拖进会话脊梁
- Guardrail: 回合/时段制

---

## Acceptance Criteria

- [ ] 把 WorldClock 接线 / `RngStreamState` 捕获 / `SaveMapper` 捕获模式抽成共享 Application 服务
- [ ] 既有竖切 GameSession 改用共享服务后**回归仍全绿**（slice 测试不破）
- [ ] CampaignSession 复用同一共享服务（去重，不各写一套）
- [ ] slice 专有件（raid/ambush/diplo RNG 流、SliceScenario）**不进**共享脊梁

---

## Implementation Notes

*Derived from ADR-0009 §去留裁定：*

- 这是支撑性重构：S1-S4 实现中会发现 GameSession 与 CampaignSession 的重复捕获/接线，本 story 收口为共享服务。
- 保持 GameSession 为运行 fixture（达内容平价前不停 slice）。

---

## Out of Scope

- 退役 GameSession（待 CampaignSession 内容平价后另议）· 新功能

---

## QA Test Cases

- **AC-1/2/3/4**: 抽取 + 回归
  - Given: 既有 slice 测试（MvpAcceptanceTests 等）
  - When: GameSession 改用共享服务
  - Then: slice 全套回归绿；CampaignSession 与 GameSession 共用同一捕获服务（无重复实现）；slice 专有流不在共享脊梁

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: 既有 slice 回归（全套 dotnet 绿）+ `tests/unit/ThreeKingdom.Domain.Tests/Session/SharedSessionServiceTests.cs`
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（可与 S2-S4 并行收口）
- Unlocks: None
