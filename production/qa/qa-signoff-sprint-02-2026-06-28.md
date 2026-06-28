# QA Sign-Off Report: Sprint 02（epic-011 战役与生涯 + epic-012 条件历史世界模型）

**Date**: 2026-06-28
**Review mode**: lean
**Smoke Check**: PASS — `production/qa/smoke-2026-06-28.md`（556/556，0 MISSING）
**QA Plan**: `production/qa/qa-plan-sprint-02-2026-06-24.md`

---

## Test Coverage Summary

| Story | Type | Auto Test | Manual QA | Result |
|-------|------|-----------|-----------|--------|
| 11-1 CareerState 骨架 | Logic | PASS（14） | — | PASS |
| 11-2 忠臣晋升门槛 | Logic | PASS（12） | — | PASS |
| 11-3 自立三分支 | Logic | PASS（14） | — | PASS |
| 11-4 太守开局守城 | Integration | PASS（7） | — | PASS |
| 11-5 生涯存档 round-trip | Integration | PASS（8） | — | PASS |
| 12-1 WorldState 骨架 | Logic | PASS（12） | — | PASS |
| 12-2 历史事件触发门 | Logic | PASS（10） | — | PASS |
| 12-3 分叉传播 | Logic | PASS（8） | — | PASS |
| 12-4 归属投影 | Integration | PASS（6） | — | PASS |
| 12-5 抽象结算器 | Logic | PASS（6） | — | PASS |
| 12-6 世界存档 round-trip | Integration | PASS（8） | — | PASS |

**自动化总计**：全套 dotnet 556/556 绿，-warnaserror 0 warning。无 Visual/Feel/UI story → 无手动 QA 范围。

---

## 确定性专项回归门（QA plan 要求，逐项核对）

| 回归门 | 覆盖测试 | 结果 |
|---|---|---|
| 状态哈希一致（同前态+同命令流→同哈希） | CareerStateTests / WorldStateTests / PromotionLadderTests（端到端确定性） | PASS |
| 存档 round-trip 矩阵 | CareerSaveRoundtripTests（8）+ WorldSaveRoundtripTests（8）含读档后续推进=直推 | PASS |
| 自立好感快照隔离 | RebellionTests `test_snapshot_isolation_captures_affinity_at_launch` | PASS |
| 历史够不着短路（不评估前置） | HistoricalEventTriggerTests `test_unreachable_fires_normal...` | PASS |
| 无旁路随机（System.Random/UnityEngine.Random/float 权威路径） | 抽象结算仅注入 IDeterministicRandom；全 Domain 定点；DomainBoundaryTests 回归门 | PASS |
| 版本/指纹校验拒不部分载入 | 两 codec 的 newer-version / fingerprint-mismatch / corrupt 测试 | PASS |

---

## Bugs Found

无。（0 S1 / 0 S2 / 0 S3 / 0 S4）

---

## Verdict: APPROVED

- 全部 11 story 自动化测试 PASS；无手动 QA 范围（纯 Domain sprint）。
- 无 S1/S2 缺陷；确定性专项回归门全 PASS。
- 已记 ADVISORY（不阻断）：① 测试统一落 `ThreeKingdom.Domain.Tests/{Career,World}/`（含 Integration），非 story header 原拟路径；② 存档为各段独立 codec（生涯/世界同构），三段物理统一信封 + 原子写属 epic-009 复用待整合；③ CitySeed 为 MVP 配置占位（权威终归世界模型）。均已在各 story Completion Notes 登记。

### Next Step

Build 已就绪。可 `/retrospective` 收尾 Sprint 02，并判 epic-011/012 关闭；如需推进阶段再 `/gate-check`。
