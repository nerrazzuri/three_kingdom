# s25 · 武将立绘 + 主菜单背景接入 Unity — 编辑器验证清单

日期：2026-07-09　｜　★需 Unity 编辑器验证（无法无头编译）

## 本次改动（已落盘）
- 资产：`Assets/Resources/Portraits/{关羽,诸葛亮,曹操,司马懿,吕布}.png` + `Assets/Resources/Backgrounds/main-menu.png`
- `Assets/UI/RosterController.cs`：武将录每行加立绘缩略图位（64×85）——`Resources.Load<Texture2D>("Portraits/"+中文名)`，有图显示立绘、其余深色剪影占位（反全知：立绘不泄任何数值）
- `Assets/UI/MainMenu.uxml`：补挂 `<ui:Style src="MainMenu.uss" />`（此前漏挂，uss 未生效）
- `Assets/UI/MainMenu.uss`：`.menu-root` 加 `background-image: resource("Backgrounds/main-menu")` + `background-size: cover`；标题/按钮组加半透宣纸衬底保可读

## 验证步骤
1. 打开/切到 Unity 编辑器（工程 `D:\Projects\三国演义\Claude-Code-Game-Studios`），等待自动导入新资源 + 重新编译。
2. **Console 检查**：应 0 编译错误、0 报错。若有红错 → 截图存 `00-console.png`。
3. 打开 `Assets/Scenes/MainMenu.unity` → Play：主菜单应显示「军府活图卷」背景（赭石色案上图卷），标题「三国演义：兵法沙盒」与三按钮在半透宣纸卡上清晰可读。截图存 `01-mainmenu.png`。
4. 停 Play → 打开 `Assets/Scenes/Roster.unity` → Play：武将录列表每行左侧应有立绘缩略图；滚动找到 **关羽 / 诸葛亮 / 曹操 / 司马懿 / 吕布**——这 5 位显示对应立绘、脸清晰可辨，其余武将为深色剪影占位。截图存 `02-roster.png`（尽量含这 5 位或其中数位）。
5. 全程 Console 无报错。

## 判据（PASS 条件）
- [ ] 编译 0 错误
- [ ] 主菜单背景到位 + 标题/按钮清晰可读
- [ ] 武将录 5 立绘正确显示、缩略图脸清晰不糊
- [ ] 全程无报错

截图存本目录；完成后写 `DONE.flag`（内容：PASS 或问题摘要）。
