# Story 004: 占城归属结算（方案 C）

> **Epic**: 出征攻城循环（epic-029-offensive-campaign-loop）
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M（~4h）[待 sprint 规划确认]
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: [由 /dev-story 实现时设置]

## Context

**GDD**: `design/gdd/gdd-019-offensive-campaign.md`
**Requirement**: `TR-offensive-004`
*(需求文本以 `docs/architecture/tr-registry.yaml` 为准——审查时读最新)*

**ADR Governing Implementation**: ADR-0010（占城归属契约）
**Secondary ADRs**: ADR-0008（城池控制权唯一权威 + 变更事件）· ADR-0006（种子化确定性随机）
**ADR Decision Summary**: 控制权转移复用 ADR-0008（出征层只发起/订阅、不自写归属）；占领归属为新增确定性判定 `OwnershipVerdict{GrantToPlayer|LordKeeps}`——前 N=2 座恒归玩家，第三座起种子化伯努利取舍；LordKeeps 累积 `rebellion_lean` 喂 GDD_014 自立线；判定为纯 Domain 函数。

**Engine**: Unity 6.3 LTS + 纯 C# Domain | **Risk**: LOW
**Engine Notes**: 纯 Domain 确定性判定，无引擎面。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只经 Application Command 路径修改；平衡值数据驱动；确定性可复现。
- Forbidden: 出征层自写城池归属（须经 GDD_004 控制权变更事件，ADR-0008）；旁路随机源（只走 ADR-0006 注入流）；float/double 权威路径（p_grant 定点）；判定读敌方真值（反全知）。
- Guardrail: 同种子 + 同 conquestIndex + 同玩家合法态 → 同 OwnershipVerdict。

---

## Acceptance Criteria

*源自 GDD_019 §4 R4 / §占城C / §5 F3-F4 / §12 AC-4：*

- [ ] **AC-4a 控制权转移**：攻城胜、敌城陷落 → 经 GDD_004 控制权变更事件转移控制权；出征层只发起/订阅该事件，**不**自行改归属。
- [ ] **AC-4b 前两座归玩家**：玩家攻下的前两座城（`conquestIndex < 2`）→ 恒 `GrantToPlayer`（默认直辖）。
- [ ] **AC-4c 第三座起种子化取舍**：`conquestIndex ≥ 2` → 种子化确定性伯努利 `seed = Hash(worldTick, playerFactionId, cityId, conquestIndex)`，`p_grant = clamp(base_grant + w_renown·名望 + w_standing·君主好感 + w_value·城价值, 0, 1)`（定点），`verdict = seeded_bernoulli(seed, p_grant) ? GrantToPlayer : LordKeeps`；同种子同结果、可复现、不可预测。
- [ ] **AC-4d 反全知 + 层级**：判定只读玩家合法态（名望/好感/城价值），**不**读敌方真值；判定为纯确定性 Domain 函数（不依赖引擎、不旁路随机）。
- [ ] **AC-4e 自立倾向累积**：`LordKeeps` → 玩家得功绩/名望/赏赐但不直辖，且 `rebellion_lean += lean_per_seizure`（喂 GDD_014 自立触发条件③）。
- [ ] **AC-4f conquestIndex 递增规则**：仅在**成功占城**时递增 conquestIndex（战败未占不消耗前两座名额）。

---

## Implementation Notes

*源自 ADR-0010 D1-D5：*

- 新增纯 Domain 判定函数：输入 `conquestIndex + 玩家合法态（名望/好感/城价值）+ 注入种子` → `OwnershipVerdict`。判定不含 I/O、不读敌方真值。
- 权重 `base_grant/w_renown/w_standing/w_value` 与 `N`（默认 2）、`lean_per_seizure` 全部数据驱动（ADR-0003），`p_grant` 定点 [0,1]。
- 归属落地路由（ADR-0010 D3）：
  - `GrantToPlayer` → 控制权变更事件新控制方 = 玩家势力；城入玩家直辖清单（多城治理由 epic-025 委任，非本 story）。
  - `LordKeeps` → 新控制方 = 君主势力；玩家得回报（GDD_014）+ `rebellion_lean` 累积。
- 控制权转移必须经 GDD_004 `ControlChanged` 事件（复用 ADR-0008 接口），出征编排（Application）只发起/订阅。
- `conquestIndex` / 各城 `OwnershipVerdict` / `rebellion_lean` 为权威会话态（存档见 Story 005）。

---

## Out of Scope

*由相邻 story 处理：*

- Story 002/003：战斗与胜负产生（本 story 消费"胜利"结果）。
- Story 005：回报数值→晋升、失败续局、存档 round-trip（本 story 只产出 verdict + rebellion_lean 增量）。
- epic-025：玩家直辖多城的委任治理。
- GDD_014 自立触发**判定**本身（本 story 只累积 rebellion_lean；触发消费在 GDD_014，W1 已接线）。

---

## QA Test Cases

*lean 模式 inline 编写。*

- **AC-4a 控制权经 004**
  - Given: 出征胜、敌城陷落
  - When: 落地归属
  - Then: 控制权经 GDD_004 ControlChanged 事件转移；出征层无直接写归属路径（结构断言）
  - Edge cases: 事件发起后订阅方（015 投影）同步更新
- **AC-4b 前两座**
  - Given: conquestIndex = 0 与 1
  - When: 判定归属
  - Then: 两次均 GrantToPlayer（不进随机分支）
  - Edge cases: conquestIndex 边界 =1 仍归玩家、=2 进随机
- **AC-4c 第三座种子化**
  - Given: conquestIndex=2，固定 worldTick/faction/city + 固定玩家合法态
  - When: 判定两次（及读档后再判）
  - Then: 两次 verdict 相同（可复现）；改变种子任一分量 → 可得不同 verdict；p_grant 为定点且 clamp 到 [0,1]
  - Edge cases: p_grant=0（恒 LordKeeps）/ p_grant=1（恒 GrantToPlayer）；名望/好感/城价值极值
- **AC-4d 反全知 + 纯函数**
  - Given: 判定函数签名与实现
  - When: 反射/结构断言其输入
  - Then: 输入不含敌方真值类型；无引擎依赖、无旁路随机（只注入种子）
  - Edge cases: —
- **AC-4e 自立累积**
  - Given: 判定结果 = LordKeeps
  - When: 落地
  - Then: rebellion_lean 增加 lean_per_seizure；玩家得回报但城归君主；GrantToPlayer 时 rebellion_lean 不变
  - Edge cases: 连续多次 LordKeeps → rebellion_lean 单调累积至可触发 014 条件③阈值
- **AC-4f conquestIndex 递增**
  - Given: 一次成功占城 vs 一次战败未占
  - When: 结算后检查 conquestIndex
  - Then: 仅成功占城 +1；战败不消耗前两座名额
  - Edge cases: 占城后立即被敌反扑夺回（走守城，不回退 conquestIndex）

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Offensive/conquest_ownership_test.cs`——单元测试须存在且通过；含前两座/种子化/反全知/自立累积/递增规则断言 + 确定性哈希。
**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 003（须有确定性胜负结果驱动占城）。跨 epic：GDD_004 `ICityControlAuthority`/ControlChanged（ADR-0008，已落地）、GDD_014 rebellion_lean 消费口（W1 已接线）、GDD_015 势力投影。
- Unlocks: Story 005。
