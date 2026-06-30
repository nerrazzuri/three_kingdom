# Epics Index

Last Updated: 2026-06-28（epic-015 创建）
Engine: Unity 6.3 LTS + C#
Manifest Version: 2 (2026-06-28)

> 本文件由 `/create-epics` + `/create-stories` 生成，为可执行 epic/story 总账。
> 概念级规划表见 `epics-index.md`（EPIC_001–010 范围说明）；story 草稿溯源见 `../story-backlog.md`。

## Foundation 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [项目与 Domain 基础](epic-001-domain-foundation/EPIC.md) | Foundation | 工程底座 | 横切 | 4 stories | ✅ Complete |
| [世界基底（时间·环境·地图）](epic-002-world-substrate/EPIC.md) | Foundation | 时间/天气/地图 | gdd-001/002/003 | 5 stories | ✅ Complete |
| [存档与复现](epic-009-save-replay/EPIC.md) | Foundation | 存档 | gdd-013 | 3 stories | ✅ Complete |

## Core 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [人物与关系](epic-003-character-relationship/EPIC.md) | Core | 人物/关系 | gdd-005/006 | 3 stories | ✅ Complete |
| [城市与后勤](epic-004-city-logistics/EPIC.md) | Core | 城市/后勤/外交入口 | gdd-004/012 | 3 stories | ✅ Complete |
| [情报与军议](epic-005-intel-council/EPIC.md) | Core | 情报/军议 | gdd-007/008 | 3 stories | ✅ Complete |
| [战前准备](epic-006-battle-preparation/EPIC.md) | Core | 战前准备 | gdd-009 | 2 stories | ✅ Complete |
| [兵法沙盒结算](epic-007-tactics-sandbox/EPIC.md) | Core | 战斗/士气 | gdd-010/011 | 3 stories | ✅ Complete |
| [后果与可玩失败](epic-008-outcome-consequence/EPIC.md) | Core | 后果结算 | gdd-010 §后果 | 2 stories | ✅ Complete |

## Presentation 层

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [Slice UX 与可访问性](epic-010-slice-ux/EPIC.md) | Presentation | Slice UX/无障碍 | design/ux/* + accessibility | 5 stories | ✅ Complete |

## Feature 层（Meta 连接层 — 大规划）

| Epic | Layer | System | GDD | Stories | Status |
|------|-------|--------|-----|---------|--------|
| [战役与生涯](epic-011-campaign-career/EPIC.md) | Feature | 生涯/晋升/自立 | gdd-014 | 5 stories | ✅ Complete |
| [条件历史世界模型](epic-012-historical-world-model/EPIC.md) | Feature | 世界骨架/历史推进 | gdd-015 | 6 stories | ✅ Complete |

> 这两个 Meta epic 把单场战役连接成可持续人生（014）+ 提供历史世界骨架（015）。
> **Sprint 02（2026-06-28）完成全部 11 story（Must 5 + Should 3 + Nice 3）**；当时 smoke/team-qa 基线为 556/556 绿；当前本地回归验证为 564/564 绿；team-qa APPROVED。
> 014/015 governing ADR-0007/0008 + 0002~0005 全 Accepted；无 untraced 需求。
> 敌方 AI（gdd-016，ADR-0006）仍 Reviewed，未建 epic。

## Assembly 层（装配 — 从内核到可玩游戏）

| Epic | Layer | System | GDD/ADR | Stories | Status |
|------|-------|--------|---------|---------|--------|
| [CampaignSession 完整会话装配](epic-013-campaign-session-assembly/EPIC.md) | Assembly（M00） | 会话脊梁 | ADR-0009 + systems-index + TR-session-* | 6 stories | ✅ Complete |
| [场景 / 战役配置目录](epic-014-scenario-catalog/EPIC.md) | Assembly（M01） | 场景目录/数据驱动开局 | ADR-0003/0009 + TR-session-003/city-001 | 2 stories | ✅ Complete |
| [太守开局循环](epic-015-opening-governor-loop/EPIC.md) | Assembly（M02） | 开局守城→胜败后果→续局 | ADR-0009/0008/0005/0004 + TR-session-004/005 + TR-career-* | 4 stories | ✅ Complete |
| [城市治理循环](epic-016-city-governance-loop/EPIC.md) | Feature（M03） | 城市治理接入会话→喂给战争/生涯 | ADR-0009/0008/0003/0004 + TR-city-001~005 | 4 stories | ✅ Complete |
| [情报与军议循环](epic-017-intel-council-loop/EPIC.md) | Feature（M04） | 情报/军议接入会话→不完全信息判断 | ADR-0009/0004/0005 + TR-intel-001~003/council-001~002 | 4 stories | ✅ Complete |
| [战役准备循环](epic-018-war-preparation-loop/EPIC.md) | Feature（M05） | 战前准备接入会话→可执行战役初始条件 | ADR-0009/0004/0005/0003 + TR-prep-001/002 | 4 stories | ✅ Complete |

> epic-013 = 完整游戏循环模块规划的 **M00 脊梁**（`production/full-game-loop-module-plan-2026-06-28.md`）；epic-014 = **M01 场景目录**（CON-5 收尾）；epic-015 = **M02 太守开局循环**（开局守城→胜败两支续局+存读档）。ADR-0009 Accepted（2026-06-28，经子代理复审）。后续装配 epic（M03~M16，epic-016~028）见模块规划 §6 切分表。

## 后续层（未展开）

- **Feature（其余）**：完整 8 阶梯队、全时间线事件网络等见各 GDD §Future Scope，MVP 不含。

## 统计

- 18 epics（3 Foundation + 6 Core + 1 Presentation + 2 Feature Meta + 3 Assembly + 1 Feature M03 + 1 Feature M04 + 1 Feature M05）；18 ✅ Complete。
- 68 stories ✅ Complete（含 epic-018 战役准备 4）；本地回归 **703/703 全绿，`-warnaserror` 0 warning**。
- **M05 战役准备达成**（epic-018）：准备态接入会话 + 草稿编辑 + 合法计划原子提交（资源锁定）+ 冲突 DAG 拒绝（无部分写入）+ 准备态存读档确定性。
- **M04 情报军议达成**（epic-017）：情报态接入会话（真值/知识四层分离反全知）+ 侦察命令 + 军议快照确定+知识变化建议过时 + 情报态存读档不交叉污染。
- **M03 城市治理达成**（epic-016）：城市治理态接入会话 + Advance 日界结算 + 治理命令（征用/修工事/安抚）+ 治理→战役条件派生 + 存读档确定性。
- **M00 脊梁达成**（epic-013）：开局→推进→战果→后果原子写回(004/015/014)→存档 round-trip→续推 端到端贯通、确定性、失败可继续。
- **M01 场景目录达成**（epic-014）：多场景按 id 开局 + 数据驱动 SliceScenarioData（CON-5 收尾）。
- **M02 太守开局循环达成**（epic-015）：开局守城胜/败两支均可继续(Advance)+存读档+部曲保留+城权经004易主+两支确定性哈希。
- 全部 governing ADR 为 Accepted（ADR-0001~0009）；无 Blocked story。
- GDD_016 敌方 AI 仍为 Reviewed：有 GDD + ADR-0006，尚无 epic/story/实现（epic-021 待建）。

## 下一步

★ **M00~M05 装配完成（2026-06-30）**，进入 **M06 兵法沙盒战斗装配**（epic-019，待建）。

**当前动作**：`/create-epics epic-019`（M06 兵法沙盒战斗装配，消费 M05 CommittedPlan 作战役初始条件 + M03 城市派生战役条件）→ `/create-stories` → 逐 story `/dev-story`。注意 CD 硬退出门：M06 宣称"兵法沙盒MVP完成"前必接 ≥1 机动招式（假退伏击/火攻）。

**后续装配序**（module-plan §6）：M03 城市治理（epic-016）→ M04 情报军议装配（epic-017）→ M05 战前准备装配（epic-018）→ M08 敌方 AI 战术层（epic-021，可与 M03 并行）。

**并行裁决项**：GDD_016 敌方 AI（ADR-0006）是否进入装配期；若进入应先建 epic-021 并修 GDD_016 缺口（CON-3 已修），否则在控制清单显式标注 MVP 不含敌方 AI。
