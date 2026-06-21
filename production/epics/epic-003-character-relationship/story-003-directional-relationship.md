# Story 003: 方向性多维关系与事件幂等

> **Epic**: 人物与关系
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-006-relationship-faction.md
**Requirement**: TR-relationship-001、TR-relationship-002

**ADR Governing Implementation**: ADR-0004（secondary ADR-0002）
**ADR Decision Summary**: 方向性多维关系（A→B 与 B→A 分存、不对称），变化由具名事件驱动且幂等。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 关系不压缩成单一好感值；向消费者提供意愿/约束（P6）
- Forbidden: 关系凭空授予法律权限
- Guardrail: 同一事件对关系幂等

---

## Acceptance Criteria

- [ ] 关系方向性多维：A→B 与 B→A 独立保存、可不对称
- [ ] 变化由具名事件驱动，且对同一事件 ID 幂等（重复应用不叠加）
- [ ] 关系只影响请求/支持/执行承诺（产出 coop_score），不凭空授权
- [ ] 授权有效性受期限/撤销规则约束（TR-relationship-002）
- [ ] 不输出单一综合好感值（P6 多维不合并）

---

## Implementation Notes

*Derived from ADR-0004:*
- 关系存为 (from, to, dimension)→value；事件带稳定 ID 用于幂等去重。
- coop_score 为已结算只读输出（供 Story 002 消费，破环顺序在前）。
- 授权 = 关系 + 职责 + 期限的组合判断，非关系单独决定。

---

## Out of Scope

- 意愿消费（Story 002）
- 后果写回关系（epic-008）

---

## QA Test Cases

- **AC-1**: 不对称与幂等
  - Given: 对 A→B 应用某具名事件两次
  - When: 结算
  - Then: 仅生效一次；B→A 不受影响
  - Edge cases: 同事件不同时段、并发同 ID
- **AC-2**: 不凭空授权
  - Given: 高信任但无职责
  - When: 请求授权
  - Then: 不授予法律权限；仅提高承诺意愿
  - Edge cases: 授权过期/被撤销

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/relationship/directional_relationship_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-001 Story 002；Story 001
- Unlocks: Story 002（消费 coop_score）、epic-008（关系写回）
