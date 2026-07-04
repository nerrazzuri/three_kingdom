# Epic: 战略外交约束循环（Strategic Diplomacy Loop / M11）

> **Layer**: Feature
> **Architecture Module**: M11（`full-game-loop-module-plan` §M11）
> **Governing ADR**: ADR-0004（确定性）· ADR-0006（种子化随机）· ADR-0008（控制权）
> **GDD**: **GDD_023 战略外交约束（Draft，2026-07-04）** · 扩 GDD_012 §8 战术外交
> **Status**: ✅ Complete（2026-07-04：GDD_023 + 4 story 全实现+测试。Domain/Diplomacy StrategicDiplomacy（立场/缔约/战争约束/背约）+ CampaignRuntime 接入（出征战争约束门）+ TR-diplomacy-101~104。dotnet 955/955 绿。平衡延后。）

## 背景与问题

战术层外交（GDD_012 §8：求援/求粮/求时限）已有，但**战争没有外交约束**——玩家可径攻任意敌城，外交不影响战略。M11 补**战略层**：与各势力的外交立场（敌对/中立/互不侵犯/盟约）约束出征，缔约换安宁/助力，背约有声誉代价。让战争置于邦交与信誉之中。

## 设计裁定（GDD_023）

1. **外交立场约束战争**：攻盟/邻须先背约（转敌对 + 声誉代价）；攻敌/中立无约束。接出征授权门之后。
2. **缔约靠条件 + 种子判定**：名望/交情/厚礼 → p_accept + 种子化确定性（可复现·非掷骰）；盟约较互不侵犯更难。
3. **背约有价**：背约→被背方转敌对 + 声誉惩罚（盟>互不侵犯），写回 GDD_006。
4. **反全知**：他势力军力/意图经情报；他势力间盟约列 Future。

## 砍 scope 尺子自检

- 外交喂给什么？约束/解锁战争方向（喂战斗决策）+ 盟军助力（Future）+ 声誉张力（喂生涯）。**过尺子。**

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | 外交立场态（敌对/中立/互不侵犯/盟约）+ 存档 | Logic | ✅ |
| 002 | 缔约（条件+种子判定·盟约更难） | Logic | ✅ |
| 003 | 战争约束门（盟/邻攻须背约）接出征 | Integration | ✅ |
| 004 | 背约代价（转敌对+声誉罚，确定性） | Logic | ✅ |

## 强制设计锁（继承）

无胜率 · 反全知 · 确定性 · 数据驱动 · 零复制三国资产。

## 完成说明

Domain/Diplomacy/StrategicDiplomacy.cs（DiplomaticStance/StanceState/PactFactors/Config/WarConstraint/BreachResult/StrategicDiplomacyService）+ CampaignRuntime（Diplomacy/CheckDiplomaticWarTarget/ProposePact/BreachPact + LaunchOffensive 战争约束门）。测试 StrategicDiplomacyTests(5) + 运行期出征外交门(1)。dotnet 955/955 绿。ADR 复用 0004/0006/0008（无新增）。平衡打磨延后。
