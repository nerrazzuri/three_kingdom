# 测试证据 — EPIC_010 Story 002 主菜单屏

> **Story**: `production/epics/epic-010-slice-ux/story-002-main-menu.md`
> **Type**: UI（BLOCKING 可测逻辑 + ADVISORY 视觉/交互）
> **签核日期**: 2026-06-23

## BLOCKING（自动化，门禁）

- **可测逻辑**: `tests/unit/ThreeKingdom.Domain.Tests/Presentation/MainMenuViewModelTests.cs`
  （5 态 / 读档错误态消费 `LoadResult` / 继续可用性）。
- **结果**: `dotnet test … -warnaserror` → **379/379 全绿，0 warning**（含全套 Presentation 测试）。
- **编译**: Unity 6000.3.18f1 batchmode `-nographics` → **无 `error CS`**，`Assembly-CSharp.dll` 产出（视觉壳正确绑定 `MainMenuViewModel` / `IntentTranslator`）。

## ADVISORY（视觉/交互，lead 签核）

- **场景**: `Assets/Scenes/MainMenu.unity`（UIDocument → `MainMenu.uxml` + `SlicePanelSettings`；`MainMenuController` + EventSystem）。
- **lead Play 签核（用户，2026-06-23）**: 进 Play 渲染正常；按钮可点击、键鼠可达；「新游戏/继续」意图经 `IntentTranslator` 触发（slice 阶段为命令载荷日志桩，非场景切换）。**功能签核通过**。
- **设计锁**: P10 无真值泄露 / P11 无最优解高亮 —— 由 `PresentationLockTests` 反射断言固化（BLOCKING）。

## 残留（ADVISORY，非阻断，可选后续）

- 精确度量未单独取证：1080p/1440p/4K × 文本 150% 无溢出的逐组合截图、焦点环对比度 ≥3:1 实测。交互层已 Play 确认可用；如需正式视觉保真留痕，后续补截图。
