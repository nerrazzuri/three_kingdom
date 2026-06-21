# ADR-0003：数据驱动配置

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
| **Domain** | Core / Configuration |
| **Knowledge Risk** | LOW — ScriptableObject 仅用于编辑期，构建时转换；Domain 配置为引擎无关不可变对象，不依赖 Unity 6.x 运行时序列化 |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`、`docs/architecture/architecture-overview.md`、`adr-0001-engine-choice.md`、`adr-0002-architecture-layering.md`、`adr-0005-save-versioning-migration.md` |
| **Post-Cutoff APIs Used** | None — ScriptableObject 是稳定 API；构建期转换不依赖 Unity 6.x 新特性 |
| **Verification Required** | 验证非法范围/缺失引用配置在进入 Domain 前被拒绝；验证运行时无法修改已加载配置（不可变） |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0001（Unity + C#）、ADR-0002（架构分层，配置加载在 Infrastructure） |
| **Enables** | 全部含平衡数值的系统 Story；ADR-0005 的配置指纹兼容判定依赖本 ADR 的指纹定义 |
| **Blocks** | 任何读取平衡值的 gameplay Story——配置管线与校验契约 Accepted 前不得硬编码或临时读配置 |
| **Ordering Note** | 与 ADR-0004 平行；配置指纹是 ADR-0005 存档兼容判定的输入，须在存档实现前定义 |

## Context

### Problem Statement

`coding-standards.md` 与全部 13 份 GDD 要求"平衡值数据驱动、不硬编码"，GDD 的 §Balancing Parameters
列出大量配置参数（耗时、权重、阈值、修正、曲线）。architecture-overview 已起草配置管线方向但未锁定。
若不正式确立配置的编辑形态、校验时机、不可变契约与指纹机制，各系统会各自以不同方式读配置，
导致：运行时配置被意外修改破坏确定性、非法配置进入 Domain 引发难复现错误、存档无法用配置指纹判定兼容（ADR-0005）。
本 ADR 锁定数据驱动配置的完整契约。

### Constraints

- 架构约束：配置加载与校验属 Infrastructure 层；Domain 只接收已校验的不可变配置（ADR-0002）。
- 注册表约束：`scriptableobject_as_runtime_authority` 已被禁止——SO 不得作运行时权威状态。
- 确定性约束：配置进入 Domain 后不可变；配置指纹纳入状态哈希与存档兼容判定（ADR-0004/0005）。
- 编辑约束：需编辑器友好的内容生产体验（设计师调参），同时保留外部数据/Mod 边界。

### Requirements

- 配置含稳定 ID、schema version、单位、合法范围、交叉引用。
- 进入 Domain 前完成解析、默认值展开和完整校验；非法范围与缺失引用被明确拒绝。
- 运行时配置不可变。
- 提供可纳入存档与状态哈希的配置指纹。
- 满足全部 GDD §Balancing Parameters 的数据驱动要求与 coding-standards 的"不硬编码"。

## Decision

数据驱动配置建立在四项契约上：**SO 编辑期 + 构建时转换**、**进入 Domain 前完整校验**、**不可变运行时配置**、**配置指纹**。

### 1. 编辑与运行形态分离

```
[编辑期]                    [构建/加载期]                [运行期]
ScriptableObject  ──转换──►  解析 + 校验 + 默认值展开  ──►  不可变 Domain 配置
（设计师友好）              （Infrastructure 层）         （Domain 只读消费）
JSON（外部数据/Mod）──────────────┘
```

- ScriptableObject **仅**用于编辑期内容生产；构建时转换为不可变 Domain 配置对象。
- JSON 用于外部数据、存档和未来 Mod 边界（与 ADR-0005 一致）。
- **SO 永不作为运行时权威**（注册表 `scriptableobject_as_runtime_authority` 禁止项）。

### 2. 配置 Schema 与校验

每个配置含：

```
ConfigSchema:
  - stable_id: 稳定 ID（跨版本不变，交叉引用用）
  - schema_version: 配置 schema 版本
  - fields: 含 unit（单位）、valid_range（合法范围）、default（默认值）
  - cross_refs: 对其他配置 stable_id 的引用
```

校验在进入 Domain 前一次性完成（Infrastructure 层）：

```
validate(config):
  - 每字段 ∈ valid_range，否则拒绝并报稳定错误码
  - 每 cross_ref 指向存在的 stable_id，否则拒绝
  - 必填字段非空；缺失默认值展开
  失败 → 阻止场景/Domain 加载，明确错误（不静默用默认值掩盖非法输入）
```

### 3. 不可变运行时配置

- 校验后的配置转为不可变对象（C# `readonly`/`init`-only / 不可变集合）。
- Domain 只读消费，运行期无任何路径可修改已加载配置。
- 平衡值**绝不**写入方法体（coding-standards + 注册表）。

### 4. 配置指纹

```
config_fingerprint = H( 所有已加载配置的 stable_id + schema_version + 规范化值 )
```

- 指纹纳入战役状态哈希输入（ADR-0004）与存档头（ADR-0005 兼容判定）。
- 配置仅数值变化（schema 兼容）时，存档兼容策略据指纹决定警告/迁移/拒绝（GDD_013 §Open Questions）。

### Key Interfaces

```csharp
// Infrastructure 实现，Domain/Application 定义端口（ADR-0002）
interface IConfigLoader {
    Result<ValidatedConfig> Load(ConfigSetId id);   // 解析 + 校验 + 默认值展开
    ConfigFingerprint Fingerprint(ValidatedConfig cfg);
}

// Domain 只接收此类型，无可变接口
sealed class ValidatedConfig {
    T Get<T>(ConfigId id) where T : IImmutableConfigEntry;
    // 无任何 Set / Mutate 方法
}
```

## Alternatives Considered

### Alternative 1：纯 JSON 著录（无 ScriptableObject）

- **描述**：所有配置用 JSON 手写，不使用 ScriptableObject。
- **Pros**：管线简单、与外部/Mod 边界统一、无 Unity 编辑期依赖。
- **Cons**：失去 Unity 编辑器的可视化调参、引用检查与设计师友好体验；手写 JSON 易出格式错误。
- **Rejection Reason**：项目重平衡调参（大量 §Balancing Parameters），编辑器友好的内容生产体验有实际价值；JSON 仍保留用于外部/Mod，二者并存最优。

### Alternative 2：代码常量配置

- **描述**：配置以 C# 常量/静态类定义。
- **Pros**：类型安全、无解析。
- **Cons**：直接违反"平衡值不硬编码"；改值需改代码重编译；无法外部调参或 Mod。
- **Rejection Reason**：违反 coding-standards 核心规则与全部 GDD 的数据驱动要求。

## Consequences

### Positive

- 设计师可在 Unity 编辑器友好调参，同时运行期保持引擎无关不可变配置。
- 非法配置在进入 Domain 前被拒绝，避免难复现的运行时错误。
- 配置指纹使存档兼容判定与确定性状态哈希可靠（ADR-0004/0005）。
- 平衡值与逻辑彻底分离，满足 coding-standards 与全部 GDD。

### Negative

- 需实现 SO→不可变配置的构建期转换层与校验框架。
- 双形态（SO + JSON）需保持 schema 一致。
- 配置 schema 变更需配套 schema_version 与（必要时）配置迁移。

### Risks

- **风险**：SO 被误当运行时状态修改。**缓解**：注册表 `scriptableobject_as_runtime_authority` 禁止 + 转换后 Domain 仅见不可变类型，编译期阻断。
- **风险**：校验遗漏使非法值进入 Domain。**缓解**：校验框架对每字段 range/cross-ref 强制；边界与非法输入测试覆盖。
- **风险**：配置指纹不稳定（值规范化不一致）。**缓解**：指纹前对配置做规范化排序与定点表示，加指纹一致性测试。

## GDD Requirements Addressed

| GDD 系统 | 需求 | 本 ADR 如何满足 |
|---|---|---|
| 全部 GDD | §Balancing Parameters：全部平衡值数据驱动、标注单位与合法范围 | ConfigSchema 含 unit/valid_range；校验强制；不可变 Domain 配置 |
| 战斗 (GDD_010) | TR-battle-001：配置指纹纳入状态哈希 | config_fingerprint 作为 ADR-0004 状态哈希输入 |
| 存档 (GDD_013) | TR-save-003：配置指纹用于加载兼容判定 | Fingerprint 写入存档头，loadable 据此判定（ADR-0005） |
| 时间/天气/城市/士气/后勤等 | §Formulas 中全部 `配置` 来源变量 | 经 IConfigLoader 校验加载为不可变配置，公式只读取不硬编码 |

## Performance Implications

- **CPU**：校验与 SO→Domain 转换在构建/加载期一次性完成；运行期零转换开销（已是不可变对象）。
- **Memory**：不可变配置常驻，体积小（数值参数）；在 8GB 上限内可忽略。
- **Load Time**：加载期一次性解析 + 校验成本；可接受，且换取运行期确定性与安全。
- **Network**：不适用（离线单机）。

## Migration Plan

当前无源码。首次实现时建立 SO→不可变配置转换层、校验框架与指纹（对接 STORY_001_002
"加载并校验版本化配置"）。后续每个含平衡值的系统经此管线读配置。

## Validation Criteria

- [ ] 非法范围值的配置在进入 Domain 前被拒绝并报稳定错误码
- [ ] 缺失交叉引用（指向不存在 stable_id）的配置被拒绝
- [ ] 运行期无任何路径可修改已加载配置（不可变性测试）
- [ ] 同一配置集的指纹稳定可复现（跨运行/跨平台一致）
- [ ] 方法体内无硬编码平衡数值（静态检查 + code-review）

## Related Decisions

- ADR-0001：选择 Unity + C#（ScriptableObject + JSON 配置管线方向源）
- ADR-0002：架构分层（配置加载/校验在 Infrastructure，Domain 只读消费）
- ADR-0004：确定性战斗模拟（配置指纹纳入状态哈希）
- ADR-0005：存档版本与迁移（配置指纹用于存档兼容判定）
- `docs/architecture/architecture-overview.md`：本 ADR 固化其"数据驱动规则"章节
