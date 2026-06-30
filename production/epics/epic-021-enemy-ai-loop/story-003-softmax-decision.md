# Story 003: 种子化 softmax 选择 + DecisionRecord（看似随机实则可复现 + 错误信念可读）

> **Epic**: Enemy AI Loop（敌方 AI 循环 / M08）
> **Status**: Complete
> **Layer**: Core（敌方 AI Domain 内核）
> **Type**: Logic
> **Estimate**: M（4–6 h，新 Domain 内核）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-016-enemy-ai.md`
**Requirement**: `TR-ai-002`、`TR-ai-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0006: 确定性效用敌方 AI（primary，§1 种子 softmax）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 选择用**种子化 softmax 抽样**而非 argmax（看似随机、实则可复现）；温度由性格调；经注入 `IDeterministicRandom` 抽样，复用 ADR-0004 流不旁路。决策产 `DecisionRecord`（含缘由码），AI 错误信念对玩家复盘可读。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Core/Domain 层)**:
- Required: 随机复用注入 IDeterministicRandom（ADR-0004）；定点
- Forbidden: 新建 System.Random/UnityEngine.Random/时间依赖源；argmax 替代 softmax
- Guardrail: 同种子 → 同选择（确定性）；温度上升 → 分布趋平（单调性）

---

## Acceptance Criteria

*来自 GDD `gdd-016` §MVP + ADR-0006 §1，作用域限本 story：*

- [ ] `IActionSelector.Select(qualified ScoredAction[], temp, IDeterministicRandom)` 用种子化 softmax 抽样选动作
- [ ] 只在 `Feasible` 动作间抽样；**绝不**选出被可行性门淘汰的动作（TR-ai-003）
- [ ] 同种子 + 同评分 → 同选择（确定性，TR-ai-002）
- [ ] 温度上升 → 选择分布趋平（单调性：低温趋近 argmax，高温趋均匀）
- [ ] `EnemyAiService.Decide(...)` 产 `DecisionRecord`：含选中动作 + 缘由码 + 候选分数（AI 错误信念可读）
- [ ] 随机复用注入 `IDeterministicRandom`（不旁路 System.Random）；同 (种子+情报+性格+配置) → 同 DecisionRecord

---

## Implementation Notes

*来自 ADR-0006 §1 实现指引：*

- 新建 `IActionSelector` + `SoftmaxActionSelector`（softmax 概率 = exp(utility/temp) 归一，定点近似；经 `rng.NextUnit()` 抽样累积分布）+ `DecisionRecord`（selected + reason code + scored candidates）+ `EnemyAiService`（编排 Score → Select → Record）。
- 种子：`EnemyAiService.Decide` 接收注入 `IDeterministicRandom`（调用方按 ADR-0006 `seed = Hash(worldTick, factionId, planId)` 派生）；不在 Domain 内新建随机源。
- softmax 定点实现：用 ADR-0004 FixedPoint；温度调节分布锐度。仅 Feasible 候选参与。
- DecisionRecord 缘由码（`AiReasonCode`）：如 TopUtility（明显最优）/ SoftmaxJitter（效用接近抖动）/ PersonalityBias（性格主导）——使玩家复盘可读 AI 为何如此（含其错误信念）。

---

## Out of Scope

*由邻近 story / 后续处理——本 story 不实现：*

- Story 001/002：AiWorldView / 评分
- Story 004：接入战区命令
- OpponentModel 记忆 / LLM 装饰（裁断留后续）

---

## QA Test Cases

- **AC-1**: 同种子同选择（确定性）
  - Given: 两 `DeterministicRandom(seed=42)` + 同 ScoredAction[]
  - When: 各 `Select(...)`
  - Then: 选出同一动作
  - Edge cases: N/A

- **AC-2**: 绝不选被淘汰动作（TR-ai-003）
  - Given: ScoredAction[] 中 Pursue.Feasible=false（即便其 utility 最高）
  - When: `Select(...)` 多种子
  - Then: 任何种子都不选 Pursue
  - Edge cases: 仅一个 Feasible → 必选它

- **AC-3**: 温度单调性
  - Given: 同评分（一动作明显占优），低温 vs 高温，遍历多种子统计
  - When: 各温度多次 Select
  - Then: 低温下占优动作被选比例更高；高温下分布更平（趋均匀）
  - Edge cases: 温度→0 趋 argmax

- **AC-4**: Decide 产 DecisionRecord（错误信念可读）
  - Given: AiWorldView（敌情来自情报，可能误判）+ 性格 + 配置 + rng
  - When: `EnemyAiService.Decide(...)`
  - Then: DecisionRecord 含选中动作 + 缘由码 + 候选分数；可读 AI 依据（含其感知敌情）
  - Edge cases: N/A

- **AC-5**: Decide 确定性（复用注入流）
  - Given: 两次同 (种子+view+性格+配置)
  - When: 各 `Decide(...)`
  - Then: 两次 DecisionRecord 逐字段相同（选中 + 缘由 + 分数）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/EnemyAI/SoftmaxDecisionTests.cs` — 必须存在且全绿

**Status**: [x] `SoftmaxDecisionTests.cs` — 6/6 通过（764/764 全绿）

---

## Dependencies

- Depends on: Story 002 DONE（评分是选择输入）
- Unlocks: Story 004（接入战区命令读 DecisionRecord）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing
**Deviations**: 定点 softmax 风格温度加权抽样（避免定点 exp，保留温度单调 + 种子可复现性质），代码注释说明。
**Test Evidence**: `tests/unit/.../EnemyAI/SoftmaxDecisionTests.cs` — 6 tests
**新生产代码**: DecisionRecord + IActionSelector + SoftmaxActionSelector + EnemyAiService（缘由码判定）
**Code Review**: 内联 — APPROVED（Lean）
