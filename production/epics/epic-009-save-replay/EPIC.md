# Epic: 存档与复现

> **Layer**: Foundation
> **GDD**: design/gdd/gdd-013-save-load.md
> **Architecture Module**: Infrastructure 存档端口（版本化 DTO + 原子写 + 迁移链）+ Domain memento/快照
> **Status**: Ready
> **Stories**: 见下方 Stories 表

## Overview

保存版本化权威状态并复现关键战役。横切所有权威状态，必须随状态模型同步设计：显式版本化 DTO + JSON 经 Infrastructure 端口（禁 Unity 序列化），临时文件原子写入，失败保留上一份有效存档，逆序逐版迁移链。Round-trip 一致性 load(save(s))≡s，含随机流位置——读档不重新抽取已发生结果。加载前验证 schema/配置指纹，不兼容不得部分载入。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|-----------------|-------------|
| ADR-0005: 存档版本与迁移 | 版本化 DTO/JSON + 原子写 + 逆序迁移链；只操作副本 | MEDIUM |
| ADR-0004: 确定性战斗模拟 | 复现依赖状态哈希 + 随机流位置序列化 | HIGH |
| ADR-0002: 架构分层 | 存档经 Infrastructure 端口，Domain 不依赖 Unity 序列化 | HIGH |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-save-001 | 版本化 DTO + 临时文件原子写 + 显式迁移链（操作副本，失败保留上一份） | ADR-0005 ✅ |
| TR-save-002 | Round-trip：load(save(s))≡s；保存随机流位置，读档不重抽已发生结果 | ADR-0005/0004 ✅ |
| TR-save-003 | 加载先验证 schema/配置指纹/校验；不兼容不得部分载入 | ADR-0005 ✅ |
| TR-time-003 | 行动耗时/期限/取消可存档且 round-trip 后产生相同事件序列 | ADR-0005 ✅ |
| TR-intel-003 | 世界真值与玩家知识分别序列化，加载不交叉污染 | ADR-0005 ✅ |

## Definition of Done

- 全部 stories 经 `/story-done` 关闭
- gdd-013 验收标准全部验证
- Round-trip 状态哈希一致；读档后续推进确定性一致
- 不兼容/损坏存档被拒绝，不部分载入、不破坏现有有效档
- 全部 Integration story 在 `tests/` 有通过测试

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | 版本化 DTO + 原子写 + 迁移链 | Integration | ✅ Complete | ADR-0005 |
| 002 | Round-trip 一致性与随机流位置保存 | Integration | Ready | ADR-0005 |
| 003 | 加载校验与不兼容拒绝 | Logic | Ready | ADR-0005 |

## Next Step

`/story-readiness production/epics/epic-009-save-replay/story-001-versioned-atomic-save.md` → `/dev-story`
