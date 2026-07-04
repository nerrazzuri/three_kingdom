# GDD_022 — 多城战区与委任（Multi-City Theater & Delegation）

> **Status**: Draft（2026-07-04；epic-025 / M12 设计锚点。守：反全知 + 委任 AI 不越权 + 仍受约束。待 /review-all-gdds。）
> **Epic**: epic-025-multi-city-theater-loop
> **关系**: 复用 GDD_004 城市 / GDD_012 后勤 / GDD_014 生涯官阶（掌管轴）/ GDD_006 关系（受任下属）/ GDD_007 情报（反全知报告）/ GDD_019 占城 C（产出多城）/ ADR-0008 控制权 / ADR-0009 装配 / ADR-0006 确定性委任。

## 1. System Purpose

占城 C（GDD_019）使玩家**真会直辖多城**。若每城都要玩家点内政 → 苦役 + 滑向 4X 全知微操（违 game-concept）。M12 的答案：**掌管范围随官阶 + 委任下属打理次要城 + 玩家只碰关键决策 + 仍不全知**。多城喂更大补给/征募/兵力池给战争、更高权限给生涯；委任抽象掉日常微操。

## 2. Player Fantasy

「打下第三座城，我一个人管不过来了——按官阶我只能亲管一两座要地，余下的托付给信得过的下属去打理。我不再事事亲为，只在关键处（调粮、战区令、危局）拍板。各城的近况靠下属回报，不是我掌上开个全知面板就一览无遗。」

## 3. Core Loop

```
占城 C → 城入战区态（默认亲管）
  → 亲管超官阶上限 → 委任下属（DelegatedGovernor）打理
  → 委任城由下属按本地态势自理（征募/治理，不越权做战略）
  → 玩家只做关键决策：跨城调粮（守恒）· 战区令 · 危局响应
  → 各城态经报告呈现（委任城非即时·反全知）；升官 → 掌管范围放宽
```

## 4. Main Rules

- **R1 掌管范围随官阶**：官阶 Rank 0–7 决定**亲管**城数上限（`SpanOfControlConfig`）。占城入战区默认亲管；超上限须委任他城或升官（收回亲管超上限 → 拒，稳定错误码）。
- **R2 委任打理**：直辖但不亲管的城 → 委任 `DelegatedGovernor`（来自僚属/招揽人才）。委任城由下属**本地自理**。
- **R3 委任不越权（负向不变量）**：下属动作空间**结构上仅本地治理**（征用/修工事/安抚/无为），**不含**出征/宣战/战区令等战略——委任 AI 绝不越权。确定性（规则化优先级，无旁路随机）。
- **R4 资源守恒**：跨城资源/补给**守恒**（无凭空产出/丢失）；调配经命令路径（`TheaterResources.Transfer`：源不足/量非法拒，无部分写入）。
- **R5 玩家仍不全知**：多城态经**战区报告**呈现——亲管城即时准确、委任城经下属汇报（`Fresh=false`，可滞后）。**无全知势力面板**（反全知）。

## 5. Formulas

- **F1 亲管门**：`可亲管 = 亲管城数 < MaxSelfGoverned(rank)`；收回亲管使 `亲管数+1 > 上限` → 拒。
- **F2 守恒**：`Σ_city stock` 调配前后不变；`Transfer(from,to,amt)`：`from−amt, to+amt`（amt>0 ∧ from≥amt）。
- **F3 委任择动**（确定性优先级）：民心<门→安抚；工事<门→修；粮≥丰门→征；否则无为。

## 6. Data Model

- `TheaterState`（直辖城集 + 各城 `GovernanceMode{SelfGoverned|Delegated}` + 受任 `CharacterId`）不可变、确定性哈希、存档。
- `SpanOfControlConfig`（rank→亲管上限，数据驱动）。
- `DelegateAction{Idle|Requisition|Repair|Appease}`（**仅本地**）· `DelegateGovernanceService`（确定性择动）。
- `TheaterResources`（城→粮，守恒 Transfer）。
- `TheaterCityReport`（城/方式/Fresh/汇报值）· `TheaterReportService`（委任城标非即时）。
- 复用：CityId/CityEconomy（GDD_004）· CharacterId（GDD_005）· Rank（GDD_014）· 占城 C（GDD_019）。

## 7. Player Inputs / System Outputs

- **输入**：委任/收回亲管（受官阶约束）· 跨城调粮 · 战区级决策 · 响应下属汇报。
- **输出**：战区态（多城+委任）· 委任城本地自理结果 · 守恒的资源账 · 反全知战区报告 · 掌管范围随官阶。

## 8. Dependencies

GDD_004/012/014/006/007/019 · ADR-0004/0006/0008/0009。反向依赖：GDD_004/014/019 须注记被 GDD_022 引用（多城/委任/占城入战区）。

## 9. Edge Cases

- 未持有城委任/收回 → 拒（NotHeld）。委任无下属 → 拒。跨城调粮源不足 → 拒（守恒）。占城已持有 → 幂等。降阶后亲管超上限 → 存量保留、不可再收回新城（渐进收敛，MVP 不强制降级）。

## 10. Failure Cases

命令拒绝不切死局（稳定错误码）；委任/资源不阻断主循环。

## 11. Balancing Parameters（延后打磨）

各官阶亲管上限 · 委任治理阈值 · 调粮成本/时延。**平衡打磨列为遗留任务。**

## 12. UI Requirements

战区面板：列直辖城（亲管/委任标注）+ 委任城标"下属汇报·可能滞后"（不显全知实时）；委任/调粮给稳定错误码文案；不显势力全知面板。键鼠可达。

## 13. AI Requirements

委任下属 AI：**仅本地治理动作空间**（结构级不越权）· 确定性规则化择动（可后续接 ADR-0006 种子化）。

## 14. Save / Load Requirements

TheaterState（持有/委任）+ TheaterResources 纳入统一存档，确定性哈希 round-trip 一致。

## 15. Test Requirements

- 直辖多城 + 委任 + 哈希 round-trip。
- 委任择动确定性 + **动作空间无战略（负向不变量）**。
- 跨城调粮守恒 + 源不足拒。
- 亲管范围随官阶（超阶拒/升官放宽）。
- 反全知报告（委任城 Fresh=false）。

## 16. MVP Scope

- TheaterState + span + 委任 + 委任本地择动 + 资源守恒 + 反全知报告。占城 C → 入战区（亲管默认）。

## 17. Future Scope

- 委任下属能力差异（能吏治得好）· 城间协同战区令 · 委任叛离（关系恶化）· 州级/战区级层级 · LLM 汇报文案。

## 18. Open Questions

- 委任城本地自理与会话日界结算的耦合粒度（每日 vs 每回合）——待接会话。
- 降阶后亲管超额的处置（强制委任 vs 渐进）——MVP 不强制。
