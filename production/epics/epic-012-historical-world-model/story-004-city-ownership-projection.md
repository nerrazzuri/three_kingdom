# Story 004: 城池归属只读投影（订阅 GDD_004 控制权变更）

> **Epic**: 条件历史世界模型
> **Status**: Ready
> **Layer**: Feature（Meta 连接层）
> **Type**: Integration
> **Estimate**: [待 sprint 规划填]
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-015-historical-world-model.md`
**Requirement**: `TR-world-003`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准，审查时读最新)*

**ADR Governing Implementation**: ADR-0008（城池控制权契约，primary）· ADR-0007（世界模型）
**ADR Decision Summary**: 城池归属唯一权威为 GDD_004（独占 CityControlChanged 事件）；世界模型在战略尺度只读反映、订阅该事件同步，不独立写；历史事件结局的 owner_change 经 GDD_004 落地后回流。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW
**Engine Notes**: 纯 Domain C# + 事件订阅；跨系统集成（GDD_004 控制权事件 → 世界模型投影）。

**Control Manifest Rules (this layer)**:
- Required: 城池归属经 GDD_004 控制权变更事件（ADR-0008）；世界模型只读
- Forbidden: 世界模型直接写 city.owner；并发写归属
- Guardrail: 单点事件订阅，无每帧成本

---

## Acceptance Criteria

*From GDD `gdd-015` §Formulas 2 / §Main Rules / §Edge Cases，scoped to this story:*

- [ ] 世界模型订阅 GDD_004 `CityControlChanged(city, newOwner, garrison)` 事件，同步 `world.city[c].owner/garrison` 只读投影
- [ ] 世界模型**不独立写** city.owner——历史事件结局的 owner_change 经 GDD_004 控制权变更发起，再回流至此
- [ ] 历史事件与玩家局部战役同争一城时，按 GDD_001 日界全局结算顺序裁定，不并发写（GDD_015 Edge Case）
- [ ] 城池归属投影与 GDD_004 权威态最终一致

---

## Implementation Notes

*Derived from ADR-0008 / ADR-0007:*

- 世界模型注册为 `CityControlChanged` 订阅者；事件到达时更新只读归属投影（story-001 的只读结构）。
- 历史事件结局含 owner_change 时：不直接写 WorldState，而是发起 GDD_004 `ICityControlAuthority.RequestControlChange`（ADR-0008），由 004 校验+写+发事件，世界模型订阅回流。
- 并发裁定复用 GDD_001 日界全局结算顺序（systems-index §跨系统结算顺序）——同城争夺由 004 单点按序结算。
- **跨 epic 依赖**：GDD_004 控制权变更事件权威在 epic-004（已 Complete）；需确认 `CityControlChanged`/`ICityControlAuthority` 接口已落地，若缺则先补一最小实现。

---

## Out of Scope

- Story 001/002/003：WorldState 骨架与历史触发
- Story 005/006
- GDD_004 控制权变更事件的**实现**（权威属 epic-004，本 story 只订阅 + 发起）

---

## QA Test Cases

*lean 模式 inline 写就。*

- **AC-1**: 订阅同步
  - Given: 世界模型订阅 CityControlChanged
  - When: GDD_004 发布某城控制权变更
  - Then: world.city[c].owner/garrison 投影同步更新为新值
  - Edge cases: 连续多次变更按事件序最终一致

- **AC-2**: 不独立写 + 结局经 004
  - Given: 一个含 owner_change 的历史事件分叉结局
  - When: 触发该结局
  - Then: 世界模型发起 GDD_004 控制权变更（非直接写 city.owner）；归属经 004 回流
  - Edge cases: 世界模型无直接写归属 API（编译级）

- **AC-3**: 并发裁定
  - Given: 历史事件与玩家战役同争一城
  - When: 同一日界结算
  - Then: 按 GDD_001 全局顺序由 004 单点裁定，无并发写、结果确定
  - Edge cases: 两来源同目标 → 单一最终归属，可复现

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/world/city_ownership_projection_test.cs` OR 文档化 playtest
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（WorldState 只读归属结构）必须 DONE；GDD_004 CityControlChanged 接口可用（epic-004 已 Complete，需确认接口落地，缺则补最小实现）
- Unlocks: None
