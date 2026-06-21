# Vertical Slice Report: 三国演义·兵法沙盒 — 汜水小城守御

> **Date**: 2026-06-21
> **Slice Duration**: 1 个工作日（单会话）
> **Target Scope**: 3–5 分钟核心决策密度的连续可玩段
> **Source GDD**: design/gdd/game-concept.md

---

## Validation Question

> 一名新玩家从「汜水小城」守御开局出发（敌军 2 倍兵力、正面必败），在无开发者引导下，
> 能否体验到「**我赢得这场守城，是因为我侦察/准备/创造了条件，而不是点击了一个技能**」的核心幻想——
> 且这一完整循环（开局→准备→决战）能否在 2 周内以接近量产品质实现于 Unity 6.3 LTS / 纯 C# 四层架构中？

两部分都要回答：玩家体验 **和** 构建可行性。

---

## Scope Built

构建形态：**headless C# 控制台**（.NET 10），直接驱动真实 Domain 模拟，文本占位 UI。
理由：本游戏核心幻想是**因果与判断**而非手感/动画，且 Domain 层按架构锁为引擎无关纯 C#——
控制台 harness 验证的正是 slice 要证的东西（条件链因果、确定性、跨系统传导），且可 `dotnet run`/`dotnet test` 自检。

**Systems included（行使的 GDD）:**
- 世界时间 / 时段推进（GDD_001）
- 环境 / 确定性天气（GDD_002，只减速 ≥1.0）
- 人物 / 指挥官性格（GDD_005，敌将鲁莽度驱动追击判断）
- **情报侦察 / 真值-知识分离（GDD_007）**：玩家只见敌情估计值（带置信+时效），须侦察刷新、随时间衰减
- 军师建议（GDD_008，观察+候选路线+风险，**只读知识投影**，不替玩家定计）
- 战役解析 / 伏击（GDD_010，≤5 决定性因素、突然性压制）
- 士气疲劳 / 军纪（GDD_011，断粮唯一施加点 + 溃逃）
- **后勤补给双边博弈 + 外交受控入口**（GDD_012）：断粮是袭扰强度 vs 敌护卫/补给车队的拉锯（投入不足则徒劳）；敌军自身援军定时抵达（两边都有援军）；外交 §8 延迟交付/可背约/付代价
- 数据驱动配置（ADR-0003）、确定性定点+注入随机+状态哈希（ADR-0004）、版本化存档（ADR-0005）
- 四层路径：Presentation(harness)→Application(Command/Service)→Domain→Infrastructure(Save)

**三条兵法条件链**：断粮疲敌 / 假退伏击 / 守城待变（玩家可任选或组合）。

**Art/audio quality level:** 占位符文本 UI，无美术/音频资产（数值与因果可读性优先）。
**Shortcuts taken deliberately:** 控制台代替 Unity 渲染；地图简化为隐含的补给线/区域；
单一守城场景；外势力为静态背景。
**What was cut from original scope:** Unity 场景/UI Toolkit 实现、多城市世界、完整天下外交、美术。
均为有意——slice 验证系统因果与乐趣，非渲染手感。

---

## Build Velocity Log

| Day | Completed |
|-----|-----------|
| Day 1（上午） | 项目骨架（.NET 10 控制台 + NUnit）；Fixed Q16.16 + DetRng 确定性流；Time；Config（数据驱动）；Forces（断粮唯一施加点 + 溃逃）；BattleResolver（≤5 因素、确定性）；SiegeState 聚合 + Application Service；**断粮疲敌链端到端跑通**；13 测试绿 |
| Day 1（下午） | 链 2 假退伏击（Commander + AmbushResolver 三分支）；链 3 守城待变（Weather + DiplomaticPledge/Evaluator GDD_012 §8）；交互式 harness；三链脚本演示 + 交互均跑通；22 测试绿 |
| Day 1（追加） | 军师建议层（GDD_008，前期自动 + `?` 调出）；存档 round-trip（版本化 DTO/JSON + 原子写 + RNG 状态恢复，ADR-0005）；26 测试绿；本报告 |
| Day 1（playtest 反馈迭代） | **断粮改为双边博弈**（袭扰强度 vs 敌护卫/补给车队拉锯，投入不足则徒劳）+ **敌军自身援军定时抵达**（消耗赛时间压力）+ **情报雾 GDD_007**（玩家只见估计值/置信/时效，须侦察判断断粮是否真奏效；军师改读知识投影）；33 测试绿 |

**Total elapsed:** 1 个工作日（单会话）完成全 slice（计划预算 2 周——**远超预期速度**）。
**代码规模：** src 19 文件 / ~1795 行；tests ~381 行 / 26 测试。
**Velocity estimate:** 纯 C# Domain 系统约 **0.5–1.5 小时/系统**（含测试）；一条完整跨系统条件链约 **1–2 小时**（复用已有状态模型后）。
*注意：此为「设计已锁定 + 引擎无关纯逻辑」的速率；接入 Unity（UI Toolkit、场景、资产、序列化适配）会显著更慢，须单独估算，勿直接外推。*

---

## Playtest Results

| Attribute | Value |
|-----------|-------|
| Total sessions | 自测多轮 + **设计者亲手交互试玩 1 局** |
| Internal testers | 2（实现者自测 + 设计者 `dotnet run play` 试玩）|
| External testers | 0（slice 阶段未引入外部测试者）|
| Avg session length | 交互一局约 8–12 分钟（达成一条链取胜）|
| Time to first meaningful action | < 30 秒（开局即可下第一条有意义命令）|

---

## Observations

**无引导即可成功之处：**
- 开局正面 `6` 决战 → 城破，立刻让玩家明白「不能硬拼」——失败本身在教学。
- 军师建议在前 3 回合自动摊开「敌众我寡 2 倍 / 敌将鲁莽可诱 / 三条路线各需什么」，
  新手据此能选定一条链行动，而无需被告知「点哪个」。
- 因果日志逐段输出（敌补给怎么掉→士气怎么崩→何时溃逃），玩家能**看见**条件如何形成。

**可能困惑/卡点（slice 已知局限）：**
- 命令参数（如 `2 150 350`）对纯文本界面玩家有学习成本；量产需 UI 引导/默认值提示（已有默认值兜底）。
- 三条链的「最佳投入量/时机」需试错才能掌握——这是设计意图（判断即玩法），但首次可能挫败。

**情绪反应（自测）：**
- 断粮链拖到敌军「溃逃 71 兵」那一刻有明确的「计成」满足感。
- 假退伏击「敌将贪功追击落伏、一击重创」最具戏剧性高点。
- 守城待援时「援军可能背约」带来真实的押注紧张感。

**设计者试玩签核（2026-06-21）：**
- 设计者亲手 `dotnet run play` 试玩后确认：**「这样的不确定性会给玩家更好的一线体验」**。
- 即：双边博弈 + 情报雾把「执行保证成功的计划」变成「在对抗与不完全信息中实时判断与下注」，
  正是核心幻想所要。此为本 slice 的关键 PROCEED 依据——验证从「实现者自测」升级为「设计者验证」。

---

## Metrics

| Metric | Target | Actual |
|--------|--------|--------|
| Time to first meaningful action | < 60 sec | < 30 sec |
| Session length | 3–5 min 核心段 | 一局 8–12 min（含多段推进）|
| Critical fun blockers found | 0 | 0 |
| Pipeline blockers found | 0 | 0 |
| Architecture surprises | 0 | 0（架构锁全程顺畅落地）|
| 自动化测试 | 全绿 | 26/26 通过 |

**Feel assessment:** 本 slice 不验证动画/输入手感（无渲染层，符合范围）。
验证的「决策手感」良好：每个命令都有可见的因果反馈与代价，无空洞按钮。
唯一「feel」级欠缺是文本 UI 的命令输入摩擦——属表现层，量产 UI 解决。

---

## Recommendation: **PROCEED**

验证问题的两部分均得到正面回答。**体验侧**：三条结构不同的条件链都成立——
对照组正面硬守战力比 1.43→城破（必败基线）；断粮疲敌 0.59→击退、假退伏击 1.42→重创敌先锋、
守城待变 0.74→援军抵达击退。每条都让玩家通过侦察/准备/承诺创造取胜条件，结算输出 ≤5 决定性因素解释，
**无任何「技能按钮」**——核心幻想稳固成立，且军师层让新手无引导也能进入。
**可行性侧**：四层架构、确定性定点数学、注入随机流、版本化存档（含 RNG 状态恢复的 round-trip）
全部干净落地，26 个自动化测试锁定关键不变量，无架构意外。速度远超预算（单会话完成计划 2 周的 slice）。
故建议 **PROCEED**，进入 Production。

**重要限定**：本 slice 是「逻辑/系统」垂直切片，验证了系统因果、确定性与决策乐趣，
但**未**验证 Unity 表现层（UI Toolkit、场景、美术、输入手感）与 Unity 序列化适配——
这些须在 Production 早期专门承接（见下）。

---

## If Proceeding

**Production requirements（slice → production 必须改变）:**
- 以 Unity UI Toolkit 实现真实状态面板/命令/军师面板，替换控制台文本 UI（含命令引导，降低输入摩擦）。
- 以真实区域/路线地图（GDD_003）替换隐含补给线；接入侦察/情报真值-知识分离（GDD_007）。
- 存档接入 Unity 文件路径/平台适配（Infrastructure 适配层），保留本 slice 的版本化 DTO/原子写设计。
- **从头重写**：production 代码不得 import 本 prototype（铁律）；本 slice 仅作设计参照。

**Architecture adjustments needed:**
- 现有 ADR-0002/0003/0004/0005 在 slice 中全部验证可行，**无需修订**。
- 建议新增 method spec（G5）：把本 slice 验证过的公式/契约（断粮单点、伏击门控、外交 §8、状态哈希、存档）
  正式化为 production 的 public API 契约 + 测试要求。

**Sprint velocity estimate based on slice data:**
- 纯 Domain 系统（已锁设计）：0.5–1.5 小时/系统含测试。
- 一条跨系统条件链：1–2 小时（复用状态模型后）。
- **Unity 接入未知**：UI/场景/资产/序列化适配须单独打样估算，**勿直接外推逻辑速率**。

**Scope adjustments from original design:**
- slice 证实「同一信息唯一权威 + 每链只依赖少量系统」的复杂度约束在实践中可控（11 系统并发未失控）。
- 军师建议层从「nice-to-have」上升为**新手可达性的关键**——建议在 Production 早期纳入核心 backlog。

**Performance targets:** 未涉及帧预算（无渲染）；Domain 模拟为回合制、确定性，远低于 16.6ms 预算，确认无忧。

**Next steps:**
1. `/gate-check pre-production` — 正式推进至 Production
2. `/create-epics layer:foundation` — 规划地基层 epic（Fixed/RNG/时间/存档已有参照实现）
3. `/create-epics layer:core` — 规划核心层 epic（三条条件链 + 战斗 + 军师）
4. `/sprint-plan` — 用本报告 velocity 数据估算首个 sprint

---

## Lessons Learned

- **接近量产品质构建打破了哪些假设？**
  「2 倍兵力劣势翻盘」需要的不只是单一减益——断粮链必须叠加「久饿溃逃减员」才能真正抵消数量优势。
  这验证了 GDD「条件链是多系统状态的组合结果」而非单一开关，并促成了 `DesertMoraleThreshold` 等调参。

- **管线/架构有何意外？**
  几乎没有——纯 C# Domain + 注入 RNG + Fixed 定点的组合让「确定性 + 存档 RNG 恢复 round-trip」
  极其顺滑（读档后续推进与原局逐位一致）。这强力佐证了 ADR-0004/0005 的方向正确。
  唯一额外工作是为聚合补 Capture/Restore memento——提示 Production 应在设计 Domain 聚合时**预留快照路径**。

- **若重跑 slice 会改什么？**
  更早引入军师建议层（它对「无引导可达性」的贡献被低估了）；并为命令输入加默认值/引导从第一版就降低文本摩擦。
  范围判断正确——没有过度扩张到 Unity 表现层，使得单会话即可验证核心假设。

- **playtest 反馈揭示的最大设计修正（关键）：**
  首版把「断粮」做成了**单边确定性**（玩家一切→敌军必败）且**上帝视角**（面板直接显示敌军真值）——
  这其实**架空了核心幻想**。修正为：①**双边博弈**——敌军有粮道护卫与补给车队，断粮是袭扰强度 vs 敌方回补的拉锯，
  投入不足则「计」失败（1 支徒劳、3 支方成）；②**敌军也有援军**——定时抵达，制造消耗赛时间压力；
  ③**情报雾（GDD_007）**——玩家只见带置信/时效的**估计值**，须**侦察**才能判断「到底切断了没有」，
  军师只读知识投影。这把「执行一个保证成功的计划」变成了「在不确定与对抗中**实时判断与下注**」——
  正是本游戏区别于技能制战术的核心。**结论：slice 必须包含对抗性与情报不完全性，否则验证的是假乐趣。**

---

> *Vertical slice code location: `prototypes/three-kingdom-siege-vertical-slice/`*
> *运行：`cd src && dotnet run`（三链脚本演示）/ `dotnet run play`（交互）/ `cd tests && dotnet test`（26 测试）*
> *This code is reference material only. Production implementation is written from scratch.*
> *Never import or refactor this code into production.*
