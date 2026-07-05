# 战略大地图 scaffold —— 已停泊（PARKED），暂不参与编译

## 为什么停泊
本文件夹已移出 `Assets/`，停放在仓库根的 `parked/campaign-map-scaffold/`。**Unity 只编译 `Assets/` 下代码**，
移出后 Unity 完全看不到它（所有 .cs 都不参与编译），而 git 正常追踪（不像 `*~` 会被 .gitignore 吞掉）。
这样做是因为该 scaffold 依赖 4 个尚未安装的包 + 若干缺失类型，会让整个 `Assembly-CSharp` 编译失败
（Codex 首轮验证：68 errors，全部源于此），从而拖垮已完成、已测的核心游戏，Unity 进 Safe Mode 无法 Play。

停泊后：核心游戏（Assets/UI 全部屏 + 3 个已测 DLL）独立编译、可进 Play 灰盒。地图是后续「地图+美术」阶段的事。
核心**不依赖**本 scaffold（`SessionRuntime.MapView()` 返回的是 DLL 里的纯数据 `CampaignMapView`，与此处无关）。

## 重新启用（进入地图/美术阶段时，一次性 turnkey 清单）
1. **移回 Assets**：`git mv parked/campaign-map-scaffold Assets/Scripts/Presentation/CampaignMap`（让 Unity 重新看到并编译）。
2. **装 4 个包**（Package Manager）：
   - Input System（`com.unity.inputsystem`）—— 摄像机平移/缩放
   - Universal RP（`com.unity.render-pipelines.universal`）—— 天气后处理 Volume
   - TextMeshPro（`com.unity.textmeshpro`）—— 信息面板文本
   - DOTween（Asset Store / OpenUPM）—— TerritoryInfoPanel 淡入淡出；**或**见第 4 点改协程免装
3. **加 `IsExternalInit` polyfill**（Unity 的 C# 9 `init` 访问器需要它，否则报 CS0518）：
   在 `Assets/Scripts/` 下新建 `IsExternalInit.cs`：
   ```csharp
   namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} }
   ```
   （一处即可，全工程 `init` 属性随即可用——影响 CampaignMapViewModels.cs / CampaignMapEvents.cs）
4. **补 4 个缺失 stub 类**（Views/UI 里被引用但未定义）：`HeroTokenView`、`FactionStatusBar`、
   `TurnPhaseHUD`、`ActionMenuPanel`。灰盒阶段给空 MonoBehaviour 即可，逐步填。
5. **DOTween 可选替换**：若不装 DOTween，把 `UI/TerritoryInfoPanel.cs` 的 `.DOFade(...)`
   换成一个 `IEnumerator` 协程改 `CanvasGroup.alpha`（去掉 `using DG.Tweening;`）。
6. **接线**：Adapters/（CampaignMapDataAdapter、CampaignActionAdapter、GameEventBus）已写好，
   把 `SessionRuntime.MapView()` 的数据喂给 CampaignMapViewController；细节见 INTEGRATION.md。
7. **美术**：territory/hero token/weather 的 prefab 与贴图，见 ART_GUIDE.md（可先灰盒纯色跑通）。

## 里面有什么
14 个 .cs（Controllers/ Adapters/ Data/ Events/ Views/ UI/）+ INTEGRATION.md + ART_GUIDE.md + SCAFFOLD_README.md。
逻辑数据源永远是 DLL；此层只做「取 view → 渲染 + 回传动作」，不放规则。
