# Story 005: 抽象结算器（不在场势力混战）

> **Epic**: 条件历史世界模型
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-004`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0007（条件历史世界模型，primary）· ADR-0004（确定性模拟）
**ADR Decision Summary**: 玩家不在场的势力混战不跑完整 GDD_010 战役，由 IAbstractResolver 按势力体量/态势 + 注入随机产出结局；确定性，入哈希；不写城池归属（经 GDD_004）。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#；随机仅注入 `IDeterministicRandom`；定点。MVP 可薄实现（单一战役框少数势力）。

**Control Manifest Rules (this layer)**:
- Required: 确定性可复现（注入随机入哈希）；归属经 GDD_004（ADR-0008）
- Forbidden: 旁路随机源（System/UnityEngine.Random）；抽象结算直接写 city.owner；float 权威路径
- Guardrail: 少量势力的定点加权，省算力，无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-015` §AI Requirements / §Main Rules，scoped to this story:*

- [ ] 玩家**不在场**的势力混战用抽象结算（非完整战役），由 `IAbstractResolver` 产出结局（占据/归属/存续变化）
- [ ] 抽象结算确定性：同一输入 + 同一注入随机流位置 → 同一结局（入状态哈希）
- [ ] 玩家**够得着**范围内的敌方势力行动改由 GDD_016 战略层驱动（非抽象）——本 story 只处理够不着范围
- [ ] 抽象结算产生的归属变化经 GDD_004 控制权变更事件落地（不直接写，见 story-004）
- [ ] 脱稿/精度为可配置策略；MVP 薄实现满足单一战役框

---

## Implementation Notes

*Derived from ADR-0007 / ADR-0004:*

- `IAbstractResolver.Resolve(FactionRecord a, FactionRecord b, ContestContext ctx, IDeterministicRandom rng)` → `AbstractOutcome`。
- 随机仅经注入 `IDeterministicRandom`（复用 ADR-0004 流），种子由世界状态派生；结果入状态哈希。
- 精度只需"不出戏"，不逐单位；MVP 按势力体量/态势定点加权 + 少量随机扰动即可。
- 归属变化不直接写 WorldState：发起 GDD_004 控制权变更（story-004 路径）。
- 与 GDD_016 边界：reachable 范围内势力由 016 战略层（够不着才用本结算器）。MVP 可只覆盖够不着分支。

---

## Out of Scope

- Story 001-004
- Story 006：存档
- GDD_016 战略层势力 AI（reachable 范围；本 story 只够不着范围）
- 完整保真势力混战模拟（GDD_015 §Future Scope）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: 抽象结算确定性
  - Given: 两势力 FactionRecord + ContestContext + 注入随机流位置
  - When: Resolve 各执行一次
  - Then: 产出同一 AbstractOutcome；入状态哈希且逐位一致
  - Edge cases: 体量悬殊 → 强势力高概率占据；势均 → 随机扰动决定但可复现

- **AC-3**: 够得着 vs 够不着边界
  - Given: 一势力在玩家 reachable 范围、另一在范围外
  - When: 推进
  - Then: reachable 的走 GDD_016 战略层（不调本结算器）；够不着的走抽象结算
  - Edge cases: 边界势力按 reachable 判定一致归类

- **AC-4**: 归属经 004
  - Given: 抽象结算结局含归属变化
  - When: 应用结局
  - Then: 经 GDD_004 控制权变更落地（非直接写 city.owner）
  - Edge cases: 无归属变化的结局不触发控制权事件

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/world/abstract_resolver_test.cs` — must exist and pass
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（WorldState/FactionRecord）必须 DONE；Story 004（归属经 004 路径）建议先行
- Unlocks: None
