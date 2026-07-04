# Epic: 统一终局循环（Unification Endgame Loop / M14）

> **Layer**: Feature（终局）
> **Architecture Module**: M14（`full-game-loop-module-plan` §M14）
> **Governing ADR**: ADR-0004（确定性）
> **GDD**: **GDD_018 统一终局（Draft，2026-07-04）** · 消费 GDD_017 争霸态
> **Status**: ✅ Complete（2026-07-04：GDD_018 + 实现+测试。Domain/Contention/Endgame（EndgameStatus/Config/Service：统一/覆灭/继续判定）+ CampaignRuntime.Endgame 接入。TR-endgame-001。dotnet 963/963 绿。原"硬阻塞 GDD_018 不存在"已解除。平衡延后。）

## 背景与问题

"打天下"需要终点：统一天下（胜）/势力覆灭（负）。此前 GDD_018 不存在为硬阻塞——本 epic 建 GDD_018 + 实现，解除阻塞，给整局一个胜负终局。

## 设计裁定（GDD_018）

1. 胜局：玩家支配度 ≥ 统一阈值（默认过半）或群雄尽灭 → 统一天下。
2. 负局：玩家领城归零 → 覆灭（终点，区别于战役级失败可继续）。
3. 确定性纯函数判定（同争霸态同判）。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | 终局判定（统一/覆灭/继续，确定性） | Logic | ✅ |
| 002 | 接争霸态 → 运行期终局钩子 | Integration | ✅ |

## 完成说明

Domain/Contention/Endgame.cs（EndgameStatus{Ongoing|PlayerUnifies|PlayerEliminated}/EndgameConfig/EndgameService）+ CampaignRuntime.Endgame()。测试 ContentionTests（覆灭/统一按阈/群雄尽灭/继续）+ 运行期占城→统一终局整合。dotnet 963/963 绿。ADR 复用 0004。平衡延后（统一阈值待调）。
