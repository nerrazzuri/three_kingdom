# Story 004: 区域/路线拓扑与确定性寻路

> **Epic**: 世界基底（时间·环境·地图拓扑）
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-003-world-map.md
**Requirement**: TR-map-001、TR-map-002

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 寻路确定性，平局按稳定 ID 序；坐标不参与结算。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Foundation)**:
- Required: 寻路确定性；区域容量门控
- Forbidden: 视觉像素坐标进入 Domain 结算
- Guardrail: 同图 + 同起讫 → 同路径

---

## Acceptance Criteria

- [ ] 地图为区域/路线拓扑图；单位无权威像素坐标（视觉坐标不参与结算）
- [ ] 确定性寻路：平局按稳定 ID 序选择
- [ ] 区域容量门控：超容量进入被阻止/排队
- [ ] 相向移动接触判定确定

---

## Implementation Notes

*Derived from ADR-0004:*
- 拓扑图：节点（区域）+ 带权边（路线，权重含地形/天气已结算修正，衔接 Story 003）。
- 寻路用整数代价；优先队列平局键 = stableId。
- 容量门控与接触判定在移动结算阶段（衔接 epic-007 管线移动步）。

---

## Out of Scope

- Story 005: 真值/知识分离
- 战斗内移动解析细节（epic-007）

---

## QA Test Cases

- **AC-1**: 寻路确定性
  - Given: 同拓扑 + 同起讫（含等代价多路径）
  - When: 寻路
  - Then: 返回同一路径（平局按 stableId）；多次一致
  - Edge cases: 不连通（返回无路径）、等代价分叉
- **AC-2**: 容量门控与接触
  - Given: 满容量区域 / 相向移动
  - When: 移动结算
  - Then: 超容被阻；相向接触确定触发
  - Edge cases: 恰好满容、同时进入

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Map/TopologyPathfindingTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（16 测，全套 140/140 绿）
**Note**: 路径由故事原写的 `tests/unit/map/topology_pathfinding_test.cs` 归一到真实可编译测试工程。

---

## Dependencies

- Depends on: epic-001 Story 002（定点）、Story 003（边权读天气修正）
- Unlocks: epic-004（补给路线）、epic-006/007（部署/移动）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 4/4 passing（区域/路线拓扑无像素坐标、确定性寻路平局按 RouteId 字典序、容量门控 occ+n≤cap、相向接触双向+进度和≥1.0）
**Files**: `src/Domain/Map/`（MapIds、Region、Route、WorldMap、MapFormulas[RouteCost/RouteContact]、Pathfinder+PathResult）+ `tests/unit/ThreeKingdom.Domain.Tests/Map/TopologyPathfindingTests.cs`（16 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程（见 Test Evidence Note）
**Test Evidence**: Logic — 测试文件存在且通过（全套 140/140 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（整数代价 Dijkstra + RouteId 序列字典序平局、坐标不参与结算、route_time 定点 ceil）
