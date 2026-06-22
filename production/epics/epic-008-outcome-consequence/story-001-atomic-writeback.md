# Story 001: 跨系统变更集校验与原子写回

> **Epic**: 后果与可玩失败
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md（§后果）+ systems-index「后果结算」契约
**Requirement**: 后果结算契约（确定性/原子性同源 TR-battle-003；守恒受 TR-city-001/TR-relationship-001 约束）

**ADR Governing Implementation**: ADR-0004（secondary ADR-0002）
**ADR Decision Summary**: 先生成跨系统变更计划并校验，再原子写回，不产生半结算态。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Core)**:
- Required: 先校验变更计划再原子写回；各权威系统独占写自身状态
- Forbidden: 半结算态；一个系统直接改另一系统权威状态
- Guardrail: 任一目标校验失败 → 整批回滚

---

## Acceptance Criteria

- [x] 战果生成跨系统变更集合（人物/关系/城市/名声）— `ConsequenceSet` + `OutcomeChange.ForCity/ForReputation/ForCharacter/ForRelationship`
- [x] 写回前整体校验；任一目标不合法 → 整批回滚，零部分写入 — `OutcomeWritebackService.Apply` 聚合错误，失败返回原快照
- [x] 各权威系统独占写自身状态（经其 Command/接口，非外部直改）— 按 Kind 路由，城市经 `CityEconomyState.With`、关系经刻度 clamp
- [x] 守恒：写回前后跨系统资源/关系恒等（无凭空增减）— `ConservationKey` 分组净额须为 0
- [x] 确定性：同战果 → 同变更集 → 同最终状态哈希 — `OutcomeWorld.ComputeHash` 规范遍历

---

## Implementation Notes

*Derived from ADR-0004 + systems-index 后果结算:*
- 变更集为意图列表（target, field, delta, reason）；校验阶段聚合检查。
- 原子写回：全通过才提交，复用 epic-006 原子事务模式。
- 写回经各系统接口（城市/关系/人物各自 Command），保持单一权威。

---

## Out of Scope

- Story 002: 可玩失败延续分支
- 后果 UI 因果链（hud.md P5）

---

## QA Test Cases

- **AC-1**: 原子写回
  - Given: 含一处非法目标的变更集
  - When: 写回
  - Then: 整批回滚，零部分写入
  - Edge cases: 多目标部分非法、边界值
- **AC-2**: 守恒
  - Given: 战果含伤亡/缴获/关系变化
  - When: 写回
  - Then: 跨系统总量恒等（无凭空）；同战果→同哈希
  - Edge cases: 同时写城市与后勤同源粮

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Outcome/AtomicWritebackTests.cs` — 8 测全通过（归一到唯一可编译测试工程，ADVISORY 偏差）
**Status**: [x] Passed — 302/302 全绿，`-warnaserror` 0 warning

---

## Dependencies

- Depends on: epic-007 Story 001/002；epic-003 Story 003；epic-004 Story 001
- Unlocks: Story 002
