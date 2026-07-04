# Epic: 生涯纵深——忠诚经营 / 被挖角 / 官职撤职

> **Layer**: Feature（太守人生支柱纵深）
> **Governing ADR**: ADR-0004（确定性）· ADR-0006（种子化随机）
> **GDD**: GDD_014 战役与生涯（忠诚维持 + 被挖角 + 官职任免增量）
> **Status**: ✅ Complete（2026-07-04：Domain 忠诚服务 + 撤职命令 + 日界推进接入。dotnet 全绿；3 DLL 同步。）

## 背景与问题

自立结局分支（拥立/部分跟随/众叛亲离）、晋升、官职任命本已存在；缺"忠诚经营 + 被挖角 + 撤职"——僚属忠诚是静止的、无被挖角威胁、无撤职。本 epic 补齐，让忠诚成活循环并喂已有自立结局（loyal_ratio）。

## 设计裁定

1. 忠诚经营：赏赐升忠诚 / 久疏衰减（不越下限）/ 低忠诚可被敌挖角（忠者不可挖，叛离带走官职）。
2. 被挖角与人心杠杆对玩家守将策反**对称**。
3. 官职撤职：撤职去任免、前任派系不满（好感降）→ 喂忠诚经营。
4. 接入日界推进：每跨一日忠诚衰减 + 对最不忠者种子化挖角。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | Domain 忠诚经营（Reward/Decay/AttemptPoach）+ 撤职命令 + 派系不满 | Logic | ✅ |
| 002 | 接入日界推进 A2（忠诚衰减 + 挖角驱动） | Integration | ✅ |

## 完成说明

Domain/Career：RetinueLoyaltyService/Config/PoachResult + RetinueState.WithMemberAffinity/WithoutMember/WithoutOffice + DismissOfficeCommand（CareerStateService.ApplyDismissOffice + 派系不满）。CampaignSessionService.TickRetinueLoyalty 接日界。测试 RetinueLoyaltyTests/OfficeDismissalTests/RetinueLoyaltyTickTests。commit 001682f→a7c7ee8→a377d7f。
