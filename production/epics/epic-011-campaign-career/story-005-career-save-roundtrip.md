# Story 005: 生涯状态存档 round-trip

> **Epic**: 战役与生涯
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Integration
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-003`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0005（存档版本与迁移，primary）· ADR-0002（四层架构）
**ADR Decision Summary**: 权威状态经显式版本化 DTO + JSON、Infrastructure 端口序列化、原子写入；round-trip 一致；不用 Unity 序列化处理 Domain 权威状态。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 显式 DTO + JSON 经 Infrastructure 端口；禁 Unity JsonUtility 处理 Domain 权威态；配置指纹/schema version 校验。

**Control Manifest Rules (this layer)**:
- Required: 存档有 schema version 与迁移策略；DTO 经 Infrastructure 端口；与世界/战役同一存档边界（GDD_013）
- Forbidden: 用 Unity 序列化处理 Domain 权威状态（ADR-0005）；交叉污染世界真值与玩家知识
- Guardrail: 仅在安全点落盘

---

## Acceptance Criteria

*From GDD `gdd-014` §Save / Load Requirements / §Test Requirements，scoped to this story:*

- [ ] 保存 CareerState、RetinueState（好感快照）、RebellionState、LordMissionLog、官阶与授权
- [ ] `load(save(s)) ≡ s`：round-trip 后生涯状态逐字段一致
- [ ] 读档后继续推进与未存档直推结果一致（确定性，状态哈希一致）
- [ ] 与 GDD_015 世界态势、GDD_010 战役状态同一存档边界（GDD_013）
- [ ] schema version 与配置指纹校验；不兼容存档不部分载入当前会话

---

## Implementation Notes

*Derived from ADR-0005 / ADR-0002:*

- 为 CareerState/RetinueState/RebellionState/LordMissionLog 定义显式版本化 DTO（PascalCase + Dto 后缀），经 Infrastructure `ISaveRepository` 序列化为 JSON；原子写入（临时文件改名）。
- round-trip 测试：构造非平凡生涯态（含在野/自立快照）→ save → load → 逐字段断言相等 + 状态哈希一致。
- 读档后续推进确定性：load 后执行同一命令流，结果哈希与未存档直推一致（复用 story-001 哈希基础）。
- 与世界/战役同一存档信封：本 story 只负责生涯 DTO 段，纳入既有 GDD_013 存档边界（epic-009 已 Complete 的 SaveSerializer 信封）。
- 不兼容版本：schema version / 配置指纹不符则拒绝载入，保留上一份有效存档（ADR-0005 逆序迁移链占位）。

---

## Out of Scope

- Story 001-004：生涯状态与结算逻辑本身
- 存档信封/原子写/迁移链框架（epic-009 已 Complete，本 story 复用）
- 世界模型（epic-012）与战役（epic-007/008）各自的 DTO 段——本 story 只接生涯段进同一边界

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: round-trip 一致
  - Given: 非平凡 CareerState + RetinueState(好感) + RebellionState(快照) + LordMissionLog
  - When: save 再 load
  - Then: 逐字段 load(save(s)) ≡ s；merit/renown/lord_standing/rank/好感快照/自立分支全一致
  - Edge cases: 在野态、自立发动后态、空 LordMissionLog 各 round-trip

- **AC-3**: 读档后续推进确定性
  - Given: 同一生涯态
  - When: 路径 A 直推命令流；路径 B save→load→同一命令流
  - Then: 两路径结果状态哈希逐位一致
  - Edge cases: 含晋升/自立判定的命令流读档后不重抽、不偏移

- **AC-4 / AC-5**: 同一边界 + 版本校验
  - Given: 含世界/战役/生涯三段的存档
  - When: load
  - Then: 三段同一信封一致载入；schema version/配置指纹不符则整体拒绝、不部分载入
  - Edge cases: 未来版本号 → 拒绝并保留上一份有效存档

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/career/career_save_roundtrip_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（CareerState 骨架）必须 DONE；story 002/003/004 完成后 round-trip 覆盖面更全（可在其后实现）；复用 epic-009 存档信封
- Unlocks: None（生涯 epic MVP 存档闭环）
