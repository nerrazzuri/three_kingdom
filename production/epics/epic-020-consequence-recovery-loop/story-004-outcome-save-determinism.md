# Story 004: 后果续局态存读档 + 确定性

> **Epic**: Consequence / Recovery Loop（后果与恢复循环 / M07）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`（§后果）
**Requirement**: `TR-outcome-002`、`TR-outcome-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 后果续局态（最近续局分支 + 选项）随会话存档 round-trip 一致；写回后的会话态（城市等）已由既有段持久化；同输入同哈希。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version；后果续局态 round-trip 一致
- Forbidden: 用 Unity 序列化处理 Domain 权威态（显式 DTO + 文本，ADR-0005）
- Guardrail: 同种子/输入 → 同哈希（含后果续局态）

---

## Acceptance Criteria

*来自 GDD `gdd-010` §后果，作用域限本 story：*

- [ ] 后果写回后的会话态（城市态等）`CaptureSnapshot → Restore` round-trip 一致（写回后损耗态持久化）
- [ ] 最近续局态（分支 + 续局选项）round-trip 一致（读档后续局选项仍可读）
- [ ] round-trip 哈希一致
- [ ] 存档不中断确定性链：开战 → 后果写回 → 存读档后会话态等于直推
- [ ] 含后果续局态存档未提供必要配置 → 整体拒绝（无部分载入）

---

## Implementation Notes

*来自 ADR-0005 实现指引（参考 M03~M06 段存档模式）：*

- 后果写回后的城市态已由 M03 城市段持久化（story-001 写回 `session.CityEconomy`）；本 story 验证写回态 round-trip。
- 最近续局态（`OutcomeBranch` + `ContinuationOption` 列表）若需 round-trip，扩展存档加后果段（outcome 行 + option 行）；配置数据驱动由载入方提供。
- 续局选项是确定性派生（同分支同上下文 → 同选项），可选择**不持久化**（读档后重算）或**持久化**（存档段）。本 story 采用持久化以保证 round-trip 可读。
- 含后果段存档未提供配置 → `SaveFormatException` 整体拒绝。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002/003：后果写回 / 续局选项 / 原子回滚
- 跨版本存档迁移（无新版本不触发）
- 会话级 reputation/relationship 持久化（裁断留后续）

---

## QA Test Cases

- **AC-1**: 后果写回态 round-trip 一致
  - Given: 后果写回（Defeat）后城市态受损的 session
  - When: `Restore(CaptureSnapshot(s), ...)`
  - Then: 写回后城市态（民心/治安/工事等）逐字段一致
  - Edge cases: N/A

- **AC-2**: 最近续局态 round-trip
  - Given: 后果写回后会话持最近续局（分支 + 选项）
  - When: round-trip
  - Then: 读档后续局分支 + 选项 Kind 集一致
  - Edge cases: 无续局态（未写回）→ round-trip 仍正常

- **AC-3**: round-trip 哈希一致
  - Given: 后果写回后 session，`before = ComputeHash()`
  - When: `loaded = Restore(...)`
  - Then: `loaded.ComputeHash() == before`
  - Edge cases: N/A

- **AC-4**: 存档不中断确定性链
  - Given:
    - 直推：开战 → 后果写回 → `directHash`
    - 切割：开战 → 存读档 → 后果写回
  - When: 比较
  - Then: 哈希相等（存档切割点不影响后果写回确定性）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOutcomeSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignOutcomeSaveTests.cs` — 5/5 通过（743/743 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（后果写回态）；Story 002 DONE（续局态）
- Unlocks: epic-020 完成 → M08（敌方 AI，epic-021）或后续装配

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing（+无后果态向后兼容副验证）
**Deviations**: 续局态（分支+选项）持久化为后果段（无独立配置，纯状态恢复）；写回后城市态由 M03 城市段持久化。
**Test Evidence**: `tests/unit/.../Session/CampaignOutcomeSaveTests.cs` — 5 tests
**新生产代码**: CaptureSnapshot/Restore 加后果段（outcome/outcomeopt）+ 构造 lastOutcomeBranch/lastOptions 参数
**Code Review**: 内联 — APPROVED（Lean）
