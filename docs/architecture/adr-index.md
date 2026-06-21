# 架构决策记录索引

| ADR | 标题 | 状态 | 日期 |
|---|---|---|---|
| [ADR-0001](adr-0001-engine-choice.md) | 默认选择 Unity + C# | Accepted | 2026-06-21 |
| [ADR-0002](adr-0002-architecture-layering.md) | 架构分层（四层 + 依赖方向） | Accepted | 2026-06-21 |
| [ADR-0003](adr-0003-data-driven-configuration.md) | 数据驱动配置 | Accepted | 2026-06-21 |
| [ADR-0004](adr-0004-deterministic-battle-simulation.md) | 确定性战斗模拟 | Accepted | 2026-06-21 |
| [ADR-0005](adr-0005-save-versioning-migration.md) | 存档版本与迁移 | Accepted | 2026-06-21 |

## 规则

- ADR 一经 Accepted 不直接覆写结论；变更通过新 ADR 取代并建立链接。
- 架构初稿中的重要未决选择必须在实现前形成 ADR。
- `/technical-setup` 负责创建 ADR_0002 至 ADR_0005，而不是 gameplay 代码。
