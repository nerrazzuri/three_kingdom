# Story 002: 历史事件四元组 + reachability 触发门 + 配置校验

> **Epic**: 条件历史世界模型
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-002`（触发部分）、`TR-world-005`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0007（条件历史世界模型，primary）· ADR-0003（数据驱动配置）
**ADR Decision Summary**: 事件四元组为版本化配置；到达时间窗按 reachability 门判定——够不着则前置恒成立走正常结局，够得着且破坏前置走分叉；配置校验拒绝缺前置/缺分叉分支的事件。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#；事件定义/时间窗/前置为不可变配置（构建时转换 + 配置指纹）；定点。

**Control Manifest Rules (this layer)**:
- Required: 平衡值/事件定义数据驱动；确定性可复现；配置校验拒非法
- Forbidden: 硬编码历史事件/时间窗/前置；float 进入权威路径
- Guardrail: 够不着的事件短路（不评估前置），无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-015` §Main Rules / §Formulas 1 / §Failure Cases，scoped to this story:*

- [ ] `HistoricalEvent = {时间窗 + 前置条件谓词 + 正常结局 + 分叉结局 + 下游事件引用}`，为版本化配置
- [ ] 到达时间窗：`¬reachable` → 触发正常结局（前置恒成立，不评估前置）
- [ ] `reachable ∧ 全部前置成立` → 触发正常结局
- [ ] `reachable ∧ 前置被破坏` → 置 diverged + 触发分叉结局（下游重评估属 story-003）
- [ ] `ReachPredicate` 判定玩家势力圈是否触及事件前置主体
- [ ] 配置校验拒绝缺前置或缺分叉分支的历史事件（TR-world-005）
- [ ] MVP：单一战役框的 1-2 个带条件历史事件可端到端验证；同输入同走向（确定性）

---

## Implementation Notes

*Derived from ADR-0007 / ADR-0003:*

- `HistoricalEvent` 不可变 record（EventId、TimeWindow、Preconds、NormalOutcome、DivergenceOutcome、Downstream）；diverged 标志属 WorldState（story-001 可变态），不属事件定义。
- `on_time_window_enter`：先 `reachable(e, player)` 短路——够不着直接 fire 正常结局，不评估前置（"早期历史便宜"来源）。
- `ReachPredicate` 判定势力圈触及（已据有/灭掉相关势力或城池）。MVP 实现单一战役框够用的判定。
- 配置加载（Infrastructure，ADR-0003）校验：事件缺前置或缺分叉分支 → 拒绝载入（不得只有正常结局却允许被破坏）。
- 下游重评估（reevaluate downstream）见 story-003；本 story 只置 diverged + fire 分叉结局。

---

## Out of Scope

- Story 001：WorldState 骨架
- Story 003：分叉下游重评估（稳定序）
- Story 004：城池归属订阅（结局含 owner_change 时经 004，见 story-004）
- Story 005：抽象结算器
- 全 96 年时间线事件网络（GDD_015 §Future Scope）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-2**: 够不着 → 正常结局
  - Given: 一个历史事件，玩家势力圈未触及其前置主体
  - When: 到达时间窗
  - Then: 触发正常结局，不评估前置（reachable 短路）
  - Edge cases: 玩家圈恰在边缘未触及 → 仍走正常

- **AC-3 / AC-4**: 够得着 → 前置成立走正常 / 破坏走分叉
  - Given: 玩家势力圈触及前置主体
  - When: 前置全成立 / 前置被破坏（如已灭相关势力）两种
  - Then: 前者正常结局；后者置 diverged + 分叉结局
  - Edge cases: 时间窗内最后一次判定为准（GDD_015 Edge Case），结果确定

- **AC-6**: 配置校验
  - Given: 一个缺分叉分支（只有正常结局）的事件配置
  - When: 配置加载
  - Then: 拒绝载入，报校验错误
  - Edge cases: 缺前置谓词同样拒绝

- **AC-7**: 确定性
  - Given: 同一存档 + 同一玩家行动序列
  - When: 推进过 1-2 事件
  - Then: 同一历史走向 + 状态哈希一致

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/world/historical_event_trigger_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（WorldState 骨架）必须 DONE
- Unlocks: Story 003（分叉传播）
