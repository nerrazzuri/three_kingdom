# Epic: 战前准备

> **Layer**: Core
> **GDD**: design/gdd/gdd-009-battle-preparation.md
> **Architecture Module**: Domain 计划草稿/承诺模型 + Application 提交校验路径
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

将人物、兵力、资源与时间转化为可追踪承诺。PlanDraft 不修改权威 state；提交全部通过验证后才原子生成 CommittedPlan（全有或全无）。硬冲突（占用/资源/可达/权限/循环依赖）阻止提交；命令依赖图须为 DAG。这是 P4 草稿/承诺双态与 P11 无自动化捷径的落地——系统校验合法性，但不替玩家补全计划。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | 草稿在 Presentation/Application 构建，提交经 Command 原子写 Domain | HIGH |
| ADR-0004: 确定性战斗模拟 | 校验与依赖图解析确定性（平局稳定 ID 序） | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-prep-001 | PlanDraft 不改权威 state；提交全通过验证才原子生成 CommittedPlan（全有或全无） | ADR-0002 ✅ |
| TR-prep-002 | 硬冲突（占用/资源/可达/权限/循环依赖）阻止提交；依赖图须为 DAG | ADR-0004 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-009 验收标准验证
- 草稿态零副作用有测试（草稿操作后权威哈希不变）
- 提交原子性有测试（任一硬冲突→零部分写入，返回稳定错误码）
- 循环依赖被检出并拒绝（DAG 校验）
- 全部 Logic/Integration story 在 `tests/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | PlanDraft 零副作用与原子提交 | Integration | Ready | ADR-0002 |
| 002 | 硬冲突校验与 DAG 依赖图 | Logic | ✅ Complete | ADR-0004 |

## Next Step

`/story-readiness production/epics/epic-006-battle-preparation/story-001-draft-atomic-commit.md` → `/dev-story`
