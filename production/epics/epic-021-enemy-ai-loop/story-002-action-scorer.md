# Story 002: 效用评分 + 硬可行性门（态势 + 性格 + 环境）

> **Epic**: Enemy AI Loop（敌方 AI 循环 / M08）
> **Status**: Complete
> **Layer**: Core（敌方 AI Domain 内核）
> **Type**: Logic
> **Estimate**: M（4–6 h，新 Domain 内核）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-016-enemy-ai.md`
**Requirement**: `TR-ai-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0006: 确定性效用敌方 AI（primary，§1 效用评分）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 对每个候选 `StrategicAction` 算定点效用分（态势 + 性格 + 环境叠加），不可行动作过**硬可行性门**淘汰。抖动只发生在效用接近的动作之间（防随机送人头显蠢）。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Core/Domain 层)**:
- Required: 定点运算（ADR-0004，权威路径禁 float）；评分确定性
- Forbidden: 硬编码效用权重（来自 ScorerConfig 数据驱动）；性格提供无条件效果
- Guardrail: 同输入 → 同评分（纯函数）

---

## Acceptance Criteria

*来自 GDD `gdd-016` §MVP + ADR-0006 §1，作用域限本 story：*

- [ ] `IActionScorer.Score(AiWorldView, PersonalityProfile, ScorerConfig)` 对每个候选动作算定点效用分
- [ ] 效用由态势（敌我对比/目标压力）+ 性格（风险/耐心等倾向）+ 环境叠加，权重来自 `ScorerConfig`（数据驱动）
- [ ] 硬可行性门：不可行动作（如无敌可追时的追击）标记 `Feasible=false`，被淘汰（不进入选择）
- [ ] 评分确定性：同 (view, personality, config) → 同 `ScoredAction[]`（定点，无 float 漂移）
- [ ] 性格影响评分：高风险性格提升进攻类（追击/诱敌）效用，高耐心提升坚守效用（差异可观测）

---

## Implementation Notes

*来自 ADR-0006 §1 实现指引：*

- 新建 `IActionScorer` + `ActionScorer`（实现）+ `ScoredAction`（action + utility FixedPoint + feasible bool + reason code）+ `ScorerConfig`（数据驱动权重）。
- 效用 = Σ(权重 × 因子)，因子取自 AiWorldView（敌我兵力比、目标压力、环境修正）+ PersonalityProfile.Strength(trait)。
- 可行性门：按动作前置（如 Pursue 需敌可追、Retreat 需有退路）判 Feasible；不可行不淘汰出 ScoredAction 但标记，选择阶段只取 Feasible。
- 全定点（FixedPoint），复用 PersonalityProfile（epic-003）。

---

## Out of Scope

*由邻近 story / 后续处理——本 story 不实现：*

- Story 001：AiWorldView（本 story 依赖其完成）
- Story 003：种子 softmax 选择 + DecisionRecord
- Story 004：接入战区命令
- OpponentModel 记忆偏置（裁断留后续）

---

## QA Test Cases

- **AC-1**: 评分产出每动作效用
  - Given: AiWorldView + PersonalityProfile + ScorerConfig
  - When: `Score(...)`
  - Then: 返回每候选动作的 ScoredAction（含 utility + feasible）
  - Edge cases: N/A

- **AC-2**: 硬可行性门淘汰不可行动作
  - Given: 无敌可追的态势（敌情缺失/敌已撤）
  - When: `Score(...)`
  - Then: Pursue 的 `Feasible == false`
  - Edge cases: 全部不可行 → 至少保留一个兜底可行动作（如 Hold）

- **AC-3**: 评分确定性
  - Given: 两次同 (view, personality, config)
  - When: 各 `Score(...)`
  - Then: 两次 ScoredAction 的 utility 逐位相同（定点无漂移）
  - Edge cases: N/A

- **AC-4**: 性格影响效用
  - Given: 高风险性格 vs 高耐心性格，同态势
  - When: 各 `Score(...)`
  - Then: 高风险者 Pursue/FeintLure 效用 > 高耐心者；高耐心者 Hold 效用更高
  - Edge cases: 中性性格 → 基线效用

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/EnemyAI/ActionScorerTests.cs` — 必须存在且全绿

**Status**: [x] `ActionScorerTests.cs` — 5/5 通过（764/764 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（AiWorldView 是评分输入）
- Unlocks: Story 003（选择读 ScoredAction）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 无（全定点效用 + 硬可行性门；坚守恒兜底可行；性格只调权重）。
**Test Evidence**: `tests/unit/.../EnemyAI/ActionScorerTests.cs` — 5 tests
**新生产代码**: ScoredAction + ScorerConfig + IActionScorer + ActionScorer
**Code Review**: 内联 — APPROVED（Lean）
