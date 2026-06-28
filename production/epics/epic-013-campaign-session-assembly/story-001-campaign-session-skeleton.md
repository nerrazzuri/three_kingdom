# Story 001: CampaignSession 骨架 + 配置驱动开局入口

> **Epic**: CampaignSession 完整会话装配
> **Status**: Ready
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: M / 1d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: 横切 `design/gdd/systems-index.md`（Command 路径）
**Requirement**: `TR-session-001`、`TR-session-003`（配置指纹部分）
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准)*

**ADR Governing Implementation**: ADR-0009（CampaignSession 装配边界，primary）· ADR-0002（四层）· ADR-0003（数据驱动配置）
**ADR Decision Summary**: CampaignSession 是 Application 装配层，只编排不拥规则；开局经配置驱动入口，不再以 `SliceScenario.Default()` 作完整游戏唯一源。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Application/Domain C#；不依赖 UnityEngine。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 经 Command/Application 路径；装配只编排
- Forbidden: UI 直接改状态；CampaignSession 持有/计算玩法规则（R-5 闸门）
- Guardrail: 回合/时段制无每帧压力

---

## Acceptance Criteria

- [ ] `CampaignSession` / `CampaignSessionService` 位于 Application 层；Domain 不依赖它（反射/引用方向断言）
- [ ] 配置驱动 `StartCampaign(scenarioConfigId, playerStartId)` 入口产出初始会话；**不以 `SliceScenario.Default()` 作为唯一源**（slice 保留为 fixture）
- [ ] CampaignSession 装配代码**不引用 UnityEngine**（反射断言）
- [ ] 配置指纹进入会话快照元数据（为 S4 统一存档奠基）
- [ ] **R-5 God-object 闸门**：装配代码不引用任何 `*Service`/`*Resolver` 内部规则、不计算 `FixedPoint` 玩法公式、不直接写 `city.owner`/势力存续

---

## Implementation Notes

*Derived from ADR-0009：*

- `CampaignSession`（聚合引用 + 会话元数据）留 Application；`CampaignSessionService` 为 Presentation 可调用入口（StartCampaign/Execute/Advance/CaptureSnapshot/Restore，本 story 先做 StartCampaign + 骨架）。
- 开局入口接收**已校验的场景配置**（最小 `CampaignStartConfig`；完整 ScenarioCatalog 属 epic-014/M01，本 story 用最小配置占位、以接口隔离）。
- 现有 `GameSession` 保留为 slice fixture，**新建** CampaignSession（不原地迁移，ADR-0009 Alt-1 已否决）。
- 错误码：失败返回 `CampaignErrorCode`（R-4），无部分写入。

---

## Out of Scope

- Story 002：日界推进编排
- Story 003：后果原子写回
- Story 004：统一存档信封
- Story 005：目标循环 E2E
- Story 006：共享服务抽取
- 完整 ScenarioCatalog / 多场景（epic-014 / M01）

---

## QA Test Cases

- **AC-1/3/5**: 层级与闸门
  - Given: CampaignSession 程序集
  - When: 反射检查
  - Then: 位于 Application；不引用 UnityEngine；无 public 写规则方法（Set/Compute 玩法）；Domain 不引用 CampaignSession
- **AC-2/4**: 配置驱动开局
  - Given: 一份最小 CampaignStartConfig
  - When: StartCampaign
  - Then: 产出含配置指纹的初始会话；非法配置返回 CampaignErrorCode、无部分写入
  - Edge cases: 缺字段配置被拒

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignSessionSkeletonTests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None（ADR-0009 Accepted；复用 epic-011/012 + FIX-8 地基）
- Unlocks: Story 002、003、004
