# Story 003: 闭合因果——准备→战果

> **Epic**: 出征攻城循环（epic-029-offensive-campaign-loop）
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L（~6h）[待 sprint 规划确认]
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-04

## Context

**GDD**: `design/gdd/gdd-019-offensive-campaign.md`
**Requirement**: `TR-offensive-003`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准——审查时读最新)*

**ADR Governing Implementation**: ADR-0004（确定性战斗模拟）
**Secondary ADRs**: ADR-0009（装配边界——映射为 Domain 规则、Application 只装配）
**ADR Decision Summary**: 权威结算用整数/定点 + 注入随机流 + 状态哈希；准备态→初始条件映射为纯确定性 Domain 函数，同输入同输出。

**Engine**: Unity 6.3 LTS + 纯 C# Domain | **Risk**: LOW
**Engine Notes**: 纯 Domain 逻辑，无引擎面。

**Control Manifest Rules (this layer)**:
- Required: 战斗结果可确定性复现；平衡值数据驱动；每个 public method 有 method spec + 测试。
- Forbidden: 兵法做成无条件按钮（AC-3 的加成必须来自条件涌现，非计策开关）；Domain 权威路径 float/double；隐式随机；硬编码映射系数。
- Guardrail: 相同准备态+种子→相同胜负与状态哈希。

---

## Acceptance Criteria

*源自 GDD_019 §4 R3 / §5 F1-F2 / §12 AC-3，本 story 为核心命门：*

- [ ] **AC-3 闭合因果（crux）**：玩家的治理/备战/情报态**真正决定**攻城初始条件——
  - 备战计划（CommittedPlan：设伏/断粮/分兵）→ 满足 GDD_010 招式涌现前提（位置/隐蔽/时机）；
  - 城市/后勤态（征募的兵、转运的粮）→ 可投入兵力与续航；
  - 情报态（GDD_007 投影）→ 军师可识别招式 + 敌预设可信度。
- [ ] **AC-3b 涌现而非按钮**：兵法结算加成来自 GDD_010 §7 条件涌现结算路径；`TacticRecognizer` 只在战后只读打复盘标签，不参与结算。**无同名计策执行按钮**。
- [ ] **AC-3c 取代脚本固定胜局**：**不同准备态 → 确定性不同攻城胜负 / 兵法识别**（准备充分可以弱胜强、裸战力硬碰可能败）；全程无胜率数字。
- [ ] **AC-3d 确定性**：相同准备态 + 相同种子 → 相同派生初始条件与最终胜负（可复现）。

---

## Implementation Notes

*源自 ADR-0004 确定性 + GDD_010/009/012/007 契约：*

- 实现"准备态 → 开战初始条件"的纯 Domain 映射（Story 002 已定义初始条件输入契约，本 story 填充派生逻辑）。
- 映射系数（准备各维度 → 战力/续航/兵法前提）**全部数据驱动**（ADR-0003），method spec 明确每维度输入范围与输出范围。
- 兵法前提：CommittedPlan 各命令在开战态里满足 GDD_010 招式的涌现前提；前提齐备 → §7 条件涌现产生结算优势。**严禁**为兵法加"成功开关"或胜率。
- 差异化验收：构造"裸战 vs 备好条件"两组准备态，同种子跑，须产生可解释的不同胜负（力证脚本固定胜局被取代）。
- 权威路径整数/定点；随机只经注入流；派生+结算全纳入状态哈希。

---

## Out of Scope

*由相邻 story 处理：*

- Story 002：战斗管线本身的编排（本 story 只填初始条件派生）。
- Story 004：胜利后占城归属。
- Story 005：回报/失败续局/存档。
- 军师建议呈现层（UI）——属 M15/UX 层，非本 story。

---

## QA Test Cases

*lean 模式 inline 编写。*

- **AC-3 闭合因果**
  - Given: 同一目标城，两组不同准备态（A=修工事+断敌粮+设伏成型 / B=裸战无备）
  - When: 各自派生开战初始条件
  - Then: A 的初始条件（可投入兵力/续航/兵法前提满足数）显著优于 B；映射系数取自配置
  - Edge cases: 单维度缺失（有兵无粮 / 有情报无设伏）→ 对应前提不满足
- **AC-3b 涌现而非按钮**
  - Given: 出征结算代码 + 招式识别
  - When: 检查兵法加成来源
  - Then: 加成来自 §7 条件涌现；TacticRecognizer 仅事后打标签、不改结算；代码中无"计策成功按钮"路径（结构断言）
  - Edge cases: 前提部分满足 → 无涌现加成（不是"半成功"按钮）
- **AC-3c 取代脚本固定胜局**
  - Given: 准备态 A（充分）vs B（裸战），同种子同目标
  - When: 跑完整出征战斗
  - Then: A 与 B 产生不同胜负分支（A 可弱胜强 / B 可败）；无胜率数字出现在任何输出
  - Edge cases: 敌远强于预估（情报误）→ 即便备战也可能败（合理）
- **AC-3d 确定性**
  - Given: 固定准备态 + 固定种子
  - When: 重复派生+结算两次
  - Then: 派生初始条件与最终胜负逐位一致、状态哈希相同
  - Edge cases: 读档后再派生

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Offensive/preparation_causality_test.cs`——单元测试须存在且通过；含"不同准备→不同胜负"差异化断言 + 确定性哈希 + 无胜率断言。
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 002（战斗管线 + 初始条件输入契约）。跨 epic：GDD_009 CommittedPlan、GDD_010 §7 结算、GDD_012 补给、GDD_007 情报投影（均已落地）。
- Unlocks: Story 004。


## Completion Notes
**Completed**: 2026-07-04
**实现**: `OffensiveSetupService`（Domain）——准备态（兵/粮/兵法条件）确定性单调映射为进攻方战力，取代脚本固定胜局；经 LaunchOffensive 接入攻城结算。
**测试**: OffensiveDomainTests（准备单调+封顶+确定性）+ 端到端 test_strong...weak_loses（准备不同→胜负不同）。871/871 绿。
**Code Review**: lean inline（本会话）· ADR-0010/0008/0006/0004 COMPLIANT · 反全知/确定性/无胜率/失败可继续均合规。
