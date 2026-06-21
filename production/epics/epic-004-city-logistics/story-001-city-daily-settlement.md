# Story 001: 城市日界产耗结算与资源守恒

> **Epic**: 城市与后勤
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-004-city-economy.md
**Requirement**: TR-city-001、TR-city-002

**ADR Governing Implementation**: ADR-0004（secondary ADR-0003）
**ADR Decision Summary**: 资源结算用整数/定点，守恒可验证；日界按稳定顺序。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 战争投入消耗城市可见资源并留后果；平衡值数据驱动
- Forbidden: 资源凭空出现；硬编码产耗率
- Guardrail: 资源不低于合法下限；守恒恒等

---

## Acceptance Criteria

- [ ] 民用与军用粮食来自同一权威库存；拨给军队后转后勤持有不重复计算（守恒）
- [ ] 日界按稳定顺序结算：承诺 → 产入 → 消耗 → 短缺后果 → 工事/治安
- [ ] 资源不低于合法下限（夹取并记短缺后果，不出负）
- [ ] 守恒恒等：产入 − 消耗 − 转移 = 库存差

---

## Implementation Notes

*Derived from ADR-0004 + systems-index 日界顺序:*
- 城市日结在全局日界顺序「补给之后」（补给不消费同边界新产粮——有意约束）。
- 粮食转移用「移交」语义：城市库存减、后勤持有增，单一权威，无双计。

---

## Out of Scope

- Story 002: 补给三持有者/断粮传导
- Story 003: 外交入口

---

## QA Test Cases

- **AC-1**: 守恒恒等
  - Given: 一个日界含产入/消耗/向军队转移
  - When: 结算
  - Then: 产入−消耗−转移 = 库存差；无双计
  - Edge cases: 转移超库存（夹取+短缺后果）、零产入
- **AC-2**: 日界顺序
  - Given: 同时存在承诺消耗与短缺
  - When: 结算
  - Then: 顺序为 承诺→产入→消耗→短缺后果→工事/治安
  - Edge cases: 资源触下限

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/city/city_daily_settlement_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-001 Story 002/003；epic-002 Story 001（日界顺序）
- Unlocks: Story 002；epic-007（城市状态约束战斗）
