# Epic: Enemy AI Loop（敌方 AI 循环 / M08）

> **Layer**: Core（敌方 AI Domain 内核——**从零开发**，非装配）+ Feature（接入战区命令）
> **GDD**: `design/gdd/gdd-016-enemy-ai.md`
> **Architecture Module**: M08 Enemy AI Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M08）
> **Governing ADR**: ADR-0006（确定性效用敌方 AI）· ADR-0004（确定性随机/哈希）· ADR-0009（接入会话）· ADR-0002（分层）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-aiworldview-antiomniscience.md) | AiWorldView 反全知锁（结构级拒真值）+ 战术动作候选 | Logic | ✅ Complete | ADR-0006/0002 |
| [002](story-002-action-scorer.md) | 效用评分 + 硬可行性门（态势 + 性格 + 环境） | Logic | ✅ Complete | ADR-0006/0004 |
| [003](story-003-softmax-decision.md) | 种子化 softmax 选择 + DecisionRecord（可复现 + 错误信念可读） | Logic | ✅ Complete | ADR-0006/0004 |
| [004](story-004-battle-command-integration.md) | 敌方 AI 决策接入战区命令（同源确定性 + 重放） | Integration | ✅ Complete | ADR-0006/0004/0009 |

## Overview

给兵法沙盒提供**可读、可骗、可复现**的对手——敌将有性格、有误判、有适应，但不是全知机器。**与前述 M03~M07 装配 epic 本质不同**：敌方 AI Domain 内核**尚未实装**，本 epic **从零开发** AI 决策层（基于已有 IntelProjection/PersonalityProfile/EnvironmentModifierSet/IDeterministicRandom/TacticRecognizer 组件）。按 ADR-0006 三支柱：种子化 softmax 效用选择（看似随机、实则可复现）、反全知锁（结构级——`AiWorldView` 构造拒真值类型，编译期杜绝偷看）、LLM 严格下游隔离（决策定局后才装饰，不入哈希）。按 GDD_016 §MVP「便宜 80% 先做」：`AiWorldView` + `ActionScorer`（3–4 动作）+ 种子 softmax + 接入战区命令 + 把 AI 错误信念暴露给玩家（复盘可读）；**暂不做** 多日 `StrategicPlan` 与跨战役 `OpponentModel`、LLM 装饰。

## Boundary（M08 范围裁断）

- **依赖已交付**：IntelProjection/IntelAssessment（epic-005）、PersonalityProfile（epic-003）、EnvironmentModifierSet（epic-002）、IDeterministicRandom/DeterministicRandom（epic-001）、TacticRecognizer（epic-007）、M06 战斗循环（epic-019，敌方命令的消费方）。
- **M08（本 epic）新增**（**新 Domain 内核**，区别于前述装配）：
  1. `AiWorldView`（反全知锁，结构级——构造只接 IntelProjection/IntelAssessment/己方/环境/目标，拒真值类型）。
  2. `StrategicAction` 候选（追击/撤退/坚守/诱敌 3–4 个）。
  3. `IActionScorer`/`ActionScorer`（效用评分：态势 + 性格，硬可行性门淘汰）。
  4. `IActionSelector`/种子化 softmax 选择（复用 ADR-0004 注入随机流，温度由性格调）。
  5. `DecisionRecord`（含缘由码——AI 错误信念对玩家复盘可读）。
  6. 接入战区命令（敌方 AI 决策 → 战斗命令，纳入战斗哈希/重放）。
- **裁断（GDD_016 §MVP + module-plan 风险「先做战术层便宜 80%」）**：
  - **不做** 多日 `StrategicPlan`（战略层）、跨战役 `OpponentModel`（记忆 EWMA）、`ILlmNarrator` 实现 → 留后续 epic（ADR-0006 已留接口）。
  - LLM 隔离：MVP 仅产出 `DecisionRecord`（缘由码）；`ILlmNarrator` 端口可空，不实现装饰。

## 关键护栏（风险）

> module-plan §M08 风险：**先做战略 AI 会膨胀；应先做战术层便宜 80%**。ADR-0006 风险：偷看真值 / 不可复现 / LLM 越权。
- **反全知锁结构级**（ADR-0006 §2 / TR-ai-001）：`AiWorldView` 构造**编译期**拒 MapTruth/WorldTruth——误判由按错误情报行动天然涌现，非约定。
- **确定性不旁路**（ADR-0006 §1 / ADR-0004 / TR-ai-002）：AI 随机复用注入 `IDeterministicRandom`，不新建 System.Random/UnityEngine.Random/时间依赖源；纳入同一状态哈希与重放。
- **可行性门**（TR-ai-003）：AI 绝不选被淘汰动作；抖动只在效用接近的动作间（防随机送人头显蠢）。
- **战术层优先**：不做战略/记忆/LLM，避免膨胀。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0006: 确定性效用敌方 AI | 种子化 softmax 效用选择 + 反全知锁（结构级）+ LLM 下游隔离 + 渐进记忆 | LOW |
| ADR-0004: 确定性 | 整数/定点 + 注入随机流 + 状态哈希；AI 随机与战斗同源 | LOW |
| ADR-0009: CampaignSession 装配边界 | AI 决策接入战区命令经会话/战斗路径，不旁路 | LOW |
| ADR-0002: 分层 | AI 在 Domain 层（纯 C#，无 UnityEngine） | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-ai-001 | 反全知锁（结构级）：AiWorldView 构造只接阵营知识投影 + 评估 + 己方/环境/目标，拒真值类型（编译期杜绝） | ADR-0006 ✅ |
| TR-ai-002 | 种子化 softmax 确定性：同(种子+情报+性格+配置)→同 DecisionRecord+哈希；同种子→同选择；温度↑→分布趋平；随机复用 ADR-0004 注入流 | ADR-0006/0004 ✅ |
| TR-ai-003 | 效用含硬可行性门：绝不选被淘汰动作；抖动只在效用接近动作间；DecisionRecord 缘由码使错误信念复盘可读 | ADR-0006 ✅ |
| TR-ai-004 | 敌方 AI 决策接入战区命令：AI 随机与战斗同源，纳入同一哈希与重放；同种子+同态势→同战区命令流 | ADR-0006/0004/0009 ✅ |

> 注：TR-ai-001~004 于 2026-06-30 补登（M08 敌方 AI；前无 TR-ai slug）。无 untraced requirement。

## Scope

### In Scope
- `AiWorldView`（反全知锁，结构级拒真值）+ `StrategicAction` 候选（3–4 动作：如追击/撤退/坚守/诱敌）。
- `IActionScorer`/`ActionScorer`（效用：态势 + 性格 + 环境，硬可行性门淘汰）+ `ScorerConfig`（数据驱动权重）。
- `IActionSelector`/种子化 softmax 选择（注入 `IDeterministicRandom`，温度由性格调；同种子同选择，温度单调性）。
- `DecisionRecord`（选中动作 + 缘由码 + 候选分数，错误信念可读）。
- 接入战区命令：敌方 AI 决策 → `BattleOrder`，纳入战斗哈希/重放（同种子+同态势→同命令流）。

### Out of Scope
- 多日 `StrategicPlan`（战略层）、跨战役 `OpponentModel`（记忆 EWMA）→ 后续 epic（ADR-0006 已留接口）。
- `ILlmNarrator` 装饰实现（MVP 仅产 DecisionRecord；端口可空）。
- 扩 `PersonalityProfile` 全量 AI 倾向（仅接 MVP 所需倾向；其余渐进）。
- 新 Unity scene / 新 UI。

## Definition of Done

This epic is complete when:
- `AiWorldView` 构造编译期拒真值（反全知锁，TR-ai-001）。
- 种子化 softmax 确定：同 (种子+情报+性格+配置) → 同 DecisionRecord + 哈希；温度单调性（TR-ai-002）。
- 效用可行性门：AI 不选被淘汰动作；DecisionRecord 缘由码可读（TR-ai-003）。
- 敌方 AI 决策接入战区命令，纳入战斗哈希/重放（TR-ai-004）。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M07 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-021-enemy-ai-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
