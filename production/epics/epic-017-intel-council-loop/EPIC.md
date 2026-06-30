# Epic: Intelligence / War Council Loop（情报与军议循环 / M04）

> **Layer**: Feature（含 Assembly 装配——情报/军议 Domain 接入会话）
> **GDD**: `design/gdd/gdd-007-intelligence-recon.md` + `design/gdd/gdd-008-war-council.md`
> **Architecture Module**: M04 Intelligence / War Council Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M04）
> **Governing ADR**: ADR-0009（CampaignSession 装配）· ADR-0004（确定性）· ADR-0005（存档）
> **Status**: Ready（2026-06-30）
> **Stories**: Not yet created — run `/create-stories epic-017-intel-council-loop`

## Overview

让玩家在**不完全信息**下判断风险，军师只提**条件化建议**——胜利来自判断与验证，而非系统告诉最优解。M00（脊梁）/M03（城市治理）已就绪；情报/军议 Domain 内核（`IntelService`/`IntelAssessmentService`/`WarCouncilService`/`WorldTruth`/`Observation`/`IntelReport`/`FactionIntel`/`CouncilAdviceSet`，epic-005 完成）已含四层分离与置信度/时效，但**尚未接入** `CampaignSession`。本 epic 把它接入可玩会话：会话持世界真值 + 玩家阵营知识，侦察命令经会话路径产生有成本/时效/暴露的报告并更新阵营知识，军议在召开时的合法知识快照下输出确定的条件化建议，知识变化后旧建议标记过时，且情报态存读档时真值与玩家知识分别序列化、不交叉污染。

## Boundary（与 M00/M03 的边界）

- **已交付**：M00 会话脊梁（时间/世界/生涯/城权/后果/存档）；M03 城市治理接入会话；情报/军议 Domain 内核（epic-005，已测但仅接旧竖切 `SliceScenario`/`GameSession`）。
- **M04（本 epic）新增**（**含新生产代码**，同 M03）：
  1. `CampaignSession` 持有 `WorldTruth` + 玩家 `FactionKnowledge`（情报态）。
  2. 侦察命令经会话命令路径（指定对象/区域/执行者/方法/持续；"侦察全部"非法）→ 解析产生 Observation/IntelReport → 更新 FactionKnowledge。
  3. 军议查询经会话当前知识快照（条件化建议；同快照 → 同输出）。
  4. 知识/资源变化后旧军议建议标记过时（不静默更新）。
  5. 情报态存读档：真值与玩家知识分别序列化、不交叉污染 + 确定性。
- 复用既有 Domain 规则（epic-005），**不新增情报/军议公式**。

## 关键护栏（风险）

> module-plan §M04 风险：**军师建议过度接近攻略；或情报 UI 泄露真值**。
- 军师**只输出条件化建议**：不给综合成功率/胜率数字、不给唯一推荐、不自动选人/兵力/时间/地点、不自动提交计划（GDD_008 §Main Rules）。
- 情报**不泄露 `WorldTruth` 真值**：UI/玩家只读阵营知识（四层分离，TR-intel-001）；暴露由确定性随机流判定。
- "侦察全部"非法：侦察须指定对象/区域/执行者/方法/持续（TR-intel-002）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；侦察/军议经会话命令/查询路径，复用既有 Domain 服务，不在会话内重写情报/军议公式 | LOW |
| ADR-0004: 确定性 | 整数/定点 + 注入随机流 + 状态哈希；侦察暴露与军议输出在同知识快照下确定可复现 | LOW |
| ADR-0005: 存档版本与迁移 | 真值与玩家知识分别序列化、加载不交叉污染（TR-intel-003）；情报态 round-trip 一致 | LOW |

> 反全知（UI 只读阵营知识）由 GDD_007 四层分离保证（已实装）；ADR-0006 的反全知锁治敌方 AI（epic-021/M08），不挂为本 epic 治理 ADR。

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-intel-001 | 世界真值、观察、报告、阵营知识四层分离；UI 只能读取阵营知识 | ADR-0009/0004 ✅ |
| TR-intel-002 | 报告含置信度（来源可靠性，非真实概率）、时效衰减、估计区间；暴露由确定性随机流判定 | ADR-0004 ✅ |
| TR-intel-003 | 世界真值与玩家知识分别序列化，加载不得交叉污染 | ADR-0005 ✅ |
| TR-council-001 | 军议读取召开时合法知识快照；知识/资源变化后建议标记过时，不静默更新 | ADR-0009/0004 ✅ |
| TR-council-002 | 军师只输出条件化建议，不输出综合成功率、唯一推荐或自动命令 | ADR-0009 ✅ |

> 注：M04 为装配 epic，复用 epic-005 的 TR-intel-*/TR-council-*（像 epic-015 复用 session/career TR），无新 TR。无 untraced requirement。

## Scope

### In Scope
- `CampaignSession` 持有 `WorldTruth` + 玩家 `FactionKnowledge`（情报权威态）。
- 侦察命令经会话命令路径：指定对象/区域/执行者/方法/持续 → 解析（确定性随机流）→ Observation/IntelReport → 更新 FactionKnowledge；非法侦察（"侦察全部"/缺字段）稳定错误码、无部分写入。
- 军议查询经会话当前知识快照：输出条件化建议集（同快照 → 同输出）。
- 知识/资源变化后旧军议建议标记过时（不静默更新）。
- 情报态存读档：真值与玩家知识分别序列化、不交叉污染 + 同种子同哈希。

### Out of Scope
- 庞大间谍网（GDD_007 §MVP 外）；多势力玩家知识（MVP 限玩家单势力视角）。
- 敌方 AI 读 FactionKnowledge / 敌方唯一敌情来源（epic-021 / M08）。
- 完整战役准备/计划承诺装配（M05 / epic-018）。
- 关系渠道/历史公开态势的完整情报源（MVP 限侦察主源）；新 Unity scene / 新 UI / 新情报或军议公式。

## Definition of Done

This epic is complete when:
- 情报态（真值 + 玩家知识）接入 `CampaignSession`；侦察命令经会话路径产生有成本/时效/暴露的报告并更新阵营知识。
- 军师在同一知识快照下输出确定；知识变化后旧建议标记过时（module-plan §M04 测试证据）。
- 军师只输出条件化建议（无成功率/唯一推荐/自动命令）；情报不泄露真值（UI 只读阵营知识）。
- 情报态存读档真值/知识分别序列化、不交叉污染 + 同种子同哈希。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00~M03 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-017-intel-council-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
