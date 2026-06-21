# Story 002: 三持有者补给守恒与路线断粮传导

> **Epic**: 城市与后勤
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-012-logistics-supply.md
**Requirement**: TR-supply-001、TR-supply-002

**ADR Governing Implementation**: ADR-0004
**ADR Decision Summary**: 守恒；断粮经实际路线切断 + 时段累积传导，无成功按钮，不立即崩溃。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 每个变化有来源与时段，资源不凭空
- Forbidden: 断粮成功按钮；断粮即全军崩溃
- Guardrail: 断粮传导单一权威（012 持 supply_state 发事件 / 011 唯一施加 morale·fatigue / 010 只读）

---

## Acceptance Criteria

- [ ] 粮食由城市/运输/携行三类持有者保存，转移守恒；同批不能同时留城与在途
- [ ] 断粮通过实际路线切断判定（衔接 epic-002 拓扑），非按钮
- [ ] 断粮按时段累积传导（012 发 supply_state 事件，011 据此施加 morale/fatigue）
- [ ] 不立即全军崩溃——渐进恶化（slice 已验证此机制为双边博弈）
- [ ] 单一权威：012 不直接改士气，010 只读 supply_state

---

## Implementation Notes

*Derived from ADR-0004 + systems-index S-W1 修复:*
- 三持有者用「移交」语义守恒（同 Story 001 模式）。
- 断粮 = 路线被切（拓扑）→ supply_state 随时段恶化 → 事件 → 011 消费。
- slice 的双边博弈（袭扰强度 vs 敌护卫 + 敌补给回补）可作设计参考（重写）。

---

## Out of Scope

- 士气/疲劳施加本体（epic-007 epic 消费）
- 外交补给入口（Story 003）

---

## QA Test Cases

- **AC-1**: 三持有者守恒
  - Given: 城市→运输→携行的转移序列
  - When: 结算
  - Then: 总量守恒；无同时留城与在途
  - Edge cases: 运输途中损耗、空运输
- **AC-2**: 断粮渐进传导
  - Given: 路线切断 N 个时段
  - When: 结算
  - Then: supply_state 逐段恶化并发事件；不立即崩溃；012 不直接改士气
  - Edge cases: 切断后恢复补给（回补）、恰好耗尽

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/supply/supply_conservation_cutoff_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001；epic-002 Story 004（路线拓扑）
- Unlocks: epic-007 Story 003（士气消费 supply 事件）
