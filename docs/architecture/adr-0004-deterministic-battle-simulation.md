# ADR-0004：确定性战斗模拟

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
| **Knowledge Risk** | LOW — 决策刻意规避引擎数值行为，核心结算用整数/定点，不依赖 Unity 6.x 浮点或物理 |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`、`docs/architecture/architecture-overview.md`、`docs/architecture/adr-0002-architecture-layering.md` |
| **Post-Cutoff APIs Used** | None — Domain 不调用 Unity 数学/物理；时间推进用 Domain 时段非 Unity Time |
| **Verification Required** | 在 Windows/macOS/Linux 三平台对同一 golden scenario 运行，验证状态哈希逐位相同；验证 Domain 无任何 `float` 进入权威结算路径 |

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | ADR-0001（Unity + C#）、ADR-0002（架构分层，已 Accepted） |
| **Enables** | ADR-0005（存档版本与迁移——回放包与状态哈希依赖本 ADR 的确定性契约） |
| **Blocks** | 战斗/时间/天气/情报/士气/后勤相关 Epic——确定性契约 Accepted 前不得开始解析逻辑实现 |
| **Ordering Note** | 必须先于战役解析 story；与 ADR-0003（数据驱动配置）平行，但配置指纹纳入本 ADR 的哈希输入 |

## Context

### Problem Statement

战斗是项目核心特色，GDD_010 要求"相同初始快照 + 配置指纹 + 随机种子 + 有序命令流 → 相同事件与状态哈希"，
GDD_013 的回放包与 round-trip 校验、GDD_001 的稳定事件排序、GDD_002/GDD_007 的确定性随机流均依赖同一确定性契约。
若数值结算用平台相关的 IEEE 754 浮点，跨平台（甚至同平台不同编译器/SIMD）可能产生微小差异，使状态哈希不一致，
回放校验与差异复现失效。本 ADR 锁定确定性模拟的数值、随机、排序与哈希契约。

### Constraints

- 技术约束：C# `float`/`double` 跨平台逐位一致性无法证明（JIT、SIMD、FMA 差异）。
- 架构约束：必须落在 ADR-0002 的 Domain 层内，不依赖 UnityEngine、帧率或系统时间。
- 设计约束：兵法须作为条件链涌现，确定性不得削弱"条件成立"的可解释性（GDD_010 设计锁）。
- 资源约束：定点运算需自建或引入定点库；须在 MVP 复杂度内可控。

### Requirements

- 所有进入状态哈希的战役结算必须确定性、跨平台逐位可复现。
- 随机性只能经显式注入的确定性随机流，在规则声明的检查点消费（对接注册表 `implicit_global_random` 禁止项）。
- 同一时间点事件按稳定复合键全序解析，不依赖帧率或集合遍历顺序。
- 每个战役阶段原子；解析异常回滚整个阶段，不产生半结算状态。
- 满足 TR-time-001/002、TR-weather-001、TR-map-002、TR-intel-002、TR-battle-001/002/003、TR-cohesion-002、TR-supply-002。

## Decision

确定性战斗模拟建立在四根支柱上：**整数/定点数值**、**注入式确定性随机流**、**稳定全序事件排序**、**状态哈希校验**。

### 1. 数值策略：整数/定点为权威结算

- 所有影响状态哈希的战役结算（战斗力、损耗、士气/疲劳/补给变化、概率判定阈值）使用整数或定点数（Q 格式定点，如 Q16.16）。
- 概率经"定点阈值 vs 定点随机值"比较实现，不用浮点比较。
- 浮点**仅**允许出现在非权威的 Presentation/UI（动画插值、显示百分比），永不回写 Domain。
- GDD 中的 `[0,1]` 比例、乘数等在 Domain 内以定点表示；公式示例中的小数是定点的可读写法。

### 2. 注入式确定性随机流

```csharp
interface IDeterministicRandom {
    // 只在规则声明的检查点消费；跳过不适用检查不改变其他流语义
    FixedPoint Next(CheckpointId checkpoint);
}
```

- 随机流位置（序号）是权威状态，纳入存档（GDD_013）与状态哈希。
- 不同子系统（天气转移、侦察暴露、溃散判定）使用独立命名流或同一流的声明式检查点，确保跳过不适用检查不污染他流。

### 3. 稳定全序事件排序

```
sort_key(e) = ( T(e.day, e.segment), e.priority, e.stable_id )
```

- 三级复合键保证同刻事件唯一全序（GDD_001 §Formulas）。优先级与 stable_id 来自配置/确定性 ID 生成器，不依赖集合遍历顺序。

### 4. 状态哈希与回放契约

```
state_hash(phase) = H( initial_snapshot ‖ config_fingerprint ‖ seed ‖ ordered_orders[0..phase] )
```

- `H` 为确定性哈希（如对定点状态的规范字节序做 FNV/xxHash），输入含配置指纹（对接 ADR-0003）。
- 每个战役阶段记录 checkpoint 哈希；回放包（ReplayPackage）逐 checkpoint 比对（GDD_013 §Formulas replay_valid）。

### 阶段解析管线（原子）

```
命令验证 → 移动/接触 → 侦测与隐蔽 → 交战匹配 → 损耗与位置
        → 士气/疲劳/军纪 → 触发与撤退 → 事件发布
解析异常 → 回滚整个原子阶段，不产生部分推进
```

### Key Interfaces

```csharp
readonly struct FixedPoint { /* Q16.16 定点，含 +-*/、clamp、比较 */ }

interface IBattleResolver {
    // 纯函数式：相同输入 → 相同输出与哈希
    BattlePhaseResult ResolvePhase(BattleSnapshot s, OrderedCommands cmds,
                                   IDeterministicRandom rng, ValidatedConfig cfg);
    StateHash HashOf(BattleSnapshot s);
}
```

## Alternatives Considered

### Alternative 1：浮点 + 容差重放

- **描述**：用 `float`/`double` 结算，重放时允许微小数值容差，哈希用量化后的值。
- **Pros**：实现简单、与 GDD 公式小数直接对应、性能略优。
- **Cons**：跨平台逐位一致无法证明；容差使状态哈希不再是严格相等判定，差异复现与回放校验不可靠；与"严格状态哈希一致"需求冲突。
- **Rejection Reason**：GDD_010/GDD_013 明确要求逐位可复现的状态哈希，容差方案无法满足；architecture-overview 已预先排除。

### Alternative 2：仅全状态快照（无哈希校验）

- **描述**：每阶段存全量状态快照，重放靠快照比对而非哈希与命令流。
- **Pros**：无需定点、无需哈希实现。
- **Cons**：存档体积大；无法用"种子+命令流"轻量回放（GDD_013 回放包失效）；差异定位困难（只能整体比对）。
- **Rejection Reason**：丢失轻量回放包能力，违背 GDD_013 §Main Rules"回放包由初始快照+配置指纹+种子+命令流生成"。

## Consequences

### Positive

- 跨平台逐位可复现，状态哈希成为严格相等判定，回放校验与差异复现可靠。
- 随机流位置作为权威状态，存档 round-trip 后续推进完全一致（GDD_013）。
- 为 ADR-0005 的回放包与 round-trip 校验提供确定性基础。
- 兵法条件链解析可逐 checkpoint 复盘，支撑因果展示（GDD_010 §10）。

### Negative

- 定点运算增加实现与表达成本（公式需从小数转定点 Q 格式）。
- 开发者须时刻警惕 `float` 渗入权威路径。
- 自建定点类型需充分测试（溢出、舍入、范围）。

### Risks

- **风险**：`float` 意外进入权威结算。**缓解**：注册表已禁止 `implicit_global_random`；新增约定"Domain 权威路径禁用 float"，静态检查 + code-review + 三平台哈希测试捕获。
- **风险**：定点溢出或舍入偏差累积。**缓解**：Q 格式范围与舍入规则配置化并测试；边界值测试覆盖。
- **风险**：哈希实现非确定（依赖字典序遍历）。**缓解**：哈希前对状态做规范字节序排序，加跨平台哈希一致测试。

## GDD Requirements Addressed

| GDD 系统 | 需求（TR） | 本 ADR 如何满足 |
|---|---|---|
| 时间 (GDD_001) | TR-time-001/002：稳定事件全序、阶段预算跨越 | 稳定复合键排序 + 阶段管线原子结算 |
| 天气 (GDD_002) | TR-weather-001：确定性随机流天气转移 | 注入式确定性随机流 + 定点权重比较 |
| 地图 (GDD_003) | TR-map-002：确定性寻路 + 相向接触 | 平局按 stable_id、定点进度比较 |
| 情报 (GDD_007) | TR-intel-002：确定性暴露判定 | 定点阈值 vs 注入随机流，检查点消费 |
| 战斗 (GDD_010) | TR-battle-001/002/003：状态哈希、条件链涌现、阶段原子回滚 | 四支柱 + 原子阶段管线 + 复盘 checkpoint 哈希 |
| 士气疲劳 (GDD_011) | TR-cohesion-002：阈值多因素判定 | 定点阈值 + 注入随机流的溃散检查 |
| 后勤 (GDD_012) | TR-supply-002：断粮按时段传导 | 定点供给状态逐时段确定性递减 |

## Performance Implications

- **CPU**：定点运算与浮点相当或略慢；回合制/时段制无每帧压力，远在预算内。
- **Memory**：状态哈希与随机流位置开销极小；回放包仅存种子+命令流，远小于全快照。
- **Load Time**：哈希计算仅在阶段边界，可忽略。
- **Network**：不适用（离线单机）。

## Migration Plan

当前无源码。首次实现战役解析时即以定点 + 注入随机流 + 哈希搭建（对接 STORY_009_002
"复现战役命令流"与 STORY_009_001"保存并加载 slice 状态"）。

## Validation Criteria

- [ ] 同一 golden scenario 在 Windows/macOS/Linux 产生逐位相同的状态哈希
- [ ] Domain 权威结算路径静态检查无 `float`/`double`
- [ ] 回放包（种子+命令流）重放 checkpoint 哈希与原运行一致（GDD_013）
- [ ] 随机流跳过不适用检查不改变其他流后续取值
- [ ] 阶段解析异常完整回滚，无半结算状态残留

## Related Decisions

- ADR-0001：选择 Unity + C#
- ADR-0002：架构分层（确定性逻辑落在 Domain 层）
- ADR-0005（待撰写）：存档版本与迁移，复用本 ADR 的状态哈希与回放契约
- `docs/architecture/architecture-overview.md`：本 ADR 固化其"确定性"章节
