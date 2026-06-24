# GDD_013 — 存档与读档

- 状态：Draft
- **Status**: Draft
- 范围：Vertical Slice

## System Purpose

可靠保存完整权威状态、玩家认知、配置版本和确定性上下文，并在兼容验证与迁移后原子恢复，使失败可继续、问题可复现。

## Player Fantasy

玩家可以放心暂停并继续自己的三国人生；读档不会改变相同命令的结果，也不会因版本更新静默损坏世界。

## Core Loop

玩家或自动策略请求保存 → 等待安全原子边界 → 生成一致快照 → 写临时文件并校验 → 原子替换目标存档 → 选择读档 → 验证头部/兼容/完整性 → 迁移副本 → 构造并验证 Domain → 原子切换会话。

## Main Rules

- 存档包含 schema version、游戏版本、配置指纹、时间、世界状态、各阵营知识、进行中计划/行动、战役状态、随机流、事件序号和校验信息。
- 正式保存只在 Command/战役原子阶段边界执行；解析中请求排队，不序列化半结算状态。
- 写入采用临时文件、完整校验和原子替换；失败时保留上一份有效存档。
- 加载先读取最小头部，再检查版本、配置和校验；不兼容不得部分载入当前会话。
- 迁移只操作内存副本或新文件，不覆盖原始存档；每个版本跃迁有显式迁移链。
- 加载完成后运行所有 Domain 不变量和引用校验，成功后才替换当前会话。
- 存档必须保存随机流位置或等价确定性状态；读档不重新抽取已发生结果。
- 离线单机不限制手动读档。设计通过可玩失败降低强制读档，而不是惩罚玩家。
- 回放包可由初始快照、配置指纹、种子和命令流生成，用于测试与复盘；它不等同于常规存档。

## Formulas

> **本系统无 gameplay 平衡数值公式**（见 §Balancing Parameters）。其"数学"是
> **逻辑契约与不变量**：兼容判定、迁移链、round-trip 一致性、原子写入、确定性回放。
> 这些不变量是强制项,由 §Test Requirements 验证。可配置产品参数（自动存档时机、
> 轮换数、检查点间隔、压缩级别）**不得影响 Domain 结果**。

### 变量定义

| 变量 | 含义 | 范围 / 单位 | 来源 |
|------|------|-----------|------|
| `schema_v` | 存档 schema 版本 | 整数 ≥ 起始版本 | SaveHeader |
| `cfg_fp` | 配置指纹 | 哈希 | SaveHeader |
| `checksum` | 完整性校验值 | 哈希 | SaveHeader |
| `rng_pos` | 随机流位置 | 序号 | DeterminismState |
| `autosave_rotation` | 自动存档轮换数 | 整数 ≥ 1（产品参数） | 配置 |

### 1. 加载兼容判定（不兼容不得部分载入）

```
loadable(save) = (save.schema_v ∈ supported_versions)
              ∧ (checksum(save.body) = save.header.checksum)
              ∧ (cfg_compatible(save.cfg_fp, current_cfg_fp))
```

- 任一条件不成立 → 不载入当前会话(全有或全无,见 §Main Rules)。配置指纹不同但 schema 兼容时,按兼容策略警告/迁移/拒绝,不静默继续(见 §Edge Cases)。

### 2. 迁移链（只操作副本，显式版本跃迁）

```
migrate(save, target_v) = M[target_v−1→target_v] ∘ … ∘ M[v→v+1] (save_copy)
其中每个 M[i→i+1] 必须存在且成功; 任一失败 → 整体失败, 原文件不变
```

- 迁移只操作内存副本或新文件,不覆盖原始存档(见 §Main Rules)。存档引用已删除配置 ID 时须显式替代或判不兼容(见 §Edge Cases)。

### 3. Round-trip 一致性不变量（强制）

```
∀ 权威状态 s: load(save(s)) ≡ s
∀ 后续命令流 C: resolve(load(save(s)), C) 产生与 resolve(s, C) 相同的事件序列与状态哈希
```

- 读档**不重新抽取已发生结果**——保存 `rng_pos` 并从该位置继续(见 §Main Rules)。这是本系统最核心的不变量,满足 §Test round-trip 与回放哈希一致。

### 4. 原子写入（失败保留上一份有效存档）

```
save_commit:
    1. 写临时文件 tmp
    2. 验证 checksum(tmp) 通过
    3. 原子替换 target ← tmp
  任一步失败 → target 不变, 保留上一份有效存档
```

- 只在 Command/战役原子阶段边界执行;解析中请求排队,不序列化半结算状态(见 §Main Rules、§Edge Cases)。

### 5. 确定性回放哈希校验

```
replay_valid(pkg) = ( replay(pkg.起始快照, pkg.命令流).checkpoint_hashes = pkg.checkpoint_hashes )
```

- 回放包由初始快照 + 配置指纹 + 种子 + 命令流生成;检查点哈希逐一比对(对接 GDD_010 §1 状态哈希)。不等同常规存档。

### 6. 自动存档轮换（产品参数，不影响 Domain）

```
保留最近 autosave_rotation 份, 超出按时间删除最旧
```

- 纯产品参数;轮换策略与压缩级别**绝不**改变 Domain 结果或确定性(见顶部说明)。

## Data Model

- `SaveHeader`：schema、游戏版本、配置指纹、创建/更新时间、场景、校验、兼容标记。
- `SaveSnapshot`：所有 Domain aggregate 的版本化 DTO。
- `KnowledgeSnapshot`：各阵营事实/报告/推测投影。
- `DeterminismState`：随机流、事件序号、稳定 ID 生成状态、关键哈希。
- `PendingWorkSnapshot`：计划、进行中行动、运输、调度事件与安全边界。
- `MigrationRecord`：源版本、目标版本、迁移 ID、结果摘要。
- `ReplayPackage`：起始快照引用、有序命令、检查点哈希。

## Player Inputs

创建命名手动存档、覆盖确认、选择存档、删除确认、请求加载、查看兼容/损坏原因。自动存档由安全策略触发；玩家不能绕过兼容校验强制写入当前会话。

## System Outputs

保存成功/排队/失败状态、存档元数据、兼容性结果、迁移报告、加载后的会话、损坏恢复选项、确定性诊断和可选回放包。

## Dependencies

依赖所有权威 GDD 状态、ARCHITECTURE 的 Application/Infrastructure 边界，以及 ADR_0005 将锁定的版本策略。UI 只调用保存/加载用例，不直接序列化 Domain。**权威状态边界含 Meta 层** GDD_014（CareerState/RetinueState/RebellionState）、GDD_015（WorldState/历史事件 diverged 标志）、GDD_016（StrategicPlan/OpponentModel/随机流位置），三者与战役/世界同一存档边界 round-trip。

## Edge Cases

- 保存请求发生在战役阶段解析中时排队并显示等待安全点。
- 应用崩溃留下临时文件时，启动后只恢复校验通过且版本合理的候选。
- 配置指纹不同但 schema 兼容时，按兼容策略警告、迁移或拒绝，不能静默继续。
- 存档引用已删除配置 ID 时迁移需显式替代或判为不兼容。
- 磁盘空间不足时不得破坏现有存档。

## Failure Cases

校验失败、版本不支持、迁移失败、引用无效、写入中断、权限/空间不足或 Domain 不变量失败均返回稳定错误并保留当前会话及原文件。玩家战败不是存档错误，也不会自动删除槽位。

## Balancing Parameters

本系统无 gameplay 平衡数值。可配置产品参数包括自动存档时机/轮换数量、回放检查点间隔、压缩级别、大小警告和支持的迁移窗口；它们不得影响 Domain 结果。

## UI Requirements

显示时间、玩家角色、城况、阶段、版本、兼容与损坏状态；保存排队有明确反馈；覆盖/删除需确认；失败给出可行动原因；不得在未成功加载前销毁当前会话。

## AI Requirements

保存 AI 的知识、目标、已承诺计划、稳定决策上下文和随机状态。加载后 AI 不得重新规划已经锁定的阶段或获得新的世界真值。

## Save / Load Requirements

本文件即权威体验需求：版本化 DTO、原子写入、迁移副本、完整性校验、配置指纹、知识隔离、随机状态、进行中工作和 round-trip 一致性均为强制项。

## Test Requirements

测试空/完整状态 round-trip、战役与运输中状态、安全点排队、写入中断、磁盘错误模拟、校验损坏、版本迁移、配置不兼容、引用缺失、加载原子性、知识隔离和回放哈希一致。

## MVP Scope

手动存档、有限轮换自动存档、一个 schema 起始版本、迁移框架与测试迁移、完整 slice 状态、战役安全点、配置指纹和确定性回放。无云存档。

## Future Scope

更多迁移版本、存档预览、分支时间线、导入导出、Mod 配置绑定、平台云同步、压缩优化和玩家可分享回放。

## Open Questions

- MVP 自动存档应在每个世界时段、重大后果后，还是两者都做并轮换？
- 配置仅数值变化时允许加载旧存档并采用新值，还是必须绑定原配置包？
- 草稿计划属于可恢复体验状态还是非权威 UI 状态，应保存在哪一层？
