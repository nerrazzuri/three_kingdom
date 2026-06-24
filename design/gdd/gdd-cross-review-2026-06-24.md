# 跨 GDD 审查报告 — 2026-06-24（G3 续，聚焦 014/015/016 + 改动 010/game-concept）

- **日期**：2026-06-24
- **范围**：新增 GDD_014（战役与生涯）/ GDD_015（条件历史世界模型）/ GDD_016（敌方 AI）+ 改动 `game-concept.md`、`gdd-010-battle-tactics-sandbox.md`，对照既有 GDD_001-013、`game-pillars.md`、`systems-index.md`。
- **审查模式**：lean（inline 综合，焦点 5 篇已全读）
- **裁定**：**CONCERNS**（无 Blocking；多项 Warning，W1 已由用户裁定）
- **前序报告**：`design/gdd/gdd-cross-review-2026-06-21.md`（13 篇 vertical slice GDD 基线）

> Registry 状态：`design/registry/entities.yaml` 的 entities/items/formulas/constants 全为 `[]`（空）。
> 一致性检查依赖全文读完成。**建议审后跑 `/consistency-check`** 填充新引入的跨系统名
> （功绩 merit / 名望 renown / 君主好感 lord_standing / 城池归属 / StrategicAction / TacticRecognizer / AiWorldView 等）。

---

## 一致性问题（Phase 2）

### Blocking
无。

### Warnings

#### W1 — 城池归属权威冲突 ★已裁定★
- `gdd-004` L27/138/147：拥有「控制权」+「控制权变更事件」+「日界前城市易主按控制权变更事件结算」。
- `gdd-015` §Formulas L61：`city.owner ← outcome.owner_change`（世界模型直接写归属）。
- `gdd-014` §System Outputs：写回「GDD_004 城池控制权」。
- 三处都在动城池归属，失城/夺城场景下（见 Phase 4）会双写或权威撕裂。
- **裁定（用户 2026-06-24）**：**GDD_004 为城级控制权唯一权威 + 唯一变更事件**；
  **GDD_015 世界模型在战略尺度只反映归属、订阅 GDD_004 控制权变更事件、不独立写**；
  **GDD_014 只读、不写归属**。
- **待修**：
  - GDD_015 §Formulas 公式 2 注明 `city.owner` 为**只读投影**，更新源唯一为 GDD_004 控制权变更事件（含历史事件结局须经 004 落地）。
  - GDD_004 显式声明城级控制权唯一权威 + 加 014/015 为 dependents。
  - GDD_014 §System Outputs 措辞从「写回控制权」改为「经 GDD_004 控制权变更触发」。

#### W2 — 依赖反向引用缺失
- 014/015/016 声明依赖 001/003/004/005/006/007/010/011/012/013/015，但既有 001-013 无一把 014/015/016 列为 dependents（新文档必然如此）。
- 附带：`gdd-010` §Dependencies（L213）未列 016 为消费者，尽管 010 §AI Requirements 已引用 GDD_016/ADR-0006。
- **待修**：相关旧 GDD 的 Dependencies 加反向引用；新跨系统名登记进 registry。

#### W3 — `TacticRecognizer` 命名前向引用
- `gdd-016` §Main Rules/Formulas 按名引用 GDD_010 的 `TacticRecognizer`（"AI 用 GDD_010 TacticRecognizer 的事后标签"）。
- `gdd-010` 描述了该行为（"系统事后识别并打复盘标签"、§10 因果复盘、`CausalTrace`）但**未以 `TacticRecognizer` 命名该组件**。
- **待修**：GDD_010（§招式库或 §Data Model）显式命名发出套路标签的识别器为 `TacticRecognizer`。

#### W4 — 敌方追击决策权威边界
- `gdd-010` §7 定义「追击决策」条件公式（w_k…pursue_threshold）。
- `gdd-016` `ActionScorer` 选 StrategicAction（含撤退/强攻/佯攻）。
- 二者可能双重决策"敌将是否追击"。
- **待修（加一句边界）**：016 ActionScorer 选战略意图、翻成 010 基础命令；具体"追击"由 010 §7 作唯一战术结算点，两层不重复判定。

### PASS
- 公式区间兼容（016 believedEnemy 消费 007 区间；014 troop_cap 消费 004 governance）——无 range mismatch。
- 验收准则无跨文档矛盾。
- GDD_007 §AI Requirements L156（AI 拥有独立 FactionKnowledge、不读真值）与 GDD_016 反全知锁一致。

---

## 游戏设计问题（Phase 3）

### Blocking
无。

### Warnings

#### W5 — 反支柱「最优玩法只需刷战斗」风险
- `game-pillars` 反支柱之一。`gdd-014` 已给功绩/名望非战斗来源（治理城池/君主任务/招揽贤才/治理盛世）——护栏在。
- 但未规定非战斗来源**累积速率**与战斗来源相当。若打仗最快出功绩 → 刷战斗成支配策略，生涯退化为薄皮战斗沙盒。
- **待修**：014 Balancing Parameters 明确「名望各来源权重」使非战斗路径速率上有竞争力，并在后续体验验证确认（呼应前序 CD-C1）。

### INFO

#### N10 — 功绩/名望为单调门槛资源、无 sink
- `gdd-014` §1 `merit ≥ merit_req[rank+1]` 是门槛闸非消耗；merit/renown 只增不减 → 满阶后空转（"有源无汇"）。
- `lord_standing` [0,1] 可因任务失败下降（有源有汇，OK）。
- **可接受**（里程碑闸常态），建议要么加恩赏/赏赐 sink，要么显式接受为里程碑闸防晚期空转。

#### N-#5 — ADR-0006 随机源与 ADR-0004 契合
- ADR-0006 用 `seed=Hash(worldTick,factionId,planId)`；建议补一句明确**消费 ADR-0004 注入式确定性流**、在声明检查点取值，非旁路随机源。

### PASS
- **进度循环不竞争**：014 是明确连接层 meta 循环，010 战斗喂给它，无并列争"核心"。
- **注意力预算**：014/015 为战斗之间的战略层（时间切片），016 为敌方侧——不增加战斗核心时刻的并发玩家活跃系统（仍 ≤5）。
- **支柱对齐**：014→支柱1/2，015→支柱4 + 世界骨架，016→支柱5（给"自由布阵"深度）。
- **玩家幻想自洽（强）**：014「挣来的地位」+ 015「活在真实推进的三国」+ 016「敌将非增兵时钟」+ 010「靠条件创造胜机」共同强化同一身份。
- **设计锁**：016 维持条件涌现（效用评分→基础命令，非按钮），010 设计锁完好；未引入无条件计策按钮。

---

## 跨系统场景走查（Phase 4）

走查 4 个多系统时刻：

### Warnings
- **失城/守城结算**（010 + 004 + 015 + 014）：失城链 010 BattleOutcome → 004 控制权变更事件 → 015 city.owner → 014 失城罢官。三系统触归属 → 坐实 **W1**（已裁定单一权威）。

### Info
- **提前灭孙权→赤壁分叉**（015 + 014 + 016）：015 Edge Case 用「日界全局结算顺序」裁定历史事件与局部战役争城，已处理；016 战略层在新 reachable 势力上启用，一致。
- **自立发动**（014 + 006 + 015 + 016）：014 Edge Case「发动瞬间按结算快照判定」确定性处理；016 旧主转敌对与 015 一致。
- **AI 多日断粮计划**（016 + 010§8 + 011/012）：断粮单一权威链（012§5→011§1/§2→010 只读）已锁，PASS。

---

## 标记需修订的 GDD

| GDD | 原因 | 类型 | 优先级 |
|---|---|---|---|
| gdd-015 | 城池归属改为只读投影、订阅 004 事件（W1） | 一致性 | Warning（裁定已定，待落） |
| gdd-004 | 确立城级控制权唯一权威 + 加 014/015 反向依赖（W1/W2） | 一致性 | Warning |
| gdd-010 | 命名 TacticRecognizer（W3）+ 追击边界（W4）+ 列 016 为消费者（W2） | 一致性 | Warning |
| gdd-014 | System Outputs 归属措辞改订阅（W1）+ 非战斗功绩源速率权重（W5）+ merit/renown sink（N10） | 一致性/设计 | Warning |
| gdd-016 | 追击边界一句（W4）+ ADR-0006 补「消费 ADR-0004 注入流」（N-#5） | 一致性 | Warning（轻） |

---

## 裁定：CONCERNS

无 Blocking；多项 Warning。W1（城池归属）已由用户裁定为 **004 唯一权威 / 015 订阅 / 014 只读**，其余 W2-W5 多为补一句/加反向引用的轻量编辑。不阻断进入架构/实现，但建议落 W1 编辑后再让 014/015/016 由 Draft 推进，避免实现期撞权威。

### 后续建议
1. 据 W1 裁定改 015/004/014 三处归属措辞（轻量 Edit）。
2. W3/W4：010 命名 TacticRecognizer + 加追击边界一句；016 加边界一句 + ADR-0006 补随机源一句。
3. W2：旧 GDD 加 014/015/016 反向依赖。
4. 跑 `/consistency-check` 填充 registry 新跨系统名。
5. ADR-0006 转 Accepted 前确认 N-#5（随机源契合）。
