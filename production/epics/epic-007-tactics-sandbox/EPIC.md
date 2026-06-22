# Epic: 兵法沙盒结算

> **Layer**: Core
> **GDD**: design/gdd/gdd-010-battle-tactics-sandbox.md · gdd-011-morale-fatigue.md
> **Architecture Module**: Domain 确定性战役解析管线 + 士气/疲劳/军纪模型
> **Status**: ✅ Complete（2026-06-22）
> **Stories**: 见下方 Stories 表

## Overview

验证假退、伏兵、断粮、守城、夜袭等条件链——非按钮式、确定性战役。确定性 Domain 模拟：相同初始快照+配置指纹+随机种子+有序命令流→相同事件与状态哈希。兵法为多条件全部成立时的涌现结果，系统只在事件后打复盘标签，绝无同名无条件执行按钮（核心设计锁）。阶段按稳定管线解析（验证→移动→侦测→交战→损耗→士气→触发→发布），异常回滚整个原子阶段。士气/疲劳/军纪三维独立不合并。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0004: 确定性战斗模拟 | 整数/定点 + 注入随机流 + 状态哈希 + 稳定管线（核心） | HIGH |
| ADR-0002: 架构分层 | 战役为 Domain 权威，UI 只读投影与因果链 | HIGH |
| ADR-0003: 数据驱动配置 | 战力/阈值/士气权重来自版本化配置 | MEDIUM |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-battle-001 | 确定性模拟：同快照+指纹+种子+有序命令流→同事件与状态哈希 | ADR-0004 ✅ |
| TR-battle-002 | 兵法为多条件涌现，事件后打复盘标签，绝无同名无条件按钮 | ADR-0004 ✅ |
| TR-battle-003 | 阶段按稳定管线解析；异常回滚整个原子阶段 | ADR-0004 ✅ |
| TR-cohesion-001 | 士气/疲劳/军纪三维独立不合并；士气事件按受众权重聚合且幂等 | ADR-0004 ✅ |
| TR-cohesion-002 | 阈值综合军纪/指挥/退路（非单值）；拆分/合并按人数加权非取最大 | ADR-0004 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-010/011 验收标准验证
- 状态哈希复现有测试（同种子→同哈希，slice 已验证 33 测试绿可作回归基线）
- 无条件按钮负向测试（无前置条件→兵法不触发）
- 管线异常原子回滚有测试；士气三维独立有测试
- 全部 Logic story 在 `tests/unit/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 确定性战役解析管线与状态哈希 | Logic | ✅ Complete | ADR-0004 |
| 002 | 条件链涌现与复盘标签（无无条件按钮） | Logic | ✅ Complete | ADR-0004 |
| 003 | 士气/疲劳/军纪三维与阈值检查 | Logic | ✅ Complete | ADR-0004 |

## Next Step

✅ **Epic 全部 3 story 关闭（2026-06-22）**。DoD 核对：状态哈希复现有测（同种子同哈希/乱序稳定/不同种子不同）；无条件按钮负向测试（缺前置不涌现 + 反射无执行方法）；管线异常原子回滚有测；士气三维独立 + 幂等 + 多输入阈值 + 人数加权有测（26 测）。解锁 epic-008（后果写回）。
