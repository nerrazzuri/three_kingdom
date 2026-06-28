# Epic: CampaignSession 完整会话装配

> **Layer**: Feature（Assembly 连接层）
> **GDD**: 横切 `gdd-001/004/009/010/011/012/013/014/015` + `systems-index.md`
> **Architecture Module**: Application Session Assembly（CampaignSession / CampaignSessionService）
> **Status**: Draft for Review
> **Stories**: Not yet created — ADR-0009 Accepted 后再运行 `/create-stories epic-013-campaign-session-assembly`

## Overview

本 epic 把已经完成但分散的 Domain / Application 内核接入一个长期 CampaignSession：开局配置、时间推进、日界结算、城市/控制权、生涯、世界模型、战前/战役、后果写回、存档和只读投影进入同一个可运行会话。它的目标不是新增玩法规则，而是建立完整游戏循环的装配脊梁，验证“太守开局 → 准备 → 战役 → 后果 → 世界/生涯变化 → 存档恢复”可以端到端成立。

## Scope

### In Scope

- CampaignSession / CampaignSessionService 的 Application 层会话边界。
- 数据驱动 Scenario / Campaign 配置入口，替代完整游戏对 `SliceScenario.Default()` 的依赖。
- 复用 `systems-index.md` 的日界跨系统顺序。
- 将 CareerState、WorldState、CityControl、OutcomeWriteback、Save Snapshot 纳入同一会话。
- 建立最小目标循环 E2E 测试：守城胜败 → 后果写回 → 城市控制权/生涯/世界投影 → 存档 round-trip。
- 只读 Projection 输出，不泄露隐藏真值。

### Out of Scope

- 不实现君主/争霸/统一玩法。
- 不实现完整敌方战略 AI；敌方 AI 是否进入后续 MVP 装配需另行裁决。
- 不创建 Unity scene。
- 不实现新 UI。
- 不新增战斗公式、士气公式、城市公式或外交公式。
- 不把兵法做成技能按钮。
- 不让军师自动替玩家排兵布阵。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0009: CampaignSession 装配边界 | CampaignSession 是 Application 装配层，只编排不拥有 gameplay 规则 | LOW |
| ADR-0002: 架构分层 | Domain 纯 C#；Presentation 只能经 Command/Query 接入 | LOW |
| ADR-0003: 数据驱动配置 | 场景/平衡值必须来自校验配置，不能硬编码在方法体 | LOW |
| ADR-0004: 确定性模拟 | 同一初始状态、配置、种子和命令流必须复现 | LOW |
| ADR-0005: 存档版本与迁移 | 完整会话 snapshot 必须 versioned、可迁移、可 round-trip | LOW |
| ADR-0007: 条件历史世界模型 | WorldState、历史事件、分叉与抽象结算进入会话但保持自身权威 | LOW |
| ADR-0008: 城池控制权契约 | 控制权变更只经 GDD_004，World/Career 只读归属投影 | LOW |

> 注：ADR-0009 目前为 Proposed。除非用户评审通过并改为 Accepted，本 epic 不得进入 story implementation。

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-time-001 | WorldDay+DaySegment 权威时间；同一时间点事件稳定全序 | ADR-0002/0004/0009 ✅ |
| TR-time-003 | 行动耗时、期限、取消可存档并 round-trip 后同序列 | ADR-0005/0009 ✅ |
| TR-city-002 | 日界按稳定顺序结算，资源不低于合法下限 | ADR-0008/0009 ✅ |
| TR-prep-001 | PlanDraft 不修改权威 state；提交原子生成 CommittedPlan | ADR-0002/0009 ✅ |
| TR-battle-001 | 战斗确定性：相同快照+配置+种子+命令流产生同状态哈希 | ADR-0004/0009 ✅ |
| TR-battle-003 | 阶段稳定管线；解析异常回滚原子阶段 | ADR-0004/0009 ✅ |
| TR-save-001 | 版本化 DTO + 原子写入 + 显式迁移链 | ADR-0005/0009 ✅ |
| TR-save-002 | Save/Load round-trip 一致，随机流位置不重抽 | ADR-0005/0009 ✅ |
| TR-save-003 | 加载先验证 schema/配置指纹/校验，不兼容不部分载入 | ADR-0005/0009 ✅ |
| TR-career-001 | CareerState 为权威 Domain 状态，结算确定性 | ADR-0002/0009 ✅ |
| TR-career-003 | 生涯状态存档 round-trip 一致 | ADR-0005/0009 ✅ |
| TR-career-004 | 城池归属只读，经 GDD_004 控制权事件 | ADR-0008/0009 ✅ |
| TR-world-001 | WorldState 为权威，历史推进确定性 | ADR-0007/0009 ✅ |
| TR-world-003 | 城池归属为只读投影，订阅 GDD_004 | ADR-0008/0009 ✅ |
| TR-world-006 | WorldState + HistoricalEvent diverged 标志存档 round-trip | ADR-0005/0009 ✅ |

## Traceability Note

CampaignSession 是横切装配模块，不对应单一 GDD。它的需求来源包括：

- `docs/reviews/full-game-review-2026-06-28.md` 的 BLK-1 / ADV-9。
- `production/full-game-loop-module-plan-2026-06-28.md` 的 M00 裁决。
- `design/gdd/systems-index.md` 的 Command 路径与日界顺序。

如果用户评审要求更严格的 TR 追踪，应在后续新增一个 `TR-session-*` registry 段，或创建专门的 `GDD_017_CAMPAIGN_SESSION_ASSEMBLY.md`。当前 epic 先作为装配治理草案，不擅自修改 TR registry。

## Definition of Done

本 epic 完成当：

- ADR-0009 已由用户评审并改为 Accepted。
- 所有 stories 经 `/create-stories epic-013-campaign-session-assembly` 创建并通过 `/story-readiness`。
- CampaignSession 入口、日界推进、后果写回、存档 snapshot、只读 projection 均有 method spec 或 story-level method contract。
- 至少一个端到端测试证明：新开局 → 推进/准备 → 战役或战役结果注入 → 后果写回 → Career/World/CityControl 更新 → Save/Load round-trip。
- 所有相关测试通过；Domain/Application 层无 UnityEngine 依赖；无硬编码平衡数值。
- Presentation 仍只能读取 Projection 并提交 Command，不能直接修改 Domain 状态。

## Required Story Themes（非 story 文件）

以下只是后续 `/create-stories` 的拆分方向，不在本 epic 中直接创建 story：

| Theme | Purpose | Must Not Include |
|-------|---------|------------------|
| 会话骨架 | 建立 CampaignSessionId、session metadata、配置化开局入口 | 不接 UI、不写完整 gameplay 规则 |
| 日界推进 | 按 `systems-index.md` 顺序推进跨系统结算 | 不私改日界顺序、不回读未结算值 |
| 后果写回 | Battle/Outcome change set 原子写回 City/Career/World | 不直接写城池归属，必须经 ADR-0008 |
| Save round-trip | 捕获完整 CampaignSession snapshot 并恢复 | 不用 Unity 序列化 Domain |
| 目标循环 E2E | 验证守城胜败后仍可玩并进入世界/生涯后果 | 不要求完整君主/争霸 |
| Projection | 给 Presentation 提供只读状态与解释数据 | 不泄露敌方真值 |

## Acceptance Criteria

- `CampaignSession` 明确位于 Application 层，Domain 不依赖它。
- `CampaignSessionService` 是 Presentation 可调用入口；所有写操作以 Command/Use Case 形式进入。
- 开局不再把完整游戏锁死在 `SliceScenario.Default()`。
- 日界推进顺序与 `systems-index.md` 一致，并有回归测试覆盖。
- 控制权变更经 GDD_004 / ADR-0008；World/Career 只读归属投影。
- Save snapshot 包含配置指纹、时间、随机流位置、知识分区、CareerState、WorldState 和必要战役 checkpoint。
- 读档失败不会替换当前会话。
- E2E 测试覆盖至少一个胜利或失败后果链，并证明失败后仍可继续。

## Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Epic 变成“重写全部游戏” | High | 限定为装配 epic；不新增玩法公式、不做君主、不做 UI |
| CampaignSession 变成上帝类 | High | ADR-0009 story gate；规则仍留各 Domain service |
| Save snapshot 一次性过大 | Medium | 先覆盖必要 aggregate；每次新增状态必须有 versioned DTO |
| E2E 测试不稳定 | High | 固定配置指纹、随机种子、命令序列和状态哈希 |
| 与现有 slice UI 接缝断裂 | Medium | Presentation 后置；先建立 Application/Domain 会话投影 |

## Open Questions for Review

1. 是否批准 ADR-0009 从 Proposed 改为 Accepted？
2. 是否需要新增 `TR-session-*` 到 `docs/architecture/tr-registry.yaml`，还是允许 CampaignSession 作为横切装配 epic 只引用现有 TR？
3. epic-013 第一批 stories 是否只覆盖五个主题：会话骨架、日界推进、后果写回、存档 round-trip、目标循环 E2E？
4. 敌方 AI 是否作为 epic-013 的 mock/最小输入，还是完全留到后续独立 epic？
5. 现有 `GameSession` 是迁移为 CampaignSession，还是保留为 slice fixture 并新建完整会话类？

## Next Step

用户评审通过后：

1. 将 ADR-0009 状态从 Proposed 改为 Accepted。
2. 更新 `docs/registry/architecture.yaml` 的 registry candidates（需单独用户批准）。
3. 运行 `/create-stories epic-013-campaign-session-assembly`。
4. 对第一条 story 跑 `/story-readiness`。
5. 通过后再进入 `/dev-story`。
