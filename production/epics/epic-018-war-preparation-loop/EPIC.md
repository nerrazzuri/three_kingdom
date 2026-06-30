# Epic: War Preparation / Commitment Loop（战役准备循环 / M05）

> **Layer**: Feature（含 Assembly 装配——战前准备 Domain 接入会话）
> **GDD**: `design/gdd/gdd-009-battle-preparation.md` + `design/gdd/gdd-010-battle-tactics-sandbox.md`
> **Architecture Module**: M05 War Preparation / Commitment Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M05）
> **Governing ADR**: ADR-0009（CampaignSession 装配）· ADR-0004（确定性）· ADR-0005（存档）· ADR-0003（数据驱动配置）
> **Status**: ✅ Complete（4/4 stories，2026-06-30）
> **Stories**: 4（story-001~004 均 ✅ Complete，见下表）

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| [001](story-001-prep-state-into-session.md) | 准备态接入会话 + 计划草稿编辑 | Integration | ✅ Complete | ADR-0009/0004 |
| [002](story-002-commit-plan-atomic.md) | 合法计划原子提交（CommittedPlan + 资源锁定） | Integration | ✅ Complete | ADR-0009/0004 |
| [003](story-003-conflict-dag-rejection.md) | 冲突 DAG 拒绝非法计划（失败无部分写入） | Integration | ✅ Complete | ADR-0009/0004 |
| [004](story-004-prep-save-determinism.md) | 准备态存读档 + 确定性 | Integration | ✅ Complete | ADR-0005/0004 |

## Overview

把人物、资源、情报、时间转成**可追踪的计划承诺**——兵法条件是**提前创造**出来的，不是开战后临时点击。M00~M04 已就绪（脊梁/城市治理/情报军议）；战前准备 Domain 内核（`PlanCommitService`/`PlanValidator`/`PlanDraft`/`CommittedPlan`/`PreparationContext`，epic-006 完成）已实现原子提交与硬冲突校验，但**尚未接入** `CampaignSession`。本 epic 把它接入可玩会话：会话持资源池 + 准备配置 + 可达区域 + 授权命令 + 当前计划草稿，玩家经会话命令路径编辑草稿并提交；提交全部通过验证（占用/资源/可达/权限/循环依赖）才**原子**生成 `CommittedPlan`（全有或全无），任一硬冲突则全单拒绝、资源池不变、无部分写入；准备态存读档一致 + 确定性。这是把前四个循环（城市/情报/军议）的产出**收口为一场可打的战役初始条件**的关键一环。

## Boundary（与 M00~M04 的边界）

- **已交付**：M00 脊梁；M03 城市治理（派生守城强度/补给/民心风险战役条件输入）；M04 情报军议（玩家据知识做计划）；战前准备 Domain 内核（epic-006，已测但仅接旧竖切）。
- **M05（本 epic）新增**（**含新生产代码**，同 M03/M04）：
  1. `CampaignSession` 持有资源池 + 准备配置 + 可达区域 + 授权命令 + 当前 `PlanDraft`。
  2. 计划编辑命令（增/删命令草稿）经会话路径。
  3. 提交命令经 `PlanCommitService.Submit`：合法计划原子生成 `CommittedPlan` + 资源锁定；硬冲突全单拒绝、资源不变。
  4. 冲突 DAG 拒绝非法计划（循环依赖/占用/资源/可达/权限）。
  5. 准备态（草稿/承诺/资源池）存读档一致 + 确定性。
- 复用既有 Domain 规则（epic-006），**不新增准备/校验公式**。

## 关键护栏（风险）

> module-plan §M05 风险：**准备阶段过重，玩家疲劳；需要模板和复盘辅助，但不能自动布阵**。
- **不自动布阵**（强制设计锁）：系统不替玩家选人/兵力/地点/时间组合提交计划；玩家手动构造草稿，系统只校验与原子提交。
- 模板/复盘是**辅助**（候选模板由数据驱动提供），不是自动最优解。
- 准备 MVP 限单场战役的计划承诺，不做多战役流水线。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；提交经会话命令路径复用 PlanCommitService，不在会话重写校验/扣减公式 | LOW |
| ADR-0004: 确定性 | 整数/定点 + 状态哈希；同草稿+同资源+同命令流 → 同提交结果与哈希 | LOW |
| ADR-0005: 存档版本与迁移 | 准备态（草稿/承诺/资源池）显式序列化 round-trip 一致 | LOW |
| ADR-0003: 数据驱动配置 | 准备校验配置（资源键/冲突规则等）来自版本化配置，不硬编码 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-prep-001 | PlanDraft 不修改权威 state；提交全部通过验证后才原子生成 CommittedPlan（全有或全无） | ADR-0009/0004 ✅ |
| TR-prep-002 | 硬冲突（占用/资源/可达/权限/循环依赖）阻止提交；命令依赖图须为 DAG | ADR-0009/0004 ✅ |

> 注：M05 为装配 epic，复用 epic-006 的 TR-prep-*（像 epic-015/017 复用既有 TR），无新 TR。无 untraced requirement。战斗结算执行（GDD_010 兵法沙盒）属 M06；本 epic 只产出"可执行战役初始条件"，不打战斗。

## Scope

### In Scope
- `CampaignSession` 持有资源池 + 准备配置 + 可达区域 + 授权命令 + 当前 `PlanDraft`。
- 计划编辑命令（增/删 PreparedOrder 草稿）经会话路径；草稿不改权威态（TR-prep-001）。
- 提交命令经 `PlanCommitService.Submit`：合法 → 原子生成 `CommittedPlan` + 资源锁定（扣减）；
- 硬冲突（占用/资源/可达/权限/循环依赖 DAG）→ 全单拒绝、资源池不变、无部分写入（TR-prep-002）。
- 准备态（草稿/承诺/资源池）存读档一致 + 同种子同哈希。

### Out of Scope
- 战斗结算执行（GDD_010 兵法沙盒 / M06）；本 epic 只到"可执行战役初始条件"为止。
- 自动布阵 / 系统替玩家组计划（强制设计锁禁止）。
- 多战役流水线 / 复盘 AI 全量（GDD_009 §Future）。
- 新 Unity scene / 新 UI / 新准备或校验公式。

## Definition of Done

This epic is complete when:
- 准备态接入 `CampaignSession`；计划草稿编辑 + 提交经会话命令路径。
- 合法计划原子提交生成 `CommittedPlan` + 资源锁定；冲突 DAG 拒绝非法计划；失败无部分写入（资源池不变，module-plan §M05 测试证据）。
- 准备态存读档一致 + 同种子同哈希。
- 不自动布阵（系统只校验与原子提交，玩家手动构造计划）。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M04 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-018-war-preparation-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
