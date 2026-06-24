# Story 003: 分叉传播（下游按 EventId 稳定序重评估）

> **Epic**: 条件历史世界模型
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-002`（分叉传播部分）
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0007（条件历史世界模型，primary）· ADR-0004（确定性模拟）
**ADR Decision Summary**: 事件分叉后，依赖它的下游事件按 EventId 稳定序重检前置，避免顺序不确定；脱稿范围默认仅玩家势力圈（可配置深度）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#；稳定排序确定性；脱稿深度 `divergence_spread_depth` 配置化。

**Control Manifest Rules (this layer)**:
- Required: 确定性可复现（稳定序）；脱稿深度数据驱动
- Forbidden: 依赖非稳定顺序（如哈希表遍历序）；硬编码脱稿深度
- Guardrail: 默认仅玩家圈脱稿，避免全图涟漪

---

## Acceptance Criteria

*From GDD `gdd-015` §Main Rules / §Formulas 1 / §Edge Cases，scoped to this story:*

- [ ] 事件分叉后，`reevaluate(downstream(e))` 按 **EventId 稳定序**遍历下游事件、重检其前置
- [ ] 多个下游依赖同一被分叉事件时，按事件 id 稳定序重评估，结果确定（GDD_015 Edge Case）
- [ ] 脱稿范围默认只玩家势力圈；`divergence_spread_depth`（默认 0）控制涟漪深度，配置化
- [ ] 重评估自身可能再触发下游分叉，链式传播仍确定性、可终止（无环/有界）

---

## Implementation Notes

*Derived from ADR-0007 / ADR-0004:*

- `reevaluate(downstream)` 按 EventId 升序（稳定 ID 序）遍历，逐个重检前置——这是确定性的关键，禁用字典/集合的非稳定遍历序。
- 脱稿深度：`divergence_spread_depth` 限制传播跳数（默认 0 = 仅玩家圈直接相关事件）；超出深度的远方事件按历史/抽象推进，不重评估。
- 链式传播：重评估若使某下游也走分叉，继续按规则传播；以 depth 上限与"已处理集合"保证有界、可终止。
- 本 story 依赖 story-002 的触发与 diverged 置标；只新增"下游重评估"环。

---

## Out of Scope

- Story 002：事件触发与首次分叉（本 story 处理其下游连锁）
- Story 004/005/006
- 全图架空史推进（GDD_015 §Future Scope，本 MVP 默认仅玩家圈）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: 下游稳定序重评估
  - Given: 事件 A 分叉，下游 {B, C} 依赖 A（前置含 A 的结局）
  - When: reevaluate(downstream(A))
  - Then: 按 EventId 稳定序重检 B、C 前置；多次运行顺序与结果一致
  - Edge cases: B、C 乱序输入仍按 id 序处理；无下游时 no-op

- **AC-3**: 脱稿深度
  - Given: divergence_spread_depth=0（默认）
  - When: 玩家圈事件分叉
  - Then: 仅玩家圈直接相关下游重评估；远方事件不受影响
  - Edge cases: depth=1 时涟漪扩一跳；配置缺省回落默认 0

- **AC-4**: 链式有界确定性
  - Given: 分叉连锁（A→B→C 均可能再分叉）
  - When: 传播
  - Then: 有界终止、无环、同输入同结果（哈希一致）
  - Edge cases: 自引用/循环依赖配置被拒或安全终止

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/world/divergence_propagation_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 002（事件触发 + diverged 置标）必须 DONE
- Unlocks: None（触发模型 MVP 闭环）
