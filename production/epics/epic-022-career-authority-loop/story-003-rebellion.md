# Story 003: 自立资格三分支 + 反叛发起（转新势力/在野路径）

> **Epic**: Career / Authority Loop（生涯与权限循环 / M09）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，命令接入）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-002`、`TR-career-001`、`TR-career-005`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 自立资格三分支（军事/政治/压迫）确定性判定（`CheckEligibility`）；发起（`Launch`）资格达成转新势力/在野，不达稳定错误码。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 自立经 Command 路径；资格由配置阈值 + 好感快照确定性判定
- Forbidden: 资格不足却自立；隐式随机
- Guardrail: 失败稳定错误码无写入；失败不切死局（在野亦合法续局）

---

## Acceptance Criteria

- [ ] `CheckRebellionEligibility(session, config, context)` 三分支资格（军事/政治/压迫）确定性判定，只读不写
- [ ] `LaunchRebellion(session, config, context)` 资格达成 → 转新势力（Faction 变）或在野，写回会话
- [ ] 资格不足 → 稳定错误码、无写入（生涯态不变）
- [ ] 自立确定性：同 (生涯态, 配置, 上下文) → 同结果 + 同哈希
- [ ] 自立失败/资格不足后会话可继续（非死局）

---

## Implementation Notes

- `CampaignSessionService.CheckRebellionEligibility`/`LaunchRebellion` 已实现：复用 `RebellionService`，成功 `session.SetCareer`。
- 三分支：军事（城数/补给/兵力）、政治（名望/好感）、压迫（君主压迫触发）。`RebellionContext`（CitiesOwned/SupplyReady/TroopsReady/LordOppression/NewFactionId）。
- 自立 MVP 改 career 归属（Faction/IsUnaffiliated）；新势力实体完整接入世界经 ConsequenceTransaction R-3 留后续。

---

## Out of Scope

- Story 001/002：功绩/晋升
- Story 004：生涯态存读档
- 新势力实体完整接入世界（留后续）；君主/争霸（M13 红线）

---

## QA Test Cases

- **AC-1**: 资格判定三分支（只读）
  - Given: 满足军事分支（城数≥门槛+补给+兵力）的会话
  - When: `CheckRebellionEligibility(s, config, context)`
  - Then: `CanRebel==true`；会话态不变（只读）
  - Edge cases: 压迫分支（LordOppression）单独可触发资格

- **AC-2**: 资格达成发起自立
  - Given: 资格满足 + NewFactionId 的会话
  - When: `LaunchRebellion(s, config, context)`
  - Then: `Launched==true`；会话生涯转新势力或在野
  - Edge cases: N/A

- **AC-3**: 资格不足稳定错误码无写入
  - Given: 资格不足的会话；记录 `before=ComputeHash()`
  - When: `LaunchRebellion(s, config, context)`
  - Then: `Launched==false`；稳定错误码；`ComputeHash()==before`
  - Edge cases: N/A

- **AC-4**: 自立确定性
  - Given: 两会话同生涯态 + 同配置 + 同上下文
  - When: 各 `LaunchRebellion`
  - Then: 两者结果 + 哈希相同
  - Edge cases: N/A

- **AC-5**: 失败可继续（非死局）
  - Given: 资格不足自立失败
  - When: 后续合法命令
  - Then: 会话仍可继续
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignRebellionTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignRebellionTests.cs` — 5/5 通过（782/782 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（功绩/名望影响政治分支资格）
- Unlocks: Story 004（自立后归属存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Deviations**: M09 轻量装配——命令接受配置参数（数据驱动，同 ResolveSiege 模式）；career 态已在存档 career 段，无新存档代码。
**Code Review**: 内联 — APPROVED（Lean）
