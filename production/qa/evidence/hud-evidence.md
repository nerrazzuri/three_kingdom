# 测试证据 — EPIC_010 Story 003 HUD 五态呈现

> **Story**: `production/epics/epic-010-slice-ux/story-003-hud.md`
> **Type**: UI（BLOCKING 可测逻辑 + ADVISORY 视觉/交互）
> **签核日期**: 2026-06-23

## BLOCKING（自动化，门禁）

- **可测逻辑**: `tests/unit/ThreeKingdom.Domain.Tests/Presentation/HudViewModelTests.cs`
  （情境→元素集 + 模态隐去 / 因果链跳过终值不变 / 通知合并 500ms·临界绕队·并发≤3）。
- **结果**: `dotnet test … -warnaserror` → **379/379 全绿，0 warning**。
- **编译**: Unity batchmode `-nographics` → **无 `error CS`**，`Assembly-CSharp.dll` 产出。

## ADVISORY（视觉/交互，lead 签核）

- **场景**: `Assets/Scenes/Hud.unity`（UIDocument → `Hud.uxml` + `SlicePanelSettings`；`HudController` + EventSystem）。
- **lead Play 签核（用户，2026-06-23）**: 进 Play 渲染正常、可交互；无障碍设置经 `AccessibilityApplier` 挂接（HUD 元素可见性复合于情境可见性之上——只额外隐藏，不强制显示）。**功能签核通过**。
- **设计锁**: P10 不完全信息不泄露真值 / P6 多维（士气·疲劳·军纪）不合并 —— `PresentationLockTests` 反射断言固化（BLOCKING）。

## 残留（ADVISORY，非阻断，可选后续）

- 精确度量未单独取证：文本 150% 五态 × 多分辨率无溢出截图、对比度实测、色盲去色可辨逐项核对。交互层已 Play 确认；如需正式留痕，后续补截图。
