# 可玩游戏装配路线图（从内核到"能测的游戏"）

**日期**：2026-06-28
**依据**：`docs/reviews/full-game-review-2026-06-28.md`（评审结论：内核质量极高、装配极薄）
**目标**：把已验证的 Domain 内核（epic-001~012，564 测试全绿）**装配成一段可玩的太守人生循环**，逐模块完成，最后一模块完成即可在 Unity 里测整段游戏。

---

## 现状与目标

- **现在能玩**：一局守城（汜水关竖切）+ 四屏 UI。= "一局"，非"一段人生"。
- **目标循环**（game-concept）：`开局守城 → 治理城池/发育人才 → 君主任务/攒功绩名望 → 晋升/扩张夺城 → 终极抉择（继承/自立）→ 写回更大棋盘`。
- **缺的不是内核，是把内核装成被驱动的可玩循环。**

---

## 模块总览（M1→M7，= 未来 epic-013~019）

| 模块 | 名称 | 类型 | 依赖 | 完成后可测 |
|---|---|---|---|---|
| **M1** | 会话装配内核 CampaignSession | 装配（Application/Domain） | epic-011/012 ✅ | headless：开局→打一局→功绩涨+归属变→推进数日→三层态更新，确定性 |
| **M2** | 场景配置化 + 战役目录 | 数据驱动（含 CON-5） | M1 | 从数据载入 ≥2 场景、切换无代码改动、配置校验 |
| **M3** | 回合间治理/推进循环 | 装配（Application） | M1, M2 | headless：打→治理 N 日→触发1历史事件+1任务→晋升成功→进入下一战 |
| **M4** | 敌方 AI（便宜 80%） | 新系统（Domain） | epic-005/007 ✅, gdd-016(已修) | 同种子同决策、反全知锁、错误信念可复盘、接进一局战斗 |
| **M5** | Meta 层 UI（三屏） | Presentation（UI Toolkit） | M1, M3, epic-010 ✅ | 三屏渲染+可交互：生涯进度/战略态势/治理·战役决策 |
| **M6** | 首个可玩循环内容 | 内容+平衡 | M2, M3, M4 | 能从头玩一段：守城→治理→事件/任务→晋升→下一战 |
| **M7** | 端到端装配 + 可玩验证 + 平衡 | 集成（Unity 运行时） | M1~M6 | **Unity 里开局→玩完一段太守循环→存读档→确定性 = 可测游戏** |

**依赖图**：M1 → M2 → M3 → M6 → M7；M4 在 M1 后可并行；M5 在 M3 后可并行；M6 汇 M2/M3/M4；M7 汇全部。

**最小可玩集**（若想最快摸到可玩）：M1 + M2 + M3 + M4(便宜版) + M5(精简) + M6(1-2场景) → M7。无可跳项，但 M4/M5 可与 M6 并行压缩工期。

---

## M1 — 会话装配内核（CampaignSession）

> **这是脊梁**：把生涯+世界+战斗装进一个被驱动的会话。

- **目标**：建统一 `CampaignSession`（或扩 `GameSession`），同时持有 `CareerSnapshot` + `WorldState` + 战斗/竖切态；日界按 `systems-index` 全局结算顺序（FIX-2：环境→补给→城市/控制权→状态事件→015→014→016）推进三层；战果经既有服务写回生涯（`GovernorOutcomeService`）+ 世界归属（经 `ICityControlAuthority`/004）。
- **范围内**：会话聚合 + 推进编排 + 战果回写路由 + 确定性哈希 + 统一存档（复用 `CampaignSaveCodec`，FIX-8）。
- **范围外**：UI（M5）、AI（M4）、多场景（M2）、治理玩法细节（M3）。
- **触及**：`src/Application/Session/`（新 `CampaignSession`/`CampaignService`）、复用 11-4/12-4 链（FIX-9 已证通）。
- **验收（可测）**：headless 测——开局绑定太守→驱动一局守城→`merit/renown` 增、城归属经 004 变、`WorldCityProjection` 同步；推进 N 日三层态确定性演进；存读档 round-trip；同输入同哈希。

## M2 — 场景配置化 + 战役目录

- **目标**：`SliceScenario` → 不可变 `SliceScenarioData`（**收尾 CON-5**，数据/逻辑分离）；建 `ScenarioCatalog`（多场景从数据生成）；多城 `CitySeed` 数据；配置校验拒非法（ADR-0003）。
- **范围外**：完整 Unity SO 编辑器管线（留 M7/后续；本模块到"不可变配置 + 数据源接缝"即可）。
- **触及**：`src/Application/Session/SliceScenario*`、新 `ScenarioCatalog`、`CitySeed` 数据。
- **验收**：从数据载入 ≥2 个不同场景、切换无代码改动；非法场景配置被拒、不部分载入；既有竖切场景行为不变（564 测试不回归）。

## M3 — 回合间治理/推进循环

> **最大的"内容+装配"缺口**：现在战与战之间是空的。

- **目标**：把"治理/发育/任务/晋升"组织成可玩阶段 + campaign loop。城市治理跨日结算（已有 `CityDaySettlementService`）；君主任务（接受/完成→生涯，经 `LordMissionLog`/`CareerProgressionService`）；招揽人才；晋升申请（已有 `LoyalistAdvancementService`）；世界推进触发历史事件（12-2 `HistoryAdvancer`）与任务 → 产出下一情境（战役/事件）。
- **触及**：`src/Application/`（campaign loop + 任务/招揽命令）、复用 11-2/11-3/12-2/12-3。
- **验收**：headless 跑"一段人生"——打一局→治理 N 日（粮/民心/产出按 FIX-5 护栏演进）→触发 1 历史事件 + 1 君主任务→完成任务 merit 涨→申请晋升成功→进入下一战；全程确定性、同哈希。

## M4 — 敌方 AI（便宜 80%）

> gdd-016 §MVP；替换脚本敌人为会决策的对手。

- **目标**：`PersonalityProfile` 倾向 + `AiWorldView`（由 `FactionKnowledge`+`effective_conf` 构造，反全知）+ `ActionScorer`（3-4 动作）+ 种子 softmax（FIX-3 已修温度/lerp）+ 接入战区基础命令 + **把 AI 错误信念暴露给玩家**（复盘可读）。
- **范围外**：多日 `StrategicPlan`、跨战役 `OpponentModel`（gdd-016 Future）。
- **触及**：新 `src/Domain/EnemyAi/`（建 epic-016）、接 epic-007 战斗。
- **验收**：同种子同决策 + 温度单调性；反全知锁（`AiWorldView` 构造拒真值，编译级）；AI 错误信念可复盘；接进一局战斗替换脚本敌；确定性入哈希。

## M5 — Meta 层 UI（三屏）

- **目标**：① **生涯面板**（官阶/功绩名望进度/距下一阶/僚属好感/自立条件清单+结局风险预览，不泄真值）；② **战略态势图**（谁占哪城/历史事件/势力关系，受 GDD_007 情报限制）；③ **治理·战役选择屏**（下治理/任务/晋升/出战决策）。UI Toolkit + `Presentation` 投影/Intent 接 Application。
- **触及**：`src/Presentation/Screens|Projections|Intents`、`Assets/UI`、`Assets/Scenes`（沿 epic-010 模式）。
- **验收**：三屏渲染 + 键鼠可交互；只读投影即时反映 session 状态；玩家决策经 Command 路径下达（不直接改 Domain）；无障碍沿 epic-010 标准。

## M6 — 首个可玩太守循环内容

- **目标**：真内容——1-2 城 `CitySeed`、数条历史事件配置（讨董/汜水关一带，原创表达守红线）、数个君主任务、前 2-3 阶晋升门槛调平、1-2 战役场景；FIX-5 平衡护栏（雪球/粮上限/民心回升/刷战斗）在内容里落实数值。
- **触及**：配置数据（M2 的数据格式）、平衡数值。
- **验收**：能从头玩一段完整循环：守城→治理→事件/任务→晋升→下一战，内容自洽、可复现。

## M7 — 端到端装配 + 可玩验证 + 平衡

> **终点：可以测试游戏。**

- **目标**：Unity 运行时 `SessionRuntime`→`CampaignSession` 全接；端到端一段人生 playtest；平衡 pass（雪球/粮/刷战斗/民心 在玩中验证 FIX-5 护栏有效）；端到端"打一段完整循环"自动化测试 + 手动 playtest 签核。
- **触及**：`Assets/UI/SessionRuntime.cs`、场景接线、`/smoke-check`+`/team-qa`。
- **验收（= 可测游戏）**：Unity 里从主菜单开局→玩完一段太守循环（守城→治理→事件/任务→晋升/抉择）→存读档→确定性；端到端测试绿 + 设计者 playtest 签核。

---

## 执行方式

- 每模块走标准管线：`/create-epics`/`/create-stories`（按本路线图的模块定义）→ `/dev-story` 逐 story → `/code-review` → `/story-done` → commit+push。
- 每模块完成即有可测产物（见各模块"验收"）；M7 完成 = 整段游戏可测。
- 建议先开 **M1（会话装配内核）**——脊梁，解锁其余全部。

## 跟踪

- 本路线图为权威模块清单。各模块开工时建对应 epic-013~019，状态回写本表。
- 关联：评审报告 `docs/reviews/full-game-review-2026-06-28.md`；已修 Concern/Advisory（提交 `4b389e4`/`e993847`）；CON-5 收尾并入 M2。
