# Story 005: 新手循环序 + 无障碍关键项对齐

> **Epic**: 表现与理解循环（M15 / epic-028）
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M / ~3h
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: —（/dev-story 开始时填写）

## Context

**UX 契约**: `design/ux/m15-campaign-loop-ux.md`（Approved 2026-07-03）§3 新手循环序（含 2026-07-03 卡点裁定：果·长线为重点）· §4.5/4.6 无障碍与文本膨胀 · §5 Tuning Knobs
**Requirement**: `TR-ux-002`（军议自动显示分量）· AC-7（无障碍，ADVISORY 级证据）
*(需求原文在 `docs/architecture/tr-registry.yaml`，评审时读取最新版)*

**ADR Governing Implementation**: ADR-0002: 四层架构（primary）· ADR-0003: 数据驱动配置（Tuning Knobs）
**ADR Decision Summary**: 引导触发条件与档位阈值全部来自版本化配置（勿硬编码回合数）；引导为表现层状态，不进 Domain/存档权威体。

**Engine**: Unity 6.3 LTS | **Risk**: MEDIUM
**Engine Notes**: 竖切已有 `AccessibilityRuntime`/`AccessibilityApplier`（文本缩放/高对比等），本 story 是对齐检查+补缺，非重建。batchmode 验证 + 用户人工走查。

**Control Manifest Rules (this layer)**:
- Required: 沿自然循环序、in-world 缘由教学（§3 原则）；所有引导参数数据驱动
- Forbidden: 模态弹窗串讲式教程；引导替玩家选择（军师不替选，P11）
- Guardrail: 色盲不靠纯色区分；文本缩放可用；键鼠全可达、无仅悬停可触发状态；40% 文本扩展预算

---

## Acceptance Criteria

*From `m15-campaign-loop-ux.md` §3/§6（AC-7）+ §7 Q2 裁决，scoped to this story:*

- [ ] 军议自动显示：新手前 N 回合自动展开军师建议（N 来自 Tuning Knob，默认 3），之后按键调出——§5
- [ ] §3 渐进暴露序落地为 in-world 提示（察→谋→备→战各一条情境提示，不模态、不替选）
- [ ] **果·长线加重引导**（§7 Q2 卡点裁决）：战果后首次进入生涯/历史相位时，提示「战果→记功→晋升」与「推进时段→历史在轨上跑」的意义各一条；首次失败强化「可继续」提示（与 Story 002 联动）
- [ ] AC-7 无障碍走查通过：色盲冗余编码（本 epic 各新屏抽查）、文本缩放在新屏生效、键鼠全可达、无仅悬停才可见的信息
- [ ] 新屏文本元素有字符预算标注（40% 扩展余量，§4.6）——落在各屏 evidence 文档的检查表里
- [ ] 引导状态可跳过/可关闭；关闭后不再打扰（表现层偏好，不入权威存档体）

---

## Implementation Notes

*Derived from §3 原则 + ADR-0003:*

- 引导配置：`OnboardingConfig`（提示触发条件/文案键/前 N 回合值）走 SO 编辑期→不可变配置管线（ADR-0003），表现层读取。
- 提示呈现复用 `NotificationFeed`（竖切已建）而非新弹窗系统；一次一条、可关。
- 果·长线提示的触发点：`ResolveBattleOutcome` 后首次 / `ApplyCareerGain` 首次 / `AdvanceHistory` 首次——由表现层记录「已见」标志（PlayerPrefs 级偏好，非权威存档）。
- 无障碍对齐按 `design/ux/accessibility-requirements.md` §2.3/§7 的 MVP 范围核对，超范围项（屏幕阅读器全覆盖等）不在本 story 承诺。
- 教学文案原创（红线）；先中文，键值化以备本地化。

---

## Out of Scope

*Handled by neighbouring stories — do not implement here:*

- 完整难度矩阵/教程关卡（M16）
- Story 002/003/004 各屏自身的内容契约（本 story 只加引导层与无障碍核对）
- 屏幕阅读器/AT 全覆盖（accessibility-requirements §7 标注的非 MVP 部分）

---

## QA Test Cases

*lean 模式 inline 编写。UI story：ViewModel 自动测试 + 人工走查。*

**自动（ViewModel/配置，纯 C#）**
- **AC-1（军议自动显示）**
  - Given: OnboardingConfig N=3；会话回合 1/3/4 各一投影
  - When: 渲染军议入口 ViewModel
  - Then: 回合 1、3 自动展开标志为真；回合 4 为假（需按键调出）
  - Edge cases: N=0（关闭引导）→ 任何回合不自动展开
- **AC-3（果·长线首次提示）**
  - Given: 首次战果结算的会话投影 + 空「已见」集
  - When: 渲染提示队列 → 标记已见 → 再渲染
  - Then: 首次含记功/晋升提示；标记后不再出现
- **AC-6（可关闭）**
  - Given: 引导关闭偏好为真
  - When: 渲染任一引导点
  - Then: 无引导输出；权威会话状态哈希不受偏好影响（引导不进 Domain）
- **配置校验**：非法 N（负数）被配置校验拒绝（ADR-0003）

**人工走查（用户签核）**
- Setup: 全新偏好开新局，完整玩到首个战果 + 首次晋升记功；再以 125% 文本缩放重进各新屏
- Verify: 提示按序出现且不弹模态、不替选；果·长线两处提示可读有用；缩放后无截断；全流程仅键鼠可完成
- Pass condition: 新玩家视角「每步知道为什么和下一步」；无障碍检查表全勾

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- `production/qa/evidence/story-005-onboarding-a11y-evidence.md`（人工走查 + 无障碍检查表 + 字符预算标注 + 用户签核）
- ViewModel/配置自动测试于统一测试工程（`Presentation/OnboardingViewModelTests.cs`）

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: Story 002 + Story 003 + Story 004（在成型屏上加引导层与无障碍核对）
- Unlocks: None（本 epic 收尾 story；完整难度矩阵归 M16）
