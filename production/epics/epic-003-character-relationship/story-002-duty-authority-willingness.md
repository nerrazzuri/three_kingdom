# Story 002: 职责权限与命令执行意愿

> **Epic**: 人物与关系
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-005-character.md（+ gdd-006 关系输入）
**Requirement**: TR-character-002（消费 TR-relationship-001 coop_score）

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟（secondary ADR-0002）
**ADR Decision Summary**: 执行意愿读关系已结算 coop_score，确定性计算（破环：关系事件结算 → 人物意愿）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 职责决定合法权限；同时段只承担兼容任务
- Forbidden: 能力高绕过授权
- Guardrail: 意愿计算确定性可复现

---

## Acceptance Criteria

- [ ] 职责决定合法权限范围；能力高不绕过授权
- [ ] 同一时段只能承担兼容任务（冲突任务被拒）
- [ ] 执行意愿由关系已结算 coop_score + 职责 + 人物因素确定性计算
- [ ] 破环顺序遵守：关系事件结算 → 人物意愿/质量（不读未结算关系值）

---

## Implementation Notes

*Derived from ADR-0004 + systems-index 破环（005↔006）:*
- 权限校验为纯函数（职责→允许命令集）。
- 意愿 = f(coop_score_已结算, 职责适配, 人物性格)，定点确定性。
- 任务兼容性：同时段任务集冲突检测。

---

## Out of Scope

- Story 003: 关系本体与 coop_score 产出（本 story 只消费已结算值）
- 部署占用冲突（epic-006）

---

## QA Test Cases

- **AC-1**: 授权不可绕过
  - Given: 高能力但无职责权限的人物
  - When: 请求越权命令
  - Then: 拒绝（能力不补权限）
  - Edge cases: 恰在权限边界的职责
- **AC-2**: 意愿确定性
  - Given: 固定 coop_score + 职责 + 性格
  - When: 计算意愿
  - Then: 结果确定可复现
  - Edge cases: coop_score 极端值、性格冲突

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/character/duty_authority_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001、Story 003（coop_score）
- Unlocks: epic-006（部署权限）、epic-007（执行质量）
