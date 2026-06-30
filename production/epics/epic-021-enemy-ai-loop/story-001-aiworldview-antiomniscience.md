# Story 001: AiWorldView 反全知锁（结构级拒真值）+ 战术动作候选

> **Epic**: Enemy AI Loop（敌方 AI 循环 / M08）
> **Status**: Complete
> **Layer**: Core（敌方 AI Domain 内核）
> **Type**: Logic
> **Estimate**: M（4–6 h，新 Domain 内核）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-016-enemy-ai.md`
**Requirement**: `TR-ai-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0006: 确定性效用敌方 AI（primary，§2 反全知锁）；ADR-0002: 分层（secondary）
**ADR Decision Summary**: AI 唯一敌情入口是 `AiWorldView`，其构造**只接受阵营知识投影 + 评估 + 己方/环境/目标，绝不接受 MapTruth/WorldTruth 真值类型**——编译期即杜绝偷看。误判由按错误情报行动天然涌现。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Core/Domain 层)**:
- Required: Domain 纯 C# 无 UnityEngine；反全知（AI 不读真值）
- Forbidden: AiWorldView 构造接受真值类型；UI/AI 读 WorldTruth/MapTruth
- Guardrail: 结构级约束（编译期），非运行时约定

---

## Acceptance Criteria

*来自 GDD `gdd-016` §MVP + ADR-0006 §2，作用域限本 story：*

- [ ] `AiWorldView` 构造**只接受**阵营知识投影（`IntelProjection`）+ 敌情评估（`IntelAssessment`）+ 己方态（`OwnForceSnapshot`）+ 环境（`EnvironmentModifierSet`）+ 目标压力（`ObjectivePressure`）
- [ ] `AiWorldView` **无任何** API 暴露或接受 `MapTruth`/`WorldTruth`/`TruthRecord` 真值类型（反全知，结构级）
- [ ] AI 感知的敌情来自 `IntelProjection`（可能与真值不同——误判涌现）
- [ ] `StrategicAction` 候选定义（追击/撤退/坚守/诱敌 ≥3 动作）
- [ ] `AiWorldView` 不可变，字段只读可查（供后续评分）

---

## Implementation Notes

*来自 ADR-0006 §2 实现指引：*

- 新建 `src/Domain/EnemyAI/`：`AiWorldView`（+ `OwnForceSnapshot` 己方兵力/士气、`ObjectivePressure` 目标压力）+ `StrategicAction` enum + `AiReasonCode` enum。
- `AiWorldView` 构造签名严格按 ADR-0006：`(IntelProjection ownIntel, IntelAssessment enemyAssessment, OwnForceSnapshot own, EnvironmentModifierSet env, ObjectivePressure obj)`——无真值参数。
- 复用 `IntelProjection`（epic-005）、`EnvironmentModifierSet`（epic-002）。
- `StrategicAction`：Pursue（追击）/Retreat（撤退）/Hold（坚守）/FeintLure（诱敌）。

---

## Out of Scope

*由邻近 story / 后续处理——本 story 不实现：*

- Story 002：效用评分 + 可行性门
- Story 003：种子 softmax 选择 + DecisionRecord
- Story 004：接入战区命令
- OpponentModel（记忆）/ StrategicPlan（战略）/ LLM（裁断留后续）

---

## QA Test Cases

- **AC-1**: AiWorldView 持阵营知识，不暴露真值
  - Given: 构造 AiWorldView（IntelProjection 敌情 + 己方 + 环境 + 目标）
  - When: 查 AiWorldView 公共 API
  - Then: 可读阵营知识/己方/环境/目标；**无**任何成员返回或接受 MapTruth/WorldTruth/TruthRecord
  - Edge cases: 反全知是结构级——构造无真值参数（编译期保证；测试验证 API 面无真值类型）

- **AC-2**: AI 感知敌情来自情报投影（可能误判）
  - Given: IntelProjection 报告敌军兵力 = 3000（实际真值不同，但 AI 不可知）
  - When: AiWorldView 读敌情
  - Then: AI 感知 = 3000（来自投影，非真值）
  - Edge cases: 未侦察主题 → AI 无该敌情（信息缺失涌现保守/误判）

- **AC-3**: StrategicAction 候选完整
  - Given: StrategicAction 枚举
  - When: 枚举值
  - Then: 含 Pursue/Retreat/Hold/FeintLure（≥3 动作）
  - Edge cases: N/A

- **AC-4**: AiWorldView 不可变
  - Given: 构造后的 AiWorldView
  - When: 多次读字段
  - Then: 字段值稳定（不可变快照）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/EnemyAI/AiWorldViewTests.cs` — 必须存在且全绿

**Status**: [x] `AiWorldViewTests.cs` — 6/6 通过（764/764 全绿）

---

## Dependencies

- Depends on: epic-005 情报 + epic-002 环境 + epic-003 性格 Domain（已 Complete）
- Unlocks: Story 002（评分读 AiWorldView）、Story 003、Story 004

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 反全知锁运行时无法测编译失败，改以反射断言 AiWorldView API 面/构造参数无 Truth 类型 + 感知敌情来自情报投影。
**Test Evidence**: `tests/unit/.../EnemyAI/AiWorldViewTests.cs` — 6 tests
**新生产代码**: src/Domain/EnemyAI/ — AiWorldView（反全知，构造拒真值）+ OwnForceSnapshot + ObjectivePressure + StrategicAction + AiReasonCode
**Code Review**: 内联 — APPROVED（Lean）
