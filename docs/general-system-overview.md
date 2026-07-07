# 武将系统总览（General System Overview）

> 状态：后端全线完成，仅剩 Unity 表现层。最后更新 2026-07-07。
> 关联：GDD_027（武将全局融入）· GDD_005（人物系统）· GDD_025（武将标签/隐秘心/羁绊）· GDD_026（生卒/纪元）· ADR-0016（归属层）· ADR-0017（运行时人生态）。

## 一、现状定位

武将已从「可查询的静态资料库」升级为「**在世界里持续变化、影响战争的活人**」——即从"档案层"跨到"活人层"。后端**再无缺口**，仅剩 Unity 表现层（招揽/人生/任用屏）。

**规模**：500 武将 · 8 纪元锚点（184/190/194/200/208/219/220/234）· 10 桩演义事件 · dotnet **1217 测试全绿** · 3 DLL 同步。

## 二、系统全景

### 数据层
- 500 武将档案：标签 + 隐秘心（忠义倾向/野心）+ 战阵档 + 谋略档（**非数值卡**，玩家反全知只见名声与已知标签）。
- 8 纪元归属派生（在职/在野/未出世）+ 生卒门 + 驻城。
- 内容完整性校验守卫（`GeneralDataValidation`）：悬空引用/非法生卒/重复布防，CI 拦一类静默数据 bug。
- 数据外部化管线（`GeneralDossierCodec` + `assets/data/generals.tkdata` + `GeneralDossierLoader`）：记录↔对象保真。

### 战斗层
- **守将进战斗**：守城武将按标签择位（善守/铁骨镇正面、诡谋/远图护粮道），携标签/战阵档/谋略档/羁绊入结算。
- **副将分区**：主将坐镇招牌列，副将 `TakeBest` 补次列。
- **三维差异化**：统率/武勇/智略从战阵档/谋略档 + 标签确定性派生（吕布高武低智、诸葛低武高智），围绕旧均值散开无平衡回归。
- **守备接城池**：守军取世界大盘真值（邺900/虎牢600/小沛400），工事随城规模分级。
- **战斗界面可见**：我方支队显主将名；守方将领反全知投影（未侦察→「未探明之将」，已侦察→真名如「高顺」）。

### 招揽层
- 反全知知晓状态机（`TalentRecruitment`）：未闻名→听闻→定位→接触→招揽（入伙/婉拒/结怨/投敌）。
- 发觉渠道（侦察/军师/羁绊/事件/访贤）+ 确定性招揽结算（阻力=难度−待遇+屡拒累积）。
- `KnownPool` 只呈已闻名者——替代裸 `PoolAt`，堵住"UI 直调露全部在野将"的反全知漏洞。

### 运行时人生层（GeneralState / ADR-0017）
- 每将运行时态：当前主君/驻城/忠诚(0..100)/健康(康健·负伤·重创)/疲劳/被俘/**记忆**。不可变（With\* 返新）、确定性。
- 记忆（被背叛/救命/受赏/羞辱/宽宥/拔擢）**联动忠诚**并长期留痕；叛离风险由忠诚−被背叛记忆派生（反全知定性档）。
- **自动流动**：
  - 演义事件 → 台账（`GeneralLifeReconciler`：斩杀标重创、移籍换主，幂等）。
  - 战斗结算 → 台账（`ConcludeOffensive`：参战积劳、败则主将受创、**破城俘敌将**）。

### 演义事件引擎（GDD_027 R6 / ADR-0016 D6）
- 10 桩关目：桃园结义/温酒斩华雄/三英战吕布/三顾茅庐/许攸夜奔/白门楼/赤壁鏖兵/水淹七军/败走麦城/秋风五丈原。
- 产 typed 效果（登场/移籍/斩杀）→ 确定性重放覆盖层，优先于 baseFaction 改写归属。可推演不入档（触发只依赖已持久化态）。
- 触发时机：有效果事件推进过纪元锚点年后方生效（开局快照干净，残局不在第一回合殒星）。

## 三、原始 7 大问题 → 解决对照

| # | 问题 | 结果 |
|---|---|---|
| 1 | 无运行时人生 | ✅ GeneralState + 自动流动 + 入存档 |
| 2 | 招揽无反全知门 | ✅ 知晓状态机 + 堵 pool 泄漏 + 入存档 |
| 3 | 守将不进战斗（最严重） | ✅ 机制 + 单测 + Unity 界面可见 |
| 4 | 副将不进区域 | ✅ 分区部署 + 界面显名 |
| 5 | 三维压平 / 维护性 | ✅ 派生差异 + 外部化管线 |
| 6 | 守备脱节城池 | ✅ 取真实守军 + 工事分级 |
| 7 | 数据地雷 / 无校验 | ✅ char-jiling 修复 + 校验守卫 + 纪元覆盖史准 |

## 四、存档持久化

三本账均经伴生存档槽（`slot.generals`/`slot.talents`/`slot.appoint`）跨存读档存活，向后兼容旧存档（无此槽 → 空），均有 round-trip 单测 + console 实测：
- **人生态**（忠诚/记忆/健康）— `GeneralLedgerCodec`
- **招揽发觉进度** — `TalentKnowledgeCodec`
- **太守任用** — `AppointmentCodec`

## 五、验证情况

- **后端**：1217 单测全绿 + console 端到端实测每条链。
- **Unity 编辑器**（computer-use + Claude 独立核图，证据在 `production/qa/evidence/s23*`）：编译 0 error · HUD/战略图/战斗全场景无恙 · 出征全链走通 · 守将界面可见（「敌将：高顺」侦察现名）。

## 六、剩余（全部为 Unity 表现层，★编辑器）

- **招揽屏 / 人生屏 / 任用屏**（console 均可操作，Unity 未接屏）。
- **权威源真正切外部文件**（须 Unity StreamingAssets 装载 + 启动装配）。
- **副将 UI**（`HudController.AddDeputy` 现硬编码限 1 副将；放开 2 副将 + 可选谁——纯 UI 改，后端已支持）。

**武将后端到此再无缺口。**

## 七、关键代码索引

| 系统 | 位置 |
|---|---|
| 归属层 | `src/Application/Scenarios/GeneralAffiliations.cs` |
| 档案数据 | `src/Application/Scenarios/GeneralDossiers.cs` |
| 出征将投影/守备 | `src/Application/Scenarios/PlayableCampaign.cs` |
| 招揽状态机 | `src/Application/Scenarios/TalentRecruitment.cs` |
| 演义事件引擎 | `src/Application/Scenarios/LoreEvents.cs` |
| 事件↔人生桥 | `src/Application/Scenarios/GeneralLifeReconciler.cs` |
| 运行时人生态 | `src/Domain/Characters/GeneralState.cs` · `GeneralLifeService.cs` |
| 守/副将分区 | `src/Application/Battle/OffensiveDeploymentPlanner.cs` |
| 战斗界面投影 | `src/Presentation/Screens/ZoneBattleView.cs` |
| 持久化编解码 | `src/Domain/Persistence/GeneralLedgerCodec.cs` · `AppointmentCodec.cs` · `Application/Scenarios/TalentKnowledgeCodec.cs` |
| 运行期装配 | `src/Presentation/Runtime/CampaignRuntime.cs` |
