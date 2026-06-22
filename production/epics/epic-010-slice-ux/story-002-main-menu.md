# Story 002: 主菜单屏（5 态 + 读档错误态 + 键鼠可达）

> **Epic**: Slice UX 与可访问性
> **Status**: In Progress（可测逻辑 BLOCKING 完成；UXML 视觉壳待 Unity）
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: `design/ux/main-menu.md`
**Requirement**: main-menu §14 验收（布局/输入/事件/无障碍/本地化）

**ADR Governing Implementation**: ADR-0002 架构分层（secondary ADR-0005 读档错误态）
**ADR Decision Summary**: 本屏只提交 New Game/Load/Quit 意图 + 读 SaveSlot 投影，不直接写权威状态；不兼容/损坏存档走错误态（LoadResult.Reason）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM
**Engine Notes**: UI Toolkit UXML/USS + UGUI 不混用；UXML 绑定 Story 001 的 `MainMenuViewModel`。视觉/渲染验证需 Editor（ADVISORY）。

**Control Manifest Rules (Presentation)**:
- Required: 全部项纯键盘可达、焦点可见；本屏不直接写权威状态
- Forbidden: 根屏 Esc 退出游戏；hover-only 状态
- Guardrail: 损坏/不兼容存档不部分加载、不崩溃

---

## Acceptance Criteria

- [ ] 全部状态（有/无存档/读档中/失败/退出确认）正确渲染（§6/§14）
- [ ] 全部项纯键盘可达（↑↓/Tab + Enter），焦点全程可见，根屏 Esc 不退出游戏
- [ ] New Game/Load/Quit 事件以正确载荷在各退出路径触发；本屏不直接写权威状态（仅经 Application）
- [ ] 损坏/不兼容存档走错误态，不部分加载、不崩溃（消费 epic-009 LoadResult.Reason）
- [ ] 全部文本走本地化字符串，最长译文无溢出

---

## Implementation Notes

*Derived from ADR-0002 + ADR-0005:*
- `MainMenuViewModel`（Story 001 底座扩展）：状态机 { NoSave, HasSave, Loading, LoadError, QuitConfirm }；读 SaveSlot 投影派生「继续」可用性。
- 读档错误态：消费 `LoadResult`（IncompatibleNewer/Corrupted/FingerprintMismatch）→ 展示 Reason 可行动文案。
- UXML/USS：Zone 布局见 §5；键盘导航 + 焦点环（朱批 2px）。意图经映射提交，不触达 Domain。

---

## Out of Scope

- Story 001：展示模型/意图映射底座
- Story 005：文本缩放/色盲/减少动态横切（本屏挂接，规则在 005）

---

## QA Test Cases

**可测逻辑（BLOCKING，EditMode/dotnet）：**
- **AC-1**: 状态机派生
  - Given: 无存档 / 有存档 / LoadResult 失败 三种投影
  - When: 构造 MainMenuViewModel
  - Then: 状态分别为 NoSave/HasSave/LoadError；「继续」仅 HasSave 可用
- **AC-4**: 读档错误态映射
  - Given: IncompatibleNewer / Corrupted 的 LoadResult
  - When: 派生错误态
  - Then: 含可行动 Reason 文案，不进入游戏会话（零部分加载）

**视觉/交互（ADVISORY，Unity 截图 + 签核）：**
- **AC-2**: 键鼠可达
  - Setup: batchmode 外用 Editor 打开主菜单 Scene
  - Verify: ↑↓/Tab 遍历全部项、焦点环可见、根屏 Esc 不退游戏
  - Pass condition: 全部项纯键盘可达且焦点全程可见
- **AC-1 渲染**: 1080p/1440p/4K + 16:9/21:9 无重叠/截断/拉伸

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- 可测逻辑：`tests/unit/ThreeKingdom.Domain.Tests/Presentation/MainMenuViewModelTests.cs`（BLOCKING）
- 视觉/交互：`production/qa/evidence/main-menu-evidence.md` + Unity 截图 + lead 签核（ADVISORY）
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（展示模型/意图底座）、epic-009（LoadResult）
- Unlocks: 进入 HUD（Story 003）/ 暂停（Story 004）的导航闭环
