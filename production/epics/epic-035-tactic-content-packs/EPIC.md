# Epic: 战法内容包（火攻/水攻/诈降/围点打援）+ 失败可继续红线

> **Layer**: Feature（战斗纵深——加厚兵法条件涌现，非技能按钮）
> **Governing ADR**: ADR-0012（区域战斗引擎）· ADR-0004（确定性）
> **GDD**: GDD_010 兵法沙盒 / GDD_021 区域战斗
> **Status**: ✅ Complete（2026-07-04：四战法条件涌现 + 复盘链注册 + 失败可继续验证。dotnet 全绿；3 DLL 同步。）

## 背景与问题

区域战斗此前只有假退伏击/断粮/守城待变/夜袭四链。本 epic 加四大经典战法（火攻/水攻/诈降/围点打援）作为**多条件涌现**（非按钮），证明"战法内容包"架构；并逐条验证失败可继续红线。

## 设计裁定

1. 四战法皆条件涌现，条件计入攻方战力（现有 condMul，无需另设按钮）。
2. 各配区分门避免误触发：火攻需干燥、水攻需湿润、诈降需佯攻姿态、围点打援需骑兵机动。
3. 禀赋按地形：粮营火攻/水攻、城门诈降、侧翼伏援。
4. 全部注册 TacticChainConfig（复盘可识别兵法名）。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | 火攻（条件涌现 + 天时 + 禀赋）+ 失败可继续红线验证 | Logic | ✅ |
| 002 | B 内部缺口（火攻标签注册 / 敌AI守城攻心 / 军纪裁定） | Logic | ✅ |
| 003 | D 内容（水攻/诈降/围点打援三战法 + 复盘链） | Logic | ✅ |

## 完成说明

TacticEnums（+4 tag +12 condition）+ ZoneConditionService 涌现门 + BattleField 禀赋 + TacticChainConfig 注册 + ZoneBattleContext.IsDry。测试 FireAttackTests/ContentTacticsTests/FailureContinuableTests。commit 39c0f04→a377d7f→6d2f8fe。
