# ADR-0002：架构分层（Domain / Application / Infrastructure / Presentation）

## Status

Accepted

## Date

2026-06-21

## Last Verified

2026-06-21

## Engine Compatibility

| 字段 | 值 |
|------|-----|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — 本决策为引擎无关的纯 C# 分层架构，不依赖任何 Unity 6.x 特有 API |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`、`docs/architecture/architecture.md` |
| **Post-Cutoff APIs Used** | None — Domain 层禁止依赖 UnityEngine；分层本身不调用引擎 API |
| **Verification Required** | 验证纯 C# Domain 程序集可独立于 UnityEngine 编译与单元测试（无 Unity 引用即可 `dotnet test`） |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0001（Unity + C#，已 Accepted） |
| **Enables** | ADR-0003（数据驱动配置）、ADR-0004（确定性战斗模拟）、ADR-0005（存档版本与迁移） |
| **Blocks** | 全部 gameplay 实现 Epic——分层契约 Accepted 前不得开始 Domain/Application 编码 |
| **Ordering Note** | 必须先于 ADR-0003/0004/0005，因后续决策均在本分层边界内定义其机制 |

## Context

### Problem Statement

项目是离线单机三国沙盒战略 RPG，核心是大量互相影响、需测试与调平、必须确定性可复现的模拟规则。
若不强制分层，Unity 的 MonoBehaviour/ScriptableObject 会侵入 gameplay 规则，导致：状态权威分散、
模拟依赖帧率/Unity 时间而无法确定性复现、规则无法在无引擎环境下单元测试。`architecture.md`
已起草分层方向但未正式锁定为可强制的决策。本 ADR 将其固化，使所有后续 ADR、GDD 实现和 story 都受同一边界约束。

### Constraints

- 技术约束：Domain 必须可在无 UnityEngine 引用下编译与测试（ADR-0001 已确立纯 C# Domain）。
- 兼容约束：Unity 序列化限制要求显式 DTO、校验与版本迁移，不能让 Unity 类型渗入 Domain。
- 确定性约束：模拟不得依赖帧率、Unity 时间或隐式全局随机（见 GDD_010、GDD_013）。
- 资源约束：单人/小团队开发，分层须可由静态约定 + 程序集边界自动强制，不依赖人工审查。

### Requirements

- 必须定义四层及其单向依赖方向，且依赖方向可由程序集引用强制。
- 必须确立"所有玩家操作经 Command / Application Service 进入 Domain"的唯一写路径。
- 必须确立 Domain 为唯一权威状态持有者，Presentation 只读投影。
- 必须支持 30 条已注册 TR 中跨系统状态权威与契约类需求（TR-*-001/002 多项）。

## Decision

采用四层 Clean/Onion 架构，依赖方向严格单向收敛到 Domain：

### 各层职责

- **Domain**：纯 C# 权威状态与规则。不依赖 UnityEngine、MonoBehaviour、Scene、UI、文件系统或系统时间。
  随机性只能通过显式注入的确定性随机源或预生成随机流进入。产生结果与 Domain Events，不直接播放表现或写文件。
- **Application**：接收 Commands，校验调用上下文（身份/时机/前置），编排 Domain 操作、事务边界、存档与投影更新。
  Queries 返回只读 DTO，不泄露可变 Domain 对象。
- **Infrastructure**：配置加载与校验、存档序列化与迁移、日志、平台文件路径、Unity 适配。实现上层定义的接口（端口），不拥有 gameplay 规则。
- **Presentation**：MonoBehaviour、UI、输入、Scene 组织。只能提交 Command、执行 Query、订阅只读投影与 Domain Event 映射，不能直接修改核心状态。

### 状态变更协议（唯一写路径）

```
Presentation 构造玩家意图 Command
  → Application Service 校验身份/时机/前置输入
  → Domain 用当前状态 + 已验证配置 + 显式随机上下文解析命令
  → Domain 返回结构化结果 + 事件（失败返回稳定错误码，无部分写入）
  → Application 原子提交状态、更新投影、触发存档策略
  → Presentation 根据投影与事件展示结果
```

### 架构图

```
   Presentation  ──submit Command──►  Application  ──►  Domain（纯 C# 权威状态/规则）
   (UI/Input/View)                    (Commands/        ▲
        │  ◄──read-only Projection──   Queries/         │ 实现端口
        │       + Domain Event map      UseCases)       │
        └────────────────────────────►  Infrastructure ─┘
                                        (Save/Config/Logging)

   依赖方向：Presentation → Application → Domain ◄ Infrastructure
   （Infrastructure 依赖 Domain 定义的端口接口；Domain 不依赖任何其他层）
```

### Key Interfaces（契约）

```csharp
// 唯一写路径：所有玩家意图经此进入
interface ICommandHandler<TCommand> {
    Result<TResult> Handle(TCommand cmd, IDeterministicContext ctx);
}

// 只读查询：返回 DTO，不泄露可变 Domain 对象
interface IQueryHandler<TQuery, TDto> {
    TDto Query(TQuery q);
}

// Infrastructure 实现的端口（由 Application/Domain 定义）
interface IConfigLoader { ValidatedConfig Load(ConfigId id); }
interface ISaveRepository { void Commit(SaveSnapshot s); SaveSnapshot Load(SaveId id); }

// 确定性随机源：唯一允许的随机入口
interface IDeterministicRandom { double Next(CheckpointId at); }
```

### 程序集边界（强制依赖方向）

- `Domain.dll` — 无 UnityEngine 引用（可 `dotnet test`）
- `Application.dll` — 引用 Domain
- `Infrastructure.dll` — 引用 Domain + Application 端口
- `Presentation`（Unity 程序集）— 引用 Application API + 只读 DTO，**不引用 Domain 内部类型**

## Alternatives Considered

### Alternative 1：经典 Unity MonoBehaviour 中心架构

- **描述**：组件（MonoBehaviour）直接持有状态，ScriptableObject 作运行时状态，逻辑写在组件中。
- **Pros**：Unity 原生、上手快、编辑器可视化直接。
- **Cons**：状态权威分散、逻辑与引擎强耦合无法无引擎单测、模拟依赖帧生命周期难以确定性复现。
- **Rejection Reason**：直接违反 ADR-0001 约束与确定性/可测试要求，无法满足 GDD_010/GDD_013 的可复现需求。

### Alternative 2：两层精简（引擎层 + 逻辑层）

- **描述**：只分逻辑层与 Unity 层，不显式拆分 Application 与 Infrastructure。
- **Pros**：层数少、初期样板代码少。
- **Cons**：命令校验/事务/存档/配置职责无处安放，易回流进 Domain 或 Presentation；端口与适配混杂，存档迁移与配置校验难隔离测试。
- **Rejection Reason**：本项目存档版本迁移（GDD_013）、配置校验（数据驱动）、命令前置校验是核心复杂度，需要 Application/Infrastructure 显式分离才能独立测试与演进。

## Consequences

### Positive

- Domain 可独立于引擎测试和复现，满足确定性与 round-trip 需求。
- 展示层与模拟层明确隔离，Unity 无法砸坏权威状态。
- 端口/适配分离使配置校验与存档迁移可独立契约测试。
- 为 ADR-0003/0004/0005 提供统一边界，避免各自定义不一致的状态访问方式。

### Negative

- 增加样板代码（Command/Query/DTO/端口接口）与跨层映射成本。
- 必须主动防止 MonoBehaviour/ScriptableObject 侵入 Domain（靠程序集边界 + 评审强制）。
- 简单读取也需经 Query/投影，初期开发节奏略慢。

### Risks

- **风险**：开发者图方便在 Presentation 直接改 Domain 状态。**缓解**：程序集边界使 Presentation 不引用 Domain 内部类型，编译期即阻断。
- **风险**：DTO 与 Domain 模型重复维护漂移。**缓解**：投影由 Application 单点生成，加契约测试覆盖映射。
- **风险**：确定性随机源被绕过（直接用 System.Random）。**缓解**：列入 technical-preferences 禁止模式，code-review 与 story 验收检查。

## GDD Requirements Addressed

| GDD 系统 | 需求（TR） | 本 ADR 如何满足 |
|---|---|---|
| 城市 (GDD_004) | TR-city-001/002：库存守恒、日结稳定顺序 | Domain 单点持有库存权威，经 Application Command 原子结算 |
| 人物 (GDD_005) | TR-character-001/002：能力影响过程质量、职责权限 | 规则在 Domain，权限校验在 Application Service 前置 |
| 关系 (GDD_006) | TR-relationship-001/002：方向性多维、授权有效性 | 关系状态 Domain 权威，授权判定经 Application 校验 |
| 军议 (GDD_008) | TR-council-001：知识快照、过时标记 | 快照为 Domain 只读投影，Application 管理生命周期 |
| 战前准备 (GDD_009) | TR-prep-001/002：草稿无副作用、原子提交 | 草稿不入 Domain；提交经 Application 单一事务原子化 |
| 士气疲劳 (GDD_011) | TR-cohesion-001：三维独立状态 | 三维状态 Domain 权威，Presentation 只读等级投影 |
| 天气/地图/情报 (GDD_002/003/007) | TR-weather-002、TR-map-003、TR-intel-001：具名修正契约、真值/知识分离 | 真值在 Domain，阵营知识为独立投影，UI 只读知识投影 |

## Performance Implications

- **CPU**：跨层映射有轻微开销；Domain 为纯 C# 内存计算，回合制/时段制无每帧压力，远在 16.6ms 预算内。
- **Memory**：DTO 投影产生额外短生命周期对象；可通过投影缓存与按需查询控制，在 8GB 上限内无忧。
- **Load Time**：配置经 Infrastructure 一次性解析校验为不可变 Domain 配置，加载期一次性成本。
- **Network**：不适用（离线单机）。

## Migration Plan

当前无源码，无需迁移。首次建立 Unity 工程时即按四程序集边界搭建（对应 STORY_001_001
"建立 Unity 项目与纯 C# Domain 测试边界"）。后续所有 story 必须落在既定层内。

## Validation Criteria

- [ ] `Domain.dll` 无任何 UnityEngine 引用，可独立 `dotnet test`
- [ ] 存在一条 Presentation → Command → Application → Domain → Event → Projection 的完整贯通用例测试
- [ ] 静态检查（或程序集引用约束）证明 Presentation 不引用 Domain 内部类型
- [ ] 任一玩家操作绕过 Command 路径直接改 Domain 状态的尝试在编译期或测试中被捕获

## Related Decisions

- ADR-0001：选择 Unity + C#（本 ADR 的上游基础）
- `docs/architecture/architecture.md`：本 ADR 正式固化其分层与依赖方向章节
- 即将撰写：ADR-0003（数据驱动配置）、ADR-0004（确定性战斗模拟）、ADR-0005（存档版本与迁移）
