# 测试证据：story-001 会话接缝（SessionRuntime → CampaignSession）

> **Story**: epic-028 story-001 | **Type**: Integration | **Date**: 2026-07-03

## 自动证据（已完成，Claude）

| 项 | 结果 |
|---|---|
| 自动测试 `tests/unit/ThreeKingdom.Domain.Tests/PresentationRuntime/CampaignRuntimeTests.cs` | ✅ 8/8 通过（全套 813/813，`-warnaserror` 0） |
| console harness 全循环脚本自检（场景源迁移后回归） | ✅ 通过 |
| Unity batchmode 编译（6000.3.18f1，`-batchmode -nographics -quit`） | ✅ 0 error CS，正常退出 |
| Plugins DLL 同步（Domain/Application/Presentation，Release netstandard2.1） | ✅ 2026-07-03 重建（旧 DLL 为 2026-06-24，不含 CampaignSession） |
| 编译期边界（Assets/UI 零 `GameSession`/`SessionService` 类型引用） | ✅ grep + 编译双重可证 |

## 人工走查（待用户在 Unity Editor 完成并签核）

按序操作并勾选：

- [ ] **MainMenu「新游戏」**：进入 HUD，时间条显示「第 1 日 · 黎明」
- [ ] **HUD「推进时段」×4**：时段依次黎明→白昼→黄昏→夜间→（跨日提示出现）第 2 日 · 黎明
- [ ] **HUD 未接线面板**：账本等面板显示「接入战役会话中……」，侦察/袭扰/伏击/求援/军议按钮为禁用态（非报错）
- [ ] **HUD「存档」**：显示「已存档」
- [ ] **返回主菜单 →「继续」**：可用；点击后回到 HUD，时间与存档时一致
- [ ] **退出 Play 再进（PlayerPrefs 持久）**：「继续」仍可用，读档后时间正确

**签核**：＿＿＿＿＿＿（用户）　**日期**：＿＿＿＿＿＿

**已知临时限制**（非缺陷，story-003/004 恢复）：HUD 仅时间/推进/存档三项接入战役会话；其余面板占位。旧竖切存档槽（`campaign`）不被新槽（`campaign-session`）读取，属预期。
