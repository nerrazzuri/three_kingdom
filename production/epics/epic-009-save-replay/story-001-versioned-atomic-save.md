# Story 001: 版本化 DTO + 原子写 + 迁移链

> **Epic**: 存档与复现
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-013-save-load.md
**Requirement**: TR-save-001

**ADR Governing Implementation**: ADR-0005 存档版本与迁移
**ADR Decision Summary**: 版本化 DTO + JSON 经 Infrastructure 端口；临时文件原子写；逆序逐版迁移链，只操作副本，失败保留上一份有效存档。

**Engine**: Unity 6.3 LTS + C# | **Risk**: MEDIUM
**Engine Notes**: 禁 Unity JsonUtility 处理 Domain 权威状态；用显式 DTO + 标准 JSON。

**Control Manifest Rules (Foundation)**:
- Required: 存档有 schema version 与迁移；原子写
- Forbidden: Unity 序列化 Domain 权威状态
- Guardrail: 写失败保留上一份有效存档

---

## Acceptance Criteria

- [x] Domain 快照 ↔ 版本化 DTO 双向映射（经 Infrastructure 端口）— `SaveSnapshot` + `ISaveSerializer`/`CanonicalSaveSerializer`（纯 BCL，禁 Unity 序列化）
- [x] 写入经临时文件 + 原子 rename，失败不破坏现有有效存档 — `SaveRepository.Save`（Write tmp → Move），临时写/改名失败均保留旧档
- [x] 逆序逐版迁移链：旧版本逐版迁移至当前，只操作副本 — `SaveMigrator`（按 From 唯一链路，快照不可变即副本）
- [x] 迁移失败保留原存档并返回稳定错误 — 步骤抛错→`MigrationErrorCode.StepFailed`，原快照引用未变；断链→`NoMigrationPath`

---

## Implementation Notes

*Derived from ADR-0005:*
- `ISaveRepository` 端口（Domain 接口，Infrastructure 实现）。
- DTO 含 SaveVersion（epic-001 Story 004）+ 配置指纹（epic-001 Story 003）。
- 迁移链：migrate(vN→vN+1) 函数序列，逆序应用于副本；slice 已有信封/迁移占位可参考重写。

---

## Out of Scope

- Story 002: Round-trip 一致性 / 随机流位置
- Story 003: 加载校验/不兼容拒绝

---

## QA Test Cases

- **AC-1**: 原子写不破坏现有档
  - Given: 已有有效存档 + 模拟写入中途失败
  - When: 保存
  - Then: 原存档完好，返回稳定错误
  - Edge cases: 临时文件写满/rename 失败
- **AC-2**: 迁移链逆序应用
  - Given: 旧版本存档
  - When: 加载触发迁移
  - Then: 逐版迁移至当前；只动副本
  - Edge cases: 迁移中途失败保留原档

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Persistence/VersionedAtomicSaveTests.cs` — 7 测全通过（归一到唯一可编译测试工程，ADVISORY 偏差）
**Status**: [x] Passed — 316/316 全绿，`-warnaserror` 0 warning

---

## Dependencies

- Depends on: epic-001 Story 003（配置指纹）、Story 004（SaveVersion）
- Unlocks: Story 002, 003
