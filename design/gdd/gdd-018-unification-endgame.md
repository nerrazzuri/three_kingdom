# GDD_018 — 统一终局（Unification Endgame）

> **Status**: Draft（2026-07-04；epic-027 / M14。终局判定：统一天下=胜、势力覆灭=负。建于 GDD_017 争霸态。待 /review-all-gdds。）
> **Epic**: epic-027-unification-endgame-loop
> **关系**: 消费 GDD_017 争霸态（各势力领城/存续）判终局；玩家经出征（GDD_019/021）/多城（GDD_022）壮大至统一；覆灭走 GDD_014 失败可继续的终点。

## 1. System Purpose

给"打天下"一个**终点**：玩家领土支配度达阈值（或群雄尽灭）→ **统一天下（胜）**；玩家势力领城归零 → **覆灭（负）**；否则争霸继续。这是整局游戏的胜负终局条件。

## 2. Player Fantasy

「群雄次第翦灭，天下城池半数尽入我手——大势已成。最后一战克定中原，我一统山河，成就霸业。抑或，我众叛亲离、城池尽失，身死国灭——这也是一种结局。」

## 3. Core Loop

```
争霸态（GDD_017）每战略步更新
  → 终局判定：玩家支配度 ≥ 统一阈值（或群雄尽灭）→ 统一（胜局）
              玩家领城归零 → 覆灭（负局）
              否则 → 继续争霸
  → 终局达成 → 游戏收束（胜局叙事 / 负局收场）
```

## 4. Main Rules

- **R1 胜局（统一）**：玩家支配度 `dominance ≥ UnificationThreshold`（默认过半，MVP 可设阈）**或**其余主要势力尽灭 → **统一天下**。
- **R2 负局（覆灭）**：玩家领城归零（势力被灭）→ **覆灭**。
- **R3 持续**：未达胜/负 → 争霸继续（GDD_017 循环）。
- **R4 确定性**：终局判定为争霸态的确定性纯函数（同态同判）。
- **R5 失败可继续边界**：覆灭是**终点**（区别于战役级失败可继续）——但仅当领城真归零；一时失利不终局（GDD_017 R2 延续）。

## 5. Formulas

- **F1 终局**：
  ```
  if player.领城 == 0 → PlayerEliminated
  elif dominance(player) ≥ UnificationThreshold ∨ 其余主要势力尽灭 → PlayerUnifies
  else → Ongoing
  ```
- **F2 支配度**：`dominance = 玩家领城 / 天下总城`（GDD_017 F2）。

## 6. Data Model

- `EndgameStatus { Ongoing | PlayerUnifies | PlayerEliminated }`。
- `EndgameConfig`（UnificationThreshold 定点 [0,1]）· `EndgameService.Evaluate(ContentionState, playerFaction, config)`（确定性纯函数）。
- 复用：ContentionState/PowerStanding（GDD_017）· FactionId。

## 7. Player Inputs / System Outputs

- **输入**：（无直接输入；经争霸态演化触发）。
- **输出**：终局状态（继续/统一/覆灭）+ 收束（胜局/负局叙事）。

## 8. Dependencies

GDD_017（争霸态）· GDD_014（覆灭=生涯终点）· GDD_019/021/022（壮大路径）· ADR-0004。反向依赖：GDD_017 注记被 GDD_018 消费。

## 9. Edge Cases

- 玩家与最后一敌同归于尽（同步归零）→ 优先判玩家覆灭（负）。统一阈值边界（恰达阈）→ 判统一。仅剩玩家一家 → 统一。

## 10. Failure Cases

覆灭为终点（负局收场），非中途卡死——是完整结局的一部分。

## 11. Balancing Parameters（延后打磨）

UnificationThreshold（统一所需支配度）。**平衡延后。**

## 12. UI Requirements

终局屏：统一（胜局叙事）/覆灭（负局收场）；争霸中显支配度进度（距统一还差几何）。键鼠可达。

## 13. AI Requirements

无（终局为确定性判定）。

## 14. Save / Load Requirements

终局状态由 ContentionState 派生，无独立存档态（随争霸态存档）。

## 15. Test Requirements

- 领城归零 → PlayerEliminated。
- 支配度达阈 → PlayerUnifies。
- 群雄尽灭 → PlayerUnifies。
- 未达 → Ongoing。确定性（同态同判）。

## 16. MVP Scope

- EndgameStatus + EndgameService（阈值/尽灭/归零判定）。接 GDD_017 争霸态。

## 17. Future Scope

- 多种胜局（称帝/禅让/正统）· 分阶段终局叙事 · 历史结局对照（演义 vs 玩家改写）· LLM 终局史书。

## 18. Open Questions

- 统一阈值定为"过半"还是"尽灭群雄"——MVP 双条件取一（阈或尽灭），具体阈待平衡。
