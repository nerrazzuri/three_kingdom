# Epic: 项目与 Domain 基础

> **Layer**: Foundation
> **GDD**: 横切（无单一 GDD；服务全部系统）— 受 systems-index「权威状态与系统契约」约束
> **Architecture Module**: Domain 内核 + Application 命令路径 + Infrastructure 配置/存档端口骨架（architecture §分层）
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

建立纯 C# Domain 边界、版本化配置加载与校验、SaveVersion 值对象，以及不依赖 UnityEngine 的可测试基础。本 epic 是所有实现 epic 的技术前置：它锁定层依赖方向（Domain 不依赖任何上层）、确定性数值底座（定点 Q16.16 + 注入随机流，ADR-0004）、配置管线（SO 编辑期 → 不可变 Domain 配置 + 指纹，ADR-0003）和存档版本对象（ADR-0005）。不实现任何 gameplay 规则。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | Domain/Application/Infrastructure/Presentation 四层 + 单向依赖；Domain 纯 C# | HIGH（Unity 程序集边界 + asmdef 隔离 UnityEngine） |
| ADR-0003: 数据驱动配置 | SO 编辑期 → 构建时转不可变 Domain 配置 + 配置指纹 | MEDIUM |
| ADR-0004: 确定性战斗模拟 | 整数/定点 Q16.16 + 注入确定性随机流 + 状态哈希；Domain 权威路径禁 float | HIGH（跨平台浮点一致性） |
| ADR-0005: 存档版本与迁移 | 版本化 DTO + JSON 经 Infrastructure 端口；SaveVersion 表达兼容 | MEDIUM |

## GDD Requirements

本 epic 为工程底座，不直接消费单条 GDD TR；它实现下列系统 TR 所依赖的**前置能力**（确定性数值、配置、存档版本）。具体 TR 在消费它的 epic 中追踪（如 TR-battle-001 依赖本 epic 的定点+随机流，TR-save-001 依赖本 epic 的 SaveVersion）。

| 前置能力 | 服务的下游 TR | ADR |
|---|---|---|
| 纯 C# Domain 程序集边界 | 全部 | ADR-0002 ✅ |
| 定点 Q16.16 + 注入随机流 + 状态哈希 | TR-battle-001/003、TR-weather-001、TR-intel-002 | ADR-0004 ✅ |
| 版本化配置加载/校验 | TR-weather-001、TR-city-002、全部数值 | ADR-0003 ✅ |
| SaveVersion 值对象 | TR-save-001/003、TR-time-003 | ADR-0005 ✅ |

## Definition of Done

本 epic 完成当：
- 全部 stories 经 `/story-done` 实现、审查、关闭
- Domain 程序集不引用 UnityEngine（asmdef 验证）
- 定点/随机流/状态哈希确定性测试通过（同种子→同结果）
- 配置非法范围/缺失引用被明确拒绝且无部分写入
- SaveVersion 能表达 schema 兼容关系并有测试
- 全部 Logic story 在 `tests/unit/` 有通过的测试文件

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 建立纯 C# Domain 与测试边界 | Integration | ✅ Complete | ADR-0002 |
| 002 | 定点数值与确定性随机流底座 | Logic | ✅ Complete | ADR-0004 |
| 003 | 版本化配置加载与校验 | Logic | ✅ Complete | ADR-0003 |
| 004 | SaveVersion 值对象 | Logic | ✅ Complete | ADR-0005 |

## Next Step

✅ **epic-001 全部 story 完成（2026-06-22）**。DoD 已核对：Domain 程序集纯 C# 无 UnityEngine（asmdef/csproj 边界）、定点/随机流/状态哈希确定性、配置非法范围/缺失引用被拒且无部分写入、SaveVersion 表达兼容关系并有测试、全部 Logic story 在 `tests/unit/` 有通过测试（74/74 绿）。
下一模块：`/story-readiness production/epics/epic-002-world-substrate/story-001-deterministic-time.md` → `/dev-story`
