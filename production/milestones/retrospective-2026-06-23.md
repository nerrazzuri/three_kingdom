# 阶段回顾 — Domain 完成 + 可玩竖切（2026-06-23）

> 范围：从项目采纳模板到「10 epics Domain 全绿 + Application 层补全 + Unity 可玩垂直切片」。
> 当前 HEAD = `tk/main` = `319a5a8`。审查模式 lean。

## 一、做出了什么（事实）

### 设计与闸门
- 13 份 GDD（含 Formulas/MVP Scope）+ 3 关键屏 UX（main-menu/hud/pause，均 Approved）+ art-bible v1.0 + 无障碍需求（WCAG 2.1 AA）。
- 跨系统审查 G3（/review-all-gdds）阻断闭合（外交受控入口归 GDD_012 §8）+ 5 Warning 全修。
- /gate-check pre-production → CONCERNS（可接受）→ stage = Production。
- 5 份 ADR（0001 引擎 / 0002 四层 / 0003 数据驱动 / 0004 确定性 / 0005 存档）全 Accepted。

### 垂直切片（throwaway 原型）
- `prototypes/three-kingdom-siege-vertical-slice`：headless C# 控制台驱动真实四层逻辑，三条破局链（假退伏击/断粮疲敌/守城待变）+ 军师 + 双边断粮博弈 + 情报雾 + 存档 round-trip 全跑通。
- **设计者亲手试玩签核 → PROCEED**。铁律：production 从头重写，永不 import 原型。

### Domain（生产代码，10 epics / 28 stories，全 ✅ Complete）
| Epic | 模块 | 关键能力 |
|------|------|---------|
| 001 | Numerics/边界 | FixedPoint Q16.16、DeterministicRandom、StateHasher、纯 C# 禁 Unity 边界回归 |
| 002 | Time/Environment/Map | 确定性时间推进、嵌套战役时段、天气/风向、拓扑寻路、真值/知识分离 |
| 003 | Characters/Relationships | 能力/性格/职责/意愿、方向性多维关系 |
| 004 | City/Supply/Diplomacy | 城市日界产耗守恒、三持有者补给+断粮事件、外交受控入口 |
| 005 | Intel/Council | 情报四层+置信/时效、军师条件化建议（无最优解） |
| 006 | Preparation | 计划草稿零副作用 + 硬冲突校验 + 原子提交 |
| 007 | Battle/Cohesion | 确定性战役管线+条件链复盘标签、士气/疲劳/军纪三维 |
| 008 | Outcome | 跨系统原子写回 + 可玩失败延续分支 |
| 009 | Persistence | 版本化 DTO+原子写+迁移链+round-trip+加载校验 |

### Presentation（EPIC_010，5 stories ✅）+ Unity 表现层
- 纯 C# Presentation 逻辑（投影→展示模型、UI 意图→Command、ViewModel、无障碍），设计锁 P10/P11/P6 反射断言固化。
- Unity 6.3 项目：三屏 UXML/USS/Controller + 可 Play 场景 + 无障碍设置面板 + 三屏挂接，lead Play 签核通过。

### Application 层 + 可玩竖切（本轮新增）
- **补齐 ADR-0002 四层中缺失的 Application 层**（`src/Application`：Session 编排 + Save 协调）。
- Unity 端真实驱动一局，**4 条命令路径 Play 签核通过**：推进时段、城市日界结算（账本）、敌情侦察+时效、存档/读档（经真实 epic-009 持久栈，跨 Play 会话持久）。

### 工程指标
- **dotnet 408/408 全绿，`-warnaserror` 0 warning**（确定性、无 Unity 运行时依赖，CI 可旁路许可）。
- 四层依赖方向 Domain ← Application ← Presentation 单向；Domain/Presentation 禁 UnityEngine 边界回归常驻。

## 二、做得好的地方

1. **Domain-first + test-first**：408 个确定性测试构成回归网，重构/扩展有底气。
2. **原型先验乐趣**：throwaway 切片在写大量 story 前验证了核心假设，避免在错误方向上盖楼。
3. **lean 节奏可持续**：readiness→dev-story→code-review→story-done→commit 跑通 30+ story 无失控。
4. **设计锁用反射测试固化**：P10 无真值泄露 / P11 无最优解 / P6 不合并 —— 不靠人工 review 守，靠测试守。
5. **接缝在运行中实证**：竖切把「能编译 + 单测绿」升级为「Unity 里真能玩」，闭合了贯穿全程的 CD-C3/TD 悬案。

## 三、需要警惕的（风险与债）

1. **一局尚未闭环**：还没有胜负条件；三条核心条件链只在 throwaway 原型证明过，**production 可玩构建里还表达不出来**。
2. **多数 Domain 系统未接表现层**：军议/准备/战斗/士气/后果/外交/人物花名册都有 Domain + 测试，但**未在 Unity 实证**（重复 CD-C3 的风险面，只是范围更小）。
3. **内容极薄**：`SliceScenario` 仅 1 城 + 1 敌主题，无人物、无拓扑、无天气接入；数值是占位非平衡。
4. **ADVISORY 签核不全**：精确对比度/文本 150% 无溢出/色盲调色板未实测；视觉仍占位。
5. **工程债**：DLL 桥需手动重建（改 src 后易忘）；GitHub Actions 首绿未确认；`tools/_unity_probe/` 残留待删。

## 四、教训

- **单测绿 ≠ 架构完整**：Application 层缺失被 408 个单测完全掩盖，直到竖切强迫接缝落地才暴露——「能跑起来的集成」揭示单测发现不了的洞。
- **场景生成的引用易失效**：`SliceSceneBuilder` 循环外捕获 PanelSettings 致非首屏空白，仅靠用户 Play 才发现——印证「渲染 + 可交互」双重签核不可省（只签「能渲染」会漏）。
- **OnEnable 接线脆弱**：横切 Apply 抛异常会吞掉后续按钮绑定——UI 壳的失败要在 Play 里以「点得动」验证，而非只看「画得出」。

## 五、结论

**Domain 与架构地基已就绪且经实证**；MVP 的剩余工作几乎不再是「写新系统」，而是**把已有系统组装成一局完整、有目标、会赢会输的游戏**（Application 编排 + Unity 接线 + 内容 + 收口 + 质量验收）。详见同目录 `mvp-roadmap-2026-06-23.md`。
