# ADR-0001：选择 Unity + C#

- 决策范围：默认客户端引擎与主要语言

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
| **Knowledge Risk** | HIGH — Unity 6.x 超出 LLM 训练截止日期（2025-05），使用任何 Unity API 前须交叉核对 `docs/engine-reference/unity/` |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md` |
| **Post-Cutoff APIs Used** | 暂无具体 API 依赖；本 ADR 仅锁定引擎与语言方向。具体 LTS 版本号、程序集边界由后续 ADR 确定 |
| **Verification Required** | 创建 Unity 工程时锁定确切 LTS 补丁版本；验证纯 C# Domain 程序集可独立于 UnityEngine 编译与测试 |

> **注**：Knowledge Risk 为 HIGH，若项目升级 Unity 版本，本 ADR 须重新验证——标记为 Superseded 并撰写新 ADR。

## ADR Dependencies

| 字段 | 值 |
|------|-----|
| **Depends On** | None — 本 ADR 为根技术决策 |
| **Enables** | ADR-0002（架构分层）、ADR-0003（数据驱动配置）、ADR-0004（确定性战斗模拟）、ADR-0005（存档版本与迁移） |
| **Blocks** | 全部 gameplay 实现 Epic——引擎与语言锁定前不得开始编码 |
| **Ordering Note** | 必须先于 ADR-0002–0005 完成，因后续决策均假设 Unity + C# 技术栈 |

## 背景（Context）

项目是离线单机三国沙盒战略 RPG，包含大量互相影响、需要测试与调平的模拟规则，并预期支持回合制/大地图/战棋表达、存档、编辑器工具和未来数据扩展。

## 决策

默认使用 Unity + C#。核心 Domain Layer 使用不依赖 UnityEngine 的纯 C#；Unity 负责输入、展示、Scene 组织、资源管线和平台集成。

## 理由

- C# 强类型适合复杂状态、契约和自动化测试。
- Unity 对回合制、地图、战棋、UI、离线构建和编辑器工具有成熟支持。
- ScriptableObject 与 JSON 可组成编辑友好且数据驱动的配置管线。
- Unity 生态适合本地存档、资产管理与未来 Mod 工具探索。
- 项目维护与自动生成 C# 的稳定性符合预期工作流。

## 后果

### 正面

- Domain 可独立于引擎测试和复现。
- 展示层与模拟层能明确隔离。
- 可利用 Unity 编辑器改善配置和内容生产。

### 成本与风险

- 必须主动防止 MonoBehaviour 侵入 gameplay 规则。
- ScriptableObject 不能直接成为运行时权威状态。
- Unity 序列化限制要求显式 DTO、校验和版本迁移。
- 引擎升级和包版本必须受控。

## 约束

- UI 与 MonoBehaviour 不得直接修改 Domain state。
- 所有玩家操作通过 Commands / Application Services。
- 平衡数值进入版本化配置，不写入方法体。
- 战斗模拟不依赖帧率、Unity 时间或隐式随机。

## 未采用方向

Godot、Unreal 与自研引擎在本阶段不重新比较，因为项目已明确默认技术方向。只有在出现经验证的阻断性需求时，才创建新 ADR 重新评估。

## 后续决策

Unity LTS 版本、程序集边界、配置转换、确定性数值和存档序列化由后续 ADR 锁定。

## GDD Requirements Addressed

基础性决策——无直接单一 GDD 需求来源；本 ADR 为所有 GDD 系统提供技术基底。其满足的跨系统需求如下：

| GDD 来源 | 系统 | 需求 | 本 ADR 如何满足 |
|---------|------|------|----------------|
| 全部 GDD §Data Model | 全部 | 权威状态须可确定性模拟、可复现 | 纯 C# Domain 层不依赖 UnityEngine，模拟引擎无关且可独立测试 |
| 全部 GDD §Balancing Parameters | 全部 | 平衡数值须数据驱动、不硬编码 | ScriptableObject + JSON 配置管线，构建时转换为不可变 Domain 配置 |
| GDD_013 存档与读档 §Save/Load | 存档 | 状态须可序列化、可版本迁移 | C# 显式 DTO + 版本化序列化，Unity 序列化限制通过显式 DTO 规避 |
| GDD_010 兵法沙盒战斗 §Test Requirements | 战斗 | 战役结算须确定性、可复盘 | Domain 不依赖帧率/Unity 时间/隐式随机，随机性经显式注入种子 |

> 启用（Enables）：本决策解锁所有需要确定性模拟、数据驱动配置和可测试 Domain 逻辑的 GDD 系统。
