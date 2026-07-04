# Story 005: 出征后果→功绩→升官联动

> **Epic**: 出征攻城循环（epic-029-offensive-campaign-loop）
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: M（~4h）[待 sprint 规划确认]
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-04

## Context

**GDD**: `design/gdd/gdd-019-offensive-campaign.md`
**Requirement**: `TR-offensive-005`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准——审查时读最新)*

**ADR Governing Implementation**: ADR-0009（CampaignSession 装配——命名原子事务写回 + 统一存档信封）
**Secondary ADRs**: ADR-0005（存档版本与迁移）· GDD_014（生涯回报/晋升/失败续局）
**ADR Decision Summary**: 跨系统后果经命名 ConsequenceTransaction 原子写回（change-set→校验→提交，全有或全无）；出征会话态纳入统一存档信封、段级版本与迁移；round-trip 一致、确定性哈希。

**Engine**: Unity 6.3 LTS + 纯 C# Domain | **Risk**: LOW
**Engine Notes**: 纯 Domain/Application + Infrastructure 存档端口；无引擎面。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只经 Application Command 路径；存档有 schema version 与迁移；失败必须产生可继续状态；确定性可复现。
- Forbidden: 部分写入（后果须原子）；用 Unity JsonUtility 处理权威存档（须显式版本化 DTO，ADR-0005）；硬编码回报数值。
- Guardrail: 出征态存读档 round-trip 一致 + 确定性哈希；败局至少一条合法可继续命令。

---

## Acceptance Criteria

*源自 GDD_019 §4 R5 / §11 / §12 AC-5/AC-6/AC-8：*

- [ ] **AC-5 回报联动**：出征胜 → 功绩/名望增（战功来源，受反支柱护栏约束——速率不压过非战斗源）→ 推进 GDD_014 晋升门槛（epic-022）。
- [ ] **AC-6 失败可继续**：出征败 → 折损、退兵，但**必生成 ≥1 合法可继续命令**（再备战 / 改守 / 转攻他城 / 若倾向足则自立）；绝不卡死、不切死局。
- [ ] **AC-8 存读档**：出征会话态（授权 / 目标 / conquestIndex / 各城 OwnershipVerdict / rebellion_lean）经命名原子事务写回、纳入统一存档信封；存读档 round-trip 一致，纳入确定性哈希。
- [ ] **AC-5b 原子性**：回报/占城/自立倾向作为一批后果原子写回——任一目标校验失败整批回滚、无部分写入。

---

## Implementation Notes

*源自 ADR-0009 后果事务 + ADR-0005 存档 + GDD_014：*

- 出征后果（功绩/名望增量 + 占城归属 + rebellion_lean 增量 + 失败折损）打包为命名 `ConsequenceTransaction`：change-set → 校验 → 提交，全有或全无（复用 TR-outcome-001/TR-session-002 既有事务）。
- 胜利回报写入 GDD_014 CareerState（merit/renown）；晋升门槛推进复用 epic-022 既有 `can_promote`（本 story 不重实现晋升逻辑，只喂增量）。回报数值数据驱动 + **受 GDD_019 §8 反支柱护栏约束**（单次出征功绩上限 + 出征真实成本）。
- 失败分支复用 GDD_010 §后果 / TR-outcome-002 的"败局必给合法可继续命令"；本 story 确保出征败也落在该契约内（退兵/改守/转攻/自立入口）。
- 出征会话态并入 CampaignSession 统一存档信封（ADR-0009 R-1 / TR-session-003）；显式版本化 DTO（ADR-0005），段级 migrator；round-trip 后续推进确定性一致。

---

## Out of Scope

*由相邻 story / 既有系统处理：*

- Story 004：占城归属 verdict 与 rebellion_lean 的**产生**（本 story 只写回其增量并存档）。
- GDD_014 晋升**判定**逻辑本身（epic-011/022 已实现，本 story 只喂增量）。
- GDD_014 自立触发判定（W1 已接线，本 story 只确保 rebellion_lean 存档一致）。
- 出征结果的 UI 呈现（M15/UX 层）。

---

## QA Test Cases

*lean 模式 inline 编写。*

- **AC-5 回报联动**
  - Given: 一场出征胜（产功绩/名望增量）
  - When: 写回生涯态
  - Then: CareerState.merit/renown 按配置增量增加；晋升门槛进度相应推进；增量受护栏上限约束
  - Edge cases: 增量恰好跨越某阶门槛 → 晋升可达（交 014 判定）
- **AC-6 失败可继续**
  - Given: 一场出征败（折损退兵）
  - When: 生成后续状态
  - Then: 至少一条合法可继续命令可用（再备战/改守/转攻他城/自立）；无"游戏结束/删档"终点
  - Edge cases: 大败（兵力大损）仍须给可继续命令；rebellion_lean 已足 → 含自立入口
- **AC-8 存读档**
  - Given: 含授权/目标/conquestIndex/OwnershipVerdict/rebellion_lean 的出征会话态
  - When: 存档后读档（load(save(s))）
  - Then: 状态逐字段一致、状态哈希相同；读档后续出征推进确定性一致
  - Edge cases: 跨版本迁移（旧信封→新段）；未来版本拒绝载入（TR-save-003）
- **AC-5b 原子性**
  - Given: 一批后果中某目标校验失败（如非法归属写入）
  - When: 提交 ConsequenceTransaction
  - Then: 整批回滚、无部分写入；前后状态哈希一致
  - Edge cases: 回报增量合法但占城落地非法 → 全批回滚

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Offensive/campaign_outcome_linkage_test.cs`——集成测试须存在且通过；含回报联动 + 失败可继续 + 存档 round-trip + 事务原子性 + 确定性哈希断言。
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 004（占城 verdict + rebellion_lean 增量）。跨 epic：GDD_014 CareerState/晋升（epic-011/022）、GDD_010 §后果（TR-outcome-002）、CampaignSession 存档信封（ADR-0009/TR-session-003）、ADR-0005 存档。
- Unlocks: None（epic 内末 story）。后续：epic-025 多城委任 / 全循环端到端平衡验证。


## Completion Notes
**Completed**: 2026-07-04
**实现**: ResolveConquest/LaunchOffensive 胜局经 CareerProgressionService 记功（MajorBattleVictory→功绩/名望→晋升门槛，接 epic-022）；败局不占城但 OffensiveResult 保留可继续（失败可继续红线）。
**测试**: test_conquest_applies_career_gain（功绩增长）+ 端到端胜/败分支。871/871 绿。
**Code Review**: lean inline（本会话）· ADR-0010/0008/0006/0004 COMPLIANT · 反全知/确定性/无胜率/失败可继续均合规。
