# ADR-0007：条件历史世界模型架构（事件四元组 + reachability 触发门 + 分叉传播 + 抽象结算）

## Status

Accepted

> Accepted 2026-06-24（设计在 GDD_015 已完整规格化；经 architecture-review-2026-06-24 识别为缺口 1，覆盖 TR-world-002/004）。

## Date

2026-06-24

## Last Verified

2026-06-24

## Engine Compatibility

| 字段 | 值 |
|------|-----|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — 世界模型为纯 Domain C# 状态机，不依赖 Unity 数值/物理/序列化 |
| **References Consulted** | `adr-0002-architecture-layering.md`、`adr-0004-deterministic-battle-simulation.md`、`adr-0003-data-driven-configuration.md`、`adr-0005-save-versioning-migration.md`、`design/gdd/gdd-015-historical-world-model.md`、`gdd-014-campaign-and-career.md`、`gdd-001-game-time.md` |
| **Post-Cutoff APIs Used** | None — Domain 不调用 Unity；时间来自 GDD_001 WorldTime，随机（如抽象结算）经注入 `IDeterministicRandom` |
| **Verification Required** | 同一存档 + 同一玩家行动序列 → 同一历史走向（逐位）；够不着的事件前置恒成立、照常触发；玩家破坏前置 → 走分叉且下游重评估 |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0002（分层：世界模型落 Domain）、ADR-0004（确定性：定点 + 注入随机 + 状态哈希）、ADR-0003（数据驱动：事件定义/阈值/脱稿深度为版本化配置）、ADR-0005（存档：WorldState/diverged 入版本化 DTO） |
| **Enables** | GDD_015 实现；GDD_014 生涯所处态势；GDD_016 战略层势力态势输入 |
| **Blocks** | 历史推进 / 抽象结算 story——本 ADR Accepted 前不得开始 |
| **Ordering Note** | 与 ADR-0008（城池控制权契约）协同：世界模型的城池归属为只读投影，写入唯一经 ADR-0008 定义的 GDD_004 控制权变更事件 |

## Context

### Problem Statement

GDD_015 要求世界"大势严格沿《演义》推进"却又"可被强大玩家改写"，且必须**确定性可复现、省算力**。三处张力需架构裁定：① 历史既是"轨道"又能"岔开"——如何统一；② 玩家够不着的远方历史如何**廉价**推进而不跑完整战役模拟；③ 城池归属既被历史事件改、又被战役/夺城改——谁是权威。ADR-0004 只保证"给定输入→确定性输出"，**不定义历史推进的结构**。若用无条件脚本推进历史，玩家无法改写（退化为过场）；若全图跑真实势力 AI，算力爆炸；若世界模型与战役各自写城池归属，则双写撕裂。

### Constraints

- 技术约束：复用 ADR-0004——世界推进仅定点、随机仅注入流、WorldState 入状态哈希；时间唯一来自 GDD_001。
- 架构约束：世界模型落 Domain（ADR-0002）；城池归属写入不归本模型（见 ADR-0008）。
- 设计约束：分叉成本须随玩家体量上升、推迟到晚期（GDD_015 §玩家触及边界）；远方默认不脱稿。

### Requirements

- 同一存档 + 同一玩家行动序列 → 同一历史走向（确定性，TR-world-001/002）。
- 玩家够不着的事件前置**恒成立**、照常触发；够得着且破坏前置 → 分叉 + 下游重评估（TR-world-002）。
- 玩家不在场势力混战用**抽象结算**，不跑完整战役（TR-world-004）。
- 配置校验拒绝缺前置/缺分叉分支的事件（TR-world-005）。

## Decision

条件历史世界模型建立在四个机制上：**事件四元组**、**reachability 触发门**、**分叉传播**、**抽象结算器**；数值/随机/哈希/存档全部复用 ADR-0002/0003/0004/0005。

### 1. 事件四元组（数据驱动）

每个历史事件 = `{时间窗 + 前置条件谓词 + 正常结局 + 分叉结局}`，为版本化配置（ADR-0003），构建时转不可变 Domain 配置并入配置指纹。

```csharp
sealed record HistoricalEvent(
    EventId Id, TimeWindow Window, IReadOnlyList<Precondition> Preconds,
    Outcome NormalOutcome, Outcome DivergenceOutcome,
    IReadOnlyList<EventId> Downstream);   // 依赖本事件的下游事件
// diverged 标志属 WorldState（可变运行态），不属此不可变定义
```

- **配置校验（Failure Case）**：缺前置或缺分叉分支的事件被 Infrastructure 配置加载拒绝（ADR-0003），不得只有正常结局却允许被玩家破坏。

### 2. reachability 触发门（保持便宜的关键）

到达事件时间窗时按玩家**当前体量/势力圈**判定可达性，决定走哪条结局：

```
on time_window_enter(e):
    if ¬reachable(e, player):        # 够不着 → 前置恒成立（零额外成本，历史在轨）
        fire(e.NormalOutcome)
    else if all(e.Preconds hold):    # 在场但未破坏前置
        fire(e.NormalOutcome)
    else:                            # 破坏了前置
        world.mark_diverged(e)
        fire(e.DivergenceOutcome)
        reevaluate(e.Downstream)     # 下游按稳定 EventId 序重检前置
```

- `reachable(e, player)` 由 `ReachPredicate` 判定玩家势力圈是否触及事件前置主体（已据有/灭掉相关势力或城池）。够不着即短路，**不评估前置、不跑 AI**——这是"早期历史便宜"的来源。
- 分叉成本天然推迟到晚期：要破坏官渡/赤壁前置，玩家须已强到能据有/灭掉相关主体（已走到自立晚期）。

### 3. 分叉传播（确定性、稳定序）

`reevaluate(downstream)` 按 **EventId 稳定序**遍历下游事件、重检其前置，避免顺序不确定（GDD_015 Edge Case）。脱稿范围默认**只玩家势力圈**；远方按历史或抽象推进，深度为可配置策略 `divergence_spread_depth`（默认 0 = 仅玩家圈）。

### 4. 抽象结算器（省算力）

玩家**不在场**的势力混战不跑完整 GDD_010 战役，由 `IAbstractResolver` 按势力体量/态势 + 注入随机产出结局（占据/归属/存续变化）：

```csharp
interface IAbstractResolver {
    AbstractOutcome Resolve(FactionRecord a, FactionRecord b,
                            ContestContext ctx, IDeterministicRandom rng);
}
```

- 确定性（注入随机，入哈希）；精度只需"不出戏"，不需逐单位。玩家**够得着**范围内的敌方势力行动改由 GDD_016 战略层驱动（非抽象）。

### 5. 城池归属为只读投影（边界，详见 ADR-0008）

世界模型**不独立写**城池归属。历史事件结局中的 `owner_change` 不直接写 WorldState，而是**发起 GDD_004 控制权变更事件**，再由本模型订阅同步（ADR-0008）。这统一了"历史改归属""战役改归属""夺城改归属"三条路径到单一权威。

### Key Interfaces

```csharp
interface IHistoryAdvancer { void OnTimeWindowEnter(EventId e, WorldState w, PlayerReach r); }
interface IReachPredicate  { bool Reachable(HistoricalEvent e, PlayerReach r); }
interface IAbstractResolver { AbstractOutcome Resolve(FactionRecord a, FactionRecord b, ContestContext ctx, IDeterministicRandom rng); }
```

## Alternatives Considered

### Alternative 1：无条件历史脚本（固定时间线）

- **描述**：历史事件到点必发，玩家无法改写。
- **Pros**：实现最简、零分叉成本。
- **Cons**：玩家强大也撼不动赤壁，自立线"改写历史"幻想落空（GDD_014/015 核心）。
- **Rejection Reason**：违背"个人靠抉择"与自立架空线。

### Alternative 2：全图真实势力 AI 模拟

- **描述**：所有势力每刻跑真实战役/决策。
- **Pros**：最高保真、处处可改写。
- **Cons**：算力爆炸；远方玩家看不见的混战也全量模拟，浪费且不确定性面扩大。
- **Rejection Reason**：违背"省 scope/便宜骨架"；reachability 门 + 抽象结算以极小成本达到同等可感效果。

### Alternative 3：世界模型自持城池归属权威

- **描述**：WorldState 直接写 `city.owner`，战役结果回灌世界模型。
- **Pros**：世界模型自洽。
- **Cons**：与 GDD_004 控制权 + 战役/夺城三方双写，权威撕裂（跨系统审查 W1）。
- **Rejection Reason**：归属唯一权威须是 GDD_004（ADR-0008），世界模型只读反映。

## Consequences

### Positive

- "大势随历史 + 个人靠抉择"用 reachability 门统一，早期便宜、晚期才付分叉成本。
- 抽象结算使远方历史以极小算力推进，确定性不受损。
- 城池归属单一权威（经 ADR-0008），消除双写。
- 事件四元组数据驱动，新增历史事件以配置加入，不改推进代码。

### Negative

- `ReachPredicate` 判定逻辑须谨慎设计（"触及前置主体"的定义影响分叉面与平衡）。
- 抽象结算精度与"出戏"之间需玩测校准（Open Question）。

### Risks

- **风险**：分叉传播顺序不确定致不可复现。**缓解**：按 EventId 稳定序重评估。
- **风险**：抽象结算用了非注入随机。**缓解**：复用 ADR-0004 注入流 + 三平台哈希测试。
- **风险**：脱稿涟漪失控扩散全图。**缓解**：`divergence_spread_depth` 默认 0（仅玩家圈），可配置上限。

## GDD Requirements Addressed

| GDD 系统 | 需求（TR） | 本 ADR 如何满足 |
|---|---|---|
| 世界模型 (GDD_015) | TR-world-001：WorldState 权威、确定性推进 | Domain 状态机 + 复用 ADR-0004 |
| 世界模型 (GDD_015) | TR-world-002：四元组 + reachability 门 + 分叉下游重评估 | §1/§2/§3 机制 |
| 世界模型 (GDD_015) | TR-world-004：不在场混战抽象结算 | §4 IAbstractResolver |
| 世界模型 (GDD_015) | TR-world-005：配置校验拒缺分叉事件 | §1 + ADR-0003 加载校验 |
| 世界模型 (GDD_015) | TR-world-006：WorldState/diverged 存档 round-trip | 复用 ADR-0005 |
| 生涯 (GDD_014) | TR-career-004：归属只读、经 004 事件 | §5 + ADR-0008 |

## Performance Implications

- **CPU**：reachability 门对够不着的事件短路，绝大多数历史事件零评估成本；抽象结算为少量势力的定点加权，远在预算内。
- **Memory**：WorldState（势力/城池/事件集合）规模小。
- **Load Time**：事件定义经 Infrastructure 一次性解析校验为不可变配置。
- **Network**：不适用（离线单机）。

## Migration Plan

当前无世界模型源码。按 GDD_015 §MVP 先实现**单一历史战役框**（讨董/汜水关一带）：少数势力、若干城池归属、1–2 个带条件历史事件、够不着则照常触发；抽象结算与全时间线为 Future。当前竖切（SliceScenario）不依赖本模型，可平行推进。

## Validation Criteria

- [ ] 同一存档 + 同一玩家行动序列 → 逐位相同历史走向与 WorldState 哈希
- [ ] 够不着的事件：前置恒成立、走正常结局（不评估前置）
- [ ] 玩家破坏前置：置 diverged、走分叉结局、下游按 EventId 序重评估
- [ ] 抽象结算确定性（同种子同结果）且不写城池归属（归属经 ADR-0008）
- [ ] 配置校验拒绝缺前置或缺分叉分支的历史事件

## Related Decisions

- ADR-0002：架构分层（世界模型落 Domain）
- ADR-0004：确定性模拟（复用定点/注入随机/状态哈希）
- ADR-0003：数据驱动配置（事件定义/阈值/脱稿深度）
- ADR-0005：存档版本与迁移（WorldState/diverged 入 DTO）
- ADR-0008：城池控制权跨系统所有权契约（归属唯一权威，本模型只读订阅）
- GDD_015：条件历史世界模型规格；GDD_014：生涯；GDD_016：战略层势力 AI
