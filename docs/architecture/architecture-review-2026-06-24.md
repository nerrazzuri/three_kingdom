# 架构审查报告 — 2026-06-24（聚焦 GDD_014/015 Meta 层覆盖）

> **⏫ 验证重跑（同日，coverage 模式）：裁定 CONCERNS → PASS。** ADR-0007/0008 Accepted 后复核：
> 4 个缺口 TR（world-002/004、career-004、world-003）**全部闭合**（已逐条 grep 确认两篇 ADR 的
> 「GDD Requirements Addressed」实际命名这些 TR）。Cross-ADR **无依赖环**（0007 Depends On 0002/0004/0003/0005；
> 0008 Depends On 0002；两者仅在 Related/Ordering 互引，均未列对方为 Depends On），依赖链全 Accepted，
> W1 状态所有权冲突由 ADR-0008 消解。014/015/016 **全覆盖**；整体仅余 3 个历史 cosmetic partial
> （TR-map-001/council-002/supply-001，仅缺可追溯列名，非阻断，源自 2026-06-21 复审）。详见文末「验证重跑」节。


- **日期**：2026-06-24
- **引擎**：Unity 6.3 LTS
- **范围**：GDD_014（战役与生涯）、GDD_015（条件历史世界模型）的架构覆盖缺口；GDD_016 已由 ADR-0006 覆盖，不重复。
- **ADR 基线**：ADR-0001~0006 全 Accepted。
- **背景**：承接 `design/gdd/gdd-cross-review-2026-06-24.md`（跨系统审查 CONCERNS，5 项 Warning 全落，W1 城池归属裁定 004 唯一权威）。
- **裁定**：**CONCERNS**（3 缺口，全 Meta 层，不阻断当前竖切 MVP）

> 引擎专家咨询已跳过（有据）：014/015 为纯 Domain Meta 逻辑，无 Unity API 面、无 post-cutoff API、无引擎特定决策，引擎风险 LOW。

---

## 可追溯矩阵

### GDD_014 战役与生涯

| TR-ID | 需求 | ADR 覆盖 | 状态 |
|---|---|---|---|
| TR-career-001 | CareerState（merit/renown/lord_standing/rank）为权威 Domain 状态；晋升/自立结算确定性、入状态哈希 | ADR-0002/0004 | ✅ |
| TR-career-002 | 晋升逐级门槛 + 自立三分支由配置阈值 + 好感快照确定性判定，无隐式随机 | ADR-0003/0004 | ✅ |
| TR-career-003 | 生涯状态（Career/Retinue/Rebellion/LordMission）存档 round-trip 一致 | ADR-0005 | ✅ |
| TR-career-004 | 城池归属只读；夺城/易主经 GDD_004 控制权变更事件，本层不独立写归属 | — | ❌ GAP |
| TR-career-005 | 非法操作（门槛未达申请晋升/自立）返回稳定错误码、无部分写入 | ADR-0002 | ✅ |

### GDD_015 条件历史世界模型

| TR-ID | 需求 | ADR 覆盖 | 状态 |
|---|---|---|---|
| TR-world-001 | WorldState（势力/城池归属反映/事件集合）为权威；历史推进确定性 | ADR-0002/0004 | ✅ |
| TR-world-002 | HistoricalEvent 四元组 + reachability 触发门 + 分叉下游重评估 | — | ❌ GAP |
| TR-world-003 | 城池归属为只读投影、订阅 GDD_004 控制权变更事件，世界模型不独立写 | — | ❌ GAP（同 career-004） |
| TR-world-004 | 玩家不在场势力混战用抽象结算（非完整战役）、确定性 | — | ❌ GAP |
| TR-world-005 | 配置校验拒绝缺前置/缺分叉分支的历史事件 | ADR-0003 | ✅（机制；schema 属 015） |
| TR-world-006 | WorldState + diverged 标志存档 round-trip | ADR-0005 | ✅ |

**汇总**：11 需求 — **7 ✅ covered · 0 ⚠️ partial · 3 ❌ gaps**（career-004 与 world-003 为同一契约）

---

## 覆盖缺口 → 建议 ADR

### ❌ 缺口 1：条件历史触发模型（TR-world-002 + TR-world-004）
事件四元组 `{时间窗 + 前置条件 + 正常结局 + 分叉结局}`、reachability 门、分叉传播（下游重评估）、玩家不在场的**抽象结算器**——全新架构模式，现有 ADR 无一覆盖（ADR-0004 只保证"确定性"，不定义"历史如何推进"）。
- **→ ADR-0007：条件历史世界模型架构**（含抽象结算策略）。依赖 ADR-0002/0004/0003/0005。引擎风险 LOW。

### ❌ 缺口 2：城池控制权跨系统所有权契约（TR-career-004 + TR-world-003）
W1 裁定已定（GDD_004 唯一权威 + 控制权变更事件；GDD_015 订阅只读；GDD_014 只读），已写进三份 GDD + registry，但未升为架构决策。ADR-0002 只立"Domain 是权威"总则，未定这条具体跨系统边界。
- **→ ADR-0008：城池控制权跨系统所有权契约**（轻量；裁定已成，ADR 固化）。依赖 ADR-0002，引用 GDD_004。

---

## Cross-ADR 冲突 / 依赖

- 无新冲突。ADR-0006（016）依赖 0002/0003/0004/0005 均 Accepted，无悬挂依赖、无环。
- 拟新增：ADR-0007 依赖 0002/0004/0003/0005；ADR-0008 依赖 0002 + 引用 GDD_004。两者均不形成环。

## 引擎兼容

- 014/015 纯 Domain、无引擎面；无 deprecated/post-cutoff API 问题；无 GDD 修订旗标。

## 裁定：CONCERNS

3 个缺口全在 **Meta 层**（非 Foundation/Core），**不阻断**当前竖切 MVP（竖切用 `SliceScenario` 硬编码，不需完整历史模型）。014/015 正式进实现前应补 ADR-0007（历史模型是真新架构）；缺口 2 裁定已成、固化成本低。

### 建议下一步（按基础性排序）
1. **ADR-0007** 条件历史世界模型架构（真新架构，最高优先）。
2. **ADR-0008** 城池控制权所有权契约（裁定已定，固化）。
3. 二者 Accepted 后，014/015 可由 Reviewed → Locked for Slice，进 method specs / story。
4. 重跑 `/architecture-review` 验证覆盖率升至全覆盖。

---

## 验证重跑（2026-06-24，coverage 模式）

ADR-0007 + ADR-0008 写就并 Accepted 后复核：

### 缺口闭合核验
| TR | 上轮 | 本轮覆盖 | 状态 |
|---|---|---|---|
| TR-world-002 | ❌ GAP | ADR-0007 §1/2/3（GDD Requirements Addressed 明列） | ✅ 闭合 |
| TR-world-004 | ❌ GAP | ADR-0007 §4 IAbstractResolver | ✅ 闭合 |
| TR-career-004 | ❌ GAP | ADR-0007 §5 + ADR-0008 §1/2 | ✅ 闭合 |
| TR-world-003 | ❌ GAP | ADR-0008 §1/2 | ✅ 闭合 |

4 个缺口 TR 全部闭合（逐条 grep 确认两篇 ADR 实际命名，非仅声称）。

### Cross-ADR 依赖 / 冲突
- **无环**：ADR-0007 Depends On = 0002/0004/0003/0005；ADR-0008 Depends On = 0002。两者仅在 Related/Ordering 互引，均未列对方为 Depends On。
- 所有 Depends On 目标皆 Accepted，无悬挂依赖。
- **状态所有权冲突消解**：ADR-0008 立 GDD_004 为城池归属唯一权威，ADR-0007 §5 让渡；W1 隐性双写正式解决。

### 全局覆盖
- 014/015/016 全覆盖；遗留 3 个历史 cosmetic partial（TR-map-001/council-002/supply-001，仅缺可追溯列名，非阻断）。

### 验证裁定：**PASS**（014/015 Meta 层范围）
