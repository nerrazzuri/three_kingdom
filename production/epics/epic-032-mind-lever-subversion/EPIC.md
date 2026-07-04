# Epic: 人心杠杆（离间/策反/攻心 · Mind-Lever Subversion）

> **Layer**: Feature（护城河核心——把战斗接入关系与情报，game-concept 明列独门差异）
> **Governing ADR**: **[ADR-0014](../../../docs/architecture/adr-0014-mind-lever-subversion.md)** · 复用 ADR-0004/0006/0008/0010/0011/0012
> **GDD**: **GDD_024 人心杠杆（Reviewed，2026-07-04）**
> **Status**: ✅ Complete（2026-07-04：Domain 核心 + 战斗接缝 + 战役闭环 + 存档 + 运行期接入。dotnet 全绿；3 DLL 同步。）
> **设计来源**: 2026-07-04 用户裁定"人心杠杆不砍反而做进来"——兑现 game-concept 护城河。

## 背景与问题

game-concept 白纸黑字的"人心杠杆（离间/策反/攻心）"是区别于 Total War / 三国志的独门差异，此前只有零件（Intel/Relationships/Cohesion/Diplomacy）无完整"施计→改变战斗条件"闭环。本 epic 建 GDD_024 + ADR-0014 + 实现，把护城河做实。

## 设计裁定（GDD_024 / ADR-0014）

1. 三计产统一 `SubversionEffect`，在出征/区域/抽象攻城接缝削弱守方（士气/军纪/有效守军倒戈）。
2. 反全知门：守将画像投影自情报/关系，未侦察大折扣。
3. 种子化确定性 + 可反噬（暴露+守方士气反升，失败可继续）。
4. 撬动而非替代六维准备（W5：裸兵靠施计破不了坚城）。
5. 敌对称威胁（守城可遭策反 + 预警，不作弊）。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | Domain 核心（三计/画像/效果/服务）+ 战斗接缝 + W5 | Logic | ✅ |
| 002 | 战役闭环（AttemptSubversion + 会话待生效 + 存档往返） | Integration | ✅ |
| 003 | 运行期接入 A1（CampaignRuntime 命令 + 画像工厂 + 反噬暴露写回 + SubversionView） | Integration | ✅ |

## 完成说明

Domain/Subversion（SubversionScheme/TargetProfile/Effect/Config/Outcome/Service）+ 接缝（OffensiveDeploymentPlanner/ZoneBattleRuntime/CampaignSessionService.ApplySubversion）+ 运行期（CampaignRuntime.AttemptSubversion + SubversionTargetProfileFactory + SubversionView）。测试 SubversionTests/SubversionBattleIntegrationTests/SubversionCampaignTests/SubversionRuntimeTests。commit 链 4e31467→2a37c3a→d7abbec。
