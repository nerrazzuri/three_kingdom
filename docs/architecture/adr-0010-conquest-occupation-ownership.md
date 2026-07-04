# ADR-0010 — 占城归属契约（Conquest Occupation Ownership）

> **Status**: Proposed（2026-07-04，用户设计对话裁定；待 /architecture-decision 或 /architecture-review 转 Accepted 后 story 方可引用）
> **相关**: GDD_019 出征攻城 · epic-029 · ADR-0008（城池控制权唯一权威）· ADR-0006（种子化确定性随机）· ADR-0004（确定性）· ADR-0009（会话装配）

## Context（背景）

epic-029 引入"主动出征攻城"。攻城胜后有两件事要定权威：
1. **城池控制权转移**（敌 → 谁）——这已由 ADR-0008 规定：GDD_004 是城级控制权**唯一权威** + 唯一"控制权变更事件"。
2. **占领后归属**（该城归**君主直辖**还是**玩家直辖**）——这是**新问题**：用户裁定方案 C（前两座默认归玩家，第三座起君主种子化随机取舍），并要求"屡被收走→喂自立动机"。

归属结果影响谁直接治理该城、以及玩家自立倾向的累积——属**权威状态**，须确定性、可存档、可复现，不得引入旁路随机或全知。

## Decision（决策）

**D1 控制权转移复用 ADR-0008，不新增权威。**
攻城胜 → 敌城陷落 → 经 GDD_004 的**控制权变更事件**转移控制权。出征循环（Application 装配，沿 ADR-0009）**只发起/订阅**该事件，**不**自行改归属（R-5 装配层不拥规则）。

**D2 占领归属判定为新增确定性判定，输出 `OwnershipVerdict {GrantToPlayer | LordKeeps}`。**
- **前两座**玩家攻下的城（`conquestIndex < N`，默认 N=2）→ 恒 `GrantToPlayer`。
- **第三座起** → 种子化确定性伯努利：
  ```
  seed = Hash(worldTick, playerFactionId, cityId, conquestIndex)   // 注入式确定性流，ADR-0006 同款
  p_grant = clamp(base_grant + Σ w_i·factor_i, 0, 1)               // 名望/君主好感/城价值，定点，数据驱动
  verdict = seeded_bernoulli(seed, p_grant) ? GrantToPlayer : LordKeeps
  ```
- 判定**只读**玩家合法状态（名望/好感/城价值），**不**读敌方真值（反全知）。

**D3 归属落地路由。**
- `GrantToPlayer` → 控制权变更事件的新控制方 = 玩家势力；城进入玩家直辖清单（多城由 epic-025 委任打理）。
- `LordKeeps` → 新控制方 = 君主势力；玩家得功绩/名望/赏赐（GDD_014），**并累积自立倾向**（`rebellion_lean += lean_per_seizure`，喂 GDD_014 自立线触发）。

**D4 权威态 + 存档。**
`conquestIndex`、各城 `OwnershipVerdict`、`rebellion_lean` 为权威会话态，纳入统一存档信封（ADR-0009 R-1 / 存档段），存读档 round-trip 一致，进确定性哈希。

**D5 判定归属层。**
占领归属判定是**Domain 规则**（纯确定性函数：输入 conquestIndex + 玩家合法态 + 种子 → verdict），由 Application（出征编排）在控制权转移时调用；判定不依赖引擎、不旁路随机。

## Consequences（后果）

**正面**
- 复用 ADR-0008，城级控制权仍单一权威，无双写冲突。
- 种子化随机 → "君主取舍"不可预测却可复现（存读档、回放一致），守住 ADR-0004/0006。
- 自立倾向由"战果被夺"确定性累积 → 自立线动机**涌现自玩法**，非脚本触发。
- 前两座默认归玩家 = 明确的 onboarding 动力，规则简单可测。

**负面 / 代价**
- 玩家会真直辖多城 → 依赖 epic-025 委任机制扛住多城治理，否则微操膨胀（已知，M12 前置）。
- 归属权重（名望/好感/城价值）需平衡，否则"总被收走"或"总归玩家"都失张力 → 列入 Tuning + 平衡验证。

## Alternatives Considered（备选）

- **A（城归君主，玩家只得功绩）**：太守尺度纯粹，但玩家无"打天下"实感 → 用户否决（无动力）。
- **B（城全归玩家直辖）**：接近 4X，微操膨胀 + 弱化"忠臣受君主节制"的身份 → 否决。
- **C（本决策：前两默认归玩家 + 后续君主种子化取舍）**：兼顾启动动力、太守身份节制、自立张力 → 采纳。

## Compliance / Guardrails

- 禁旁路随机：归属随机**只**走注入式确定性流（ADR-0006），种子含 worldTick/faction/city/index。
- 反全知：判定不读敌方真值。
- 权威路径整数/定点（ADR-0004）；`p_grant` 定点 [0,1]。
- 装配层不拥规则（ADR-0009 R-5）：归属判定在 Domain，编排在 Application。

## Related

GDD_019 · GDD_004 · GDD_014 · GDD_015 · ADR-0004 · ADR-0006 · ADR-0008 · ADR-0009 · epic-029/025。
