# 续13 · UI 布局 Bug 修复验证报告

- **Unity 版本**：Unity 6.3 LTS (6000.3.18f1)
- **验证时间**：2026-07-06（北京时间约 08:48–09:48，无人值守 computer-use 实测）
- **是否重跑过场景生成器**：是（菜单「三国 → 构建 Slice 场景」；输出「[SliceSceneBuilder] 完成：10 场景 + PanelSettings + Build Settings」）
- **代码版本**：本机工作树 844d5cc（含续13 两处修复；git pull 因授权弹窗超时 BLOCKED，但本机即修复版，不影响结论）
- **判读**：截图由 Claude 主会话逐张核对（非转述）。报告原 md 因 computer-use 环境写长 CJK 路径 IO 截断，本文件为规整重写。

## 【A】HUD 无叠加遮挡 + 生涯·君命·人才卡 — ✅ 全 PASS

| # | 检查点 | 结果 | 备注 |
|---|---|---|---|
| 1 | HUD 无全屏遮挡层（原 GameStatusPanel） | PASS | 顶栏/侧栏/左列卡均无遮挡，第二 UIDocument 全屏面板已废除 |
| 2 | 武将录按钮 | PASS | 跳 Roster，武将列表可滚动 |
| 3 | 外交按钮 | PASS | 跳 Diplomacy，显各诸侯立场 |
| 4 | 多城战区按钮 | PASS | 跳 Theater |
| 5 | 推进时段按钮 | PASS | 日期第1日→第2日，按钮响应正常 |
| 6 | 生涯·君命·人才卡存在 | PASS | HUD 左列可见，含标题/太守信息/君命 |
| 7 | 结算君命按钮 | PASS | 点击有响应 |
| 8 | 献纳按钮 | PASS | 点击有响应 |
| 9 | 打听人才按钮 | PASS | 点击后出结果行 |
| 10 | 招揽按钮 | PASS | 点击有响应（★见下「已修」） |
| 11 | 顶栏目标显示 | PASS | 小沛开局显「刘玄德·小沛·守土图强，锋指下邳」，非汜水关 |

截图：`s13-screens/hud-overview.png`（小沛整屏）、`s13-screens/hud-career-card.png`（左列卡）。

## 【B】GameSetup 陈留「就此起家」可达 — ✅ PASS（功能）

| 检查点 | 结果 | 备注 |
|---|---|---|
| 陈留可选中 | PASS | `gamesetup-chenliu.png` 陈留高亮，「已选：陈留（曹操·部将 6 员）」 |
| 「就此起家」可点、进 HUD | PASS | `hud-chenliu.png` 顶栏「陈留太守·奉曹操号令·守土图强，锋指虎牢关」——computer-use 仅能点可见像素，能进此 HUD 即证明按钮可见可点 |

> 备注：`gamesetup-chenliu.png` 截图未把「就此起家」按钮框进画面（列表下方奶油区），但功能上已由成功进入陈留太守 HUD 证明「按钮被顶出屏幕、外层不滚动」的原 bug 已解。若需纯视觉证明 footer 钉底，可补一张完整 GameSetup 截图。

截图：`s13-screens/gamesetup-chenliu.png`、`s13-screens/hud-chenliu.png`。

## 【C】Console 洁净 — ✅ PASS

| 检查点 | 结果 | 备注 |
|---|---|---|
| 无 error（红） | PASS | 仅 1 条 info 级 Debug.Log「意图 NewGameIntent → 命令 StartNewGameCommand」，Error Pause 未触发 |

截图：`s13-screens/console-final.png`。

## 总判定：✅ 两个阻断级布局 bug 均已修复并验证通过

- HUD 叠加层拦截点击 → **已解**（单 UIDocument 化，4 导航 + career 卡按钮均可点）。
- GameSetup「就此起家」够不到 → **已解**（功能性进入陈留太守 HUD 证明可达）。
- 顺带印证续12 顶栏 SeatObjective 修复（小沛→下邳，非汜水关）。

## 附带发现与处置

1. **[已修] 招揽结果泄露原始 ID**：原显示「char-nengli 未招得」。根因 `DisplayNames` 未登记三名原型人才。修法：补 `char-wolong`=卧龙 / `char-xiaojiang`=骁将 / `char-nengli`=能吏，并加回归测试 `test_talent_view_name_resolves_chinese_not_raw_id`（dotnet 1136 绿）。
2. **[环境噪声] NVIDIA GeForce Overlay 残影**：部分截图中央区域有系统级渲染残留，非游戏 bug，不影响功能判读。
3. **[待你在编辑器验证]** 见 active.md 复验清单。
