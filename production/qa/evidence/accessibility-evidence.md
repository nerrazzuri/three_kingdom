# 测试证据 — EPIC_010 Story 005 无障碍横切 + 设置面板

> **Story**: `production/epics/epic-010-slice-ux/story-005-accessibility.md`
> **Type**: UI（BLOCKING 可测逻辑 + ADVISORY 视觉/交互）
> **签核日期**: 2026-06-23

## BLOCKING（自动化，门禁）

- **可测逻辑**:
  - `tests/unit/ThreeKingdom.Domain.Tests/Presentation/AccessibilitySettingsTests.cs`（设置模型校验 + 序列化 round-trip）
  - `…/AccessibilitySettingsStoreTests.cs`（持久 round-trip / 缺失·损坏优雅回落默认 / 写失败保留旧档 + 临时键清理）
  - `…/AccessibilitySettingsViewModelTests.cs`（文本缩放循环档位 / 不可变变换 / 色盲设定 / HUD 元素翻转 / persist→load 一致）
  - `…/StatusChannels`（去色冗余，`PresentationViewTests`）
- **结果**: `dotnet test … -warnaserror` → **379/379 全绿，0 warning**。
- **编译**: Unity batchmode `-nographics` → **无 `error CS`**，`Assembly-CSharp.dll` 产出。

## ADVISORY（视觉/交互，lead 签核）

- **设置面板场景**: `Assets/Scenes/AccessibilitySettings.unity`（UIDocument → `AccessibilitySettings.uxml` + `SlicePanelSettings`；`AccessibilitySettingsController` + EventSystem）。
- **挂接**: 三屏（MainMenu/Hud/PauseMenu）`OnEnable` 调 `AccessibilityApplier.Apply(root, AccessibilityRuntime.Current)`；设置经 `PlayerPrefsSettingsMedium` + `SettingsStore` 原子持久。
- **lead Play 签核（用户，2026-06-23）**: 面板渲染正常、按钮可点击；**文本缩放/色盲/减少动态/HUD 可见性切换即时生效（自我演示），功能全测 OK**。
- **AC-4 跨会话持久**: 由 `SettingsStore` round-trip 单测 + PlayerPrefs 介质实现共同保障（BLOCKING + 运行期）。

## 残留（ADVISORY，非阻断，可选后续）

- 精确度量未单独取证：文本 150% × 三屏+HUD 9 组合无溢出逐截图、焦点环对比度 ≥3:1 实测、色盲调色板（slice 阶段 `cb-*` 仅留 USS 钩子不改色，量产期补调色板/纹样）。
- 屏幕阅读器深度集成（accessibility-requirements OQ-03）属 Future，本 slice 不在范围。
