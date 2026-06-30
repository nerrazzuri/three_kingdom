# Story 004: 敌方 AI 决策接入战区命令（同源确定性 + 重放）

> **Epic**: Enemy AI Loop（敌方 AI 循环 / M08）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配——AI 决策接入战斗）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-016-enemy-ai.md`
**Requirement**: `TR-ai-004`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0006: 确定性效用敌方 AI（primary）；ADR-0004: 确定性（secondary）；ADR-0009: 装配（secondary）
**ADR Decision Summary**: AI 随机消费与战斗模拟同源，纳入同一状态哈希与重放契约（ADR-0004）。敌方 AI 决策 → 战区命令（`BattleOrder`），驱动 M06 战斗，替代 M06 的确定性预设敌方命令。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: AI 决策经命令路径接入战斗；随机与战斗同源（ADR-0004）
- Forbidden: AI 旁路随机源；AI 读战斗真值绕过 AiWorldView
- Guardrail: 同种子 + 同态势 → 同战区命令流（重放）

---

## Acceptance Criteria

*来自 GDD `gdd-016` §MVP + ADR-0006，作用域限本 story：*

- [ ] 敌方 AI 决策（`DecisionRecord` / `StrategicAction`）映射为战区命令（`BattleOrder`）
- [ ] AI 战区命令驱动 M06 战斗阶段解析（替代确定性预设敌方命令）
- [ ] 同种子 + 同态势 → 同 AI 战区命令流（确定性重放，TR-ai-004）
- [ ] AI 随机与战斗同源：AI 决策 + 战斗解析纳入同一状态哈希
- [ ] AI 按 AiWorldView（阵营知识）决策——不读战斗真值（反全知贯穿到战区）

---

## Implementation Notes

*来自 ADR-0006 / ADR-0009 实现指引：*

- 新建 `EnemyAiBattleAdapter`（或 Service 方法）：`StrategicAction` → `BattleOrder`（如 Pursue→Engage、Retreat→Retreat、Hold→Hold、FeintLure→Move+诱敌序）。
- AI 决策的 `IDeterministicRandom` 与战斗 `seed` 同源（ADR-0004）：从战斗种子派生 AI 决策种子，确保 AI 随机与战斗模拟纳入同一重放。
- 接入点：M06 `ResolveBattlePhase` 前，AI 为敌方单位生成命令（经 EnemyAiService.Decide → 映射 BattleOrder），与玩家命令合并解析。
- 反全知贯穿：AI 的 AiWorldView 由其 FactionIntel 投影构造，不读 BattleSnapshot 真实敌（玩家）态。

---

## Out of Scope

*由邻近 story / 后续处理——本 story 不实现：*

- Story 001/002/003：AiWorldView / 评分 / softmax 决策
- OpponentModel 记忆 / StrategicPlan 战略 / LLM 装饰（裁断留后续）
- 完整 AI 战区编排（多单位协同/多阶段战略）→ 渐进；本 story MVP 单决策→命令映射

---

## QA Test Cases

- **AC-1**: AI 决策映射战区命令
  - Given: AI DecisionRecord（选中 Pursue）+ 敌方单位
  - When: 映射为 BattleOrder
  - Then: 产出对应命令（Pursue→Engage 含 target）
  - Edge cases: 各 StrategicAction 都有对应 BattleOrder 映射

- **AC-2**: AI 命令驱动战斗 + 确定性重放
  - Given: 两 session 同开战态 + 同 AI 种子
  - When: 各经 AI 生成敌方命令 → ResolveBattlePhase
  - Then: 两者 BattleResolution.Hash 相同 + 会话哈希相同（同种子同态势→同命令流）
  - Edge cases: N/A

- **AC-3**: AI 随机与战斗同源
  - Given: AI 决策种子从战斗种子派生
  - When: AI 决策 + 战斗解析
  - Then: 纳入同一确定性重放（同种子整局可复现）
  - Edge cases: N/A

- **AC-4**: AI 反全知贯穿战区
  - Given: AI 的 FactionIntel 不含玩家某单位（未侦察）
  - When: AI 决策
  - Then: AI 按其有限情报决策（AiWorldView 不读战斗真值）；误判可能涌现
  - Edge cases: AI 情报与战场真实不符 → 决策按情报（错误信念可读）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/EnemyAI/EnemyAiBattleIntegrationTests.cs` — 必须存在且全绿

**Status**: [x] `EnemyAiBattleIntegrationTests.cs` — 4/4 通过（764/764 全绿）

---

## Dependencies

- Depends on: Story 003 DONE（DecisionRecord）；epic-019（M06 战斗）Complete
- Unlocks: epic-021 完成 → M08 战术层 80% 达成；OpponentModel/战略/LLM 留后续 epic

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: MVP 单决策→命令映射（Pursue→Engage/Retreat→Retreat/Hold→Hold/FeintLure→Conceal）；AI 与战斗同源（同 battleSeed）；多单位协同/会话级编排留后续。
**Test Evidence**: `tests/unit/.../EnemyAI/EnemyAiBattleIntegrationTests.cs` — 4 tests
**新生产代码**: EnemyAiBattleAdapter（StrategicAction→BattleOrder + DecideOrder）
**Code Review**: 内联 — APPROVED（Lean）
