# Epic: 君主争霸循环（Warlord Contention Loop / M13）

> **Layer**: Feature（顶层战略）
> **Architecture Module**: M13（`full-game-loop-module-plan` §M13）
> **Governing ADR**: ADR-0004（确定性）· ADR-0006（种子化随机）· ADR-0008（控制权）
> **GDD**: **GDD_017 君主争霸（Draft，2026-07-04）** · 建于 GDD_015 世界势力
> **Status**: ✅ Complete（2026-07-04：GDD_017 + 实现+测试。Domain/Contention（争霸态/对手种子化兼并/支配度/被灭）+ CampaignRuntime 接入（占城→领土增 + 对手推进 + 终局钩子）+ TR-contention-001/002。dotnet 963/963 绿。原"硬阻塞 GDD_017 不存在"已解除。平衡延后。）

## 背景与问题

玩家经自立线（GDD_014）成为割据一方后，进入**群雄争霸**顶层战略：与曹魏/蜀汉/东吴等争天下。此前 GDD_017 不存在为硬阻塞——本 epic 建 GDD_017 + 实现，解除阻塞。

## 设计裁定（GDD_017）

1. 争霸态 = 各势力领城/存续（建于 GDD_015）；玩家为其一。
2. 玩家经出征攻城（GDD_019/021）+ 多城（GDD_022）扩张；被夺则减。
3. 对手种子化确定性扩张（强吞弱，ADR-0006），非掷骰、可复现。
4. 反全知：他势力意图经情报，领土消长为世界事实。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | 争霸态 + 支配度 + 存档 | Logic | ✅ |
| 002 | 对手种子化兼并（强吞弱·确定性） | Logic | ✅ |
| 003 | 玩家占城→领土增（接出征/多城） | Integration | ✅ |

## 完成说明

Domain/Contention/Contention.cs（PowerStanding/ContentionState/ContentionConfig/RivalExpansionService）+ CampaignRuntime（Contention/AdvanceContention + ConcludeOffensive 领土更新）。终局判定见 epic-027（GDD_018）。测试 ContentionTests（争霸/兼并/支配度）+ 运行期占城→争霸整合。dotnet 963/963 绿。ADR 复用 0004/0006/0008。平衡延后。
