# Story 004: 两支 E2E 确定性——同种子同 hash + 两结果不同 hash

> **Epic**: Opening Governor Loop（太守开局循环 / M02）
> **Status**: Ready
> **Layer**: Assembly（Integration 装配层）
> **Type**: Integration
> **Estimate**: S（2–3 h，纯测试，无新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: —

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirements**: `TR-session-005`、`TR-session-004`、`TR-career-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0004: 确定性战斗模拟（primary）；ADR-0005: 存档版本与迁移（secondary）
**ADR Decision Summary**:
- ADR-0004：同种子 + 同配置 + 同命令流 → 同状态哈希；`StateHash` 覆盖 career + world + control 全部权威状态。
- ADR-0005：存档不中断确定性链——读档后续推进哈希等于不读档直推哈希。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# NUnit 测试，不调用 UnityEngine。

**Control Manifest Rules (Assembly 层)**:
- Required: 战斗结果可确定性复现（control-manifest §战斗结果可确定性复现）
- Forbidden: 测试不使用随机种子、不依赖时间（`DateTime.Now` 等）
- Guardrail: 每个测试独立建立 session，互不共享状态

---

## Acceptance Criteria

*来自 GDD `gdd-014-campaign-and-career.md`，作用域限本 story：*

- [ ] 相同 `Config()` + `ResolveSiege(Defended)` 执行两次，两个独立 session 产生相同哈希（胜支确定性）
- [ ] 相同 `Config()` + `ResolveSiege(Fallen)` 执行两次，两个独立 session 产生相同哈希（败支确定性）
- [ ] 胜支与败支从相同 `Config()` 开局，产生**不同**哈希（不同决策路径 → 不同状态，哈希有区分力）
- [ ] 败支 → `Advance(1)` → `CaptureSnapshot` → `Restore` → `Advance(1)`，最终哈希等于不存读档直推 `Advance(2)` 的哈希（存档不中断确定性链，TR-session-005 核心）

---

## Implementation Notes

*来自 ADR-0004 / ADR-0005 实现指引：*

- `session.ComputeHash()` 已实装，覆盖 `Career`、`World`、`Control` 全部权威状态。
- 确定性前提：`CampaignStartConfig` 完全相同 → `StartCampaign` 产生相同初态；`ResolveSiege` 无内部随机（GovernorOutcomeService 为纯函数）。
- **本 story 零新生产代码**：只新增测试文件 `CampaignOpeningDeterminismTests.cs`。
- AC-4 是 TR-session-005 在开局两支的 Session 层验证——比现有 `CampaignSessionSaveTests.test_advance_after_restore_matches_direct_advance` 更完整（后者不含 ResolveSiege）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：Advance 可用性基线
- Story 002：胜支存读档内容验证
- Story 003：败支存读档内容验证
- 多步 Advance 后多次存读档链（M03+）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 胜支重放产生相同哈希
  - Given: `s1 = NewSession()`；`s2 = NewSession()`；两个独立 session，相同 `Config()`
  - When: 两者均执行 `Service.ResolveSiege(s, SiegeOutcome.Defended, Win(), Fall())`
  - Then: `s1.ComputeHash() == s2.ComputeHash()`
  - Edge cases: Config 完全相同是前提——任何差异都应产生不同 hash（副验证：不同 merit config → 不同 hash）

- **AC-2**: 败支重放产生相同哈希
  - Given: `s1 = NewSession()`；`s2 = NewSession()`；相同 `Config()`
  - When: 两者均执行 `Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall())`
  - Then: `s1.ComputeHash() == s2.ComputeHash()`
  - Edge cases: N/A

- **AC-3**: 胜支与败支从相同开局产生不同哈希（哈希区分力验证）
  - Given: `s_win = NewSession()`；`s_lose = NewSession()`；相同 `Config()`
  - When: `s_win` 执行 `ResolveSiege(Defended)`；`s_lose` 执行 `ResolveSiege(Fallen)`
  - Then: `s_win.ComputeHash() != s_lose.ComputeHash()`（不同决策 → 不同状态）
  - Edge cases: 哈希不同即可，不要求特定数值

- **AC-4**: 存档不中断确定性链（TR-session-005 Session 层核心验证）
  - Given:
    - `s_direct = NewSession()` → `ResolveSiege(Fallen)` → `Advance(2)` → `directHash = s_direct.ComputeHash()`
    - `s_restored = NewSession()` → `ResolveSiege(Fallen)` → `Advance(1)` → `CaptureSnapshot` → `Restore` → `Advance(1)`
  - When: 查询 `s_restored.ComputeHash()`
  - Then: `s_restored.ComputeHash() == directHash`
  - Edge cases: 此测试验证存档切割点不影响后续推进确定性

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningDeterminismTests.cs` — 必须存在且全绿

**Status**: [ ] 尚未创建

---

## Dependencies

- Depends on: Story 002 DONE（胜支基线）；Story 003 DONE（败支基线）
- Unlocks: epic-016（M03 城市治理）可安全开始——M02 确定性已验证
