# GDD_024：人心杠杆（离间 · 策反 · 攻心）

> **Layer**: Feature（护城河核心——把战斗接入关系与情报）
> **Status**: Draft（2026-07-04）
> **Governing ADR**: 拟 ADR-0014（人心杠杆·施计改变战斗条件契约）· 复用 ADR-0004（确定性）· ADR-0006（种子化随机·反全知）· ADR-0008（控制权）· ADR-0011/0012（出征/区域战斗接缝）
> **消费**: GDD_006 关系 · GDD_007 情报 · GDD_011 凝聚力（士气/军纪）· GDD_019 出征 · GDD_021 区域战斗

## 1. System Purpose（系统目的）

把"人心"做成**战前可施加、直接改变战斗条件**的战略杠杆：不是加一个"离间"技能按钮，而是让**情报→关系→军纪→战斗**四系统贯通——你先侦察守将弱点，再施离间/策反/攻心，改变敌守方的士气/军纪/有效守军，从而以弱胜强。这是本作区别于 Total War / 三国志的**独门差异**（game-concept 明列护城河）。

## 2. Player Fantasy（玩家幻想）

"我打不下这座坚城——但我探明守将与太守早有嫌隙，散布流言、许以高位，让他临阵倒戈。破城靠的不是我兵多，是我攻了他的心。"

## 3. Core Loop（核心循环）

侦察守将（Intel 反全知门）→ 读出弱点（低忠诚 / 与主君有怨 / 贪财 / 警觉度）→ 选施计（离间/策反/攻心）→ 投入时间与资源、暴露风险 → **种子化确定性结算**（成/无效/反噬）→ 产出 `SubversionEffect` → 出征时在战斗接缝生效（守方士气↓/军纪↓/有效守军↓/守将倒戈）→ 战果反哺关系与情报。

## 4. Main Rules（主要规则）

- **R1 反全知门（强制）**：未侦察到目标守将 → 只能盲施，成功率大幅折扣且无法读弱点（承接 GDD_007）。侦察质量越高，弱点越清晰、施计越准。
- **R2 三计（杠杆非按钮）**：
  - **离间（SowDiscord）**：作用于守将对主君的关系（Trust↓/Resentment↑，复用 GDD_006 维度）。累积到阈值 → 战斗中守方**军纪罚**（更易溃）；越阈值上限 → 触发**倒戈倾向**。
  - **策反（InciteDefection）**：仅当守将**低忠诚 + 高怨恨**方可成型（门）。种子化伯努利 → 成功 = 守军**一部倒戈**（有效守军按比例减，被夺战果记入 ADR-0010 自立倾向语义）。
  - **攻心流言（UnderminedMorale）**：散布流言 → 守方**开战士气↓**（一次性，随流言强度/守将魅力抵抗）。
- **R3 确定性（ADR-0004/0006）**：`seed = Hash(worldTick, targetCity, schemeId, attemptIndex)`；同种子同结果，非掷骰、可复现、可存档续算。
- **R4 可反噬（失败可继续，红线）**：被识破（守将警觉度 × 抵抗）→ 反噬：守方士气**反升**、守将对你**怨恨↑**、你的**情报暴露**（该城对你侦察门收紧）。反噬是合法可继续状态，非死局。
- **R5 成本与护栏（W5）**：每次施计耗**时间 + 资源**，单城**同计有递减**（反复散谣边际衰减），且施计产出**不压过正面准备**——人心是"撬动"不是"替代"六维准备（承接 GDD_019 W5）。
- **R6 授权/立场**：对盟友/己方城施计受 GDD_023 立场约束（背信有代价）。

## 5. Formulas（公式，结构）

> 权威整数/定点（ADR-0004）；系数进 §11。

- **F1 施计成功度**：`s = clamp( base + w_intel·intelQuality + w_weak·weakness − w_resist·(charm + alertness) − decay·priorAttempts , 0, 1)`；`weakness` 按计种取（离间用 resentmentToLord、策反用 (lowLoyalty ∧ resentment)、攻心用 (1−charm)）。
- **F2 种子判定**：`roll = DeterministicRandom(seed).NextUnit()`；`roll < s → 成功`；`roll ≥ s ∧ roll < s + backfireBand → 反噬`；否则**无效**（无效果、仍耗成本）。
- **F3 战斗效果映射**（`SubversionEffect`，接 GDD_021 守方）：
  - 离间成型：`defenderDisciplineDelta = −d_discord·tier`；越上限追加 `garrisonDefectRatio`。
  - 策反成功：`garrisonDefectRatio = r_defect`（有效守军 = garrison×(1−ratio)）。
  - 攻心成功：`defenderMoraleDelta = −m_rumor·strength`。
  - 反噬：`defenderMoraleDelta = +m_backfire`（守方同仇敌忾）。
- **F4 抵抗衰减**：`priorAttempts` 每次 +1，`decay·priorAttempts` 令重复施计边际递减。

## 6. Data Model（数据模型）

- `SubversionScheme`（enum：SowDiscord / InciteDefection / UnderminedMorale）
- `SubversionTargetProfile`（守将：`CharacterId`、忠诚 loyalty、对主君怨恨 resentmentToLord、贪财 greed、魅力抵抗 charm、警觉 alertness、是否已侦察 scouted、侦察质量 intelQuality）——**投影自 Intel + Relationships，非世界真值**（反全知）。
- `SubversionAttempt`（command：目标城/守将、计种、投入强度 intensity；前置校验、稳定错误码、无部分写入）
- `SubversionOutcome`（Success / Backfired / Ineffective + `SubversionEffect` + 关系/情报副作用事件）
- `SubversionEffect`（战斗接缝修正：`defenderMoraleDelta`、`garrisonDefectRatio`、`defenderDisciplineDelta`；不可变，可存档、确定性哈希）
- `SubversionConfig`（权重/阈值/反噬带/递减/各计效果系数——数据驱动）
- 会话态：某城**待生效**的 `SubversionEffect` + 已施计计数（priorAttempts，用于递减）

## 7. Player Inputs / System Outputs

- **输入**：选目标城守将（须先侦察）→ 选计种 → 投入强度 → 确认施计。
- **输出**：施计结果（成/无效/反噬，**无胜率数字**，给缘由/条件/风险，承接 GDD_008）+ 待生效效果标记 + 关系/情报变化 + 确定性哈希。

## 8. Dependencies（依赖）

GDD_006（关系维度·离间写入）· GDD_007（情报·反全知门/暴露）· GDD_011（凝聚力·守方士气军纪）· GDD_019（出征·施计效果在发起时并入）· GDD_021（区域战斗·守方受效）· GDD_023（立场约束施计对象）· ADR-0004/0006/0008/0011/0012 · 拟 ADR-0014。
> 反向依赖：GDD_006/007/011/019/021 须注记被 GDD_024 引用（守方受人心杠杆修正）。

## 9. Edge Cases（边界）

- 未侦察即施计：合法但盲施（成功度大折扣、读不到弱点）——设计意图非报错。
- 策反门不齐（守将高忠诚或无怨）：策反不成型（返回 Ineffective，不报错）；离间/攻心仍可施。
- 对己方/盟友城施计：受 GDD_023 门；越界即拒（稳定错误码）或背信记代价。
- 重复施计：priorAttempts 递减，边际趋零（防"无脑刷谣言"必胜，W5）。
- 施计后目标城易主：待生效效果绑定"城+守将"，守将换人则效果作废（不可转移）。
- 施计中被发起反侦察：走 GDD_007 暴露，收紧后续门。

## 10. Failure Cases（失败即可继续，红线）

反噬不是死局：守方士气反升 + 守将怨你 + 情报暴露，均为合法可继续状态（可改走正面强攻、换目标、或先修复情报）。绝不卡死。

## 11. Balancing Parameters（平衡参数）

`base`、`w_intel`、`w_weak`、`w_resist`、`decay`、`backfireBand`；各计效果系数 `d_discord`、`r_defect`、`m_rumor`、`m_backfire`；离间成型/倒戈阈值；单城递减率。
> **W5 护栏**：人心杠杆产出**撬动**而非**替代**六维准备——裸靠施计不足以稳破坚城；施计 + 准备才是最优。**列入平衡验证用例**（施计降门槛但不单独决定胜负）。

## 12. UI Requirements（UI 需求）

守将情报卡（弱点投影：忠诚/怨恨/贪财/警觉，**过时情报标注**，反全知）+ 三计选择（给条件/缘由/风险，**无胜率**）+ 待生效效果提示（出征界面显示"此城已被离间：守军军纪-X"）。键鼠可达，1080p/4K 可读。

## 13. AI Requirements（AI 需求）

敌方亦可对**玩家守城**施人心杠杆（对称，GDD_016）：种子化、反全知、同规则不作弊；玩家守城时可能遭遇守将被策反（给预警与反制窗口，非无解）。LLM 仅装饰（谣言文本），不参与结算。

## 14. Save / Load Requirements（存档需求）

待生效 `SubversionEffect` + priorAttempts + 施计种子纳入统一存档信封（ADR-0009/0005），round-trip 一致、确定性哈希（ADR-0004）。

## 15. Test Requirements（测试需求）

- 反全知门：未侦察成功度显著低于已侦察（同其余条件）。
- 三计门：策反须低忠诚+高怨恨方成型；否则 Ineffective。
- 确定性：同种子同结果（哈希一致）。
- 效果映射：离间→守方军纪↓、策反→有效守军↓、攻心→守方士气↓；反噬→守方士气↑。
- **W5**：施计降胜负门槛但**不单独决定胜负**（施计+弱准备仍可能败；施计+备足更稳胜）。
- 递减：重复施计边际递减。
- 存档 round-trip：待生效效果一致。

## 16. MVP Scope（MVP 范围）

三计各一条最小闭环 + 反全知门 + 反噬 + 出征接缝生效 + 敌对称（守城遭策反预警）。**不做**：多守将连锁策反政治网、举城反叛的完整势力分裂（→ 后续/新 GDD）。

## 17. Future Scope（后续范围）

连环计（多计叠加协同）· 举城反叛势力分裂 · 反间（把敌探子策反为己用）· 攻心事件网（离间引发历史分叉，接 GDD_015）。

## 18. Open Questions（开放问题）

- 施计资源用哪种（金/粮/情报点）——待接 GDD_004/012 经济。
- 敌对玩家施计的预警强度（防挫败又要有威胁）——待原型。
