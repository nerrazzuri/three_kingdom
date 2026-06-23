# Epic: Slice UX 与可访问性

> **Layer**: Presentation
> **GDD**: design/ux/main-menu.md · design/ux/hud.md · design/ux/pause-menu.md（均 Approved）+ design/accessibility-requirements.md + design/ux/interaction-patterns.md
> **Architecture Module**: Presentation Layer（ADR-0002：MonoBehaviour/UI/输入/Scene；只提交 Command、执行 Query、订阅只读投影，不改核心状态）
> **Status**: Ready
> **Stories**: 见下方 Stories 表（详见 `/create-stories epic-010-slice-ux`）

## Overview

让玩家读懂信息、风险、命令与结果，并以接近功能原型（非完整美术）的品质在 Unity 6.3 UI Toolkit 上呈现 vertical slice 的核心闭环：主菜单 → HUD（生活观察/判断布局/行动承诺/战争应变/战果延续五态）→ 暂停菜单。本层**只读取 Domain 只读投影并提交 Command**，不拥有任何规则（systems-index「UI 只能展示状态并提交意图」）。所有降摩擦设计须过 P11 审查（无自动化捷径/无最优解高亮），关键设计锁（P10 无真值泄露 / P11 无最优解 / P6 多维不合并 / P4 双态）落入各屏验收。无障碍对齐 WCAG 2.1 AA（键鼠全可达、文本缩放、色盲冗余、减少动态、HUD 可见性控制）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | Presentation 仅依赖 Application API + 只读 DTO；构造意图 Command，订阅投影/事件，反向依赖禁止 | HIGH |
| ADR-0004: 确定性战斗模拟 | 表现层用 float 仅限非权威显示；不参与权威结算，不得反写状态哈希路径 | HIGH |
| ADR-0005: 存档版本与迁移 | 读档错误态（不兼容/损坏）经稳定错误码显示可行动文案（main-menu/pause 错误态） | MEDIUM |

## GDD Requirements

Presentation 层在 tr-registry 中**无独立 system slug**（它不拥有规则，只投影既有权威状态——同 epic-008「后果结算」处理方式）。本 epic 不引入未规约需求：其行为由下列 Approved UX 规格的验收标准 + ADR-0002 依赖方向约束派生。

| 契约项 | 来源 | ADR Coverage |
|---|---|---|
| 只读投影 → 展示状态；UI 意图 → Command（不直改核心状态） | architecture.md §状态变更协议 1/6；systems-index §命令与因果链 | ADR-0002 ✅ |
| 隐藏情报不泄露真值（P10）；无成功率/最优解高亮（P11） | hud.md §10 / main-menu §12 / interaction-patterns P10/P11 | ADR-0002/0004 ✅ |
| 多维状态不合并显示（P6）；双态呈现（P4） | hud.md §6 / interaction-patterns P4/P6 | ADR-0002 ✅ |
| 键鼠全可达 + 文本缩放 + 色盲冗余 + 减少动态 + HUD 可见性控制 | 三屏 §11/§12；accessibility-requirements WCAG 2.1 AA | ADR-0002 ✅ |
| 读档不兼容/损坏 → 可行动错误态 | pause-menu/main-menu §错误态；epic-009 LoadResult.Reason | ADR-0005 ✅ |

> 说明：若后续需独立追踪 Presentation 投影契约，运行 `/architecture-review` 追加 TR-ux-NNN。

## Definition of Done

This epic is complete when:
- 全部 stories 经 `/story-done` 关闭；三屏 §14/§12 + hud §12 验收标准全部验证
- Presentation 仅经 Application API + 只读投影/Command 交互（反向依赖审查通过，ADR-0002）
- 可测的表现逻辑（投影→展示模型、UI 意图→Command 映射）有通过的 EditMode/单元测试（BLOCKING）
- UXML/USS/Scene 视觉与无障碍以 Unity 截图 + lead 签核为证（ADVISORY，需 Editor）
- 设计锁负向断言通过：无真值泄露、无最优解高亮、无多维合并、键鼠全可达

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 投影→展示模型 + UI 意图→Command 映射底座（可测表现逻辑） | Logic | ✅ Complete | ADR-0002 |
| 002 | 主菜单屏（新游戏/继续/读档错误态 + 键鼠可达） | UI | 🔶 逻辑✅ + UXML 壳 compile✅ · 视觉签核待 Editor(ADVISORY) | ADR-0002/0005 |
| 003 | HUD 五态呈现（不完全信息 + 多维不合并 + 因果链） | UI | 🔶 逻辑✅ + UXML 壳 compile✅ · 视觉签核待 Editor(ADVISORY) | ADR-0002/0004 |
| 004 | 暂停菜单（存档/读档/设置 + 失败延续「继续」契约） | UI | 🔶 逻辑✅ + UXML 壳 compile✅ · 视觉签核待 Editor(ADVISORY) | ADR-0002/0005 |
| 005 | 无障碍横切（文本缩放/色盲冗余/减少动态/HUD 可见性） | UI | 🔶 逻辑✅ · 屏内 UI 集成待（设置面板未建） | ADR-0002 |

## Unity 视觉壳进度（2026-06-23）

- repo 根 Unity 6000.3.18f1 项目（matches CI 默认 projectPath）；`Assets/Plugins` 经 DLL 桥引用 `src/` 权威逻辑。
- 三屏 UXML/USS/Controller（MainMenu/Hud/PauseMenu）**batchmode 编译通过**（Assembly-CSharp.dll 产出，无 error CS）——证明视觉壳正确绑定 Presentation ViewModel/Intent。
- Editor 预览窗 `三国/UXML 视觉壳预览`（无需 Play/Scene）供视觉+无障碍**截图签核（ADVISORY）**。
- **剩余（ADVISORY，须 graphics 模式 Editor，属用户侧）**：三屏视觉/无障碍截图签核（对比度实测、文本 150%、键鼠焦点、色盲冗余）；可选 Scene+PanelSettings 进 Play 模式；S5 无障碍设置面板 + 各屏挂接。

> 上表为预期分解；以 `/create-stories epic-010-slice-ux` 正式产出为准。

## Next Step

Run `/create-stories epic-010-slice-ux` to break this epic into implementable stories.
