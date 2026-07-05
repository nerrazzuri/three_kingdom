# 战略大地图（Campaign Map）接入说明

来源：`three_kingdom_campaign_map_scaffold.zip`（Unity 6.3 / URP / C#）。此前项目**无大地图层**，本次落地并接上我们已单测的世界数据。

## 已接（Adapters/）
- **CampaignMapDataAdapter**（`ICampaignMapQueryService` + `ITerritoryPositionLookup`）：
  把纯 C# 投影 `ThreeKingdom.Presentation.Screens.CampaignMapView`（经 `SessionRuntime.MapView()`，dotnet 已单测）
  映射为 scaffold 的 `MapSnapshot/TerritoryViewModel/FactionViewModel`。含 37 城世界坐标 + 粗略邻接 + 势力配色（表现层布局，可在编辑器再调）。
- **CampaignActionAdapter**（`ICampaignActionService`）：EndTurn→推进一周、Attack→出征授权；余动作预留。
- **GameEventBus**（`IGameEventBus`）：类型化 pub/sub 字典。

## 数据流
```
Domain/Application（争霸态+世界城归属，已单测）
   → CampaignRuntime.MapView() → CampaignMapView（纯 C#，无 Unity）
   → SessionRuntime.MapView()（Unity 壳桥）
   → CampaignMapDataAdapter（映射到 scaffold ViewModel）
   → CampaignMapViewController / TerritoryLayer / …（scaffold 渲染）
```

## 仍需在 Unity 编辑器完成（★我无法无头验证）
1. **场景搭建**：按 `SCAFFOLD_README.md` 的层级建 CampaignMapScene；`CampaignMapViewController.ResolveServices()` 里把
   `FindObjectOfType<…Adapter>()` 指向本 Adapters（或接你的 DI）。
2. **包依赖**：Input System / URP / TextMeshPro；DOTween（可选，或把 `.DOFade` 换协程）；Addressables（英雄立绘）。
3. **领土多边形/美术**：TerritoryView 的网格/图元与美术管线。
4. **英雄棋子**：`HeroPositionViewModel` 目前空——地图上武将棋子（用 GeneralDossiers + 立绘 Addressables）为后续。
5. **坐标/邻接微调**：Positions/Adjacency 现为粗摆，编辑器里对着底图再调。

## 备注
- 默认「汜水关太守」的汜水关不在世界大盘 17 席内（第 18 独立席），当前地图投影只列 36 座世界城；扮演诸侯/任选城开局时玩家城即世界城，无此问题。坐标表已含 fanshui 以备。
- 纯 C# 侧（CampaignMapView）已有单测（CampaignMapViewTests）；Adapters/Controllers 为 Unity 侧，需编辑器验证。
