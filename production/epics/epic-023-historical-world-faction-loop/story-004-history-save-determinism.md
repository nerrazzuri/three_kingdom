# Story 004: 历史世界态存读档 + 同序列同走向

> **Epic**: Historical World / Faction Loop（历史世界与势力循环 / M10）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，装配）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-006`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0007（条件历史世界模型）+ ADR-0004/0009（确定性/装配）
**ADR Decision Summary**: 历史事件四元组 + reachability 触发门 + 分叉下游传播，经会话命令复用 `HistoryAdvancer`/`DivergencePropagationService`，不重写历史公式。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 历史经命令路径复用 Domain；reachability 门；城权只读经 GDD_004
- Forbidden: 会话内重写历史公式；世界模型独立写归属
- Guardrail: 同行动序列同历史走向（确定性）

---

## Acceptance Criteria

见 QA Test Cases（本 story 验收以测试为准，复用 TR-world-* / ADR-0007）。

---

## QA Test Cases

见 `tests/unit/ThreeKingdom.Domain.Tests/World/CampaignHistorySaveTests.cs`（详见 Completion Notes）。

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/World/CampaignHistorySaveTests.cs` — 必须存在且全绿

**Status**: [x] 全绿（797/797）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-012 历史世界 Domain（已 Complete）
- Unlocks: 链内后续

---

## Completion Notes
**Completed**: 2026-06-30
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/World/CampaignHistorySaveTests.cs` — 4 tests, 全绿（797/797）
**Deviations**: M10 装配——AdvanceHistory 按时间窗稳定序触发；catalog/reach/config 数据驱动开局注入，历史态(triggered/diverged)在 world 段（无新存档代码）；PlayerReach 开局固定（动态更新留后续）。
**Code Review**: 内联 — APPROVED（Lean）
