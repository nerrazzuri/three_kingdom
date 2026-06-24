# Story 001: CareerState 权威状态与确定性结算骨架

> **Epic**: 战役与生涯
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-001`、`TR-career-005`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0002（四层架构，primary）· ADR-0004（确定性模拟）
**ADR Decision Summary**: 生涯状态落 Domain 唯一权威；玩家操作经 Command/Application Service 唯一写路径，失败返回稳定错误码、无部分写入；结算定点 + 注入随机、纳入状态哈希。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#，不依赖 UnityEngine；无 post-cutoff API。确定性须三平台同输入→同状态哈希。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只由 Domain 经 Application Command 路径修改；Domain 纯 C# 不依赖 UnityEngine；所有平衡值数据驱动
- Forbidden: UI 直接改状态；硬编码平衡数值；Domain 权威路径用 float（ADR-0004，用定点）
- Guardrail: 回合/时段制，无每帧预算压力

---

## Acceptance Criteria

*From GDD `design/gdd/gdd-014-campaign-and-career.md`（§Data Model / §Failure Cases / §Test Requirements），scoped to this story:*

- [ ] `CareerState` 持有 merit、renown、lord_standing、rank、所属势力、在野标志，为 Domain 唯一权威状态
- [ ] `RetinueState` 持有核心部曲/僚属列表及其好感（引用 GDD_006）、可任免官职位（城守/副将/内政主事/军师）的骨架
- [ ] 所有生涯状态变更经 Application Command 路径；非法操作返回稳定错误码、无部分写入
- [ ] 同一前态 + 同一命令流 → 同一生涯结算（状态哈希一致）
- [ ] merit/renown 为非负整数、lord_standing 为 [0,1] 定点（无 float 进入权威路径）

---

## Implementation Notes

*Derived from ADR-0002 / ADR-0004:*

- `CareerState` 置于 `Domain` 程序集，不引用 UnityEngine。字段用整数（merit/renown/rank）与 Q16.16 定点（lord_standing）。
- 变更经 `ICommandHandler<TCommand>`：命令先经 Application 校验（身份/时机/前置），再由 Domain 解析；失败返回 `Result` 稳定错误码，不部分写入。
- lord_standing 等定点值复用 ADR-0004 的 FixedPoint；任何随机（本 story 通常无）经注入 `IDeterministicRandom`。
- CareerState 纳入状态哈希（StateHasher），为 TR-career-001 确定性与 story-005 存档 round-trip 奠基。
- 数值阈值不在此 story 出现（晋升门槛等属 story-002，配置化）。

---

## Out of Scope

*由相邻 story 处理，本 story 不实现:*

- Story 002：晋升门槛判定与功绩/名望累积来源
- Story 003：自立触发与三分支结局
- Story 004：太守开局绑定与守城事件后果接入
- Story 005：存档 round-trip

---

## QA Test Cases

*本 story 由本技能 inline 写就（lean 模式，未派 qa-lead）。开发者据此实现，勿在实现期另造用例。*

- **AC-1 / AC-5**: CareerState 字段与类型
  - Given: 新建 CareerState
  - When: 读取字段
  - Then: 含 merit(int≥0)、renown(int≥0)、lord_standing(Q16.16∈[0,1])、rank(枚举0-7)、势力 id、在野标志；无 float 字段
  - Edge cases: lord_standing 上下越界（<0 / >1）构造被拒或钳制为合法值

- **AC-3**: 唯一写路径 + 非法操作
  - Given: 一个合法 CareerState
  - When: 经 Command 提交非法变更（如 rank 越级、merit 负增量）
  - Then: 返回稳定错误码、CareerState 不变（无部分写入）
  - Edge cases: 并发两条命令——第二条基于第一条已结算态，不读未结算值

- **AC-4**: 确定性结算
  - Given: 同一 CareerState 前态 + 同一命令流
  - When: 各执行一次
  - Then: 结算后状态哈希逐位一致
  - Edge cases: 命令流顺序变化 → 哈希不同（顺序敏感性正确）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/career/career_state_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None（复用既有 Numerics FixedPoint/StateHasher、ADR-0002 Command 框架——已在 epic-001 落地）
- Unlocks: Story 002、003、004、005
