# Story 001: WorldState 权威状态与确定性推进骨架

> **Epic**: 条件历史世界模型
> **Status**: Complete
> **Layer**: Feature（Meta 连接层）
> **Type**: Logic
> **Estimate**: M / 0.5d（sprint-02 / sprint-status.yaml）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-27

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-001`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0002（四层架构，primary）· ADR-0004（确定性模拟）
**ADR Decision Summary**: 世界模型落 Domain 唯一权威；按世界时间确定性推进、纳入状态哈希；玩家经 Command 间接改变，UI 只读投影。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C#，不依赖 UnityEngine；时间唯一来自 GDD_001 WorldTime；无 post-cutoff API。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 经 Domain Command 路径；Domain 纯 C#；确定性可复现
- Forbidden: UI 直接改世界状态；硬编码；float 进入权威路径
- Guardrail: 回合/时段制无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-015` §Data Model / §Main Rules / §Test Requirements，scoped to this story:*

- [ ] `WorldState` 持有：当前时间引用（GDD_001）、各势力存续与疆域、各城归属反映与守备、已触发/已分叉事件集合
- [ ] `FactionRecord`：势力 id、君主、存续状态、领有城池、对玩家关系
- [ ] 世界推进只由世界时间前进驱动（GDD_001），确定性：同一存档 + 同一行动序列 → 同一世界态
- [ ] 同一前态推进 → 状态哈希逐位一致
- [ ] 城池归属字段为只读反映（写入权威属 GDD_004，见 story-004），本 story 只建只读结构

---

## Implementation Notes

*Derived from ADR-0002 / ADR-0004:*

- `WorldState`/`FactionRecord` 置 Domain，不引用 UnityEngine。时间引用复用 GDD_001 WorldTime（已在 epic-002 落地）。
- 城池归属在本 story 为只读投影类型（read-only），实际更新订阅留给 story-004（ADR-0008）。
- 推进纳入状态哈希（StateHasher），为 story-002 触发与 story-006 存档奠基。
- 本 story 不含历史事件触发逻辑（story-002）、不含抽象结算（story-005）。

---

## Out of Scope

- Story 002：历史事件四元组与触发门
- Story 003：分叉传播
- Story 004：城池归属订阅 GDD_004
- Story 005：抽象结算器
- Story 006：存档 round-trip

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1 / AC-2**: WorldState/FactionRecord 字段
  - Given: 新建单一历史战役框的 WorldState（少数势力 + 若干城）
  - When: 读取
  - Then: 含时间引用、势力存续/疆域、城池归属反映/守备、已触发·已分叉事件集合；FactionRecord 字段齐全
  - Edge cases: 空事件集合、单势力初态合法

- **AC-3 / AC-4**: 确定性推进
  - Given: 同一 WorldState 前态 + 同一时间推进序列
  - When: 各推进一次
  - Then: 结果世界态 + 状态哈希逐位一致
  - Edge cases: 推进顺序变化 → 哈希不同（顺序敏感正确）

- **AC-5**: 归属只读
  - Given: WorldState
  - When: 尝试直接写 city.owner
  - Then: 类型层面只读（无写 API）；归属更新须经 story-004 订阅路径
  - Edge cases: 编译级阻止直接写

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/World/WorldStateTests.cs` — must exist and pass
**Status**: [x] Created — 12 test functions, all passing（全套 477/477 绿，-warnaserror 0）
**Path note**: 实际落点为统一测试工程 `ThreeKingdom.Domain.Tests/World/`（沿 epic-001 约定），非原 story 拟的 `tests/unit/world/`（不在编译工程内）。

---

## Dependencies

- Depends on: None（复用 GDD_001 WorldTime + Numerics StateHasher，已在 epic-002 落地）
- Unlocks: Story 002、003、004、005、006

---

## Completion Notes
**Completed**: 2026-06-27
**Criteria**: 5/5 passing（无 deferred；全部 COVERED）
**Deviations**:
- ADVISORY — 测试落统一工程 `tests/unit/ThreeKingdom.Domain.Tests/World/WorldStateTests.cs`（沿 epic-001 约定），非 story 原拟 `tests/unit/world/`（不在编译工程内）。
- ADVISORY — QA AC-3/4 edge「推进顺序变化→哈希不同」：本骨架时间推进为线性可交换（GDD_001），严格非交换序敏感性随 story-002 历史事件触发到来；已改以「不同序列→不同哈希」+「输入集合序无关」证明确定性。
- 简化 — `WorldState.Cities`（城侧只读投影）与 `FactionRecord.OwnedCities`（势力侧领有）为两视图，本骨架不交叉校验一致性；权威同步随 story-004 订阅 GDD_004 接入。
**Test Evidence**: Logic — `tests/unit/ThreeKingdom.Domain.Tests/World/WorldStateTests.cs`（12 测；全套 dotnet 477/477 绿，-warnaserror 0）
**Code Review**: Complete — inline lean review，ADR-0002/0004 COMPLIANT，AC-5 归属只读编译级（无 setter + 反射断言）
**实现文件**: `src/Domain/World/`（SurvivalStatus/RelationToPlayer/CityOwnership/FactionRecord/WorldState/WorldProgressionService）
