# UI 层接入状态审计（2026-07-05）

**核心结论**：所有系统的**纯 C# 视图投影**基本齐备且已单测（UI 只需绑定）；**Unity 屏**多数已建但 ★需编辑器验证；
少数系统只有"显示"没有"交互屏"，两处系统还缺视图。整个游戏在 console 已可玩（1130 测试绿）。

## 逐系统状态

| 系统 | 纯C#视图（已单测） | Unity 屏 | 状态 |
|---|---|---|---|
| 纪元/一生（年·季·龄·阶段·寿终） | ArrivalLifeView / GameHudView | HudController 时间条 + GameStatusPanel | ★屏待验，视图全 |
| 生涯（官阶头衔/功名/在野） | CareerView / GameHudView | GameStatusPanel | ★屏待验 |
| 行动容量（手令 X/Y） | GameHudView | GameStatusPanel | ★屏待验 |
| 君主任务（讨伐/守土/献纳） | LordMissionView / GameHudView | GameStatusPanel（**仅显示**） | ⚠ 无交互屏（结算/献纳按钮）——命令在 console |
| 选年选城开局 | ScenarioChoiceView / GovernorCityChoiceView | GameSetup | ★屏待验 |
| 治理（征粮/修工事/安抚） | GovernanceActionView | Hud | ★屏待验（含容量拒绝反馈） |
| 情报/军议 | EnemyIntelPanelView / CampaignCouncilView | Hud | ★屏待验 |
| 出征攻城（授权→组装→区域战） | OffensiveTargetsView / OffensivePlanView / ZoneBattleView | Hud 出征面板 + ZoneBattle | ★屏待验 |
| 守城 | ZoneBattleView | ZoneBattle | ★屏待验 |
| 人心施计 | SubversionView | Hud（按钮 stub） | ⚠ 仅一键 stub，无选城/选计屏 |
| 人才招揽 | **TalentRecruitView**（反全知无数值） | GameStatusPanel（**仅显示**） | ⚠ 无 reveal/recruit 交互屏——命令在 console |
| 武将录 | GeneralRosterView | Roster | ★屏待验 |
| 战略大地图 | CampaignMapView（含英雄棋子） | scaffold + Adapters（Assets/Scripts/…/CampaignMap） | ★需场景搭建 + 美术（见 ART_GUIDE.md） |
| 被灭续局（被俘→归顺/投奔） | DefeatFlow（直接） | Defeat | ★屏待验 |
| 传承 | ArrivalLifeView | Defeat/HUD 触发 | ★屏待验 |
| 外交（缔约/背约） | **DiplomacyView**（各势力立场中文·可否径攻） | ⚠ 无屏 | 视图已补，待做屏（console 有 pact/breach） |
| **多城战区（委任/亲管）** | TheaterCityReport（部分） | ⚠ **无屏** | ✗ 待补（console 有 theater/delegate） |

## 还没完成的（按优先级）

1. ~~外交视图/屏~~ ✓ 已建（DiplomacyView + DiplomacyScreen：立场 + 缔约/背约按钮）。★待编辑器验证。
2. ~~多城战区屏~~ ✓ 已建（TheaterScreen：列直辖/委任城）。★待编辑器验证。
3. ~~交互补全~~ ✓ 大部已补：GameStatusPanel 加 结算君命/献纳 + 打听人才/招揽 按钮（经 SessionRuntime 便捷桥）。
   仅剩**施计选城选计屏**（现 Hud 一键 stub）与**委任交互**（现只列，委任经 console）待做。
4. **战略大地图**：已**停泊**（移出 Assets → `parked/campaign-map-scaffold/`，Unity 看不到，不参与编译）——它依赖 4 包+缺类，会拖垮核心编译（Codex 首轮 68 errors 全源于此）。进地图/美术阶段再启用，turnkey 步骤见 `parked/campaign-map-scaffold/PARKED-REENABLE.md`。
5. **★核心屏编辑器验证（第 2 轮）**：核心已可独立编译。跑菜单 `三国/构建 Slice 场景` 一键生成全部场景（MainMenu/GameSetup/Hud+状态叠加/Roster/Diplomacy/Theater/ZoneBattle/Defeat/PauseMenu/Accessibility）+ 写 Build Settings，然后进 Play 冒烟。
6. ~~HUD 导航补全~~ ✓ 已接：HUD 头部加 `hud-nav`（武将录/外交/多城战区 → 各自场景，返回回 HUD）；
   PauseMenu 继续游戏→Hud、设置→AccessibilitySettings（带返回路径）、退出→MainMenu；
   AccessibilitySettings 返回→`ReturnScene`（镜像 ZoneBattleSession.ReturnScene，默认 MainMenu）。★需编辑器再验一轮。
7. ~~Hud 顶部叠加重叠 + 拦截点击~~ ✓ 修（★待编辑器复验）：computer-use 第 3 轮发现 GameStatusPanel 叠加层（全屏第二 UIDocument）
   遮挡 HUD 顶栏并**拦截点击**→武将录/外交/多城/推进时段点不到。**根治**：废弃叠加层，把君命/献纳/打听人才/招揽 + 官阶/手令/君命
   并入 HUD 左列新卡 `career-mission`（单 UIDocument，无叠加）。SliceSceneBuilder 移除 Hud 的 OverlayUxml。
   **需重跑「三国/构建 Slice 场景」再验。** GameStatusPanel.uxml/Controller 保留但已不实例化（dormant）。
8. ~~GameSetup 任选一城·开局按钮被挤出屏幕~~ ✓ 修（★待复验）：36 城列表撑高把「就此起家」顶出可视区、外层不滚动 → 太守开局无法开始。
   加 GameSetup.uss：整屏纵向弹性布局，choice-list `flex-grow:1;min-height:0` 内部滚动，footer `flex-shrink:0` 恒钉底。

## computer-use 第 3 轮验收（2026-07-05）
- ★**头号 bug 修复确认 PASS**：命名「刘玄德·小沛」开局 → HUD 顶栏显「刘玄德·小沛 · 守土图强，锋指下邳」，不再是汜水关。
- 暂停菜单三路径全 PASS（继续/设置→无障碍→返回/退出→主菜单）；无障碍设置屏布局工整无重叠。
- 出征「未授权即发起」→ 正确显示「先请缨受命」拒绝原因 PASS。
- Console 全程 0 error 0 warning（除无害 Input Manager deprecation 提示）。
- **两个阻断 bug**（上面 7/8，已改待复验）：HUD 顶部叠加拦截点击致导航点不到；GameSetup 太守开局按钮够不到。

## Codex 首轮验证结论（2026-07-05）
- 核心 UI 屏绑定名静态检查**通过**；3 DLL 加载正常，**无** DLL 缺方法错误。→ 我的核心屏是干净的。
- 68 编译错误**全部**在战略地图 scaffold（缺 Input System/URP/DOTween + 缺 4 stub 类 + `init` 缺 IsExternalInit）。
- 处置：**停泊 scaffold**（去出编译）→ 核心独立可编译可 Play；**扩 SliceSceneBuilder** 覆盖全部核心屏。

## 给做 UI 的人的一句话
每个系统都有一个**纯 C# 只读视图**（多数已单测），Unity 侧只需 `SessionRuntime.XxxView()` 取来绑定，
不碰任何领域内部、不显任何隐藏数值（反全知红线由视图层保证）。console（`dotnet run --project src/Console`）
是行为参照——UI 照着 console 能做的做即可。
