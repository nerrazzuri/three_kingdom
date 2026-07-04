# GDD_020 — 人才招揽（Talent Recruitment）

> **Status**: Draft（2026-07-04；epic-030 设计锚点。守本作两魂：反全知 + 条件涌现/无胜率。待 /review-all-gdds。）
> **Epic**: epic-030-talent-recruitment-loop
> **关系**: 复用 GDD_005 人物 / GDD_006 关系 / GDD_007 情报（反全知） / GDD_015 条件历史（登场时间窗） / ADR-0006 种子化随机 / GDD_014 生涯僚属 / GDD_019/021 出征战斗（人才为将）。本文只定义"把人才连成可招揽循环"的规则。

## 1. System Purpose（系统目的）

给玩家**主动搜寻、招揽武将/文臣**的能力，但**不做三国志式全知人才列表 + 掷骰入伙**。三层守魂：**何时出现随历史**（时间窗）· **能否知道靠情报**（反全知）· **愿不愿跟靠条件**（名望/官职/关系/待遇/志向 + 种子化随机，人各有志）。招来的人才喂给战斗（为将带兵/兵法/单挑）与生涯（内政/外交/情报/更大编制）。

## 2. Player Fantasy（玩家幻想）

「我听闻荆襄有卧龙——但 184 年他还是个种地的少年，急不得。待时局到了，我遣人访贤、托部曲人脉打听，才摸到他的下落。请他出山，靠的是我的名望、许他的官、与他的交情，还有他自己出仕的心思——不是我掷个骰子他就来。有的贤才你请三次也不来，有的一见如故。」

## 3. Core Loop（核心循环）

```
历史推进（GDD_015）→ 人才按时间窗登场（未到窗不存在）
  → 知晓：侦察/军师/部曲人脉/历史事件 带出 → 进入玩家视野（未听说的不可见，反全知）
  → 招揽：备名望/官职/关系/待遇 → 条件 + 种子化随机判定是否出仕（可复现·不可预测·非掷骰）
  → 入伙：进僚属（GDD_014）→ 喂战斗（为将）+ 生涯（能力/编制）
```

## 4. Main Rules（主要规则）

**R1 出现随历史（时间窗）**：每位人才有登场时间窗（起始世界时间，可选终止）。未到起始窗 → **世界中不存在**（不可知、不可招）。经 GDD_015 条件历史时间推进解锁。数据驱动、确定性。

**R2 知晓靠情报（反全知）**：人才须经某**知晓渠道**（侦察 / 军师举荐 / 部曲人脉 / 历史事件带出）才进入玩家视野。**未知晓的人才不在玩家可见面**——类型层玩家投影取不到未知人才真值（GDD_007 反全知锁延续）。**无全知人才列表。**

**R3 招揽靠条件 + 种子化随机（无胜率）**：对已知晓且已登场的人才发起招揽，是否出仕由 `p_join`（名望/官职/关系/待遇/该人出仕志向的条件式）+ **种子化确定性判定**（ADR-0006 同款注入流）决定。同条件 + 同种子 → 同结果（可复现）；**不显示胜率数字**；有的贤才屡请不来（人各有志）。

**R4 喂给战斗/生涯**：入伙人才 → 进僚属花名册（GDD_014 RetinueState）→ ① **战斗**：可为将统兵（复用出征/区域战斗 `OffensiveGeneral` 统率/武勇/智略），带兵法加成；② **生涯**：提供内政/外交/情报能力、支撑更大编制。

**R5 反悔与流失**：招揽失败可再试（待遇/关系变化后 p_join 变）；已入伙者关系恶化可能求去（后续，MVP 记为 Future）。

## 5. Formulas（公式）

- **F1 登场门**：`appeared(talent, t) = t ≥ talent.AppearFrom ∧ (talent.AppearUntil = ∅ ∨ t ≤ talent.AppearUntil)`。
- **F2 招揽概率**（定点 [0,1]，权威路径整数/定点 ADR-0004）：
  ```
  p_join = clamp( base_will + w_renown·名望norm + w_office·官职norm + w_relation·关系norm + w_offer·待遇norm
                  − reluctance(该人出仕志向门槛) , 0 , 1 )
  ```
- **F3 种子化判定**：`seed = Hash(worldTick, talentId, playerFactionId, attemptIndex)`；`joined = DeterministicRandom(seed).NextUnit() < p_join`（注入式确定性流，ADR-0004/0006；非掷骰，可复现）。`attemptIndex` 使多次尝试各有独立确定性结果。

## 6. Data Model（数据模型）

- `TalentId`（稳定 id）· `TalentProfile`（id + 统率/武勇/智略 Q16.16 + 专长 + 出仕志向 `Willingness` + 招揽阻力 `Reluctance` + 登场时间窗 AppearFrom/AppearUntil）。
- `TalentRoster`（人才目录，数据驱动，含各人时间窗）。
- `TalentKnowledge`（玩家已知晓人才集 + 知晓渠道）· `TalentChannel { Scouting | Council | RetinueNetwork | HistoricalEvent }`。
- `RecruitmentOffer`（名望norm/官职norm/关系norm/待遇norm，定点 [0,1]）。
- `TalentRecruitmentConfig`（base_will + 各权重 + 温度/阻力，数据驱动）。
- `RecruitmentVerdict { Joined | Declined }`。
- 复用：`OffensiveGeneral`（GDD_019/021 为将属性）· `RetinueMember`/`RetinueState`（GDD_014）· `WorldTime`（GDD_001）· `FactionIntel`/知识投影（GDD_007）。

## 7. Player Inputs / System Outputs

- **输入**：搜寻/访贤（触发知晓渠道）· 发起招揽（备名望/官职/关系/待遇）· 委任入伙者职务。
- **输出**：已登场且已知晓人才视图（反全知）· 招揽判定（出仕/婉拒）· 入伙人才的战斗/生涯能力 · 僚属花名册更新。

## 8. Dependencies（依赖）

GDD_005 人物 · GDD_006 关系 · GDD_007 情报/反全知 · GDD_014 生涯僚属 · GDD_015 条件历史（登场时间窗）· GDD_019/021 战斗（人才为将）· ADR-0004 确定性 · ADR-0006 种子化随机 · ADR-0007 条件历史。
> 反向依赖：GDD_005/006/007/015 须注记被 GDD_020 引用（人才登场/知晓/为将）。

## 9. Edge Cases（边界）

- 未到时间窗即尝试招揽 → 拒（人才不存在，稳定错误码/空视图）。
- 未知晓即尝试招揽 → 拒（反全知：不在视野）。
- 招揽失败 → 可再试（attemptIndex+1，p_join 随待遇/关系变化）；同 attempt+同种子 → 同结果。
- 已入伙者重复招揽 → 幂等（已在僚属）。
- p_join=1 恒出仕、p_join=0 恒婉拒（边界确定）。

## 10. Failure Cases（失败即可继续）

招揽婉拒不切死局：可改善待遇/关系后再请，或转招他人。人才系统不阻断主循环。

## 11. Balancing Parameters（平衡参数，延后打磨）

base_will · w_renown/w_office/w_relation/w_offer · 各人 Willingness/Reluctance · 时间窗 · 知晓渠道命中率。**平衡打磨列为遗留任务（模块完成后统一进行）。**

## 12. UI Requirements

访贤/招揽面板：只列**已登场且已知晓**人才（反全知）；招揽给**条件式提示**（名望够不够/关系亲疏/待遇厚薄），**不显 p_join 数字/胜率**；婉拒给 in-world 说辞（志不在此/待时）。键鼠可达。

## 13. AI Requirements

招揽判定为确定性种子判定（非 AI 对抗）；人才"出仕志向"以 Willingness/Reluctance 表达，不需运行期 AI。（敌方势力招揽同款机制可后续复用。）

## 14. Save / Load Requirements

已知晓人才集 + 已入伙人才 + 各人 attemptIndex 纳入统一存档（ADR-0009/0005），确定性哈希；读档后招揽判定续算一致。

## 15. Test Requirements

- 登场时间窗门（未到窗不可用；到窗可用）确定性。
- 反全知：未知晓人才不入玩家视图；类型层取不到未知真值（负向不变量）。
- 招揽判定：同条件+同种子→同结果（可复现）；p_join 边界（0 恒拒/1 恒出仕）；无胜率数字暴露。
- 条件单调：名望/官职/关系/待遇↑ → p_join 不降。
- 喂给：入伙人才提供为将属性 + 进僚属。
- 存读档 round-trip（知晓/入伙/attemptIndex）一致。

## 16. MVP Scope

- 人才目录 3~5 人，各带时间窗 + 属性 + 志向（数据驱动占位，如卧龙/凤雏/良将各一）。
- 知晓渠道：侦察 + 历史事件（军师/部曲人脉可后续）。
- 招揽：F2/F3 条件 + 种子判定；入伙进僚属 + 提供为将属性。
- 反全知 + 确定性 + 存读档。

## 17. Future Scope

- 已入伙者关系恶化求去 / 被挖角；敌方势力招揽竞争；单挑；举荐链（人才荐人才）；更丰富知晓渠道；LLM 装饰性访贤对白。

## 18. Open Questions

- 招揽"待遇"的具体资源成本（粮/官职名额）与生涯经济的耦合——待平衡。
- 时间窗与历史分叉（GDD_015 分叉后人才是否改变登场）——MVP 按主线窗，分叉联动列 Future。
