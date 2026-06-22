# Story 003: 军师条件化建议（无最优解/无成功率）

> **Epic**: 情报与军议
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-008-war-council.md
**Requirement**: TR-council-001、TR-council-002

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: 军议读召开时知识快照；军师只输出条件化建议，不输出综合成功率/唯一推荐/自动命令。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 军师建议而不自动排兵布阵或执行最优方案（强制设计锁）
- Forbidden: 综合成功率、唯一推荐、自动命令、暴露隐藏真值
- Guardrail: 知识/资源变化后建议标记过时，不静默更新

---

## Acceptance Criteria

- [ ] 军议读取召开时合法知识快照（只读阵营知识投影）
- [ ] 知识/资源变化后建议标记过时，不静默更新
- [ ] 军师输出：观察 / 假设 / 所需条件 / 主要风险 / 缺失情报 / 置信（条件化）
- [ ] 不输出综合成功率、不排优劣唯一推荐、不自动提交命令（负向断言）
- [ ] 不暴露隐藏真值（只基于阵营知识投影）

---

## Implementation Notes

*Derived from ADR-0002 + systems-index 军师边界:*
- 军师读 epic-005 Story 001 投影 + Story 002 报告；纯函数生成候选路线。
- 候选不带 score 排序字段；提供「缺失情报」而非补全。
- slice 的 Advisor（观察+候选+缺失+置信+免责，不排优劣）可作参考（重写）。

---

## Out of Scope

- 玩家据建议构建计划（epic-006）
- 建议 UI 渲染（已在 hud.md 定义）

---

## QA Test Cases

- **AC-1**: 无最优解/无成功率
  - Given: 任意局势
  - When: 请求军师建议
  - Then: 输出含条件/风险/缺口，无综合成功率字段、无唯一「最优」标记
  - Edge cases: 单一候选时仍不标「最优」
- **AC-2**: 过时标记
  - Given: 召开后知识变化
  - When: 查看建议
  - Then: 标记过时，不静默刷新
  - Edge cases: 资源变化、新情报抵达

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/council/advisor_conditional_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001, 002
- Unlocks: epic-006（玩家据建议构建计划）

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 5/5 passing（无 deferred）
**Deviations**: ADVISORY — 测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/Council/`；无最优解/成功率以结构性+反射断言守护
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/Council/AdvisorConditionalTests.cs`（10 测全绿，250/250 总）
**Code Review**: Complete（APPROVED）
