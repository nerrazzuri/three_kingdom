# Story 005: 目标循环端到端 + 确定性哈希

> **Epic**: CampaignSession 完整会话装配
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: S / 0.5d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes
**Completed**: 2026-06-28 · 全套 591/591 绿
**Test**: CampaignTargetLoopE2ETests（4 测：目标循环端到端确定性/守城败存档后可继续/CD护栏①CausalTrace≤5/CD护栏⑥代偿与B3同schema）
**Code Review**: inline lean — ADR-0004（确定性）/ADR-0009 R-6/CD 护栏①⑥ COMPLIANT
**实现**: BattleOutcomeSummary（M00 消费的稳定 BattleOutcome 契约，携≤5因素 CausalTrace）+ E2E 串 S1-S4（开局→推进→战果→后果004/015/014→存档round-trip→续推）

## Context

**GDD**: `design/gdd/systems-index.md` + `gdd-014` + `gdd-015`
**Requirement**: `TR-session-004`、`TR-session-005`

**ADR Governing Implementation**: ADR-0004（确定性，primary）· ADR-0009（R-6 验收钩子）
**ADR Decision Summary**: 目标循环确定性——同种子+同配置+同命令流→同状态哈希；守城败→004→015→014→存档 round-trip 全一致；CD 护栏要求 BattleOutcome 携 CausalTrace。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 C# headless E2E；无引擎面。

**Control Manifest Rules (this layer)**:
- Required: 确定性可复现；失败可继续
- Forbidden: 旁路随机；半结算
- Guardrail: 回合/时段制

---

## Acceptance Criteria

- [ ] 端到端目标循环：开局守城 → 注入 BattleOutcome（胜/败）→ 后果写回（经 S3）→ 004 归属 → 015 投影 → 014 生涯 → 存档 round-trip（经 S4）
- [ ] **确定性**：同种子 + 同配置 + 同命令流 → 同状态哈希（逐位）
- [ ] **失败可继续**：守城败后会话进入合法可继续态（在野），可推进下一步
- [ ] **CD 护栏①**：BattleOutcome 携 ≤5 决定性因素 CausalTrace（结算可读，非黑盒）
- [ ] **CD 护栏⑥**：BattleOutcome 契约冻结——代偿路径与未来 B3 路径同 schema（本 E2E 对契约形状断言）

---

## Implementation Notes

*Derived from ADR-0009 §R-6 + CD 护栏：*

- 这是 epic-013 的总验收 E2E，串起 S1-S4。复用 FIX-9 的链做基线，扩成"打→后果→世界/生涯→存档恢复→续推"完整一圈。
- 哈希断言覆盖 CampaignSession 整态（含 ConsequenceTransaction 回滚后哈希一致 R-6）。
- BattleOutcome 用注入（代偿/抽象，§5b.1 裁决），携 CausalTrace 摘要字段。

---

## Out of Scope

- 真实 GDD_010 战役命令层（M06）· 治理/任务/晋升循环（M03）· UI（M15）

---

## QA Test Cases

- **AC-1/2/3**: 端到端 + 确定性 + 可继续
  - Given: 固定种子 + 配置 + 命令流
  - When: 跑两次完整目标循环
  - Then: 两次状态哈希逐位一致；守城败后会话可继续推进；存档 round-trip 一致
- **AC-4/5**: CD 护栏
  - Then: BattleOutcome 含 ≤5 因素 CausalTrace；代偿与（mock）B3 路径 BattleOutcome schema 一致

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignTargetLoopE2ETests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 002、003、004（DONE）
- Unlocks: None（epic-013 MVP 装配验收）
