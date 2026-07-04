# Story 001: 君主授权出征入口

> **Epic**: 出征攻城循环（epic-029-offensive-campaign-loop）
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: M（~4h）[待 sprint 规划确认]
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-019-offensive-campaign.md`
**Requirement**: `TR-offensive-001`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准——审查时读最新)*

**ADR Governing Implementation**: ADR-0009（CampaignSession 装配边界）
**Secondary ADRs**: ADR-0008（城池控制权唯一权威——目标合法性只读控制权）
**ADR Decision Summary**: Application 层是装配脊梁，只编排不拥规则；出征授权入口作为会话命令路径提交，授权/目标判定委派 Domain 规则；城池控制权读取只经 GDD_004 唯一权威。

**Engine**: Unity 6.3 LTS + 纯 C# Domain | **Risk**: LOW
**Engine Notes**: 纯 Domain/Application 逻辑，无引擎面；NUnit + dotnet test 旁路 Unity 许可。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只由 Domain 经 Application Command 路径修改；所有平衡值数据驱动；每个 public method 实现前有 method spec 和测试要求。
- Forbidden: Domain 依赖 UnityEngine；把兵法/出征做成无条件按钮；UI 直接改状态；硬编码数值。
- Guardrail: 失败/非法命令返回稳定错误码且无部分写入（原子性）。

---

## Acceptance Criteria

*源自 GDD_019 §12，scoped 到本 story：*

- [ ] **AC-1 授权门**：出征须君主授权（GDD_014 君主任务"出征授权"子类型）；未授权 / 越权（攻打非授权目标、越战区）→ 拒绝、返回稳定错误码、无出征、无部分写入。
- [ ] **AC-1b 授权随官阶**：授权目标范围 / 战区 / 额度随官阶（Rank）正确放宽（数据驱动，低阶就近、高阶独领战区）。
- [ ] **AC-2 目标合法**：目标须为世界模型登记的敌方控制城（经 GDD_004 控制权 / GDD_015 投影只读判定）；己方城、盟友城、不可达城 → 拒绝、稳定错误码。
- [ ] **AC-7 反全知（目标选择侧）**：出征前对目标敌城的了解只经玩家情报知识投影（GDD_007 FactionKnowledge）；授权/目标选择路径类型层取不到敌方真值（MapTruth/WorldTruth）。

---

## Implementation Notes

*源自 ADR-0009 装配边界 + ADR-0008 控制权契约：*

- 新增出征授权命令（如 `RequestCampaignAuthorizationCommand` / `SubmitOffensiveTargetCommand`）走 Application 会话命令路径；Application 只做前置校验编排，授权判定与目标合法性为 Domain 纯函数。
- 授权额度/战区/官阶映射从版本化配置读取（ADR-0003），**不硬编码**。授权态属 GDD_014 生涯层（君主任务子类型），本 story 通过读 CareerState.Rank 判定额度。
- 目标合法性经 GDD_004 `ICityControlAuthority` 只读接口判定敌控城（复用 epic-004/ADR-0008 既有接口），**不**新增归属权威。
- 目标情报视图只从玩家 FactionKnowledge 投影构造（GDD_007）；构造签名不接受真值类型（编译级反全知，参照 AiWorldView 锁）。
- 非法命令（未授权/越权/非法目标）返回稳定 `OffensiveErrorCode`，原子——失败即原态不变。

---

## Out of Scope

*由相邻 story 处理，勿在此实现：*

- Story 002：攻城战本身的进攻侧编排（战斗解析）。
- Story 003：准备态→开战初始条件的闭合因果映射。
- Story 004：占城后归属结算。
- Story 005：出征回报/失败续局/存档。

---

## QA Test Cases

*lean 模式 inline 编写。开发者据此实现，勿在实现期另造用例。*

- **AC-1 授权门**
  - Given: 玩家未获君主出征授权（或授权不含目标城 X）
  - When: 提交对城 X 的出征命令
  - Then: 返回稳定错误码（如 `NotAuthorized` / `TargetOutOfAuthorizedScope`）；无出征态写入；命令前后会话状态哈希不变
  - Edge cases: 授权存在但已过期/被撤销；越战区目标；空授权
- **AC-1b 授权随官阶**
  - Given: 两个不同 Rank 的 CareerState + 同一套授权额度配置
  - When: 分别查询可授权目标范围/额度
  - Then: 高阶 Rank 的目标范围/战区/额度 ⊇ 低阶（按配置单调放宽）；额度值全部来自配置、无硬编码
  - Edge cases: 最低阶（仅就近单一目标）；最高阶（独领战区）；配置边界值
- **AC-2 目标合法**
  - Given: 世界模型中城 A=敌控、城 B=玩家控、城 C=盟友控、城 D=不可达
  - When: 分别以 A/B/C/D 为目标提交出征
  - Then: 仅 A 通过合法性；B/C/D 各返回对应稳定错误码、无部分写入
  - Edge cases: 目标城归属在提交前刚经控制权变更事件易主（须读最新 GDD_004 权威）
- **AC-7 反全知（目标选择）**
  - Given: 目标敌城真实强度与玩家情报投影不一致
  - When: 构造/查询出征目标情报视图
  - Then: 视图只含玩家 FactionKnowledge 投影值（估计+来源+时效）；类型层无法从该路径取到敌方真值（反射断言构造签名不接受真值类型）
  - Edge cases: 玩家对目标零情报（视图为未知，不泄露真值）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Offensive/authorize_campaign_entry_test.cs`（沿项目统一测试工程惯例）——集成测试须存在且通过；确定性/反全知/原子性专项断言。
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None（epic 内首 story）。跨 epic 前置：CampaignSession 装配（ADR-0009/epic-013）、GDD_004 `ICityControlAuthority`（epic-004/ADR-0008，已落地）、GDD_007 情报投影、GDD_014 CareerState.Rank（epic-011/022，已落地/进行中）须可用。
- Unlocks: Story 002。
