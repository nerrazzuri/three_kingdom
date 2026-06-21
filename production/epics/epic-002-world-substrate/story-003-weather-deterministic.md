# Story 003: 配置驱动天气/风向确定性解析

> **Epic**: 世界基底（时间·环境·地图拓扑）
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: design/gdd/gdd-002-season-weather.md
**Requirement**: TR-weather-001、TR-weather-002

**ADR Governing Implementation**: ADR-0004（secondary ADR-0003）
**ADR Decision Summary**: 天气转移由配置权重 + 显式确定性随机流；环境只产出具名修正。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH

**Control Manifest Rules (Foundation)**:
- Required: 随机性经显式注入流；具名修正供消费者自取
- Forbidden: 天气直接触发计策成功；隐式随机源
- Guardrail: 同流位置 + 同前态 → 同天气结果

---

## Acceptance Criteria

- [ ] 天气转移由配置权重 + 注入确定性随机流驱动
- [ ] 同随机流位置 + 同前态 → 同结果（确定性）
- [ ] 环境产出具名修正（移动/侦察/隐蔽/疲劳/补给各自取），不直接触发计策
- [ ] 天气只减速 ≥1.0（对齐 003 地图修正；地形保留 0.5–2.0，systems-index C-W3 修复）
- [ ] 破环顺序：地形（静态）→ 天气 → 地图通行（002↔003）

---

## Implementation Notes

*Derived from ADR-0004 + systems-index 破环:*
- 天气随机流注入（epic-001 DetRng），独立于其他随机流，position 可存档。
- 具名修正以只读结构暴露；消费者在各自结算时读取已结算修正。
- 天气先读地形派生道路，地图通行耗时再读天气已结算修正（不互读未结算值）。

---

## Out of Scope

- 各消费者如何使用修正（epic-004/005/007）
- 地图寻路本体 → Story 004

---

## QA Test Cases

- **AC-1**: 天气确定性
  - Given: 同种子流 + 同前态
  - When: 解析下一天气
  - Then: 结果一致；流位置一致前进
  - Edge cases: 权重和为 0/单一权重、流位置恢复后续一致
- **AC-2**: 修正范围与方向
  - Given: 各天气态
  - When: 取移动修正
  - Then: 天气修正 ≥1.0（只减速）；不直接产出「计策成功」
  - Edge cases: 极端天气上限夹取

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Environment/WeatherDeterministicTests.cs` — 须存在并通过
**Status**: [x] 已创建并通过（13 测，全套 124/124 绿）
**Note**: 路径由故事原写的 `tests/unit/weather/weather_deterministic_test.cs` 归一到真实可编译测试工程。

---

## Dependencies

- Depends on: Story 001（时间）、epic-001 Story 002/003（随机流/配置）
- Unlocks: epic-004（补给）、epic-007（战斗读天气修正）

---

## Completion Notes
**Completed**: 2026-06-22
**Criteria**: 5/5 passing（配置权重+注入流转移、同流位置+同前态→同结果、具名修正供 5 消费者自取不触发计策、天气只减速≥1.0+极端夹取、破环以已结算只读修正暴露供地图通行后读）
**Files**: `src/Domain/Environment/`（WeatherType、Wind+WindDirection、WeatherTransitionTable、WeatherResolver、EnvironmentModifierSet+EnvironmentConsumer+EnvironmentModifier）+ `tests/unit/ThreeKingdom.Domain.Tests/Environment/WeatherDeterministicTests.cs`（13 测）
**Deviations**: ADVISORY — 测试路径归一到真实测试工程；AC-5 破环顺序在本 story 以「天气产出已结算只读修正集」结构性保证，跨 story 通行读取在 Story 004 落地。
**Test Evidence**: Logic — 测试文件存在且通过（全套 124/124 绿，`-warnaserror` 0 warning）
**Code Review**: Complete — `/code-review` = APPROVED（定点累积加权选择、续抽一致、权重校验、Movement≥1.0、无计策消费者）
