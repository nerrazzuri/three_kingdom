# Story 005: 无障碍横切（文本缩放/色盲冗余/减少动态/HUD 可见性持久）

> **Epic**: Slice UX 与可访问性
> **Status**: Complete（BLOCKING 逻辑 dotnet 379/379 + batchmode 编译干净；设置面板+三屏挂接 lead Play 签核通过 2026-06-23）
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-23

## Context

**GDD**: `design/accessibility-requirements.md`（WCAG 2.1 AA）+ hud §10 + 三屏 §11/§12
**Requirement**: 无障碍 MVP：键鼠全可达、文本缩放、色盲冗余、减少动态、HUD 可见性控制、跨会话持久

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: 无障碍设置为表现层关注点；不改 gameplay 规则；设置经 Infrastructure 端口持久（与存档分离）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM
**Engine Notes**: 文本缩放 150% 不溢出需 Editor 实测；屏幕阅读器 API 能力见 accessibility OQ-03（需 Spike）。

**Control Manifest Rules (Presentation)**:
- Required: 信息不靠颜色单一通道；键鼠全可达、焦点可见；可调项跨会话持久
- Forbidden: hover-only 信息；仅颜色区分关键操作
- Guardrail: 减少动态停用动效后无信息丢失

---

## Acceptance Criteria

- [ ] 文本 150% 缩放下三屏 + HUD 无溢出/重叠（9 组合）
- [ ] 无任一元素仅靠颜色区分（去色后信息仍可得；色盲冗余=纹样/形状/文字）
- [ ] 减少动态停用全部动效，无信息丢失（转场即时、雾叠加静止）
- [ ] HUD 可见性控制（§10.5）全部可调项可用且**跨会话持久**
- [ ] 完全静音下全程可玩，无信息丢失（字幕/旁白等价）
- [ ] 全部交互键鼠可达、焦点可见（朱批 2px ≥3:1），无 hover-only

---

## Implementation Notes

*Derived from ADR-0002 + accessibility-requirements:*
- `AccessibilitySettings` 模型（纯 C#）：textScale、colorblindMode、reduceMotion、hudVisibility 各项；范围校验。
- 持久：经 `ISettingsStore` 端口（与存档分离，复用 epic-009 原子写模式或独立设置文件）→ 跨会话 round-trip。
- 「信息不靠颜色」：展示模型字段携带冗余通道（形状/纹样/文字 token），可测断言关键状态去色后仍可区分。
- 减少动态：动效开关读 reduceMotion；逻辑层暴露「动效启用」布尔供 UXML 绑定。

---

## Out of Scope

- 各屏 UXML 本体（Story 002–004，本屏提供横切模型与持久）
- 屏幕阅读器深度集成（OQ-03 Spike；MVP 播报关键态）

---

## QA Test Cases

**可测逻辑（BLOCKING，EditMode/dotnet）：**
- **AC-4**: 设置持久 round-trip
  - Given: 一组无障碍设置
  - When: 保存→读取
  - Then: 各项值一致（跨会话持久）；非法值被拒
- **AC-2**: 信息不靠颜色单一通道
  - Given: 关键状态（如断粮/期限警告/势力归属）展示模型
  - When: 去色（仅看冗余通道）
  - Then: 形状/纹样/文字 token 仍唯一区分状态
- **AC-3**: 减少动态无信息丢失
  - Given: reduceMotion = true
  - When: 派生展示模型
  - Then: 动效启用=false，但全部信息字段仍在（无依赖动画才出现的信息）

**视觉/交互（ADVISORY，Unity 截图 + 签核）：**
- **AC-1**: 文本 150% 三屏 + HUD 9 组合无溢出/重叠（Editor 实测）
- **AC-6**: 焦点环 ≥3:1、全键鼠可达、无 hover-only

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- 可测逻辑：`tests/unit/ThreeKingdom.Domain.Tests/Presentation/AccessibilitySettings{,Store,ViewModel}Tests.cs`（BLOCKING，379/379 绿）
- 视觉/交互：`production/qa/evidence/accessibility-evidence.md`（lead Play 签核 2026-06-23，ADVISORY）
**Status**: [x] Passed — BLOCKING 全绿 + 编译干净 + lead Play 签核；精确视觉度量留 ADVISORY 可选后续

---

## Dependencies

- Depends on: Story 001（展示模型底座）；横切挂接 Story 002–004
- Unlocks: epic Definition of Done（无障碍 WCAG 2.1 AA MVP）
