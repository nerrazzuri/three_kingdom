# Story 003: 军议经会话知识快照（同快照确定 + 知识变化建议过时 + 只条件化）

> **Epic**: Intelligence / War Council Loop（情报与军议循环 / M04）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-008-war-council.md`
**Requirement**: `TR-council-001`、`TR-council-002`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 军议经会话当前知识快照 `Convene`——军师只输出条件化建议；同知识快照 → 同输出（确定）；侦察改变知识 → 快照变 → 已召开建议标记过时（不静默更新）。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature 层)**:
- Required: 军师建议而不自动排兵布阵或执行最优方案（强制设计锁）；建议读召开时合法知识快照
- Forbidden: 军师输出综合成功率/唯一推荐/自动命令；知识变化后静默更新旧建议
- Guardrail: 同快照军议输出确定（确定性）

---

## Acceptance Criteria

*来自 GDD `gdd-008-war-council.md`，作用域限本 story：*

- [ ] 军议经会话当前知识快照 `Convene` → 条件化建议集（绑定召开时 `KnowledgeSnapshotId`）
- [ ] 同知识快照下两次召开 → 相同建议输出（确定，module-plan §M04 测试证据）
- [ ] 侦察改变知识后，先前召开的建议 `IsStaleAgainst(当前快照) == true`（知识变化建议过时，TR-council-001）
- [ ] 军师只输出条件化建议：每条含观察/假设/条件/风险/缺失情报/置信，**无**综合成功率、**无**唯一推荐、**无**自动命令（TR-council-002）
- [ ] 军议读阵营知识投影，不触真值（反全知）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `GameSession.Convene` / `CurrentKnowledgeSnapshotId`）：*

- `CampaignSessionService` 新增 `ConveneCouncil(session)` 返回 `CouncilAdviceSet`。
- 流程：算 `session` 当前 `KnowledgeSnapshotId`（由玩家知识 entries 确定性派生）→ `playerIntel.Project()` → claimConfidences（经 IntelAssessment 或配置）→ `WarCouncilService.Convene(snapshotId, knowledge, confidences, advisor, templates, councilConfig)`。
- `KnowledgeSnapshotId` 派生须确定性（主题+估计值+观察时间排序），与 `GameSession.CurrentKnowledgeSnapshotId` 同模式。
- 过时判定用既有 `CouncilAdviceSet.IsStaleAgainst(currentSnapshotId)`：会话暴露当前 snapshotId，调用方比对。
- 军议配置（advisor/templates/councilConfig）数据驱动，经会话开局配置注入（同情报态可选）。
- 军师"只条件化"由 `WarCouncilService` 既有逻辑保证（AdviceStatement 无成功率字段）；本 story 验证装配后仍成立。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002：情报态接入 / 侦察命令
- Story 004：情报态存读档
- 军议参与者多人冲突全量 / 玩家追问交互（GDD_008 §Future / UI）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 军议经会话快照输出建议集
  - Given: 启用情报/军议、含已知主题的 session
  - When: `Service.ConveneCouncil(s)`
  - Then: 返回 `CouncilAdviceSet`，含条件化建议；`SnapshotId` == 会话当前 KnowledgeSnapshotId
  - Edge cases: 零已知主题 → 建议集仍可生成（条件多标"缺情报"）

- **AC-2**: 同快照两次召开输出相同
  - Given: 知识未变的 session
  - When: 两次 `ConveneCouncil(s)`（中间无侦察）
  - Then: 两次建议集的 SnapshotId 相同；建议内容逐条相同（确定）
  - Edge cases: N/A

- **AC-3**: 侦察后旧建议过时（TR-council-001）
  - Given: 召开军议得 `advice`，记录其 SnapshotId
  - When: `Service.Scout(s, newSubject, Scouting)` 改变知识
  - Then: `advice.IsStaleAgainst(s.CurrentKnowledgeSnapshotId) == true`（不静默更新）
  - Edge cases: 侦察未改变知识（重复同主题同值）→ 快照不变则不过时

- **AC-4**: 军师只输出条件化建议（TR-council-002）
  - Given: 召开军议
  - When: 检查每条 `AdviceStatement`
  - Then: 每条含观察/假设/条件/风险/缺失情报/置信；**无**成功率字段、**无**唯一"最佳"标记、**无**自动命令
  - Edge cases: 高能力军师识别更多缺口，但仍不输出成功率

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignWarCouncilTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignWarCouncilTests.cs` — 5/5 通过（680/680 全绿）

---

## Dependencies

- Depends on: Story 002 DONE（侦察改变知识是"建议过时"的驱动）
- Unlocks: Story 004（军议/知识态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing（+军议未启用抛异常副验证）
**Deviations**: 军议建议（LastAdvice）不持久化（军议是查询，重开即可）；军议配置经 SessionCouncilSetup 数据驱动注入。CurrentKnowledgeSnapshotId 由知识确定性派生（同 GameSession 模式）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignWarCouncilTests.cs` — 5 tests
**新生产代码**: SessionCouncilSetup（军议装配配置打包）；CampaignSession.CurrentKnowledgeSnapshotId + Council；CampaignSessionService.ConveneCouncil
**Code Review**: 内联 — APPROVED（Lean）
