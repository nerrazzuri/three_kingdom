# Story 006: WorldState 存档 round-trip

> **Epic**: 条件历史世界模型
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Integration
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-006`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0005（存档版本与迁移，primary）· ADR-0002（四层架构）
**ADR Decision Summary**: 权威世界状态经显式版本化 DTO + JSON、Infrastructure 端口序列化、原子写入；round-trip 一致；与生涯/战役同一存档边界；不用 Unity 序列化处理 Domain 权威态。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 显式 DTO + JSON 经 Infrastructure 端口；禁 Unity JsonUtility 处理 Domain 权威态；schema version / 配置指纹校验。

**Control Manifest Rules (this layer)**:
- Required: 存档有 schema version 与迁移；DTO 经 Infrastructure 端口；与生涯/战役同一边界（GDD_013）
- Forbidden: 用 Unity 序列化处理 Domain 权威状态（ADR-0005）
- Guardrail: 仅在安全点落盘

---

## Acceptance Criteria

*From GDD `gdd-015` §Save / Load Requirements / §Test Requirements，scoped to this story:*

- [ ] 保存 WorldState（时间、势力存续、城池归属反映、已触发/已分叉事件集合）+ HistoricalEvent diverged 标志
- [ ] `load(save(s)) ≡ s`：round-trip 后世界态逐字段一致（含 diverged 标志集合）
- [ ] 读档后续推进与未存档直推结果一致（确定性，状态哈希一致）
- [ ] 与 GDD_014 生涯、GDD_010 战役同一存档边界（GDD_013）
- [ ] schema version / 配置指纹校验；不兼容存档不部分载入

---

## Implementation Notes

*Derived from ADR-0005 / ADR-0002:*

- 为 WorldState（含 FactionRecord、城池归属投影、事件集合、diverged 标志）定义显式版本化 DTO，经 Infrastructure `ISaveRepository` 序列化 JSON；原子写。
- round-trip 测试：构造含已分叉事件的非平凡世界态 → save → load → 逐字段断言相等 + 哈希一致。
- 读档后续推进确定性：load 后执行同一时间推进/行动序列，结果哈希与未存档直推一致（复用 story-001 哈希）。
- 同一存档信封：世界段纳入既有 GDD_013 边界（epic-009 SaveSerializer 信封，与生涯 story-005、战役共存）。
- 不兼容版本：schema version / 配置指纹不符则拒绝载入，保留上一份有效存档。

---

## Out of Scope

- Story 001-005：世界状态与逻辑本身
- 存档信封/原子写/迁移链框架（epic-009 已 Complete，本 story 复用）
- 生涯（epic-011）与战役各自 DTO 段——本 story 只接世界段进同一边界

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: round-trip 一致
  - Given: 含已触发/已分叉事件、城池归属、势力存续的非平凡 WorldState
  - When: save 再 load
  - Then: 逐字段 load(save(s)) ≡ s；diverged 标志集合一致
  - Edge cases: 空事件集合、含多个 diverged 的态各 round-trip

- **AC-3**: 读档后续推进确定性
  - Given: 同一世界态
  - When: 路径 A 直推时间序列；路径 B save→load→同一序列
  - Then: 两路径世界态哈希逐位一致
  - Edge cases: 含历史事件触发的推进读档后不重抽、不偏移

- **AC-4 / AC-5**: 同一边界 + 版本校验
  - Given: 含世界/生涯/战役三段的存档
  - When: load
  - Then: 三段同一信封一致载入；schema version/配置指纹不符则整体拒绝
  - Edge cases: 未来版本号 → 拒绝并保留上一份有效存档

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/world/worldstate_save_roundtrip_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（WorldState 骨架）必须 DONE；story 002/003 完成后 round-trip 覆盖 diverged 更全；复用 epic-009 存档信封
- Unlocks: None（世界模型 epic MVP 存档闭环）
