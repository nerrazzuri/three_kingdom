# Epic: onboarding 教新系统 + 人设入 HUD

> **Layer**: Presentation（首局可读性 / onboarding）
> **Governing ADR**: ADR-0002（分层）
> **GDD**: —（表现层偏好，不进权威存档）
> **Status**: ✅ Complete（可验证部分：dotnet 全绿；★ Unity 视觉待编辑器验证）

## 背景与问题

核心机制引导（"兵法条件组合非按钮"/"敌情只给估计"）本已存在。本 epic 补教新系统的引导点（六维备足决定胜负 / 攻心撬动 / 主角人设），并把人设显示接入 HUD。

## 设计裁定

1. 纯 C# 引导文案（OnboardingHints）dotnet 可测；Unity 侧只是薄壳。
2. 新引导点接 HUD 相位候选流（人设→治理相位、备足/攻心→备战相位）。
3. 人设标签入 HUD（SessionRuntime.Persona 委托 + Hud.uxml + HudController）。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | OnboardingHints +3 引导点（PreparationDecides/MindLever/PersonaIntro）+ HUD 候选流 + 人设标签 | UI | ✅（逻辑）/ ★ Unity 视觉待编辑器验证 |

## 完成说明

OnboardingHints +3 cue（原创文案，dotnet 已测）+ HudController 候选流接入 + SessionRuntime.Persona + Hud.uxml/HudController 人设标签。★ Assets/UI 改动用 UnityEngine，未经无头验证，需编辑器过一遍。测试 OnboardingViewModelTests。commit 1762c44→a377d7f。
