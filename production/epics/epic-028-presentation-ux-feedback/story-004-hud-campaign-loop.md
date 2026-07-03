# Story 004: HUD 战役主循环——治理/备战/战斗条件/下一步可做

> **Epic**: 表现与理解循环（M15 / epic-028）
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M / ~4h
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: —（/dev-story 开始时填写）

## Context

**UX 契约**: `design/ux/m15-campaign-loop-ux.md`（Approved 2026-07-03）§1 全循环信息架构 · §2.1 因果契约（治理→战役条件）· §4.4 多维不压扁
**Requirement**: `TR-ux-001`（治理因果分量）· `TR-ux-005`
*(需求原文在 `docs/architecture/tr-registry.yaml`，评审时读取最新版)*

**ADR Governing Implementation**: ADR-0002: 四层架构（primary）· ADR-0009: CampaignSession 装配 · ADR-0004: 确定性
**ADR Decision Summary**: 所有玩家操作构造 Command 经 `CampaignSessionService` 提交，失败返回稳定错误码无部分写入；HUD 只读投影。备战草稿不入 Domain，提交原子生成 CommittedPlan（TR-prep-001）。

**Engine**: Unity 6.3 LTS | **Risk**: MEDIUM
**Engine Notes**: 竖切 `HudController` 已有面板骨架与朱批视觉约定（hud.md §4）；本 story 是**数据源重定向 + 循环补全**，尽量不动布局。batchmode 验证 + 用户人工走查。

**Control Manifest Rules (this layer)**:
- Required: 兵法是条件组合非按钮（「还差 N 条」呈现方式不得暗示可点击触发）；所有平衡值数据驱动
- Forbidden: UI 直接改 Domain 状态；把民心/治安/工事/士气/疲劳合并为单一「实力」数字
- Guardrail: 任一循环态玩家须能看到「现在可做的合法动作」（AC-5）；1080p/1440p/4K 可读

---

## Acceptance Criteria

*From `m15-campaign-loop-ux.md` §1/§6（AC-1 治理分量 / AC-5 / AC-6）+ hud.md §4，scoped to this story:*

- [ ] HUD 世界状态/城市账本/推进时段全部重定向到 CampaignSession 投影（不再读 `GameSession`）
- [ ] 治理三动作（征用军粮/修工事/安抚）经 `RequisitionFood`/`RepairFortification`/`Appease` 提交；每个动作旁显示其**对后续战役条件的因果说明**（修工事↑守城强度 / 征用↑补给↓民心）——TR-ux-001 治理分量
- [ ] 备战面板：`AddPlanOrder`/`RemovePlanOrder` 草稿态与 `SubmitPlan` 已提交态视觉区分（朱批朱红，hud.md §4）；提交后不可反悔的承诺感明确
- [ ] 战斗态显示兵法条件进度（「还差 N 条」——已满足/未满足分列），**不呈现为可点击的技能按钮**
- [ ] AC-5：任一循环态（治理/备战/战中/战后）HUD 显示当前合法可做动作集；非法命令提交后按稳定错误码显示原因（不做 UI 侧预判吞掉）
- [ ] 多维状态分列：民心/治安/工事/守备/士气/疲劳各自成维显示，无合并总分——TR-ux-005
- [ ] ViewModel 纯函数渲染恒等

---

## Implementation Notes

*Derived from ADR-0002 状态变更协议 + ADR-0009 R-5:*

- 数据源切换清单：`WorldStatusView`（时间/时段）、`CityLedgerView`（账本）→ 会话投影；治理/备战/战斗命令 → `CampaignSessionService` 对应方法（已存在，见 `src/Application/Session/CampaignSessionService.cs` 公共面）。
- 治理因果说明文案来自配置/常量表（数据驱动），与 TR-city-004 的差异化影响一致——**说明「方向」不说明「精确数值预测」**（不泄露结算公式结果，无胜率精神同源）。
- 「还差 N 条」数据源：会话战斗投影的条件清单（console harness 文本原型已验证）；视觉上是**状态指示**（清单+勾选态）而非按钮排。
- 可做动作集：按会话当前相位（治理/备战中/战斗中/战后）由 ViewModel 过滤合法命令列表呈现——console `CampaignDriver` 的命令菜单即最小参考实现。
- 错误码→文案映射集中一处（勿散落各 Controller）；文案预留 40% 扩展。

---

## Out of Scope

*Handled by neighbouring stories — do not implement here:*

- Story 002: 战后复盘屏（战斗结束即跳转，本 story 不做复盘内容）
- Story 003: 军议/敌情面板内容契约（本 story 只保证 HUD 入口可达）
- Story 005: 新手期引导与无障碍专项
- 治理/备战/战斗 Domain 规则（M03/M05/M06 已交付，零新规则）

---

## QA Test Cases

*lean 模式 inline 编写。UI story：ViewModel 自动测试 + 人工走查。*

**自动（ViewModel，纯 C#）**
- **AC-2（治理+因果说明）**
  - Given: 会话治理相位投影
  - When: 渲染治理面板 ViewModel
  - Then: 三动作各带因果说明文本（含方向词↑↓）；提交 `RequisitionFood` 超库存 → 显示稳定错误码对应文案、账本不变
- **AC-3（草稿 vs 提交）**
  - Given: 加 2 条 PlanOrder 未提交
  - When: 渲染备战面板 → `SubmitPlan` → 再渲染
  - Then: 提交前草稿态标记、可移除；提交后已提交态标记、不可移除
- **AC-4（条件非按钮）**
  - Given: 战斗中投影（满足 2/3 条件）
  - When: 渲染条件面板
  - Then: 输出为条件清单（已满足✓/未满足✗ + 「还差 1 条」），无任何以兵法名命名的可执行命令项
- **AC-5（可做动作）**
  - Given: 治理/备战/战中/战后四相位投影各一
  - When: 渲染动作集
  - Then: 每相位动作集非空且互不相同、与该相位合法命令一致
- **AC-6/7（多维+恒等）**：账本渲染含民心/治安/工事各自字段无总分字段；同投影两次渲染逐字段相等

**人工走查（用户签核）**
- Setup: 新局→治理一轮→备战提交→开战→战中看条件→战毕
- Verify: 每一步 HUD 都能看到下一步可做什么；备战提交后视觉变化明显（朱批）；1080p 与 1440p 各查一遍可读性
- Pass condition: 全程无需查文档即可知道「现在能做什么」；无单一实力数字

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- `production/qa/evidence/story-004-hud-loop-evidence.md`（人工走查记录 + 截图 + 用户签核）
- ViewModel 自动测试于统一测试工程（`Presentation/HudCampaignViewModelTests.cs`）

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（会话接缝）
- Unlocks: Story 005（新手序在主循环 HUD 上加引导层）
