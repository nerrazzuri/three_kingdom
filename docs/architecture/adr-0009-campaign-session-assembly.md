# ADR-0009：CampaignSession 装配边界（完整会话脊梁）

## Status

Proposed

> 草案 2026-06-28：源自 `production/full-game-loop-module-plan-2026-06-28.md` 的 M00 裁决草案。待用户评审后才可改为 Accepted；在 Accepted 前，不得据此进入 gameplay code 实现。

## Date

2026-06-28

## Last Verified

2026-06-28

## Decision Makers

- 用户：提出“先做完整游戏循环模块化规划，所有事情在 worktree 中进行，不碰 main”。
- Codex：按 `architecture-decision`、`create-epics`、`map-systems`、`scope-check` 规则起草；Codex 环境不能原生调用 Claude Code Task agents，因此本 ADR 为按 agent 职责口径写出的草案，非 formal full gate。

## Summary

本 ADR 定义 CampaignSession 的装配边界：它是 Application 层的完整会话脊梁，负责把时间、城市、人物、生涯、世界、战役、后果、存档和投影串成可运行循环。CampaignSession 不拥有 gameplay 规则、不硬编码平衡值、不绕过 Domain 权威；所有状态变更仍经 Command/Application Service/Domain Rule/Domain Event/后果写回路径完成。

## Engine Compatibility

| 字段 | 值 |
|------|-----|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Core / Scripting / Application Assembly |
| **Knowledge Risk** | LOW — 本决策约束纯 C# Application/Domain 装配边界，不依赖 Unity 6.3 后截断 API |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`、`breaking-changes.md`、`deprecated-apis.md`、`current-best-practices.md`、`docs/architecture/architecture.md`、`docs/architecture/control-manifest.md`、`docs/registry/architecture.yaml` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | 实现前确认 CampaignSession 所在程序集不引用 `UnityEngine`；所有 Session 写操作只经 Application Command；存档 round-trip 与日界推进 E2E 测试在纯 .NET 测试环境可运行 |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0002（分层与唯一写路径）、ADR-0003（数据驱动配置）、ADR-0004（确定性模拟）、ADR-0005（存档版本与迁移）、ADR-0007（条件历史世界模型）、ADR-0008（城池控制权唯一权威） |
| **Enables** | `production/epics/epic-013-campaign-session-assembly/EPIC.md`；后续 CampaignSession stories；完整太守循环 E2E 测试 |
| **Blocks** | 任何“完整 CampaignSession 装配”实现 story 在本 ADR Accepted 前不得进入 Ready；君主/争霸/统一仍需后续 `ADR-0010 Sovereign Information Boundary` |
| **Ordering Note** | 本 ADR 只裁定会话装配边界；不替代后续 method spec，也不授权 UI、Unity scene 或完整 gameplay 扩 scope。 |

## Context

### Problem Statement

截至 2026-06-28，epic-001~012 的 Domain / Application / Presentation 逻辑内核已完成并通过测试，但完整游戏循环仍未装配。`docs/reviews/full-game-review-2026-06-28.md` 指出当前 `src/Application/Session/GameSession.cs` 主要驱动 `SliceScenario`、`WorldClock`、城市日结、情报、军议、外交求粮、袭扰、伏击等竖切流程；Career、WorldState、BattleResolver、PlanCommit、SupplySettlement、Cohesion、OutcomeWriteback、Relationship、Map 等多数已实现内核未进入同一个可运行 session。

如果继续新增系统而不先定义 CampaignSession 边界，城市、人物、生涯、世界、战役、后果和存档会继续成为可测试但未连接的孤岛；反过来，如果把 CampaignSession 做成“什么都管”的上帝类，又会破坏 ADR-0002 的分层和唯一写路径。

### Current State

- `SessionService.NewGame()` 仍创建 `SliceScenario.Default()`。
- `GameSession` 持有 slice 需要的 Domain 状态，并封装为只读投影。
- 存档映射已存在 `SaveCoordinator` / `SaveMapper` 等 slice 边界，但完整 Career/World/Battle 同一会话 snapshot 尚未统一。
- `systems-index.md` 已定义玩家意图路径：“输入 → Command → Application Service → Domain Rule → Domain Event → 后果投影”。
- `systems-index.md` 已定义日界跨系统结算顺序：时间推进 → 环境 → 补给 → 城市/控制权 → 状态事件 → 历史世界模型 → 生涯 → 敌方 AI。

### Constraints

- Domain 层必须保持纯 C#，不依赖 UnityEngine、MonoBehaviour、Scene、UI、文件系统或系统时间。
- Presentation 只能提交 Command、执行 Query、读取只读 Projection，不能直接修改 gameplay state。
- 可平衡数值必须来自版本化配置；CampaignSession 不得内联场景数值或规则公式。
- 跨系统写回必须原子化：先生成变更计划并校验，再提交到各权威系统。
- 城池控制权变更必须经 GDD_004 / ADR-0008 的唯一权威路径。
- 存档必须 versioned，并能 round-trip 后恢复相同随机流位置、时间、知识分区、世界/生涯状态。

### Requirements

- 必须把时间、城市、人物、生涯、世界、战前准备、战役、后果、存档接入一个长期会话边界。
- 必须保持 Domain 规则所有权：CampaignSession 只编排，不计算具体 gameplay 公式。
- 必须提供统一 command 入口和只读 projection 出口。
- 必须复用全局日界结算顺序，禁止跨系统读取未结算值。
- 必须能形成完整 save snapshot，并拒绝部分载入当前会话。
- 必须支持端到端验证：开局 → 日界推进 → 战前/战役/后果 → 控制权/生涯/世界写回 → 存档 round-trip。

## Decision

**CampaignSession 定义为 Application 层装配模块，而不是 Domain 玩法规则模块。**

CampaignSession 负责维护“当前加载会话”的聚合引用、事务边界、命令调度、日界推进、跨系统事件顺序、后果路由、投影刷新和存档 snapshot 组装。它不得拥有各系统内部规则，不得直接实现战斗、城市、外交、生涯或世界模型公式。

### Architecture

```text
Presentation / UI
    │
    │  PlayerIntent
    ▼
Application
    CampaignSessionService
        │
        ├─ validates command timing / authority / session state
        ├─ calls CampaignSession assembly boundary
        │
        ▼
    CampaignSession
        │
        ├─ Time / Weather / Map services
        ├─ City / Supply / Control authority
        ├─ Character / Relationship services
        ├─ Intel / War Council services
        ├─ Battle Preparation / Battle Resolver
        ├─ Outcome Writeback / Failure Continuation
        ├─ Career / World Model / Enemy AI
        └─ Save Snapshot Builder
        │
        ▼
Domain
    Pure C# authoritative state + rules + Domain Events
        │
        ▼
Application Projections + Save DTOs
        │
        ▼
Presentation reads only
```

### Boundary Rules

| 事项 | CampaignSession 可以做 | CampaignSession 不得做 |
|------|------------------------|-------------------------|
| 会话状态 | 持有当前会话的 Domain aggregate 引用和 session metadata | 成为所有 gameplay 状态的唯一大对象 |
| 命令入口 | 校验命令时机、玩家权限、当前 session 是否可处理 | 让 UI 或 MonoBehaviour 直接改 Domain |
| 日界推进 | 按 `systems-index.md` 全局顺序编排系统结算 | 任意改变系统顺序或同边界回读未结算值 |
| 后果写回 | 接收后果包并路由到权威系统 | 在装配层直接写城池归属、生涯或世界规则 |
| 战役 | 启动/恢复/提交 battle snapshot 与 outcome | 实现战斗规则、士气公式或计策按钮效果 |
| 配置 | 接收已校验 Scenario/Campaign 配置 | 内联平衡数值、用 `SliceScenario.Default()` 固定完整游戏开局 |
| 存档 | 组装完整会话 snapshot，调用 save 端口 | 直接使用 Unity 序列化 Domain 权威状态 |
| 投影 | 发布只读 projection 与解释数据 | 泄露敌方真值或隐藏情报 |

### Day Boundary Order

CampaignSession 必须复用 `design/gdd/systems-index.md` 的日界顺序：

```text
时间推进
  → 环境（GDD_002）
  → 补给（GDD_012）
  → 城市/控制权（GDD_004 / ADR-0008）
  → 状态事件（GDD_011 等）
  → 历史世界模型（GDD_015，读已结算 004 归属）
  → 生涯（GDD_014，读已结算 004 + 015）
  → 敌方 AI（GDD_016，读已结算 011 / 012 / 015）
```

任何 Meta 层发起的控制权变更请求不得在同一边界即时回读；必须经 GDD_004 单点落地，并在下一结算点对 014/015/016 可见。

### Key Interfaces

以下接口名为后续 method spec 的契约方向。实现可以在 method spec 中细化类型，但不得违反职责边界和依赖方向。

```csharp
public sealed record CampaignSessionId(string Value);

public sealed record CampaignCommandEnvelope(
    CampaignSessionId SessionId,
    string ActorId,
    string CommandId,
    CampaignCommand Command);

public abstract record CampaignCommand;

public sealed record CampaignAdvanceCommand(int Segments) : CampaignCommand;

public sealed record CampaignSessionSnapshot(
    int SchemaVersion,
    string ScenarioConfigId,
    string ConfigFingerprint,
    IReadOnlyList<string> AggregateSnapshots);

public interface ICampaignSessionService
{
    Result<CampaignSessionProjection> StartCampaign(string scenarioConfigId, string playerStartId);
    Result<CampaignCommandResult> Execute(CampaignCommandEnvelope envelope);
    Result<CampaignAdvanceResult> Advance(CampaignSessionId sessionId, CampaignAdvanceCommand command);
    Result<CampaignSessionSnapshot> CaptureSnapshot(CampaignSessionId sessionId);
    Result<CampaignSessionProjection> Restore(CampaignSessionSnapshot snapshot);
}
```

### Implementation Guidelines

- `CampaignSession` 留在 Application 层；Domain 不依赖 Session。
- `CampaignSessionService` 是 Presentation 可调用入口；Presentation 不持有可变 Domain aggregate。
- `CampaignSessionSnapshot` 只由版本化 DTO 组成；不引用 Unity 对象、MonoBehaviour、Scene 实例或运行时可变配置。
- 对外 projection 必须分离“真值”和“玩家知识”；UI 只能读玩家合法知识。
- 所有 command 失败必须返回稳定错误码，不产生部分写入。
- 所有跨系统后果必须先形成可验证的 change set；验证通过后按权威系统提交。

## Alternatives Considered

### Alternative 1：继续扩展现有 slice `GameSession`

- **Description**：在当前 `GameSession` 里继续加入 Career、WorldState、Battle、Save 等字段和方法。
- **Pros**：短期改动少，能快速接上现有竖切 UI。
- **Cons**：容易把 slice 场景、完整游戏会话、规则公式、投影和存档都塞进一个类；难以建立稳定 method spec；后续会变成上帝类。
- **Estimated Effort**：短期低，长期高。
- **Rejection Reason**：违背 M00 “装配而非规则”的边界，且无法解决完整游戏循环的长期扩展问题。

### Alternative 2：把 CampaignSession 做成 Domain aggregate

- **Description**：在 Domain 层创建一个总 aggregate，直接拥有所有子系统状态和规则。
- **Pros**：表面上状态集中，E2E 测试入口单一。
- **Cons**：Domain 会直接耦合所有系统；跨系统规则所有权模糊；容易绕过 GDD_004/ADR-0008 等唯一权威；存档迁移粒度变粗。
- **Estimated Effort**：中。
- **Rejection Reason**：破坏 ADR-0002 分层和各系统权威状态边界。

### Alternative 3：完全事件总线驱动，无显式 CampaignSession

- **Description**：所有系统只通过事件互相订阅，Session 只启动事件循环。
- **Pros**：系统表面松耦合。
- **Cons**：调试难、顺序隐式、日界破环难以审计；存档 snapshot 的一致切点不清晰；确定性重放风险高。
- **Estimated Effort**：高。
- **Rejection Reason**：本项目需要可测试、可复现、可解释的固定顺序装配；显式 session boundary 更可控。

## Consequences

### Positive

- 为完整游戏循环提供统一入口，不再继续制造孤岛系统。
- 明确 CampaignSession 不是 gameplay 规则层，降低上帝类风险。
- 为 save/load round-trip、目标循环 E2E、后果写回测试提供稳定边界。
- 后续 epic/story 可以引用 M00 判断是否 ready。

### Negative

- 增加一层 Application 装配文档与 method spec 工作量。
- 短期内需要重构现有 slice `GameSession` 的开局配置与投影边界，不能只靠追加字段快速推进。

### Neutral

- 现有 `GameSession` 可以作为过渡实现参考，但完整 CampaignSession 需要独立 method spec 后再实施。
- `SliceScenario` 可以保留为测试 fixture 或迁移输入，但不得继续作为完整游戏开局的唯一来源。

## Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| CampaignSession 变成上帝类 | Medium | High | epic-013 story readiness 必须检查“只编排、不写规则”；规则公式仍在各 Domain service |
| 日界顺序被实现者私自调整 | Medium | High | ADR 引用 `systems-index.md` 全局顺序；E2E 测试固定状态哈希 |
| Save snapshot 粒度过粗，迁移困难 | Medium | Medium | snapshot 按 aggregate DTO 组合，不序列化运行时对象 |
| 完整装配一次性过大 | High | High | epic-013 只做会话骨架、日界、后果写回、存档、目标循环 E2E；不做君主/争霸 |
| 隐藏情报泄露到 UI | Medium | High | projection 必须分 truth / knowledge；UI 只能读合法知识 |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|----------------|--------|
| CPU（Domain/Application 测试） | slice session 级 | E2E 会话推进增加多系统编排 | 单次日界推进在纯 .NET 测试中保持毫秒级；具体预算在 method spec / benchmark story 中锁定 |
| Memory | slice 状态 | 增加 Career/World/Battle/Save DTO 引用 | 仅持当前会话与必要 checkpoint；不得复制大型只读配置 |
| Load Time | slice load | 完整 snapshot 校验与迁移 | 加载先验证后构造；失败不替换当前会话 |
| Network | None | None | 离线单机，无网络路径 |

## Migration Plan

1. 写入并评审本 ADR；Accepted 前不进入实现。
2. 创建 `epic-013-campaign-session-assembly`，只定义装配 scope，不创建 story 文件。
3. ADR 通过后，为 M00 创建 method spec 或 story-level spec，明确 public methods、错误码、测试。
4. 先建立 `CampaignSession` / `CampaignSessionService` 骨架和配置化开局入口。
5. 按日界顺序接入已完成 Domain services；每接一个系统必须有回归测试。
6. 建立完整目标循环 E2E：守城胜败 → 城市控制权 → 世界归属投影 → 生涯后果 → 存档 round-trip。
7. 当完整 session 可运行后，再决定敌方 AI / Presentation / 后续主循环扩展。

**Rollback plan**：若装配边界验证失败，保留现有 slice `GameSession` 作为可运行竖切，暂停 epic-013 stories，回到 ADR 评审修订；不得把失败的半装配状态合入 main。

## Validation Criteria

- [ ] 所有 CampaignSession 写操作均经 Application Command / Service，Presentation 无直接 Domain 写入。
- [ ] `CampaignSession` / `CampaignSessionService` 不引用 `UnityEngine`。
- [ ] 开局配置不再由 `SliceScenario.Default()` 作为完整游戏唯一入口；配置指纹进入 snapshot。
- [ ] 日界推进按 `systems-index.md` 全局顺序产生稳定事件序列。
- [ ] 战役后果通过 change set 原子写回 City / Career / World，不产生半结算。
- [ ] 城池归属变更只经 GDD_004 / ADR-0008 控制权权威路径。
- [ ] Save/Load round-trip 后，时间、随机流位置、知识分区、CareerState、WorldState 与目标循环状态一致。
- [ ] 至少一个 E2E 测试覆盖“守城胜败 → 后果写回 → 存档恢复”。

## GDD Requirements Addressed

| GDD Document | System | Requirement | How This ADR Satisfies It |
|--------------|--------|-------------|---------------------------|
| `design/gdd/systems-index.md` | Cross-system | 玩家意图必须经过“输入 → Command → Application Service → Domain Rule → Domain Event → 后果投影” | 将 CampaignSession 定义为 Application 装配边界，并禁止 Presentation 直接写状态 |
| `design/gdd/systems-index.md` | Cross-system | 日界按固定全局顺序解析，禁止跨系统回读未结算值 | 固化 CampaignSession 的 Day Boundary Order |
| `design/gdd/gdd-013-save-load.md` | Save / Load | TR-save-001/002/003：版本化 DTO、round-trip、一致校验、不部分载入 | 要求 CampaignSession 组装完整 snapshot 并通过 save/load round-trip |
| `design/gdd/gdd-014-campaign-and-career.md` | Career | TR-career-001/003/004：CareerState 权威、存档一致、归属只读 | CampaignSession 只路由生涯后果，不直接写归属；Career 进入同一 snapshot |
| `design/gdd/gdd-015-historical-world-model.md` | World | TR-world-001/003/006：WorldState 权威、归属只读投影、存档 round-trip | CampaignSession 在日界顺序中接入 WorldState，并遵守 ADR-0008 |
| `design/gdd/gdd-010-battle-tactics-sandbox.md` | Battle | TR-battle-001/003：确定性战斗、阶段原子回滚 | CampaignSession 只启动/提交战役，不拥有战斗规则；战果以原子后果包写回 |
| `design/gdd/gdd-004-city-economy.md` | City | TR-city-001/002：城市库存守恒、日界稳定结算 | CampaignSession 复用城市日界结算并以 004 作为控制权权威 |

## Related

- `production/full-game-loop-module-plan-2026-06-28.md`
- `production/epics/epic-013-campaign-session-assembly/EPIC.md`
- `docs/reviews/full-game-review-2026-06-28.md`
- `docs/architecture/adr-0002-architecture-layering.md`
- `docs/architecture/adr-0003-data-driven-configuration.md`
- `docs/architecture/adr-0004-deterministic-battle-simulation.md`
- `docs/architecture/adr-0005-save-versioning-migration.md`
- `docs/architecture/adr-0007-conditional-history-world-model.md`
- `docs/architecture/adr-0008-city-control-ownership-contract.md`
