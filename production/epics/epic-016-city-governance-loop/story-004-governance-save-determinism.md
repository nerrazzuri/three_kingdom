# Story 004: 治理态存读档 round-trip + 日界确定性

> **Epic**: City Governance Loop（城市治理循环 / M03）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: S（2–3 h，纯测试）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-004-city-economy.md`
**Requirement**: `TR-city-005`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 治理态经显式版本化 DTO/JSON 存档；read-back 后账本与下一日结一致（GDD §Save/Load）；存档不中断确定性链——读档后续推进哈希等于不读档直推。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version 与迁移策略；read-back round-trip 状态一致
- Forbidden: 用 Unity 序列化处理 Domain 权威态（须显式 DTO + JSON，ADR-0005）
- Guardrail: 同种子 → 同哈希（含城市日结）

---

## Acceptance Criteria

*来自 GDD `gdd-004-city-economy.md`，作用域限本 story：*

- [ ] 治理态（库存/保留量/民心/治安/工事/守备/下一结算时间）`CaptureSnapshot → Restore` round-trip 逐字段一致
- [ ] 治理命令 + 多日推进后的非平凡态 round-trip 哈希一致
- [ ] read-back 后下一日结结果与未存档直接日结一致（GDD §Save/Load："读档 round-trip 后账本与下一日结必须一致"）
- [ ] 日界确定性：同开局 + 同治理命令流 + 同推进 → 同会话哈希
- [ ] 存档不中断确定性链：治理命令 → Advance(1) → 存读档 → Advance(1) == 直推 Advance(2)

---

## Implementation Notes

*来自 ADR-0005 / ADR-0004 实现指引：*

- 复用既有 `CampaignSessionService.CaptureSnapshot` / `Restore`（M00 统一信封）；本 story 验证城市态已正确纳入信封（Story 001 接入 + 本 story 端到端验证）。
- **本 story 零新生产代码**（前提：Story 001 已把城市态纳入存档信封映射）；若 Story 001 未完整映射，本 story 补映射。
- 参考 `CampaignSessionSaveTests` 的 round-trip 测试模式 + epic-015 story-004 的确定性链测试模式。
- 城市态哈希纳入须与 Story 001 AC-4 一致。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002/003：城市态接入 / 命令 / 战役条件派生
- 跨版本存档迁移（迁移链属 ADR-0005 既有机制，无新版本则不触发）
- 征募（移出本 epic）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 治理态 round-trip 逐字段一致
  - Given: 经若干治理命令 + Advance 的非平凡城市态 session
  - When: `Restore(CaptureSnapshot(s), Fp)`
  - Then: 库存/保留量/民心/治安/工事/守备/下一结算时间逐字段等于存档前
  - Edge cases: 短缺态（库存=FLOOR、民心已降）也完整恢复

- **AC-2**: round-trip 哈希一致
  - Given: 非平凡治理态 session，`before = ComputeHash()`
  - When: `loaded = Restore(CaptureSnapshot(s), Fp)`
  - Then: `loaded.ComputeHash() == before`
  - Edge cases: N/A

- **AC-3**: read-back 后下一日结一致（GDD §Save/Load）
  - Given: 直推 session `Advance` 一次得 `directNext`；另一 session 在 Advance 前存读档
  - When: 读档 session `Advance` 一次
  - Then: 读档后日结账本/库存与 `directNext` 一致
  - Edge cases: 含 reserved（待移交军粮）的态读档后日结仍正确移交

- **AC-4**: 日界确定性
  - Given: 两 session 同开局 + 同治理命令流
  - When: 各 `Advance(3)`
  - Then: 两者 `ComputeHash()` 相同
  - Edge cases: N/A

- **AC-5**: 存档不中断确定性链
  - Given:
    - 直推：治理命令 → `Advance(2)` → `directHash`
    - 切割：同命令 → `Advance(1)` → 存读档 → `Advance(1)`
  - When: 比较两者哈希
  - Then: 相等（存档切割点不影响后续推进确定性）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignCityGovernanceSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignCityGovernanceSaveTests.cs` — 7/7 通过（657/657 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（城市态接入存档信封）；Story 002 DONE（治理命令产生非平凡态）
- Unlocks: epic-016 完成 → M04（epic-017 情报军议装配）可开始

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing（+边界拒绝 +向后兼容副验证）
**Deviations**: 城市**配置**（settlementConfig/governanceConfig/populationPressure）数据驱动，按指纹由 Restore 调用方提供，不入存档体；存档体只存城市**态**。含城市态存档未提供配置 → 整体拒绝（SaveFormatException）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignCityGovernanceSaveTests.cs` — 7 tests, 7/7 pass
**新生产代码**: CaptureSnapshot 加城市段；Restore 加可选城市段解析 + 城市配置参数 + ParseCity
**Code Review**: 内联 — APPROVED（Lean 模式）
