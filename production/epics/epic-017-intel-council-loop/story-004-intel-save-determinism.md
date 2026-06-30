# Story 004: 情报态存读档（真值/知识分别序列化不交叉污染 + 确定性）

> **Epic**: Intelligence / War Council Loop（情报与军议循环 / M04）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（3–5 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-007-intelligence-recon.md`
**Requirement**: `TR-intel-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 世界真值与玩家知识**分别序列化**，加载不得交叉污染（TR-intel-003）；情报态 round-trip 一致，读档后续侦察/军议确定性不变。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version；真值与知识分别序列化、加载不交叉污染
- Forbidden: 用 Unity 序列化处理 Domain 权威态（显式 DTO + 文本，ADR-0005）
- Guardrail: 同种子 → 同哈希（含情报态）

---

## Acceptance Criteria

*来自 GDD `gdd-007-intelligence-recon.md`，作用域限本 story：*

- [ ] 情报态（世界真值 + 玩家知识）`CaptureSnapshot → Restore` round-trip 逐字段一致
- [ ] 真值与玩家知识**分别序列化**：加载后真值含玩家未知主题、玩家知识不含该主题（不交叉污染，TR-intel-003）
- [ ] 侦察 + 军议后的非平凡情报态 round-trip 哈希一致
- [ ] read-back 后续侦察/军议确定性不变（读档后侦察 == 直接侦察）
- [ ] 含情报态存档未提供情报配置 → 整体拒绝（无部分载入）

---

## Implementation Notes

*来自 ADR-0005 实现指引（参考 M03 城市段存档 + `GameSession` 情报存档）：*

- 扩展 `CaptureSnapshot`/`Restore` 加情报段（同 M03 城市段模式）：序列化世界真值记录 + 玩家知识条目，**两段独立**。
- 配置（IntelConfig/军议配置）数据驱动，按指纹由 Restore 调用方提供，不入存档体（同 M03）。
- **不交叉污染关键**：真值段与知识段分别解析、分别重建 `WorldTruthLedger` 与 `FactionIntel`；玩家知识只来自知识段，绝不从真值段填充。
- 含情报态存档但未提供情报配置 → `SaveFormatException` 整体拒绝。
- 参考 epic-016 story-004 的 round-trip + 确定性链测试模式。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002/003：情报态接入 / 侦察 / 军议
- 跨版本存档迁移（无新版本不触发）
- 敌方知识存档（epic-021 / M08）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 情报态 round-trip 逐字段一致
  - Given: 侦察后非平凡情报态 session
  - When: `Restore(CaptureSnapshot(s), Fp, intelConfig...)`
  - Then: 真值记录 + 玩家知识条目逐字段等于存档前
  - Edge cases: 多主题混合（部分已知部分未知）完整恢复

- **AC-2**: 真值/知识分别序列化不交叉污染（TR-intel-003）
  - Given: 真值含敌方主题、玩家知识**不含**该主题的 session
  - When: round-trip
  - Then: 恢复后真值 `Has(subject)==true`、玩家知识 `Knows(subject)==false`（未被真值污染）
  - Edge cases: 玩家已知主题的估计值（含置信/时效）恢复后仍≠真值直读

- **AC-3**: round-trip 哈希一致
  - Given: 侦察 + 军议后 session，`before=ComputeHash()`
  - When: `loaded = Restore(...)`
  - Then: `loaded.ComputeHash() == before`
  - Edge cases: N/A

- **AC-4**: 存档不中断确定性链
  - Given: 直推 `Scout → Scout` 得 directHash；切割 `Scout → 存读档 → Scout`
  - When: 比较
  - Then: 哈希相等（读档后续侦察确定性不变）
  - Edge cases: 军议在读档后仍同快照同输出

- **AC-5**: 含情报态存档未提供配置 → 整体拒绝
  - Given: 含情报态的存档文本
  - When: `Restore(text, Fp)`（不提供 intelConfig）
  - Then: 抛 `SaveFormatException`，无部分载入
  - Edge cases: 无情报态的存档不受影响（向后兼容）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignIntelSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignIntelSaveTests.cs` — 6/6 通过（680/680 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（情报态接入信封）；Story 002 DONE（侦察产生非平凡态）；Story 003（军议态，可选）
- Unlocks: epic-017 完成 → M05（epic-018 战役准备装配）可开始

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 5/5 passing（+向后兼容副验证）
**Deviations**: 配置（IntelConfig/CouncilSetup）数据驱动按指纹由 Restore 调用方提供，不入存档体；真值段/知识段独立解析重建（知识只经报告路径 ApplyReport，绝不读真值段）→ 不交叉污染。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignIntelSaveTests.cs` — 6 tests
**新生产代码**: CaptureSnapshot 加情报段（intel/truth/knowledge 三类行）；Restore 加情报段解析 + intelConfig/councilSetup 参数 + ParseTruth/ParseKnowledge
**Code Review**: 内联 — APPROVED（Lean）
