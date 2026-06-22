# Story 001: 投影→展示模型 + UI 意图→Command 映射底座

> **Epic**: Slice UX 与可访问性
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Logic
> **Estimate**: L（6h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-22

## Context

**GDD**: `design/ux/interaction-patterns.md`（P4/P6/P10/P11）+ `docs/architecture/architecture.md` §状态变更协议
**Requirement**: Presentation 契约——只读投影 → 展示模型；UI 意图 → Command（无独立 TR slug，派生自 ADR-0002 依赖方向）

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: Presentation 仅依赖 Application API + 只读 DTO；构造意图 Command、订阅只读投影，**不**直接修改核心状态，反向依赖禁止。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: 表现逻辑为纯 C#（可 `dotnet test` / EditMode 验证），不依赖 MonoBehaviour；UXML 绑定为后续 UI story 的薄壳。

**Control Manifest Rules (Presentation)**:
- Required: UI 只展示状态并提交意图；经 Application 路径
- Forbidden: UI 直接改状态；隐藏情报泄露真值；把兵法做成无条件按钮
- Guardrail: 表现层 float 仅限非权威显示，不反写状态哈希路径

---

## Acceptance Criteria

- [x] 展示模型由 Domain **只读投影**确定性派生，不含任何写状态路径 — `EnemyIntelPanelView`/`CohesionView`/`RelationshipView`/`CouncilView`，仅 getter
- [x] UI 意图经显式映射产出 Application Command，不直接触达 Domain — `IntentTranslator` 纯 switch 映射 5 类意图→命令载荷
- [x] 敌方展示模型**只**含探报字段，**绝不**含权威真值（P10 负向断言）— 反射断言 `EnemyIntelView` 无 truth/actual/real 字段
- [x] 多维状态在展示模型中**分列字段**，无任何单一综合值（P6 负向断言）— 反射断言 `CohesionView`/`RelationshipView` 无 combined/overall/score 等
- [x] 军师/建议展示模型不含「成功率/最优解/排序最优项」字段（P11 负向断言）— 反射断言 `AdviceView`/`CouncilView` 无 success/optimal/rank 等；置信为定性标签
- [x] 同一投影 → 同一展示模型（确定性，可快照测试）— EnemyIntelPanelView 确定性排序，逐字段相等

---

## Implementation Notes

*Derived from ADR-0002:*
- 新增 `src/Presentation/`（纯 C# netstandard2.1，禁 MonoBehaviour/UnityEngine 于此逻辑层）：`ViewModels/`、`Intents/`、`Projections/`。
- 展示模型从既有只读投影构造：`Intel`（FactionIntel/IntelProjection 探报）、`Cohesion`（三维独立）、`Council`（条件化建议）、`Outcome`（因果链）、`Persistence`（LoadResult 错误态）。
- 意图→Command：定义 `IUiIntent` → `ApplicationCommand` 纯映射；不在表现层执行规则。
- 负向不变量用反射/结构断言固化（同 Domain 既有做法）：敌方 VM 无真值字段、建议 VM 无成功率字段、cohesion VM 无合并值。

---

## Out of Scope

- Story 002–004：各屏 UXML/USS 视觉外壳
- Story 005：无障碍设置模型与持久

---

## QA Test Cases

- **AC-1/AC-6**: 投影→展示模型确定性
  - Given: 一份固定 Domain 只读投影
  - When: 构造展示模型两次
  - Then: 两次字段逐一相等（确定性）；无任何 setter 暴露写状态
- **AC-3**: 敌方无真值泄露（P10）
  - Given: 含世界真值 + 阵营探报的情报投影
  - When: 构造敌方展示模型
  - Then: 反射断言展示模型类型无映射到真值的字段；只含推测/区间/时效/来源
  - Edge cases: 真值与探报同名字段不串台
- **AC-4**: 多维不合并（P6）
  - Given: cohesion 三维 + 关系四维
  - When: 构造对应展示模型
  - Then: 各维独立字段存在，无「综合士气」「综合好感」单值
- **AC-5**: 无最优解（P11）
  - Given: 军师条件化建议集
  - When: 构造建议展示模型
  - Then: 反射断言无 successRate/optimal/ranking 字段；候选不排优劣
- **AC-2**: 意图→Command 映射
  - Given: 各 UI 意图
  - When: 映射
  - Then: 产出对应 Application Command 载荷；表现层不含规则求解

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/ThreeKingdom.Domain.Tests/Presentation/{PresentationViewTests,PresentationLockTests,IntentTranslationTests}.cs` — 16 测全通过（BLOCKING）
**Status**: [x] Passed — 345/345 全绿，`-warnaserror` 0 warning
**ADVISORY note**: AdviceView 置信定性分档阈值（0.34/0.67）为展示阈值（非 gameplay 平衡值），未来可移入展示配置。

---

## Dependencies

- Depends on: epic-005（Intel/Council 投影）、epic-007（Cohesion）、epic-008（Outcome 因果链）、epic-009（LoadResult）
- Unlocks: Story 002–005（各屏复用展示模型与意图映射）
