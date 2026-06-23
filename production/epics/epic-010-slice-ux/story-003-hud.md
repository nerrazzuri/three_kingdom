# Story 003: HUD 五态呈现（不完全信息 + 多维不合并 + 因果链）

> **Epic**: Slice UX 与可访问性
> **Status**: Complete（BLOCKING 逻辑 dotnet 379/379 + batchmode 编译干净；视觉壳 lead Play 签核通过 2026-06-23）
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: `design/ux/hud.md`
**Requirement**: hud §12 验收（布局/情境正确性/不完全信息核心设计锁/无障碍/通知/平台）

**ADR Governing Implementation**: ADR-0002 架构分层（secondary ADR-0004 因果链确定性/float 仅显示）
**ADR Decision Summary**: HUD 订阅只读投影展示；敌方仅探报不显真值（P10）；军师无最优解（P11）；多维不合并（P6）；战果因果链可展开但终值不变（ADR-0004）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: UI Toolkit 运行时 UI；中央 40% 区无常驻面板；视觉预算/帧需 Editor 实测（ADVISORY）。

**Control Manifest Rules (Presentation)**:
- Required: 己方=完整账本、敌方=探报；多维分列；UI 只展示+提交意图
- Forbidden: 敌方精确实时真值；最优解/一键布阵；多维合并；hover-only
- Guardrail: 战果因果链可整体跳过，终值不变（确定性）

---

## Acceptance Criteria

- [ ] 每个 §5 情境（生活观察/判断布局/行动承诺/战争应变/战果延续）只显示规定元素，转场计时符合 §10.3
- [ ] 全屏模态（军议/暂停/读档）正确隐去全部 HUD
- [ ] 敌方探报**绝不**显示精确实时真值（仅推测/区间/时效/来源，P10/P1/P2）
- [ ] 军师入口不提供「最优解/一键布阵」（P11）；己方=账本、敌方=探报，数据形态不同（P10）
- [ ] 多维状态（士气/疲劳/军纪、关系）分列不合并（P6）
- [ ] 战果态因果链可逐步展开且可整体跳过，终值不变（P12/ADR-0004）
- [ ] 同类通知 500ms 合并；临界警告（断粮/期限/校验失败）绕队列立即显示；同时 toast ≤3
- [ ] 全 HUD 键盘可达、焦点可见、无 hover-only

---

## Implementation Notes

*Derived from ADR-0002 + ADR-0004:*
- `HudViewModel`（Story 001 底座扩展）：情境枚举 → 元素集映射；敌方行只绑探报展示模型（无真值字段）。
- 因果链：消费 `OutcomeContinuation`/复盘标签，逐步展开为「来源→修正→结果」，整体跳过直达终值（哈希一致）。
- 通知队列：合并窗 500ms、临界绕队、并发 ≤3、场景切换清空——纯逻辑可测。
- 多维：cohesion 三维 + 关系四维各自绑定，无综合值控件。

---

## Out of Scope

- Story 004：暂停/读档模态本体（本屏只负责被其隐去）
- Story 005：色盲/文本缩放/减少动态/可见性持久（本屏挂接）

---

## QA Test Cases

**可测逻辑（BLOCKING，EditMode/dotnet）：**
- **AC-1**: 情境→元素集
  - Given: 五种情境
  - When: 派生 HudViewModel
  - Then: 元素集与 §5 规定一致；全屏模态态元素集为空（HUD 隐去）
- **AC-3/AC-4**: 不完全信息负向断言
  - Given: 含真值的世界 + 敌方探报投影
  - When: 构造敌方 HUD 展示模型 + 军师入口模型
  - Then: 反射断言无真值字段、无 successRate/optimal 字段
- **AC-6**: 因果链跳过终值不变
  - Given: 一条因果链
  - When: 逐步展开 vs 整体跳过
  - Then: 终态值/哈希相等
- **AC-7**: 通知合并/优先级
  - Given: 同类 + 临界混合通知流
  - When: 入通知系统
  - Then: 同类 500ms 合并、临界绕队即时、并发 ≤3

**视觉/交互（ADVISORY，Unity 截图 + 签核）：**
- **AC-1 布局**: 1280×720/1080p/1440p/4K + 21:9 无重叠/截断；生活态 <12%、战争态 <26% 占屏；中央 40% 无常驻面板

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- 可测逻辑：`tests/unit/ThreeKingdom.Domain.Tests/Presentation/HudViewModelTests.cs`（BLOCKING）
- 视觉/交互：`production/qa/evidence/hud-evidence.md` + Unity 截图 + lead 签核（ADVISORY）
**Status**: [x] Passed — BLOCKING 全绿 + 编译干净 + lead Play 签核（见 hud-evidence.md）；精确视觉度量留 ADVISORY 可选后续

---

## Dependencies

- Depends on: Story 001（展示模型/意图）、epic-005/007/008（情报/士气/因果链投影）
- Unlocks: 完整 slice 游玩闭环呈现
