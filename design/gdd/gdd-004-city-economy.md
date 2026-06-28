# GDD_004 — 城市经济

- 状态：Implemented
- **Status**: Implemented
- 范围：Vertical Slice

## System Purpose

让城市成为战争的物质与政治基础。守军征用粮食、修复工事和派出兵力都会改变居民生存、民心、治安与失败后的恢复能力。

## Player Fantasy

玩家不是从无限仓库调兵，而是在“守住今天”和“让城市明天仍能活下去”之间做有后果的取舍。

## Core Loop

查看库存、民需和城况 → 分配粮食与工事投入 → 提交征用/修复/安抚等行动 → 到日界结算产耗与事件 → 将军需交给后勤 → 战果和损耗写回城市。

## Main Rules

- 城市拥有民用粮食库存、可拨军粮、民心、治安、工事状态、人口压力与控制方。
- 民用与军用粮食来自同一权威库存；拨给军队后转由后勤系统持有，不能重复计算。
- 日界按“已承诺投入 → 基础产入 → 民用消耗 → 短缺后果 → 工事/治安结算”稳定顺序处理。
- 资源不得低于合法下限。需求超过库存时生成短缺量，并转化为可解释后果，不凭空补齐。
- 修复工事需要时间、资源和可用人物；围城或敌军压力可改变效率。
- 民心与治安是不同状态：民心表达支持/怨恨，治安表达秩序与执行能力。
- 失城改变控制权、库存与人物后果，但不默认结束存档。

## Formulas

> 城市经济严守资源守恒：军用粮拨给后勤后转移所有权，不重复计算。日结按
> §Main Rules 的稳定顺序处理。所有产入/消耗/阈值来自版本化配置，标注单位与合法范围。

### 变量定义

| 变量 | 含义 | 范围 / 单位 | 来源 |
|------|------|-----------|------|
| `stock` | 当前粮食库存 | ≥ 0 | CityState |
| `reserved` | 已保留量（已承诺未执行） | 0..stock | ResourceStock |
| `available` | 可自由分配量 | ≥ 0（派生） | 派生 |
| `base_yield` | 基础日产入 | ≥ 0 | 配置 |
| `civ_demand` | 民用消耗需求 | ≥ 0 | 配置×人口压力 |
| `pop_pressure` | 人口压力系数 | ≥ 0 | CityState |
| `req_amount` | 本次征用军粮量 | ≥ 0 | 玩家命令 |
| `k_morale_req` | 征用对民心影响系数 | 建议 0.0–1.0 | 配置 |
| `STOCK_FLOOR` | 库存合法下限 | ≥ 0 | 配置 |

### 1. 可分配量（库存守恒不变量）

```
available = stock − reserved
```

- **不变量**：`reserved ≤ stock` 恒成立；任何分配先校验 `available ≥ 需求`，否则原子拒绝（见 §Edge Cases 同步分配冲突）。
- **示例**：stock=100，reserved=30 → available=70；欲分配 80 → `80 > 70` → 拒绝。

### 2. 日结账本（按稳定顺序）

日界依次结算，每步产出可解释条目：

```
stock_1 = stock_0 + base_yield                  （基础产入）
consumed = min(stock_1, civ_demand)             （民用消耗，不超库存）
shortage = max(0, civ_demand − stock_1)         （短缺量）
stock_2 = max(STOCK_FLOOR, stock_1 − consumed)  （结算后库存）
```

- **约束**：`stock_2 ≥ STOCK_FLOOR`，资源不凭空补齐；短缺转化为后果（见步骤 4）。
- **库存上限与和平期汇（防「源>>汇、断粮杠杆晚期失效」，ADV-2）**：`stock_2 = min(stock_2, STOCK_CEIL(fort_level, governance))`——库存有**上限**（仓容由工事/治理派生）；超出部分**不无界累积**：溢出按 `spoilage = (stock − STOCK_CEIL) × spoil_rate` 腐损丢弃，或转为可上缴/恩赏的外部 sink（喂 GDD_014 名望/君主好感）。使已稳城池粮食不无界膨胀，断粮/补给杠杆（GDD_010 六杠杆）晚期仍有效。`STOCK_CEIL/spoil_rate` 配置化。
- **示例**：stock_0=50，base_yield=20 → stock_1=70；civ_demand=90 → consumed=70，shortage=20，stock_2=0（若 FLOOR=0）。

### 3. 民用消耗需求

```
civ_demand = base_civ_consume × pop_pressure
```

- 人口压力为零时仍保留基础维护需求（见 §Edge Cases）：`civ_demand = max(base_maintenance, base_civ_consume × pop_pressure)`。
- **示例**：base_civ_consume=30，pop_pressure=1.5 → `30×1.5=45`。

### 4. 短缺后果（民心/治安传导）

> **命名消歧**：本系统的 `civ_morale` 指**城市民心**（居民支持/怨恨），与 GDD_011 的部队士气 `unit_morale` 是**两个不同权威系统的不同状态**，配置与代码不得混用同名 `morale`。

```
civ_morale' = clamp( civ_morale − k_shortage × shortage, 0, CIV_MORALE_MAX )
unrest_risk = (shortage > shortage_threshold) ? high : low
```

- 短缺越大，民心下降越多，超阈值触发骚乱风险（见 §Failure Cases 饥饿/骚乱）。
- **民心被动回升项（防单调下行死亡螺旋，ADV-4）**：非短缺、非高征用的稳定日，民心按 `civ_morale' = min(CIV_MORALE_MAX, civ_morale + recovery_rate × (1 − shortage_pressure))` **被动回升**——温饱且无骚乱时缓慢恢复，使民心成为**有源有汇**值，而非只靠"安抚"主动动作的单向下行。长围/连续征用仍压过回升（回升随 `shortage_pressure` 归零），符合"围城民心恶化"。`recovery_rate` 配置化（建议小于短缺扣减系数，使压力期净下行）。
- **示例**：civ_morale=60，k_shortage=0.5，shortage=20 → `60−0.5×20=50`。

### 5. 征用军粮对民心的影响

```
civ_morale' = clamp( civ_morale − k_morale_req × req_amount, 0, CIV_MORALE_MAX )
army_food += req_amount   （转移给后勤，城市 stock 同步扣减，守恒）
```

- 拨出后由后勤系统持有，城市不再计入（见 §Main Rules 不重复计算）。`k_morale_req` 为征用对**城市民心**的影响系数。
- **示例**：civ_morale=70，k_morale_req=0.3，req_amount=40 → `70−0.3×40=58`。

### 6. 工事修复

```
repair_done = min( fort_max − fort_cur, repair_rate × siege_mod )
fort_cur' = fort_cur + repair_done
```

- 受时间、资源和可用人物约束；围城/敌压通过 `siege_mod`（≤1.0）降低效率（见 §Main Rules）。工事满时多余投入不转其他资源（见 §Edge Cases）。
- **示例**：fort_max=100，fort_cur=80，repair_rate=15，siege_mod=0.6 → `min(20, 15×0.6=9)=9` → fort_cur'=89。

### 7. 失城资源处置（可控行动而非固定结果）

```
captured = stock × capture_ratio
burned   = stock × burn_ratio
moved    = stock × move_ratio
其中 capture_ratio + burn_ratio + move_ratio ≤ 1.0
```

- 各比例由玩家行动/守军命令决定，不是固定结果（见 §Open Questions）。剩余部分留城。

## Data Model

- `CityState`：城市 ID、控制方、库存、民心、治安、工事、人口压力。
- `ResourceStock`：资源类型、数量、已保留量。
- `CityDemand`：来源、时间窗、需求量、优先级。
- `CityAllocation`：用途、数量、批准者、开始/完成时间、状态。
- `FortificationState`：最大状态、当前状态、受损组件标签。
- `CitySettlementResult`：收支账本、短缺、状态变化和原因事件。

## Player Inputs

分配或撤回尚未执行的粮食计划；征用军粮；安排修复、安抚或守备；查看账本预测。所有操作通过有权限的角色命令提交。

## System Outputs

城市日结账本、可供后勤提取的军粮、工事防御状态、民心/治安变化、短缺与骚乱风险、控制权变化和战后恢复状态。

## Dependencies

依赖 GDD_001 时间、GDD_005 角色职责、GDD_006 权限关系和 GDD_003 城市区域。向 GDD_009、010、012 与失败后果提供资源和状态。被 GDD_014（生涯读城池资源/控制权）、GDD_015（世界模型订阅城池归属变更）消费。

> **城池控制权权威（跨系统裁定 2026-06-24）**：本 GDD 为**城级控制权的唯一权威**，独占「控制权变更事件」。GDD_015 世界模型在战略尺度**只反映**归属并**订阅**本事件，不独立写；GDD_014 生涯层**只读**控制权。历史事件导致的易主亦须经本 GDD 的控制权变更事件落地。

## Edge Cases

- 多个分配争用同一库存时，只允许已验证的原子承诺。
- 日界前城市易主时，按控制权变更事件定义谁承担结算。
- 工事已满时多余修复投入不自动转成其他资源。
- 人口压力为零仍保留基础维护需求，具体规则由配置定义。

## Failure Cases

库存不足、权限不足、重复分配、无效资源类型或工事目标不可修复时拒绝命令且不扣除资源。饥饿、骚乱和失城是 gameplay 后果，必须提供继续状态。

## Balancing Parameters

基础产入、民用/军用消耗、库存容量、征用对民心影响、短缺阈值、治安变化、修复成本/耗时、工事防御贡献、围城修正和恢复速度。全部来自版本化配置并标注单位与合法范围。

## UI Requirements

显示当前库存、已保留量、下次日结预测、每项来源和去向；军粮征用前展示对民用供给的影响区间；民心与治安不得合并；城市受损在地图和面板持续可见。

## AI Requirements

城市 AI/敌方后勤共享资源守恒规则。MVP 敌营经济可简化为显式场景库存，但不得无限生成；AI 需保留最低生存/补给预算并根据风险调整。

## Save / Load Requirements

保存库存、保留量、需求、分配、工事、民心、治安、控制方和下一结算时间。读档 round-trip 后账本与下一日结必须一致。

## Test Requirements

测试资源守恒、同步分配冲突、短缺结算、工事上限、权限失败无副作用、城市易主、军粮向后勤转移、失城继续状态和日结确定性。

## MVP Scope

一座小城；粮食为唯一完整流转资源；民心、治安和工事为核心城市状态；基础修复、征用和日结。不做税收、产业链、市场和人口职业模拟。

## Future Scope

多资源生产、税收、贸易、建筑、人口阶层、官僚、灾害、难民、长期围城和城市间运输网络。

## Open Questions

- 军粮拨出后，在离开城市前由城市还是后勤系统显示为“在途保留”？
- 民心与治安对征募/守城的影响需要直接规则还是通过事件与执行意愿传导？
- 失城时粮食被缴获、焚毁和转移的默认比例如何表达为可控行动而非固定结果？
