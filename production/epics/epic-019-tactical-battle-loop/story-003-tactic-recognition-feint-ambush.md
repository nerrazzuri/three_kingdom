# Story 003: 兵法事后识别（FeintAmbush 机动招式 + 涌现无按钮）— CD 硬退出门

> **Epic**: Tactical Battle Loop（兵法沙盒战役循环 / M06）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`
**Requirement**: `TR-battle-002`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0004: 确定性战斗模拟（primary）；ADR-0009: CampaignSession 装配边界（secondary）
**ADR Decision Summary**: 兵法是多条件**全部成立**时的涌现结果，系统只在事件后经 `TacticRecognizer` 打复盘标签，**绝无同名无条件执行按钮**。会话装配层只编排识别，不造兵法公式。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature 层)**:
- Required: 兵法是条件组合不是无条件技能按钮（强制设计锁）；兵法只作事后标签
- Forbidden: 同名无条件兵法执行按钮；条件不全却识别
- Guardrail: 识别确定性（同满足条件集 → 同识别结果）

---

## Acceptance Criteria

*来自 GDD `gdd-010-battle-tactics-sandbox.md` + CD 硬退出门，作用域限本 story：*

- [ ] 战斗后会话调 `TacticRecognizer.Recognize` 对满足条件集识别兵法
- [ ] **FeintAmbush（假退伏击）机动招式**：当受控撤退保形 + 敌追击 + 伏兵突然性**全部**成立时被识别（CD 硬退出门）
- [ ] 条件不全（缺任一）则**不识别**该兵法（涌现，无无条件按钮，TR-battle-002）
- [ ] 识别确定性：同满足条件集 → 同识别结果
- [ ] 多兵法链各自独立识别（一条成立不影响另一条判定）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `TacticRecognizer.Recognize`）：*

- `CampaignSessionService` 新增 `RecognizeTactics(session)` 返回 `IReadOnlyList<RecognizedTactic>`。
- 满足条件集（`RetrospectiveContext`）由会话战斗过程累积：阶段命令 + 事件按确定性规则映射到 `TacticCondition`（如 Retreat 命令→ControlledRetreatKeptFormation；敌 Engage 追击事件→EnemyPursued；伏击触发→AmbushSurprise）。
- 兵法链配置（`TacticChainConfig`）数据驱动，经会话开战配置注入；FeintAmbush 链 Required = {ControlledRetreatKeptFormation, EnemyPursued, AmbushSurprise}。
- 识别复用 `TacticRecognizer`（全条件成立才纳入），装配层不重写判定。
- **CD 硬退出门**：本 story 接入 FeintAmbush 机动招式，证明非薄皮战斗沙盒。

---

## Out of Scope

*由邻近 story 处理 / 后续：本 story 不实现：*

- Story 001/002：战斗态接入 / 阶段解析
- Story 004：战斗态存读档
- 完整兵法谱（火攻/水攻等）→ 渐进；本 story 仅需 ≥1 机动招式（FeintAmbush）满足硬退出门

---

## QA Test Cases

- **AC-1**: FeintAmbush 全条件成立时识别（CD 硬退出门核心）
  - Given: 战斗满足 {ControlledRetreatKeptFormation, EnemyPursued, AmbushSurprise}
  - When: `RecognizeTactics(s)`
  - Then: 识别结果含 `TacticTag.FeintAmbush`；其 MatchedConditions 含三条件
  - Edge cases: N/A

- **AC-2**: 条件不全不识别（无无条件按钮，TR-battle-002）
  - Given: 战斗只满足 {ControlledRetreatKeptFormation, EnemyPursued}（缺 AmbushSurprise）
  - When: `RecognizeTactics(s)`
  - Then: 识别结果**不含** FeintAmbush
  - Edge cases: 零条件满足 → 空识别

- **AC-3**: 识别确定性
  - Given: 两 session 同满足条件集
  - When: 各 `RecognizeTactics`
  - Then: 两者识别结果相同（同 tag 同顺序）
  - Edge cases: N/A

- **AC-4**: 多兵法链独立识别
  - Given: 同时满足 FeintAmbush 全条件 + NightRaid 全条件
  - When: `RecognizeTactics(s)`
  - Then: 两兵法均识别；一条不影响另一条
  - Edge cases: 仅一条全成立 → 只识别该条

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignTacticRecognitionTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignTacticRecognitionTests.cs` — 5/5 通过（723/723 全绿）

---

## Dependencies

- Depends on: Story 002 DONE（阶段事件是兵法条件的来源）
- Unlocks: Story 004（战斗态存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing（+零条件副验证）
**Deviations**: 满足条件经 MarkTacticCondition 累积（确定性集），战斗命令/事件→条件的细化映射留后续；本 story 接 FeintAmbush 机动招式满足 **CD 硬退出门**。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignTacticRecognitionTests.cs` — 5 tests
**新生产代码**: CampaignSessionService.MarkTacticCondition + RecognizeTactics（经 TacticRecognizer，全条件成立才识别，无无条件按钮）
**Code Review**: 内联 — APPROVED（Lean）
**CD 硬退出门**: ✅ 已接 FeintAmbush（假退伏击）机动招式，非薄皮战斗沙盒。
