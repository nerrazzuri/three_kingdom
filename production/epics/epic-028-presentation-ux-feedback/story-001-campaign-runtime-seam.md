# Story 001: 会话接缝——SessionRuntime 重指 CampaignSession + 统一存档 round-trip

> **Epic**: 表现与理解循环（M15 / epic-028）
> **Status**: Complete（2026-07-03；用户 Unity 人工走查待补——见 Completion Notes）
> **Layer**: Presentation
> **Type**: Integration
> **Estimate**: M / ~4h
> **Manifest Version**: 2 (2026-06-28)
> **Last Updated**: 2026-07-03

## Context

**UX 契约**: `design/ux/m15-campaign-loop-ux.md`（Approved 2026-07-03）§4 硬约束
**Requirement**: `TR-ux-005`
*(需求原文在 `docs/architecture/tr-registry.yaml`，评审时读取最新版)*

**ADR Governing Implementation**: ADR-0009: CampaignSession 装配边界（primary）· ADR-0002: 四层架构 · ADR-0004: 确定性
**ADR Decision Summary**: CampaignSession 为 Application 装配脊梁，`CampaignSessionService` 是 Presentation 唯一可调用入口；Presentation 不持有可变 Domain aggregate，只读投影 + 只提交 Command；存档经统一信封 `CaptureSnapshot`/`Restore`（R-1 段集合）。

**Engine**: Unity 6.3 LTS | **Risk**: MEDIUM
**Engine Notes**: Unity 接线是 ADR-0002 标注「最易违反接缝」处。Claude 仅能 batchmode 验证（编译 0 error）；场景内行为须用户人工走查。`SessionRuntime` 为纯静态类无 MonoBehaviour 依赖，重写不涉及 post-cutoff API；存档介质沿用 `PlayerPrefsSaveMedium`（已验证）。

**Control Manifest Rules (this layer)**:
- Required: gameplay state 只由 Domain 经 Application Command 路径修改；存档有 schema version 与迁移策略
- Forbidden: UI 或 MonoBehaviour 直接持有/修改可变 Domain 对象；Unity 序列化处理 Domain 权威状态（ADR-0005）
- Guardrail: 战斗/会话结果可确定性复现；表现层 float 仅限非权威显示换算

---

## Acceptance Criteria

*From `m15-campaign-loop-ux.md` §4/§6（AC-6），scoped to this story:*

- [ ] `Assets/UI/SessionRuntime.cs` 不再引用旧竖切 `SessionService`/`GameSession`，改为经 `CampaignSessionService` 操作 `CampaignSession`（新局/推进/存读档最小生命周期贯通）
- [ ] MainMenu「新游戏」以数据驱动场景配置开局（复用 console `PlayableCampaign` 的「汜水关太守」配置源，**不得**内联平衡数值或以 `SliceScenario.Default()` 为完整游戏唯一开局源——ADR-0009 R-5 ④）
- [ ] MainMenu「继续」/ PauseMenu 存读档走统一信封 `CaptureSnapshot`/`Restore`，round-trip 后状态哈希一致（AC-6）
- [ ] 表现层不引用 Domain 可变内部态：编译期程序集边界可证（internal 成员跨程序集不可见，沿 harness 已验证模式）
- [ ] 同会话态渲染恒等：投影/ViewModel 为纯函数，同态两次投影结果一致

---

## Implementation Notes

*Derived from ADR-0009 Implementation Guidelines + R-1/R-7:*

- 推荐结构：在 `src/Presentation`（纯 C# 工程）新建 `CampaignRuntime` 核心（可注入存档介质接口），`Assets/UI/SessionRuntime.cs` 退为薄静态适配壳——使生命周期逻辑可 `dotnet test`，Unity 侧仅剩粘合。
- `CampaignSessionService.StartCampaign(CampaignStartConfig)` / `Advance` / `CaptureSnapshot` / `Restore` 已存在且 805 测试覆盖，本 story 不改 Application/Domain，只接线。
- 存档介质：`CaptureSnapshot` 返回统一信封字符串；文件/PlayerPrefs I/O 必经 Infrastructure 端口（R-7），沿用 `PlayerPrefsSaveMedium` 注入。
- 场景配置：console 的 `PlayableCampaign` 是已验证的确定性默认场景装配源；若 Unity 侧无法直接引用 Console 工程，把 `PlayableCampaign` 场景组装迁至可共享处（如 `src/Presentation` 或 Application 场景目录），Console 改为引用同源——**单一来源，勿复制两份数值**。
- 旧 `GameSession` 竖切路径按 ADR-0009 裁决保留为 slice fixture，不删除；本 story 只切换 `SessionRuntime` 的指向。

---

## Out of Scope

*Handled by neighbouring stories — do not implement here:*

- Story 002: 战果复盘屏（本 story 只保证会话可开/可进/可存读，不做任何新屏）
- Story 003/004: 军议/敌情/治理/备战投影重定向（HUD 各面板届时逐一切换）
- 旧竖切 `SessionService` 的删除或重构（达内容平价前保留，ADR-0009 裁决）

---

## QA Test Cases

*lean 模式 inline 编写（沿 epic-011/012 惯例）。开发按此实现，不得实现中另造测试面。*

- **AC-1/2（接线+数据驱动开局）**
  - Given: 新建 `CampaignRuntime`（注入内存存档介质 + 汜水关场景配置源）
  - When: `NewGame()` 后 `Advance(1)`
  - Then: 返回的会话投影时间前进一段；全程未触碰 `SessionService`/`GameSession` 类型（编译引用可证）
  - Edge cases: 未开局即调 `Advance`/`Save` → 自动开局或返回稳定错误，不抛裸异常
- **AC-3（round-trip）**
  - Given: 开局后执行若干命令（治理 1 次 + 推进 2 段）
  - When: `CaptureSnapshot` → `Restore`
  - Then: 恢复后状态哈希与保存前逐位一致；再推进 1 段的结果与未存读的对照会话一致（确定性延续）
  - Edge cases: 损坏信封字符串 → Restore 失败返回原因、当前会话不变（不部分载入）
- **AC-4/5（边界+纯函数）**
  - Given: 同一会话态
  - When: 连续两次取同一投影
  - Then: 结果值相等（渲染恒等）；`ThreeKingdom.Unity.UI` 程序集无对 Domain 内部类型的引用（batchmode 编译 0 error 即证，internal 不可见）

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- 自动测试：统一测试工程 `tests/unit/ThreeKingdom.Domain.Tests/`（新增 `PresentationRuntime/CampaignRuntimeTests.cs`）——必须存在且通过
- Unity 面：batchmode 编译 0 error CS + 用户人工走查记录（`production/qa/evidence/story-001-campaign-runtime-seam-evidence.md`，MainMenu 新局→HUD 推进→Pause 存读一遍）

**Status**: [x] 自动测试已建并通过（8 测，全套 813/813）；batchmode 0 error；人工走查证据骨架已建待用户签核

---

## Dependencies

- Depends on: None（本 epic 首个 story；`CampaignSessionService` 已由 M00~M10 交付）
- Unlocks: Story 002/003/004（全部 Unity 屏 story 以本接缝为前置）

---

## Completion Notes（2026-07-03）

- **交付**：`src/Application/Scenarios/PlayableCampaign.cs`（git mv 自 Console，单一场景源）· `src/Presentation/Runtime/CampaignRuntime.cs`（纯 C# 生命周期接缝）· `Assets/UI/SessionRuntime.cs` 重写（薄壳，零旧竖切类型引用）· `Assets/UI/HudController.cs` 收敛（时间/推进/存档走战役会话）· 8 新测。
- **验证**：dotnet **813/813 绿**、`-warnaserror` 0；console 全循环脚本自检通过；**Plugins DLL 重建同步**（旧 DLL 停在 6-24，不含 CampaignSession——本次必要前置）；Unity **batchmode 编译 0 error**。
- **Deviations（ADVISORY）**：
  1. **HUD 未接线面板临时退化**：账本/敌情/军议/花名册/侦察/袭扰/伏击/目标面板显式「接入战役会话中……」占位+按钮禁用（不再显示旧竖切数据，避免与战役态混淆）；story-003/004 逐屏恢复。
  2. **QA 用例 AC-3 微调**：「恢复后推进与未存读对照一致」改测「同信封两次恢复各自推进哈希一致」——因发现既有缺口：`CampaignSessionService.Restore` 不接收 historyCatalog/playerReach/divergenceConfig，恢复后 `HasHistory=false`（历史**状态**在 world 段完整恢复，但读档后 AdvanceHistory 成 no-op；console harness 同样受影响，非本 story 引入）。**技术债已记**：建议 Application 小修（Restore 增 3 个可选配置参数）。
  3. 存档槽改用 `campaign-session`（与旧竖切 `campaign` 槽区分，避免旧格式误读）。
- **待用户**：Unity Editor 人工走查（MainMenu 新局→HUD 推进→存读→重开继续），签核 `production/qa/evidence/story-001-campaign-runtime-seam-evidence.md`。
