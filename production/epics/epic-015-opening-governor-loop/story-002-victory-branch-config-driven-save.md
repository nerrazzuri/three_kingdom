# Story 002: 胜支后果——配置驱动生涯初值 + 胜支存读档

> **Epic**: Opening Governor Loop（太守开局循环 / M02）
> **Layer**: Assembly（Integration 装配层）
> **Type**: Integration
> **Estimate**: S（2–3 h，纯测试，无新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Status**: Complete
> **Last Updated**: 2026-06-29

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirements**: `TR-session-004`、`TR-career-001`、`TR-career-004`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0008: 城池控制权所有权契约（primary）；ADR-0009: CampaignSession 装配边界（secondary）；ADR-0005: 存档版本与迁移（secondary）
**ADR Decision Summary**:
- ADR-0008：城池归属只读；夺城/保城经 GDD_004 控制权变更事件，生涯/世界层只读归属。
- ADR-0009：装配层编排不拥规则；`GovernorStartConfig` 值由调用方从配置数据取得，不在服务内硬编码。
- ADR-0005：存档经显式 DTO/JSON；round-trip 后 career 状态完整恢复。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# NUnit 测试，不调用 UnityEngine。

**Control Manifest Rules (Assembly 层)**:
- Required: 生涯初值来自外部传入的 `GovernorStartConfig`，不在服务内硬编码（ADR-0003 数据驱动）
- Forbidden: 测试不直接修改 `session.Career` 字段；城池归属不经 session 直写
- Guardrail: 存档 round-trip 哈希一致是强制测试要求（ADR-0005）

---

## Acceptance Criteria

*来自 GDD `gdd-014-campaign-and-career.md`，作用域限本 story：*

- [ ] 传入不同 `GovernorStartConfig(merit, standing)` 值（与默认值不同），`ResolveSiege(Defended)` 后 `career.Merit` 等于传入值（数据驱动，非硬编码）
- [ ] 传入不同 `GovernorStartConfig`，`career.LordStanding` 等于传入的 `standing`（配置驱动初值）
- [ ] 胜支 `CaptureSnapshot → Restore` round-trip 后 `career.IsUnaffiliated == false`（非在野态持久化）
- [ ] 胜支 round-trip 后 `session.World.OwnershipOf(city).Owner == Player`（城池归属持久化，ADR-0008 合规）
- [ ] 胜支 round-trip 后 `loaded.ComputeHash() == beforeHash`（哈希一致）

---

## Implementation Notes

*来自 ADR-0009 / ADR-0005 实现指引：*

- `Service.ResolveSiege(session, SiegeOutcome.Defended, GovernorStartConfig config, SiegeContext ctx)` 已实装。
- `GovernorStartConfig` 由调用方构造并传入——本 story 验证不同 config 值产生对应 career 初值。
- `Service.CaptureSnapshot(session)` / `Service.Restore(text, fingerprint)` 已实装于 M00。
- **本 story 零新生产代码**：只新增测试文件 `CampaignOpeningVictoryBranchTests.cs`。
- 注意：`ConsequenceTransactionTests` 已验证 `merit==30, standing==0.1, city==Player`（默认值路径）；本 story 验证**非默认**值路径 + 存读档。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：胜/败支 Advance 可用性
- Story 003：败支存读档 + 部曲保留
- Story 004：确定性哈希重放

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 非默认 `GovernorStartConfig` 产生对应 merit
  - Given: `GovernorStartConfig(merit: 50, standing: Frac(2, 10))`（与默认 30/0.1 不同）；`NewSession()` 已初始化
  - When: `Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2,10)), Fall())`
  - Then: `s.Career.Career.Merit == 50`
  - Edge cases: `merit=0` 时 `career.Merit==0`（允许零功绩配置）

- **AC-2**: 非默认 `GovernorStartConfig` 产生对应 standing
  - Given: 同 AC-1
  - When: 同 AC-1
  - Then: `s.Career.Career.LordStanding == Frac(2, 10)`
  - Edge cases: 上界 `Frac(1,1)` 时 standing 不超出 `[0,1]`

- **AC-3**: 胜支存读档后 career 保持非在野
  - Given: `NewSession()` → `ResolveSiege(Defended, Win(), Fall())` → `StateHash before = s.ComputeHash()`
  - When: `Service.Restore(Service.CaptureSnapshot(s), Fp)`
  - Then: `loaded.Career.Career.IsUnaffiliated == false`；`loaded.Career.Career.Merit == s.Career.Career.Merit`
  - Edge cases: 恢复后的 merit / standing 完全等于存档前

- **AC-4**: 胜支存读档后城池归属保持 Player
  - Given: 同 AC-3
  - When: `loaded.World.OwnershipOf(Fanshui)`
  - Then: `Owner == Player`（经 GDD_004 控制权路径，ADR-0008 合规）
  - Edge cases: N/A

- **AC-5**: 胜支 round-trip 哈希一致
  - Given: `before = s.ComputeHash()` after ResolveSiege(Defended)
  - When: `loaded = Restore(CaptureSnapshot(s), Fp)`
  - Then: `loaded.ComputeHash() == before`
  - Edge cases: 哈希应包含 career 状态；不同 merit 值下哈希不同（副验证）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningVictoryBranchTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignOpeningVictoryBranchTests.cs` — 7/7 通过（613/613 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（确认 Advance 路径通，本 story 复用同一 test fixture 结构）
- Unlocks: Story 004（两支确定性哈希重放需要本 story 确立的胜支基线）

---

## Completion Notes
**Completed**: 2026-06-29
**Criteria**: 5/5 passing（+1 哈希区分力副验证）
**Deviations**: None
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningVictoryBranchTests.cs` — 7 tests, 7/7 pass（613/613 全绿）
**Code Review**: Complete — APPROVED（内联 review 2026-06-29，Lean 模式）
