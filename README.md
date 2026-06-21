<h1 align="center">三国演义：兵法沙盒</h1>

<p align="center">
  离线单机 · 三国沙盒战略 RPG
  <br />
  以一名武将度过乱世人生 —— 兵法不是技能按钮,而是你亲手创造的世界条件。
</p>

<p align="center">
  <img src="https://img.shields.io/badge/engine-Unity%206.3%20LTS-000000?logo=unity" alt="Unity 6.3 LTS">
  <img src="https://img.shields.io/badge/language-C%23-239120?logo=c-sharp" alt="C#">
  <img src="https://img.shields.io/badge/platform-PC%20(Win%2FmacOS%2FLinux)-blue" alt="PC">
  <img src="https://img.shields.io/badge/stage-Production-brightgreen" alt="Production">
</p>

---

## 一句话定位

玩家以一名身处三国乱世的武将度过人生、经营关系与势力,并通过**侦察、地形、天气、时机、后勤和人心**亲手创造兵法成立的条件。

## 核心体验

本项目**不是**把计策包装成技能按钮,而是让玩家**理解局势 → 改变条件 → 承担代价 → 利用结果**。

- 战争由人物、城市、外交、关系和政治共同塑造;战果也必须反向改变这些系统。
- 军师提供信息整理、风险判断和候选思路,**不替玩家执行最优解**。
- 失败生成新局面(撤退、失城、改投),而不是轻易毁档或强迫读档。
- 所有规则**可配置、可测试、可追踪、可存档、可演进**。

## 五大产品支柱

| # | 支柱 | 说明 |
|---|------|------|
| 1 | 人物人生系统 | 武将的一生:成长、际遇、抉择与代价 |
| 2 | 关系与派系系统 | 人际、忠诚、派系角力 |
| 3 | 城市经营与政治系统 | 内政、资源、治理 |
| 4 | 外交与天下局势系统 | 势力间的博弈与大局演变 |
| 5 | 兵法沙盒战斗系统 | 由可观察、可干预、可组合的世界条件产生的战斗 |

## 目标玩家

- 喜欢三国人物与历史情境,而不只追求战斗数值的玩家。
- 喜欢通过信息、准备和取舍解决问题的策略玩家。
- 接受失败、撤退、失城和改投等可持续叙事结果的沙盒玩家。

---

## 技术栈

- **引擎**:Unity 6.3 LTS · **语言**:C# · **渲染**:URP
- **架构**:四层分层(Domain / Application / Infrastructure / Presentation),依赖单向内指 —— 详见 [ADR-0002](docs/architecture/adr-0002-architecture-layering.md)
- **核心技术原则**:
  - 确定性战斗模拟(整数/定点 Q16.16 + 注入随机流 + 状态哈希) —— [ADR-0004](docs/architecture/adr-0004-deterministic-battle-simulation.md)
  - 数据驱动配置(ScriptableObject 编辑期 → 构建时转不可变配置) —— [ADR-0003](docs/architecture/adr-0003-data-driven-configuration.md)
  - 存档版本与迁移(显式 DTO/JSON + 原子写入 + 逆序迁移链) —— [ADR-0005](docs/architecture/adr-0005-save-versioning-migration.md)
- **目标平台**:PC(Windows 10/11、macOS、Linux),键鼠为主,可选手柄
- **性能预算**:60fps / 16.6ms 帧预算 · ≤1000 draw calls · ≤8GB 峰值内存

> 当前实现许可:**Domain/Application 层实现已授权**(Pre-Production→Production 门禁已通过,裁定 CONCERNS)。Presentation/UI/Scene 实现仍待表现层里程碑前置验证后开启。

## 当前阶段:Production — Foundation 实现

Pre-Production→Production 门禁已通过(CONCERNS,四总监 Panel 裁定),垂直切片乐趣已验证,正按 epic/story 推进 Domain 层确定性底座。

- ✅ 13 份系统 GDD(含 Formulas 公式节)+ G3 跨系统矛盾审查闭环
- ✅ 5 份 ADR 全部 Accepted + 架构审查
- ✅ 美术圣经 v1.0(AD 签核 APPROVED)· 无障碍需求(Approved)· 三关键屏 UX(全 APPROVED)
- ✅ 垂直切片「汜水小城守御」PROCEED(设计者试玩签核)
- ✅ 9 epics / 28 stories · 测试脚手架 + CI(`dotnet test` 旁路 Unity 许可)
- 🔄 进行中:epic-001 Domain 基础(S1 Domain 边界 ✅ / S2 定点+随机流+状态哈希 ✅ → S3 配置加载校验)
- ⏳ 下一步:Foundation 余下 story → 世界基质 / 人物 / 城池 / 情报 / 兵法沙盒各 epic

> **权威进度跟踪**:`production/epics/*/story-*.md`(每 story 含状态/验收/测试证据)。`production/story-backlog.md` 为已取代的早期初稿。

## 文档导航

```
design/
  gdd/                  13 份系统 GDD(游戏时间/天气/地图/经济/人物/关系/
                        情报/军议/战前/战术沙盒/士气/后勤/存档)+ 概念与支柱
  art/art-bible.md      美术圣经(视觉身份、色彩系统、资产标准)
  ux/                   交互模式库
  accessibility-requirements.md   无障碍需求(WCAG 2.1 AA 基线)
docs/
  project-brief.md      项目简报(定位/支柱/门禁)
  architecture/         ADR-0001~0005、架构总览、可追溯矩阵、控制清单
  test-strategy.md      测试策略 · balance-strategy.md 平衡策略
production/
  epics/ sprints/ story-backlog.md   生产管理
  stage.txt             当前阶段标记
tests/                  Unity Test Framework 脚手架 + CI workflow
```

入门建议:先读 [`docs/project-brief.md`](docs/project-brief.md),再看 [`design/gdd/systems-index.md`](design/gdd/systems-index.md) 总览系统地图。

---

## 关于开发方式

本项目基于 **[Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios)** 代理工作流模板构建 —— 通过一组协作的 Claude Code 子代理(导演 / 部门负责人 / 专家三层)与斜杠命令工作流,以"提问 → 选项 → 决策 → 草稿 → 批准"的协作方式推进设计、架构与生产管理。模板配置位于 `.claude/`,详见 [`CLAUDE.md`](CLAUDE.md)。

所有交流与文档均使用中文。
