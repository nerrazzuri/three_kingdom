# Story 003: 自立触发判定与三分支结局

> **Epic**: 战役与生涯
> **Status**: Complete
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: M / 0.5d（sprint-02 / sprint-status.yaml）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-27

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-002`（自立部分）
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0004（确定性模拟，primary）· ADR-0003（数据驱动配置）
**ADR Decision Summary**: 自立可发动判定与三分支结局由配置阈值 + 好感快照确定性产出；发动后好感变化不回溯改分支。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#；定点好感运算；判定确定性，无隐式随机。

**Control Manifest Rules (this layer)**:
- Required: 平衡值数据驱动；确定性可复现；失败必须产生可继续状态
- Forbidden: 硬编码自立阈值/分支阈值；float 进入权威路径
- Guardrail: 回合/时段制无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-014` §Formulas 2/3 / §Main Rules / §Edge Cases，scoped to this story:*

- [ ] `can_rebel = (cities_owned≥rebel_city_min ∧ supply_ready ∧ troops_ready) ∨ (renown≥rebel_renown_min ∧ avg(affinity)≥rebel_affinity_min) ∨ lord_oppression_flag`
- [ ] 三组触发条件任一成立即可发动（各组独立可验）
- [ ] `loyal_ratio = |{i: affinity[i]≥defect_threshold}| / N`；`≥hi`→全员拥立，`mid≤ratio<hi`→部分跟随，`<mid`→众叛亲离
- [ ] 三分支结局由发动时**好感快照**确定性判定；发动后好感变化不回溯改分支（GDD_014 §Edge Cases）
- [ ] 阈值 `rebel_city_min/renown_min/affinity_min`、`hi/mid/defect_threshold` 全部配置化
- [ ] 自立失败（众叛亲离）不卡死——沦为流浪势力为合法可继续状态

---

## Implementation Notes

*Derived from ADR-0004 / ADR-0003:*

- `RebellionState` 保存自立触发标志、发动时好感快照、结局分支、新势力初始态。
- `can_rebel` 与 `loyal_ratio` 为纯函数读 CareerState + RetinueState(好感, story-001) + 配置阈值；avg/比率用定点。
- 发动为 Command（带二次确认意图）：发动瞬间对 affinity 取快照存入 RebellionState，分支据快照定，后续好感变动不改已定分支。
- 三分支产出不同新势力初始态（拥立=完整继承；部分=半数跟随+开局平乱；众叛=少量亲卫流浪）。结局后玩法可简化（MVP）。
- 众叛亲离仍生成合法可继续的"流浪发育"最小态（与 story-004 在野态一致）。

---

## Out of Scope

- Story 001：CareerState/RetinueState/RebellionState 骨架
- Story 002：忠臣晋升线
- 自立后君主级国策与争霸玩法（GDD_014 §Future Scope）
- supply_ready/troops_ready 的完整后勤判定细节（读既有 GDD_012 状态，本 story 只消费布尔）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: can_rebel 三组条件独立
  - Given: 三组触发条件配置阈值
  - When: 分别只满足第 1 组（城池+补给+兵力）/ 第 2 组（名望+好感）/ 第 3 组（压迫 flag）
  - Then: 每组单独成立即 can_rebel=true；三组全不满足则 false
  - Edge cases: 第 1 组城池数恰等于 rebel_city_min（≥ 边界）；avg(affinity) 恰等于 rebel_affinity_min

- **AC-3 / AC-4**: 三分支由好感快照确定性
  - Given: N 名僚属好感分布
  - When: 计算 loyal_ratio 并判分支
  - Then: ratio≥hi→拥立 / mid≤ratio<hi→部分跟随 / <mid→众叛；同一快照→同一分支（哈希一致）
  - Edge cases: ratio 恰等于 hi / mid 边界；发动后改某 affinity 不改已定分支（快照隔离）

- **AC-6**: 众叛可继续
  - Given: 全员低好感发动自立
  - When: 结算众叛亲离分支
  - Then: 产出"少量亲卫流浪势力"合法可继续状态，不卡死、无非法态
  - Edge cases: N=0（无僚属）时分支判定不除零异常

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Career/RebellionTests.cs` — must exist and pass
**Status**: [x] Created — 14 test functions, all passing（全套 503/503 绿，-warnaserror 0）
**Path note**: 统一测试工程 `ThreeKingdom.Domain.Tests/Career/`（沿 epic-001 约定）。

---

## Dependencies

- Depends on: Story 001（CareerState/RetinueState/RebellionState 骨架）必须 DONE
- Unlocks: None（自立线 MVP 闭环；与忠臣线 story-002 并列）

---

## Completion Notes
**Completed**: 2026-06-27
**Criteria**: 6/6 passing（全部 COVERED）
**Deviations**: ADVISORY — 测试路径统一工程（同 11-1）。RebellionState 在本 story 建（11-1 未含，其 Out of Scope 指向本 story）。在 CareerState 补 `IntoOwnFaction`/`IntoWandering` 自立转换（11-1 review 时移除的 faction 转换以专门方法回归，避免 null 哨兵）。
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/Career/RebellionTests.cs`（14 测；全套 503/503 绿，-warnaserror 0）
**Code Review**: Complete — inline lean，ADR-0004（确定性+好感快照隔离+N=0 不除零+零 float）/ADR-0003（阈值配置化+指纹）COMPLIANT
**实现文件**: `src/Domain/Career/`（RebellionOutcome/RebellionConfig/RebellionContext+Eligibility/RebellionState/RebellionResult/RebellionService + CareerState 转换方法）+ `src/Application/Career/RebellionCommandService.cs`
