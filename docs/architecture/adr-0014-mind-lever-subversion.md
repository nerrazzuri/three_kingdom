# ADR-0014 — 人心杠杆·施计改变战斗条件契约（Mind-Lever Subversion）

> **Status**: Accepted（2026-07-04；lean 内联裁定——复用既有确定性/反全知/控制权基座，无新违反禁则，story 可引用。首次落地 GDD_024 人心杠杆，兑现 game-concept 独门护城河。）
> **相关**: GDD_024 人心杠杆 · GDD_006 关系 · GDD_007 情报 · GDD_011 凝聚力 · GDD_019 出征 · GDD_021 区域战斗 · epic-032 · ADR-0004（确定性）· ADR-0006（种子化随机·反全知）· ADR-0008（控制权契约）· ADR-0010（占城归属）· ADR-0011/0012（出征/区域战斗接缝）

## Context（背景）

game-concept 明列"人心杠杆（离间/策反/攻心）"为区别于 Total War / 三国志的**独门护城河**——把战斗接入关系与情报系统。此前该护城河只有零件（Intel/Relationships/Cohesion/Diplomacy），无完整"施计→改变战斗条件"闭环。核心问题：如何让战前施计**确定性、反全知、可反噬、且撬动而非替代**六维准备地改变战斗结果，而不新造随机基座、不违反既有禁则。

## Decision（决策）

**D1 施计产出 `SubversionEffect`，在战斗接缝消费——不新增战斗维度。**
三计（离间/策反/攻心）经确定性纯函数结算，产出统一的 `SubversionEffect`（守方士气 delta / 有效守军倒戈比 / 守方军纪 delta）。该效果在出征→战斗接缝（`PlanDefender` 重载 / `ZoneBattleRuntime.FromOffensive` / 抽象攻城 `ApplySubversion`）**削弱守方**。不给区域引擎新增独立军纪字段——军纪罚并入守方有效稳定度（士气/工事），见 GDD_024 §17。

**D2 反全知门（复用 ADR-0006/GDD_007）。**
守将弱点画像**投影自 Intel + Relationships**，非世界真值。未侦察 → 弱点不可读、成功度大折扣（`EffectiveIntelQuality=0` + 盲施惩罚）。**结构上不接受真值**。

**D3 种子化确定性结算（复用 ADR-0006）。**
成功度 `s = clamp(base + w·信号 − w·抵抗 − decay·已施次数, 0, 1)`；`roll = DeterministicRandom(seed).NextUnit()`；`roll<s`成、`roll<s+band`反噬、否则无效。注入式确定性流，非掷骰、可复现、随存档。

**D4 可反噬（失败可继续，红线）。**
被识破 → 守方士气反升 + 该城暴露（守将警觉↑、后续更难）。反噬是合法可继续状态，非死局。暴露态 `session.MarkSubversionExposed` 随会话。

**D5 撬动而非替代（W5 护栏）。**
施计**降低**破城门槛但**不单独决定胜负**——裸兵纵有强施计仍破不了坚城；施计 + 六维准备才最优。单城同计递减（防无脑刷）。由 `SubversionBattleIntegrationTests` 端到端锁定。

**D6 对称威胁（反全知不作弊）。**
敌方亦可对玩家守城施人心杠杆（种子化、同规则、不作弊），成功削弱玩家守方 + 预警（非无解）。落地 `CampaignRuntime.StartDefenseBattle`。

**D7 占城归属复用 ADR-0008/0010。**
策反导致的守军倒戈只改变**有效守军**（战斗输入），不独立写城池归属；破城后归属仍走 ADR-0010 占城 C（GDD_004 唯一控制权权威）。

## Consequences（后果）

- **正面**：护城河兑现——情报/关系/军纪/战斗四系统贯通；以弱胜强多一条"攻心"路径；与既有确定性/反全知/存档体系一致，零新禁则违反。
- **代价**：守将性格画像目前为按城 id 种子化（MVP），非世界模型权威人物属性——后续可升级为真实武将属性投影。军纪并入有效稳定度而非独立维（有意，避免核心引擎增维）。
- **权衡**：施计成功度/效果系数为占位默认，待平衡打磨（GDD_024 §11 / C11）。

## ADR Dependencies（依赖）

复用 ADR-0004（确定性定点）· ADR-0006（种子化随机 + 反全知）· ADR-0008（控制权唯一权威）· ADR-0010（占城归属）· ADR-0011/0012（出征/区域战斗接缝）。不取代任何既有 ADR。

## Engine Compatibility（引擎兼容）

Domain 纯 C#，无 UnityEngine 依赖（ADR-0002 分层）。确定性定点（ADR-0004）。Presentation `SubversionView`/`EventNoticeView` 只读映射。

## GDD Requirements Addressed（覆盖的 GDD 需求）

GDD_024 全部主要规则 R1–R6 + 公式 F1–F4 + 测试要求（反全知门 / 策反门 / 确定性 / 三计效果映射 / W5 撬动非替代 / 递减 / 存档 round-trip）。落地 epic-032。
