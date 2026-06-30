# Story 002: 侦察命令经会话路径（有成本/时效/暴露，"侦察全部"非法）

> **Epic**: Intelligence / War Council Loop（情报与军议循环 / M04）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-007-intelligence-recon.md`
**Requirement**: `TR-intel-002`、`TR-intel-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 侦察经会话命令路径——指定对象/执行者/方法；解析经既有 `IntelService.Observe → ToReport`，结果并入玩家知识；暴露由确定性随机流判定（同种子同结果）。"侦察全部"非法。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 侦察经 Command 路径；命令前置校验失败稳定错误码、无部分写入
- Forbidden: "侦察全部"合法化；侦察绕过 IntelService 直写知识
- Guardrail: 暴露判定确定性（注入随机流，纳入哈希）

---

## Acceptance Criteria

*来自 GDD `gdd-007-intelligence-recon.md`，作用域限本 story：*

- [ ] 侦察命令指定对象（subject）/执行者（observer=玩家势力）/方法（IntelSource）→ `Observe → ToReport → ApplyReport` 更新玩家知识
- [ ] 侦察后玩家知识投影含新报告（`Knows(subject) == true`）
- [ ] "侦察全部"/缺必填字段（无 subject）→ 稳定错误码、无部分写入（哈希不变）
- [ ] 侦察解析确定性：同会话态 + 同命令 → 同知识结果（同哈希）
- [ ] 侦察前未知主题在 `PlayerKnowledge` 中 `Knows==false`，侦察后为 true（知识增长可观察）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `GameSession.ResolveScout`）：*

- `CampaignSessionService` 新增 `Scout(session, IntelSubjectId subject, IntelSource method)` 命令，返回 `CampaignCommandResult`。
- 流程：`IntelService.Observe(truth, subject, playerFaction, session.CurrentTime)` → `ToReport(observation, playerFaction, method)` → `playerIntel.ApplyReport(report)`。
- 校验：会话须启用情报（否则错误码）；subject 须在真值账本登记（`truth.Has(subject)`），否则稳定错误码、无写入。
- MVP 即时返报（观察时间 = 当前会话时间）；延时返报队列属 Future。
- 暴露/置信/时效由 IntelService + IntelAssessmentService 既有逻辑处理（确定性随机流），不在会话重写。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：情报态接入会话（本 story 依赖其完成）
- Story 003：军议快照 + 知识变化建议过时
- Story 004：情报态存读档
- 庞大间谍网 / 延时返报队列（GDD_007 §MVP/Future）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 侦察命令更新玩家知识
  - Given: 真值含敌方主题、玩家未知该主题的 session
  - When: `Service.Scout(s, enemySubject, IntelSource.Scouting)`
  - Then: `Applied==true`；`s.PlayerKnowledge!.Knows(enemySubject) == true`
  - Edge cases: 重复侦察同主题 → 知识刷新（不报错）

- **AC-2**: 侦察非法主题稳定错误码 + 无写入
  - Given: 真值未登记主题 X 的 session；`before = ComputeHash()`
  - When: `Service.Scout(s, unregisteredSubject, IntelSource.Scouting)`
  - Then: `Applied==false`；稳定错误码；`ComputeHash() == before`
  - Edge cases: 情报未启用的会话 → 稳定错误码（IntelDisabled）

- **AC-3**: 侦察解析确定性
  - Given: 两 session 同开局情报态
  - When: 各 `Scout(enemySubject, Scouting)`
  - Then: 两者 `ComputeHash()` 相同（暴露/置信确定性）
  - Edge cases: N/A

- **AC-4**: 知识增长可观察（反全知不漏真值）
  - Given: 侦察前 `Knows(subject)==false`
  - When: 侦察后
  - Then: `Knows(subject)==true`；但投影中的估计值来自报告（含置信/时效），非真值直读
  - Edge cases: 投影值可与真值不同（置信/区间），不得等于真值直拷

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignScoutCommandTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignScoutCommandTests.cs` — 6/6 通过（680/680 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（情报态接入会话是侦察的执行基底）
- Unlocks: Story 003（军议读侦察后知识；知识变化驱动建议过时）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: MVP 即时返报（观察时间=当前会话时间）；延时返报队列属 Future。"侦察全部"非法体现为须指定登记的具体 subject（否则 UnknownIntelSubject）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignScoutCommandTests.cs` — 6 tests
**新生产代码**: CampaignErrorCode +2（IntelDisabled/UnknownIntelSubject）；CampaignSessionService.Scout（Observe→ToReport→ApplyReport，前置校验零写入）
**Code Review**: 内联 — APPROVED（Lean）
