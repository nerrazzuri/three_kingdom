# Story 003: 后果原子写回（ConsequenceTransaction）

> **Epic**: CampaignSession 完整会话装配
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: M / 1d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes
**Completed**: 2026-06-28 · 全套 582/582 绿
**Test**: ConsequenceTransactionTests（7 测：守城胜归属不变/守城败在野+失城经004/原子校验失败零应用哈希不变/R-3势力创建经015/重复势力拒/确定性）
**Code Review**: inline lean — ADR-0009 R-6（原子写回 validate-first+回滚）/R-3（势力创建经015）/ADR-0008（归属经004）COMPLIANT
**实现**: ConsequenceTransaction（StageCareer/StageControlChange/StageFactionCreation + Commit 原子）· CampaignCommandResult · CampaignSessionService.ResolveSiege/BeginConsequence；WorldState.WithFaction + 投影 CreateFaction/RestoreTo；CampaignSession SetCareer/CreateFaction/RestoreWorld（internal）

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md` + `gdd-015` + `systems-index.md`
**Requirement**: `TR-session-002`、`TR-session-004`

**ADR Governing Implementation**: ADR-0009（primary，R-3/R-6）· ADR-0008（城池控制权）
**ADR Decision Summary**: 跨系统后果经命名 `ConsequenceTransaction` 原子写回（change-set→校验→提交，任一失败整批回滚、前后哈希一致）；城池归属经 GDD_004、势力创建经 GDD_015。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 C# 编排；无引擎面。

**Control Manifest Rules (this layer)**:
- Required: 后果经命令路径原子写回；失败必产生可继续状态
- Forbidden: 装配层直接写 city.owner / 势力存续；半结算
- Guardrail: 回合/时段制

---

## Acceptance Criteria

- [ ] 战役/事件后果经 `ConsequenceTransaction` 路由：change-set → 各权威系统校验 → 提交
- [ ] **原子性**：任一目标校验失败 → 整批回滚，回滚后状态哈希与提交前逐位一致
- [ ] 城池归属变更经 GDD_004 `ICityControlAuthority`（ADR-0008），装配层不直接写
- [ ] **自立新势力创建经 GDD_015**（R-3）：`RebellionState.NewFaction` → 015 创建 `FactionRecord`；装配层与 014 只读，不在 014 侧直写势力存续
- [ ] 守城败后果产出**合法可继续态**（生涯转在野，保留部曲）

---

## Implementation Notes

*Derived from ADR-0009 §R-3/R-6：*

- 复用 FIX-9 已证的链：GovernorOutcome（生涯）+ CityControlAuthority（归属）+ WorldCityProjection（世界）。本 story 把它们封进 `ConsequenceTransaction` 的原子提交语义。
- 势力创建：扩 `RebellionState.NewFaction` → 调 015 创建 FactionRecord（M00 发起请求，015 为权威，类比 ADR-0008）。
- 注入 BattleOutcome（真实战役解算 mock；代偿/抽象战果即可，§5b.1 裁决）。

---

## Out of Scope

- Story 004：存档 · 真实 GDD_010 战役命令层（M06）· 完整治理循环（M03）

---

## QA Test Cases

- **AC-1/2**: 原子写回
  - Given: 一个含 city+career+world 变更的后果包
  - When: 提交；构造其中一目标校验失败
  - Then: 整批回滚，状态哈希 == 提交前；成功则三系统一致更新
- **AC-3/4/5**: 归属/势力/败局
  - Given: 守城败（注入 BattleOutcome=Fallen）+ 自立分支
  - When: ConsequenceTransaction 提交
  - Then: 归属经 004 转敌方；自立 NewFaction 经 015 建 FactionRecord；生涯转在野可继续；装配层无直写 city.owner/势力存续

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/ConsequenceTransactionTests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001；**ADR-0009 R-3 落 method spec**（势力创建权威，已裁定）
- Unlocks: Story 005
