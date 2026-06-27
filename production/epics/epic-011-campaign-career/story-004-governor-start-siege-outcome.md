# Story 004: 太守开局 + 守城事件胜败后果接入

> **Epic**: 战役与生涯
> **Status**: Complete
> **Layer**: Feature（Meta 连接层）
> **Type**: Integration
> **Estimate**: M / 1d（sprint-02 / sprint-status.yaml）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-27

## Context

**GDD**: `design/gdd/gdd-014-campaign-and-career.md`
**Requirement**: `TR-career-001`、`TR-career-004`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0008（城池控制权契约，primary）· ADR-0002（四层架构）
**ADR Decision Summary**: 城池归属唯一权威为 GDD_004，经 CityControlChanged 事件变更；生涯层只读归属、夺城/失城经事件发起，不直接写。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C# + Application 编排；跨系统集成（战役后果→生涯→城市控制权事件）。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 经 Command 路径；失败必须产生可继续状态；城池归属经 GDD_004 控制权变更事件（ADR-0008）
- Forbidden: 生涯层直接写 city.owner；硬编码开局禀赋数值
- Guardrail: 回合/时段制无每帧压力

---

## Acceptance Criteria

*From GDD `gdd-014` §Main Rules（开局守城事件）/ §Failure Cases / §Edge Cases，scoped to this story:*

- [ ] 太守开局：玩家绑定为某势力一城太守，开局城池禀赋（文武团队/兵力/城防/产出）来自配置（CitySeed）
- [ ] 守城开局事件（强制）触发一场 GDD_010 战役；胜 → 解锁全城权限 + 君主初始信任(lord_standing) + 初始功绩(merit)
- [ ] 败 → 失城罢官、沦为在野、保留少量核心部曲；**败局必须生成合法可继续状态**（撤退/求和/失职/流亡/投效/东山再起之一）
- [ ] 城池归属变更（守城败失城）**经 GDD_004 控制权变更事件**触发，生涯层只读归属、不独立写（ADR-0008）
- [ ] 战役胜负后果（GDD_010 BattleOutcome）确定性写回 CareerState

---

## Implementation Notes

*Derived from ADR-0008 / ADR-0002:*

- 开局绑定读 `CitySeed`（开局禀赋）。**跨 epic 依赖**：CitySeed 权威在 epic-012（GDD_015 世界模型）；本 story MVP 先用最小 CitySeed 配置占位，待 epic-012 落地后切换为世界模型来源——以配置接口隔离，不硬编码。
- 守城事件经 Application 编排：发起 GDD_010 战役 → 读 BattleOutcome → 据胜负调用生涯结算 Command。
- 失城：生涯层**不写** city.owner；而是请求 GDD_004 `ICityControlAuthority.RequestControlChange`（ADR-0008），由 004 校验+写+发 `CityControlChanged`，生涯层与世界模型订阅同步。
- 败局结算须产出合法可继续态（在野 CareerState：保留部曲、可投效/流浪），与 story-003 众叛流浪态共用最小态结构。
- 全程确定性：同一 BattleOutcome + 同一前态 → 同一 CareerState 与控制权变更（哈希一致）。

---

## Out of Scope

- Story 001/002/003：生涯状态骨架、晋升、自立判定
- Story 005：存档 round-trip
- GDD_010 战役本身的解算（既有 epic-007/008，已 Complete）——本 story 只消费 BattleOutcome
- GDD_004 控制权变更事件的**实现**（其权威在 GDD_004 / epic-004，本 story 只发起+订阅）
- CitySeed 的世界模型权威来源（epic-012 / story 待建）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1**: 太守开局绑定
  - Given: 一份最小 CitySeed 配置
  - When: 开局绑定
  - Then: CareerState 势力/城/rank=太守 + 城池禀赋（兵力/城防/产出）按 CitySeed 加载，无硬编码
  - Edge cases: CitySeed 缺字段 → 配置校验拒绝（不部分加载）

- **AC-2**: 守城胜后果
  - Given: 守城战役 BattleOutcome=胜
  - When: 生涯结算
  - Then: 解锁全城权限 + lord_standing 增初始信任 + merit 增初始功绩；城池归属不变
  - Edge cases: 同一胜利 BattleOutcome 重放 → 同一结算（幂等/确定性）

- **AC-3 / AC-4**: 守城败 + 归属经 004 事件
  - Given: 守城战役 BattleOutcome=败
  - When: 生涯结算
  - Then: CareerState 转在野(罢官/保留部曲) + 生成合法可继续态；城池归属变更经 GDD_004 CityControlChanged（生涯层未直接写 city.owner）
  - Edge cases: 失城后立即可执行下一步（投效/流浪），无卡死/非法态

- **AC-5**: 确定性写回
  - Given: 同一 BattleOutcome + 同一 CareerState 前态
  - When: 各结算一次
  - Then: 结果 CareerState 与控制权变更逐位一致
  - Edge cases: 胜/败两路径各自确定性

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Career/GovernorStartSiegeTests.cs` — must exist and pass
**Status**: [x] Created — 7 test functions, all passing（全套 520/520 绿，-warnaserror 0）
**Path note**: 集成测试落统一 NUnit 工程（无独立 integration 工程，沿 epic-001 约定）。多系统编排（CitySeed→生涯→GDD_004 控制权事件），无 I/O。

---

## Dependencies

- Depends on: Story 001（CareerState 骨架）、Story 002（晋升接收初始功绩）必须 DONE；GDD_004 控制权变更事件可用（epic-004 已 Complete，需确认 CityControlChanged 接口落地——若缺则补一最小实现 story）
- 软依赖：epic-012 CitySeed（用配置占位解耦，不硬阻断）
- Unlocks: Story 005（存档含开局/守城后果态）

---

## Completion Notes
**Completed**: 2026-06-27
**Criteria**: 5/5 passing（全部 COVERED）
**Deviations / Decisions**:
- ADVISORY — 集成测试落统一 NUnit 工程（无独立 integration 工程，沿 epic-001 约定）。
- **补缺实现**：ADR-0008 的 `ICityControlAuthority`/`CityControlChanged`/`Garrison`/`ChangeCause` 此前未落地（epic-004 已 Complete 但契约系 ADR-0008 后补）。本 story 按 story 指示「若缺则补一最小实现」在 `src/Domain/City/` 落地最小内存权威 `CityControlAuthority`（GDD_004 唯一写点）。**同时解锁 12-4（归属投影订阅）**。
- 占位 — CitySeed 为 MVP 配置占位（权威终归 epic-012 世界模型）；BattleOutcome 以最小 `SiegeOutcome{Defended,Fallen}` 摘要消费（战役解算属 epic-007/008，Out of Scope）。
- ADR-0008 分离天然满足：CareerState 无 city.owner 字段，生涯层结构上无法直接写归属；失城归属经 `RequestControlChange` 发起、`CityControlChanged` 发布。
**Test Evidence**: Integration — `tests/unit/ThreeKingdom.Domain.Tests/Career/GovernorStartSiegeTests.cs`（7 测；全套 520/520 绿，-warnaserror 0）
**Code Review**: Complete — inline lean，ADR-0008（归属唯一权威 GDD_004 + 事件）/ADR-0002（编排）/ADR-0003（CitySeed 数据驱动）COMPLIANT
**实现文件**: `src/Domain/City/`（CityControl + CityControlAuthority）· `src/Domain/Career/`（CitySeed/SiegeOutcome+GovernorStartConfig+SiegeContext/GovernorOutcomeService）· `src/Application/Career/GovernorCampaignService.cs`
