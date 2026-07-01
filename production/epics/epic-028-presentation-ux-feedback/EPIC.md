# Epic: Presentation / UX / Feedback（表现与理解循环 / M15）

> **Layer**: Presentation
> **UX Design**: `design/ux/m15-campaign-loop-ux.md`（primary）+ `hud.md` · `main-menu.md` · `pause-menu.md` · `interaction-patterns.md`
> **Architecture Module**: M15 Presentation / UX / Feedback Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M15）
> **Governing ADR**: ADR-0002（四层架构：Presentation 只读投影 + 只提交 Command）· ADR-0009（CampaignSession 装配接缝执行端）· ADR-0004（确定性渲染）
> **Status**: Draft for Review（design-first；交付物① 控制台 harness 已完成并作首个参考实现；待用户实玩反馈再 /create-stories）
> **Stories**: 待拆（见 §Next Step）

## Overview

让 M00~M10 建成的 11 循环**不只能操作，而是能被理解**。module-plan 给 M15 一句话职责：**让玩家知道自己为什么能赢、为什么会败、下一步还能做什么**。本 epic 不新增玩法规则，而交付贯穿整条战役循环的**理解与反馈层**——四个跨循环反馈契约（因果链 / 无胜率风险 / 情报置信时效 / 失败可继续续局）、全循环信息架构、新手循环序，以及表现层硬约束（只读投影 + 只提交 Command + 反全知类型兜底 + 确定性渲染）。

**交付物①（已完成，2026-07-01）**：CampaignSession 交互控制台 harness（`src/Console/`）——纯 C# 文本表面，11 循环端到端可玩，805/805 测试绿。它是本 epic 四反馈契约的首个最小参考实现。
**交付物②（后续 story）**：把同一 `CampaignSessionService` 接进 Unity 表现层（重写 `SessionRuntime` 指向 CampaignSession——现指向旧竖切 GameSession），按同一组验收在引擎面重新满足。

## Boundary（与既有 UX / 竖切的边界）

- **已交付**：竖切期单屏 UX 规范（hud.md / main-menu.md / pause-menu.md，2026-06-21，Approved，针对旧 `GameSession`）；M15 交付物① 控制台 harness（CampaignSession 全循环可玩）。
- **M15（本 epic）新增**：
  1. **全循环（loop-to-loop）信息架构**——11 循环之间如何流转、每步必须传达什么（既有 hud.md 只覆盖单局世界态常驻层）。
  2. **四个跨循环反馈契约**的统一设计 + 实现（因果/风险/置信/续局）。
  3. **新手循环序**（非仅首小时教程；完整难度矩阵归 M16）。
  4. **交付物② Unity 接线**：SessionRuntime → CampaignSession，4 场景 ViewModel/投影重定向。
- 复用既有只读投影/Intent（`src/Presentation/`，竖切期已建）；**不新增玩法规则**（M15 风险：UI 直接改 state 或 UI 自动推荐最优解）。

## 关键护栏（风险）

> module-plan §M15 风险：**UI 直接改 state；或用 UI 自动推荐最优解**。
- **只读投影 + 只提交 Command**：表现层不引用 Domain 可变内部态（ADR-0002）；harness 已由跨程序集可见性在编译期兜底。
- **无胜率 / 无最优解**（P11 / GDD_008 设计锁）：任何界面不出现成功率数字或唯一推荐；军师只给缘由/条件/风险/置信。
- **反全知**（P10）：敌情只呈估计值+来源+时效；真值类型层不可达。
- **失败可继续**（control-manifest 强制设计锁 + 终盘决策一「失败不删档」）：无任何「Game Over/删档」终点。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0002: 四层架构 | Presentation 只读 Application 投影 DTO + 只经用例提交 Command；不持/改可变 Domain 对象 | MED（Unity 接线时最易违反，须守接缝） |
| ADR-0009: CampaignSession 装配 | 表现层经会话用例操作；会话为唯一可玩脊梁；达内容平价前保留竖切 fixture | LOW |
| ADR-0004: 确定性 | 同会话态渲染恒等（纯函数）；同输入序列同结果；表现层 float/double 仅限非权威显示换算 | LOW |

## 需求（四反馈契约 + 硬约束）

> 详见 `design/ux/m15-campaign-loop-ux.md` §2/§4/§6。下列为待登记 TR（/create-stories 时补登 tr-registry，参照 M03 补登 TR-city-003~005 的做法）。

| 拟 TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-ux-001（因果） | 每个循环结果可展示 ≤5 主因素因果链；战斗复盘列出满足的兵法条件 + 识别出的成型兵法 | ADR-0002/0004 |
| TR-ux-002（风险） | 全游戏不出现成功率数字/唯一最优推荐；军议只给缘由/条件/风险/缺失情报/定性置信 | ADR-0002（P11/GDD_008 锁） |
| TR-ux-003（置信） | 敌情仅以估计值+来源+观测时效呈现；军议绑定知识快照、知识变化后旧建议标记过时；真值不可达 | ADR-0002（P10 反全知） |
| TR-ux-004（续局） | 四类战果分支每类呈现 ≥1 合法续局选项；无「游戏结束/删档」终点 | ADR-0009（失败可继续锁） |
| TR-ux-005（确定性渲染） | 同会话态渲染恒等；表现层不引用 Domain 可变内部态（编译期 + 单测可证） | ADR-0004/0002 |

## Scope

### In Scope
- 全循环信息架构 + 四反馈契约的设计落地（console harness 参考实现已交付①）。
- 新手循环序（沿自然循环序、in-world 缘由教学）。
- 交付物②：SessionRuntime 重写指向 CampaignSession；4 场景（MainMenu/Hud/PauseMenu/Accessibility）ViewModel/投影重定向到会话用例。
- 无障碍 Comprehensive 层关键项对齐（色盲冗余、文本缩放、键鼠全可达）。
- 表现层确定性 + 只读投影 + 只提交 Command 的测试证据。

### Out of Scope
- 新玩法规则 / 新平衡公式（M16 内容平衡）。
- 完整难度矩阵 / 教程关卡设计（M16）。
- 美术资产生产（art-bible 指导下的正式美术属后续；harness 为占位文本）。
- 外交（M11/epic-024）/ 多城（M12/epic-025）/ 终盘（M13-14）等尚未实现循环的 UI——随各循环落地再补。
- 音频实现（team-audio）。

## Definition of Done

This epic is complete when:
- 四反馈契约（因果/风险/置信/续局）在表现层统一落地，满足 `m15-campaign-loop-ux.md` §6 AC-1~AC-7。
- 表现层只读投影 + 只提交 Command（ADR-0002）；不引用 Domain 可变内部态（编译期 + 单测可证）。
- 反全知类型兜底：无显示敌方精确真值的路径。
- 失败可继续：四类战果分支均有续局选项；无 Game Over/删档终点。
- 交付物② Unity 4 场景接 CampaignSession，关键循环可在引擎面走通（batchmode 无 error CS + 人工走查证据）。
- 既有 M00~M10 + 竖切回归全绿。

## Next Step

1. **用户实玩 console harness**（`dotnet run --project src/Console`），攒可读性/反馈/onboarding 卡点反馈，回填 `m15-campaign-loop-ux.md` §7 Open Questions。
2. `/ux-review design/ux/m15-campaign-loop-ux.md` 验证后转 Approved。
3. `/create-stories epic-028-presentation-ux-feedback` 拆 story（补登 TR-ux-001~005）；建议首 story = 「战果复盘因果链」（最吃 UI、最能验证核心幻想），交付物② Unity 接线随后。
