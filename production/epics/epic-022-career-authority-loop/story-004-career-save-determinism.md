# Story 004: 生涯权限态存读档 + 确定性（晋升/自立后 round-trip）

> **Epic**: Career / Authority Loop（生涯与权限循环 / M09）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: S（2–3 h，纯测试为主）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-003`、`TR-career-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0005: 存档版本与迁移（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 生涯权限态（晋升后 Rank / 自立后 Faction/IsUnaffiliated / merit/renown）随会话存档 round-trip 一致（复用既有 career 段）；同输入同哈希。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 存档有 schema version；生涯态 round-trip 一致
- Forbidden: 用 Unity 序列化处理 Domain 权威态（ADR-0005）
- Guardrail: 同输入 → 同哈希（含生涯权限态）

---

## Acceptance Criteria

- [ ] 功绩累积后生涯态（merit/renown）`CaptureSnapshot → Restore` round-trip 一致
- [ ] 晋升后官阶（Rank）round-trip 一致
- [ ] 自立后归属（Faction/IsUnaffiliated）round-trip 一致
- [ ] round-trip 哈希一致
- [ ] 存档不中断确定性链：功绩→晋升→存读档后会话态等于直推

---

## Implementation Notes

- 生涯态已在会话存档 career 段（M00 CaptureSnapshot）；本 story 验证 M09 命令产生的非平凡生涯态 round-trip。
- 复用既有 career 段；无需新存档段（晋升/自立结果在 CareerState：Rank/Faction/IsUnaffiliated）。
- 参考 epic-015/016 story-004 的 round-trip + 确定性链测试模式。

---

## Out of Scope

- Story 001/002/003：功绩/晋升/自立
- RebellionState/LordMissionLog 独立态完整持久化（MVP career 态反映结果，留后续）
- 跨版本存档迁移

---

## QA Test Cases

- **AC-1**: 功绩态 round-trip
  - Given: 累积功绩后的会话
  - When: `Restore(CaptureSnapshot(s), Fp)`
  - Then: merit/renown 逐字段一致
  - Edge cases: N/A

- **AC-2**: 晋升后官阶 round-trip
  - Given: 晋升后会话（Rank 提升）
  - When: round-trip
  - Then: Rank 一致
  - Edge cases: N/A

- **AC-3**: 自立后归属 round-trip
  - Given: 自立后会话（Faction 变 / IsUnaffiliated）
  - When: round-trip
  - Then: Faction/IsUnaffiliated 一致
  - Edge cases: N/A

- **AC-4**: round-trip 哈希一致
  - Given: 非平凡生涯态会话，`before=ComputeHash()`
  - When: `loaded=Restore(...)`
  - Then: `loaded.ComputeHash()==before`
  - Edge cases: N/A

- **AC-5**: 存档不中断确定性链
  - Given: 直推 功绩→晋升 得 directHash；切割 功绩→存读档→晋升
  - When: 比较
  - Then: 哈希相等
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignCareerSaveTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignCareerSaveTests.cs` — 5/5 通过（782/782 全绿）

---

## Dependencies

- Depends on: Story 001/002 DONE（功绩/晋升产生非平凡态）；Story 003（自立态）
- Unlocks: epic-022 完成 → M10（历史世界势力，epic-023）

---

## Completion Notes
**Completed**: 2026-06-30
**Deviations**: M09 轻量装配——命令接受配置参数（数据驱动，同 ResolveSiege 模式）；career 态已在存档 career 段，无新存档代码。
**Code Review**: 内联 — APPROVED（Lean）
