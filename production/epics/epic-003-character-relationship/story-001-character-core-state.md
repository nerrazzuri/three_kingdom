# Story 001: 人物核心状态与不变量

> **Epic**: 人物与关系
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-005-character.md
**Requirement**: TR-character-001

**ADR Governing Implementation**: ADR-0002 架构分层（secondary ADR-0004）
**ADR Decision Summary**: 人物为 Domain 权威；能力/性格/健康影响过程质量与意愿，不解锁无条件技能。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 人物状态变化产生可追踪原因；经 Command 路径
- Forbidden: 无条件技能/战斗光环；战斗临时复制角色数值
- Guardrail: 不变量由构造与测试保护

---

## Acceptance Criteria

- [ ] 人物核心状态：能力、性格、健康、身份、职责，构造时不变量校验
- [ ] 能力/性格/健康影响过程质量与执行意愿（输出修正系数，非解锁开关）
- [ ] 无任何「能力达标即解锁无条件技能」的路径（负向断言）

---

## Implementation Notes

*Derived from ADR-0002:*
- 人物为不可变值/聚合，状态变更经 Command 产生带原因的事件。
- 能力影响以确定性定点系数表达（epic-001），消费方（执行质量）自取。

---

## Out of Scope

- Story 002: 职责权限/执行意愿计算
- Story 003: 关系

---

## QA Test Cases

- **AC-1**: 不变量保护
  - Given: 非法构造参数（越界能力/冲突身份）
  - When: 构造人物
  - Then: 拒绝，不产出非法实例
  - Edge cases: 边界能力值、缺职责
- **AC-2**: 无无条件技能
  - Given: 高能力人物
  - When: 查询其可用行为
  - Then: 无任何无条件解锁的技能；能力只产出过程质量修正

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Characters/CharacterCoreStateTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（12 测，全套 160/160 绿）
**Note**: 路径由故事原写的 `tests/unit/character/character_core_state_test.cs` 归一到真实可编译测试工程。

---

## Dependencies

- Depends on: epic-001 Story 001/002
- Unlocks: Story 002, 003；epic-005/006/007（人物消费）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 3/3 passing（核心状态+构造不变量、能力/健康→过程质量系数、无无条件技能解锁负向断言）
**Files**: `src/Domain/Characters/`（CharacterId+RoleId、CapabilitySet+CapabilityDomain、PersonalityProfile+PersonalityTrait、HealthState+HealthLevel、TaskCapabilityWeights、CharacterState）+ `tests/unit/ThreeKingdom.Domain.Tests/Characters/CharacterCoreStateTests.cs`（12 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程（见 Test Evidence Note）
**Test Evidence**: Logic — 测试文件存在且通过（全套 160/160 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（质量公式匹配 GDD §1、满能力→系数 1.0 非解锁、连续无阈值跳变）
