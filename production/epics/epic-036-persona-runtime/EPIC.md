# Epic: 主角人设接入运行期 + PersonaView（全局装配收尾）

> **Layer**: Presentation（ViewModel 层——把主角人设接进可玩循环）
> **Governing ADR**: ADR-0002（分层）· ADR-0009（会话装配）
> **GDD**: GDD_015 事件分级（人设）
> **Status**: ✅ Complete（2026-07-04：CampaignRuntime.Persona + PersonaView，存读档一致。dotnet 全绿；3 DLL 同步。）

## 背景与问题

全局循环由既有 CampaignRuntime 装配（NewGame/Advance/Council/Governance/Prep/Battle/Offensive/Defense + 大量 *View）。本 epic 把 GDD_015 主角人设接进运行期并展示，收尾其 Presentation 侧。

## 设计裁定

1. 人设由 Session.Id FNV-1a 确定性派生 → 存读档一致，无需新存档字段（会话 id 已持久化）。
2. PersonaView：人设 → 中文名（雄心/忠义/务实/谨慎）+ 性情描述。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | CampaignRuntime.Persona（Session.Id 派生）+ PersonaView | Integration | ✅ |

## 完成说明

CampaignRuntime.Persona/PersonaView（PersonaSeed FNV-1a）+ Presentation/Screens/PersonaView。测试 PersonaWiringTests（赋人设+展示 / 存读档稳定）。commit b55e488。
