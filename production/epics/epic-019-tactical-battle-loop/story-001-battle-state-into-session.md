# Story 001: 战斗态接入会话 + 从 CommittedPlan 开战

> **Epic**: Tactical Battle Loop（兵法沙盒战役循环 / M06）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`
**Requirement**: `TR-battle-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性战斗模拟（secondary）
**ADR Decision Summary**: 装配层只编排——会话持战斗快照 + 战斗配置 + 战斗种子；从 M05 `CommittedPlan` 构造开战快照（玩家 + 确定性预设敌方单位）。复用既有 `BattleSnapshot`，不重写战斗逻辑。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 状态变更经 Application 路径；战斗确定性（纳入哈希）
- Forbidden: 会话内重写战斗公式；硬编码战斗平衡值
- Guardrail: 战斗种子注入（确定性，ADR-0004）

---

## Acceptance Criteria

*来自 GDD `gdd-010-battle-tactics-sandbox.md`，作用域限本 story：*

- [ ] `CampaignSession` 持有战斗态（`BattleSnapshot`）+ `BattleConfig` + 战斗种子
- [ ] 开战命令从 `CommittedPlan`（+ 预设敌方单位）构造初始 `BattleSnapshot`
- [ ] 战斗态纳入 `session.ComputeHash()`
- [ ] 战斗态可选（场景未开战时为 null，向后兼容现有测试）
- [ ] 开战后战斗单位可读（玩家 + 敌方单位在快照中）

---

## Implementation Notes

*来自 ADR-0009 实现指引：*

- 复用 `BattleSnapshot`/`BattleUnitState`/`BattleConfig`（epic-007 已实装）。
- 会话新增可选字段：`BattleSnapshot? _battle`、`BattleConfig?`、`ulong _battleSeed`。
- 开战命令 `StartBattle(session, ...)`：从 `CommittedPlan` + 战役条件构造玩家单位 + 确定性预设敌方单位 → `BattleSnapshot`。
- 敌方单位为**确定性预设**（场景提供），非智能 AI（智能 AI 留 M08）。
- ComputeHash 追加战斗态（单位按 id 排序，确定性）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：阶段命令解析
- Story 003：兵法事后识别（FeintAmbush）
- Story 004：战斗态存读档
- 智能敌方 AI（M08 / epic-021）

---

## QA Test Cases

- **AC-1**: 会话持有战斗态
  - Given: 启用准备并提交计划的 session
  - When: `StartBattle(session, enemyUnits)`
  - Then: `HasBattle == true`；战斗快照含玩家 + 敌方单位
  - Edge cases: 无 CommittedPlan 时开战拒绝（无可执行战役初始条件）

- **AC-2**: 战斗态纳入哈希
  - Given: 两 session 除某单位兵力外相同战斗态
  - When: 各 `ComputeHash()`
  - Then: 哈希不同
  - Edge cases: 战斗态相同 → 哈希相同

- **AC-3**: 开战单位可读
  - Given: `StartBattle` 已执行
  - When: 读战斗快照
  - Then: 玩家单位 + 敌方单位均在；各单位 force/morale 等字段可读
  - Edge cases: N/A

- **AC-4**: 无战斗的会话向后兼容
  - Given: 未开战的 session
  - When: 读战斗态
  - Then: `HasBattle == false`；现有测试不受影响

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattleStateTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignBattleStateTests.cs` — 6/6 通过（723/723 全绿）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-018（M05）Complete（已满足）；epic-007 战斗 Domain 内核（已 Complete）
- Unlocks: Story 002（阶段解析）、Story 004（战斗存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 战斗态可选（nullable）向后兼容；开战须有 CommittedPlan（M05 衔接）；敌方单位确定性预设（智能 AI 留 M08）；DetectionState 加只读 Entries 供哈希/存档。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattleStateTests.cs` — 6 tests
**新生产代码**: DetectionState.Entries；CampaignSession 持战斗态 + AppendBattle 哈希 + StartBattleState/SetBattle/AddBattleCondition；CampaignSessionService.StartBattle
**Code Review**: 内联 — APPROVED（Lean）
