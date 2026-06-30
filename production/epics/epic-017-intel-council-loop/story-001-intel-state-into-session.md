# Story 001: 情报态接入会话（真值 + 阵营知识，反全知只读投影）

> **Epic**: Intelligence / War Council Loop（情报与军议循环 / M04）
> **Status**: Complete
> **Layer**: Feature（含 Assembly 装配）
> **Type**: Integration
> **Estimate**: M（4–6 h，含新生产代码）
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-06-30

## Context

**GDD**: `design/gdd/gdd-007-intelligence-recon.md`
**Requirement**: `TR-intel-001`
*（需求原文见 `docs/architecture/tr-registry.yaml`）*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）；ADR-0004: 确定性（secondary）
**ADR Decision Summary**: 装配层只编排——会话持世界真值（WorldTruthLedger）+ 玩家阵营知识（FactionIntel），但**只读暴露阵营知识投影**，绝不暴露真值（四层分离，反全知）。复用既有 Domain 类型，不重写情报逻辑。

**Engine**: Unity 6.3 LTS | **Risk**: LOW
**Engine Notes**: 纯 C# Domain + NUnit，不调用 UnityEngine。

**Control Manifest Rules (Feature/Assembly 层)**:
- Required: 状态变更经 Application 路径；UI/只读出口仅阵营知识（反全知）
- Forbidden: 向 Presentation/只读投影暴露 WorldTruth 真值；硬编码情报配置
- Guardrail: 情报态确定性（纳入会话哈希）

---

## Acceptance Criteria

*来自 GDD `gdd-007-intelligence-recon.md`，作用域限本 story：*

- [ ] `CampaignSession` 持有 `WorldTruthLedger`（世界真值）+ 玩家 `FactionIntel`（阵营知识）+ `IntelConfig`
- [ ] 会话只读暴露玩家阵营知识投影（`IntelProjection`），**不暴露** `WorldTruthLedger` 真值（TR-intel-001 反全知）
- [ ] 情报态纳入 `session.ComputeHash()`
- [ ] 情报态可选（场景未启用情报时为 null，向后兼容现有测试）
- [ ] 开局配置驱动情报态（真值记录 + 初始已知知识）

---

## Implementation Notes

*来自 ADR-0009 实现指引（参考 `GameSession` 既有接入模式）：*

- 复用 `WorldTruthLedger`/`FactionIntel`/`IntelConfig`（epic-005 已实装）。
- 会话新增可选字段：`WorldTruthLedger? _truth`、`FactionIntel? _playerIntel`、`IntelConfig?`、玩家势力 id。
- 只读出口：`public IntelProjection? PlayerKnowledge => _playerIntel?.Project();`（**不**暴露 `_truth`）。
- `_truth` 仅 internal（供 S002 侦察、S004 存档），绝不进只读 public 出口。
- ComputeHash 追加情报态（真值 + 知识，确定性顺序）；情报态 null 则跳过（向后兼容）。
- 情报态进 `CampaignStartConfig` 可选参数（同 M03 城市态模式）。

---

## Out of Scope

*由邻近 story 处理——本 story 不实现：*

- Story 002：侦察命令经会话路径
- Story 003：军议快照 + 知识变化建议过时
- Story 004：情报态存读档
- 敌方 AI 读知识（epic-021 / M08）

---

## QA Test Cases

*以下测试已在 Story 创建时规划，开发者对照实现，不得另行发明测试用例。*

- **AC-1**: 会话持有情报态
  - Given: `StartCampaign(config)`（config 含真值 + 初始知识 + IntelConfig）
  - When: 读会话情报态
  - Then: `HasIntel == true`；玩家知识投影含开局已知主题
  - Edge cases: 缺 IntelConfig 但有真值 → 开局拒绝（无部分初始化）

- **AC-2**: 只读出口不暴露真值（反全知）
  - Given: 真值含敌方真实兵力、玩家知识不含该主题
  - When: 读 `session.PlayerKnowledge`
  - Then: 投影只含玩家已知主题；无任何 public 出口可读 `WorldTruthLedger`
  - Edge cases: 玩家未侦察的主题在投影中 `Knows==false`

- **AC-3**: 情报态纳入会话哈希
  - Given: 两 session 除真值某主题兵力外相同
  - When: 各 `ComputeHash()`
  - Then: 哈希不同（情报态进哈希）
  - Edge cases: 情报态相同 → 哈希相同

- **AC-4**: 无情报配置的会话向后兼容
  - Given: 旧式 config（不传情报态）
  - When: `StartCampaign`
  - Then: `HasIntel == false`；`PlayerKnowledge == null`；现有测试不受影响

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignIntelStateTests.cs` — 必须存在且全绿

**Status**: [x] `CampaignIntelStateTests.cs` — 6/6 通过（680/680 全绿）

---

## Dependencies

- Depends on: epic-013（M00）+ epic-016（M03）Complete（已满足）；epic-005 情报 Domain 内核（已 Complete）
- Unlocks: Story 002（侦察命令）、Story 004（情报存读档）

---

## Completion Notes
**Completed**: 2026-06-30
**Criteria**: 4/4 passing
**Deviations**: 情报态可选（nullable）向后兼容现有测试；真值仅 internal、public 只读出口仅 `PlayerKnowledge` 投影（反全知）。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignIntelStateTests.cs` — 6 tests
**新生产代码**: WorldTruthLedger.Records（只读枚举）；CampaignSession 持真值+知识+IntelConfig、PlayerKnowledge 只读出口、ComputeHash 含情报（AppendIntel）；CampaignStartConfig 加情报参数
**Code Review**: 内联 — APPROVED（Lean）
