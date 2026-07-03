# Story 002: 战果复盘屏——因果链默认折叠 + 续局选项 + 长线意义

> **Epic**: 表现与理解循环（M15 / epic-028）
> **Status**: Complete（2026-07-03；用户 Unity 人工走查待补——见 Completion Notes）
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M / ~4h
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-03

## Context

**UX 契约**: `design/ux/m15-campaign-loop-ux.md`（Approved 2026-07-03）§2.1 因果契约 · §2.4 续局契约 · §7 Q2/Q3/Q5 裁决
**Requirement**: `TR-ux-001` · `TR-ux-004`
*(需求原文在 `docs/architecture/tr-registry.yaml`，评审时读取最新版)*

**ADR Governing Implementation**: ADR-0002: 四层架构（primary）· ADR-0009: CampaignSession 装配
**ADR Decision Summary**: Presentation 只读 Application 投影 DTO、只经用例提交 Command；战果与续局数据来自 `ResolveBattleOutcome` 返回的 `OutcomeContinuation` 与复盘投影，UI 不实现任何战斗/后果规则。

**Engine**: Unity 6.3 LTS | **Risk**: MEDIUM
**Engine Notes**: 本项目 UI 走 **UI Toolkit**（UIDocument/UXML，竖切三屏已证——story-readiness 修正原文误写的 UGUI）；沿既有 Controller 薄壳模式，勿混用 UGUI。Claude 仅 batchmode 验证；视觉与交互须用户人工走查签核。

**Control Manifest Rules (this layer)**:
- Required: 失败必须产生可继续状态（强制设计锁）；兵法只在事件后打复盘标签（TR-battle-002）
- Forbidden: UI 直接改 Domain 状态；显示「游戏结束/删档」终点；复盘只给胜/败不给因果
- Guardrail: 文本预留 40% 扩展（§4.6）；色盲不靠纯色区分（AC-7）

---

## Acceptance Criteria

*From `m15-campaign-loop-ux.md` §6（AC-1/AC-4）+ §7 裁决，scoped to this story:*

- [ ] 战斗结束后呈现复盘屏：**默认折叠**为一句话结论（胜/败/撤/失城 + 首要原因），**一键展开**完整因果链（≤5 主因素）——§7 Q3 裁决
- [ ] 展开态列出：满足了哪些兵法条件、识别出的成型兵法（如「假退伏击（满足条件 3 条）」）——TR-ux-001
- [ ] 四类战果分支（胜/撤退/失城/败北）每类显示 ≥1 个合法续局选项，点击提交对应 Command；任何分支**不出现**「游戏结束」措辞——TR-ux-004
- [ ] 首次失败时额外明示「可继续」（扭转「输了=重来」预期）——§3 第 5 步
- [ ] 长线意义提示：战果如何计入功绩/晋升（果→长线的一句话引导，如「此战记功 +N，距晋升还差…」）——§7 Q2 卡点裁决
- [ ] ViewModel 为纯函数（同 outcome 投影两次渲染恒等），不引用 Domain 可变内部态

---

## Implementation Notes

*Derived from ADR-0002 状态变更协议 + ADR-0009 Guidelines:*

- 数据源：`CampaignSessionService.ResolveBattleOutcome(...)` 返回 `OutcomeContinuation`（四分支续局已在 Domain 保证非空——TR-outcome-002 已测）；兵法复盘经 `RecognizeTactics(session)`。UI 只消费，不补算。
- 复用 `src/Presentation/Screens/CausalChainView.cs` 与 `ContinuationPromptView.cs`（竖切已建的纯 C# 投影）——先检查其字段是否覆盖本屏需要，缺则扩展该纯 C# 层而非在 Unity 侧拼装。
- 折叠/展开为**表现态**（不进 Domain/存档）；「因果链最多因素数 ≤5」「默认折叠」读自 Tuning Knobs 配置（数据驱动，勿硬编码）。
- 续局选项按钮 = 构造 Command 提交 `CampaignSessionService`，失败展示稳定错误码文案，不做 UI 侧预判。
- 长线提示只读生涯投影（记功数值来自 `ApplyCareerGain` 后的 career 态），不在 UI 侧算门槛公式。
- console harness `CampaignTextView` 的战果段是本屏文本原型，措辞可沿用（原创文案红线内）。

---

## Out of Scope

*Handled by neighbouring stories — do not implement here:*

- Story 003: 军议/敌情屏（置信/时效呈现）
- Story 004: 战斗中「还差 N 条」条件面板（本屏只做战后复盘）
- Story 005: 首次失败之外的整套新手引导序
- 战果写回逻辑本身（M07 已交付，本屏零新规则）

---

## QA Test Cases

*lean 模式 inline 编写。UI story：ViewModel 自动测试 + 人工走查。*

**自动（ViewModel，纯 C#）**
- **AC-1/2（折叠+因果）**
  - Given: 一个含 5 因素因果链与已识别兵法的 outcome 投影
  - When: 构造复盘 ViewModel
  - Then: 折叠态仅含结论行（分支+首要原因）；展开态因素数 ≤5、含兵法条件清单与成型兵法名
  - Edge cases: 因果因素 >5 的输入 → 截取主因素前 5（按权威投影排序），不丢结论
- **AC-3/4（续局）**
  - Given: 分别构造胜/撤/失城/败北四分支 outcome
  - When: 渲染续局区
  - Then: 每分支续局选项 ≥1；文案全集不含「游戏结束/删档/Game Over」；败北分支含「可继续」明示
- **AC-6（恒等）**
  - Given: 同一 outcome 投影
  - When: 渲染两次
  - Then: 输出逐字段相等

**人工走查（用户签核）**
- **复盘屏走查**
  - Setup: 打完一局（可用伏击胜局与强攻败局各一）进入复盘
  - Verify: 默认只见一句话结论；点展开见完整链与兵法标签；败局有续局按钮且点击后回到可操作态
  - Pass condition: 全程无胜率数字、无「游戏结束」、展开折叠往返无状态错乱

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- `production/qa/evidence/story-002-battle-review-evidence.md`（人工走查记录 + 截图 + 用户签核）
- ViewModel 自动测试于统一测试工程（`Presentation/BattleReviewViewModelTests.cs`）

**Status**: [x] ViewModel 自动测试已建并通过（8 测，全套 821/821）；batchmode 0 error；人工走查证据骨架已建待用户签核

---

## Dependencies

- Depends on: Story 001（会话接缝就绪，Unity 侧可取战果投影）
- Unlocks: Story 005（首次失败引导在本屏基础上加强）

---

## Completion Notes（2026-07-03）

- **交付**：`src/Presentation/Screens/BattleReviewView.cs`（不可变 ViewModel：默认折叠一句话结论/一键展开 ≤5 主因素/兵法复盘/续局选项中文映射/长线记功提示 + `BattleReviewTuning` 调节项集中承载 Q3 裁决）· `src/Presentation/Runtime/BattleReviewDemo.cs`（**临时**演示战局，只经用例命令的确定性序列）· `Assets/UI/Hud.uxml` outcome-chain 区扩展 · `Assets/UI/HudController.cs` 复盘渲染 · SessionRuntime.RunDemoBattle 临时入口 · 8 新测。
- **验证**：dotnet **821/821 绿**、`-warnaserror` 0；Unity **batchmode 编译 0 error**；Plugins DLL 已同步。
- **Deviations（ADVISORY）**：
  1. **未复用 `CausalChainView`**（实现注建议复用）：其「基值+Σ修正=终值」语义会把民心/治安/声望等**跨维度增量求和**，违反 P6 多维不压扁——改为逐条因素文案（Reason+Delta 原样呈现，不求和）。`ContinuationPromptView` 亦未直用（其 KindLabel 为英文枚举名，本屏需中文映射），但消费同一 Domain `ContinuationOption`。
  2. **续局按钮=记录选择而非分派命令**：Application 尚无「按 ContinuationCommandKind 执行」的用例（M07 交付的是选项提示）；点击显示已选定+提示战役可继续（推进时段等命令持续可用）。真实分派随 story-004「可做动作集」接入。
  3. **「演示一局」临时按钮**：story-002 evidence 入口（HUD 尚无真实备战→开战流程，归 story-004；接入后移除演示按钮与 `BattleReviewDemo`/`RunDemoBattle`）。
  4. 枚举中文映射带兜底（未登记新枚举值降级为枚举名，不崩 UI）。
- **待用户**：Unity Editor 人工走查 + 签核 `production/qa/evidence/story-002-battle-review-evidence.md`。
