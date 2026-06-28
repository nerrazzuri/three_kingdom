# 项目控制清单

Manifest Version: 2 (2026-06-28)

## 当前阶段

`PRODUCTION — Domain 内核全部实现，待装配可玩循环`

Pre-Production→Production 闸门已通过（2026-06-21，CONCERNS）。**2026-06-28 校准**：epic-001~012 全部 Complete（44 story；Sprint 02 smoke/team-qa 基线 556 测试全绿；当前本地回归 564/564 全绿、`-warnaserror` 0；零禁则违反、零确定性泄漏）——Foundation/Core/Feature/Meta 各层 Domain 内核均已落地并通过测试。

**当前阶段焦点 = 装配（assembly）**：Domain 内核已就绪，但被装配进可运行 session 的仅竖切守城那一局；Meta 层（epic-011/012 生涯/世界模型）与多数 Domain 系统是已验证但**未接入可玩循环**的内核。下一阶段重心从"造内核"转向"装配可玩太守循环 + 敌方 AI"——详见 `docs/reviews/full-game-review-2026-06-28.md`（全游戏 review，2 项 Blocking）。

仍须遵守全部强制设计锁与层级。CONCERNS guardrail 持续有效。

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
- **【红线】零复制现有三国游戏/作品资产**：不得复制、扫描、临摹、AI 喂图仿制、二次加工，或以可识别方式借用任何**现有**三国题材游戏或商业作品（含其前作/同系列/同 IP）的美术、音频、UI 布局、图标、字体、文案、命名、数值表或任何专有表达。全部资产**原创**。历史史料与《三国演义》原著（公有领域）可作**题材与素材来源**，但具体表达（构图、配色搭配、角色造型、措辞）必须自创。任何资产入库前须能说明其原创依据；存疑即不入库。违反此红线的产出一律拒收、不得 commit。
- **所有文件中文撰写**：本项目全部文档、设计、面向团队的产出、代码内注释/doc 注释、commit 说明与 PR 正文均以**中文**撰写。**唯一例外**——代码标识符（类/方法/字段/命名空间/文件名）遵循 `technical-preferences.md` 的英文命名约定；引擎/第三方 API 名、ADR/TR 编号、技术专名保留原文。即「中文叙述 + 英文标识符」。

## 阶段门禁

| Gate | 必需交付物 | 当前状态 | 通过条件 |
|---|---|---|---|
| G0 初始化 | `/start` 文档集 | Passed | 19 个要求文件齐全；路径、非空、范围与设计锁检查通过 |
| G1 概念锁定 | 概念验证、系统地图、MVP、非目标、美术方向 | Passed | 九项核心问题已回答；slice 边界、区域/路线表达、2D 美术方向已锁定；低层问题已分派至 GDD |
| G2 系统设计 | 首批 13 份 GDD | Passed | 13 份 GDD 已创建；每份 18 个强制章节完整；状态保持 Draft，等待 G3 审查 |
| G3 跨系统审查 | contradiction report | Passed（CONCERNS） | 报告 `design/gdd/gdd-cross-review-2026-06-21.md`；唯一阻断（支柱4 外交无 GDD 归属）已闭合——外交受控入口回写 GDD_012 §8；5 项 Warning（断粮传导单点/知识TTL/morale命名/mod_weather范围/破环顺序）**已全部回写修复** |
| G4 技术锁定 | 架构、ADR_0002—0005、质量基础 | Pending | 分层、确定性、配置、存档边界 Accepted |
| G5 方法就绪 | first slice method specs | Pending | 所有 public API 契约与测试要求明确 |
| G6 预制作 | vertical slice、资产、UX、backlog、Sprint | Passed（CONCERNS） | Vertical Slice PROCEED（33 测试绿）；art-bible v1.0 全 9 节 + AD 签核 APPROVED；三关键屏 UX（main-menu/hud/pause）/ux-review APPROVED；9 epics + 28 stories（`production/epics/`）；sprint-01 重生指向真实 story 路径。Pre-Production→Production 闸门通过（2026-06-21，CONCERNS） |
| G7 Story 开发 | 单个 Ready story | Unblocked（Foundation） | G0—G6 通过；可按 `/story-readiness`→`/dev-story` 实现 Foundation story（epic-001/002/009）。**硬前置已闭合**：纯 C# Domain + NUnit 示例测试 `dotnet test` 本地绿（`tests/unit/ThreeKingdom.Domain.Tests`），CI `domain-tests` job 无需 UNITY_LICENSE 即绿（首次 GitHub 绿待 push 验证） |

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

**无阻断项**。G0—G6 全部通过；Pre-Production→Production 闸门 2026-06-21 通过（CONCERNS）。Foundation story（epic-001/002/009）可开工。

**Production 早期 guardrail（CONCERNS，非阻断，须主动守护——详见 active.md「四总监 Panel 裁定」段）**：
- CD：非战斗决策空间须经体验验证（防薄皮战斗沙盒）；降输入摩擦须过 P11 审查（尤其军师层）；核心幻想须在 Unity 表现层重验（无 player-journey）。
- TD/PR：CI 首次 GitHub 绿待 push（本地 `dotnet test` 已绿、workflow 已无许可依赖）；表现层帧/draw-call/内存 + Unity 序列化适配须早期打样；里程碑日历/容量基线 sprint 1 内回填。
- AD：§4 对比度首批资产入库前工具实测；art-bible §7 高对比变体配色待补（高对比功能实现前）。
- 排程注意：epic-009 S3（存档校验）实质依赖 epic-005 S1（情报四层）→ 归 Core 期，勿当 Foundation 早期任务。
