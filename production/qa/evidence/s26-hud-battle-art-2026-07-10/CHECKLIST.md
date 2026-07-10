# s26 · HUD 麾下立绘 + 战斗屏城战背景 — 编辑器验证清单

日期：2026-07-10　｜　★需 Unity 编辑器验证

## 本次改动（已落盘）
- 资产：`Assets/Resources/Backgrounds/battle-city.png`（坚城，战斗屏背景）
- `Assets/UI/ZoneBattleController.cs`：OnEnable 给 zb-root 挂 `Backgrounds/battle-city` 背景图（不透明宣纸卡叠其上仍可读）。
- `Assets/UI/HudController.cs`：新增 `RenderRetinue()` 填「麾下人物」卡——`SessionRuntime.DeputyRoster` 每员一行（40×53 立绘缩略图 + 中文名 `DisplayNames.Of`）；有图显立绘、无图深色剪影占位；反全知只名不呈数值。OnEnable + 推进时段各调一次刷新。

## 验证步骤
1. 打开/切到 Unity 编辑器，等自动导入 `battle-city.png` + 重编译。
2. **Console**：0 编译错误、0 报错。有红错 → 截图 `00-console.png` + 记进 DONE.flag。
3. **战斗屏背景**：打开 `Assets/Scenes/ZoneBattle.unity` → Play。确认屏幕背景为「坚城·古城墙」战场图，各态势/布阵卡片（宣纸底）叠于其上清晰可读。截图 `01-zonebattle-bg.png`。
4. **HUD 麾下人物**：打开 `Assets/Scenes/Hud.unity` → Play（或经主菜单→新游戏进入）。找「麾下人物」卡：应列出当前部将，每行左侧有 40×53 立绘缩略图位（该武将若属已出立绘的 关羽/诸葛亮/曹操/司马懿/吕布 则显立绘，其余深色剪影占位），右侧中文名。截图 `02-hud-retinue.png`。
   - 注：默认剧本部将是否含这 5 位视开局而定；即便全是剪影占位也证明布线成功（名+缩略图位都在）。
5. 全程 Console 无报错。

## 判据（PASS）
- [ ] 编译 0 错误
- [ ] 战斗屏坚城背景到位 + 卡片可读
- [ ] HUD 麾下人物列表填充（名 + 立绘/剪影缩略图位），有 5 famous 者显立绘
- [ ] 全程无报错

截图存本目录；完成写 `DONE.flag`（PASS 或问题摘要）。
