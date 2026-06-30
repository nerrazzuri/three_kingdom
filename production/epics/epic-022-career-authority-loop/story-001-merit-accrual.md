# Story 001: 功绩累积接入会话（含非战斗功绩源）

> **Epic**: Career / Authority Loop（生涯与权限循环 / M09）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: S（2–3 h，命令接入）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-002`、`TR-career-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 功绩累积经会话命令复用 `CareerProgressionService.ApplyGain`，按来源（含非战斗源）增长生涯；成功才写回会话生涯态。装配层不重写功绩公式。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 功绩经 Command 路径；非战斗源速率有竞争力（强制设计锁：战斗不压倒其他）
- Forbidden: 会话内重写功绩公式；硬编码功绩数值
- Guardrail: 同来源 → 同增长（确定性）

---

## Acceptance Criteria

- [ ] `ApplyCareerGain(session, ladder, source, count)` 按功绩来源增长生涯 merit/renown/standing
- [ ] **非战斗功绩源**（LordMissionComplete/CityGovernance/TalentRecruited）也能累积成长（速率有竞争力）
- [ ] 功绩累积后会话生涯态更新（merit/renown 增加）
- [ ] 累积确定性：同 (来源, count, 生涯态) → 同结果 + 同哈希
- [ ] 多次累积叠加正确

---

## Implementation Notes

- `CampaignSessionService.ApplyCareerGain` 已实现：复用 `CareerProgressionService.ApplyGain`，成功 `session.SetCareer`。
- `CareerGainSource`：战斗源（CombatVictory/MajorBattleVictory）+ 非战斗源（LordMissionComplete/CityGovernance/TalentRecruited/RebellionSuppressed）。
- 功绩增量由 `PromotionLadderConfig.GainFor(source)` 数据驱动。

---

## Out of Scope

- Story 002：晋升申请
- Story 003：自立反叛
- Story 004：生涯态存读档

---

## QA Test Cases

- **AC-1**: 战斗功绩累积
  - Given: 太守开局会话（merit=0）
  - When: `ApplyCareerGain(s, ladder, CombatVictory)`
  - Then: `Applied==true`；会话 merit 增加
  - Edge cases: count>1 → 按倍累积

- **AC-2**: 非战斗功绩源成长（竞争力）
  - Given: 太守开局会话
  - When: `ApplyCareerGain(s, ladder, LordMissionComplete)`
  - Then: merit/renown 增加（非战斗源也成长）
  - Edge cases: 非战斗源 merit 增量与战斗源可比（TR-career-002）

- **AC-3**: 累积确定性
  - Given: 两会话同生涯态
  - When: 各 `ApplyCareerGain(同来源)`
  - Then: 两者 `ComputeHash()` 相同
  - Edge cases: N/A

- **AC-4**: 多次累积叠加
  - Given: 连续多次 ApplyCareerGain
  - Then: merit 单调累积
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignMeritAccrualTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignMeritAccrualTests.cs` — 4/4 通过（782/782 全绿）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-011 生涯 Domain（已 Complete）
- Unlocks: Story 002（晋升读功绩）

---

## Completion Notes
**Completed**: 2026-06-30
**Deviations**: M09 轻量装配——命令接受配置参数（数据驱动，同 ResolveSiege 模式）；career 态已在存档 career 段，无新存档代码。
**Code Review**: 内联 — APPROVED（Lean）
