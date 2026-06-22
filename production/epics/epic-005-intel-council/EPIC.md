# Epic: 情报与军议

> **Layer**: Core
> **GDD**: design/gdd/gdd-007-intelligence-recon.md · gdd-008-war-council.md
> **Architecture Module**: Domain 情报四层模型 + 军师建议层（只读知识投影）
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

让玩家基于来源、时效和置信度判断局势。世界真值/观察/报告/阵营知识四层分离——UI 只读阵营知识，绝不泄露真值。报告含置信度（来源可靠性，非真实概率）、时效衰减、估计区间；暴露由确定性随机流判定。军议读召开时合法知识快照，知识/资源变化后建议标记过时不静默更新；军师只输出条件化建议（观察/假设/条件/风险/缺口/置信），不输出综合成功率、唯一推荐或自动命令（P11 设计锁）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | 知识投影只读；UI 不触真值 | HIGH |
| ADR-0004: 确定性战斗模拟 | 暴露判定/时效衰减用确定性随机流 | HIGH |
| ADR-0003: 数据驱动配置 | 来源可靠性/衰减率/区间宽度来自版本化配置 | MEDIUM |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-intel-001 | 真值/观察/报告/阵营知识四层分离；UI 只读阵营知识 | ADR-0002 ✅ |
| TR-intel-002 | 报告含置信度(来源可靠性非真实概率)/时效衰减/估计区间；暴露由确定性随机流判定 | ADR-0004 ✅ |
| TR-intel-003 | 真值与玩家知识分别序列化，加载不交叉污染 | ADR-0005 ✅（落档于 epic-009） |
| TR-council-001 | 军议读召开时知识快照；变化后建议标记过时不静默更新 | ADR-0002 ✅ |
| TR-council-002 | 军师只输出条件化建议，不输出综合成功率/唯一推荐/自动命令 | ADR-0002 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-007/008 验收标准验证
- 四层分离有测试（UI 投影不含真值字段）；时效衰减确定性可复现
- 军师输出无综合成功率/最优解（断言式负向测试）
- 知识 TTL 时效权威归 007（003 只记 observed_time 复用 007 降级）
- 全部 Logic story 在 `tests/unit/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 情报四层分离与只读投影 | Integration | ✅ Complete | ADR-0002 |
| 002 | 报告置信/时效/区间与确定性暴露 | Logic | ✅ Complete | ADR-0004 |
| 003 | 军师条件化建议（无最优解/无成功率） | Logic | Ready | ADR-0002 |

## Next Step

`/story-readiness production/epics/epic-005-intel-council/story-001-intel-four-tier.md` → `/dev-story`
