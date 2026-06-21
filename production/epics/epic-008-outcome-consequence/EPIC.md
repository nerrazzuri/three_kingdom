# Epic: 后果与可玩失败

> **Layer**: Core
> **GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md（§后果/复盘）+ systems-index「后果结算」权威契约（跨 004/005/006/012）
> **Architecture Module**: Domain 后果结算（跨系统变更集 → 校验 → 原子写回）
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

将战果写回人物、关系、城市与名声，并保证失败必产生可继续状态。后果结算先生成跨系统变更计划并校验，再原子写回，不产生半结算状态。胜、败、撤退、失城均为分支——战败延续至少提供撤退/失城/问责一条可继续路径（核心设计锁：失败必须可玩）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0004: 确定性战斗模拟 | 变更集生成与原子写回确定性，不留半结算态 | HIGH |
| ADR-0002: 架构分层 | 后果经 Application 协调，各权威系统独占写自身状态 | HIGH |

## GDD Requirements

本 epic 实现 systems-index「后果结算」权威契约（独占写入「跨系统变更集合」，主要消费者为各权威系统）。其确定性与原子性由 TR-battle-001/003 的同源管线保证；写回目标字段的守恒由 TR-city-001/TR-relationship-001 约束。

| 契约项 | 关联 TR | ADR Coverage |
|---|---|---|
| 先生成变更计划并校验，再原子写回，不产生半结算态 | TR-battle-003（原子回滚同源） | ADR-0004 ✅ |
| 结果写回人物/关系/城市/名声（守恒） | TR-city-001、TR-relationship-001、TR-character-001 | ADR-0002/0004 ✅ |
| 失败产生可继续状态（撤退/失城/问责分支） | （gdd-010 §后果验收） | ADR-0002 ✅ |

> 说明：「后果结算」在 tr-registry 中尚无独立 system slug。本 epic 不引入未规约需求——其行为完全由上述既有 TR 的原子性/守恒约束派生。若后续需独立追踪，运行 `/architecture-review` 追加 TR-outcome-NNN。

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-010 §后果验收标准验证
- 原子写回有测试（任一目标校验失败→零部分写入，整批回滚）
- 守恒有测试（写回前后跨系统资源/关系恒等）
- 战败延续有测试（败局后存在至少一条合法可继续命令）
- 全部 Integration story 在 `tests/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 跨系统变更集校验与原子写回 | Integration | Ready | ADR-0004 |
| 002 | 可玩失败延续（撤退/失城/问责分支） | Integration | Ready | ADR-0002 |

## Next Step

`/story-readiness production/epics/epic-008-outcome-consequence/story-001-atomic-writeback.md` → `/dev-story`
