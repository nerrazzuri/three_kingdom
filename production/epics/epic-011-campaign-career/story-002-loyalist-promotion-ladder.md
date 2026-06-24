# Story 002: 忠臣晋升逐级门槛与功绩/名望累积

> **Epic**: 战役与生涯
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-002`（晋升部分）
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0003（数据驱动配置，primary）· ADR-0004（确定性模拟）
**ADR Decision Summary**: 晋升门槛/名望来源权重为版本化配置，构建时转不可变并锁配置指纹；判定确定性、无隐式随机。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#；阈值数组来自配置不硬编码；定点运算。

**Control Manifest Rules (this layer)**:
- Required: 所有平衡值数据驱动；gameplay state 经 Command 路径；确定性可复现
- Forbidden: 方法体内硬编码晋升阈值/名望权重；float 进入权威路径
- Guardrail: 回合/时段制无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-014` §Formulas 1 / §Main Rules / §Test Requirements，scoped to this story:*

- [ ] `can_promote(rank→rank+1) = merit≥merit_req[rank+1] ∧ renown≥renown_req[rank+1] ∧ lord_standing≥standing_req[rank+1]`
- [ ] 三项门槛（merit/renown/lord_standing）各自独立可验：任一不足则不可晋升
- [ ] `merit_req/renown_req/standing_req` 为按阶递增的版本化配置数组（不硬编码）
- [ ] merit 由作战胜利/完成君主任务/治理城池累积；renown 由大型战役胜利/平定叛乱/招揽贤才/治理盛世累积；lord_standing 由任务完成度/战功/治理质量决定
- [ ] **非战斗来源速率护栏（W5）**：治理/任务/招揽的功绩/名望累积速率与作战来源在配置上具竞争力（防"刷战斗"支配）
- [ ] 前 2-3 阶（太守→资深太守→州刺史）晋升判定可端到端验证

---

## Implementation Notes

*Derived from ADR-0003 / ADR-0004:*

- `PromotionLadder` 保存各阶 `*_req[]` 与带兵上限/治理范围/授权，全部来自不可变配置（ADR-0003）；配置指纹纳入存档校验。
- `can_promote` 为纯函数：读 CareerState + PromotionLadder 配置，返回布尔 + 缺口明细（哪项未达），供 UI 显示"距下一阶差距"（GDD_014 §UI）。
- merit/renown 累积为具名来源事件叠加；各来源权重配置化。非战斗源权重须在配置注释标注护栏来由（W5 / 反支柱）。
- 申请晋升为 Command：门槛达标才晋级 rank；未达返回稳定错误码（story-001 路径）。
- 不实现 rank 4-7 的全权限差异（Future Scope）。

---

## Out of Scope

- Story 001：CareerState/PromotionLadder 状态骨架与 Command 路径
- Story 003：自立线（本 story 只做忠臣晋升）
- Story 004：守城事件产出的初始功绩/君主信任
- 完整 7 阶权限差异、复杂君主任务库（GDD_014 §Future Scope）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2 / AC-3**: 逐级门槛三项独立判定
  - Given: PromotionLadder 配置含 rank+1 的 merit_req/renown_req/standing_req
  - When: CareerState 仅满足其中两项（逐一缺一项）
  - Then: can_promote 返回 false，并标明缺失项；三项全满足才返回 true
  - Edge cases: 恰好等于门槛（≥ 边界）→ 通过；阈值数组按阶递增（rank+1 门槛 ≥ rank）

- **AC-4 / AC-5**: 功绩/名望多来源累积与非战斗源权重
  - Given: 配置定义作战/治理/任务/招揽各来源权重
  - When: 触发各来源事件
  - Then: merit/renown 按配置权重累加；非战斗源单位投入产出与作战源在同量级（护栏）
  - Edge cases: 权重为 0 的来源不贡献；累积为单调非负

- **AC-6**: 前 2-3 阶端到端
  - Given: 太守起步
  - When: 依次累积达标并申请晋升
  - Then: 太守→资深太守→州刺史 逐级成功；同输入同结果（确定性哈希一致）
  - Edge cases: 中途某阶门槛未达 → 停在该阶，申请返回错误码

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/career/promotion_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（CareerState/PromotionLadder 骨架 + Command 路径）必须 DONE
- Unlocks: Story 004（守城产初始功绩接入晋升）
