# 架构审查报告

> **日期**：2026-06-21（复审，覆盖同日初版）
> **引擎**：Unity 6.3 LTS
> **审查 GDD 数**：13
> **审查 ADR 数**：5（ADR-0001~0005，全部 Accepted）
> **模式**：full

---

## 可追溯性摘要

- 总需求：31
- ✅ 覆盖：28（90%）
- ⚠️ 部分：3（10%）
- ❌ 缺口：0

> **复审对比**：初版（同日早些）为 1 覆盖 / 12 部分 / 17 缺口（裁定 CONCERNS）。
> 此后 ADR-0002/0003/0004/0005 全部撰写并 Accepted，缺口清零。
> 注：初版报「30 总需求」系少计 1 条；当前注册表实有 31 条 TR，本报告以 31 为准。

## 完整可追溯矩阵

| TR-ID | 系统 | 需求摘要 | ADR 覆盖 | 状态 |
|---|---|---|---|---|
| TR-time-001 | 时间 | 权威时间 WorldDay+DaySegment，稳定全序 | ADR-0004 | ✅ |
| TR-time-002 | 时间 | 嵌套 BattlePhase 按预算消耗、跨时段结算 | ADR-0004 | ✅ |
| TR-time-003 | 时间 | 行动/期限/取消可存档 round-trip | ADR-0005 | ✅ |
| TR-weather-001 | 天气 | 天气转移由配置权重 + 确定性随机流 | ADR-0004 | ✅ |
| TR-weather-002 | 天气 | 环境只产具名修正，消费者自取 | ADR-0002 | ✅ |
| TR-map-001 | 地图 | 拓扑图、视觉坐标不入 Domain 结算 | ADR-0002 + ADR-0004 | ⚠️ |
| TR-map-002 | 地图 | 确定性寻路 + 容量门控 + 接触判定 | ADR-0004 | ✅ |
| TR-map-003 | 地图 | 真值与阵营知识分离 | ADR-0002 + ADR-0005 | ✅ |
| TR-city-001 | 城市 | 民/军粮同源、守恒 | ADR-0002 | ✅ |
| TR-city-002 | 城市 | 日界稳定顺序结算 | ADR-0002 | ✅ |
| TR-character-001 | 人物 | 能力影响过程质量，不解锁无条件技能 | ADR-0002 | ✅ |
| TR-character-002 | 人物 | 职责决定合法权限 | ADR-0002 | ✅ |
| TR-relationship-001 | 关系 | 方向性多维、事件幂等 | ADR-0002 | ✅ |
| TR-relationship-002 | 关系 | 关系只影响承诺、授权受约束 | ADR-0002 | ✅ |
| TR-intel-001 | 情报 | 四层分离、UI 只读知识 | ADR-0002 | ✅ |
| TR-intel-002 | 情报 | 置信度/时效/确定性暴露 | ADR-0004 | ✅ |
| TR-intel-003 | 情报 | 真值与知识分别序列化 | ADR-0005 | ✅ |
| TR-council-001 | 军议 | 知识快照、过时标记 | ADR-0002 | ✅ |
| TR-council-002 | 军议 | 军师只输出条件化建议、不自动下令 | ADR-0002 | ⚠️ |
| TR-prep-001 | 战前准备 | PlanDraft 无副作用、原子提交 | ADR-0002 | ✅ |
| TR-prep-002 | 战前准备 | 硬冲突阻止提交、命令图 DAG | ADR-0002 | ✅ |
| TR-battle-001 | 战斗 | 确定性模拟 + 状态哈希 | ADR-0004 + ADR-0003 | ✅ |
| TR-battle-002 | 战斗 | 兵法涌现、无无条件按钮 | ADR-0004 | ✅ |
| TR-battle-003 | 战斗 | 阶段管线原子回滚 | ADR-0004 | ✅ |
| TR-cohesion-001 | 士气疲劳 | 三维独立、幂等聚合 | ADR-0002 | ✅ |
| TR-cohesion-002 | 士气疲劳 | 阈值多因素、加权拆分 | ADR-0004 | ✅ |
| TR-supply-001 | 后勤 | 三类持有者守恒、转移不重复 | ADR-0002 | ⚠️ |
| TR-supply-002 | 后勤 | 断粮按时段确定性传导 | ADR-0004 | ✅ |
| TR-save-001 | 存档 | 版本化 DTO + 原子写入 + 迁移链 | ADR-0005 | ✅ |
| TR-save-002 | 存档 | round-trip + 随机流位置 | ADR-0005 | ✅ |
| TR-save-003 | 存档 | 加载兼容判定 + 配置指纹 | ADR-0005 + ADR-0003 | ✅ |

## 覆盖缺口（无 ADR）

**无。** 0 缺口。

## ⚠️ 部分覆盖说明（均由通用架构原则实质覆盖，非架构空白，无需新 ADR）

三者都被 ADR-0002/0004 的通用原则覆盖，但相关 ADR 的「GDD Requirements Addressed」
表未显式列名该 TR-ID，属可追踪性收尾：

1. **TR-map-001**：架构约束（视觉坐标不入 Domain）由 ADR-0002 层隔离 + ADR-0004 权威路径
   禁 float 覆盖；拓扑图数据模型本身是 Domain 建模选择，无需独立 ADR。
2. **TR-council-002**：「军师不绕过命令路径」由 ADR-0002 唯一写路径覆盖；「不输出综合成功率」
   是设计/UX 规则，非架构决策。
3. **TR-supply-001**：与 TR-city-001 同构，由 ADR-0002 单一权威 + 原子命令路径覆盖，
   但 ADR-0002 GDD 表只列城市未列后勤。

**建议（可选，低优先）**：ADR-0002 表补 `TR-supply-001`、`TR-council-002`，
ADR-0004 表补 `TR-map-001`，即升为 ✅，无需任何设计或代码变更。

## 跨 ADR 冲突

🟢 **未发现冲突**。数据所有权、集成契约、性能预算、架构模式、状态权威逐项核对一致。

**轻微可追踪性瑕疵（非冲突）**：ADR-0005 的 `loadable()` 依赖 ADR-0003 的配置指纹定义，
但 ADR-0005「Depends On」只列 0001/0002/0004，把 0003 放在「协同/Related」。
建议将 ADR-0003 提为 ADR-0005 的显式 Depends On。

## ADR 依赖顺序（拓扑排序）

```
基础层（无依赖）:   ADR-0001（Unity + C#）
依赖基础:          ADR-0002（架构分层）← 0001
特性层（并行）:     ADR-0003（数据驱动配置）← 0001,0002
                  ADR-0004（确定性战斗）← 0001,0002
汇聚层:            ADR-0005（存档迁移）← 0001,0002,0004（+0003 指纹）
```

🟢 无依赖环；无悬空依赖（5 份全 Accepted）。

## GDD 修订标记（Architecture → Design 反馈）

**无。** 所有 ADR 的 `Post-Cutoff APIs Used` 均为 None，无已验证引擎行为与任何 GDD 假设冲突。

## 引擎兼容性

| 检查 | 结果 |
|---|---|
| 版本一致性 | ✅ 5/5 ADR 均 Unity 6.3 LTS |
| Engine Compatibility 节 | ✅ 5/5 齐全 |
| Post-Cutoff API 冲突 | ✅ 无（全部声明 None） |
| 弃用 API 引用 | ✅ 无 |
| 陈旧版本引用 | ✅ 无 |

> **引擎专家会诊（unity-specialist）**：本轮跳过——5 份 ADR 均无 post-cutoff/引擎特有 API
> 依赖，会诊增益极低。需要时可单独触发。

## 架构文档覆盖

`docs/architecture/architecture.md` 存在，被全部 5 份 ADR 引用为草案来源。
未发现孤立架构。建议后续轻量交叉核对 13 系统在总览分层中均有位置（非阻断）。

---

## 裁定：CONCERNS（极轻微，无阻断项）

实质已达 PASS 水平：**0 缺口、0 冲突、0 依赖环、引擎完全一致**。
仅 3 处「部分」是可追踪性列名收尾（非架构空白），故按规则不给纯 PASS。**无阻断项。**

### 阻断项

无。

### 建议后续 ADR

无（架构覆盖已完整）。仅建议对现有 ADR 做 3 处可选的可追踪性列名补充（见上）。

### Pre-gate 清单（进入 /gate-check pre-production 前须补齐）

- ❌ `tests/unit/`、`tests/integration/` 目录 → 运行 `/test-setup`
- ❌ `.github/workflows/tests.yml` → 运行 `/test-setup`
- ❌ `design/ux/accessibility-requirements.md` → 运行 `/ux-design`
- ❌ `design/ux/interaction-patterns.md` → 运行 `/ux-design`
