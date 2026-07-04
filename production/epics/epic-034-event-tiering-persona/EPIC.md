# Epic: 事件分级通报 + 主角人设心里话（升级 GDD_015）

> **Layer**: Feature（历史世界模型体验层——破全演义内容爆炸）
> **Governing ADR**: ADR-0007（条件历史世界模型）· ADR-0006（种子化随机）
> **GDD**: GDD_015 条件历史世界模型 §1b（2026-07-04 升级）
> **Status**: ✅ Complete（2026-07-04：Domain 分级+人设 + 运行期通报流。dotnet 全绿；3 DLL 同步。）
> **设计来源**: 2026-07-04 用户裁定——够不着的事件只作通报 + 主角心里话；心里话随开局随机人设着色（丰富体验，非机械种子）。

## 背景与问题

"全演义事件网"内容量巨大曾被压 Later。用户裁定按可达性分级：够得着走完整事件，够不着走轻量通报 + 主角心里话——无关事件近零成本、仍丰富代入。心里话口吻随开局随机人设（雄心/忠义/务实/谨慎）。

## Stories（Complete）

| # | Story | Type | Status |
|---|-------|------|--------|
| 001 | Domain 分级（NoticeTier）+ 人设（ProtagonistPersona/Roll）+ 心里话表（MonologueCatalog）+ EventReflectionService | Logic | ✅ |
| 002 | 接入推进 A3（Advance 触发历史→人设通报流 + EventNoticeView + 袁术称帝可见） | Integration | ✅ |

## 完成说明

Domain/World：ProtagonistPersona/NoticeTier/MonologueRule/EventReflection/EventReflectionService。CampaignRuntime.Advance 触发 AdvanceHistory → 反射成通报流 + EventNoticeView。PlayableCampaign 加袁术称帝够不着事件。测试 EventReflectionTests/EventNoticeWiringTests。commit a156115→fcadfca→6d2f8fe。
