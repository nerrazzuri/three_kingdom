# Story 004: 暂停菜单（原子存档错误态 + 草稿处置 P9 + 失败延续「继续」）

> **Epic**: Slice UX 与可访问性
> **Status**: In Progress（可测逻辑 BLOCKING 完成；UXML 视觉壳待 Unity）
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: `design/ux/pause-menu.md`
**Requirement**: pause §14 验收（布局/输入/事件数据/无障碍/本地化）

**ADR Governing Implementation**: ADR-0002 架构分层（secondary ADR-0005 原子存档/读档错误态）
**ADR Decision Summary**: 暂停期间无任何权威状态推进；保存经端口原子写、失败给可行动原因；未提交草稿退出/读取先经 P9 明确处置；本屏不直接写权威状态。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM
**Engine Notes**: 背景暗化 60% 静止；视觉需 Editor 验证（ADVISORY）。

**Control Manifest Rules (Presentation)**:
- Required: 保存原子写失败给可行动原因；草稿退出/读取经 P9；Esc 即时 Resume
- Forbidden: 从屏内直接退游戏；暂停期间推进权威状态；部分加载
- Guardrail: 保存失败不留部分状态（ADR-0005）

---

## Acceptance Criteria

- [ ] 全部状态（默认/有草稿/保存中/保存失败/退出确认）正确渲染
- [ ] 背景世界暗化 60% 且静止（暂停期间无任何权威状态推进）
- [ ] 全部项纯键盘可达；Esc 即时 Resume，绝不从屏内直接退游戏
- [ ] 保存为原子写：失败不留部分状态（ADR-0005），给可行动原因
- [ ] 有未提交草稿时，退出/读取均先经 P9 明确草稿处置（默认焦点安全项）
- [ ] 读取不兼容/损坏存档走错误态，不崩溃、不部分加载
- [ ] 失败延续「继续」契约：败局存档可继续（消费 epic-008 OutcomeContinuation 合法命令集）

---

## Implementation Notes

*Derived from ADR-0002 + ADR-0005:*
- `PauseMenuViewModel`（Story 001 底座扩展）：状态 { Default, HasDraft, Saving, SaveError, QuitConfirm }。
- 保存：经 `SaveRepository.Save`（epic-009）→ `SaveResult` 成功/失败（TempWriteFailed/CommitFailed）→ 错误态展示可行动 Reason。
- 草稿处置 P9：HasDraft 时退出/读取前强制确认（默认焦点安全项=不丢草稿）。
- 「继续」契约：消费 `OutcomeContinuation.Options`（非空保证），即便败局也提供可继续命令入口。

---

## Out of Scope

- Story 003：HUD 本体（暂停时被隐去）
- Story 005：无障碍横切规则
- 存档/读档机制本体（epic-009，已完成）

---

## QA Test Cases

**可测逻辑（BLOCKING，EditMode/dotnet）：**
- **AC-4**: 保存错误态映射
  - Given: SaveResult 失败（TempWriteFailed）
  - When: 派生 PauseMenuViewModel
  - Then: 进入 SaveError 态，含可行动 Reason；不留部分状态
- **AC-5**: 草稿处置门控
  - Given: HasDraft = true
  - When: 触发退出/读取意图
  - Then: 先要求 P9 处置确认，默认焦点为安全项
- **AC-7**: 失败延续「继续」
  - Given: 一个败局 OutcomeContinuation
  - When: 构造「继续」入口模型
  - Then: Options 非空，存在 ≥1 合法可继续命令

**视觉/交互（ADVISORY，Unity 截图 + 签核）：**
- **AC-2/AC-3**: 背景暗化 60% 静止；Esc 即时 Resume、绝不退游戏；纯键盘可达

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- 可测逻辑：`tests/unit/ThreeKingdom.Domain.Tests/Presentation/PauseMenuViewModelTests.cs`（BLOCKING）
- 视觉/交互：`production/qa/evidence/pause-menu-evidence.md` + Unity 截图 + lead 签核（ADVISORY）
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（展示模型/意图）、epic-008（OutcomeContinuation）、epic-009（SaveResult/LoadResult）
- Unlocks: 胜败均可继续的循环闭环呈现
