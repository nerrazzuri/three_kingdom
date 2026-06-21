# Story Backlog 初稿

## 状态说明

以下 Story 均为 `Draft / Not Ready`。编号用于追踪，不授权实现；只有相关 GDD、method spec、ADR 与验收标准通过门禁后才可执行 `/dev-story STORY_ID`。

| Story ID | Epic | Story | 主要验收结果 | 状态 |
|---|---|---|---|---|
| STORY_001_001 | EPIC_001 | 建立 Unity 项目与纯 C# Domain 测试边界 | Domain 测试不依赖 UnityEngine | Not Ready |
| STORY_001_002 | EPIC_001 | 加载并校验版本化配置 | 非法范围与缺失引用被明确拒绝 | Not Ready |
| STORY_001_003 | EPIC_001 | 定义 SaveVersion 值对象 | 支持比较、解析和兼容判断 | Not Ready |
| STORY_002_001 | EPIC_002 | 推进 Domain 时间 | 同一输入得到相同时段变化与事件 | Not Ready |
| STORY_002_002 | EPIC_002 | 解析基础天气与风向 | 仅通过显式种子和配置变化 | Not Ready |
| STORY_003_001 | EPIC_003 | 创建角色核心状态 | 能力、性格、职责满足不变量 | Not Ready |
| STORY_003_002 | EPIC_003 | 计算命令执行意愿 | 关系与职责产生可解释结果 | Not Ready |
| STORY_004_001 | EPIC_004 | 结算城市时段产耗 | 粮食、民心、工事变化可追踪 | Not Ready |
| STORY_004_002 | EPIC_004 | 结算军队补给消耗 | 路线与断粮影响供给状态 | Not Ready |
| STORY_005_001 | EPIC_005 | 生成侦察报告 | 报告包含来源、时效、置信度 | Not Ready |
| STORY_005_002 | EPIC_005 | 生成军师候选建议 | 建议描述条件与风险，不自动部署 | Not Ready |
| STORY_006_001 | EPIC_006 | 提交战前部署计划 | 校验人物、兵力、资源和时机冲突 | Not Ready |
| STORY_007_001 | EPIC_007 | 解析伏兵条件链 | 无条件不触发，成立/暴露均可解释 | Not Ready |
| STORY_007_002 | EPIC_007 | 解析断粮与士气连锁 | 后勤变化经时段影响士气和行为 | Not Ready |
| STORY_007_003 | EPIC_007 | 解析假退诱敌 | 敌将性格、情报和军纪共同作用 | Not Ready |
| STORY_008_001 | EPIC_008 | 结算战役跨系统后果 | 结果写回人物、关系、城市与名声 | Not Ready |
| STORY_008_002 | EPIC_008 | 提供战败延续状态 | 至少撤退、失城或问责路径可继续 | Not Ready |
| STORY_009_001 | EPIC_009 | 保存并加载 slice 状态 | round-trip 后权威状态一致 | Not Ready |
| STORY_009_002 | EPIC_009 | 复现战役命令流 | 重放状态哈希与原运行一致 | Not Ready |
| STORY_010_001 | EPIC_010 | 展示情报置信度和来源 | 不泄露隐藏真值且不只依赖颜色 | Not Ready |
| STORY_010_002 | EPIC_010 | 展示战果因果链 | 玩家能查看关键来源、修正和后果 | Not Ready |

## Story 就绪规则

每个 Story 在进入 Ready 前必须补齐：引用文档、Command/API 契约、配置依赖、存档影响、错误码、确定性要求、具体测试用例与可观察验收场景。`Not Ready` Story 不得开始编码。

## 优先原则

优先建立最短可复现闭环，而非并行铺开所有系统。第一条目标链为：配置与版本 → 时间 → 人物/城市最小状态 → 情报 → 准备 → 单一条件链结算 → 后果 → 存档重放。
