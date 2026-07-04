# GDD_019 — 出征攻城（Offensive Campaign）

> **Status**: Revised v2（2026-07-04；在 v1 Reviewed 基础上**扩「出征准备维度」**——把出征准备从「兵力/补给/已成型条件」三维扩为**六维闭合因果**：兵力 · 补给 · 将领编成 · 兵种编成 · 布势路线 · 时机/天气，外加侦察情报门。设计锁守恒：布势=路线非坐标、兵种=杠杆非克制、招式=条件涌现。见 ADR-0011。v1 的占城归属方案 C / 授权门 / 反全知不变，已实现且保留。）
> **Epic**: epic-029-offensive-campaign-loop
> **关系**: 本文是"进攻侧"的循环规则，**复用**既有系统而非新造——出征战斗走 GDD_009/010/011，占城走 GDD_004，回报走 GDD_014，情报走 GDD_007，天气走 GDD_002，僚属走 GDD_006/014。本文只定义**把它们连成"主动攻城"的规则 + 多维准备的闭合因果 + 占城归属**。

## 1. Overview（概述）

出征攻城是太守的**主动进取能力**：经君主授权率军攻打敌城，用**在治理/征兵/备战/情报中真实攒出的多维准备**打赢攻城战，胜则占城（归属见 §占城 C）、得功绩名望而升官，败则折损但可继续。出征的胜负**不是点一个"攻城"按钮**，而是由**六维准备**（带多少兵、备多少粮、派谁挂帅、如何编成兵种、走哪条布势路线、择何时机天气）经**确定性闭合因果**派生出的进攻方战力与兵法条件决定——**准备决定胜负**。它把此前**只有防守**（GDD_014 太守开局守城）的循环补成**攻守双向**，为玩家提供持续的目标、纵深与"派谁·投多少·借何势·择何时"的真实决策。

## 2. Player Fantasy（玩家幻想）

「我不是坐守孤城、点个按钮就出兵的太守——我**主动请缨出征**：点**我信得过的宿将**挂帅、配上**擅奇袭的副将**与随军**军师**，按敌情**编好步骑弓的比例**，选定**夜里借雾偷袭**还是**长围断其粮道**，靠事先修好的工事、备足的粮、探明的敌情，**打下一座城**。这一胜是我**挣来的**（我把条件一样样凑齐了），不是骰子给的。若我兵不够、粮不足、又贸然裸战，也可能大败折兵——那也是我准备不足的账。打下来君主可能赏给我治理，也可能收走——收多了，我便有了自立的心。」

## 3. Core Loop（核心循环）

```
治理/征兵/备战/侦察（攒兵·攒粮·攒将·攒情报·攒条件）
  → 请缨 + 【君主授权出征】选目标敌城
  → 【组装出征】：兵力 · 粮草 · 将领编成 · 兵种编成 · 布势路线 · 时机/天气
  → 攻城战（GDD_010 兵法沙盒；你六维准备派生的战力与条件决定胜负）
  → 胜：占城（§占城 C）+ 功绩/名望 → 升官（GDD_014）→ 更大战区/出征权/直辖城
     败：折损、退兵，但必留合法可继续状态（不卡死）
```

## 4. Main Rules（主要规则）

**R1 授权前置**：出征须君主授权（属朝廷政令任务，GDD_014 忠臣线）。未授权/超出权限（如攻打非授权目标、越战区）→ 拒绝，稳定错误码，无出征。授权额度/范围随官阶放宽（GDD_014 Rank）。

**R2 目标合法性**：目标须为世界模型中登记的敌方控制城（GDD_004 控制权 / GDD_015 世界投影）。己方城、盟友城、不可达城不可攻。

**R3 闭合因果（核心·多维）**：攻城战的初始条件（进攻方战力/士气/携入兵法条件）**由玩家的六维准备态确定性派生**，不再脚本固定，也不是单按钮。六维见 §4a。**准备充分且路线的条件门齐备 → 触发兵法优势、以弱胜强；准备不足/裸战 → 战力硬碰、可能败。** 胜负经 GDD_010 确定性结算（无胜率、条件涌现）。

**R4 占城归属（方案 C）**：见 §占城 C（v1 不变，已实现）。

**R5 回报与失败**：胜 → 功绩/名望增（非战斗源之外的战功来源，GDD_014）→ 升官门槛推进（epic-022）。败 → 折损兵力、退兵；生涯不切死局，退兵后可再备战/再请缨/或转守（失败可继续，红线）。

**R6 反全知**：出征前对敌城的了解**只经玩家情报知识**（GDD_007 投影）——你可能情报有误（敌比预估强/弱），这本身是风险。军师建议不报成功率。**未经侦察的出征**拿不到"敌方未察觉/伏击突然性"这类需摸清敌情才成立的条件（见 §4a D6/门表），并承受情报盲区的战力折扣。

### 4a. 出征准备六维（权威，闭合因果输入）

> 每一维都**喂给闭合因果一个具体量**（战力 / 士气 / 某兵法条件能否成型），喂不到即不入模型（砍 scope 尺子）。全部权威路径整数/定点（ADR-0004）。

- **D1 兵力（数量）**：可投入（征募）兵力 `MusteredTroops`。**单调** → 基础战力。兵源来自城市/后勤征募（GDD_004/012）。
- **D2 粮草（补给）**：随军续航 `Supply`。**单调、封顶** → 士气与久围能力（补给杠杆，GDD_012）。粮足→士气高、可长围断粮；粮薄→只能速战。
- **D3 将领编成（派谁）**：主将（必选）+ 副将（0..N）+ 军师随军（可选）。
  - **主将**：`统率`（→ 战力/军纪加成）、`武勇`（→ 士气加成）、`智略`（→ 兵法条件可行性/识破敌预设）、`专长`（善奇袭/善攻坚/善骑战/善辎重…→ 降低对应路线条件的成型门槛）。
  - **副将**：各按其属性贡献**递减**的分量（避免堆将碾压；护栏见 §8）。
  - **军师随军**：随军则开启/强化"敌预设可信度"与"伏击突然性"一类**需智谋才成立**的条件门（GDD_008），并提供战前可行性提示（军师不报成功率、不替选，设计锁）。
  - 将领来源 = 玩家僚属花名册（GDD_014 RetinueState / GDD_006 好感）；只有己方且在城可调之将可挂。
- **D4 兵种编成（杠杆，非克制三角）**：把 `MusteredTroops` 按兵种分配比例——步卒 / 骑兵 / 弓弩 /（水军，非本场景）。**不做 A>B>C 抽象克制**；每种兵在**特定地形/时机**下**强化特定兵法条件 + 给一个小战力契合修正**：
  - 骑兵 + 平原/机动时机 → 追击/机动类条件（`EnemyPursued`/`AmbushSurprise`）更易成型。
  - 弓弩 + 隘口/守势 → 远程压制（守城反打，本场景次要）。
  - 步卒 → 攻坚主力（正面强攻/长围的战力契合）。
  - 水军 + 渡口/水战 → 水战条件（非虎牢关陆战场景，模型预留不实装）。
- **D5 布势路线（阵型的正确落法·非坐标）**：从四条路线择一，**即是你要凑成的那条兵法链**（复用 GDD_010 现有 TacticTag）：
  - **正面强攻** `FrontalAssault`：不图兵法，纯战力硬碰；步卒契合、见效快、无条件加成（裸战底）。
  - **假退诱敌** `FeintLure`（= FeintAmbush）：目标条件 {受控撤退保形, 敌军追击, 伏兵突然性}。
  - **长围断粮** `ProtractedSiege`（= SupplyExhaustion）：目标条件 {切断敌补给, 断粮达宽限, 敌士气/疲劳跨阈}。
  - **夜袭** `NightRaid`：目标条件 {值夜, 隐蔽成功, 守方未察觉, 袭方军纪达标}。
  > 路线**只声明意图（目标条件集）**；这些条件**能否真正携入开战态**取决于其他维的**门**（D2/D3/D4/D6 + 侦察）。选了诱敌却无骑兵、无军师、非隘口 → 条件不成型，退化为硬碰。**这就是"布势"——摆的是条件，不是坐标。**
- **D6 时机/天气窗口（何时）**：发起时段 `DaySegment` + 当前 `WeatherType`（GDD_002）。**门**若干路线条件：夜袭需**夜间时段**（`IsNight`）；隐蔽成功需**雾**（`Fog`，或军纪足）；长围断粮需**推进足够时段**（时间成本）。时机不对 → 对应条件不成型。
- **D7 侦察情报门（反全知，R6）**：目标是否经玩家侦察摸清（GDD_007 知识投影）。**门** {守方未察觉, 伏兵突然性} 一类需"知己知彼"的条件；未侦察则拿不到这些条件，且承受**情报盲区战力折扣**（你摸黑攻城）。敌方守备的**呈现**只经玩家知识投影（军师/目标视图显示估计值+时效，不显真值）。

## 5. Formulas（公式）

> 权威路径整数/定点（ADR-0004）；系数进 §8 Tuning / 数据驱动配置。示例用 §8 默认值。

- **F1 出征战力（进攻方）**：
  ```
  atk_force = BaseForce
            + ForcePerTroop · MusteredTroops
            + round( 统率lead · CommandForceWeight )
            + Σ_deputy round( 统率dep · DeputyForceWeight · decay^k )   // k=第 k 名副将，decay∈(0,1] 递减
            + CompositionFit(兵种比例, 路线, 地形)                       // 小幅 ±，见 F2b
            + ApproachForceMod(路线)                                    // 强攻 +、长围 −（换耗敌）、诱敌/夜袭 0
            − IntelBlindPenalty(未侦察 ? 1 : 0)
  ```
  取值范围：force ≥ 0（各项 checked，负结果夹到 BaseForce 之上不为负）。
  **示例**（默认值）：BaseForce=200，600 兵、主将统率0.8、无副将、步卒为主打正面强攻、已侦察：
  `200 + 1·600 + round(0.8·100=80) + 0 + 0(强攻步卒契合中性) + 50(强攻+) − 0 = 930`。
- **F2 士气（进攻方）**：
  ```
  morale = BaseMorale + SupplySteps · MoralePerStep + 武勇lead · ValorMoraleWeight
  SupplySteps = Supply / SupplyStep   (整除)
  morale = min(morale, MaxMorale)
  ```
  取值范围：morale ∈ [0, MaxMorale=1]（定点）。
  **示例**：Base0.5，Supply300、SupplyStep50 → 6 档 ·0.05=0.30；武勇0.7·0.1=0.07 → `0.5+0.30+0.07=0.87`。
- **F2b 兵种契合 CompositionFit**（杠杆，非克制）：对所选路线，若匹配兵种份额达门槛给 `+FitBonus`，否则 0；**无任何"克制敌兵种"减益**。例：诱敌路线且骑兵份额 ≥ CavalryMinShare → 契合+；否则 0。
- **F3 兵法条件成型门（路线模板 ∩ 门）**：所选路线的每条目标条件，仅当其门全通过才携入 `OffensiveForce.Conditions`（供 GDD_010 §7 条件涌现结算 + 战后 `TacticRecognizer` 只读打标签，二者分述）：
  | 条件 | 门（须全真才成型） |
  |---|---|
  | 受控撤退保形 | 路线=诱敌 |
  | 敌军追击 | 路线=诱敌 且 骑兵份额≥CavalryMinShare |
  | 伏兵突然性 | 路线=诱敌 且 （军师随军 或 主将智略≥GuileMin） 且 已侦察 且 地形=隘口 |
  | 切断敌补给 | 路线=长围 且 Supply≥StarveSupplyMin |
  | 断粮达宽限 | 路线=长围 且 已推进≥StarveSegments（时间成本，发起时以"承诺长围"标记，结算按已耗时段判定） |
  | 敌士气/疲劳跨阈 | 路线=长围 且 上二者成立 |
  | 值夜 | 路线=夜袭 且 时段∈夜 |
  | 隐蔽成功 | 路线=夜袭 且 （天气=雾 或 主将统率≥DisciplineMin=军纪） |
  | 守方未察觉 | 路线=夜袭 且 已侦察 |
  | 袭方军纪达标 | 路线=夜袭 且 主将统率≥DisciplineMin |
  正面强攻无目标条件（纯 F1 战力）。**任一路线选了但门不齐 → 该条件不成型、不打标签**（TR-battle-002 负向不变量延续）。
- **F4 攻城胜负**：`atk_force`（F1）+ 携入条件（F3，经 GDD_010 §7 结算加成）对 `SiegeDefense`（守方战力，见 §6）确定性比较——胜负无胜率数字（GDD_010）。
- **F5 占城归属取舍（C，第三座起）**（v1 不变，已实现）：
  ```
  seed = Hash(worldTick, playerFactionId, cityId, conquestIndex)
  p_grant = clamp( base_grant + w_renown·名望norm + w_standing·君主好感norm + w_value·城价值norm , 0, 1 )
  归属 = seeded_bernoulli(seed, p_grant) ? GrantToPlayer : LordKeeps
  前两座 conquestIndex < 2 → 恒 GrantToPlayer
  ```
- **F6 自立倾向累积**（v1 不变）：`rebellion_lean += lean_per_seizure`（每次被君主收走战果）。

## 6. Data Model（数据模型）

- `CampaignAuthorization`（君主授权：目标城集合/战区/额度，随官阶）—— 已实现（OffensiveAuthorization）。
- `OffensivePreparation`（**扩为六维**）：`MusteredTroops` · `Supply` · `OffensiveCommand` · `TroopComposition` · `ApproachPlan` · `OffensiveTiming` · `Scouted`。
  - `OffensiveCommand`：`OffensiveGeneral Lead` + `IReadOnlyList<OffensiveGeneral> Deputies` + `bool AdvisorAccompanies`。
  - `OffensiveGeneral`：`CharacterId` + `Command/Valor/Guile`（Q16.16 ∈[0,1]）+ `GeneralSpecialty`（None/Ambush/Siege/Cavalry/Logistics）。
  - `TroopComposition`：`IReadOnlyDictionary<TroopType,int>`（各兵种数，和 ≤ MusteredTroops）；`TroopType { Infantry, Cavalry, Archer, Marine }`。份额 = 该兵种数 / 总兵。
  - `ApproachPlan`（enum）：`FrontalAssault | FeintLure | ProtractedSiege | NightRaid`。
  - `OffensiveTiming`：`DaySegment Segment` + `WeatherType Weather`。
- `OffensiveForce`（派生输出，已有）：`Force` · `Morale` · `Conditions`（携入条件集）。
- `OffensiveSetupConfig`（**扩**）：F1/F2 系数 + F2b/F3 各门槛（CommandForceWeight、DeputyForceWeight、decay、ValorMoraleWeight、CavalryMinShare、GuileMin、DisciplineMin、StarveSupplyMin、StarveSegments、各路线 ForceMod、FitBonus、IntelBlindPenalty…）。
- `ConquestRecord`（conquestIndex + 各城归属）· `OwnershipVerdict { GrantToPlayer | LordKeeps }`—— 已实现。
- 复用：CommittedPlan（GDD_009）、BattleSnapshot（GDD_010）、CityControlAuthority（GDD_004）、CareerState/RetinueState（GDD_014）、WeatherType（GDD_002）、TacticCondition/TacticTag（GDD_010）。

## 7. Player Inputs / System Outputs

- **输入**：请缨/接受出征政令、选目标城、**组装出征六维**（挂帅点将/编兵种/择路线/定时机）、（复用）备战计划、发起、解析、结算。
- **输出**：进攻方战力/士气/携入条件（六维派生）、攻城战果、城池控制权变更事件（GDD_004）、功绩/名望增量（GDD_014）、占城归属判定、自立倾向增量、失败续局选项（GDD_010 §后果）。

## 8. Tuning Knobs（可调值）

- 各官阶授权额度/战区范围。
- 占城 C：`base_grant`、`w_renown`、`w_standing`、`w_value`、前 N 座默认归玩家的 N（默认 2）、`lean_per_seizure`。
- 闭合因果：`BaseForce`(默认200)、`ForcePerTroop`(1)、`BaseMorale`(0.5)、`MoralePerStep`(0.05)、`SupplyStep`(50)、`MaxMorale`(1.0)。
- 将领：`CommandForceWeight`(100)、`DeputyForceWeight`(60)、`decay`(0.5)、`ValorMoraleWeight`(0.1)、`GuileMin`(0.6)、`DisciplineMin`(0.6)。安全范围：CommandForceWeight ∈ [50,200]（过大则将碾压兵力，踩堆将反支柱）。
- 兵种/路线门：`CavalryMinShare`(0.3, 骑兵占三成方利追击)、`StarveSupplyMin`(200)、`StarveSegments`(8)、各 `ApproachForceMod`（强攻+50/长围−80/诱敌0/夜袭0）、`FitBonus`(40)、`IntelBlindPenalty`(80)。

> **反支柱护栏（W5，强制）**：出征是**强战功→功绩/名望**通道。平衡须保证**出征功绩产出速率不压过治理/招揽/外交/平叛**——单次出征功绩设上限 + 出征有真实成本（兵粮消耗/时间/失败折损）+ 堆将有递减（decay），使"只刷出征"不优于均衡经营（承接 GDD_014 N10 与 2026-06-24 W5）。列入平衡验证用例。
>
> **打磨确认（2026-07-04）**：§8 上列出征派生值经 `OffensiveDomainTests` 单调性/边界/决定性用例确认（兵力↑→战力↑、补给↑→士气↑并封顶、统率/勇武↑→战力/士气↑、副将贡献 decay 递减、组装校验拒越界）——**默认值无需改动**。W5 决定性护栏由 `ZoneBattleBalanceTests.test_preparation_decides_outcome_against_same_garrison` 端到端锁定：**同守备（400）下，唯"准备"决定胜负**——备足（900兵）经六维准备派生足够战力破城（AttackerVictory），裸战（120兵）同守备下退兵（DefenderVictory·失败可继续）。堆将递减/占城归属频谱见 GDD_021 §11 与 `OffensiveDomainTests` 占城 C 用例（前 N 归玩家 / p_grant 极值 / renown 单调 / 种子决定性）。

## 9. Edge Cases（边界情况）

- 情报错误/未侦察：敌远强于预估或摸黑攻城 → 无突袭类条件 + 情报盲区折扣 → 可能大败（合理风险，非 bug）；败后可继续。
- 兵种和 > 兵力：`TroopComposition` 构造校验各兵种和 ≤ MusteredTroops；越界即拒（稳定错误码，无部分写入）。
- 选路线但门不齐：合法但退化为硬碰（不报错——这是设计意图，不是异常）；军师/目标视图应提示"缺 X 条件"。
- 无主将：主将必选，缺则组装被拒（稳定错误码）。副将/军师可缺省。
- 堆将：副将战力贡献 `decay^k` 递减，且总加成受护栏；不得线性堆叠碾压。
- 长围时间：发起时承诺长围，`断粮达宽限` 按结算时已耗时段判定；未撑够时段则条件不成型。
- 授权撤销：出征中君主撤令 → 已开战不受影响，未开战则取消。
- 前两座之一战败未占：conquestIndex 只在**成功占城**时递增。
- 占城后立即被攻（敌反扑）：走守城（GDD_014 防守侧），控制权可再变。

## 10. Dependencies（依赖）

GDD_002（天气·时机门）· GDD_004（城池控制权/占城写入）· GDD_006（僚属好感/将领来源）· GDD_007（情报/反全知/侦察门）· GDD_008（军师随军·可行性提示）· GDD_009（备战）· GDD_010/011（战斗/士气/兵法条件涌现）· GDD_012（补给/征兵）· GDD_014（生涯/授权/回报/自立/花名册）· GDD_015（世界投影/目标合法性）· ADR-0004（确定性定点）· ADR-0006（种子化随机）· ADR-0008（控制权契约）· ADR-0010（占城归属契约）· **ADR-0011（多维确定性出征准备模型）**。
> 反向依赖：本文扩用 GDD_002/006/008，须在其 Dependencies 注记被 GDD_019 出征准备维度引用（六维准备门）。

## 11. Failure Cases（失败即可继续，红线）

出征败 → 折损退兵，**必生成合法可继续状态**：再备战、改守、转攻他城、或（若倾向足）自立。绝不卡死、不切死局。

## 12. Acceptance Criteria（验收，可测）

- **AC-1 授权门**：未授权/越权出征被拒（稳定错误码）；授权范围随官阶正确放宽。
- **AC-2 目标合法**：只可攻登记的敌控城；己方/盟友/不可达城被拒。
- **AC-3 闭合因果（多维·核心）**：相同种子下，**改变任一准备维（兵力/粮/主将/兵种/路线/时机/侦察）→ 可致不同攻城胜负或不同兵法识别**；准备充分（对路线凑齐门）可胜、裸战/门不齐可败；全程确定性、无胜率数字。
- **AC-3a 单调性**：兵力↑→战力不降；补给↑→士气不降（封顶）；主将统率↑→战力不降、武勇↑→士气不降。
- **AC-3b 兵种=杠杆非克制**：模型**无**兵种间克制减益（负向不变量：任何兵种组合都不会因"被克"而减战力，只有匹配加成或 0）；`OffensiveSetupService` 无 A>B>C 表。
- **AC-3c 布势=路线非坐标**：`ApproachPlan` 为有限路线枚举，**无坐标/格子/朝向字段**（负向不变量：数据模型不含 position/grid/facing）。
- **AC-3d 门成型**：选路线但门不齐 → 对应条件**不携入**（不打标签）；门齐 → 携入。逐条件可验证。
- **AC-4 占城 C**：前两座恒归玩家直辖；第三座起按 F5 种子化取舍（同种子同结果，可复现）；占城经 GDD_004 控制权变更事件写入。（v1 已实现）
- **AC-5 回报联动**：胜→功绩/名望→晋升门槛推进；被君主收走战果→自立倾向累积。（v1 已实现）
- **AC-6 失败可继续**：败局必含 ≥1 合法可继续命令。
- **AC-7 反全知**：出征前敌情只经玩家知识投影，类型层取不到敌方真值；未侦察拿不到突袭类条件。
- **AC-8 存读档**：出征态（授权/目标/占城计数/归属/自立倾向）存读档 round-trip 一致，确定性哈希。（v1 已实现；出征草稿为发起前临时态，不入存档，读档后可重组。）
