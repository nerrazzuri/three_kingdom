# Sprint 02 — 2026-06-24 至 2026-07-07

> **Status**: Planned
> **Sprint Goal 来源**: epic-011-campaign-career + epic-012-historical-world-model（Meta 层）
> **Manifest Version**: 1 (2026-06-21)
> **Review Mode**: lean

## Sprint Goal

落地生涯（epic-011）与条件历史世界模型（epic-012）的 **Domain 内核与标志性机制**：CareerState/WorldState 骨架 + 忠臣晋升 + 自立三分支 + 历史事件条件触发——证明 Meta 两层「个人靠抉择 × 大势随历史」的核心可确定性实现。

## 容量基线（沿用并再标定）

> 唯一 velocity 数据点来自 vertical slice（solo、headless 纯 C#）：~0.5–1.5h/系统、1–2h/条件链（含测试）。
> 本 sprint 11 story 几乎全属**纯 C# Domain Logic/Integration**，落在该包络内；Unity 接线不在内、不外推。
> **本 sprint 兼作 Meta 层容量再标定**：收尾用实际速率回填，细化里程碑日历。
> Must-Have 目标估算 ~20h。

## Capacity
- Must-Have 估算：~20h
- Buffer（20%）：~4h 预留意外
- Available（Must+Should 目标）：~33h（视 solo 实际可用工时浮动）

## Tasks

### Must Have（关键路径）
| ID | Task | Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------|-----------|-------------|--------------------|
| 11-1 | CareerState 权威骨架（Command 路径/稳定错误码/状态哈希） | gameplay-programmer | M(4h) | None（DAG 根） | TR-career-001/005；同输入同哈希、非法操作无部分写入 |
| 12-1 | WorldState 权威骨架（确定性推进） | gameplay-programmer | M(4h) | None（DAG 根） | TR-world-001；同存档+同行动序列→同世界态哈希 |
| 11-2 | 忠臣晋升逐级门槛 + 功绩/名望累积 | gameplay-programmer | M(4h) | 11-1 | TR-career-002；三项门槛独立可验、前 2-3 阶端到端、非战斗源速率护栏 |
| 11-3 | 自立触发判定 + 三分支结局 | gameplay-programmer | M(4h) | 11-1 | TR-career-002；三分支由好感快照确定性、众叛可继续 |
| 12-2 | 历史事件四元组 + reachability 门 + 配置校验 | gameplay-programmer | L(5h) | 12-1 | TR-world-002/005；够不着恒成立、破坏前置走分叉、缺分叉事件被拒 |

### Should Have
| ID | Task | Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------|-----------|-------------|--------------------|
| 11-4 | 太守开局 + 守城事件胜败后果（Integration） | gameplay-programmer | L(5h) | 11-1, 11-2；软依赖 12-1/epic-004 | TR-career-001/004；胜解锁权限、败可继续、归属经 004 事件 |
| 12-3 | 分叉传播（下游 EventId 稳定序重评估） | gameplay-programmer | M(4h) | 12-2 | TR-world-002；稳定序、脱稿深度可配置、链式有界确定性 |
| 11-5 | 生涯状态存档 round-trip（Integration） | gameplay-programmer | M(4h) | 11-1 | TR-career-003；load(save(s))≡s、读档后续推进哈希一致 |

### Nice to Have
| ID | Task | Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------|-----------|-------------|--------------------|
| 12-4 | 城池归属只读投影（订阅 004，Integration） | gameplay-programmer | M(4h) | 12-1；需 epic-004 CityControlChanged 接口 | TR-world-003；订阅同步、不独立写、并发按日界裁定 |
| 12-5 | 抽象结算器（不在场势力混战） | gameplay-programmer | M(4h) | 12-1 | TR-world-004；确定性、归属经 004、reachable 边界正确 |
| 12-6 | WorldState 存档 round-trip（Integration） | gameplay-programmer | M(4h) | 12-1 | TR-world-006；含 diverged 标志 round-trip、同一边界 |

## Carryover from Previous Sprint
| Task | Reason | New Estimate |
|------|--------|-------------|
| 无 | sprint-01（Foundation）随竖切已 Complete（epic-001~010，28+ story 全绿） | — |

## Risks
| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| epic-004 `CityControlChanged` 接口未落地 | 中 | 阻 12-4/11-4 归属路径 | 先确认接口；缺则补最小实现 story（不阻 Must-Have） |
| CitySeed（开局禀赋）权威在 epic-012，11-4 需占位 | 中 | 11-4 用占位配置 | story-004 已用最小 CitySeed 配置解耦，不硬阻断 |
| Meta 层速率无独立基线 | 低 | 估算偏差 | 本 sprint 标定 Meta 速率，收尾回填里程碑日历 |

## Dependencies on External Factors
- 既有 epic-004（城市/后勤，已 Complete）的 `CityControlChanged`/`ICityControlAuthority` 接口（ADR-0008）须可用——11-4/12-4 前置。
- 复用 epic-001 Numerics（FixedPoint/IDeterministicRandom/StateHasher）、epic-009 存档信封（均已 Complete）。

## Definition of Done for this Sprint
- [ ] 所有 Must Have 任务完成
- [ ] 所有任务过验收标准
- [ ] QA plan 存在（`production/qa/qa-plan-sprint-02.md`）
- [ ] 所有 Logic/Integration story 有通过的单元/集成测试
- [ ] Smoke check 通过（`/smoke-check sprint`）
- [ ] QA 签核报告 APPROVED 或 APPROVED WITH CONDITIONS（`/team-qa sprint`）
- [ ] 无 S1/S2 缺陷
- [ ] 偏差回写设计文档
- [ ] 代码 reviewed + merged
- [ ] 收尾回填 Meta 层容量基线

## Sprint 退出条件

所有 Must-Have story 达 Definition of Done；未通过的 story 返回 backlog 记录实际阻断，不以跳过测试或文档换取完成。收尾回填 Meta 层容量基线 → 细化里程碑日历。
