# Epic: 出征攻城循环（Offensive Campaign Loop）

> **Layer**: Feature（复用 M05 备战 / M06 战斗 / M07 后果 / GDD_004 控制权，反向用于**进攻**）
> **Architecture Module**: 计划外新增循环（`full-game-loop-module-plan` 原缺"主动出征"，2026-07-04 用户实玩裁定补入）
> **Governing ADR**: ADR-0009（会话装配）· ADR-0004（确定性）· ADR-0008（城池控制权唯一权威 + 变更事件）· ADR-0006（种子化随机）· **ADR-0010（占城归属契约，Proposed）**
> **GDD**: **GDD_019 出征攻城（Draft，2026-07-04 起草）**
> **Status**: Ready for Stories（2026-07-04：GDD_019 → Reviewed、ADR-0010 → Accepted，焦点跨系统审查 gdd-cross-review-2026-07-04.md 5 Warning 全闭；下一步 /create-stories epic-029）
> **设计来源**: 2026-07-04 用户设计对话裁定（见下"设计裁定"）

## 背景与问题

当前可玩循环**只有防守**（epic-015 太守开局守城）——玩家没人来打时只能治内政，无聊、无进取目标。
用户裁定：**"出征"须是太守的主动能力**——经君主授权率军攻打敌城，帮君主（或自己）扩土，
打赢即升官/得地、打输可续。这给整个循环补上"主动进取 + 成长目标"的引擎。

## 设计裁定（2026-07-04，权威）

1. **出征 = 君主授权的主动攻城**：属忠臣线"朝廷政令任务"的一种（game-concept 已有此框架）。玩家选目标敌城 + 获君主授权后率军出征。
2. **闭合因果（核心命门）**：玩家的治理/备战/情报态**真正决定攻城胜负**——工事/兵力→战力，补给→续航，断敌粮→敌弱，情报+设伏成型→兵法优势。准备好→挣来胜利；裸战→可能败。**取代当前脚本固定胜局。**
3. **占城归属 = 方案 C**：
   - **前两座**攻下的城**默认归玩家直辖**（启动动力，尝到"打下来是我的"）。
   - **之后**由君主**种子化确定性随机**决定"自己接管"或"交玩家直辖"（ADR-0006 同款，可复现）。
   - 君主屡抢战果 → **自立动机涌现**（喂 game-concept 自立线，是特意的张力，非缺陷）。
4. **回报联动**：出征胜 → 功绩/名望 → 升官阶（接 epic-022）→ 更大战区 / 更多出征权 / 直辖更多城。败 → 折损但必留合法可继续状态（epic-020，失败可继续）。
5. **不做成 4X**：出征是**离散的授权战役**（率军打一座城），非自由漫游的帝国地图微操；仍受人/粮/令/情报四重约束。直辖城多了靠 epic-025 委任打理。

## 砍 scope 尺子自检（强制）

- 出征喂给战斗？✅ 整场攻城战。喂给生涯抉择？✅ 打赢=升官/得地/自立张力。**过尺子,是核心不是抄。**

## Stories（已细化，2026-07-04 /create-stories epic-029）

| # | Story | Type | Status | ADR（primary） | TR-ID | 依赖 |
|---|-------|------|--------|------|-------|------|
| 001 | [君主授权出征入口](story-001-lord-authorized-campaign-entry.md) | Integration | Ready | ADR-0009 | TR-offensive-001 | None（epic 内首） |
| 002 | [攻城战接入（进攻视角）](story-002-offensive-battle-integration.md) | Integration | Ready | ADR-0009 | TR-offensive-002 | 001 |
| 003 | [闭合因果：准备→战果](story-003-preparation-to-outcome-causality.md) | Logic | Ready | ADR-0004 | TR-offensive-003 | 002 |
| 004 | [占城归属结算（方案 C）](story-004-conquest-occupation-ownership.md) | Logic | Ready | ADR-0010 | TR-offensive-004 | 003 |
| 005 | [出征后果→功绩→升官联动](story-005-campaign-outcome-promotion-linkage.md) | Integration | Ready | ADR-0009 | TR-offensive-005 | 004 |

依赖链（线性）：001 → 002 → 003 → 004 → 005。5 story = 2 Logic + 3 Integration，0 Blocked（governing ADR 全 Accepted）。
覆盖 GDD_019 全 8 AC：AC-1/2/7→001 · AC-3/7→002 · AC-3→003 · AC-4→004 · AC-5/6/8→005。

## 实现前置（Draft → Ready）

- ✅ **GDD 已审**：`design/gdd/gdd-019-offensive-campaign.md`（**Reviewed**，12 段含公式/边界/验收）。
- ✅ **ADR 已定**：`docs/architecture/adr-0010-conquest-occupation-ownership.md`（**Accepted**，占城归属 C 契约）。
- ✅ **跨系统审查**：`design/gdd/gdd-cross-review-2026-07-04.md`（CONCERNS → 5 Warning 全闭：W1 rebellion_lean 接 GDD_014 自立触发 · W2 GDD_019 F2 厘清条件涌现/TacticRecognizer · W3 GDD_014 出征授权子类型 · W4 5 份反向依赖 · W5 GDD_019 §8 功绩速率护栏）。
- ⏳ **待**：`/create-stories epic-029`（细化各 story 的 AC/TR-ID/QA 用例）→ ADR 落定后 `/consistency-check` 登记 conquestIndex/rebellion_lean/OwnershipVerdict/CampaignAuthorization → 可开工。

## 强制设计锁（继承）

无胜率 · 反全知 · 失败可继续 · 确定性 · 数据驱动 · 兵法条件涌现 · 零复制三国资产。
