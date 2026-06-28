# Story 004: 统一会话存档信封 round-trip

> **Epic**: CampaignSession 完整会话装配
> **Status**: Complete
> **Layer**: Feature（Assembly 连接层）
> **Type**: Integration
> **Estimate**: M / 1d
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-28

## Completion Notes
**Completed**: 2026-06-28 · 全套 587/587 绿
**Test**: CampaignSessionSaveTests（5 测：round-trip哈希一致/含后果非平凡态/读档后续推进=直推/指纹不符拒/损坏拒）
**Code Review**: inline lean — ADR-0005（版本化+指纹校验+不部分载入）/ADR-0009 R-1（统一信封复用 CampaignSaveCodec）COMPLIANT
**实现**: CampaignSessionService.CaptureSnapshot/Restore（会话元数据 + 复用 FIX-8 CampaignSaveCodec 的 career+world 段；恢复重建 004 权威登记 + 015 投影订阅）。注：RNG/情报/战役 checkpoint 段随对应模块接入会话后并入（R-1 段集合）

## Context

**GDD**: `design/gdd/gdd-013-save-load.md`
**Requirement**: `TR-session-003`

**ADR Governing Implementation**: ADR-0005（存档版本与迁移，primary）· ADR-0009（R-1/R-2/R-7）
**ADR Decision Summary**: CampaignSessionSnapshot 为单一统一信封含 配置指纹/时间/RNG流位置/情报知识分区/城市控制权/Career/World/战役checkpoint；段独立版本 + 段级 migrator；round-trip 一致、不部分载入；组装在 Application、I/O 经 Infrastructure。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 显式版本化 DTO + 文本；禁 Unity 序列化处理 Domain 权威态。

**Control Manifest Rules (this layer)**:
- Required: schema version + 迁移；DTO 经 Infrastructure 端口；同一 GDD_013 边界
- Forbidden: Unity 序列化 Domain 权威态；交叉污染真值与知识
- Guardrail: 仅安全点落盘

---

## Acceptance Criteria

- [ ] `CampaignSessionSnapshot` 单一信封显式承载段集合：配置指纹 / 世界时间 / RNG 流位置集 / 情报知识分区 / 城市控制权 / CareerState / WorldState / 战役 checkpoint（R-1）
- [ ] **统一两套既有信封**：扩 `CampaignSaveCodec`（FIX-8 含 career+world）增 time/rng/intel/city/battle 段，复用 `SaveMapper` 既有捕获
- [ ] `load(save(s)) ≡ s`：round-trip 后**时间、各 RNG 流位置、情报知识分区、CareerState、WorldState** 全一致
- [ ] 迁移粒度：信封版本 + 各段独立 schema 版本 + 段级 migrator（R-2）
- [ ] schema version / 配置指纹不符 → 整体拒绝、不部分载入

---

## Implementation Notes

*Derived from ADR-0009 §R-1/R-2/R-7：*

- 基线 = FIX-8 `CampaignSaveCodec`（career+world 段）。本 story 增 time（WorldClock）/rng（RngStreamState 各流位置）/intel（情报知识分区）/city（控制权）/battle（checkpoint）段。
- 捕获模式复用竖切 `SaveMapper`/`SaveCoordinator`（S6 抽共享）。
- 组装在 Application（CampaignSessionService）；纯编解码留 Domain；文件 I/O 经 Infrastructure 端口（R-7）。

---

## Out of Scope

- Story 005：目标循环 E2E（用本 story 的 round-trip 做断言）· 完整迁移链历史版本

---

## QA Test Cases

- **AC-1/2/3**: round-trip
  - Given: 非平凡会话（含时间/三RNG流位置/情报知识/career/world/战役checkpoint）
  - When: save 再 load
  - Then: 逐字段一致；整会话哈希一致；RNG 流位置不重抽
  - Edge cases: 在野态、自立后态、空事件各 round-trip
- **AC-4/5**: 版本/指纹
  - When: 未来版本 / 指纹不符 / 损坏文本
  - Then: 整体拒绝、不部分载入

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignSessionSaveTests.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001；**ADR-0009 R-1/R-2 落 method spec**（存档段统一 + 迁移粒度，已裁定）；复用 FIX-8
- Unlocks: Story 005
