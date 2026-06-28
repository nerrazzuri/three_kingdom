# Story 002: 治理命令入口（征用军粮/修工事/安抚）+ 非法命令稳定错误码

> **Epic**: City Governance Loop（城市治理循环 / M03）
> **Status**: Ready
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: —

## Context

**GDD**: `design/gdd/gdd-004-city-economy.md`
**Requirement**: `TR-city-003`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0003: 数据驱动配置（secondary）
**ADR Decision Summary**: 玩家治理动作经统一会话命令入口（Command 路径）；命令前置校验失败返回稳定错误码且无部分写入（ADR-0009 R-2 命令调度）；征用/修复代价等数值来自版本化配置（ADR-0003），不在方法体内硬编码。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: gameplay state 只由 Domain 经 Application Command 路径修改；失败必须产生可继续状态
- Forbidden: UI/测试直接改城市态字段；硬编码征用/修复代价数值
- Guardrail: 命令前置校验失败 → 零应用、状态哈希不变（原子性）

---

## Acceptance Criteria

*来自 GDD `gdd-004-city-economy.md`，作用域限本 story：*

- [ ] 征用军粮命令：经会话命令路径设置 reserved（日结移交后勤 + 按 `k_morale_req` 扣民心）
- [ ] 修工事命令：经会话命令路径安排修复（日结按 `repair_rate × siege_mod` 修，受工事上限约束）
- [ ] 安抚命令：经会话命令路径提升民心（夹至 `CIV_MORALE_MAX`）
- [ ] 非法命令（库存不足/可分配量不够/重复分配/无效工事目标）→ 返回稳定错误码、拒绝命令、不扣资源（TR-city-003 + GDD §Failure Cases）
- [ ] 非法命令后 `session.ComputeHash()` 不变（原子性：零部分写入）

---

## Implementation Notes

*来自 ADR-0009 / ADR-0003 实现指引：*

- `CampaignSessionService` 新增治理命令方法，返回 `CampaignCommandResult`（与 `ResolveSiege` 一致的命令结果模式）。
- 征用走既有 reserved 机制：命令设 reserved（校验 `available ≥ req_amount`）→ 日结（Story 001 已接入）执行移交。
- 命令前置校验复用 GDD §Edge Cases / §Failure Cases 规则：`available = stock − reserved`，分配先校验 `available ≥ 需求` 否则原子拒绝。
- 错误码沿用既有 `CampaignErrorCode` 模式（参考 ResolveSiege/StartCampaign）。
- 所有代价系数（`k_morale_req`/`repair_rate`/安抚增益）来自 `CitySettlementConfig` 或扩展配置，**不硬编码**。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 001：城市态接入会话 + Advance 日结（本 story 依赖其完成）
- Story 003：治理选择改变战役条件派生
- Story 004：治理态存读档
- 征募（移出本 epic）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 征用军粮命令经会话路径
  - Given: 库存充足的 session
  - When: 提交征用命令（req_amount 合法）→ `Advance(1)`
  - Then: 命令 `Applied==true`；日结后后勤持有 +req_amount、城市库存 −req_amount、民心按 `k_morale_req×req_amount` 下降
  - Edge cases: req_amount = available（边界）→ 接受；req_amount = 0 → 接受（无副作用）

- **AC-2**: 修工事命令经会话路径
  - Given: 工事未满的 session
  - When: 提交修工事命令 → `Advance(1)`
  - Then: 工事按 `min(fort_max − fort_cur, repair_rate × siege_mod)` 增加
  - Edge cases: 工事已满 → 多余投入不转其他资源（GDD §Edge Cases）

- **AC-3**: 安抚命令经会话路径
  - Given: 民心未满的 session
  - When: 提交安抚命令
  - Then: 民心提升且夹至 `CIV_MORALE_MAX`
  - Edge cases: 民心已达上限 → 不溢出

- **AC-4**: 非法命令稳定错误码 + 无部分写入
  - Given: 库存不足以征用 req_amount 的 session；记录 `before = ComputeHash()`
  - When: 提交超额征用命令
  - Then: 返回稳定错误码（如 InsufficientStock）；`Applied==false`；命令后 `ComputeHash() == before`（零应用）
  - Edge cases: 重复分配（available 已被占）→ 同样拒绝；无效工事目标 → 拒绝

- **AC-5**: 非法命令后可继续
  - Given: 一条被拒绝的命令之后
  - When: 提交一条合法命令 + `Advance(1)`
  - Then: 合法命令正常执行（失败命令不卡死会话）
  - Edge cases: N/A

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignGovernanceCommandTests.cs` — 必须存在且全绿

**Status**: [ ] 尚未创建

---

## Dependencies

- Depends on: Story 001 DONE（城市态 + Advance 日结接入是命令的执行基底）
- Unlocks: Story 003（治理改变战役条件需命令产生差异态）
