# Sprint 01：Foundation 地基与可测试 Domain

> **Status**: Planned（Pre-Production→Production 闸门 CONCERNS 已记录；进 Production 后开工）
> **Sprint Goal 来源**: epic-001-domain-foundation + epic-002-world-substrate（部分）
> **Manifest Version**: 1 (2026-06-21)
> **最后更新**: 2026-06-21（重生：指向 `production/epics/*/` 实际 story 文件路径，替换旧 STORY_NNN_NNN 草稿引用）

## 容量基线（首次标定）

> 唯一 velocity 数据点来自 vertical slice（solo、headless 纯 C#）：~0.5–1.5h/系统、1–2h/条件链（含测试），单会话跑通 11 系统并发 + 三链 + 存档 round-trip。
> **仅适用纯 C# Domain/Application/Infrastructure-port 逻辑工作**（本 sprint 全属此类）。
> ⚠️ Unity 接入（UI/场景/资产/序列化平台适配）速率未知，不在此基线内，不得外推（slice REPORT 明确警告）。
> **本 sprint 即作为容量基线标定 sprint**：收尾时用实际速率回填，建立粗粒度里程碑日历（PR 总监 CONCERNS 承接项）。

## Sprint Goal

建立项目地基与确定性 Domain 底座：纯 C# Domain 边界 + 测试框架跑通、定点/随机流/状态哈希、配置加载校验、SaveVersion，以及世界基底的确定性时间推进。使后续 slice 系统拥有可测试、可版本化、引擎无关的基础。

## 承诺范围（按依赖序，引用真实 story 文件）

| # | Story | 路径 | Type | 估算 | Depends on |
|---|---|---|---|---|---|
| 1 | 建立纯 C# Domain 与测试边界 | `epic-001-domain-foundation/story-001-domain-test-boundary.md` | Integration | M(4h) | None（DAG 根） |
| 2 | 定点数值与确定性随机流底座 | `epic-001-domain-foundation/story-002-fixedpoint-rng.md` | Logic | M(4h) | S1 |
| 3 | 版本化配置加载与校验 | `epic-001-domain-foundation/story-003-config-load-validate.md` | Logic | M(4h) | S1, S2 |
| 4 | SaveVersion 值对象 | `epic-001-domain-foundation/story-004-saveversion.md` | Logic | S(2h) | S1 |
| 5 | 确定性时间推进与时段/日界结算 | `epic-002-world-substrate/story-001-deterministic-time.md` | Logic | M(4h) | epic-001 S2 |

总估算 ~18h。Stretch（容量允许再取）：`epic-002-world-substrate/story-002-battle-phase-budget.md`、`story-003-weather-deterministic.md`。

## Definition of Ready（本 sprint 已满足）

- ✅ 概念、GDD 跨系统审查（G3 CONCERNS）、技术 ADR（0001~0005 全 Accepted）通过。
- ✅ 每个候选 story 内嵌 TR-ID + governing ADR + AC + QA Test Cases + 测试证据路径。
- ✅ Unity LTS、测试程序集、配置格式、存档版本规则已锁定（technical-preferences + ADR）。
- ✅ 验收标准可自动化，不依赖 gameplay UI。

## Sprint 验收

- Domain 程序集不引用 UnityEngine（asmdef 验证）。
- public API 均有契约 + 正常/边界/失败测试。
- **CI 至少一次绿灯**（`dotnet test` 纯 NUnit 旁路 Unity 许可——见进 Production 硬前置）。
- 定点/随机流/状态哈希、时间推进、配置解析确定性可复现（同种子→同结果）。
- 配置非法 → 稳定错误，无部分写入；SaveVersion 表达兼容关系。
- 未创建战斗/计策/UI/Scene gameplay 实现。

## 进 Production 硬前置（本 sprint 第一动作，S1 内闭合）

- 配置 CI 用 `dotnet test`（纯 C# NUnit）跑 Domain Logic 测试 → **首次 CI 绿灯**（旁路 UNITY_LICENSE，TD/PR 建议）。
- UNITY_LICENSE secret 仅 EditMode/PlayMode 集成测试需要，作为后续 Integration 期单独处理。
- 示例测试文件随 S1 交付（确认框架可用）。

## 风险与控制

- 若配置系统追求通用 Mod 框架 → 收缩为 slice 所需 schema 与校验。
- 若 Unity 适配渗入 Domain → 停 story 并修正程序集依赖（asmdef 无 UnityEngine 引用）。
- 若 CI 因许可受阻 → 用 `dotnet test` 旁路，Unity runner 留待集成测试期。

## Sprint 退出条件

所有承诺 story 达到 Definition of Done；未通过的 story 返回 backlog 记录实际阻断，不以跳过测试或文档换取完成。收尾回填容量基线 → 建里程碑日历。
