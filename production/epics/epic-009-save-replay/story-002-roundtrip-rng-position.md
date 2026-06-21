# Story 002: Round-trip 一致性与随机流位置保存

> **Epic**: 存档与复现
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-013-save-load.md
**Requirement**: TR-save-002（+ TR-time-003 事件序一致）

**ADR Governing Implementation**: ADR-0005（secondary ADR-0004）
**ADR Decision Summary**: load(save(s))≡s；保存随机流位置，读档不重抽已发生结果。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Foundation)**:
- Required: 存档兼容已验证；确定性回归通过
- Forbidden: 读档重新抽取已发生随机结果
- Guardrail: round-trip 状态哈希一致

---

## Acceptance Criteria

- [ ] load(save(s)) 的状态哈希 ≡ s 的状态哈希（epic-001 哈希底座）
- [ ] 随机流位置随档保存；读档后续抽取与未存档继续一致
- [ ] round-trip 后行动耗时/期限事件序列一致（TR-time-003）
- [ ] 在途外援/在途行动 round-trip 后存活（slice 已验证此类）

---

## Implementation Notes

*Derived from ADR-0005 + ADR-0004:*
- 快照含全部权威状态 + 各随机流 position（epic-001 DetRng position）。
- 测试：构造态 s → save → load → 比哈希；再各推进 N 段比事件序。
- slice 的 4 项存档测试（哈希保持/读档后续确定/在途外援存活/未来版本拒绝）可作回归参考（重写，不 import）。

---

## Out of Scope

- Story 003: 不兼容拒绝（本 story 假设兼容路径）
- 真值/知识序列化分离 → Story 003 关联 TR-intel-003

---

## QA Test Cases

- **AC-1**: Round-trip 哈希一致
  - Given: 任意权威态 s
  - When: load(save(s))
  - Then: 状态哈希相等
  - Edge cases: 含在途行动/外援、空集合
- **AC-2**: 随机流位置保持
  - Given: 抽取若干次后的态
  - When: save→load→再抽
  - Then: 后续序列与未存档继续一致
  - Edge cases: position 在抽取边界

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/save/roundtrip_rng_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（序列化）、epic-001 Story 002（随机流/哈希）
- Unlocks: 全战役复现回归（epic-007）
