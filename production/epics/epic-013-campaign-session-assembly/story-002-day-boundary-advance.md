# Story 002: 日界推进复用全局结算顺序

> **Epic**: CampaignSession 完整会话装配
> **Status**: Ready
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: S / 0.5d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/systems-index.md` §跨系统结算顺序（含 Meta 层）
**Requirement**: `TR-session-001`

**ADR Governing Implementation**: ADR-0009（primary）· ADR-0004（确定性）
**ADR Decision Summary**: CampaignSession 日界推进必须复用 systems-index 全局结算顺序，禁止私改顺序或回读未结算值。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 C#；确定性时段推进。

**Control Manifest Rules (this layer)**:
- Required: 复用全局结算顺序；确定性可复现
- Forbidden: 私改日界顺序；跨系统回读未结算值
- Guardrail: 回合/时段制

---

## Acceptance Criteria

- [ ] `Advance(segments)` 按 `systems-index` 全局序推进：时间→环境(002)→补给(012)→城市/控制权(004)→状态事件(011)→历史世界模型(015)→生涯(014)→敌方AI(016 mock)
- [ ] 015/014/016 只读基础层与彼此的**已结算**值；不回读未结算
- [ ] 同一前态 + 同一推进序列 → 稳定事件序列 + 状态哈希一致（确定性）

---

## Implementation Notes

*Derived from ADR-0009 §Day Boundary Order：*

- 顺序权威在 `systems-index.md`（已含 FIX-2 的 Meta 层序 + 014↔015↔004↔016 破环）。本 story 只编排调用既有各系统结算服务，不实现结算规则。
- 敌方 AI(016) 本 story 用 mock/no-op（真 AI 属 epic-021/M08）。
- Meta 层发起的控制权变更不在本边界即时回读，经 004 单点落地（S3 处理写回）。

---

## Out of Scope

- Story 003：后果写回 · Story 004：存档 · 真实敌方 AI（epic-021）

---

## QA Test Cases

- **AC-1/2/3**: 确定性日界推进
  - Given: 一个初始 CampaignSession
  - When: Advance(N) 两次（同前态同序列）
  - Then: 事件序列逐项一致 + 状态哈希逐位一致；各 Meta 层读值来自已结算基础层
  - Edge cases: 推进序列变化 → 哈希不同（顺序敏感）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignDayBoundaryTests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（CampaignSession 骨架）
- Unlocks: Story 005
