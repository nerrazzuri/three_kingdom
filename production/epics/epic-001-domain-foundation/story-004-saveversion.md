# Story 004: SaveVersion 值对象

> **Epic**: 项目与 Domain 基础
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: S（2h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-013-save-load.md（版本兼容）
**Requirement**: TR-save-001（前置部分）、支撑 TR-save-003 / TR-time-003

**ADR Governing Implementation**: ADR-0005 存档版本与迁移
**ADR Decision Summary**: 显式版本化 DTO/JSON 经 Infrastructure 端口；逆序逐版迁移链；SaveVersion 表达 schema 兼容关系。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM

**Control Manifest Rules (Foundation)**:
- Required: 存档有 schema version 与迁移策略
- Forbidden: 用 Unity JsonUtility / Unity 序列化处理 Domain 权威状态
- Guardrail: 不兼容版本被明确识别，不静默降级

---

## Acceptance Criteria

- [ ] SaveVersion 值对象：解析、比较（<、=、>）、兼容判断
- [ ] 能表达「可迁移」「兼容」「不兼容（更新版本）」三类关系
- [ ] 不可变 + 相等性按值；非法版本字符串被拒绝

---

## Implementation Notes

*Derived from ADR-0005:*
- SaveVersion 为纯 Domain 值对象（无 IO）；迁移链本身落于 epic-009。
- 兼容判断规则：同主版本可迁移；存档主版本 > 当前 → 不兼容拒绝（TR-save-003）。

---

## Out of Scope

- 实际序列化/原子写/迁移链 → epic-009 Story 001/003
- Round-trip → epic-009 Story 002

---

## QA Test Cases

- **AC-1**: 版本比较与兼容判断
  - Given: 两个 SaveVersion
  - When: 比较 / 判断兼容
  - Then: 顺序与兼容结论符合规则
  - Edge cases: 相等版本、主版本差、存档版本高于当前（不兼容）
- **AC-2**: 非法版本被拒绝
  - Given: 非法版本字符串
  - When: 解析
  - Then: 抛出/返回稳定错误，不产出对象

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/foundation/saveversion_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（Domain 边界）
- Unlocks: epic-009 Story 001/002/003（存档实现）
