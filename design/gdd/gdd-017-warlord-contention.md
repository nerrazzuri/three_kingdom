# GDD_017 — 君主争霸（Warlord Contention）

> **Status**: Draft（2026-07-04；epic-026 / M13。顶层战略：群雄割据的争霸态 + 对手扩张。终局判定见 GDD_018。待 /review-all-gdds。）
> **Epic**: epic-026-warlord-contention-loop
> **关系**: 建于 GDD_015 条件历史世界（势力/存续/领城）之上；玩家经 GDD_014 自立线 / 君主继承成为争霸一方；以 GDD_019/021 出征攻城、GDD_022 多城战区扩张；对手扩张走 ADR-0006 种子化确定性。

## 1. System Purpose

游戏顶层：玩家自立/称雄后，成为**割据群雄之一**，与曹魏/蜀汉/东吴等争霸天下。各方以领城数/实力衡量势；对手也在扩张/兼并；玩家靠出征攻城、多城经营壮大。这是"打天下"的战略层——终局（统一/覆灭）见 GDD_018。

## 2. Player Fantasy

「我从一城太守起家，屡立战功、屡抢战果，终于自立门户。如今我不再受君主节制，而是与曹操、孙权同列群雄——各据州郡，此消彼长。他吞并弱邻壮大，我亦攻城略地。天下棋局，我落一子。」

## 3. Core Loop

```
玩家成争霸一方（自立/继承，GDD_014）
  → 群雄争霸态：各势力领城数/存续（建于 GDD_015 世界势力）
  → 玩家扩张：出征攻城（GDD_019/021）→ 占城 → 领土增（GDD_022 多城）
  → 对手扩张：种子化确定性推进（强者兼并弱者，ADR-0006）
  → 势力消长 → 有势力被灭/有势力壮大 → 趋向终局（GDD_018）
```

## 4. Main Rules

- **R1 争霸态**：`ContentionState` = 各主要势力的 `PowerStanding`（领城数 + 存续）。玩家为其一。建于 GDD_015 势力/领城。
- **R2 玩家扩张**：经出征攻城（GDD_019/021）占城 → 玩家领城增（GDD_022 入战区）；被夺城 → 减。
- **R3 对手扩张（种子化确定性）**：每战略步，对手势力按实力**种子化确定性**兼并弱邻（强者概率高吞并相邻弱者），非掷骰、可复现（ADR-0006）。反全知：对手真实意图经情报，扩张结果为世界事实。
- **R4 势力消长**：领城归零 → 势力被灭（存续=false，GDD_015 SurvivalStatus）。领城/实力用于终局判定（GDD_018）。
- **R5 玩家不全知**：他势力领土消长为世界事实（可观测），但其战略意图/内情经情报（GDD_007）——无全知战略面板。

## 5. Formulas

- **F1 实力**：`power(faction) = 领城数`（MVP；可扩为城价值加权）。
- **F2 支配度**：`dominance(faction) = 领城数 / 天下总城数`。
- **F3 对手兼并（种子化）**：`seed=Hash(worldTick, rival, target)`；强邻以 `p=clamp(strengthRatio·w, 0, 1)` 兼并相邻弱势力一城，`DetRng(seed).NextUnit()<p`。确定性、可复现。

## 6. Data Model

- `PowerStanding`（FactionId + 领城数 + 存续）· `ContentionState`（各势力 standing）不可变、哈希、存档。
- `ContentionConfig`（对手兼并权重/阈值）· `RivalExpansionService`（种子化确定性对手推进）· `HegemonyService`（支配度/被灭判定）。
- 复用：FactionId/SurvivalStatus/领城（GDD_015）· 占城 C（GDD_019）· 多城（GDD_022）· 种子流（ADR-0006）。

## 7. Player Inputs / System Outputs

- **输入**：（经出征/外交/多城）改变自身领土；观测群雄态。
- **输出**：争霸态（各方领城/存续）· 对手扩张结果 · 支配度 · 被灭势力 · 喂终局判定（GDD_018）。

## 8. Dependencies

GDD_014/015/019/021/022 · GDD_007（反全知）· ADR-0004/0006/0008 · GDD_018（终局）。反向依赖：GDD_014/015/019 注记被 GDD_017 引用。

## 9. Edge Cases

- 玩家领城归零 → 玩家势力被灭（终局覆灭，GDD_018）。对手互兼并至一家独大 → 趋向该家统一（若非玩家则玩家告负风险）。无相邻弱邻 → 对手不扩张（稳态）。

## 10. Failure Cases

玩家一时失利（丢城）不即终局，仍可反攻（除非领城归零）。失败可继续（红线）延续到战略层。

## 11. Balancing Parameters（延后打磨）

对手兼并权重/频率 · 实力权重 · 相邻关系。**平衡延后。**

## 12. UI Requirements

群雄态面板：列各势力领城/存续（世界事实可观测）；他势力战略意图经情报（不全知）。键鼠可达。

## 13. AI Requirements

对手扩张：种子化确定性兼并（强吞弱），反全知（不预知玩家隐秘）；MVP 简化，深度（结盟/合纵）列 Future。

## 14. Save / Load Requirements

ContentionState 纳入统一存档，确定性哈希 round-trip 一致；对手扩张种子续算一致。

## 15. Test Requirements

- 争霸态 + 哈希 round-trip。
- 对手兼并确定性（同种子同走向）+ 强者更易兼并（单调）。
- 领城归零 → 势力被灭。
- 支配度计算正确。

## 16. MVP Scope

- ContentionState（各势力领城/存续）+ 对手种子化兼并 + 支配度/被灭。接玩家占城→领土增。终局判定见 GDD_018。

## 17. Future Scope

- 对手结盟/合纵连横 · 城价值加权实力 · 玩家称帝/正统性 · 更丰富的对手战略 AI · LLM 群雄叙事。

## 18. Open Questions

- 对手扩张与 GDD_015 历史事件推进的耦合（历史轨 vs 自由争霸）——MVP 简化为独立种子步，历史联动列 Future。
