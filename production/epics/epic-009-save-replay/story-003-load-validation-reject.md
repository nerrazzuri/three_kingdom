# Story 003: 加载校验与不兼容拒绝

> **Epic**: 存档与复现
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-013-save-load.md
**Requirement**: TR-save-003（+ TR-intel-003 真值/知识分离序列化）

**ADR Governing Implementation**: ADR-0005 存档版本与迁移
**ADR Decision Summary**: 加载先验证 schema/配置指纹/校验；不兼容不得部分载入当前会话。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM

**Control Manifest Rules (Foundation)**:
- Required: 加载先校验；不兼容不部分载入
- Forbidden: 真值与玩家知识序列化交叉污染
- Guardrail: 不兼容版本明确拒绝，给可行动原因

---

## Acceptance Criteria

- [x] 加载先验证 schema 版本 + 配置指纹 + 数据校验，任一不过则拒绝 — `SaveLoadService.Load` 结构完整性→版本→指纹→迁移 顺序校验
- [x] 不兼容（存档版本高于当前）被拒绝，不部分载入当前会话 — `LoadErrorCode.IncompatibleNewer`，纯函数零副作用，磁盘存档未改
- [x] 世界真值与玩家知识分别序列化，加载不交叉污染（TR-intel-003）— 同名键各归各位；知识段缺失走 Corrupted 拒绝而非真值回填
- [x] 拒绝返回可行动原因（供 main-menu/pause 错误态显示）— `LoadResult.Reason` 稳定错误码 + 中文可行动文案

---

## Implementation Notes

*Derived from ADR-0005 + epic-005 知识分离:*
- 校验顺序：版本兼容（SaveVersion）→ 配置指纹比对（epic-001 Story 003）→ 数据完整性。
- 真值段与知识段分块序列化，反序列化各归各位，断言无字段串台。
- 错误码稳定，UI（main-menu/pause-menu 错误态）据此显示可行动文案。

---

## Out of Scope

- 迁移链本体（Story 001）— 本 story 处理不可迁移的不兼容拒绝
- UI 错误态渲染（epic-010 / 已在 ux 规格定义）

---

## QA Test Cases

- **AC-1**: 不兼容拒绝
  - Given: 版本高于当前 / 指纹不符的存档
  - When: 加载
  - Then: 拒绝，当前会话状态不变（零部分载入），返回稳定错误
  - Edge cases: 版本恰高一位、指纹仅差一字段
- **AC-2**: 真值/知识不交叉污染
  - Given: 含真值 + 阵营知识的存档
  - When: 加载
  - Then: 两段各归位，知识投影不含真值
  - Edge cases: 知识段缺失 → 拒绝而非用真值回填

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Persistence/LoadValidationTests.cs` — 8 测全通过（归一到唯一可编译测试工程，ADVISORY 偏差）
**Status**: [x] Passed — 329/329 全绿，`-warnaserror` 0 warning

---

## Dependencies

- Depends on: Story 001（序列化）、epic-001 Story 004（SaveVersion）、epic-005 Story 001（知识分离）
- Unlocks: main-menu/pause 读档错误态（ux 规格已定义）
