# Story 002: 条件链涌现与复盘标签（无无条件按钮）

> **Epic**: 兵法沙盒结算
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md
**Requirement**: TR-battle-002

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 兵法为多条件全部成立时的涌现结果，系统只在事件后打复盘标签，绝无同名无条件执行按钮。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Core)**:
- Required: 兵法是条件组合，不是无条件技能按钮（强制设计锁）
- Forbidden: 同名无条件执行按钮；把条件链封装成单一开关
- Guardrail: 无前置条件 → 兵法不触发

---

## Acceptance Criteria

- [ ] 三条 slice 链可解析：假退伏击 / 断粮疲敌 / 守城待变（条件成立才涌现）
- [ ] 系统只在事件后打复盘标签，不存在「执行假退」按钮
- [ ] 无前置条件 → 兵法不触发（负向断言，逐链验证）
- [ ] 成立/暴露/失败均可解释（衔接 P5 因果链 Top≤5）
- [ ] 夜袭为执行手段，可与链组合，非独立技能

---

## Implementation Notes

*Derived from ADR-0004 + systems-index 兵法条件链:*
- 每链定义：可观察线索 / 必要条件 / 增强条件 / 投入 / 执行窗口 / 敌方反制 / 暴露方式 / 失败结果。
- 复盘标签在事件后由模式匹配打上（不影响结算，仅解释）。
- slice 三链（断粮 0.59→击退 / 伏击重创支队 / 守城待援击退）可作回归参考（重写）。

---

## Out of Scope

- Story 003: 士气三维细节
- 后果写回（epic-008）

---

## QA Test Cases

- **AC-1**: 无条件不触发
  - Given: 缺必要条件（如未保军纪/未侦察补给线）
  - When: 解析
  - Then: 对应兵法不涌现，无复盘标签
  - Edge cases: 条件恰好差一项
- **AC-2**: 条件成立涌现 + 标签
  - Given: 全部必要条件成立的命令流
  - When: 解析
  - Then: 涌现对应结果并事后打复盘标签；可解释 Top≤5 因素
  - Edge cases: 多链组合（断粮+伏击）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/battle/condition_chain_emergence_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（管线）；epic-004 Story 002（断粮）；epic-005（情报/军师）
- Unlocks: epic-008（后果写回）
