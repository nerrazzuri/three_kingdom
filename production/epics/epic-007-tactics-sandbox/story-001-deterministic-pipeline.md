# Story 001: 确定性战役解析管线与状态哈希

> **Epic**: 兵法沙盒结算
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md
**Requirement**: TR-battle-001、TR-battle-003

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 同快照+配置指纹+种子+有序命令流→同事件与状态哈希；阶段按稳定管线解析，异常回滚整个原子阶段。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: 权威路径禁 float；跨平台一致靠定点（epic-001）。

**Control Manifest Rules (Core)**:
- Required: 战斗结果可确定性复现
- Forbidden: float 在 Domain 权威路径；隐式随机
- Guardrail: 同种子+同输入→同状态哈希

---

## Acceptance Criteria

- [ ] 解析管线固定顺序：验证 → 移动 → 侦测 → 交战 → 损耗 → 士气 → 触发 → 发布
- [ ] 同初始快照 + 配置指纹 + 随机种子 + 有序命令流 → 同事件与状态哈希
- [ ] 解析异常回滚整个原子阶段（不留半结算态）
- [ ] slice 33 测试可作回归基线（重写为正式测试，验证哈希复现）

---

## Implementation Notes

*Derived from ADR-0004 + GDD_010 §2 管线:*
- 管线为有序步骤，每步纯函数式推进快照；阶段事务化（失败回滚）。
- 命令流有序化（按稳定 ID）；随机流注入（epic-001），position 随档（epic-009）。
- 复用 slice BattleResolver 设计（≤5 决定性因素、确定性）——重写，不 import。

---

## Out of Scope

- Story 002: 条件链涌现/复盘标签
- Story 003: 士气三维
- 跨系统后果写回（epic-008）

---

## QA Test Cases

- **AC-1**: 状态哈希复现
  - Given: 同快照+指纹+种子+命令流
  - When: 两次解析
  - Then: 事件序与状态哈希位级一致
  - Edge cases: 不同种子→不同哈希；命令流乱序→稳定化后一致
- **AC-2**: 阶段原子回滚
  - Given: 某阶段中途异常
  - When: 解析
  - Then: 整个阶段回滚，无半结算态
  - Edge cases: 最后一步异常

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/battle/deterministic_pipeline_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: epic-001 Story 002；epic-002 Story 002（时段）；epic-006（CommittedPlan）
- Unlocks: Story 002, 003；epic-008（后果）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（slice 回归基线以新正式测试覆盖哈希复现/突然性/回滚）
**Deviations**: ADVISORY — 测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/Battle/`；士气/触发/撤退步为占位（士气步读 GDD_011 只读，触发/撤退链由 S2 承接）
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/Battle/DeterministicPipelineTests.cs`（7 测全绿，275/275 总）
**Code Review**: Complete（APPROVED）
