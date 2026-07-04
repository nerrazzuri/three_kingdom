# 架构决策记录索引

| ADR | 标题 | 状态 | 日期 |
|---|---|---|---|
| [ADR-0001](adr-0001-engine-choice.md) | 默认选择 Unity + C# | Accepted | 2026-06-21 |
| [ADR-0002](adr-0002-architecture-layering.md) | 架构分层（四层 + 依赖方向） | Accepted | 2026-06-21 |
| [ADR-0003](adr-0003-data-driven-configuration.md) | 数据驱动配置 | Accepted | 2026-06-21 |
| [ADR-0004](adr-0004-deterministic-battle-simulation.md) | 确定性战斗模拟 | Accepted | 2026-06-21 |
| [ADR-0005](adr-0005-save-versioning-migration.md) | 存档版本与迁移 | Accepted | 2026-06-21 |
| [ADR-0006](adr-0006-deterministic-enemy-ai.md) | 确定性效用敌方 AI | Accepted | 2026-06-24 |
| [ADR-0007](adr-0007-conditional-history-world-model.md) | 条件历史世界模型架构 | Accepted | 2026-06-24 |
| [ADR-0008](adr-0008-city-control-ownership-contract.md) | 城池控制权跨系统所有权契约 | Accepted | 2026-06-24 |
| [ADR-0009](adr-0009-campaign-session-assembly.md) | CampaignSession 装配边界（完整会话脊梁） | Accepted | 2026-06-28 |
| [ADR-0010](adr-0010-conquest-occupation-ownership.md) | 占城归属契约（出征占城复用 004 控制权 + 归属方案 C 种子化判定） | Accepted | 2026-07-04 |
| [ADR-0011](adr-0011-offensive-preparation-model.md) | 多维确定性出征准备模型（六维闭合因果 + 路线复用兵法链 + 兵种杠杆非克制 + 布势非坐标 + 反全知侦察门） | Accepted | 2026-07-04 |
| [ADR-0012](adr-0012-deterministic-zone-battle-engine.md) | 确定性区域战斗引擎（命名区域图非坐标 + 回合状态机 + 部署/战中调整命令 + 条件按区涌现 + 攻守统一 + 战中存档续战） | Accepted | 2026-07-04 |
| [ADR-0013](adr-0013-enemy-zone-ai.md) | 敌方区域AI效用模型（特化 ADR-0006 于区域动作 + 反全知 AiWorldView + 种子softmax + 渐进记忆 + LLM隔离；落地 GDD_016） | Accepted | 2026-07-04 |

## 规则

- ADR 一经 Accepted 不直接覆写结论；变更通过新 ADR 取代并建立链接。
- 架构初稿中的重要未决选择必须在实现前形成 ADR。
- `/technical-setup` 负责创建 ADR_0002 至 ADR_0005，而不是 gameplay 代码。
- ADR_0006 至 ADR_0008 为 2026-06-24 后续生产治理裁定，分别覆盖敌方 AI、条件历史世界模型与城池归属唯一权威。
