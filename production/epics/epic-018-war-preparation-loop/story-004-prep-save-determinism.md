# Story 004: 准备态存读档 + 确定性

> **Epic**: War Preparation / Commitment Loop（战役准备循环 / M05）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-009-battle-preparation.md`
**Requirement**: `TR-prep-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 准备态（草稿/承诺/资源池）经显式版本化序列化 round-trip 一致；存档不中断确定性链——读档后续提交哈希等于不读档直推。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version；准备态 round-trip 一致
- Forbidden: 用 Unity 序列化处理 Domain 权威态（显式 DTO + 文本，ADR-0005）
- Guardrail: 同种子 → 同哈希（含准备态）

---

## Acceptance Criteria

*来自 GDD `gdd-009-battle-preparation.md`，作用域限本 story：*

- [ ] 准备态（资源池 + 计划草稿 + 已承诺计划）`CaptureSnapshot → Restore` round-trip 逐字段一致
- [ ] 含草稿命令（执行者/目标/时间窗/资源需求/依赖）的非平凡态 round-trip 一致
- [ ] 提交后（资源已扣减 + CommittedPlan）round-trip 哈希一致
- [ ] 存档不中断确定性链：编辑草稿 → 存读档 → 提交 == 直接编辑+提交
- [ ] 含准备态存档未提供准备配置 → 整体拒绝（无部分载入）

---

## Implementation Notes

*来自 ADR-0005 实现指引（参考 M03 城市段 / M04 情报段存档）：*

- 扩展 `CaptureSnapshot`/`Restore` 加准备段（同 M03/M04 模式）：序列化资源池 + 草稿命令 + 承诺计划。
- 配置（PreparationConfig/可达区域/授权命令）数据驱动，按指纹由 Restore 调用方提供，不入存档体（同 M03/M04）。
- `PreparedOrder` 序列化：id/executor/target/window(start,end)/resourceNeeds(键值对)/dependencies(id 列表)。
- 含准备态存档但未提供准备配置 → `SaveFormatException` 整体拒绝。
- 参考 epic-016/017 story-004 的 round-trip + 确定性链测试模式。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001/002/003：准备态接入 / 提交 / 冲突拒绝
- 跨版本存档迁移（无新版本不触发）
- 战斗结算（GDD_010 / M06）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 准备态 round-trip 逐字段一致
  - Given: 含草稿命令（多字段）的 session
  - When: `Restore(CaptureSnapshot(s), Fp, prepConfig...)`
  - Then: 资源池 + 草稿命令（id/executor/target/window/needs/deps）逐字段等于存档前
  - Edge cases: 空草稿 / 多命令草稿均一致

- **AC-2**: 提交后承诺态 round-trip
  - Given: 已 SubmitPlan（资源扣减 + CommittedPlan）的 session，`before = ComputeHash()`
  - When: `loaded = Restore(...)`
  - Then: `loaded.ComputeHash() == before`；承诺计划 + 扣减后资源池一致
  - Edge cases: N/A

- **AC-3**: 存档不中断确定性链
  - Given:
    - 直推：编辑草稿 → 提交 → `directHash`
    - 切割：编辑草稿 → 存读档 → 提交
  - When: 比较
  - Then: 哈希相等（存档切割点不影响后续提交确定性）
  - Edge cases: N/A

- **AC-4**: 含准备态存档未提供配置 → 整体拒绝
  - Given: 含准备态的存档文本
  - When: `Restore(text, Fp)`（不提供 prepConfig）
  - Then: 抛 `SaveFormatException`，无部分载入
  - Edge cases: 无准备态的存档不受影响（向后兼容）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPreparationSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignPreparationSaveTests.cs` — 5/5 通过（703/703 全绿）

---

## Dependencies

- Depends on: Story 001 DONE（准备态接入信封）；Story 002 DONE（提交产生承诺态）
- Unlocks: epic-018 完成 → M06（兵法沙盒战斗装配，消费 CommittedPlan 作战役初始条件）可开始

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing（+向后兼容副验证）
**Deviations**: 配置（PreparationConfig/可达/授权）数据驱动按指纹由 Restore 提供，不入存档体；PreparedOrder 编码 needs（key=val;）+ deps（id,）受控字符串 MVP 不转义。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignPreparationSaveTests.cs` — 5 tests
**新生产代码**: CaptureSnapshot 加准备段（prep/pool/draftorder/committed/committedorder）；Restore 加准备段解析 + prepConfig/reachable/authorized 参数 + EncodeOrder/ParseOrder/EncodeResources/DecodeResources
**Code Review**: 内联 — APPROVED（Lean）
