# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6.3 LTS
- **Language**: C#
- **Rendering**: Universal Render Pipeline (URP)
- **Physics**: 不适用（Domain 层不依赖物理引擎；展示层如需视觉效果可选 Unity Physics）

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: PC（Windows 10/11、macOS、Linux）
- **Input Methods**: Keyboard/Mouse（主要）、Gamepad（可选）
- **Primary Input**: Keyboard/Mouse
- **Gamepad Support**: Partial
- **Touch Support**: None
- **Platform Notes**: 策略 UI 须在 1080p 及 1440p/4K 下可读；所有交互元素须通过键鼠可达，不得有仅悬停才可触发的状态。

## Naming Conventions

- **Classes**: PascalCase（e.g., `WorldDay`、`BattleResolutionService`、`SubmitDeploymentCommand`）
- **Variables**:
  - Public 属性/字段: PascalCase（e.g., `CurrentSegment`、`FactionId`）
  - Private 字段: `_camelCase`（e.g., `_currentSegment`、`_moveSpeed`）
- **Signals/Events**: Domain Events 用 PascalCase + `Event` 后缀（e.g., `BattleResolvedEvent`）；C# 委托用 `EventHandler` 后缀（e.g., `BattleStartedEventHandler`）
- **Files**: PascalCase 与类名一致（e.g., `WorldDayService.cs`、`CharacterRepository.cs`）
- **Scenes/Prefabs**: PascalCase 对应根概念（e.g., `WorldMapScene.unity`、`CityPanel.prefab`）
- **Constants**: Domain 常量用 PascalCase（e.g., `DefaultMoveSpeed`）；ScriptableObject 配置键可用 `UPPER_SNAKE_CASE`
- **Domain 层专有约定**:
  - Value Objects: PascalCase 无后缀（e.g., `WorldDay`、`DaySegment`、`FactionId`）
  - Commands: PascalCase + `Command` 后缀（e.g., `SubmitDeploymentPlanCommand`）
  - Application Services: PascalCase + `Service` 后缀（e.g., `BattleResolutionService`）
  - Interfaces: `I` 前缀（e.g., `ICharacterRepository`、`IConfigLoader`）
  - DTOs: PascalCase + `Dto` 后缀（e.g., `CharacterStateDto`、`BattleResultDto`）
  - ScriptableObjects（配置）: PascalCase + `Config` 或 `Data` 后缀（e.g., `WeatherConfig`、`FactionData`）

## Performance Budgets

- **Target Framerate**: 60fps（玩家可见帧；回合制模拟处理不计入帧预算）
- **Frame Budget**: 16.6ms（玩家可见帧）
- **Draw Calls**: ≤1000 帧/游戏期间
- **Memory Ceiling**: 8GB 峰值工作内存（模拟状态 + 资产 + Unity 运行时开销）

## Testing

- **Framework**: NUnit（Unity 标准；Domain 层纯 C# 单元测试 + Unity EditMode 集成测试）
- **Minimum Coverage**: Domain 层全部 public 方法；Application 层全部用例
- **Required Tests**:
  - 时间推进确定性（同一种子 → 同一结果）
  - 战役条件链结算（各条件独立可验证）
  - 存档读档 round-trip 状态一致性
  - 配置校验（非法范围与缺失引用被拒绝）
  - Command 前置校验（失败返回稳定错误码，无部分写入）

## Forbidden Patterns

- Domain 层出现 `MonoBehaviour` 或任何 `UnityEngine.*` 类型
- `ScriptableObject` 作为运行时权威状态（仅用作配置来源，构建时转换为不可变 Domain 配置）
- Presentation 层直接修改 Domain 状态（所有玩家操作必须经 Command / Application Service 路径）
- 方法体内硬编码平衡数值（所有数值来自版本化配置）
- Domain 逻辑依赖帧率或 Unity 时间（模拟必须确定性且引擎无关）
- Domain 使用隐式随机源（所有随机性通过显式注入的确定性种子或预生成随机流）
- UI 或 MonoBehaviour 直接持有或修改可变 Domain 对象
- Domain 权威结算路径使用 `float`/`double`（影响状态哈希的计算用整数/定点 Q16.16；float 仅限非权威 Presentation/UI）— ADR-0004
- 用 Unity JsonUtility / Unity 序列化处理 Domain 权威状态（存档须用显式版本化 DTO + JSON 经 Infrastructure 端口）— ADR-0005

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here only when actively integrating, not speculatively. -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [ADR-0001](../docs/architecture/adr-0001-engine-choice.md): Unity + C# 为引擎与主语言 — Accepted（2026-06-21）
- [ADR-0002](../docs/architecture/adr-0002-architecture-layering.md): 四层架构（Domain/Application/Infrastructure/Presentation）+ 依赖方向 — Accepted（2026-06-21）
- [ADR-0004](../docs/architecture/adr-0004-deterministic-battle-simulation.md): 确定性战斗模拟（整数/定点 + 注入随机流 + 状态哈希）— Accepted（2026-06-21）
- [ADR-0003](../docs/architecture/adr-0003-data-driven-configuration.md): 数据驱动配置（SO 编辑期 + 构建时转不可变配置 + 配置指纹）— Accepted（2026-06-21）
- [ADR-0005](../docs/architecture/adr-0005-save-versioning-migration.md): 存档版本与迁移（显式 DTO/JSON + 原子写入 + 逆序逐版迁移链）— Accepted（2026-06-21）
- [ADR-0006](../docs/architecture/adr-0006-deterministic-enemy-ai.md): 确定性效用敌方 AI（种子化 softmax + 反全知锁 + LLM 隔离）— Accepted（2026-06-24）
- [ADR-0007](../docs/architecture/adr-0007-conditional-history-world-model.md): 条件历史世界模型（事件四元组 + reachability 触发门 + 分叉传播 + 抽象结算）— Accepted（2026-06-24）
- [ADR-0008](../docs/architecture/adr-0008-city-control-ownership-contract.md): 城池控制权跨系统所有权契约（GDD_004 唯一权威 + 控制权变更事件；015 订阅/014 只读）— Accepted（2026-06-24）
- [ADR-0009](../docs/architecture/adr-0009-campaign-session-assembly.md): CampaignSession 装配边界（Application 装配脊梁，只编排不拥规则；统一存档信封 + 日界全局序 + 命名原子写回；势力创建经 015）— Accepted（2026-06-28）
- [ADR-0010](../docs/architecture/adr-0010-conquest-occupation-ownership.md): 占城归属契约（出征占城复用 ADR-0008 控制权变更；归属方案 C——前2默认归玩家、后续君主种子化确定性取舍；被夺战果累积自立倾向）— Accepted（2026-07-04）
- [ADR-0011](../docs/architecture/adr-0011-offensive-preparation-model.md): 多维确定性出征准备模型（六维闭合因果：兵力/补给/将领/兵种/布势/时机；布势路线复用 4 兵法链非坐标、兵种杠杆非克制、其余维做条件成型门、反全知侦察门；纯函数整数/定点）— Accepted（2026-07-04）
- [ADR-0012](../docs/architecture/adr-0012-deterministic-zone-battle-engine.md): 确定性区域战斗引擎（命名区域图非坐标 + 回合纯函数状态机 + 部署/战中调整命令[相邻+在途] + 条件按区按回合涌现 + 结算确定性优先序 + 攻守统一 + 战中存档续战）— Accepted（2026-07-04）
- [ADR-0013](../docs/architecture/adr-0013-enemy-zone-ai.md): 敌方区域AI效用模型（特化 ADR-0006：区域动作空间 + 反全知 AiWorldView 不接受真值 + 数据驱动效用 + 种子softmax + 渐进记忆 + 同规则不作弊 + LLM隔离；落地 GDD_016，架构一次到位、深度分期迭代）— Accepted（2026-07-04）

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist（C# 代码审查——Primary 覆盖）
- **Shader Specialist**: unity-shader-specialist（Shader Graph、HLSL、URP/HDRP 材质）
- **UI Specialist**: unity-ui-specialist（UI Toolkit UXML/USS、UGUI Canvas、运行时 UI）
- **Additional Specialists**: unity-addressables-specialist（资产加载、内存管理、内容目录）；unity-dots-specialist（ECS/Jobs/Burst——仅在 ADR 批准 DOTS 采用后启用）
- **Routing Notes**: 架构决策与通用 C# 代码审查使用 Primary。渲染和视觉效果使用 Shader 专家。所有 UI 实现使用 UI 专家。资产管理使用 Addressables 专家。DOTS 专家在无 ADR 批准前**不得**启用——本项目 Domain 层为纯 C#，非 ECS。

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs files) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss, Canvas prefabs) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Config assets (.asset, ScriptableObjects) | unity-specialist |
| Native extension / plugin files (.dll, native plugins) | unity-specialist |
| General architecture review | unity-specialist |
