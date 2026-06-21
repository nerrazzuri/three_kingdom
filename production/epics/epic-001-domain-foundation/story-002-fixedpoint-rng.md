# Story 002: 定点数值与确定性随机流底座

> **Epic**: 项目与 Domain 基础
> **Status**: Ready
> **Layer**: Foundation
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: 横切（支撑 TR-battle-001/003、TR-weather-001、TR-intel-002）
**Requirement**: 前置能力 — 定点 Q16.16 + 注入确定性随机流 + 状态哈希

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: Domain 权威路径用整数/定点（Q16.16），禁 float/double；随机性经显式注入的确定性随机流；状态哈希用于复现校验。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: 跨平台浮点不一致是确定性破坏源——权威结算一律定点；float 仅限非权威 Presentation/UI。

**Control Manifest Rules (Foundation)**:
- Required: 战斗结果可确定性复现；所有随机性经显式种子/预生成流
- Forbidden: Domain 权威路径使用 float/double（ADR-0004）；隐式全局随机源
- Guardrail: 同种子 + 同输入 → 同状态哈希（位级一致）

---

## Acceptance Criteria

- [ ] Fixed（Q16.16）值类型：加减乘除、比较、与 int 互转，确定性且跨平台一致
- [ ] DetRng 确定性随机流：同种子 → 同序列；流位置可读取/恢复
- [ ] 状态哈希函数：对同一 Domain 快照产生稳定哈希，字段顺序无关性明确定义
- [ ] 单元测试覆盖：定点运算边界（溢出/舍入）、随机流复现、哈希稳定性

---

## Implementation Notes

*Derived from ADR-0004:*
- Q16.16：32 位定点（高 16 整数 / 低 16 小数）；定义舍入规则（向零/最近）并测试。
- DetRng：注入式（构造传种子），暴露 position 以便存档（衔接 epic-009 TR-save-002）。
- 状态哈希：对权威字段以稳定顺序累积（FNV/xxHash 等整数哈希）；slice 已有此模式可参考重写（铁律：不 import prototype）。

---

## Out of Scope

- Story 003: 配置加载 / Story 004: SaveVersion
- 随机流位置的存档序列化 → epic-009 Story 002

---

## QA Test Cases

- **AC-1**: 定点运算确定性
  - Given: 两个 Fixed 值
  - When: 执行 +−×÷ 与比较
  - Then: 结果与预期定点值位级一致；舍入符合定义
  - Edge cases: 最大/最小值溢出、除零、负小数舍入
- **AC-2**: 随机流复现
  - Given: 两个同种子 DetRng
  - When: 各抽 N 次
  - Then: 序列完全一致；恢复 position 后续抽一致
  - Edge cases: position 跨抽取边界恢复
- **AC-3**: 状态哈希稳定
  - Given: 同一快照构造两次
  - When: 计算哈希
  - Then: 哈希相等；任一权威字段变更 → 哈希变化

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/foundation/fixedpoint_rng_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001（Domain 边界）
- Unlocks: 全部确定性结算 epic（002/004/005/006/007/008）
