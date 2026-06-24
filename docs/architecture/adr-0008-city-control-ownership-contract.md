# ADR-0008：城池控制权跨系统所有权契约（GDD_004 唯一权威 + 控制权变更事件）

## Status

Accepted

> Accepted 2026-06-24（裁定源自 gdd-cross-review-2026-06-24 W1，用户拍板"004 唯一权威 / 015 订阅 / 014 只读"；经 architecture-review-2026-06-24 识别为缺口 2，覆盖 TR-career-004/world-003，本 ADR 固化）。

## Date

2026-06-24

## Last Verified

2026-06-24

## Engine Compatibility

| 字段 | 值 |
|------|-----|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — 纯 Domain 状态所有权契约，无引擎 API 面 |
| **References Consulted** | `adr-0002-architecture-layering.md`、`adr-0007-conditional-history-world-model.md`、`design/gdd/gdd-004-city-economy.md`、`gdd-015-historical-world-model.md`、`gdd-014-campaign-and-career.md`、`gdd-010-battle-tactics-sandbox.md`、`design/gdd/gdd-cross-review-2026-06-24.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | 失城/夺城/历史易主三路径下，城池归属只有一个写入点（GDD_004 控制权变更事件）；015/014 无独立写归属路径（编译/测试级） |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0002（分层：Domain 权威状态 + 唯一写路径） |
| **Enables** | GDD_004 控制权变更事件实现；GDD_015 归属只读订阅（ADR-0007）；GDD_014 生涯只读归属 |
| **Blocks** | 任何写城池归属的 story——须经本契约定义的单一事件 |
| **Ordering Note** | 与 ADR-0007 协同：世界模型订阅本事件同步归属投影 |

## Context

### Problem Statement

跨系统审查（W1）发现**三处都在动城池归属**：GDD_004 拥有「控制权」+「控制权变更事件」（失城改控制权）；GDD_015 世界模型写 `city.owner ← outcome.owner_change`；GDD_014 生涯写回「城池控制权」。失城/夺城/历史易主场景下，三方各写各的会产生**双写 bug 或权威撕裂**（场景走查坐实：失城链 010→004→015→014 四系统触归属）。ADR-0002 立了"Domain 是权威状态持有者"的总则，但未指定**城池归属这一具体跨系统状态由哪个系统独占写入**。

### Constraints

- 架构约束：遵循 ADR-0002 唯一写路径（经 Command/Application Service + Domain Event）。
- 设计约束：归属变更来源多元（历史事件、战役战果、生涯夺城），但写入点须唯一。

### Requirements

- 城池归属（owner/garrison）有**唯一权威写入系统**（TR-career-004、TR-world-003）。
- 其余系统对归属**只读**；变更须经统一事件，不并发写。
- 历史事件、战役战果、夺城三条路径统一到该唯一写入点。

## Decision

**GDD_004（城市经济/政治）为城池控制权的唯一权威**，独占发布「控制权变更事件」`CityControlChanged`。其余系统只读、只订阅、只**发起**变更（不直接写）。

### 1. 权威与角色

| 系统 | 对城池归属的角色 |
|---|---|
| **GDD_004 城市** | **唯一权威**：持有 `city.owner` / `city.garrison` 权威态；独占发布 `CityControlChanged(city, newOwner, garrison)` Domain Event |
| **GDD_015 世界模型** | **只读投影**：订阅 `CityControlChanged` 同步战略尺度归属反映；不独立写（ADR-0007 §5） |
| **GDD_014 生涯** | **只读**：读控制权判断生涯态势；夺城**发起** 004 控制权变更，不直接写 |
| **GDD_010 战役** | **发起**：战果（失城/夺城）经后果包请求 004 控制权变更，不直接写归属 |

### 2. 变更流（唯一写入点）

```
[历史事件结局 owner_change | 战役战果失城/夺城 | 生涯夺城]
        │  （三条来源路径，皆为「请求」非「写入」）
        ▼
GDD_004 城市系统：校验 → 写 city.owner/garrison（唯一权威写）→ 发布 CityControlChanged
        │
        ├─► GDD_015 世界模型：订阅，同步战略归属投影（只读）
        ├─► GDD_014 生涯：订阅，更新所处态势（只读）
        └─► 其他消费者（情报/UI 投影）
```

### 3. 并发裁定

历史事件与玩家局部战役同时争夺同一城时，按 **GDD_001 日界全局结算顺序**裁定，不并发写（GDD_015 Edge Case 已定）。控制权变更在该顺序内由 004 单点结算。

### Key Interfaces

```csharp
// GDD_004 独占发布；其余系统订阅或经后果包发起，绝不直接写 city.owner
record CityControlChanged(CityId City, FactionId NewOwner, Garrison Garrison, ChangeCause Cause);
interface ICityControlAuthority {          // 仅 GDD_004 实现
    void RequestControlChange(CityId c, FactionId newOwner, ChangeCause cause);  // 校验后写 + 发事件
}
```

## Alternatives Considered

### Alternative 1：世界模型持归属权威，城市订阅

- **Pros**：战略视角"世界拥有地图"直觉。
- **Cons**：城市经济/守备/失城后果都依赖归属，订阅产生回环；战役/夺城仍要双向同步。
- **Rejection Reason**：归属与城市经济/守备强耦合，权威留在 GDD_004 耦合最低、写路径最短。

### Alternative 2：各系统各写、事件最终一致

- **Pros**：实现自由。
- **Cons**：双写撕裂、非确定性、难复现（正是 W1 暴露的问题）。
- **Rejection Reason**：违背确定性与单一权威；审查直接判为 Warning。

### Alternative 3：并入 ADR-0002，不单立 ADR

- **Pros**：少一份文档。
- **Cons**：ADR-0002 是通用分层总则，把具体跨系统状态归属塞入会稀释其抽象层级；该契约涉及 4 个系统、需可被 story 单独引用。
- **Rejection Reason**：离散的跨系统状态所有权决策值得独立 ADR，便于 story 追溯与后续演进。

## Consequences

### Positive

- 城池归属单一写入点，消除失城/夺城/历史易主三路径的双写撕裂。
- 确定性：归属变更经 004 单点 + 日界全局顺序，可复现。
- 015/014/010 解耦为只读/发起方，依赖方向清晰。

### Negative

- 所有改归属的逻辑须绕经 GDD_004 的变更请求，略增一层间接。

### Risks

- **风险**：开发者图方便在 015/014 直接写 `city.owner`。**缓解**：ADR-0002 程序集边界 + 本契约列入 code-review 检查；归属字段在世界模型侧设为只读投影类型。

## GDD Requirements Addressed

| GDD 系统 | 需求（TR） | 本 ADR 如何满足 |
|---|---|---|
| 生涯 (GDD_014) | TR-career-004：归属只读、经 004 事件 | §1/§2 角色与变更流 |
| 世界模型 (GDD_015) | TR-world-003：归属只读投影、订阅 004 | §1/§2 + ADR-0007 §5 |
| 城市 (GDD_004) | TR-city-001/002（既有）：控制权变更结算 | 固化 004 为唯一权威 + 事件 |

## Performance Implications

- **CPU/Memory/Load/Network**：忽略——单点事件发布 + 订阅同步，无每帧成本，离线单机。

## Migration Plan

当前无源码。GDD_004 控制权变更实现时即按本契约：归属字段权威留 004，发布 `CityControlChanged`；015/014/010 侧实现为订阅/发起。当前竖切（SliceScenario）城池易主已是单点，符合契约方向。

## Validation Criteria

- [ ] 失城/夺城/历史易主三路径下，城池归属只有 GDD_004 一个写入点（其余系统无写归属 API）
- [ ] GDD_015 世界模型归属字段为只读投影，仅由 `CityControlChanged` 更新
- [ ] GDD_014 生涯无直接写归属路径，夺城经 004 变更请求
- [ ] 历史事件与局部战役争同城时按 GDD_001 日界顺序单点裁定，无并发写

## Related Decisions

- ADR-0002：架构分层（Domain 权威 + 唯一写路径，本 ADR 在其下具体化城池归属）
- ADR-0007：条件历史世界模型（世界模型订阅本事件同步归属）
- GDD_004：城市经济/政治（控制权权威）；GDD_015/014/010：归属消费者/发起方
- `design/gdd/gdd-cross-review-2026-06-24.md`：W1 裁定来源
