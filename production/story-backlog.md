# Story Backlog（已取代 — SUPERSEDED）

> ⚠️ **本文件为 Pre-Production 期的早期初稿,已被取代,不再维护。**
>
> 旧的扁平编号体系(`STORY_001_001`、`EPIC_010` 等)与现行 epic/story 结构已分叉。
> **当前唯一权威的 story 跟踪来源**:`production/epics/*/story-*.md`
> ——每个 story 文件内含权威 `Status`、验收标准、QA 测试用例与测试证据路径。
>
> 总账见 `production/epics/epics-index.md`;迭代计划见 `production/sprints/`。

---

## 当前进度快照（2026-06-22）

阶段:**Production — Foundation 实现**(Pre-Production→Production 门禁已过,裁定 CONCERNS)。

9 epics / 28 stories(全为 Domain/Application 层 Logic·Integration)。

| Epic | Story 数 | 已完成 | 状态 |
|---|---|---|---|
| epic-001 项目与 Domain 基础 | 4 | 2 | 🔄 S1✅ S2✅ → S3/S4 Ready |
| epic-002 世界基质 | 5 | 0 | Ready |
| epic-003 人物关系 | 3 | 0 | Ready |
| epic-004 城池后勤 | 3 | 0 | Ready |
| epic-005 情报军议 | 3 | 0 | Ready |
| epic-006 战前准备 | 2 | 0 | Ready |
| epic-007 兵法沙盒 | 3 | 0 | Ready |
| epic-008 结果后果 | 2 | 0 | Ready |
| epic-009 存档回放 | 3 | 0 | Ready |
| **合计** | **28** | **2** | — |

> 上表为概览,可能滞后。**以各 `story-*.md` 文件内的 `Status` 字段为准。**

---

## 为何取代

早期初稿(`STORY_NNN_NNN` 编号)在 Pre-Production→Production 门禁补完时被
`/create-epics` + `/create-stories` 生成的结构化 story 文件取代:新文件每个嵌入
TR-ID + governing ADR + 验收标准 + QA 测试用例 + 测试证据路径,满足
`/story-readiness` 就绪门禁所需的全部字段。旧扁平表缺这些字段,且编号无法一一对应
(旧含 Presentation 层 `EPIC_010`,新体系将表现层归入后续里程碑),故整体退役而非逐条迁移。
