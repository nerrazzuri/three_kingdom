# Story 003: 版本化配置加载与校验

> **Epic**: 项目与 Domain 基础
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: 横切（支撑 TR-weather-001、TR-city-002 及全部数值）
**Requirement**: 前置能力 — 版本化配置加载/校验 + 配置指纹

**ADR Governing Implementation**: ADR-0003 数据驱动配置
**ADR Decision Summary**: ScriptableObject 仅编辑期；构建时转为不可变 Domain 配置 + 配置指纹；运行时权威状态不依赖 SO。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM

**Control Manifest Rules (Foundation)**:
- Required: 所有平衡值数据驱动；方法体内不硬编码平衡数值
- Forbidden: ScriptableObject 作为运行时权威状态
- Guardrail: 配置非法 → 稳定错误，无部分写入

---

## Acceptance Criteria

- [ ] 配置经端口加载为**不可变** Domain 配置对象（构建期 SO→不可变转换）
- [ ] 范围校验：超出合法范围的数值被拒绝并返回稳定错误码
- [ ] 引用完整性校验：缺失引用被拒绝
- [ ] 配置指纹：对一份配置产生稳定指纹（供战役复现校验 TR-battle-001 / 存档校验 TR-save-003）
- [ ] 校验失败不产生部分加载状态

---

## Implementation Notes

*Derived from ADR-0003:*
- 定义 `IConfigLoader` 端口（Domain 侧接口，Infrastructure 实现）。
- 校验分两阶段：schema/范围 → 引用完整性；任一失败聚合错误后整体拒绝。
- 指纹对不可变配置内容哈希（复用 Story 002 哈希底座）。

---

## Out of Scope

- 各系统具体配置 schema（落于各自 epic）— 本 story 只建管线 + 校验框架
- 存档配置指纹比对 → epic-009 Story 003

---

## QA Test Cases

- **AC-1**: 非法范围被拒绝
  - Given: 一份含超范围数值的配置
  - When: 加载
  - Then: 返回稳定错误码，无 Domain 配置对象产出
  - Edge cases: 边界值（恰在上/下限）、空集合、负值
- **AC-2**: 缺失引用被拒绝
  - Given: 引用不存在键的配置
  - When: 加载
  - Then: 拒绝 + 指出缺失引用
- **AC-3**: 配置指纹稳定
  - Given: 同一配置加载两次
  - When: 计算指纹
  - Then: 指纹相等；任一值变更 → 指纹变化

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Configuration/ConfigLoadValidateTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（18 测，全套 48/48 绿）
**Note**: 路径由故事原写的 `tests/unit/foundation/config_load_validate_test.cs` 归一到真实可编译测试工程（`foundation/` 不在任何 csproj，否则不会被编译/运行）。

---

## Dependencies

- Depends on: Story 001（Domain 边界）、Story 002（哈希用于指纹）
- Unlocks: 全部依赖配置的 epic（002/004/005/007）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 5/5 passing（范围/边界/负值/空集、引用完整性、指纹稳定+变化+顺序无关、无部分写入、稳定错误码）
**Files**: `src/Domain/Configuration/`（ConfigIds、ConfigResult、ConfigSchema、ConfigDraft、ValidatedConfig、ConfigValidator、IConfigLoader）+ `tests/unit/ThreeKingdom.Domain.Tests/Configuration/ConfigLoadValidateTests.cs`（18 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程（见 Test Evidence Note）
**Test Evidence**: Logic — 测试文件存在且通过（全套 48/48 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（重构 Validate 拆分以满足 <40 行/复杂度<10）
