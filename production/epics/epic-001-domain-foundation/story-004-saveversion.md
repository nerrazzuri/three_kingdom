# Story 004: SaveVersion 值对象

> **Epic**: 项目与 Domain 基础
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: S（2h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

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
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Persistence/SaveVersionTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（26 测，全套 74/74 绿）
**Note**: 路径由故事原写的 `tests/unit/foundation/saveversion_test.cs` 归一到真实可编译测试工程（`foundation/` 不在任何 csproj）。

---

## Dependencies

- Depends on: Story 001（Domain 边界）
- Unlocks: epic-009 Story 001/002/003（存档实现）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 3/3 passing（解析/比较/兼容三类关系、非法版本拒绝、不可变+值相等）
**Files**: `src/Domain/Persistence/SaveVersion.cs`（SaveVersion + SaveCompatibility）+ `tests/unit/ThreeKingdom.Domain.Tests/Persistence/SaveVersionTests.cs`（26 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程（见 Test Evidence Note）
**Test Evidence**: Logic — 测试文件存在且通过（全套 74/74 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（纯值对象，ADR-0005 兼容三类 + 不静默降级）
