# ADR-0005：存档版本与迁移

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
| **Domain** | Core / Persistence |
| **Knowledge Risk** | LOW — 存档用显式 DTO + JSON，不依赖 Unity 序列化 Domain 类型，规避 Unity 6.x 序列化行为 |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`、`docs/architecture/architecture-overview.md`、`adr-0002-architecture-layering.md`、`adr-0004-deterministic-battle-simulation.md` |
| **Post-Cutoff APIs Used** | None — 存档 I/O 经 Infrastructure 端口；用 System.Text.Json 或等价，不用 Unity JsonUtility 处理 Domain 权威状态 |
| **Verification Required** | 验证空/完整状态 round-trip 一致；验证旧版存档经迁移链加载后状态哈希与等价新存档一致；验证写入中断不破坏既有存档 |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0001（Unity + C#）、ADR-0002（架构分层，存档在 Infrastructure）、ADR-0004（确定性：复用状态哈希与回放契约） |
| **Enables** | 失败可继续与 round-trip 相关 Epic；回放/复盘工具 |
| **Blocks** | 存档/读档相关 Story（STORY_009_001/002）——契约 Accepted 前不得实现序列化 |
| **Ordering Note** | 须在 ADR-0004 之后，因回放包与 round-trip 校验复用其状态哈希；配置指纹与 ADR-0003 协同 |

## Context

### Problem Statement

GDD_013 要求可靠保存完整权威状态、玩家认知、配置版本和确定性上下文，并在兼容验证与迁移后原子恢复，
使失败可继续、问题可复现。项目是核心承诺"失败生成新局面而非毁档/强迫读档"的离线单机游戏，存档可靠性与
版本演进是基本体验保障。Unity 序列化不适合承载确定性权威状态（ADR-0001/0002 已禁止 Unity 类型入 Domain）。
本 ADR 锁定存档的序列化形态、原子写入、版本迁移、兼容判定与 round-trip 契约。

### Constraints

- 架构约束：存档 I/O 属 Infrastructure 层，实现 Application/Domain 定义的端口（ADR-0002）。
- 确定性约束：必须保存随机流位置与状态哈希，读档不重抽已发生结果（ADR-0004、GDD_013）。
- 兼容约束：Unity 序列化限制要求显式 DTO、校验与版本迁移（ADR-0001）。
- 知识隔离约束：世界真值与各阵营知识必须分别序列化，加载不交叉污染（GDD_007/GDD_013）。

### Requirements

- 版本化 DTO + 原子写入（临时文件 + 校验 + 原子替换，失败保留上一份有效存档）。
- 逆序逐版迁移链，只操作内存副本/新文件，不覆盖原始存档。
- Round-trip 一致性：load(save(s)) ≡ s；后续命令流产生相同事件序列与状态哈希。
- 加载先验证 schema/配置指纹/校验，不兼容不得部分载入当前会话。
- 满足 TR-time-003、TR-intel-003、TR-save-001/002/003。

## Decision

存档系统建立在五项契约上：**显式版本化 DTO（JSON）**、**原子写入**、**逆序逐版迁移链**、**round-trip 不变量**、**确定性上下文持久化**。

### 1. 序列化形态：显式版本化 DTO + JSON

- Domain 权威状态通过显式 DTO（非 Unity 序列化 Domain 对象）序列化为 JSON。
- 存档头 `SaveHeader` 含 schema version、游戏版本、配置指纹、时间戳、场景、校验和、兼容标记。
- 世界真值（`SaveSnapshot`）与各阵营知识（`KnowledgeSnapshot`）**分区序列化**，加载时分别构造，不交叉污染（GDD_007）。

### 2. 原子写入

```
save_commit:
  1. 序列化到临时文件 tmp
  2. 校验 checksum(tmp) 通过
  3. 原子替换 target ← tmp（rename）
  任一步失败 → target 不变，保留上一份有效存档
```

- 仅在 Command/战役原子阶段边界落盘；解析中的保存请求排队到安全点（GDD_013）。

### 3. 逆序逐版迁移链

```
migrate(save, target_v) = M[target_v−1 → target_v] ∘ … ∘ M[from_v → from_v+1] (save_copy)
```

- 每个版本只维护一个相邻迁移器 `M[i → i+1]`，可独立单元测试。
- 只操作内存副本或新文件，不覆盖原始存档；任一跳转失败 → 整体失败，原文件不变。
- 存档引用已删除配置 ID 时，迁移器须显式替代或判为不兼容（GDD_013 §Edge Cases）。

### 4. 加载流程（兼容判定 + 不变量校验）

```
loadable(save) = (schema_v ∈ supported_versions)
              ∧ (checksum(body) = header.checksum)
              ∧ cfg_compatible(save.cfg_fp, current_cfg_fp)

load: 读最小头部 → 验证兼容/校验 → 迁移副本 → 构造 Domain
    → 运行全部 Domain 不变量与引用校验 → 成功后原子切换当前会话
```

- 不兼容不得部分载入当前会话（全有或全无，保留原会话与原文件，GDD_013 §Failure Cases）。

### 5. 确定性上下文持久化（复用 ADR-0004）

- 保存随机流位置、事件序号、稳定 ID 生成状态、关键状态哈希（`DeterminismState`）。
- 读档后下一次推进产生与原运行相同的事件序列（round-trip + ADR-0004 状态哈希）。
- `ReplayPackage`（初始快照引用 + 有序命令 + checkpoint 哈希）由确定性契约生成，用于测试与复盘，不等同常规存档。

### Key Interfaces

```csharp
// Infrastructure 实现，Application/Domain 定义
interface ISaveRepository {
    Result Commit(SaveSnapshot snapshot);          // 原子写入
    Result<SaveSnapshot> Load(SaveId id);          // 含兼容判定 + 迁移 + 不变量校验
    CompatibilityReport Inspect(SaveId id);        // 只读头部，报告兼容/损坏
}

interface ISaveMigrator {
    int FromVersion { get; }                        // 仅相邻：from → from+1
    SaveDocument Migrate(SaveDocument copy);
}
```

## Alternatives Considered

### Alternative 1：直接 N→最新迁移

- **描述**：每个旧版本写一个直达最新版的迁移器。
- **Pros**：单次加载迁移跳转少。
- **Cons**：迁移器数量随版本数平方增长；每加一个新版须重写所有旧版迁移器，维护成本高且易漏测。
- **Rejection Reason**：维护负担与测试覆盖随版本爆炸；逆序逐版链每版只加一个可独立测试的相邻迁移器。

### Alternative 2：硬版本锁定（不迁移）

- **描述**：schema 不匹配直接拒绝旧存档。
- **Pros**：实现最简单，无迁移代码。
- **Cons**：任何 schema 变更使玩家既有存档失效；违背"失败可继续、不毁档"核心承诺。
- **Rejection Reason**：离线单机长周期游玩，存档失效是严重体验损害；GDD_013 明确要求迁移框架。

## Consequences

### Positive

- 存档可靠（原子写入失败保留旧档）且可跨版本演进（逐版迁移）。
- Round-trip + 确定性上下文使失败可继续、问题可复现。
- 知识分区序列化保证情报隔离不被读档破坏。
- 复用 ADR-0004 状态哈希，回放包成为轻量复现与回归工具。

### Negative

- 显式 DTO 与 Domain 模型双向映射有维护成本（与 ADR-0002 一致的既有成本）。
- 每个 schema 版本须配套迁移器与迁移测试。
- JSON 体积大于二进制（可选压缩，属产品参数，不影响 Domain 结果）。

### Risks

- **风险**：迁移器漏写或链断裂。**缓解**：加载前校验迁移链完整（from_v 到 target_v 每一跳存在）；缺失即判不兼容并明确报错。
- **风险**：DTO 映射遗漏字段导致 round-trip 不一致。**缓解**：round-trip 属性测试（空/完整/战役中状态）+ 状态哈希比对。
- **风险**：写入中断损坏存档。**缓解**：临时文件 + 原子 rename；启动时只恢复校验通过且版本合理的候选（GDD_013 §Edge Cases）。

## GDD Requirements Addressed

| GDD 系统 | 需求（TR） | 本 ADR 如何满足 |
|---|---|---|
| 时间 (GDD_001) | TR-time-003：行动/期限/取消可存档并 round-trip | 显式 DTO 序列化 + round-trip 不变量 + 随机流位置持久化 |
| 情报 (GDD_007) | TR-intel-003：真值与知识分别序列化不交叉污染 | SaveSnapshot 与 KnowledgeSnapshot 分区序列化，分别构造 |
| 存档 (GDD_013) | TR-save-001：版本化 DTO + 原子写入 + 迁移链 | 显式 DTO/JSON + 临时文件原子替换 + 逆序逐版链 |
| 存档 (GDD_013) | TR-save-002：round-trip + 随机流位置 | round-trip 不变量 + DeterminismState 持久化（复用 ADR-0004） |
| 存档 (GDD_013) | TR-save-003：加载兼容判定 + 配置指纹 | loadable 谓词（schema/校验/配置指纹），不兼容不部分载入 |

## Performance Implications

- **CPU**：序列化/反序列化在保存/加载边界，非每帧；JSON 解析一次性成本可接受。
- **Memory**：迁移操作内存副本，峰值约两份存档大小，短生命周期；在 8GB 上限内无忧。
- **Load Time**：迁移链跳转数随版本差线性；MVP 单一起始版本无迁移成本。
- **Network**：不适用（离线单机；云存档列 Future Scope）。

## Migration Plan

当前无源码、单一 schema 起始版本（version 1）。首次实现存档时即建立头部校验、原子写入、
迁移框架（含一个测试迁移）与 round-trip 测试（对接 STORY_009_001/002）。

## Validation Criteria

- [ ] 空状态与完整 slice 状态 round-trip 后状态一致（含战役中、运输中状态）
- [ ] 旧版存档经迁移链加载后状态哈希与等价新版存档一致
- [ ] 写入中断（模拟）后原存档完好，无半写状态
- [ ] 不兼容存档被拒绝且当前会话与原文件不受影响
- [ ] 世界真值与阵营知识加载后无交叉污染（情报隔离测试）
- [ ] ReplayPackage 重放 checkpoint 哈希与原运行一致（复用 ADR-0004）

## Related Decisions

- ADR-0001：选择 Unity + C#
- ADR-0002：架构分层（存档属 Infrastructure，实现端口）
- ADR-0004：确定性战斗模拟（复用状态哈希与回放契约）
- ADR-0003（待撰写）：数据驱动配置——配置指纹是本 ADR 兼容判定的输入
- `docs/architecture/architecture-overview.md`：本 ADR 固化其"存档边界"章节
