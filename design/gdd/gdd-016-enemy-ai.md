# GDD_016 — 敌方 AI（离线确定性效用 AI）

- 状态：Reviewed（跨系统审查 2026-06-24 通过，Warning 已落；ADR-0006 已 Accepted）
- **Status**: Reviewed
- 范围：战术层（战区内敌将决策）为主；战略层（势力态势）随 GDD_015 触及边界启用
- 关联：[ADR-0006](../../docs/architecture/adr-0006-deterministic-enemy-ai.md)

## System Purpose

让敌人在战区里**对玩家的布局做出反应**——你拉它它会不会追、你藏伏兵它能不能侦破、你分兵袭粮它会不会回防——从而让 GDD_010 的“自由布阵”有真正的深度（否则退化为按既定解法点一遍的剧本谜题）。AI **不全知**、有**性格**、会**记套路**、能**多日谋划**，使敌人“不容易被摸透”，但全程**确定性、可测试、可存档复现**。大模型**只**用于台词/战报/解释，**不**改游戏状态、不决胜负。

## Player Fantasy

“这个敌将不是一台增兵时钟。它会被我的假退骗、也可能识破不追；它知道的情报是错的、会犯我能利用的错；它见我老用夜袭，几仗后开始夜里加岗——但它没有偷看我的牌。”

## Core Loop

读取 AI 自己的情报（不全知）→ 对候选行动效用评分（受性格/计划/记忆调制）→ 种子化抽样选定行动 → 翻成战区基础命令执行 → 观察战果、更新记忆与计划 → 下一阶段。

## Main Rules

- **两层 AI：**
  - **战术层（本 GDD 主体）**：战区内**敌将**决定 强攻/围困/侦察/断粮/佯攻/撤退/求援 等，并下达 GDD_010 基础命令。
  - **战略层（随 GDD_015 启用）**：玩家**够得着**范围内的敌方**势力**是否来夺城/追击/结盟；够不着的范围按历史抽象推进，不跑本 AI。
- **不全知**：AI **只读自己的** `FactionIntel`（GDD_007），**永不**读世界真值。误判天然涌现——按错误情报行动，真值不同就会犯错。此为架构级锁（见 ADR-0006）。
- **效用评分而非 if-else 阈值**：对每个候选行动算效用分，分由态势 + 性格 + 计划 + 记忆叠加；不可行动作过硬可行性门被淘汰。
- **种子化抽样选择（非 argmax）**：在合格动作上做**种子化 softmax 抽样**，温度由性格（欺骗/适应力）调节。使行为“看似随机、实则可复现”——兼顾“不易被摸透”与“可存档复现”。argmax 会被玩家背出，禁用作主选择。
- **性格**：每个敌将一份 `PersonalityProfile`（GDD_005），含攻击性/谨慎/耐心/欺骗/适应力等倾向 [-1,1]，**只改判断权重**，不给无条件战斗光环。
- **多日计划**：AI 持有 `StrategicPlan`（如“围而断粮”），带承诺期（防回合制短视翻烧饼）与继续/放弃条件；放弃条件触发才改计划。
- **记忆与渐进反制**：AI 用 GDD_010 `TacticRecognizer` 的**事后标签**累积玩家套路频率（假退/夜袭/断粮）；达**样本门槛**后才生成反制偏置，且**随时间衰减**（玩家停用即遗忘）。**不瞬间开挂**。
- **LLM 装饰层（可选、最外圈）**：只读已成定局的 `DecisionRecord` 生成台词/战报/军师解释/敌将理由；**不回写 Domain、不入状态哈希、不决胜负**；离线/失败时游戏逻辑完全不受影响。
- **共享规则**：AI 与玩家共享命令、侦测、移动、战斗与后果规则，不获强制中计脚本。

## Formulas

> 全程定点（ADR-0004），禁 float；随机**仅**经注入的 `IDeterministicRandom`，种子由世界状态派生。评分/选择/计划/记忆纳入状态哈希与存档（ADR-0005）。详见 ADR-0006。

### 变量定义

| 变量 | 含义 | 范围 | 来源 |
|---|---|---|---|
| `believedEnemy` | 性格扭曲后的敌兵力估计 | ≥ 0 | AiWorldView ← FactionKnowledge(GDD_007) |
| `EffectiveConfidence` | 情报有效置信 | [0,1] | GDD_007 `effective_conf`（IntelAssessment 实现） |
| `Aggression/Caution/...` | 性格倾向 | [-1,1] | PersonalityProfile |
| `U(action)` | 行动效用分 | 定点 | ActionScorer |

### 1. 出兵（强攻）效用示例

```
tBelief        = clamp(0.5 − 0.5·Caution + 0.5·Aggression, 0, 1)   # 插值参数钳到 [0,1]，防极端性格外推出区间（含负值）
believedEnemy = lerp(interval.Low, interval.High, tBelief)
forceRatio    = OwnStrength / max(believedEnemy, 1)
ratioScore    = clamp((forceRatio − BreakEven) / Spread, −1, 1)
confPenalty   = (1 − EffectiveConfidence) · (0.5 + 0.5·Caution)
selfSupplyUrg = supplyDaysLeft < LowSupplyDays ? PushNow : 0
persBias      = w_aggr·Aggression − w_caut·Caution
planBias      = (plan.Intent == SiegeAndStarve) ? −DiscourageAssault : 0
memBias       = OpponentModel.CounterBias(Assault)      # 达样本门槛才非零

U(Assault) = w_ratio·ratioScore + persBias + w_dead·deadlineUrg
           + selfSupplyUrg − w_conf·confPenalty + planBias + memBias

if (¬Mustered ∨ ¬PathExists ∨ TruceForbids) U(Assault) = DISQUALIFIED
```

### 2. 种子化选择

```
seed = Hash(worldTick, factionId, planId)
temperature = BaseTemp · max(TempFloor, 1 + Deception + Adaptability)   # 温度乘子钳到正下限 TempFloor(>0)，防性格[-1,1]下乘子≤0 致 softmax 反转/退化
chosen = SoftmaxSample(qualifiedActions, temperature, IDeterministicRandom(seed))
```

### 3. 记忆反制（渐进、可遗忘）

```
freq[tag] = EWMA(freq[tag], observed_this_battle)         # 由 TacticRecognizer 标签喂入
CounterBias(action) = (samples[tag] ≥ N ∧ span ≥ M) ? bias(freq[tag]) · decay(age) : 0
```

## Data Model

- `AiWorldView`：AI 的“我以为的世界”——由 GDD_007 `FactionKnowledge`（AI 自身阵营知识）+ `effective_conf` 置信评估构造，**签名不接受任何真值类型**（反全知锁，GDD_007 §AI Requirements「AI 只读自身 FactionKnowledge、永不读真值」）；含 believedEnemy/置信/区间、自有兵力补给、环境、目标压力。
- `StrategicAction`：枚举（强攻/围困/侦察/断粮/佯攻/撤退/求援）。**边界（与 GDD_010 §7）**：`ActionScorer` 只决定敌方**想不想**采取某意图（含追击），翻成 GDD_010 基础命令后，"追击是否真的成形"由 **GDD_010 §7 追击决策公式唯一结算**；本 AI 不重复判定追击成败。
- `ScoredAction` / `DecisionRecord`：效用分 + 缘由码 + 是否淘汰；最终选择 + 种子（供 UI 与 LLM 读）。
- `StrategicPlan`：意图、起始时间、承诺期、放弃/继续条件。
- `OpponentModel`：每种玩家套路标签的 EWMA 频率 + 样本数 + 末次时间。

## Player Inputs

无直接输入；玩家通过战区布局（GDD_010）间接影响 AI 的情报、计划与记忆。

## System Outputs

AI 的战区基础命令（GDD_010）；`DecisionRecord`（含缘由码，供复盘与 LLM）；记忆/计划更新；战略层势力行动（GDD_015 触及范围内）。

## Dependencies

依赖 GDD_007（情报：AI 唯一敌情来源）、GDD_005（性格）、GDD_010（战区命令/TacticRecognizer 标签）、GDD_011/012（士气补给态势）、GDD_015（势力态势）、Numerics（FixedPoint/IDeterministicRandom/StateHasher）。LLM 装饰层经 Infrastructure 端口（不在 Domain）。

## Edge Cases

- 情报严重过期：区间变宽、置信低，谨慎将更保守、激进将更冒进——误判即设计内涌现。
- 多个动作效用接近：softmax 抽样产生抖动（看似难测），但同种子复现；抖动只在接近分之间，不在“明显优 vs 明显劣”之间。
- 记忆样本不足（短战役）：CounterBias 恒 0，AI 不反制——优于“样本不够也开挂”。
- LLM 不可用/超时：跳过装饰层，游戏逻辑与状态完全不变。

## Failure Cases

AI 选出不可行动作 → 可行性门应已淘汰；若仍发生则记错误并回退到“坚守/撤退”安全动作，不崩。LLM 输出异常/越权（试图改状态）→ 端口层丢弃，不影响 Domain。

## Balancing Parameters

各效用权重 `w_*`、BreakEven/Spread、置信惩罚系数、性格偏置权重、softmax BaseTemp、**TempFloor（温度乘子正下限，>0，防退化）**、记忆样本门槛 N/时间跨度 M/衰减曲线、计划承诺期。**调参期权重做成版本化配置并锁配置指纹**，避免“调参=改代码=破坏老存档复现”。

## AI Requirements

本 GDD 即 AI 规格。核心红线：① 只读自己情报；② 仅定点 + 注入随机；③ 评分/选择/计划/记忆入状态哈希与存档；④ LLM 严格下游、不入哈希、不决胜负。

## Save / Load Requirements

保存 `StrategicPlan`、`OpponentModel`、随机流位置、当前 `DecisionRecord` 引用；round-trip 一致；与战役/世界同一存档边界（GDD_013）。LLM 文本不入权威存档（可缓存于展示层）。

## Test Requirements

同输入同评分（表驱动）；同种子同选择 + 温度单调性；不全知锁（`AiWorldView` 构造拒绝真值类型，编译级）；记忆门槛前不反制、衰减正确；计划承诺期内不翻盘、放弃条件各自可验；LLM 离线游戏正常且哈希不变。

## MVP Scope

**便宜 80% 先做**：扩 `PersonalityProfile` 倾向 + `AiWorldView` + `ActionScorer`（3–4 个动作）+ 种子 softmax + 接入战区命令，并**把 AI 的错误信念暴露给玩家**（复盘可读）。暂不做多日 `StrategicPlan` 与跨战役 `OpponentModel`。

## Future Scope

多日计划与跨战役渐进反制记忆；战略层势力 AI（GDD_015 触及范围）；更丰富的 LLM 解释（受缘由码约束，防幻觉矛盾）；联军协同 AI。

## Open Questions

- softmax 温度上限多少才“难测而不蠢”？需玩测校准。
- 记忆是否跨战役持久化（短战役样本不足问题）？持久化则需防“开挂感”的衰减与门槛调参。
- AI 的错误信念以何种 UI 暴露给玩家最自然（军师解释 / 战报 / 复盘标签）？
