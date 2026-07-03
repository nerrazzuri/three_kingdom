# Story 003: 军议与敌情屏——定性置信 + 时效 + 无胜率

> **Epic**: 表现与理解循环（M15 / epic-028）
> **Status**: Complete
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M / ~3h
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-04

## Context

**UX 契约**: `design/ux/m15-campaign-loop-ux.md`（Approved 2026-07-03）§2.2 风险契约 · §2.3 置信/时效契约 · §7 Q1 裁决
**Requirement**: `TR-ux-002` · `TR-ux-003`
*(需求原文在 `docs/architecture/tr-registry.yaml`，评审时读取最新版)*

**ADR Governing Implementation**: ADR-0002: 四层架构（primary）· ADR-0009: CampaignSession 装配
**ADR Decision Summary**: 投影出口分离真值与玩家知识，UI 只读玩家合法知识（反全知入契约，R-4）；军师建议为条件化输出，无综合成功率/唯一推荐（TR-council-002 Domain 已锁，本层不得回退）。

**Engine**: Unity 6.3 LTS | **Risk**: MEDIUM
**Engine Notes**: 沿竖切 HUD 的军议/敌情面板 Controller 模式重定向；batchmode 验证 + 用户人工走查。

**Control Manifest Rules (this layer)**:
- Required: 军师建议而不自动排兵布阵或执行最优方案（强制设计锁）
- Forbidden: 显示敌方精确真值；成功率数字；唯一推荐；侦察后旧建议静默刷新
- Guardrail: 置信档位阈值数据驱动（勿硬编码映射边界）

---

## Acceptance Criteria

*From `m15-campaign-loop-ux.md` §6（AC-2/AC-3）+ §7 Q1 裁决，scoped to this story:*

- [x] 军议面板重定向到 `CampaignSessionService.ConveneCouncil`：显示缘由 + 所需条件 + 风险 + 待证情报，**全屏无成功率数字、无「推荐方案」**——TR-ux-002
- [x] 军师「完整度/置信」以**定性档（高/中/低）**呈现，**不显示小数**——§7 Q1 裁决；档位映射阈值来自配置
- [x] 敌情面板重定向到会话知识投影：每条敌情 = 估计值 + 来源 + 「N 段前」时效；超过时效告警阈值（IntelConfig.ttlSegments）高亮为过时——TR-ux-003
- [x] 军议绑定召开时知识快照：召开后再侦察（知识变化），旧建议显式标记「过时」，不静默更新（`IsStaleAgainst` 语义）
- [x] 类型层反全知：本屏代码取不到敌方真值（只经玩家知识投影出口；编译期可证）
- [x] ViewModel 纯函数渲染恒等

---

## Implementation Notes

*Derived from ADR-0002/0009 + §7 Q1:*

- 数据源：`ConveneCouncil(session)` 返回 `CouncilAdviceSet`；敌情经会话知识投影（对应竖切 `EnemyReportView`/`ScoutView` 的会话版）。复用 `src/Presentation/Projections/CouncilView.cs`/`EnemyIntelView.cs`，把数据源从 `SessionService` 换到会话投影。
- **小数→定性档映射发生在纯 C# ViewModel 层**（如 `<0.4 低 / 0.4–0.7 中 / >0.7 高`，阈值进 Tuning Knobs 配置）；Domain 的小数完整度字段保持不动（console harness 显示小数亦不改——其定位为内部工具，§7 Q4）。
- 档位除文字外须有非纯色冗余编码（如形状/字重，AC-7 色盲要求）。
- 过时标记：对比建议快照时间与当前知识版本；「过时」为显式徽标 + 建议重开军议的提示，不禁用旧建议（玩家可仍按旧建议行事——决策自由，P11）。
- 时效告警阈值与 `IntelConfig.ttlSegments` 同源（读同一配置，勿另立常量）。

---

## Out of Scope

*Handled by neighbouring stories — do not implement here:*

- Story 004: 侦察派出操作与 HUD 主循环整合（本屏只做军议/敌情的**呈现契约**；派出按钮若已在竖切面板存在，仅重定向数据源）
- Story 005: 军议自动显示前 N 回合的新手序
- 军议/情报 Domain 规则（M04 已交付，零新规则）

---

## QA Test Cases

*lean 模式 inline 编写。UI story：ViewModel 自动测试 + 人工走查。*

**自动（ViewModel，纯 C#）**
- **AC-2（定性档）**
  - Given: 完整度 0.2 / 0.55 / 0.9 三条建议投影 + 阈值配置(0.4, 0.7)
  - When: 渲染军议 ViewModel
  - Then: 分别显示「低/中/高」；输出文本不含任何 `0.` 开头小数或 `%`
  - Edge cases: 恰在阈值边界（0.4/0.7）→ 归属确定且有测试锁定（含边界规则注明）
- **AC-1/无胜率扫描**
  - Given: 任一建议集渲染输出全文
  - When: 断言扫描
  - Then: 不含「成功率/胜率/%/推荐方案/最优」字样
- **AC-3/4（时效+过时）**
  - Given: 敌情条目观测于 3 段前（阈值 8）与 10 段前各一；军议召开后知识更新一次
  - When: 渲染敌情与军议 ViewModel
  - Then: 前者正常显示「3 段前」，后者标过时高亮；旧建议带「过时」徽标且内容与召开时快照一致（未静默变化）
- **AC-6（恒等）**：同投影两次渲染逐字段相等

**人工走查（用户签核）**
- Setup: 开局→侦察→召开军议→再侦察一次
- Verify: 置信只见高/中/低；敌情有来源与「N 段前」；第二次侦察后旧军议出现过时标记
- Pass condition: 全程无小数置信、无胜率、无敌方精确真值

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- `production/qa/evidence/story-003-council-intel-evidence.md`（人工走查记录 + 截图 + 用户签核）
- ViewModel 自动测试于统一测试工程（`Presentation/CouncilIntelViewModelTests.cs`）

**Status**: [x] Created + passing（8/8；全套 829/829）；人工走查证据待用户签核

---

## Dependencies

- Depends on: Story 001（会话接缝）
- Unlocks: Story 005（军议自动显示新手序基于本屏）

---

## Completion Notes
**Completed**: 2026-07-04
**Criteria**: 6/6 passing（全部由自动 ViewModel 测试覆盖；UI 接线经 Unity batchmode 0-error 编译 + 人工走查清单）
**Deviations**（ADVISORY）:
- 新建战役面向 ViewModel `CouncilIntelView.cs`（Screens），未按 Implementation Notes「复用 `Projections/CouncilView`/`EnemyIntelView`」——理由：直接改旧视图会破 `PresentationLockTests`/`PresentationViewTests`/`CouncilConveneTests`（断言「依据薄弱/中等/扎实」）且污染 console 内部工具（§7 Q4 保持不动）；新 VM 边界更干净。旧竖切视图仍供 console/slice。
- 「派出侦察」为即时侦察（`CampaignRuntime.ScoutEnemy` 直连 `_service.Scout`）——延迟「派出→在途→返报」派出循环 + 主循环命令托盘整合归 story-004（本 story Out of Scope 明列，仅重定向按钮数据源）。
- 触及 `CampaignRuntime`/`SessionRuntime`/`HudController`——UI 接线所需，属预期范围。
**Test Evidence**: UI story — 自动测试 `tests/unit/ThreeKingdom.Domain.Tests/Presentation/CouncilIntelViewModelTests.cs`（8 测）+ 人工走查证据 `production/qa/evidence/story-003-council-intel-evidence.md`（自动段已填，用户签核位待补）。
**Code Review**: Complete（本会话 `/code-review` lean inline → APPROVED；ADR-0002/0009 COMPLIANT，反全知/数据驱动/确定性均合规）。
