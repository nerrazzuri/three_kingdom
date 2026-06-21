# Sprint 01 初稿：基础与可测试 Domain

## 状态

`Planned / Blocked by G1—G6`

本 Sprint 是预制作后的首个候选开发 Sprint；当前只记录意图，不允许开始代码实现。

## Sprint Goal

建立项目基础、Domain Layer 测试环境、时间系统、配置加载、角色核心模型和 SaveVersion 对象，使后续 slice 系统拥有可测试、可版本化且不依赖 UnityEngine 的基础。

## 候选范围

- STORY_001_001：Unity 项目与纯 C# Domain 测试边界。
- STORY_001_002：版本化配置加载与校验。
- STORY_001_003：SaveVersion 值对象。
- STORY_002_001：Domain 时间推进。
- STORY_003_001：角色核心状态。

最终承诺范围必须在 `/preproduction` 后依据团队容量重新确认；未满足 Ready Checklist 的 Story 自动移出 Sprint，而不是降低门禁。

## Definition of Ready

- 概念、GDD 跨系统审查和技术 ADR 已通过。
- 每个候选 Story 有相关 method spec。
- Unity LTS、测试程序集、配置格式和存档版本规则已锁定。
- 验收标准可自动化，且不存在依赖 gameplay UI 的测试。

## Sprint 验收

- Domain 程序集不能引用 UnityEngine。
- public API 均有契约、正常/边界/失败测试。
- 时间推进与配置解析结果可确定性复现。
- 配置非法时产生稳定错误，不留下部分状态。
- SaveVersion 能表达 schema 兼容关系。
- 角色模型不变量由构造与测试保护。
- 未创建战斗、计策、UI 或 Scene gameplay 实现。

## 风险与控制

- 若配置系统开始追求通用 Mod 框架，收缩为 slice 所需 schema 与校验。
- 若 Unity 适配渗入 Domain，停止 Story 并修正程序集依赖。
- 若角色模型试图覆盖全武将人生，收缩到 slice 所需能力、性格、职责和标识。
- 若时间系统暗含未审查的城市/人物节奏，返回 GDD 处理尺度冲突。

## Sprint 退出条件

所有承诺 Story 达到项目 Definition of Done；未通过的 Story 返回 backlog 并记录实际阻断，不以跳过测试或文档换取 Sprint 完成。
