# Story 002: 晋升申请命令（门槛达成晋级，未达稳定错误码无写入）

> **Epic**: Career / Authority Loop（生涯与权限循环 / M09）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: S（2–3 h，命令接入）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-001`、`TR-career-005`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 晋升申请经会话命令复用 `CareerProgressionService.RequestPromotion`，门槛达成晋一阶；未达稳定错误码、无写入。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 晋升经 Command 路径；门槛由配置阈值确定性判定
- Forbidden: 跳级；门槛未达却晋升
- Guardrail: 失败稳定错误码、无部分写入（TR-career-005）

---

## Acceptance Criteria

- [ ] `RequestPromotion(session, ladder)` 门槛（merit/renown/standing）达成 → 晋一阶并写回会话
- [ ] 门槛未达 → `CareerErrorCode.PromotionThresholdNotMet` 稳定错误码、无写入（生涯态不变）
- [ ] 已达最高阶 → `AlreadyAtMaxRank` 稳定错误码
- [ ] 晋升确定性：同生涯态 + 同配置 → 同结果 + 同哈希
- [ ] 失败后会话可继续（再累积功绩达门槛后晋升成功）

---

## Implementation Notes

- `CampaignSessionService.RequestPromotion` 已实现：复用 `CareerProgressionService.RequestPromotion`，成功 `session.SetCareer`，失败不写。
- 门槛由 `PromotionLadderConfig`（MeritReq/RenownReq/StandingReq 按阶）数据驱动。

---

## Out of Scope

- Story 001：功绩累积（本 story 依赖其达门槛）
- Story 003：自立反叛
- Story 004：生涯态存读档

---

## QA Test Cases

- **AC-1**: 门槛达成晋级
  - Given: 功绩已达下一阶门槛的会话
  - When: `RequestPromotion(s, ladder)`
  - Then: `Applied==true`；会话 Rank 晋一阶
  - Edge cases: N/A

- **AC-2**: 门槛未达稳定错误码无写入
  - Given: 功绩不足的会话；记录 `before=ComputeHash()`
  - When: `RequestPromotion(s, ladder)`
  - Then: `Applied==false`；`Error==PromotionThresholdNotMet`；`ComputeHash()==before`
  - Edge cases: N/A

- **AC-3**: 已达最高阶
  - Given: 最高阶会话
  - When: `RequestPromotion`
  - Then: `Error==AlreadyAtMaxRank`，无写入
  - Edge cases: N/A

- **AC-4**: 失败后可继续（累积→晋升）
  - Given: 门槛未达 → 申请失败
  - When: ApplyCareerGain 达门槛 → RequestPromotion
  - Then: 第二次晋升成功
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPromotionTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignPromotionTests.cs` — 4/4 通过（782/782 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（功绩累积达门槛）
- Unlocks: Story 004（晋升后官阶存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Deviations**: M09 轻量装配——命令接受配置参数（数据驱动，同 ResolveSiege 模式）；career 态已在存档 career 段，无新存档代码。
**Code Review**: 内联 — APPROVED（Lean）
