# Epic: City Governance Loop（城市治理循环 / M03）

> **Layer**: Feature（含 Assembly 装配——城市治理 Domain 接入会话）
> **GDD**: `design/gdd/gdd-004-city-economy.md`（primary）+ gdd-012/005/006
> **Architecture Module**: M03 City Governance Loop（`production/full-game-loop-module-plan-2026-06-28.md` §M03）
> **Governing ADR**: ADR-0009（CampaignSession 装配）· ADR-0008（城池控制权契约）· ADR-0003（数据驱动配置）· ADR-0004（确定性）
> **Status**: Ready（2026-06-29）
> **Stories**: Not yet created — run `/create-stories epic-016-city-governance-loop`

## Overview

让城市经营成为**战争与生涯的物质/政治基础**，而非独立经营小游戏。M00（epic-013）已建会话脊梁、M02（epic-015）已完成开局守城循环；本 epic 把**已实装但尚未接入新脊梁**的城市经济 Domain 内核（`CityEconomyState`/`CityDaySettlementService`/`CitySettlementConfig`，epic-004 完成）接入 `CampaignSession`：把城市日界结算叠入 `Advance` 的日界顺序、新增玩家治理命令（征用军粮/修工事/安抚/征募）经会话命令路径、让治理选择**改变后续战役条件**（守城强度/补给/民心风险）且有可解释代价。玩家为"守住今天 + 让城市明天仍能活下去"做有后果的取舍——这是把单场守城升级为可持续治理人生的关键一环。

## Boundary（与 M00/M02 的边界）

- **已交付**：M00 会话脊梁（时间/世界/生涯/城权/后果/存档）；M02 开局守城胜败两支续局；城市经济 Domain 内核（epic-004，已测但仅接入旧竖切 `SliceScenario`）。
- **M03（本 epic）新增**：
  1. **生产代码**——`CampaignSession` 持有城市治理态；`Advance` 日界顺序叠入城市日结（004，源码已预留"M03 接入"注释）；`CampaignSessionService` 新增治理命令入口。
  2. 治理动作经会话命令路径（非法命令稳定错误码、无部分写入）。
  3. ≥3 条治理选择改变战役条件且有可解释代价。
  4. 治理态存读档 + 日界确定性。
- 复用既有 Domain 规则（epic-004 城市/后勤），**不新增平衡公式**——所有数值来自版本化配置（ADR-0003）。

## 关键护栏（风险）

> module-plan §M03 风险：**变成三国志式全量城市经营**。
- 必须保留"喂给战争/生涯"的**筛选尺子**：每个治理动作须能解释它如何改变战役/守城/生涯条件；无此关联的经营深度不入 MVP。
- **MVP 先限太守单城**（§6 切分表）——不做多城调度、税收、产业链、市场、人口职业模拟（GDD_004 §MVP Scope）。

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0009: CampaignSession 装配边界 | 装配层只编排不拥规则；治理命令经会话命令路径，不在服务内算 FixedPoint 公式、不直接写城权/势力存续、不引用 *Service 内部 | LOW |
| ADR-0008: 城池控制权所有权契约 | 城归属只读经 OwnerOf；夺城/失城经 GDD_004 唯一权威；治理只读归属、不独立写 | LOW |
| ADR-0003: 数据驱动配置 | 所有治理平衡值（产入/消耗/征用民心代价/修复成本/工事贡献等）来自版本化配置 + 配置指纹；无方法体内硬编码 | LOW |
| ADR-0004: 确定性 | 整数/定点 + 注入随机流 + 状态哈希；城市日结同种子→同结果，纳入会话哈希 | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-city-001 | 民用与军用粮食来自同一权威库存；拨给军队后转后勤持有，不重复计算（守恒） | ADR-0004/0009 ✅ |
| TR-city-002 | 日界按稳定顺序结算（承诺→产入→消耗→短缺后果→工事/治安）；资源不低于合法下限 | ADR-0004/0009 ✅ |
| TR-city-003 | 玩家治理动作（征用/修工事/安抚/征募）经会话命令路径并接入日界；非法命令稳定错误码、无部分写入 | ADR-0009/0003 ✅ |
| TR-city-004 | 治理选择改变战役条件：≥3 条治理动作对后续战役/守城产生可解释代价的差异化影响（喂给战争的筛选尺子） | ADR-0003/0004 ✅ |
| TR-city-005 | 治理态存档 round-trip 一致 + 日界结算同种子→同结果（确定性，纳入会话哈希） | ADR-0005/0004 ✅ |

> 注：TR-city-003/004/005 于 2026-06-29 补登（M03 治理接入会话）。架构模式全由 Accepted ADR 覆盖；无 untraced requirement。后勤跨城路线（TR-supply-*）属 M03 边界外，按 GDD_012 §MVP 仅保留单城军粮转移。

## Scope

### In Scope
- `CampaignSession` 持有城市治理态（库存/民心/治安/工事/守备/征募池）。
- `Advance` 日界顺序叠入城市日结（CityDaySettlementService / 004），按 systems-index 破环顺序。
- `CampaignSessionService` 新增治理命令：征用军粮、修工事、安抚、征募（经命令路径，非法稳定错误码）。
- ≥3 条治理选择改变战役条件且有可解释代价（修工事↑守城强度 / 征用↑补给↓民心 / 安抚↑民心 等）。
- 治理态存读档 round-trip + 日界确定性（纳入会话哈希）。

### Out of Scope
- 多城调度 / 全量城市经营（税收、产业链、市场、人口职业）— GDD_004 §MVP 明确不做。
- 后勤跨城补给路线截断（M03 边界外；GDD_012 完整路线属后续）。
- 情报网络治理（→ M04 / epic-017）；战前准备装配（→ M05 / epic-018）。
- 晋升梯队 / 自立（→ M07/M10 / epic-022）；君主任务全量（→ M10 / epic-024）。
- 敌方战略 AI（→ epic-021）；新 Unity scene / 新 UI。

## Definition of Done

This epic is complete when:
- 城市治理态接入 `CampaignSession`，`Advance` 日界结算城市（守恒 + 稳定顺序）。
- 玩家治理命令经会话命令路径；非法命令返回稳定错误码且无部分写入。
- ≥3 条治理选择改变战役条件且有可解释代价（测试证据，module-plan §M03 验收）。
- 治理态存读档 round-trip 一致 + 日界同种子同哈希。
- 城权得失仍经 GDD_004 唯一权威（ADR-0008）；治理只读归属。
- All Logic/Integration stories 有通过的测试文件于 `tests/`；既有 M00/M01/M02 + 竖切回归全绿。

## Next Step

Run `/create-stories epic-016-city-governance-loop` 把本 epic 拆成可实现 stories，再逐 story `/story-readiness` → `/dev-story` → `/code-review` → `/story-done`。
