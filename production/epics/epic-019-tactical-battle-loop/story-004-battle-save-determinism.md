# Story 004: 战斗态存读档 + 确定性

> **Epic**: Tactical Battle Loop（兵法沙盒战役循环 / M06）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-010-battle-tactics-sandbox.md`
**Requirement**: `TR-battle-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性战斗模拟（secondary）
**ADR Decision Summary**: 战斗态（快照单位 + 侦测态 + 种子）显式序列化 round-trip 一致；存档不中断确定性链——读档后续阶段解析哈希等于不读档直推。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version；战斗态 round-trip 一致
- Forbidden: 用 Unity 序列化处理 Domain 权威态（显式 DTO + 文本，ADR-0005）
- Guardrail: 同种子 → 同哈希（含战斗态）

---

## Acceptance Criteria

*来自 GDD `gdd-010-battle-tactics-sandbox.md`，作用域限本 story：*

- [ ] 战斗态（单位 force/morale/fatigue/region 等 + 侦测态 + 种子）`CaptureSnapshot → Restore` round-trip 逐字段一致
- [ ] 阶段解析后的非平凡战斗态 round-trip 哈希一致
- [ ] read-back 后续阶段解析确定性不变（读档后解析 == 直接解析）
- [ ] 存档不中断确定性链：开战 → 解析一阶段 → 存读档 → 解析一阶段 == 直推两阶段
- [ ] 含战斗态存档未提供战斗配置 → 整体拒绝（无部分载入）

---

## Implementation Notes

*来自 ADR-0005 实现指引（参考 M03/M04/M05 段存档模式）：*

- 扩展 `CaptureSnapshot`/`Restore` 加战斗段（同前模式）：序列化战斗单位 + 侦测态 + 战斗种子。
- 配置（BattleConfig/TacticChainConfig）数据驱动，按指纹由 Restore 调用方提供，不入存档体。
- `BattleUnitState` 序列化：id/faction/region/force/morale/fatigue/discipline/terrainMod/postureMod/support。
- 含战斗态存档但未提供战斗配置 → `SaveFormatException` 整体拒绝。
- 参考 epic-016/017/018 story-004 的 round-trip + 确定性链测试模式。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002/003：战斗态接入 / 阶段解析 / 兵法识别
- 跨版本存档迁移（无新版本不触发）
- 后果写回（M07 / epic-020）

---

## QA Test Cases

- **AC-1**: 战斗态 round-trip 逐字段一致
  - Given: 阶段解析后非平凡战斗态 session
  - When: `Restore(CaptureSnapshot(s), Fp, battleConfig...)`
  - Then: 各单位 force/morale/fatigue/region + 侦测态 + 种子逐字段等于存档前
  - Edge cases: 多单位混合态完整恢复

- **AC-2**: round-trip 哈希一致
  - Given: 解析后 session，`before = ComputeHash()`
  - When: `loaded = Restore(...)`
  - Then: `loaded.ComputeHash() == before`
  - Edge cases: N/A

- **AC-3**: 存档不中断确定性链
  - Given:
    - 直推：开战 → 解析阶段A → 解析阶段B → `directHash`
    - 切割：开战 → 解析阶段A → 存读档 → 解析阶段B
  - When: 比较
  - Then: 哈希相等（存档切割点不影响后续解析确定性）
  - Edge cases: N/A

- **AC-4**: 含战斗态存档未提供配置 → 整体拒绝
  - Given: 含战斗态的存档文本
  - When: `Restore(text, Fp)`（不提供 battleConfig）
  - Then: 抛 `SaveFormatException`，无部分载入
  - Edge cases: 无战斗态的存档不受影响（向后兼容）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattleSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignBattleSaveTests.cs` — 5/5 通过（723/723 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（战斗态接入信封）；Story 002 DONE（阶段解析产生非平凡态）
- Unlocks: epic-019 完成 → M07（后果与恢复循环，epic-020）可开始

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing（+向后兼容副验证）
**Deviations**: 配置（BattleConfig/TacticChainConfig）数据驱动按指纹由 Restore 提供，不入存档体；FixedPoint 用 Raw 序列化；侦测态 + 兵法条件均入存档（保证确定性链）。战斗会话同时含准备态，Restore 须同时提供准备+战斗配置。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignBattleSaveTests.cs` — 5 tests
**新生产代码**: CaptureSnapshot 加战斗段（battle/battleunit/detection/battlecond）；Restore 加战斗段解析 + battleConfig/tacticChains 参数 + ParseBattleUnit
**Code Review**: 内联 — APPROVED（Lean）
