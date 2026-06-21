# Epic: 人物与关系

> **Layer**: Core
> **GDD**: design/gdd/gdd-005-character.md · gdd-006-relationship-faction.md
> **Architecture Module**: Domain 人物/关系权威模型
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

让性格、职责、信任影响信息与执行，而非解锁无条件技能。人物能力/性格/健康影响过程质量与执行意愿；职责决定合法权限（能力高不绕过授权）。关系为方向性多维（A→B 与 B→A 分存、不对称），由具名事件驱动且对同一事件幂等；关系只影响请求/支持/执行承诺，不凭空授予法律权限。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0002: 架构分层 | 人物/关系为 Domain 权威，经 Command 路径修改 | HIGH |
| ADR-0004: 确定性战斗模拟 | 执行意愿/coop_score 计算用定点，确定性 | HIGH |
| ADR-0003: 数据驱动配置 | 能力/性格/关系维度数值来自版本化配置 | MEDIUM |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-character-001 | 能力/性格/健康影响过程质量与执行意愿，不解锁无条件技能/光环 | ADR-0002/0004 ✅ |
| TR-character-002 | 职责决定合法权限；能力高不绕过授权；同时段只承担兼容任务 | ADR-0002 ✅ |
| TR-relationship-001 | 方向性多维关系（不对称）；变化由具名事件驱动且幂等 | ADR-0002/0004 ✅ |
| TR-relationship-002 | 关系只影响请求/支持/执行承诺，不凭空授权；授权受期限/撤销约束 | ADR-0002 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭；gdd-005/006 验收标准验证
- 关系不对称性与事件幂等有测试；执行意愿确定性可复现
- 破环顺序遵守（关系事件结算 → 人物意愿/质量，systems-index §跨系统结算顺序）
- 全部 Logic story 在 `tests/unit/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 人物核心状态与不变量 | Logic | ✅ Complete | ADR-0002 |
| 002 | 职责权限与命令执行意愿 | Logic | Ready | ADR-0004 |
| 003 | 方向性多维关系与事件幂等 | Logic | Ready | ADR-0004 |

## Next Step

S1 ✅ Complete（2026-06-22）。下一步：`/story-readiness production/epics/epic-003-character-relationship/story-002-duty-authority-willingness.md` → `/dev-story`
