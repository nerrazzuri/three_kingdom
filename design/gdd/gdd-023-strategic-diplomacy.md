# GDD_023 — 战略外交约束（Strategic Diplomacy）

> **Status**: Draft（2026-07-04；epic-024 / M11。战略层外交——立场约束战争 + 缔约 + 背约代价。战术层求援见 GDD_012 §8。待 /review-all-gdds。）
> **Epic**: epic-024-strategic-diplomacy-loop
> **关系**: 扩 GDD_012 §8（战术受控外交=求援/求粮/求时限）到**战略层**：与各势力的外交立场约束出征/守土。复用 GDD_006 声望 · GDD_019/021 出征（战争约束门）· GDD_015 世界势力 · ADR-0004/0006。

## 1. System Purpose

给战争加**外交战略约束**：与其他势力的立场（敌对/中立/互不侵犯/盟约）决定你能否径攻。缔约换来安宁与助力，但**背约有声誉代价**——外交不是自由开战，是有邦交、有信誉的战略层。

## 2. Player Fantasy

「我想打虎牢关，可我与其主新缔了互不侵犯——要打，就得先背约，担这声誉的骂名，让他与我反目。若我与东吴结盟，西线便可安心，专攻北面。邦交有度，背信有价。」

## 3. Core Loop

```
与各势力立场（默认中立）
  → 缔约（互不侵犯/盟约）：备名望/交情/厚礼 → 条件+种子判定成否
  → 出征受约束：攻盟/邻须先背约（转敌对 + 声誉代价）；攻敌/中立无约束
  → 背约代价写回声誉（GDD_006）；盟约破裂影响后续邦交
```

## 4. Main Rules

- **R1 外交立场**：与每势力的 `DiplomaticStance{Hostile|Neutral|NonAggression|Alliance}`（默认中立）。权威态、存档、确定性哈希。
- **R2 缔约**：提议互不侵犯/盟约 → `p_accept = clamp(base + w·(名望/交情/厚礼), 0,1)`（对各条件单调不降），**盟约较互不侵犯更难**（额外阻力）；种子化确定性判定（可复现·非掷骰）。成则立约。
- **R3 战争约束门**：攻打某势力——敌对/中立**无约束**；互不侵犯/盟约**须背约方可攻**（`CheckWarTarget` → RequiresBreach + 声誉代价）。接出征授权门之后（GDD_019）。
- **R4 背约代价**：背约攻打 → 被背方**转敌对** + **声誉惩罚**（背盟重于背互不侵犯，写回 GDD_006 名望/问责）。确定性。
- **R5 反全知**：他势力真实军力/意图仍经情报（GDD_007）；己方立约是自知的邦交，敌方结盟需情报探知（MVP：己方立场已知，敌方间盟约列 Future）。

## 5. Formulas

- **F1 缔约**：`p=clamp(base+w_renown·名望+w_relation·交情+w_gift·厚礼 − (盟约?allianceResistance:0),0,1)`；`accepted=DetRng(seed).NextUnit()<p`。
- **F2 战争约束**：`stance∈{NonAggression,Alliance} → RequiresBreach, cost=breachPenalty(stance)`；否则 Allowed。
- **F3 背约**：`stance→Hostile`；`reputationPenalty=breachPenalty(stance)`（盟>互不侵犯>0）。

## 6. Data Model

- `DiplomaticStanceState`（FactionId→stance；缺省中立）不可变、哈希、存档。
- `PactFactors`（名望/交情/厚礼 norm）· `StrategicDiplomacyConfig`（base/权重/盟约阻力/背约代价）。
- `WarConstraint{Allowed,RequiresBreach,BreachReputationCost}` · `BreachResult{State,ReputationPenalty}` · `PactResult`。
- `StrategicDiplomacyService`（ProposePact/CheckWarTarget/Breach）。复用 FactionId（GDD_015）· 声望（GDD_006）· 出征门（GDD_019）。

## 7. Player Inputs / System Outputs

- **输入**：提议缔约 · （出征前）查战争约束 · 背约。
- **输出**：外交立场态 · 缔约成否 · 战争约束（可攻/须背约+代价）· 背约后果（转敌对+声誉罚）。

## 8. Dependencies

GDD_006/007/012/015/019/021 · ADR-0004/0006。反向依赖：GDD_012/019 须注记被 GDD_023 引用（战术↔战略外交、出征战争约束门）。

## 9. Edge Cases

- 攻中立/敌对 → 无约束（Allowed，无背约）。背中立 → 无背约、无罚。缔约被拒 → 立场不变、可再议。已盟再提盟约 → 幂等（保留）。

## 10. Failure Cases

缔约被拒/受约束不切死局：改条件再议、背约、或改攻他敌。

## 11. Balancing Parameters（延后打磨）

base_accept · 各权重 · 盟约阻力 · 背约声誉代价 · 缔约厚礼资源成本。**平衡延后。**

## 12. UI Requirements

外交面板：列各势力立场；缔约给条件式提示（名望/交情/厚礼够不够）、**不显 p 数字**；出征前若受约束，明示"须背约（声誉代价 X）或改目标"。键鼠可达。

## 13. AI Requirements

缔约为确定性种子判定；他势力邦交 AI（结盟/背盟）列 Future（MVP 玩家侧）。

## 14. Save / Load Requirements

DiplomaticStanceState 纳入统一存档，确定性哈希 round-trip 一致。

## 15. Test Requirements

- 立场默认中立 + 哈希 round-trip。
- 缔约概率单调 + 盟约更难 + 边界 + 确定性 + 成则立约。
- 战争约束按立场（敌/中立 Allowed；互不侵犯/盟约 RequiresBreach + 代价）。
- 背约转敌对 + 声誉罚（盟>互不侵犯>0）。
- 出征受约束门（有约→受阻；背约→可攻）整合。

## 16. MVP Scope

- 立场态 + 缔约 + 战争约束门 + 背约代价。接出征授权后的战争约束门（CampaignRuntime）。

## 17. Future Scope

- 他势力间邦交（需情报探知）· 盟军参战助力 · 多方合纵连横 · 背约的连锁不信任 · LLM 外交辞令。

## 18. Open Questions

- 背约声誉罚与生涯名望/问责的耦合力度——待平衡。
- 敌方 AI 主动缔约/背盟——列 Future。
