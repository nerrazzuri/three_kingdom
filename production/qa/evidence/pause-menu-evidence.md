# 测试证据 — EPIC_010 Story 004 暂停菜单

> **Story**: `production/epics/epic-010-slice-ux/story-004-pause-menu.md`
> **Type**: UI（BLOCKING 可测逻辑 + ADVISORY 视觉/交互）
> **签核日期**: 2026-06-23

## BLOCKING（自动化，门禁）

- **可测逻辑**: `tests/unit/ThreeKingdom.Domain.Tests/Presentation/PauseMenuViewModelTests.cs`
  （5 态 + 保存失败错误态 + 草稿 P9 门控 / `ContinuationPromptView` 消费 epic-008 `OutcomeContinuation`，败局仍可继续）。
- **结果**: `dotnet test … -warnaserror` → **379/379 全绿，0 warning**。
- **编译**: Unity batchmode `-nographics` → **无 `error CS`**，`Assembly-CSharp.dll` 产出。

## ADVISORY（视觉/交互，lead 签核）

- **场景**: `Assets/Scenes/PauseMenu.unity`（UIDocument → `PauseMenu.uxml` + `SlicePanelSettings`；`PauseMenuController` + EventSystem）。
- **lead Play 签核（用户，2026-06-23）**: 进 Play 渲染正常、可交互；无障碍设置经 `AccessibilityApplier` 挂接（文本缩放/色盲/减少动态）。**功能签核通过**。
- **设计锁**: 失败延续「继续」契约（败局不切死局）由 epic-008 `OutcomeContinuation` 保障；读档不兼容/损坏 → 稳定错误码可行动文案（ADR-0005）。

## 残留（ADVISORY，非阻断，可选后续）

- 精确度量未单独取证：文本 150% 无溢出截图、对比度实测、焦点环 ≥3:1。交互层已 Play 确认；如需正式留痕，后续补截图。
