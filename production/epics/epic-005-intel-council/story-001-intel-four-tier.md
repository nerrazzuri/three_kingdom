# Story 001: 情报四层分离与只读投影

> **Epic**: 情报与军议
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-007-intelligence-recon.md
**Requirement**: TR-intel-001

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: 真值/观察/报告/阵营知识四层分离；UI 只读阵营知识。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 同一信息唯一权威来源；显示层只读投影
- Forbidden: UI 读世界真值
- Guardrail: 玩家投影不泄露真值

---

## Acceptance Criteria

- [ ] 四层独立结构：世界真值 / 观察 / 报告 / 阵营知识
- [ ] UI 只能读取阵营知识投影（不含真值字段）
- [ ] 与地图真值/知识分离（epic-002 Story 005）契约一致
- [ ] 投影构造为只读，断言无真值泄露

---

## Implementation Notes

*Derived from ADR-0002:*
- 四层为分离类型；侦察产生「观察」→ 生成「报告」→ 更新「阵营知识」。
- 投影器只导出阵营合法字段；与 P1（不完全信息四层标识）UI 契约对齐。

---

## Out of Scope

- Story 002: 报告置信/时效/暴露
- Story 003: 军师建议

---

## QA Test Cases

- **AC-1**: 投影无真值泄露
  - Given: 真值含未观察敌情
  - When: 构造阵营知识投影
  - Then: 投影不含该真值
  - Edge cases: 报告与真值冲突时投影只含报告
- **AC-2**: 四层独立
  - Given: 更新观察
  - When: 结算
  - Then: 仅观察/报告/知识更新，真值不变

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/intel/intel_four_tier_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-002 Story 005（地图真值/知识分离）
- Unlocks: Story 002, 003；epic-009 Story 003（知识序列化分离）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（无 deferred）
**Deviations**: ADVISORY — 测试路径 `tests/integration/intel/*.cs` → 归一到 `tests/unit/ThreeKingdom.Domain.Tests/Intel/`
**Test Evidence**: Integration — `tests/unit/ThreeKingdom.Domain.Tests/Intel/IntelFourTierTests.cs`（8 测全绿，229/229 总）
**Code Review**: Complete（APPROVED）
