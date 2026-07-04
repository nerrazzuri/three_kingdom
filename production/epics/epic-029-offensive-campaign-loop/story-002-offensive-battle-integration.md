# Story 002: 攻城战接入（进攻视角）

> **Epic**: 出征攻城循环（epic-029-offensive-campaign-loop）
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: L（~6h）[待 sprint 规划确认]
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-04

## Context

**GDD**: `design/gdd/gdd-019-offensive-campaign.md`
**Requirement**: `TR-offensive-002`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准——审查时读最新)*

**ADR Governing Implementation**: ADR-0009（CampaignSession 装配边界）
**Secondary ADRs**: ADR-0004（确定性战斗模拟）
**ADR Decision Summary**: 出征战斗复用既有备战→战斗→后果管线，反向用于攻打敌城；Application 只编排（装配脊梁）不拥规则；战斗解析确定性可复现（整数/定点 + 注入随机流 + 状态哈希）。

**Engine**: Unity 6.3 LTS + 纯 C# Domain | **Risk**: LOW
**Engine Notes**: 复用既有 Domain 战斗系统；无新引擎面。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只经 Application Command 路径修改；战斗结果可确定性复现；平衡值数据驱动。
- Forbidden: 兵法做成无条件按钮；Domain 依赖 UnityEngine；Domain 权威路径用 float/double（用整数/定点 Q16.16，ADR-0004）；隐式随机源。
- Guardrail: 相同初始快照+配置指纹+种子+有序命令流→相同事件与状态哈希。

---

## Acceptance Criteria

*源自 GDD_019 §3/§7/§12，scoped 到本 story：*

- [ ] **AC-3a 进攻侧编排**：复用 M05 备战 → M06 战斗 → M07 后果 管线，反向攻打敌城；**攻方 = 玩家投入**（兵力/补给/备战计划），**守方 = 敌方确定性预设**（由目标城守备 + 敌 AI 预设构造）。
- [ ] **AC-3b 装配边界**：出征战斗编排在 Application（只发起/订阅、组织顺序），战斗规则结算仍在 Domain（GDD_010 §7 唯一结算点）；Application 不复制/不旁路 Domain 规则。
- [ ] **AC-2/AC-3 确定性**：相同准备态 + 相同种子 + 相同命令流 → 相同事件序列与状态哈希（可复现、可重放）。
- [ ] **AC-7 反全知（战斗侧）**：敌方预设可信度基于玩家情报投影构造；玩家进攻决策路径不读敌方真值。

---

## Implementation Notes

*源自 ADR-0009 装配 + ADR-0004 确定性：*

- 复用竖切/epic 既有 BattleResolution 管线，进攻方 = 玩家 CommittedPlan 派生的投入，守方 = 目标城守备构造的敌预设。**不新写战斗结算**——只装配输入/输出方向。
- 出征战斗作为一个战役 checkpoint 纳入 CampaignSession（ADR-0009 统一存档信封的战役段）；日界/全局序遵循 systems-index 结算顺序，不私改。
- 随机消费复用注入式确定性流（ADR-0004），与战斗模拟同源；纳入同一状态哈希与重放契约。
- 敌方预设（守备兵力/性格/补给）以玩家情报投影可信度为界构造；真实值与投影不一致本身是风险（AC-7），非 bug。
- 本 story 可先用"默认/中性初始条件"打通管线；**准备态如何真正决定初始条件由 Story 003 实现**（本 story 定义初始条件输入契约，003 填充派生逻辑）。

---

## Out of Scope

*由相邻 story 处理：*

- Story 003：准备态→开战初始条件的派生映射（闭合因果 crux）。
- Story 004：占城归属结算（胜利后）。
- Story 005：回报/失败续局/存档写回。

---

## QA Test Cases

*lean 模式 inline 编写。*

- **AC-3a 进攻侧编排**
  - Given: 玩家出征投入（兵力/补给/CommittedPlan）+ 目标敌城守备预设
  - When: 运行出征战斗管线
  - Then: 攻方由玩家投入构造、守方由敌预设构造；产出合法战果（胜/败/撤退/失城分支之一）
  - Edge cases: 玩家投入为最小合法值；敌守备极强/极弱
- **AC-3b 装配边界**
  - Given: 出征战斗编排代码
  - When: 静态/结构断言其对战斗规则的调用
  - Then: 结算走 Domain GDD_010 结算点；Application 层无重复规则实现（反射/程序集边界断言）
  - Edge cases: —
- **AC-2/AC-3 确定性**
  - Given: 固定准备态 + 固定种子 + 固定命令流
  - When: 连续运行出征战斗两次（及读档后再跑）
  - Then: 两次事件序列逐位相同、终态状态哈希一致
  - Edge cases: 读档后续推进（RNG 流位置须保持）；命令流顺序敏感
- **AC-7 反全知（战斗侧）**
  - Given: 敌方真实守备 ≠ 玩家情报投影
  - When: 构造敌预设并运行
  - Then: 敌预设可信度基于投影；玩家进攻路径类型层不可达真值
  - Edge cases: 情报严重失真 → 玩家可大败（合理，非 bug）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Offensive/offensive_battle_integration_test.cs`——集成测试须存在且通过；确定性哈希+重放+装配边界断言。
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（授权+目标入口须先给出合法目标城）。跨 epic：既有 BattleResolution 管线（epic-006/007/010）、CampaignSession 战役段（ADR-0009）。
- Unlocks: Story 003。


## Completion Notes
**Completed**: 2026-07-04
**实现**: `SiegeResolutionService`（Domain，攻方战力 vs 守方战力确定性结算）+ `LaunchOffensive`（Application 端到端：授权→闭合因果→攻城→占城/退兵）+ `OffensiveResult`。复用 M07 后果精神（败可继续）。
**测试**: test_strong_preparation_wins_and_conquers_weak_loses / test_offensive_rejected_when_unauthorized。871/871 绿。
**Code Review**: lean inline（本会话）· ADR-0010/0008/0006/0004 COMPLIANT · 反全知/确定性/无胜率/失败可继续均合规。
