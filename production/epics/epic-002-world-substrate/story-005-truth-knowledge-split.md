# Story 005: 地图真值与阵营知识分离

> **Epic**: 世界基底（时间·环境·地图拓扑）
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-003-world-map.md
**Requirement**: TR-map-003

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: 真值与阵营知识分离；UI 只读知识投影，不触真值。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Foundation)**:
- Required: 同一信息唯一权威来源；显示层用只读投影
- Forbidden: 控制权改变自动揭示全部敌情；UI 读真值
- Guardrail: 敌区信息须由侦察更新

---

## Acceptance Criteria

- [ ] 地图真值与阵营知识为独立结构
- [ ] 敌区信息只能由侦察更新（epic-005 消费），不随控制权变更自动揭示
- [ ] 阵营知识投影为只读，不含真值字段
- [ ] 与情报四层（epic-005 TR-intel-001）契约一致（地图层是其空间载体）

---

## Implementation Notes

*Derived from ADR-0002:*
- 地图真值（区域真实归属/驻军）与「阵营已知」分表存储。
- 投影构造器只导出阵营合法知道的字段；测试断言投影无真值泄露。
- 与 epic-005 情报系统对接：侦察写阵营知识，不写真值。

---

## Out of Scope

- 侦察机制本体（epic-005）
- 知识序列化分离 → epic-009 Story（TR-intel-003）

---

## QA Test Cases

- **AC-1**: 投影无真值泄露
  - Given: 真值含未侦察敌区驻军
  - When: 构造阵营知识投影
  - Then: 投影不含该敌区真值字段
  - Edge cases: 控制权刚变更的区域仍不自动揭示
- **AC-2**: 侦察更新知识
  - Given: 一次侦察行动结果
  - When: 应用
  - Then: 仅阵营知识更新，真值不变

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Map/TruthKnowledgeSplitTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（8 测，全套 148/148 绿）
**Note**: 集成测试置于唯一可编译的 .NET 测试工程（自动化测试，跨 MapTruth/FactionKnowledge/投影/侦察联动验证）；路径由原写的 `tests/integration/map/truth_knowledge_split_test.cs` 归一。

---

## Dependencies

- Depends on: Story 004（拓扑）
- Unlocks: epic-005（情报四层空间载体）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（真值/知识独立结构、敌区只能由侦察更新不随控制权自动揭示、阵营知识投影只读且无真值字段、KnowledgeSource 对齐情报四层契约）
**Files**: `src/Domain/Map/`（FactionId、MapTruth+RegionTruth、FactionKnowledge+RegionKnowledge+MapKnowledgeProjection+KnowledgeSource、ScoutingService）+ `tests/unit/ThreeKingdom.Domain.Tests/Map/TruthKnowledgeSplitTests.cs`（8 测）
**Deviations**: ADVISORY — 集成测试置于 .NET 单元测试工程（唯一可编译测试工程）；测试路径归一。
**Test Evidence**: Integration — 自动化测试存在且通过（全套 148/148 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（真值/知识分表、投影无真值泄露、控制权变更不自动揭示、侦察为唯一跨越点）
