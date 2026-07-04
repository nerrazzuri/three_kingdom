# Epic: 战斗核心层——战场区域部署与区域战斗（Zone Battle Core）

> **Layer**: Core（**游戏核心玩法**——战斗是与其他三国演义游戏的差异点，须最完善）
> **Architecture Module**: 战斗执行层，替换竖切脚本战斗；实现 GDD_010 兵法沙盒执行、首次落地 GDD_016 敌方AI（吸收 epic-021-enemy-ai-loop 意图于区域战斗层）
> **Governing ADR**: **ADR-0012（确定性区域战斗引擎）** · **ADR-0013（敌方区域AI）** · ADR-0004（确定性）· ADR-0006（种子化随机）· ADR-0011（多维准备/无克制/无坐标）· ADR-0009（会话装配/存档）· ADR-0008（城池控制权）
> **GDD**: **GDD_021 战场区域部署与区域战斗（Draft，2026-07-04）** · 复用 GDD_010/011/007/008/002/016/019
> **Status**: ✅ Core Complete（2026-07-04：S1-S7 全实现+测试。Domain 区域战斗引擎 + 敌AI + Application 编排/部署桥 + Presentation 可玩运行期/视图 + Unity 战斗屏壳。dotnet 924/924 绿(-warnaserror；+32 战斗测试)；3 DLL 同步。**待接**：把区域引擎接入战役出征/守城流（替换脚本战斗的接线，见下"迁移"）；敌AI 深度迭代（ADR-0013 D8）。）
> **设计来源**: 2026-07-04 用户设计对话裁定（排兵布阵=核心；见下"设计裁定"）

## 背景与问题

当前战斗是**一击结算 / 竖切脚本**——没有"排兵布阵"，而用户裁定**排兵布阵是本游戏的重点、战斗是与其他三国演义游戏的区别点、全套战斗功能必须最完善**。需把战斗升级为可玩核心层：区域部署 + 战中动态调整 + 敌方AI区域博弈 + 攻守统一，且守住确定性与设计锁。

## 设计裁定（2026-07-04，权威）

1. **排兵布阵 = 区域布阵**（非纯角色布阵[单薄]、非三国志坐标微操）：把支队（将+兵种+兵力）派到**命名区域**（指范围非坐标）；军师按区给方法（指范围不指坐标，无胜率）。
2. **战中可动态调整**（核心好玩点）：逐回合调动（相邻+在途）/投预备/改姿态，有代价。
3. **敌方AI区域博弈必做、且一步到位做到最完善**：反全知 + 种子softmax效用 + 渐进记忆 + 会设伏反扑断粮收缩；架构一次到位，深度按平衡迭代（ADR-0013 D8）。
4. **区域引擎替换竖切脚本战斗**（统一引擎，不留两套）。
5. **先固定 5 类区域，预留每城/战场景自定义区域**（数据驱动，Future）。
6. **攻守统一**：守城=玩家守方布防、攻方AI来攻；出征=玩家攻方。接 GDD_019 六维准备为初始部署输入、占城C为攻城结算。

## 砍 scope 尺子自检（强制）

- 喂给战斗？✅ **它就是战斗核心**。喂给生涯抉择？✅ 攻城胜负→占城/功绩/自立张力。**过尺子，是核心差异点，须最完善。**

## 守设计锁（强制，负向不变量可断言）

- 区域**非坐标格子**（数据模型无 position/grid/facing）；兵种**杠杆非克制**（无克制减益）；兵法**条件涌现非按钮**（门不齐不成型、不打标签）。延续 ADR-0011。

## Stories（拟，待 /create-stories 细化 AC/QA）

| # | Story | Type | ADR（primary） | TR-ID | Status |
|---|-------|------|------|-------|------|
| 001 | 战场区域模型（BattleField/Zone/Detachment/ZoneEngagementState + 确定性哈希 + 无坐标负向不变量） | Logic | ADR-0012 | TR-zone-001/002 | ✅ Complete |
| 002 | 部署 + 按区条件涌现映射（ZoneConditionService + 上下文/配置；六维→条件门） | Logic | ADR-0012 | TR-zone-003/004 | ✅ Complete |
| 003 | 回合同步结算（RoundResolutionService：各区交战/减员随战力比/条件按回合累积/涌现冲击/确定性优先序） | Logic | ADR-0012 | TR-zone-004/005 | ✅ Complete |
| 004 | 战中调整（ZoneCommandService：调动邻接+在途/改姿态 + 稳定错误码 + 不作弊） | Logic | ADR-0012 | TR-zone-003 | ✅ Complete |
| 005 | 敌方区域AI（AiWorldView 反全知 + 种子化整数加权效用 + 渐进记忆 + 同规则不作弊 + LLM隔离；落地 GDD_016） | Logic | ADR-0013 | TR-zone-007~010 | ✅ Complete |
| 006 | 攻守统一编排 + 六维→部署桥（ZoneBattleService/OffensiveDeploymentPlanner/ZoneBattleOutcome；破正面即破城） | Integration | ADR-0009/0012 | TR-zone-006 | ✅ Complete |
| 007 | Presentation 可玩运行期 + 战斗视图（ZoneBattleRuntime/ZoneBattleView）+ Unity 战斗屏壳（ZoneBattleController/uxml） | UI | ADR-0002 | — | ✅ Complete |

依赖链 001→002→003→004→006→007；005 在 003 后并行。**7/7 Complete。**

## 迁移待办（接线，非新能力）

区域引擎已完整替换**脚本战斗的规则/结算**；剩「接线替换」——把战役 `CampaignRuntime.StartBattle/ResolveOutcome`（守城脚本）与 `LaunchOffensive`（一击结算）改为进入 `ZoneBattleRuntime`（多回合区域战斗）→ 终局接占城 C/后果。属集成接线，建议单独一 story 谨慎替换（勿破 892 既有出征/HUD 测试），故本 EPIC 标 Core Complete 而非全 Complete。

## 实现前置

- ✅ GDD_021 Draft · ADR-0012/0013 Accepted · TR-zone-001~010 已登记。
- ▶ 建议 `/review-all-gdds`（焦点 GDD_021）→ `/create-stories epic-031`（细化 AC/QA）→ `/dev-story` 从 S1 起。
- 迁移注意：S6 替换现有 `ScriptedBattle` / CampaignRuntime 的 StartBattle→ResolveOutcome 脚本战斗路径为区域引擎。

## 完成定义

7 story 实现 + 全套 dotnet 测试绿（确定性/门/调整合法性/优先序/敌AI反全知+种子复现+记忆/攻守对称/无坐标+无克制负向不变量/战中存档续战）+ Presentation 可玩 + 平衡首轮校准。战斗"最完善"是持续目标，MVP 后按 ADR-0013 D8 迭代 AI 深度。
