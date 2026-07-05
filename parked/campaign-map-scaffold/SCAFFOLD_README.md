# Campaign Map Presentation Layer — Scaffold

Generated scaffold for `three_kingdom` · Unity 6.3 LTS · URP · C#

---

## File Structure

```
Presentation/CampaignMap/
├── Controllers/
│   ├── CampaignMapViewController.cs   ← Root presenter. Start here.
│   ├── TerritoryLayerController.cs    ← Manages all territory GameObjects
│   ├── HeroTokenLayerController.cs    ← Manages hero token movement/spawn
│   ├── CampaignMapCameraController.cs ← Pan / zoom / edge-scroll / clamp
│   └── WeatherOverlayController.cs    ← URP volume + particle weather
├── Views/
│   └── TerritoryView.cs               ← Single territory prefab script
├── UI/
│   ├── TerritoryInfoPanel.cs          ← Selected territory side panel
│   ├── FactionStatusBar.cs            ← (stub — add your faction HUD)
│   ├── TurnPhaseHUD.cs                ← (stub — add turn/phase display)
│   └── ActionMenuPanel.cs             ← (stub — add action buttons)
├── Data/
│   ├── CampaignMapViewModels.cs       ← All DTOs crossing App→Presentation
│   ├── CampaignMapServiceInterfaces.cs ← Interfaces Presentation depends on
│   └── CampaignMapConfig.cs           ← ScriptableObject config asset
└── Events/
    └── CampaignMapEvents.cs           ← Event DTOs from Application layer
```

---

## Wiring Checklist

### 1. Domain → ViewModel mapping
Implement `ICampaignMapQueryService` in your Application layer.
Map your domain `Territory` entity → `TerritoryViewModel`.
Map your domain `Hero` entity → `HeroPositionViewModel`.

### 2. Service resolution
In `CampaignMapViewController.ResolveServices()`, replace
`FindObjectOfType<...Adapter>()` calls with your actual DI / ServiceLocator.

### 3. Event bus
Implement `IGameEventBus`. A simple typed dictionary of `Action<T>` delegates works.
Your Application layer use cases `Publish<T>` events after state changes.
Presentation layer `Subscribe<T>` and react without knowing domain internals.

### 4. Territory world positions
`TerritoryViewModel.WorldPosition` must be populated with real Unity world-space
coordinates. Store your map layout in a separate ScriptableObject
(`MapLayoutData`) keyed by territory ID — keep position data out of domain entities.

### 5. Hero portraits
Load portraits via **Addressables** keyed by hero ID.
Set `HeroPositionViewModel.PortraitSprite` in your mapper after async load.

### 6. Movement range highlight
In `TerritoryLayerController.HighlightMovementRange()`, wire in your domain
graph traversal (BFS/DFS on territory adjacency) to get reachable IDs.

### 7. DOTween
`TerritoryInfoPanel` uses DOTween for panel fade. Either:
- Add DOTween (Free) from the Unity Asset Store, **or**
- Replace `.DOFade()` calls with a simple `IEnumerator` coroutine tween.

---

## Scene Hierarchy (suggested)

```
CampaignMapScene
├── CameraRig                          ← CampaignMapCameraController
│   └── Main Camera (URP, orthographic)
├── Layers
│   ├── TerritoryLayer                 ← TerritoryLayerController
│   ├── HeroTokenLayer                 ← HeroTokenLayerController
│   └── WeatherOverlay                 ← WeatherOverlayController
├── UI (Screen Space - Overlay Canvas)
│   ├── TerritoryInfoPanel
│   ├── FactionStatusBar
│   ├── TurnPhaseHUD
│   └── ActionMenuPanel
├── CampaignMapRoot                    ← CampaignMapViewController
├── GlobalVolume (URP Post-processing)
└── ServiceAdapters (your DI wiring)
```

---

## Dependencies

| Package | Purpose |
|---|---|
| Unity Input System | Camera pan/zoom input |
| Unity URP | Post-processing (weather volume) |
| TextMeshPro | All text labels |
| DOTween (optional) | Panel fade animations |
| Addressables | Hero portrait async loading |

---

## What's NOT in this scaffold (next steps)

- `FactionStatusBar.cs` — faction resource bars at top of screen
- `TurnPhaseHUD.cs` — turn number + phase indicator
- `ActionMenuPanel.cs` — radial or list action menu
- `HeroTokenView.cs` — the hero token prefab script (portrait + faction ring)
- `MapLayoutData.cs` — ScriptableObject storing territory world positions
- Territory polygon meshes / art asset pipeline
- Battle transition animator
