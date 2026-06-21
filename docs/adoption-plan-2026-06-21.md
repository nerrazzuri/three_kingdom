# 模板采纳计划

> **生成日期**：2026-06-21
> **项目阶段**：技术设置（Technical Setup）
> **引擎**：Unity + C#（已在 ADR-0001 锁定；尚未写入模板配置）
> **模板版本**：v1.0+

按顺序逐项完成。每完成一项打勾。随时重新运行 `/adopt` 检查剩余差距。

---

## 第一步：修复阻断性差距（BLOCKING）

### 1a. 将全部外部文档迁移至模板路径

**问题**：所有设计、架构、生产和概念文档存放在 `D:\Projects\三国演义\docs`，
模板技能无法读取该外部目录。模板的 `design/gdd/`、`docs/architecture/`、
`production/sprints/` 等路径均为空，所有技能调用均无法找到任何文件。

**迁移映射表**：

| 外部来源 | 模板目标路径 |
|---|---|
| `docs/01_concept/CONCEPT.md` | `design/gdd/game-concept.md` |
| `docs/01_concept/CORE_PILLARS.md` | `design/gdd/game-pillars.md` |
| `docs/01_concept/MVP_SCOPE.md` | `design/concept/mvp-scope.md` |
| `docs/01_concept/NON_GOALS.md` | `design/concept/non-goals.md` |
| `docs/01_concept/ART_DIRECTION.md` | `design/concept/art-direction.md` |
| `docs/01_concept/SYSTEM_MAP.md` | `design/gdd/systems-index.md` |
| `docs/01_concept/CONCEPT_VALIDATION.md` | `design/concept/concept-validation.md` |
| `docs/00_project_brief.md` | `docs/project-brief.md` |
| `docs/02_design/gdd/GDD_001_GAME_TIME.md` | `design/gdd/gdd-001-game-time.md` |
| `docs/02_design/gdd/GDD_002_SEASON_WEATHER.md` | `design/gdd/gdd-002-season-weather.md` |
| `docs/02_design/gdd/GDD_003_WORLD_MAP.md` | `design/gdd/gdd-003-world-map.md` |
| `docs/02_design/gdd/GDD_004_CITY_ECONOMY.md` | `design/gdd/gdd-004-city-economy.md` |
| `docs/02_design/gdd/GDD_005_CHARACTER.md` | `design/gdd/gdd-005-character.md` |
| `docs/02_design/gdd/GDD_006_RELATIONSHIP_FACTION.md` | `design/gdd/gdd-006-relationship-faction.md` |
| `docs/02_design/gdd/GDD_007_INTELLIGENCE_RECON.md` | `design/gdd/gdd-007-intelligence-recon.md` |
| `docs/02_design/gdd/GDD_008_WAR_COUNCIL.md` | `design/gdd/gdd-008-war-council.md` |
| `docs/02_design/gdd/GDD_009_BATTLE_PREPARATION.md` | `design/gdd/gdd-009-battle-preparation.md` |
| `docs/02_design/gdd/GDD_010_BATTLE_TACTICS_SANDBOX.md` | `design/gdd/gdd-010-battle-tactics-sandbox.md` |
| `docs/02_design/gdd/GDD_011_MORALE_FATIGUE.md` | `design/gdd/gdd-011-morale-fatigue.md` |
| `docs/02_design/gdd/GDD_012_LOGISTICS_SUPPLY.md` | `design/gdd/gdd-012-logistics-supply.md` |
| `docs/02_design/gdd/GDD_013_SAVE_LOAD.md` | `design/gdd/gdd-013-save-load.md` |
| `docs/02_design/GDD_INDEX.md` | `design/gdd/gdd-index.md` |
| `docs/03_technical/adrs/ADR_0001_ENGINE_CHOICE.md` | `docs/architecture/adr-0001-engine-choice.md` |
| `docs/03_technical/ADR_INDEX.md` | `docs/architecture/adr-index.md` |
| `docs/03_technical/ARCHITECTURE.md` | `docs/architecture/architecture.md` |
| `docs/03_technical/CONTROL_MANIFEST.md` | `docs/architecture/control-manifest.md` |
| `docs/04_production/EPICS.md` | `production/epics/epics-index.md` |
| `docs/04_production/SPRINT_01.md` | `production/sprints/sprint-01.md` |
| `docs/04_production/STORY_BACKLOG.md` | `production/story-backlog.md` |
| `docs/05_quality/BALANCE_STRATEGY.md` | `docs/balance-strategy.md` |
| `docs/05_quality/TEST_STRATEGY.md` | `docs/test-strategy.md` |
| `docs/05_quality/UX_ACCESSIBILITY_FOUNDATION.md` | `docs/ux-accessibility.md` |

**操作**：复制（不要移动——保留原件作备份）每个文件至模板目标路径，按需创建目录。
**时间估计**：30–45 分钟（批量复制 + 验证）

- [x] `design/gdd/` 已填充（13 份 GDD + game-concept + game-pillars + systems-index + gdd-index）
- [x] `design/concept/` 已填充（mvp-scope、non-goals、art-direction、concept-validation）
- [x] `docs/architecture/` 已填充（adr-0001、adr-index、architecture、control-manifest）
- [x] `production/sprints/` 已填充（sprint-01）
- [x] `production/story-backlog.md` 已就位
- [x] `docs/project-brief.md` 已就位

### 1b. 配置模板引擎（technical-preferences.md）

**问题**：`.claude/docs/technical-preferences.md` 中 Engine、Language 等所有字段仍显示
`[TO BE CONFIGURED]`。`/code-review`、`/dev-story`、`/architecture-decision` 以及各团队
技能均依赖此文件进行专家路由。字段未填写则所有路由失效。

**修复**：运行 `/setup-engine`。ADR-0001 已决定使用 Unity + C#，本次会话只需确认
已有决策并指定 Unity LTS 版本，时间较短。
**时间估计**：约 15 分钟

- [x] `/setup-engine` 完成——Engine 字段读取为 `Unity`，Language 字段读取为 `C#`

### 1c. 为 ADR-0001 添加 `## Status` 节

**问题**：ADR_0001 在文件头部有 `- 状态：Accepted`，但无 `## Status` 标题节。
模板的 `/story-readiness` 检查每份 ADR 是否包含 `## Status` 节；缺失时，
所有 ADR 的状态检查均会静默通过，使门禁形同虚设。

**修复**：将 ADR-0001 迁移至 `docs/architecture/adr-0001-engine-choice.md` 后，运行：
`/architecture-decision retrofit docs/architecture/adr-0001-engine-choice.md`
**时间估计**：5 分钟

- [x] `docs/architecture/adr-0001-engine-choice.md` 包含 `## Status` 节，值为 `Accepted`

---

## 第二步：修复高优先级差距（HIGH）

### 2a. 移除 GDD 节标题中的编号前缀（全部 13 份）

**问题**：所有 GDD 使用 `## 1. System Purpose`、`## 2. Player Fantasy` 等格式。
模板技能扫描 `## Player Fantasy`、`## Edge Cases` 等纯文本标题；带编号前缀的
`## 2. Player Fantasy` 不匹配 `## Player Fantasy` 的精确匹配模式。技能可能将
实际存在且有内容的节报告为"缺失"。

**修复**：在每份已迁移 GDD 中，移除所有 18 个节标题的 `N. ` 编号前缀。
示例：`## 2. Player Fantasy` → `## Player Fantasy`。内容不变。
先完成第 1a 步迁移，再编辑模板内的副本——原件保持不变。
**时间估计**：约 30 分钟（可用脚本批量处理）

- [x] 全部 13 份 GDD：节标题编号前缀已移除

### 2b. 为全部 13 份 GDD 添加 `## Formulas` 节

**问题**：模板要求每份 GDD 包含 `## Formulas` 节。13 份 GDD 均未包含此节。
检查公式的技能将始终将这些 GDD 标记为不完整，即使相关系统本无独立公式，
也会阻断 `/gate-check`。

**修复**：迁移后为每份 GDD 添加 `## Formulas` 节。
无独立公式的系统（如 GDD_013_SAVE_LOAD）可使用空桩节：

```markdown
## Formulas

本系统无独立公式——序列化规则由架构层的基础设施层定义
（参见 architecture.md §Infrastructure Layer）。
确定性由不可变快照保障，非计算公式。
```

有实际公式的系统（时间推进、士气衰减、补给消耗等），在此节完整记录公式。
**时间估计**：约 1 个会话（13 份 GDD；部分需实际内容，部分为空桩）

- [x] 全部 13 份 GDD 包含 `## Formulas` 节

### 2c. 为 ADR-0001 添加缺失节

**问题**：ADR-0001 缺少 `## ADR Dependencies`（高优先级——`/architecture-review`
的依赖排序失效）和 `## Engine Compatibility`（高优先级——后截止日期 API 风险评估
处于盲区）。

**修复**：完成第 1c 步后，在 ADR-0001 中添加：

```markdown
## ADR Dependencies
无——本 ADR 为根技术决策。

## Engine Compatibility
- 引擎：Unity（具体 LTS 版本在 ADR-0002 中锁定）
- 语言：C#（.NET Standard 2.1 / .NET 6+）
- 后截止日期风险：2023 LTS 之后的 Unity LTS 版本可能引入 API 变更。
  请交叉参考 `docs/engine-reference/unity/VERSION.md` 了解已知破坏性变更。
```

**时间估计**：10 分钟

- [x] ADR-0001 包含 `## ADR Dependencies`
- [x] ADR-0001 包含 `## Engine Compatibility`

### 2d. 初始化 TR 注册表

**问题**：`docs/architecture/tr-registry.yaml` 不存在。`/story-readiness` 和
`/architecture-review` 依赖稳定的 TR-ID（技术需求 ID）追踪 Story 是否实现了
已记录的需求。无注册表则无基线可核查。

**修复**：完成第 1–2c 步后运行 `/architecture-review`。该技能读取 GDD 和 ADR，
从中自动初始化 `tr-registry.yaml`。
**时间估计**：1 个会话（审查内容对 13 份 GDD 来说较为充分）

- [x] `docs/architecture/tr-registry.yaml` 已创建（30 条 TR-ID）

---

## 第三步：基础设施初始化

### 3a. 初始化 TR 注册表（创建 tr-registry.yaml）

运行 `/architecture-review`——读取已迁移的 GDD 和 ADR，初始化 TR 注册表。
此步与第 2d 步相同，完成一次即可同时满足两项需求。
**时间估计**：1 个会话

- [x] `docs/architecture/tr-registry.yaml` 已创建（30 条 TR-ID）

### 3b. 为控制清单添加 Manifest Version 版本戳

迁移后的 `docs/architecture/control-manifest.md` 需要在文件头部添加
`Manifest Version:` 字段，供陈旧性检查使用。

**修复**：在迁移后的 control-manifest.md 顶部添加：
```
Manifest Version: 1
```

**时间估计**：2 分钟

- [x] `docs/architecture/control-manifest.md` 包含 `Manifest Version:` 字段

### 3c. 创建 Sprint 追踪文件

迁移 sprint-01.md 后运行 `/sprint-plan update`。
**时间估计**：5 分钟

- [ ] `production/sprint-status.yaml` 已创建

### 3d. 验证权威项目阶段

完成第 1–3b 步后运行 `/gate-check Technical Setup`。
**时间估计**：15–30 分钟（门禁检查读取全部制品）

- [ ] `production/stage.txt` 已验证（若门禁通过则推进至 Pre-Production）

---

## 第四步：中优先级差距（MEDIUM）

### 4a. 将 GDD 节名称调整为模板约定

移除编号前缀（第 2a 步）后，四个节名称与模板约定仍有差异：

| 当前名称 | 模板约定 | 影响技能 |
|---|---|---|
| `## System Purpose` | `## Overview` | `/review-all-gdds`、`/design-system` |
| `## Main Rules` | `## Detailed Rules` 或 `## Core Rules` | `/design-system`、`/create-stories` |
| `## Balancing Parameters` | `## Tuning Knobs` | `/balance-check` |
| `## Test Requirements` | `## Acceptance Criteria` | `/create-stories`、`/story-readiness` |

**建议**：优先将 `## Test Requirements` 改为 `## Acceptance Criteria`（对流水线影响最大），
其余节名保留（大多数技能使用内容感知匹配，而非精确标题匹配）。
**时间估计**：仅改 Acceptance Criteria 约 30 分钟；全部改完约 3 小时

- [ ] 全部 13 份 GDD 包含 `## Acceptance Criteria`（或确认流水线能正常识别 `## Test Requirements`）

### 4b. 将 GDD 状态字段更新为模板格式

GDD 当前使用 `- 状态：Draft`（中文，位于文件头块）。
模板技能检查 `**Status**: [值]`（英文）。

**修复**：在每份已迁移 GDD 的头块中添加 `**Status**: Draft`。
保留中文行——双语冗余无妨。
**时间估计**：每份约 5 分钟，共约 1 小时（可批量编辑）

- [x] 全部 13 份 GDD 包含 `**Status**: Draft`

### 4c. 为 ADR-0001 添加 `## GDD Requirements Addressed` 节

可追溯性：哪些 GDD 需求推动了本 ADR？

```markdown
## GDD Requirements Addressed
- 确定性模拟需求（所有 GDD §数据模型）
- 可测试 Domain 层（编码标准）
- 数据驱动配置（所有 GDD §Balancing Parameters）
```

**时间估计**：10 分钟

- [x] ADR-0001 包含 `## GDD Requirements Addressed`

### 4d. 创建 ADR_0002–ADR_0005 文件

ADR_INDEX 列出了四份已规划但尚未创建的 ADR。这些是
`/gate-check Technical Setup → Pre-Production` 的前置条件。

依据 ADR_INDEX：
- ADR_0002：架构分层——GDD 跨系统审查后创建
- ADR_0003：数据驱动配置——技术架构阶段
- ADR_0004：确定性战斗模拟——技术架构阶段
- ADR_0005：存档版本与迁移——技术架构阶段

**修复**：按 ADR_INDEX 指定顺序依次运行 `/architecture-decision`。
不要提前创建——在跨系统 GDD 审查完成后按序创建。
**时间估计**：每份 30–60 分钟（共 4 份 ADR，约 2–4 个会话）

- [x] ADR-0002 已创建并为 Accepted（架构分层，注册表已登记 4 项 stance）
- [x] ADR-0003 已创建并为 Accepted（数据驱动配置，注册表登记 2 项 stance）
- [x] ADR-0004 已创建并为 Accepted（确定性战斗模拟，注册表登记 3 项 stance）
- [x] ADR-0005 已创建并为 Accepted（存档版本与迁移，注册表登记 3 项 stance）

### 4e. 验证 systems-index.md 表格格式

SYSTEM_MAP.md（迁移至 `design/gdd/systems-index.md`）的 Status 列值
只能为以下之一：`Not Started`、`In Progress`、`In Review`、`Designed`、
`Approved`、`Needs Revision`。Status 单元格不能含括号注释。
迁移后验证。
**时间估计**：10 分钟

- [x] `design/gdd/systems-index.md` Status 列干净（无括号值——该文件为概念地图，逐 GDD 状态追踪在 gdd-index.md）
- [x] 附带修复 `gdd-index.md` 的 13 个断链（旧路径 → 迁移后 kebab-case 路径）

---

## 第五步：可选改进（LOW）

### 5a. 架构可追溯性文档

`docs/architecture/requirements-traceability.md` 不存在。
该文档由 `/architecture-review` 自动生成——完成第 3a 步即可覆盖此需求。

- [ ] 由第 3a 步覆盖

### 5b. Story 详细拆解（细化前的 Backlog 属正常状态）

21 份 Story 以 Backlog 表格形式存在。对"Not Ready"的 Story，这是正确状态。
当某 Story 达到"Ready"状态时，运行 `/dev-story [STORY_ID]` 将其拆解为包含
验收标准、ADR 引用和 TR-ID 的完整 Story 文件。不要提前拆解 Backlog 中的 Story。

---

## 关于现有 Story 的说明

现有 Story 与所有模板技能均可正常配合。新格式检查（TR-ID 校验、
Manifest 版本陈旧性检查）在字段缺失时自动通过——不会破坏现有流程。
在通过 `/dev-story` 细化之前，这些 Story 不会受益于陈旧性追踪。
**不要重新生成正在进行中或已完成的 Story。**

---

## 重新运行

完成第三步后运行 `/adopt`，确认所有阻断性和高优先级差距已修复。
新运行将反映项目的当前状态。
