# 项目控制清单

Manifest Version: 1

## 当前阶段

`GDD DRAFTS COMPLETE / CROSS-SYSTEM REVIEW NEXT`

当前仅允许概念、设计、架构、生产和质量文档工作。禁止创建 gameplay、战斗、UI、Scene 实现或硬编码平衡值。

## 权威文档顺序

发生冲突时按以下顺序处理：

1. 用户明确指令与本项目设计原则
2. `docs/00_project_brief.md`
3. 已锁定 ADR
4. 已锁定 GDD 与 method spec
5. vertical slice 与 story 验收标准
6. 实现与测试

低层文档不得静默改变高层原则；应提出文档变更并完成审查。

## 强制设计锁

- 兵法是条件组合，不是无条件技能按钮。
- 军师建议而不自动排兵布阵或执行最优方案。
- 战斗是核心特色之一，但不能压倒人物、关系、城市、外交和政治。
- 失败必须产生可继续状态。
- gameplay state 只由 Domain 经 Application Command 路径修改。
- Domain 为纯 C#，不依赖 UnityEngine。
- 所有平衡值数据驱动。
- 战斗结果可确定性复现。
- 存档有 schema version 与迁移策略。
- 每个 public method 在实现前有 method spec 和测试要求。

## 阶段门禁

| Gate | 必需交付物 | 当前状态 | 通过条件 |
|---|---|---|---|
| G0 初始化 | `/start` 文档集 | Passed | 19 个要求文件齐全；路径、非空、范围与设计锁检查通过 |
| G1 概念锁定 | 概念验证、系统地图、MVP、非目标、美术方向 | Passed | 九项核心问题已回答；slice 边界、区域/路线表达、2D 美术方向已锁定；低层问题已分派至 GDD |
| G2 系统设计 | 首批 13 份 GDD | Passed | 13 份 GDD 已创建；每份 18 个强制章节完整；状态保持 Draft，等待 G3 审查 |
| G3 跨系统审查 | contradiction report | Passed（CONCERNS） | 报告 `design/gdd/gdd-cross-review-2026-06-21.md`；唯一阻断（支柱4 外交无 GDD 归属）已闭合——外交受控入口回写 GDD_012 §8；5 项 Warning（断粮传导单点/知识TTL/morale命名/mod_weather范围/破环顺序）**已全部回写修复** |
| G4 技术锁定 | 架构、ADR_0002—0005、质量基础 | Pending | 分层、确定性、配置、存档边界 Accepted |
| G5 方法就绪 | first slice method specs | Pending | 所有 public API 契约与测试要求明确 |
| G6 预制作 | vertical slice、资产、UX、backlog、Sprint | In Progress | **Vertical Slice 完成（2026-06-21，裁定 PROCEED）** — `prototypes/three-kingdom-siege-vertical-slice/`，三条条件链 + 军师 + 存档 round-trip，26 测试绿，[REPORT](../../prototypes/three-kingdom-siege-vertical-slice/REPORT.md)。余下 G6 项（资产/UX/backlog/Sprint）待 `/gate-check pre-production` 后展开 |
| G7 Story 开发 | 单个 Ready story | Blocked | 仅在 G0—G6 通过后按 `/dev-story` 执行 |

## Story Readiness Checklist

Story 只有同时满足以下条件才可进入开发：

- 属于已批准的 vertical slice 和 MVP。
- 有唯一 Story ID、范围和可观察验收标准。
- 引用已审查 GDD 与相关 method spec。
- 列出配置、存档、错误和确定性要求。
- 列出单元、集成或回归测试。
- 不要求 UI 直接改状态、不硬编码数值、不把兵法做成按钮。
- 不依赖未决的阻断设计问题。

## 变更控制

- 修改锁定原则：需要更新 project brief、相关概念文档和 ADR/GDD，并记录原因与影响。
- 新增系统：需要更新 SYSTEM_MAP、MVP_SCOPE/NON_GOALS、GDD_INDEX、EPICS 和测试策略。
- 新增平衡参数：必须进入版本化配置 schema、范围校验和 balance catalog。
- 新增 public method：必须先更新 method spec 与测试清单。
- 存档结构变化：必须增加版本、迁移和兼容测试。

## Definition of Done（实现阶段）

文档追踪完整；实现遵守层级；所有相关测试通过；确定性回归通过；配置无硬编码平衡值；存档兼容已验证；错误路径有测试；相关文档和变更记录已更新。

## 当前阻断项

G3 跨系统审查已通过（CONCERNS，2026-06-21）：唯一阻断已闭合，报告见 `design/gdd/gdd-cross-review-2026-06-21.md`。method specs 和 vertical slice 计划尚未完成，因此任何 gameplay story 都未 Ready。**下一门禁为 G6 预制作**：恢复 `/vertical-slice` 前置验证乐趣（active.md 既定顺序；G4 技术锁定的架构/ADR 已 Accepted，G5 method specs 随首段 Domain 代码落地）。G3 剩余 5 项 Warning 非阻断，于 slice 后或 story 拆分时批量处理。
