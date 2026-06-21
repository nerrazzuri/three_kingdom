# Story 002: 报告置信/时效/区间与确定性暴露

> **Epic**: 情报与军议
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: —

## Context

**GDD**: design/gdd/gdd-007-intelligence-recon.md
**Requirement**: TR-intel-002

**ADR Governing Implementation**: ADR-0004 确定性战斗模拟
**ADR Decision Summary**: 暴露由确定性随机流判定；报告含置信度（来源可靠性，非真实概率）、时效衰减、估计区间。

**Engine**: Unity 6.3 LTS + C# | **Risk**: LOW

**Control Manifest Rules (Core)**:
- Required: 随机性经显式流；置信用多信号（P2）
- Forbidden: 单一百分比可信度；隐式随机
- Guardrail: 同流位置 + 同输入 → 同暴露结果

---

## Acceptance Criteria

- [ ] 报告含三信号：来源可靠性（非真实概率）、时效衰减、估计区间
- [ ] 时效衰减权威归 007（003 只记 observed_time 复用 007 降级，systems-index C-W2 修复）
- [ ] 暴露由确定性随机流判定（同流位置→同结果）
- [ ] 区间随时效变宽（确定性）

---

## Implementation Notes

*Derived from ADR-0004:*
- 侦察暴露用独立注入随机流（position 可存档，epic-009）。
- 置信 = 来源类型映射（非真实命中概率）；时效 = 当前时段 − observed_time。
- 与 P2（可信度多维表达）UI 契约对齐：禁单一「75% 可信」。

---

## Out of Scope

- Story 003: 军师建议消费报告
- 序列化分离（epic-009 Story 003）

---

## QA Test Cases

- **AC-1**: 暴露确定性
  - Given: 同种子流 + 同侦察动作
  - When: 判定暴露
  - Then: 结果一致
  - Edge cases: 流位置恢复、连续侦察
- **AC-2**: 时效衰减
  - Given: 报告随时段推移
  - When: 读取
  - Then: 置信降级、区间变宽，权威来自 007
  - Edge cases: 刚观察（最高置信）、超时效（过期标记）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/intel/report_confidence_decay_test.cs` — 须存在并通过
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 001；epic-001 Story 002（随机流）；epic-002 Story 001（时段）
- Unlocks: Story 003（军师读报告）
