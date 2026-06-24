# ADR-0006：确定性效用敌方 AI（种子化 softmax + 反全知锁 + LLM 隔离）

## Status

Proposed

## Date

2026-06-24

## Last Verified

2026-06-24

## Engine Compatibility

| 字段 | 值 |
|------|-----|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — AI 决策为纯 Domain C#，不依赖 Unity 数值/物理；LLM 经 Infrastructure 端口，离线可降级 |
| **References Consulted** | `docs/architecture/adr-0004-deterministic-battle-simulation.md`、`adr-0002-architecture-layering.md`、`adr-0005-save-versioning-migration.md`、`design/gdd/gdd-016-enemy-ai.md`、`gdd-007-intelligence-recon.md` |
| **Post-Cutoff APIs Used** | None — Domain 不调用 Unity；随机用注入式 `IDeterministicRandom`，非 UnityEngine.Random |
| **Verification Required** | 三平台对同一种子+情报+配置产生逐位相同的 `DecisionRecord` 与状态哈希；LLM 关闭时哈希不变 |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0002（分层）、ADR-0004（确定性：定点+注入随机+状态哈希）、ADR-0003（数据驱动配置）、ADR-0005（存档版本，AI 状态入存档 DTO） |
| **Enables** | GDD_016 敌方 AI 实现；GDD_010 自由布阵的“敌人会反应”深度 |
| **Blocks** | 任何 AI 决策 story——本 ADR Accepted 前不得开始效用评分/选择实现 |
| **Ordering Note** | 与 GDD_016 平行；复用 ADR-0004 的 FixedPoint/IDeterministicRandom/StateHasher，不另立随机机制 |

## Context

### Problem Statement

GDD_016 要求敌人“不容易被摸透”却仍**确定性、可测试、可存档复现**，且允许大模型生成台词/解释而**不破坏游戏完整性**。三处张力需架构裁定：① 不可预测性如何与“同种子可复现”共存；② 如何结构性地保证 AI **不全知**（不偷看真值）；③ LLM 如何参与而**绝不**影响胜负或状态哈希。若用 argmax 选择，行为被玩家背诵；若 AI 读世界真值，则“被欺骗/误判”退化为“开挂”；若 LLM 输出回写 Domain，则离线不可玩且破坏确定性。

### Constraints

- 技术约束：复用 ADR-0004——权威路径仅定点、随机仅注入流、状态入哈希；禁 `float`/`System.Random`/`UnityEngine.Random`。
- 架构约束：AI 决策落 Domain 层；LLM 经 Infrastructure 端口，Presentation 消费，不进 Domain（ADR-0002）。
- 设计约束：AI 与玩家共享规则、不获强制中计脚本（GDD_010 §AI）；记忆反制须渐进、不瞬间开挂（GDD_016）。

### Requirements

- 同 `(种子, 情报, 性格, 计划, 记忆, 配置)` → 同 `DecisionRecord` 与状态哈希，跨平台逐位一致。
- AI 只读自己的 `FactionIntel`（GDD_007），无任何真值入口。
- 评分/选择/计划/记忆纳入状态哈希与存档 DTO（ADR-0004/0005）。
- LLM 输出不入哈希、不回写 Domain、不决胜负；离线/失败游戏逻辑不受影响。

## Decision

确定性效用敌方 AI 建立在三根支柱上：**种子化 softmax 效用选择**、**反全知锁**、**LLM 严格下游隔离**；数值/随机/哈希全部复用 ADR-0004。

### 1. 效用评分 + 种子化 softmax 选择（不可预测且可复现）

- 对每个候选 `StrategicAction` 算定点效用分（态势 + 性格 + 计划 + 记忆叠加），不可行动作过硬可行性门淘汰。
- 选择用**种子化 softmax 抽样**而非 argmax：`seed = Hash(worldTick, factionId, planId)`，温度由性格（欺骗/适应力）调节，经注入 `IDeterministicRandom` 抽样。
- 效果：行为“看似随机、实则可复现”。抖动**只**发生在效用接近的动作之间，不在“明显优 vs 明显劣”之间（防止随机送人头显蠢）。

### 2. 反全知锁（结构级，非约定）

```csharp
// 构造签名只接受阵营知识投影 + 评估，绝不接受 MapTruth/WorldTruth
sealed class AiWorldView {
    AiWorldView(IntelProjection ownIntel, IntelAssessment assessment,
               OwnForceSnapshot own, EnvironmentModifierSet env, ObjectivePressure obj);
}
```

- AI 的唯一敌情入口是 `AiWorldView`，其构造**不接受任何真值类型**——编译期即杜绝偷看。误判由“按错误情报行动 vs 真值不同”天然涌现。

### 3. LLM 严格下游隔离

```
Domain 决策（确定性）→ DecisionRecord（含缘由码）→ [Infrastructure: ILlmNarrator] → Presentation 文本
                                                  ↑ 只读，不回写；输出不入 StateHasher
```

- LLM **只**读已成定局的 `DecisionRecord` 产出台词/战报/解释；不得改状态、不得决胜负、不入哈希。端口层丢弃任何越权（试图改状态）的输出。
- LLM 不可用/超时 → 跳过装饰层，Domain 与哈希完全不变。

### 4. 渐进记忆（复用 TacticRecognizer）

- `OpponentModel` 以 GDD_010 `TacticRecognizer` 的**事后标签**累积玩家套路 EWMA 频率；反制偏置仅在 `样本数≥N ∧ 时间跨度≥M` 时非零，且随时间衰减。入哈希与存档。

### Key Interfaces

```csharp
interface IActionScorer { ScoredAction[] Score(AiWorldView v, PersonalityProfile p,
                                                OpponentModel m, StrategicPlan plan, ScorerConfig c); }
interface IActionSelector { StrategicAction Select(ScoredAction[] qualified, FixedPoint temp,
                                                   IDeterministicRandom rng); }
interface ILlmNarrator { string Narrate(DecisionRecord r); }   // Infrastructure 端口，可空实现
```

## Alternatives Considered

### Alternative 1：行为树 + argmax

- **描述**：固定 if-else/行为树，取最高分动作。
- **Pros**：实现简单、易调试。
- **Cons**：行为可被玩家背诵（“摸透”）；阈值树脆且难表达性格差异。
- **Rejection Reason**：违背 GDD_016 “不容易被摸透”，argmax 可预测。

### Alternative 2：LLM 直接决策

- **描述**：大模型直接输出 AI 行动。
- **Pros**：行为丰富、自然语言可解释。
- **Cons**：非确定、不可复现、离线不可玩、可能输出非法动作或被提示注入；破坏状态哈希与存档复现。
- **Rejection Reason**：违背确定性/可测试/可存档红线；LLM 只能装饰，不能决策。

### Alternative 3：AI 读世界真值 + 人为致盲

- **描述**：AI 读真值，再按规则“假装不知道”。
- **Pros**：实现省去情报建模。
- **Cons**：致盲是约定，易被绕过/泄露，“被欺骗”退化为“开挂感”。
- **Rejection Reason**：反全知必须结构级（构造不接受真值），不能靠自律。

## Consequences

### Positive

- 行为不可预测却可复现，兼顾“难摸透”与“存档回放”。
- 反全知是编译级保证，玩家的假退/侦察/断粮能真正欺骗 AI。
- LLM 隔离使游戏离线完整可玩、确定性不受影响。
- AI 决策可逐缘由码复盘，且为 LLM 解释提供约束（防幻觉矛盾）。

### Negative

- 效用权重调参成本高（多权重产出“多样而不蠢”的行为是主要工作量）。
- 调参改权重可能破坏老存档哈希复现——须把权重做成版本化配置并锁配置指纹（缓解见下）。

### Risks

- **风险**：softmax 温度过高致 AI 随机犯蠢。**缓解**：抖动限于接近分之间；温度上限玩测校准。
- **风险**：`float`/非注入随机渗入。**缓解**：复用 ADR-0004 静态检查 + 三平台哈希测试。
- **风险**：记忆样本不足（短战役）反制不生效。**缓解**：接受“宁可不反制也不开挂”；持久化为 Future Scope 决策。
- **风险**：LLM 越权改状态。**缓解**：端口层只读 DecisionRecord，输出丢弃任何状态变更意图。

## GDD Requirements Addressed

| GDD 系统 | 需求 | 本 ADR 如何满足 |
|---|---|---|
| 敌方 AI (GDD_016) | 不全知 / 不可预测 / 渐进记忆 / LLM 仅装饰 | 反全知锁 + 种子 softmax + 门槛记忆 + LLM 下游隔离 |
| 兵法战斗 (GDD_010) | AI 共享规则、无强制中计、可识破反制 | 效用评分按条件判定追击/识破，不写死中计脚本 |
| 情报 (GDD_007) | AI 只用自身知识 | AiWorldView 仅接受 IntelProjection/Assessment |
| 确定性 (ADR-0004) | 定点 + 注入随机 + 状态哈希 | 全程复用，无新增随机机制 |
| 存档 (ADR-0005) | AI 状态可复现 round-trip | 计划/记忆/随机流位置入存档 DTO |

## Performance Implications

- **CPU**：评分为少量候选动作的定点加权，回合/阶段制无每帧压力，远在预算内。
- **Memory**：DecisionRecord/OpponentModel 极小。
- **Load Time**：忽略。
- **Network**：LLM 若用云端为可选异步，超时降级；离线为默认可玩态。

## Migration Plan

当前无 AI 源码。按 GDD_016 §MVP“便宜 80%”先实现：PersonalityProfile 扩项 → AiWorldView → ActionScorer → 种子 softmax → 接战区命令 → 暴露错误信念给玩家。多日计划与跨战役记忆为后续。

## Validation Criteria

- [ ] 同 (种子+情报+性格+计划+记忆+配置) 三平台产生逐位相同 DecisionRecord 与状态哈希
- [ ] `AiWorldView` 构造无法传入任何真值类型（编译失败即通过）
- [ ] 关闭/拔除 LLM 端口，游戏逻辑与状态哈希完全不变
- [ ] 同种子→同选择；温度上升→分布趋平（单调性）
- [ ] 记忆样本未达门槛时 CounterBias 恒 0；达门槛后随时间衰减
- [ ] AI 不会选出被可行性门淘汰的动作

## Related Decisions

- ADR-0002：架构分层（AI 决策落 Domain，LLM 落 Infrastructure 端口）
- ADR-0004：确定性模拟（复用定点/注入随机/状态哈希）
- ADR-0005：存档版本与迁移（AI 状态入版本化 DTO）
- GDD_016：敌方 AI 系统规格；GDD_007：情报；GDD_010：兵法战斗
