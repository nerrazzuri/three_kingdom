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
> 敌方 AI（gdd-016，ADR-0006）已建 epic-021（M08）并完成——见下方 M08 行。

## Assembly 层（装配 — 从内核到可玩游戏）

| Epic | Layer | System | GDD/ADR | Stories | Status |
|------|-------|--------|---------|---------|--------|
| [CampaignSession 完整会话装配](epic-013-campaign-session-assembly/EPIC.md) | Assembly（M00） | 会话脊梁 | ADR-0009 + systems-index + TR-session-* | 6 stories | ✅ Complete |
| [场景 / 战役配置目录](epic-014-scenario-catalog/EPIC.md) | Assembly（M01） | 场景目录/数据驱动开局 | ADR-0003/0009 + TR-session-003/city-001 | 2 stories | ✅ Complete |
| [太守开局循环](epic-015-opening-governor-loop/EPIC.md) | Assembly（M02） | 开局守城→胜败后果→续局 | ADR-0009/0008/0005/0004 + TR-session-004/005 + TR-career-* | 4 stories | ✅ Complete |
| [城市治理循环](epic-016-city-governance-loop/EPIC.md) | Feature（M03） | 城市治理接入会话→喂给战争/生涯 | ADR-0009/0008/0003/0004 + TR-city-001~005 | 4 stories | ✅ Complete |
| [情报与军议循环](epic-017-intel-council-loop/EPIC.md) | Feature（M04） | 情报/军议接入会话→不完全信息判断 | ADR-0009/0004/0005 + TR-intel-001~003/council-001~002 | 4 stories | ✅ Complete |
| [战役准备循环](epic-018-war-preparation-loop/EPIC.md) | Feature（M05） | 战前准备接入会话→可执行战役初始条件 | ADR-0009/0004/0005/0003 + TR-prep-001/002 | 4 stories | ✅ Complete |
| [兵法沙盒战役循环](epic-019-tactical-battle-loop/EPIC.md) | Feature（M06） | 战斗/兵法接入会话→可打可复现战役 | ADR-0004/0009/0005 + TR-battle-001/002/003 | 4 stories | ✅ Complete |
| [后果与恢复循环](epic-020-consequence-recovery-loop/EPIC.md) | Feature（M07） | 战果写回完整世界→胜败都有后续 | ADR-0004/0009/0008/0005 + TR-outcome-001/002 | 4 stories | ✅ Complete |
| [敌方 AI 循环](epic-021-enemy-ai-loop/EPIC.md) | Core+Feature（M08） | 敌方 AI Domain 内核（战术 80%）→可读可骗可复现对手 | ADR-0006/0004/0009 + TR-ai-001~004 | 4 stories | ✅ Complete |
| [生涯与权限循环](epic-022-career-authority-loop/EPIC.md) | Feature（M09） | 生涯/权限接入会话→功绩/晋升/自立 | ADR-0004/0009/0008/0005 + TR-career-001~005 | 4 stories | ✅ Complete |
| [历史世界与势力循环](epic-023-historical-world-faction-loop/EPIC.md) | Feature（M10） | 历史世界接入会话→够不着继续/够得着分叉 | ADR-0007/0008/0004/0005 + TR-world-001~006 | 4 stories | ✅ Complete |
| [表现与理解循环](epic-028-presentation-ux-feedback/EPIC.md) | Presentation（M15） | 让 11 循环可理解：因果/风险无胜率/情报置信/失败可继续 | ADR-0002/0009/0004 + TR-ux-001~005（registry v4） | 5 stories（001 接缝 → 002 战果复盘 → 003∥004 → 005） | 🟢 Ready |

> epic-013 = 完整游戏循环模块规划的 **M00 脊梁**（`production/full-game-loop-module-plan-2026-06-28.md`）；epic-014 = **M01 场景目录**（CON-5 收尾）；epic-015 = **M02 太守开局循环**（开局守城→胜败两支续局+存读档）。ADR-0009 Accepted（2026-06-28，经子代理复审）。后续装配 epic（M03~M16，epic-016~028）见模块规划 §6 切分表。

## 后续层（未展开）

- **Feature（其余）**：完整 8 阶梯队、全时间线事件网络等见各 GDD §Future Scope，MVP 不含。

## 统计

- 24 epics（3 Foundation + 6 Core + 1 Presentation 竖切 + 2 Feature Meta + 3 Assembly + 7 Feature M03~M10 + 1 Core+Feature M08 + 1 Presentation M15）；**23 ✅ Complete + 1 🟢 Ready（epic-028 表现与理解循环 / M15，2026-07-03 §7 五问全裁定转 Ready）**。
- 88 stories ✅ Complete（含 epic-023 历史世界 4）；本地回归 **797/797 全绿，`-warnaserror` 0 warning**。
- **M10 历史世界达成**（epic-023）：历史事件按时间窗触发（够不着前置短路恒成立）+ 玩家触及分叉 + 下游传播 + 历史态存读档同序列同走向。
- **17 模块进度：M00~M10 完成（11/17）**。
- **M09 生涯权限达成**（epic-022）：功绩累积（非战斗源竞争力）+ 晋升申请（门槛/稳定错误码）+ 自立三分支反叛（转新势力/在野）+ 生涯态存读档确定性。
- **M08 敌方 AI 达成**（epic-021，从零 Domain 内核）：AiWorldView 反全知锁（结构级）+ 效用评分硬可行性门 + 种子 softmax（可复现 + 温度单调）+ DecisionRecord 错误信念可读 + 接入战区命令同源确定性。
- **M07 后果恢复达成**（epic-020）：战果四分支后果原子写回会话城市态 + 胜败撤退失城都有续局（败局必非空，失败不切死局）+ 原子回滚 + 后果续局态存读档确定性。
- **M06 兵法沙盒达成**（epic-019）：战斗态接入会话 + 从 CommittedPlan 开战 + 阶段解析（稳定管线+原子回滚）+ 兵法事后识别（FeintAmbush 机动招式，CD 硬退出门）+ 战斗态存读档确定性。
- **M05 战役准备达成**（epic-018）：准备态接入会话 + 草稿编辑 + 合法计划原子提交（资源锁定）+ 冲突 DAG 拒绝（无部分写入）+ 准备态存读档确定性。
- **M04 情报军议达成**（epic-017）：情报态接入会话（真值/知识四层分离反全知）+ 侦察命令 + 军议快照确定+知识变化建议过时 + 情报态存读档不交叉污染。
- **M03 城市治理达成**（epic-016）：城市治理态接入会话 + Advance 日界结算 + 治理命令（征用/修工事/安抚）+ 治理→战役条件派生 + 存读档确定性。
- **M00 脊梁达成**（epic-013）：开局→推进→战果→后果原子写回(004/015/014)→存档 round-trip→续推 端到端贯通、确定性、失败可继续。
- **M01 场景目录达成**（epic-014）：多场景按 id 开局 + 数据驱动 SliceScenarioData（CON-5 收尾）。
- **M02 太守开局循环达成**（epic-015）：开局守城胜/败两支均可继续(Advance)+存读档+部曲保留+城权经004易主+两支确定性哈希。
- 全部 governing ADR 为 Accepted（ADR-0001~0009）；无 Blocked story。
- GDD_016 敌方 AI（ADR-0006）已落地：epic-021（M08）4/4 stories Complete，战术层 80%（OpponentModel/StrategicPlan/LLM 留后续）。

## 下一步

★ **M00~M10 完成（2026-06-30，11/17 模块）**——完整循环 + 敌方 AI + 生涯权限 + 历史世界贯通。

**剩余模块 + 2026-07-04 用户实玩后新增循环（"让游戏不无聊"方向）进入"需补设计"区**：
- 🟢 **出征攻城循环**（epic-029，**Ready for Stories**，5 stories）：2026-07-04 用户裁定补入——君主授权主动出征 + 闭合因果（准备决定胜负）+ 占城归属方案 C（前2默认归玩家，后续君主种子化随机取舍→喂自立张力）+ 升官联动。**最高优先**。GDD_019 Reviewed · ADR-0010 Accepted · 焦点跨系统审查 5 Warning 全闭 · 5 story 已拆（TR-offensive-001..005，0 Blocked）。下一步 /story-readiness → /dev-story。
- 🟡 **人才招揽循环**（epic-030，**Draft**）：2026-07-04 用户裁定——出现随历史（GDD_015）/ 知晓靠情报（反全知）/ 入伙靠条件+种子化随机（ADR-0006）。
- 🟡 **M12 多城战区**（epic-025，**Draft**）：占城 C 使玩家真直辖多城 → 委任下属打理、玩家只碰关键决策、仍不全知。依赖 epic-029。
- 🟡 **M11 外交战略约束**（epic-024）：需补设计（新外交 GDD 或 GDD_012 扩展）——协作设计 GDD/ADR（user-driven）。
- 🔴 **M13 君主争霸**（epic-026）：硬阻塞，GDD_017 不存在，须先建 GDD+ADR（CLAUDE.md 红线）。
- 🔴 **M14 统一终局**（epic-027）：硬阻塞，GDD_018 不存在。
- 🟣 **M15 表现/UX**（epic-028 ✅ Complete）/ **M16 内容平衡发布**：Unity 表现层 + 内容。

> **epic-029/030/025 = 2026-07-04 用户设计对话产物**，Draft 状态：决策已嵌各 EPIC.md，story 已拆分；**实现前需补 GDD/ADR 锚点**（见各 EPIC "实现前置"）。推荐序：epic-029（出征+闭合因果）→ 025（多城委任）→ 030（人才）。

**整合验证可选**：`/team-qa` 全量回归 / 端到端竖切重验。

**敌方 AI 后续**：OpponentModel 记忆 + StrategicPlan 战略 + LLM 装饰（ADR-0006 已留接口；M08 仅战术 80%）。

**特性裁定输入**：`docs/reviews/rotk-feature-gap-verdict-2026-06-30.md`（三国志功能缺口裁定，待团队评审）——M11/M12 design-first；M13/M14 须走正式 roadmap 修订；M09 增量为设计债不重开。
