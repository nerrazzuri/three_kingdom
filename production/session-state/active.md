# 会话状态 — 大方向锁定（游戏整体定位）

## 📍 下一会话从这里续（2026-07-07 交接）

> **🏁 阶段收官：武将全局融入（GDD-027 / ADR-0016）后端全线完成。** P1-P8 八系统 + s21 复验通过 + 演义事件引擎完整（10 桩关目·登场/移籍/斩杀·确定性重放覆盖层）+ 运行期透传（城册认覆盖层，内政/军师/成军/凝聚自动跟随）+ 触发时机（推进后生效·开局快照干净）。**dotnet 1167 绿；3 DLL 同步。最新提交 e187091 已 push tk。** 详见续22 / 续22b。
>
> **下一会话可选方向（后端已闭环，均待用户拍板）**：
> 1. **P9 表现层（Unity 屏）★编辑器**：招揽屏列在野池 / 城册任用屏 / 内政·军师显武将加成 / 演义事件通报流。仿 CampaignMap 套路（UI Toolkit 建屏 + SliceSceneBuilder 重跑 + computer-use 验）。目前效果在 console 完整可玩可验，Unity 侧尚无 roster 屏消费方。
> 2. **① baseFaction 纪元覆盖细化**：现静态简化 → 190 小沛册混进黄权/杨仪/董和等史实此年未归刘备者。要史准需给归属加「(将,纪元)→势力」覆盖数据（数据活，非引擎改；部分可由演义移籍效果在事件时点纠正）。
> 3. **P3 任用态未入存档**（会话内）——若要持久化需接 CampaignSession 存档 head（同「发觉门」那类），P9 之外一条小线。
> 4. **触发时机可翻**：如想让 234/220 残局盘「开局即那悲壮时刻」（诸葛亮/关羽开局即殁），把 `OverridesAt` 的 (Anchor,Current] 改回 [Anchor,Current] + `FiredAt` 去掉 afterAnchor 门即可（一处改动 + 调时机测试）。
>
> **不要重复做**：武将 500 员 / 8 纪元 / 反全知战略图雾 / 战略图屏 / 发觉入存档 / 演义事件引擎（10桩·效果·覆盖层·运行期透传）均已完成并验证（见下续13-续22b 各段）。



## 🎨 里程碑（2026-07-08）— Phase 2 美术：签名风格锁定 + 关羽立绘定稿 + MVP 背景全出

> **Midjourney 生产管线跑通，"军府活图卷"美学落地为可用资产。产出在 `D:\Projects\三国演义\UI Test\UI\final`。全程遵守 §9.4 原创红线（不得像光荣三国志/真·三国无双）。**
> - **MVP 背景 4 张已 Upscale 定稿**：主菜单(zm-0) / 坚城(jc-3) / 遮蔽(zb-2) / 粮道(ld2-2，旧带辎重车版已弃)。签名 A 背景锚 bg3-1 仍待补 Upscale（低优先，4 张够用）。
> - **关羽立绘定稿（10 轮迭代）**：v1.4.6 prompt（§5.28.3）产出 gy10 批。经典关羽神韵完整——绿幞头(帽子非扎布)+红脸满饱和+黑长髯+绿袍+米纸底；缩略图焦点(红脸)够亮。
>   - **解耦锁定（§5.29.4）**：关羽**展示立绘**=gy10-3（用户审美选稿，水印已 PS 去除=final/wmremove-transformed.png，Claude 复核无接缝）；30 将 **`--sref` 风格锚**=gy10-2（布光均匀·缩略图安全）。二者可不同。
> - **✅ gy10-2 锚图已 Upscale 入 final**（2026-07-08，`..._Three_quarter_portrait_bust..._3fa4e042....png`，Claude 复核=布光均匀·缩略图安全版）。落地 `--sref` 时须用其 Midjourney 图 URL（右键 Copy Image Address）或直接把该图拖进 prompt 框。
> - **裁定沉淀**：art-bible §3.1 补充裁定四（关羽绿主红辅命名例外，阵营识别靠小红领口 + UI 阵营边框/徽章）；§1.2 UI 框portrait 免"同笔"一致性。prompt-pack 已推进至 v2.6（art-director 维护 §5.x/§6 registry）。
> - **✅ 诸葛亮立绘定稿（2026-07-08）**：`final`/`530823f0`（Upscale Creative 版）。高诸葛巾（竖褶银灰）+ 手持大羽扇 + 白/红袍 + 年轻清朗睿智脸 + 游戏厚涂。经十余轮试出的关键教训（已由 art-director 沉淀入 §5.30）：①**去 --sref 人物锚**——拿人物立绘当 sref 会传导神韵令众人变关羽复制人，30 将统一画风改靠共享风格文字段落（更正原 §5.29.4）；②MJ 审核拦写实人物→须声明"绘画非照片"，但框定词用 `video game key art/digital painting`，**绝不能用 `traditional Chinese gouache/ink painting`**（滑向工笔绢本/故事书）；过审去 realistic/photo/caucasian、负面避堆 weapon/screaming/muscular/vampire；③MJ 默认把"东亚大胡子男+game splash"画成猛将→文官须显式"年轻俊朗+柔光脸+少须+压猛将负面"；④招牌道具要"明确手持/正戴 prominently visible"，别写 understated；⑤收尾用 Upscale (Creative) 精细化而非重 roll。
> - **✅ 曹操立绘定稿（2026-07-09）**：`73ae56db` c_2（待 Upscale Creative 入 final）。高金饰冠 + 铁苍蓝袍 + 睥睨枭雄。**新增关键裁定——曹操 vs 司马懿神韵须掰开**：首版曹操写了 `cold/reserved/calculating` 结果撞司马懿路子（且沾光荣冷脸谋士味，踩 §9.4 边）；改判——**曹操=外放霸主（ambitious/charismatic/imperious/robust，外露野心）**，**司马懿=内敛谋主（cold/reserved/hawk-eyed/elder，内藏算计）**，两条 prompt 神韵/相貌/头冠（曹操高金饰冠 vs 司马懿低炭灰冠）显式互斥。曹操 v2 负面加 `koei rotk portrait style / cold reserved elder schemer / hawk-eyed wolf-turning`。观察：v2 背景易跑战火红底（dramatic 词拉的），可留（战意）或加 `red/crimson flame background` 负面换米纸。
> - **✅ 司马懿立绘（2026-07-09）**：`b0595d56` 一发即成——冷诈隐忍谋主、低炭灰冠（区别曹操高冠）、铁苍蓝+云纹滚边、灰白须显年长、鹰视狼顾。待选 s_0（鹰视狼顾侧目最传神）/ s_3（灰须更显年长）。
> - **✅ 吕布立绘（2026-07-09）**：`d477df46` 一发即成——骄矜暴烈飞将、紫金冠+雉羽、紫垒袍+兽纹甲、年轻俊猛（此位神韵本就该张扬故不压）。待选 l_1 / l_3（l_0/l_2 偏平面海报弃）。
> - **🎉 五位核心立绘全跑通**：关羽(gy10-3 final)/诸葛亮(530823f0 final)/曹操(c_2)/司马懿(s_0或s_3)/吕布(l_1或l_3)。曹操/司马懿/吕布待用户拍板选图 + Upscale Creative 入 final。
> - **✅ 收官**：art-director 已把最终四条 prompt 回填 §5.30 + 登记 §6（后台完成）。**通用法已验证：游戏原画框定/过审词表/柔光脸治猛将/招牌道具显著/角色神韵互斥(曹操外放霸主↔司马懿内敛谋主)/Upscale Creative 收尾——后续 ~30 将可直接套用。** gy10-2 仅作关羽自身参照，不再作跨角色 sref。

## 🖼️ 里程碑（2026-07-09 s25）— 立绘/背景接进 Unity（编辑器验证 PASS）

> **首个"美术落地进游戏"闭环。computer-use 编辑器验证 PASS，Claude 独立核图坐实。证据 `production/qa/evidence/s25-portrait-ui-2026-07-09/`（01-mainmenu / 02-roster / DONE.flag）。**
> - **资产入库**：`Assets/Resources/Portraits/{关羽,诸葛亮,曹操,司马懿,吕布}.png` + `Assets/Resources/Backgrounds/main-menu.png`（背景 ASCII 名避 USS `resource()` 中文风险；立绘保中文名，C# 按 `card.Name` 取）。
> - **武将录立绘位**（`RosterController.cs`）：每行加 64×85 立绘（`Resources.Load<Texture2D>("Portraits/"+中文名)`，有图显立绘、无图深色剪影占位）；反全知不变（仍只名+性情，立绘不泄数值）。**零维护扩展**：以后加新立绘只丢文件进 Resources，代码不动。**关键**：`SessionRuntime.Roster()=GeneralRosterView.Build()` 静态全谱，武将录可独立 Play 验证不需开局。
> - **主菜单背景**（`MainMenu.uxml` 补挂 `<Style src>`[此前漏挂 uss 未生效] + `MainMenu.uss` 加 `background-image: resource(...)`+`background-size:cover`+标题/按钮半透宣纸衬底）。
> - **验证 PASS**：编译0错(仅1条 Input Manager 弃用黄警)；主菜单活图卷背景+文字可读；武将录关羽立绘缩略图脸清晰、余者剪影占位。
> - **遗留非阻断**：武将录面板透天空盒（老问题「面板非不透明底」，归美术/USS 打磨）。
> - **下一步候选**：① HUD 主将立绘 + 战斗/城池背景接入；② 势力徽章/边框（立绘靠 UI 框标阵营，尤其关羽绿袍/诸葛亮白袍不靠袍色识别）；③ 扩产其余 ~25 核心武将立绘（套已验证通用法）；④ 武将录面板不透明底美术打磨。

## 🔀 方向转折（2026-07-20）— 换皮不够，用户转向"格子实时战斗"重造，原型已验手感

> **换皮 M1-M3（下方 s28）发布后，用户判定"更像棋盘、不是要的"。** 要的完整战斗：真地形（隘口伏击/粮仓火攻，复用现有兵法涌现空间化）+ 2.5D 等距 + 按已有 DaySegment(黎明/白昼/黄昏/夜)推进 + 兵种定速行军(设目的地→进行→按速走，BFS 绕山过隘) + 半路遭遇临机抉择 + 时间段相邻格交战 + **补给硬约束(充足回血/不足叛逃+被打×3，粮仓/补给线=战略目标)**。
> - **丢弃式网页原型验证**：`prototypes/grid-battle/battle-prototype.html`（Artifact d8d1d968-9bd1-4752-b28e-95a5e65271ad）v2 含全部机制。**用户裁定"手感还可以、数值都好说"= 方向通过。**
> - **⚠️ 记忆 project-battle-real-map-reskin 已更新**：原"否决重造"作废，转格子重造。
> - **下一步（待用户承诺）**：进正式设计 GDD(8段)+新 ADR(**supersede/extend ADR-0012** 命名区域→格子，保确定性+兵法涌现)，再分期重造（2.5D 为呈现分期）。换皮 M1-M3 大半被取代、概念部分可复用。

## 🚧 里程碑（2026-07-19 s28）— 战斗真地图化 M1（换皮·地图布局骨架）编译验证通过 + 已提交

> **大方向（用户 2026-07-19 裁定，见记忆 project-battle-real-map-reskin）**：游戏太静态（卡片+数字），战斗要**真地图 + 兵马沿路行军 + 玩家拖动布防**。方案=**A 换皮**（纯呈现层，引擎 ADR-0012 不动，1261 测试全保）；**否决**自由坐标战棋（重造）。战斗优先。分期 M1 地图布局骨架→M2 行军动画→M3 拖动布防。
> - **M1 完成（纯 Unity 侧，不动 DLL）**：`ZoneBattle.uxml` 删固定方格→地图画布 `zb-map`+连线层 `zb-edges`；`ZoneBattle.uss` 底图移到 `.zb-map`+新增 `.zb-node`(绝对定位 translate 居中)/`.zb-edges`；`ZoneBattleController.cs` 加**数据驱动锚点表** `ZoneAnchors`(zone-id→归一化坐标)+**邻接表** `Edges`(对齐 StandardAdjacency 6 路)，Render 按锚点把区域节点绝对定位到地图上，`DrawEdges` 用 Painter2D 描夯土路连线；FillZoneSlot→FillZoneNode。
> - **验证（本轮用户免 computer-use，走现有线）**：编辑器开着锁项目→batchmode 走不了；改用 **csc + Unity 参考程序集(139)+3 Plugins DLL 直接编译 19 个 Assets/UI 脚本**——退出 0、无 CS 错、程序集产出。**C# 编译已验干净**；**视觉（节点摆位/连线渲染/USS）未验**，留用户开着的编辑器 Ctrl+R+Play 一眼。s28 CHECKLIST 已备（production/qa/evidence/s28-battle-map-m1-2026-07-19，无 DONE.flag=本轮免机验）。
> - **锚点(占位坚城底图上)**：关城中央★(0.52,0.44)/隘口左(0.16,0.52)/高地左上(0.30,0.18)/粮道右上(0.82,0.24)/预备下方(0.50,0.82)。真战场地图一到只改这表即热换。
> - **并行待产**：真战场地图美术（俯瞰战场图，MJ prompt 已给 art 侧，挂签名图A --sref）。
> - **M2+M3 完成（纯 Unity 侧，不动 DLL；csc 编译校验 0 CS 错）**：
>   - **M2 行军动画**：兵马从区域卡拆为**独立可移动图元**（zb-troops 层，按 detachmentId 持久复用）；地点标记退化为固定 zb-regions（区名★+敌情+条件）。兵马摆到所在区锚点（同区下方错开），`transition: left,top 0.45s` → **换区时沿位置补间"行军"**（非瞬移）。上一帧原区→这一帧目的区（域内 Location=目的区）自然演出行军，无需 view-model 改。
>   - **M3 拖动布防**：`AttachDrag`+`DropTargetZone`——拈起兵马跟随指针→松手落**相邻合法区**发 Move 命令（否则回弹）；姿态按钮按压不触发拖拽；键盘调动列表仍无障碍兜底。落点用 view.MoveOptions 校验。
>   - 提交 6180331(M1)→本批(M2/M3)。验证清单 s28-battle-map-m2m3-2026-07-19（编译已验；视觉/交互留用户编辑器 Play）。
>   - **遗留**：图元大小/错开偏移/落点半径(0.16) 待看后微调；在途"就位目的区标·在途"，真·画半路需 view-model 加起讫区（后续小步）。

## ✅ 里程碑（2026-07-19 s27）— 视觉战斗场景 P1-P3 编辑器验证 PASS + 已提交

> **s27 编辑器验证 PASS（DONE.flag 见 production/qa/evidence/s27-battle-scene-2026-07-11/）**：Unity 6.3 LTS 重编译 0 错，Play 推进 1→4 回合破城取胜，Console 全程无红错。5 项判据全过——① 车道方格 5 区摆位 + 正面关城★金框 ② 旗帜兵力分档（我红竖旗/敌蓝倾斜旗，观测 敌200=3旗/120=2旗/70=1旗）③ 将领名牌（剪影占位+阵营金框，敌将反全知不泄身份）④ 姿态三态 UI（主攻朱红高亮）⑤ 推进回合刷新无报错。P3 涌现徽章亦验（侧翼隘口「伏兵突然性」绿◆）。证据 01-battlefield.png 已存。
> - **非阻断遗留（记后续微调）**：① 侧翼隘口卡元素较多时名牌文字压住姿态三键，纵向布局需微调；② 兵力雾化区间「敌 约X–Y」/「未探明之将」/旗帜溢出"+N" 本轮场景未触发（敌军均已侦察、兵力未超封顶），仅确认代码机制在位，建议后续用未侦察高兵力敌区补正向截图。
> - **已提交并推送**：`c28853c`（20 文件 +3226/-58）已 push **tk/main**（26f4329..c28853c）。内容=ZoneBattleView.cs + ZoneBattle.{uxml,uss,Controller} + SessionRuntime/ZoneBattleSession + 3 DLL（Release 同步）+ ZoneBattleViewFogTests + 设计规格（visual-battle-scene GDD / battle-scene-art-spec v1.1 / midjourney-prompt-pack / art-bible / art-director memory）+ s27 验证证据。**未纳入本批（仍在工作树）**：Assets/data/generals.tkdata（独立武将数据，另提）、11 个 .unity 场景（纯 fileID 重编 churn，SliceSceneBuilder 重跑所致，不提交）、_diag2.txt 等 scratch。
> - **待产美术（不挡提交）**：3 地形背景（隘口/渡口/平原，prompt 备 §8.1-8.3）+ 真 sprite（旗帜/涌现/姿态/节点框，规格见 battle-scene-art-spec）。地形切换代码待素材出图后一处小接。

## 🏗️ 原始记录（2026-07-10 s27）— 视觉战斗场景 P1-P3 建造（用户批准，后台建造中）

> **愿景：战斗屏依城市地形生成、山路埋伏/旱林火攻、sprite 军队带将。核实后关键结论——逻辑后端全有（ADR-0012 命名区战场 + BattleFieldCatalog.ForTerrain 逐地形 + 火攻/水攻/埋伏/夜袭涌现 + 兵力/将领投影），缺的纯是视觉呈现。**
> - **规格已出并经用户批准**（先设计后建造）：`design/gdd/visual-battle-scene.md`（呈现设计 8 段）+ `design/art/battle-scene-art-spec.md` v1.1 + prompt-pack §8（隘口/渡口/平原 3 背景 prompt）。
> - **用户拍板设计**：布局=车道方格（5区+6邻接连线对应 StandardAdjacency）；兵力=旗帜图元堆叠（15%容量/档，封顶7转+N）；两缺口一并修——① `ZoneBattleView.EnemyStrength` 漏反全知门（补区间估计 FogBandWidth0.20 5档）② `SetPosture` 有命令无界面（补姿态三态UI）。
> - **建造（3 个后台 agent 全空转没交付 → Claude 自建，同 s25/s26 节奏，分期验）**：
>   - **✅ P1 完成（Claude 手建，dotnet 1254/1254 绿）**：`ZoneBattleView` 加 `ZoneCapacity`（旗帜分档基准）+ `ZoneBattle.uxml/uss/Controller` 重构为车道方格战场（5区槽位按 zone-id 填）+ 队列旗帜兵力（F1：ceil(兵力/容量/0.15)、非零≥1、封顶7转+N）+ 将领名牌（立绘缩略+阵营/敌我色边框，反全知敌将未探明）+ 键盘可达调动列表 + 涌现横幅。真 sprite 未出→USS 占位（我方红竖旗/敌方蓝倾斜旗）。**待编辑器验证**（清单 s27-battle-scene-2026-07-11，Monitor 盯 DONE flag）。
>   - **✅ P2-fog 完成（Claude 手建，dotnet 1261/1261 绿）**：`ZoneBattleView.FogBand`（反全知 F2 区间估计，**整数运算**避浮点 floor 误差[0.6/0.2=2.9999→2 的坑]）+ `ZoneLineView.EnemyStrengthLabel/EnemyRevealed`；控制器敌军显"约 X–Y"+旗帜半透+问号。新增 `ZoneBattleViewFogTests`（7 项边界）。DLL 已同步。**并入 s27 一起验证。**
>   - **✅ P2-posture 完成（Claude 手建，dotnet 1261 绿）**：CampaignRuntime.OffensiveBattleSetPosture/DefenseBattleSetPosture 已存在→透传 SessionRuntime + ZoneBattleSession.SetPosture；ZoneBattleView 加结构化 `OwnUnitView`/`OwnUnits`（id/将领/兵力/姿态/在途）；控制器每支队名牌下加姿态三态切换（主攻/佯攻/守，当前高亮，在途/溃散禁用）。
>   - **✅ P3-涌现 完成（Claude 手建）**：已成条件渲染为彩色徽章（火▲红/水●青/伏◆绿/夜■靛+通用✦，形状+色双编码，不剧透未成型）。
>   - **P3-地形切换 延后**：隘口/渡口/平原背景未出图（3/5 地形无图），prompt 备在 §8.1-8.3（用 bg3-1 签名A做 --sref）。待用户出图后一处小接（controller 按 terrain 选 bg + terrain 访问器透传）。
>   - **总状态**：P1+P2+P3 代码全建完（除地形切换待素材），dotnet 1261/1261 绿，3 DLL 已同步，**待一次编辑器验证**（s27 清单已含全部：方格/旗帜/名牌/雾/姿态/徽章）。未提交（等 UI 验过）。
> - **待产美术**：3 地形背景（隘口/渡口/平原）+ 真 sprite（旗帜/涌现/姿态/节点框，规格见 battle-scene-art-spec）。

## 🖼️ 里程碑（2026-07-10 s26）— HUD 麾下立绘 + 战斗屏城战背景（编辑器验证 PASS）

> **接续 s25 的"美术进游戏"第二批。computer-use 编辑器验证 PASS（含一次 FAIL→修复→复验 PASS），Claude 独立核图坐实。证据 `production/qa/evidence/s26-hud-battle-art-2026-07-10/`。**
> - **战斗屏坚城背景**：`Assets/Resources/Backgrounds/battle-city.png`（坚城）+ 新建 `Assets/UI/ZoneBattle.uss`（`#zb-root` 背景图 cover）+ `ZoneBattle.uxml` 挂 `<Style src>`。不透明宣纸卡叠其上可读。
> - **HUD 麾下人物立绘**（`HudController.RenderRetinue`）：`SessionRuntime.DeputyRoster` 每员 40×53 立绘缩略图 + 中文名（`DisplayNames.Of`），有图显立绘、无图剪影占位；OnEnable + 推进时段刷新。
> - **关键教训（USS 背景绑定）**：`UIDocument.rootVisualElement` 是**文档根**，其子元素（带 `.hud-root` 不透明底色的 zb-root/menu-root）会遮住设在文档根上的背景图。**背景图必须设在"带 class 那个元素自身"**（zb-root/menu-root），用 USS `#name{ background-image: resource(...); background-size: cover }`（MainMenu 已验证范式），别用 C# `root.style.backgroundImage`（首版就是设错元素→FAIL）。
> - **HUD 具名立绘分支**等价验证：加载+显示路径（`Resources.Load<Texture2D>`+`Image.image`）与 s25 武将录关羽立绘完全一致，剧本出现 5 位时必显；默认190汜水关部将为无名"副将"故显剪影（符合预期）。
> - **✅ 用户选定的"接进 Unity"方向首轮完成**：主菜单背景(s25) + 武将录立绘(s25) + 战斗屏背景(s26) + HUD麾下立绘(s26)。

## 🎨 里程碑（2026-07-10，v1.1 已按 GDD 定稿校正）— art-director 出品：视觉战斗场景美术资产规格 + 逐地形背景 Prompt

> **战斗引擎从"文字卡片"升级"真·战场画面"的美术侧规格。game-designer 并行出 `design/gdd/visual-battle-scene.md`（内容/布局，已定稿），art-director 定视觉，本轮已逐类对齐该 GDD §6 资产接口契约表校正命名。**
> - **`design/art/battle-scene-art-spec.md` 升级到 v1.1**：① 逐地形战场背景（`TerrainKind`：坚城/隘口/渡口/平原/遮蔽，5 类**全部各自独立成图，不互相复用**——**v1.0 曾建议平原复用「粮道」背景 `bg_supplyroad_neutral_1920.png` 的方案已作废**，因"粮道"是区角色名非地形，本轮改为平原单独出 Prompt，见 §1.5 更正记录）；② 新增此前遗漏的**区域节点框+邻接连线**（§2）、**姿态图标×3**（§5，主攻/佯攻/守，命令语气朱红实线）、**在途/溃散状态叠加**（§6，`charfx_transit_24`/`charfx_broken_24`）三大资产族；③ 兵力队列图元架构重做（§3）——由"每档位一张图+大纛"改为"每阵营 1 枚基础图元×3 状态（own/enemy/fogged）由 UI 层重复堆叠 N 次+溢出数字徽标"，前缀改用独立 `troopicon_*`；④ 涌现标记从 4 类扩到 8 类全覆盖（4 专属+1 通用兜底卷轴图标），命名改 `uiicon_emergence_[tag]_32`；⑤ 反全知问号徽标重构为独立叠加层 `uiicon_tier_unknown_32`（不再合并进旗帜本体），未侦察态兵力图元读解为"保留真实阵营色，仅隐藏精确数量/将领身份"（本文件读解，已标注若与 game-designer 原意不符可低成本改一处）；⑥ 名牌容器命名改为复用既有 `faction_*` 前缀（`faction_[faction]_generalframe_128.png`），不再新造 `ui_generalplate_*`。
> - **prompt-pack §8 升级为 3 条新背景 Prompt**（`design/art/midjourney-prompt-pack.md`）：隘口 Pass（§8.1）/ 渡口 Ford（§8.2）/ **平原 Plain（§8.3，本轮新增，替换掉已作废的"复用粮道"方案）**，三者导出命名统一改用 GDD 命名 `bg_zonebattle_[terrain]_1920.png`（已入库的坚城/遮蔽仍保留 legacy 文件名，是否统一改名列为 TODO，见规格文件 §1.6）。平原 Prompt 已在正负面提示词中显式排除道路/车辙/农田脊元素，与粮道图刻意区隔。
> - **registry §6 本轮未改动**（三张新背景尚未实际出图，仅备好 prompt）。
> - **下一步候选**：① 项目方按 §8.1/§8.2/§8.3 prompt 实际跑 MJ 出隘口/渡口/平原三张背景，过 §3 检查表后登记入 §6；② 已产出的坚城/遮蔽背景是否改名统一到 GDD 命名，需 tech/producer 排期；③ 兵力图元/涌现标记/节点框/姿态图标等全部待实际出稿（占位符号系统已定，未出图）；④ 规格文件 §10 共 7 项 TODO 与 game-designer/tech 对齐后回填。

## ✅ 完成（2026-07-07 续22）— 演义事件引擎「全做完」（试水→完整）

> **用户裁定：效果层做「完整·可推演不入档」。dotnet 1166 绿（1158→1166，+8）；3 DLL 同步；console 端到端实测效果可见。**
> - **引擎升级**（`src/Application/Scenarios/LoreEvents.cs` 重写）：`LoreEvent` 加 `Effects`；新增 `LoreEffect`（Introduce/Reassign/Slay，typed 值）+ `LoreOverrides`（覆盖态：斩杀集/移籍表/登场集，`Apply` public 可测）+ `OverridesAt(ctx)`（逐年扫描 [Anchor,Current] 确定性重放累积）。
> - **事件表 1→10 桩**：桃园结义 / 温酒斩华雄(斩华雄) / 三英战吕布 / 三顾茅庐(登场诸葛亮) / 许攸夜奔(移籍→曹) / 白门楼(斩吕布·高顺·陈宫 + 张辽移籍曹) / 赤壁鏖兵 / 水淹七军(于禁移籍→刘) / 败走麦城(斩关羽) / 秋风五丈原(斩诸葛亮)。世事类(白门楼/官渡/走麦城/五丈原)不限玩家势力，主角亲历类限对应势力。
> - **覆盖层接入归属核**（`GeneralAffiliations`）：`AffiliationOf`/`RosterOf` 加可选 `LoreOverrides? overrides=null`（默认 null=原行为，零破坏面）；斩杀→Absent、移籍→换势力，优先序高于 baseFaction。
> - **可推演不入档**：触发只依赖已持久化态（纪元/年/玩家势力/在世门）→ 读档 `OverridesAt` 重算即复原，无新存档字段/迁移（沿用 ADR-0015 发觉门范式）。
> - **console**：`lore` 展示当轮触发事件+效果(▸行) + 「演义已改写天下」累积陨落/移籍/登场汇总。实测：234盘→关羽/诸葛亮陨落；190盘→桃园叙事+华雄陨落。
> - **测试 +8**（`LoreEventsTests`：触发门/斩杀→Absent/白门楼多效果/许攸移籍真delta/确定性重放/空覆盖/机制单测）；改 1 旧测试（P8桃园「200总数=0」→「桃园缺席」，因新增世事类200触发）。
> - **文档**：ADR-0016 +D6 覆盖层契约 + 更新试水措辞；GDD-027 事件条目+AC7 升完整。
> - **提交**：df9e862（引擎完整体）已 push tk。

## ✅ 完成（2026-07-07 续25）— AI 强化 E1-E5（用户授权一次做完）

> **dotnet 1253 绿；3 DLL 同步；全程可解释反馈 + 单测。**
> - **E1 切粮道**（提交 0b23257）：攻方乘虚突袭弱守粮道。
> - **E2 守将性格**（0b23257）：莽将悍勇/善守死守，复用守将进战斗。
> - **E3 反套路·渐进记忆**（b4b08c6）：PlayerTacticProfile 跨战记录玩家路线+连击 → 守方 AI 渐进反制（第1次弱→第4次强，封顶不作弊）；伴生槽 slot.tactics 持久化。
> - **E4.1 战略意图+威胁**（948308b）：FactionStrategy 派生扩张/趁火/固守/恢复/报复/求存/崩溃 + 对玩家威胁档；console strategy「曹操：扩张·大威胁」。
> - **E4.2 目标+多线压力**（5a6d620）：StepStrategic 意图驱动兼并 + 对玩家施压（强势报复/扩张者夺玩家城，永不夺最后一城）；LastContentionNotice「⚠孙策乘势夺你一城」。
> - **E4.3 武将任命 AI**（c7a6ad1）：GeneralAssignmentService 据标签荐守将/军师/内政/先锋；console assignai「守将关羽｜军师张松｜内政简雍｜先锋张飞」。
> - **E5 外交 AI**（491e710）：DiplomaticAI 据玩家支配度+背信度派生 臣服/中立/警惕/敌意/合纵；你太强/太反复→诸侯合纵围之；console dipai。
> **测试 +30；全程反全知不作弊、纯确定性。剩余 AI 深化（更细反套路模式识别、外交主动缔约行为）为后续增量。**

## ✅ 完成（2026-07-07 续24）— 后端收尾 A1-A3（用户深读分析 P0/§13/P2）

> A1 任用合法性门（c03b4b3）· A2 招揽统一后端桥（d3f131e）· A3 存档硬化（13bcb4d）。dotnet 1225 绿。详见提交。

## 🔄 进行中（2026-07-07 续23）— 武将「活人化」7 大问题一次性解决（用户深读后指令）

> **用户深读武将系统，判定"档案层做得多、活人层做得少"，要求 7 类问题全解决、下次深读不再是缺陷。分批推进，每批 tests+console 可验+提交。dotnet 1176 绿。**
> **批次追踪**：
> - ✅ **A 数据完整性守卫**（#7）— char-jiling 静默重复布防修复（索引器后者覆盖，非抛异常）+ `GeneralDataValidation` 全谱校验（orphan/double-city/bond/life）+ 测试。提交 24083dc。
> - ✅ **B 守将进战斗**（#3 最严重）— `PlanDefender` 加 defenders 按标签择位（善守镇正面/诡谋护粮道）+ `PlayableCampaign.DefendersFor`（城册→守将，应用覆盖层）+ FromOffensive/CampaignRuntime 注入。守将标签/档/羁绊真进守方结算。测试 +4。提交 6e4768c。
> - ✅ **C 副将分区**（#4）— `PlanAttacker` 主将坐镇招牌列，副将 `TakeBest` 按区适配补此前 null 的次列。测试 +4。提交 e2c34c4。
> - ✅ **D 三维细化**（#5）— GeneralOf 统率/武勇/智略从战阵档/谋略档+标签确定性派生（吕布高武低智/诸葛反之），围绕旧均值散开无平衡回归。+GeneralProjection 公开。测试 +3。提交 740f7e4。
> - ✅ **E 守备接城池**（#6）— DefenseOf 取世界大盘该城真实守军（邺900/虎牢600/小沛400）+ 工事分级（虎牢600→1.2 不动平衡）。测试 +3。提交 6b405cf。
> - ✅ **F 招揽状态机 + 反全知门**（#2）— TalentKnowledge 阶梯（未闻→听闻→定位→接触）+ RecruitChannel + TalentKnowledgeBook + KnownPool（只呈已闻名，替代裸 PoolAt）+ Attempt 确定性结算（入伙/婉拒/结怨/投敌）。console `pool` 改走门（堵 78 员泄漏）+ discover/hire。测试 +6。提交 5237707。诚实边界：知晓簿会话内待接存档；与旧 3-原型 TalentService 并存待统一。
> - ✅ **G GeneralState 运行时人生**（#1/#4，最大）— 基座（ADR-0017：GeneralState 不可变·记忆联动忠诚·伤病·被俘·叛离风险 + GeneralLedger + GeneralLifeSeeding，提交 cff4c2e）**+ 持久化完成**（GeneralLedgerCodec 无损 round-trip + 台账入 CampaignRuntime + 伴生存档槽 slot.generals，绕核心信封首迁移风险；提交 6b26b49）。测试 +10。实测存读档人生态往返正确。剩余渐进步：消费方全接（关系/AI/战斗/军议自动读写 GeneralState）。
> - ✅ **H 数据外部化**（#5 维护）— GeneralDossierCodec 从权威档案【生成】外部数据（零转写误差）+ 解析 + round-trip 证全 500 员等价 + assets/data/generals.tkdata 入库 + console exportgen。测试 +2。提交 7ff998e。剩余步：运行期权威源切外部文件加载（须 Unity StreamingAssets 装载，★编辑器）。
> **🏁 全 8 批（A-H）完成，dotnet 1203 绿；3 DLL 同步。武将 7 大问题全部实质解决。**
> **✅ 后端收尾 I2-I7（除 UI 外全做完，2026-07-07）dotnet 1217 绿**：
> - I2 演义事件↔GeneralState（GeneralLifeReconciler：斩杀标重创/移籍换主，幂等，接 Advance）提交 86829f6
> - I3 战斗结算→GeneralState（ConcludeOffensive：参战积劳/败则受创/破城俘敌将）提交 d883861
> - I4 招揽知晓簿入存档（TalentKnowledgeCodec + 伴生槽 slot.talents，反全知发觉跨存读档）提交 add6e18
> - I5 任用态入存档 + 接运行期（AppointmentCodec + CampaignRuntime.AppointGeneral + slot.appoint）提交 109e218
> - I6 外部档案加载器（GeneralDossierLoader 记录→对象 + export/load 保真）提交 fce421a
> - I7 baseFaction 纪元覆盖（TryEraFaction + 播种黄权/董和/法正/马超/杨仪/姜维，修190小沛史准）提交 21c6094
> - **I1（副将封顶）诊断为 UI-only**（HudController.AddDeputy 硬编码限1副将），后端正确，按"除UI"范围跳过。
> **剩余全部为 Unity 表现层（招揽屏/人生屏/任用屏 + I6 权威源切外部文件的 StreamingAssets 装载 + I1 副将UI），均 ★编辑器。**

> **✅ 战斗界面守将呈现（s23b/s23c，编辑器验证 PASS）**：s23b 验证出征战斗全链在 HUD 走通（step4 BLOCK 原为可发现性），并发现战斗UI只显数字兵力不显将领。已修（提交 eaa6705）：ZoneBattleView 我方支队拼主将名 + 守方将领反全知投影（已侦察现真名/否则未探明）+ ZoneBattleController 渲染敌将行 + 标题按模式（修硬编码虎牢关）。s23c 编辑器验证全 PASS（截图坐实：标题=出征·攻城战、我方=主公亲征·atk-front、敌将未侦察=未探明之将/侦察后=高顺[190吕布下邳守将，史实正确]）。测试 +3（1200→1203）。"打高顺守的下邳≠打无名守军"闭环。证据 s23b/s23c-*.md。
> **✅ Unity smoke-check 通过（s23，2026-07-07）**：computer-use 编辑器验证 DLL 更新未破坏现有场景。步骤1编译0 error/warning · 步骤2 场景生成11屏 · 步骤3 HUD完整渲染0 error · 步骤5 战略图反全知雾正常。步骤4战斗结算 BLOCKED——computer-use 未从 HUD 摸到出征入口（`出征`在HUD为文字提示非按钮），**非回归/非崩溃，全程0 error**；战斗逻辑（守将/副将/守备 B/C/E）由 1200 单测覆盖。证据 s23-unity-smoke-2026-07-07.md + 5 截图（Claude 独立核图）。既有小瑕（非本轮）：`君命: Pending` 未本地化占位符。**结论：A-H 后端更新对 Unity 安全，现有可玩场景无恙。**
> **诚实边界**：系统覆盖 500 将；深度手工调校先喂 ~30 核心（符合用户"深度优先勿扩 500"）。F/G/H 各为多提交独立工程（G 动存档格式）。Unity 屏（招揽/GeneralState 展示）仍 ★编辑器，C# + console 先做到可验。

## ✅ 完成（2026-07-07 续22b）— 演义事件运行期透传 + 触发时机

> **第2件（运行期透传 + 时机）落地。dotnet 1167 绿（+1 时机测试）；3 DLL 同步；console 实测前后一致。**
> - **触发时机改「推进过锚点年后生效」**（`OverridesAt` 扫描 (Anchor, Current]；`FiredAt` 有效果事件需 CurrentYear>AnchorYear，无效果flavor如桃园可开局播）：开局快照干净，残局不在第一回合殒星。锁 test `test_lore_effects_not_applied_at_anchor_year`。
> - **console 运行期透传**：`RenderCityRoster`/`RenderAffiliation` 经 `CurrentOverrides()` 把覆盖层喂给 `RosterOf`/`AffiliationOf`——下游内政/军师/成军/凝聚随城册自动跟随。
> - **实测前后一致**：234盘开局 汉中册6员含诸葛亮（军师诸葛亮）；推进到235 → 册5员诸葛亮消失、军师自动改姜维、lore报五丈原殒诸葛。两界面不再各说各话（修复续22诚实边界）。
> - **未透传**：`GeneralRecruitment.PoolAt`（在野池）——分析下效果不致池不一致（移籍者本 InService、斩杀者多已bio出局），故不接，避免扩面。Unity 侧无 roster 视图消费方（P9未做），故运行期可验证面=console，已闭环。
> - **未提交/未推**：LoreEvents(时机)/GameConsole(透传) + LoreEventsTests(调ctx+1新) + 3 DLL + active.md。

## ✅ 完成（2026-07-07 续21）— 武将全局融入 P2-P8 全落地 + console 验证命令【✅ computer-use 复验通过】

> **用户授权一口气做到 P8。dotnet 1158 绿；3 DLL 同步；console 端到端实测八系统全跑通。**
> **✅ s21 复验通过（2026-07-07）**：computer-use 跑 run1(刘备·小沛190)+run2(诸葛亮·五丈原234)，输出重定向到 `s21-run1/2-raw.txt`；Claude 逐条对回原始输出核验 A–F 全 PASS（A 城册≤20 / B 内政军师成军凝聚四行 / C 生卒在野门 / D 在野池78员带难度 / E 桃园结义190触发·234不触发 / F 无报错）。证据 `s21-console-verify-2026-07-07.md` + 2 raw + 2 png。诚实边界：两张 png 实为 File Explorer 窗口（PowerShell 点击层不可键入，改重定向），证据以 raw txt 为准、已独立核实。
> - **P2 招揽统一**（`GeneralRecruitment`）：在野在世将=可招池（全谱，非3原型）；难度自野心/傲物/仁德+名望→易招/尚可/难招。console 实测小沛开局在野池 78 员。
> - **P3 任用**（`Domain/Appointment/AppointmentBook`，不可变）：太守调拨入城册（≤20）；满员拒/一将只在一城/调他城自动移出；round-trip 就绪。
> - **P4 内政**（`GovernanceContribution`）：城册点内政官（仁德/谋略），属性→民心/征粮/城防加成。console：董和 民心+30%。
> - **P5 军师**（`CouncilCapability`）：点军师（谋略最高），档→可提策深度+质量（庸言…神算）。
> - **P6 派系**（`RetinueCohesion`）：麾下羁绊/忠诚→凝聚度+离心风险（桃园知己>吕布董卓仇怨）。
> - **P7 战斗**（`ArmyFormation`）：一军主将+可选副将（≤2），多军并进（exclude 防重复用人）；档→战力贡献。console：主将关羽·副将张飞。
> - **P8 演义事件试水**（`LoreEvents`，改名避让 GDD_015 的 Domain.World.HistoricalEvent）：事件引擎 + 招牌「桃园结义」（锚定刘关张·刘备·≤190 触发）。console 实测触发。
> - **console 命令**：affil/croster/pool/lore 四命令展示全部八系统（供 computer-use 真实驱动复验）。
> - **测试 +11**：GeneralAffiliationTests(4) + GeneralIntegrationTests(7)。
> - **诚实边界**：`baseFaction` 静态简化 → 190 小沛册含黄权/杨仪/董和等（史实此年未归刘备/在他处）；机制正确、史准需纪元覆盖细化（ADR-0016 已注，属数据refinement非引擎）。任用态未入存档（会话内，同发觉门待接存档）。**P9 表现层（Unity 屏）未做**——本轮全后端，验证走 console。
> - **未提交/未推**：见 git（新增 6 服务/值对象 + 2 测试 + console + 3 DLL + active.md）。

## ✅ 完成（2026-07-07 续20）— 武将全局融入：GDD-027/ADR-0016 + P1 地基【已开工，P2-P8 已续完】

> **用户裁定：P1-P8 全做；一城≤20 武将由太守调拨；要系统事件引擎但先招牌事件试水。dotnet 1151 绿。**
> - **设计文档**：`design/gdd/gdd-027-general-affiliation-appointment.md`（武将归属·任用·全局融入，8 消费方）+ `docs/architecture/adr-0016-general-affiliation-integration.md`（归属派生 + 城册≤20 + 依赖倒置解耦契约）。
> - **P1 地基（完成，纯 C# 无场景依赖）**：
>   - `PlayableCampaign.FactionExistsAt/FactionCapitalAt`（纪元势力存续/治所查询）。
>   - `GeneralDossiers.StationOf`（某将某纪元布防城）。
>   - `GeneralAffiliations`：baseFaction 数据（~350 员按势力分组，蜀魏吴袁吕表璋腾遂公孙董袁术单城诸侯汉庭黄巾；名士/女性/方士/异族/散雄默认在野）+ `AffiliationOf(将,年)→在职/在野/不存` + `RoleOf`（标签派生守将/先锋/谋士/内政/斥候/水军）+ `RosterOf(城,年)→城册≤20`（按档降序稳定裁剪）。
>   - 测试 GeneralAffiliationTests +4（关羽190在职·小沛·先锋 / 姜维190不存 / 司马徽在野 / 臧霸200势力灭转在野 / 角色派生 / 城册≤20确定性 / 纪元势力治所）。
> - **待续 P2-P8**：P2 招揽统一（在野池全谱）· P3 任用（太守调拨≤20）· P4 内政接入 · P5 军师接入 · P6 派系关系 · P7 战斗接入（主将+副将,多军）· P8 历史事件（先招牌事件试水）· P9 表现层（★编辑器）。
> - **未提交/未推**：gdd-027 + adr-0016 + PlayableCampaign/GeneralDossiers + GeneralAffiliations + 测试 + 3 DLL + active.md。

## ✅ 完成（2026-07-06 续19）— 战略图屏启用 + 发觉入存档【已提交+编辑器验证通过】

> **提交 a0d6620 已 push tk；s19 编辑器验证全 PASS（证据 production/qa/evidence/s19-map-verify-2026-07-06.md + s19-screens 4图）。**
> - **编辑器实测（computer-use + Claude 核图）**：重跑场景生成器出 11 场景（含 CampaignMap.unity）；HUD 顶部新增「战略图」按钮 → 进战略图屏；标题/纪元行「公元190·春 天下36城·16家逐鹿」；势力领城列表（★玩家标记）；**反全知雾正确**——己方关羽/名将郭嘉露真名、41 员敌将「未探明」；返回 HUD 正常；Console 0 error。
> - 非阻断：战略图屏背景透天空盒（面板非不透明底，同 Roster），归美术打磨。
> - **发觉入存档**：派生自持久化侦察态，save/load round-trip 测试绿。dotnet 1147。

> **dotnet 1147 绿；3 DLL 同步。发觉持久化已闭环；战略图屏 C# 侧完成，★需编辑器生成场景验证。**
> - **发觉入存档（Point 2 完成）**：把「已探知目标」从会话内标记改为**派生自持久化侦察态**（`Session.PendingScouts` / `PlayerKnowledge.Entries`，二者已在存档 head）→ 存读档天然一致，无需新字段/迁移。CampaignMapViewTests +1（scout→save→load 仍识得高顺）。
> - **战略图屏启用（Point 1，UI Toolkit 版，不碰停泊的 DOTween scaffold）**：
>   - 新增 `Assets/UI/CampaignMap.uxml` + `CampaignMapController.cs`（读 SessionRuntime.MapView：各势力领城降序 + 在场武将棋子，反全知「未探明」/名将露真名，底部提示未探明数）。
>   - HUD 加 `nav-map`「战略图」按钮 + HudController 接线 LoadScene("CampaignMap")。
>   - SliceSceneBuilder 10→11 屏（+CampaignMap）。
> - **★下一会话须做**：Unity 里聚焦编译 → 菜单「三国→构建 Slice 场景」重跑（生成 CampaignMap.unity）→ 进 Play：HUD 点「战略图」应进屏，见势力领城 + 武将棋子（己方/名将露名、敌境「未探明」）；派探目标势力后其将现形。
> - **仍 parked**：DOTween 立绘动画版大地图 scaffold（本 UI Toolkit 版为轻量可玩替代，非美术版）。
> - **未提交/未推**：Assets（uxml/controller/hud/builder/3DLL）+ src CampaignRuntime + test。

## ✅ 完成（2026-07-06 续18）— 武将登场机制验证 + 地图反全知雾 + 布防再加密【已提交】

> **回应用户「武将出现三规则」需求。dotnet 1146 绿；3 DLL 同步；console 实测雾+加密。**
> - **验证（新增 GeneralAppearanceAcrossErasTests，5 测试全绿）**：
>   - R1 该出现的必须出现·按纪元换城：关羽 190小沛→194下邳→200汝南→208长沙→219江陵。
>   - R3 生年到了才出：姜维(202)190/208不在、234现汉中；诸葛亮(181)190尚幼、208已出。
>   - R3 死了就退：华雄(191)190在200退；关羽(220)219在221退。
>   - R3 **动态**（键于当前年）：襄樊219开局关羽在战略图，推进过220后自动退出（端到端 CampaignRuntime）。
>   - R2 未招揽可隐藏可发觉：人才未探知隐藏、探知后入可见录。
> - **A 地图反全知雾（新落地，GDD_026 R6）**：`CampaignMapView` 武将棋子分「已识/未探明」——己方城之将 + 传世名将（战阵绝世 Peerless 或谋略经天纬地 Master）露真名；敌境无名之将显「未探明」（id/名不泄）。**发觉门**：占城（归己）或 `ScoutEnemy` 派探目标势力→其将现形（会话内标记 `_scoutedTarget`，非存档）。CampaignMapViewTests +2（雾/探知发觉）。
> - **B 布防再加密**：Placement 200/234/219 各 +5~8 城具名武将（此前已密 190/208/220）。默认190盘 console 实测 48 员在场（含大量「未探明」+ 名将露名如典韦）。
> - **诚实边界**：反全知雾作用于 `CampaignMapView` 数据源；Unity 战略地图 scaffold 仍停泊 parked/，故雾的**可视化**待地图屏启用（console 已可见）。「发觉」标记未入存档（读档重置，属情报时效，可后续接存档）。
> - **未提交/未推**：本轮改 CampaignMapView/CampaignRuntime/GeneralDossiers + 2 tests新增/改 + 3 DLL + active.md。

## ✅ 完成（2026-07-06 续17）— 武将谱 315→500 + 纪元 194濮阳/219襄樊 + 布防加密【已提交，待编辑器验证】

> **dotnet 1139 绿；武将录实测 500 员；3 DLL 同步；console 端到端跑通两新纪元。提交待推 tk。**
> - **武将谱 315→500（批6 +93、批7 +92）**：三处同源（生卒/档案/中文名）。批6=魏晋近臣·蜀汉文武·东吴宗室·董吕残部·袁绍谋士·刘表刘璋部将·黄巾异族·汉末名士；批7=魏晋帝室诸臣·蜀后期·东吴后期·凉州群雄·**过五关六将·吕布八健将·建安七子·荆南四郡太守·南中羌蛮·许攸/曹纯**。
> - **纪元 +2（→8 锚点年 184/190/194/200/208/219/220/234，11 开局）**：
>   - **194 兖州之战**：World194（15势力36城，曹操困守鄄城2城/吕布据濮阳2城/刘备保徐州）+ Placement194 + 预设「曹孟德·濮阳」（反攻吕布复夺兖州）。
>   - **219 襄樊之战**：World219（曹操24/蜀关羽6/吴6）+ Placement219（关羽@江陵/曹仁守樊@襄阳/吕蒙陆逊白衣渡江@建业）+ 预设「关云长·襄樊」（PlayerLord=关羽，锋指襄阳伐魏，防东吴背刺）。
> - **布防加密**：Placement190 +11城具名（益州/长沙/荆襄谋士/邺城/北平/南皮）、208 +5、220 +5。各纪元战略图在场武将更满。
> - **测试**：GeneralRosterTests ≥300→≥490 + 批6/7抽查（许攸/曹纯/王粲/黄权/吕岱/郭女王/黄皓）；GeneralPlacementTests 194/219布防；PlayableStartCatalogTests 194二城对峙/219独大三分。
> - **console 实测**：武将录 500 员；`named caocao-yanzhou`→公元194·15势力36城·陈宫高顺@濮阳；`named guanyu-xiangfan`→公元219·曹操24蜀★6吴6·吕蒙陆逊@建业。
> - **待编辑器验证**：GameSetup 命名开局应列 11 开局；194/219 进 HUD 显公元194/219；武将录约 500 员。★需编辑器。
> - **未提交/未推**：本轮改 5 src + 3 tests + 3 DLL + active.md。

## ✅ 完成（2026-07-06 续16）— 武将谱 263→315 + 纪元 184黄巾/234五丈原【已提交+编辑器验证通过】

> **提交 5f6c8a9 已 push tk；s16 编辑器验证全 PASS（证据 production/qa/evidence/s16-eras-verify-2026-07-06.md + s16-screens 5图）。**
> - **编辑器实测（computer-use + Claude 核图）**：GameSetup 命名开局列 9 开局（含黄巾/五丈原，中文名无 id）；张角·黄巾起义→HUD 公元184·锋指洛阳；诸葛孔明·五丈原→公元234·锋指长安；武将录见新武将刘禅；Console 0 error。
> - **纯 C# 内容长线，dotnet 1139 绿；3 DLL 已同步；console + 编辑器双重实测两新纪元可玩。**
> - **Part A — 武将谱 263→315（扩充批 5：+52 员）**：`GeneralDossiers`（生卒+档案）/`DisplayNames`（中文名）/`GeneralBonds`（+15羁绊）三处同源。覆盖：汉末群雄州牧（刘焉/刘虞/陶谦/公孙度/桥瑁/孔伷）、凉州李郭黄巾（边章/李肃/杨奉/张济/樊稠/郭汜/胡车儿/波才/张曼成）、魏后期（贾逵/田畴/高柔/辛毗/夏侯霸/王昶/石苞/诸葛绪）、蜀后期（刘禅/马忠/霍弋/罗宪/赵统赵广/董厥/蒋舒/刘谌）、吴后期（孙皓/孙休/孙亮/朱据/吕据/留赞/滕胤/唐咨）、汉臣荆州巾帼（杨彪/刘琦/刘琮/夏侯氏/伏寿）。羁绊：赵云父子/刘表父子争嗣/昭烈父子/黄巾三兄弟/李郭内讧/张济张绣。
> - **Part B — 纪元 184/234（ADR-0015，190 零改动）**：
>   - **184 黄巾之乱**：新增 `faction-han`(汉庭)/`faction-huangjin`(黄巾) 两势力 + `World184`（9势力36城：汉庭13/黄巾5/公孙瓒2/刘表4/益州3/孙坚4/凉州3/汉中1/交州1）+ `Placement184`（皇甫嵩曹操@洛阳/波才@汝南/董卓@长安/孙坚系@建业）+ 两预设「何大将军·讨黄巾」「张角·黄巾起义」（正邪双方可玩）。
>   - **234 五丈原**：`World234`（魏曹叡24/蜀刘禅4/吴孙权8）+ `Placement234`（诸葛姜维@汉中/司马郭淮@长安/刘禅蒋琬@成都/陆逊@建业；张郃231已亡自动过滤）+ 预设「诸葛孔明·五丈原」（PlayerLord=诸葛亮，锋指长安伐魏）。
>   - `WorldAt/PlacementFor` 加 184/234 分支；开局目录 6→**9 个**。
> - **测试**：GeneralRosterTests ≥250→≥300 + 批5抽查；GeneralPlacementTests 184/234布防；PlayableStartCatalogTests 184汉庭13城/黄巾5城 + 234三分 + 184/234公元年投影。
> - **console 实测**：`named zhangjiao-uprising`→公元184·汉庭13黄巾★5…36城；`named zhugeliang-wuzhang`→公元234·曹操24刘备★4孙吴8·姜维@汉中。武将录 315 员。
> - **待编辑器验证**：GameSetup 命名开局应列 9 开局；184/234 进 HUD 顶栏显公元184/234。★需编辑器（无头做不了）。
> - **未提交/未推**：见下 git（本轮改 6 src + 3 tests + 3 DLL + active.md）。

## ✅ 完成（2026-07-06 续15）— 武将谱 203→263 + 多锚点年世界盘（200/208/220）【已提交+编辑器验证通过】

> **提交 f327113 已 push tk；s15 编辑器验证全 PASS（证据 production/qa/evidence/s15-eras-verify-2026-07-06.md + s15-screens 5图）。**
> - **编辑器实测（computer-use + Claude 核图）**：GameSetup 命名开局列 6 开局（含官渡/赤壁/季汉，中文名无 id）；曹孟德·官渡→HUD 顶栏公元200·锋指邺城；刘玄德·季汉→公元220·锋指长安；武将录见新武将陈到/陈登/陈珪（性情文字非数值）；Console 0 error。
> - **非阻断观察**：武将录屏背景透出天空盒（面板非不透明底），纯美术问题，归 HUD/布局美术打磨长线。
> - **纯 C# 内容长线，dotnet 1136→1139 绿；3 DLL 已同步；console + 编辑器双重实测三纪元开局可玩。**
>
> ### Part A — 武将谱 203 → 263（扩充批 4：+61 员）
> - 三处数据同源补齐：`GeneralDossiers.cs`（LifeYears 生卒 + Build() 档案）+ `DisplayNames.cs`（中文名）+ `GeneralBonds.cs`（+15 羁绊）。
> - 覆盖：汉末朝堂（王允/卢植/皇甫嵩/蔡邕/董承）、魏晋文武（曹昂/杨修/崔琰/董昭/司马孚/桓范/管宁）、蜀汉后进（陈到/董允/杨仪/向宠/廖立/傅彤/吕凯）、东吴中生代（吕范/虞翻/周鲂/贺齐/孙桓/孙翊/潘濬/孙峻/孙綝）、群雄诸侯（陈登/陈珪/韩馥/张邈/鲍信/袁谭/袁尚/张燕/管亥）、巾帼（卞夫人/甘夫人/糜夫人/吴国太）。
> - 羁绊：袁绍父子/曹操长子/陈珪陈登/孙坚三子/蔡邕文姬/卢植师徒（→刘备·公孙瓒）/王允貂蝉连环计/袁氏兄弟阋墙。
> - 测试：GeneralRosterTests 下限 ≥200→≥250 + 批4 抽查 + 中文名解析。
>
> ### Part B — 多锚点年世界盘（ADR-0015 离散快照，190 零改动隔离风险）
> - `PlayableCampaign`：加 `World200/208/220` 三张 36 城大盘（同骨架、归属随纪元重绘）+ `WorldAt(year)` 选择器（190=原 World）；`BuildStartConfig` 与 `InitialContention` 改按 `_start.AnchorYear` 取盘。
> - `PlayableStart`：加 3 预设开局——**曹孟德·官渡(200)**/**孙仲谋·赤壁(208)**/**刘玄德·季汉(220)**，入 Catalog.All（共 6 开局）。
> - `GeneralDossiers`：加 `Placement200/208/220` 按年布防 + `PlacementFor(year)`；`AllPlacements/GeneralsAt` 泛化按年。
> - 测试：GeneralPlacementTests（多锚点布防随纪元）+ PlayableStartCatalogTests（三纪元盘 36 城/归属/守方/公元年投影）。
> - **console 顺手修**：状态行「你据 X 城」原硬编码汜水关席 → 改读实际玩家势力（`_rt.Scenario.PlayerFaction`）；Unity HudSummary 本就正确无需改。
> - **端到端实测**（console）：`named caocao-guandu`→公元200·曹操★12·存续11家·关羽@汝南；`named liubei-shu`→公元220·刘备★4·曹操24/孙吴8·陆逊@建业；武将录 263 员。
>
> ### 待用户在编辑器验证（非阻断）
> - Unity GameSetup 选择屏应列出 6 个开局（新增官渡/赤壁/季汉）；进纪元开局 HUD 顶栏应显对应公元年（200/208/220）。★需编辑器验证（无头做不了）。
>
> ### 未提交文件（本轮）
> - src：GeneralDossiers.cs / GeneralBonds.cs / PlayableCampaign.cs / PlayableStart.cs / DisplayNames.cs / Console/GameConsole.cs
> - tests：GeneralRosterTests.cs / GeneralPlacementTests.cs / PlayableStartCatalogTests.cs
> - Assets/Plugins：3 DLL 同步（Application/Presentation 变，Domain 未变）

## ✅ 完成（2026-07-06 续14）— 续13 两阻断布局 bug 编辑器验证通过 + 修 char-nengli ID 泄露

> **computer-use 无人值守实测 + Claude 逐张核对截图。证据：`production/qa/evidence/s13-layout-verify-2026-07-06.md`（+ `s13-screens/` 5 图）。**
> - **续13 两阻断 bug 均 PASS**：① HUD 叠加层拦截点击已解（单 UIDocument，4 导航 + career 卡按钮全可点）；② GameSetup「就此起家」够不到已解（功能性进入陈留太守 HUD 证明可达）。顺带印证续12 顶栏 SeatObjective（小沛→下邳，非汜水关）。Console 仅 1 条 info Debug.Log，0 error。
> - **修 char-nengli ID 泄露（本轮代码改动）**：招揽结果原显示「char-nengli 未招得」。根因 `DisplayNames` 未登记三名原型人才 → 补 char-wolong=卧龙 / char-xiaojiang=骁将 / char-nengli=能吏（`src/Presentation/Screens/DisplayNames.cs`）+ 回归测试 `test_talent_view_name_resolves_chinese_not_raw_id`。**dotnet 1135→1136 绿；3 DLL 已同步 Assets/Plugins。提交 ab8b789 已 push tk。**
>   - **✅ 编辑器验证通过（computer-use 实测 + Claude 核图）**：列表按钮「骁将〔善攻坚·易招〕招揽」、招揽结果「骁将 未招得」/「能吏 出仕入伙!」均中文无原始 id；Console 0 error。证据 `production/qa/evidence/s14-charname-verify-2026-07-06.md`（+ s14-screens 2 图）。
> - **证据整理**：computer-use 报告 md 因写长 CJK 路径 IO 截断（文件名丢尾 + 内容止于 B 标题）→ 规整重写为 ASCII 名完整报告，截图目录改 `s13-screens`，清 DONE.flag。
> - **仍待编辑器验证（非本轮）**：Defeat 真实败亡流程未触发验证；HUD/布局美术打磨。（char-nengli 与续13 两布局 bug 均已验证通过。）
> - **已提交**：ab8b789（char-nengli 修复 + 续13 验证 + 场景重建）已 push tk；s14 验证证据随后提交。

## ✅ 完成（2026-07-05 续13）— computer-use 第3轮验收 + 修两个阻断布局 bug（已验证通过，见续14）

> **给下一会话：读完本段即可续。核心 dotnet 1135/1135 绿；工程可编译进 Play。**
>
> ### 本轮已确认（computer-use 实测）
> - ★**顶栏 bug 修复 PASS**（续12 的 SeatObjective，提交 a238d31）：选「刘玄德·小沛」→ HUD 顶栏显小沛，非汜水关。
> - 暂停菜单导航全 PASS；无障碍设置屏工整；出征未授权拒绝文案 PASS；Console 0 error（仅无害 InputManager 提示）。
>
> ### 本轮发现两个【阻断】布局 bug → 我已改代码，但 ★未经编辑器验证（提交见下）
> 1. **HUD 顶部叠加层拦截点击**：GameStatusPanel 是全屏第二 UIDocument，盖住 HUD 顶栏且吃掉点击 →
>    武将录/外交/多城/推进时段按钮点不到。**根治**：废叠加层，把 4 按钮（结算君命/献纳/打听人才/招揽）+ 官阶/手令/君命
>    并入 **HUD 左列新卡 `career-mission`**（单 UIDocument）。改动文件：
>    - `Assets/UI/HudController.cs`：+`RenderCareerMission()` +`KnownTalents` + OnEnable 里 3 个 Wire + advance-time 后刷新。
>    - `Assets/UI/Hud.uxml`：左列顶部加 `career-mission` 卡（hud-career/hud-mission/btn-check-mission/btn-tribute/btn-scout-talents/mission-feedback/talent-list）。
>    - `Assets/Editor/SliceSceneBuilder.cs`：Hud 的 ScreenDef 移除 OverlayUxml（不再挂 GameStatusPanel）。
>    - GameStatusPanel.uxml/Controller 保留但 dormant（不再实例化）。
> 2. **GameSetup 太守开局按钮够不到**：36 城列表撑高把「就此起家」顶出屏幕、外层不滚动。改：
>    - 新增 `Assets/UI/GameSetup.uss`（纵向弹性：choice-list flex-grow:1/min-height:0 内部滚动，footer flex-shrink:0 钉底），
>      `Assets/UI/GameSetup.uxml` 加 `<ui:Style src="GameSetup.uss" />`。
>
> ### ⚠️ 下一会话必须做的第一件事（给 computer-use / Codex 的复验指令）
> **这两个修复只有重跑场景生成器才生效**（Hud.unity 里旧的 GameStatusPanel 叠加 GameObject 要被重建移除）：
> 1. Unity 里 git pull（若这些提交已推）→ 等编译 → 菜单 **「三国 → 构建 Slice 场景」**重跑一次（关键！）。
> 2. 复验：命名小沛开局进 HUD → 点 **武将录/外交/多城/推进时段** 应都能点到、无叠加遮挡；HUD 左列有「生涯·君命·人才」卡且 4 按钮可用。
> 3. 复验：GameSetup「任选一城」选陈留 → **「就此起家」按钮应可见可点** → 进 HUD 顶栏显「陈留太守 · 奉曹操号令」（这张截图上轮没拿到，补上）。
> 4. 若仍有遮挡/够不到，把整屏截图发来定位。
>
> ### 提交（本轮，均已 push tk）
> - a238d31 顶栏 SeatObjective 修复（续12 尾）
> - 【本段代码改动的提交哈希见 git log 最新一条：fix(epic-044) HUD 单UIDocument化 + GameSetup 滚动】
>
> ### 仍挂起（非阻断，重启后可择机）
> - Defeat 真实败亡流程未在编辑器触发验证；HUD+布局的美术打磨；战略地图 scaffold 仍停泊 parked/；多锚点年/roster→250/传承谱系（大内容长线）。

## ✅ 完成（2026-07-05 续12）— Codex 第2轮通过（可进 Play）+ 补齐屏间导航

> **提交 8f48fc5（收纳 Codex 产物）+ ab23839（导航接线）。** Codex 第 2 轮：核心编译 0 error、可进 Play、
> `MainMenu→GameSetup→Hud` 跑通；10 屏均渲染。Codex 3 处修复已收纳（TheaterScreen using / Defeat 未覆灭防御 / MainMenu→GameSetup）。
> - **补导航**：Hud 头 hud-nav（武将录/外交/多城）；PauseMenu resume→Hud·settings→Accessibility·quit→MainMenu；
>   Accessibility back→static ReturnScene（默认 MainMenu，Pause 设 PauseMenu）。★需编辑器再验一轮。
> - **遗留非阻断**：Hud+状态面板灰盒布局重叠 → 归美术/USS 阶段；真实败亡 Defeat 流程未在编辑器触发验证；
>   地图 scaffold 仍停泊 parked/（地图阶段再启用）。
> - 核心 dotnet 1132/1132 不受影响。

## ✅ 完成（2026-07-05 续11）— Codex 首轮验证 → 停泊地图 scaffold 解阻 + 补全场景生成器

> **提交 fc1bfb3。** Codex 在 Unity 6000.3.18f1 首轮验证：Safe Mode，68 编译错误**全部**在战略地图 scaffold
> （缺 Input System/URP/DOTween + 缺 4 stub 类 + init 缺 IsExternalInit）；**核心屏干净**（绑定名过、3 DLL 正常、无缺方法）。
> - **停泊 scaffold**：`Assets/Scripts/Presentation/CampaignMap` → `parked/campaign-map-scaffold`（移出 Assets，Unity 不编译，git 正常追踪；
>   不用 `~` 以免撞 .gitignore `*~`）。核心不依赖它。重启用清单见 `parked/campaign-map-scaffold/PARKED-REENABLE.md`。
> - **扩 SliceSceneBuilder**：5→10 屏（补 GameSetup/Roster/Diplomacy/Theater/Defeat）+ HUD 挂 GameStatusPanel 叠加层。
>   菜单「三国/构建 Slice 场景」一键出全部场景 + Build Settings。
> - **待第 2 轮 Codex**：拉取最新 → 确认核心编译通过 → 跑场景生成器 → 进 Play 冒烟导航。
> - **已知待办**：HUD 缺到 Roster/Diplomacy/Theater 的按钮（各屏可独立开验，游戏内可达性待补按钮）。
> - dotnet 1132/1132 不受影响（本次仅动 Assets/Editor + 停泊移动）。

## ✅ 完成（2026-07-05 续10）— 清 pending：羁绊崩解 + 外交/多城屏 + 交互补全

> **提交 9005dd2 + 92204db。dotnet 1132/1132 绿。**
> - **羁绊崩解（T4 尾巴）**：血脉/知己同场阵亡→余者狂怒死战(+18%士气)，ZoneBattleRuntime 回合后钩子，去重，师徒/仇怨不触发。测 +1。
> - **外交屏/多城屏**：DiplomacyScreen(立场+缔约/背约按钮)、TheaterScreen(直辖/委任城)。★需编辑器验证。
> - **交互补全**：GameStatusPanel 加 结算君命/献纳 + 打听人才/逐个招揽 按钮 + SessionRuntime 便捷桥。
> - **施计占容量→已定不占**：施计属间谍/关系渠道，不与能吏/斥候的手令池竞争(设计裁定,无需代码)。
> - **剩余 pending 两类**：① 我做不了(只你能)——★所有 Unity 屏编辑器验证(无法无头编译)、战略地图场景+棋子prefab+美术(ART_GUIDE)、施计选城屏/委任交互屏(小,console已有)。② 大内容/设计(非bounded)——多锚点年200/208/220/184世界盘、roster→250(现203)、传承子嗣谱系深化。

## ✅ 完成（2026-07-05 续9）— UI 层接入审计 + 补齐（聚合HUD/人才录/外交视图）

> **提交 fb929f6 + 7e3f61b。dotnet 1131/1131 绿。审计文档 Assets/UI/UI_STATUS.md。**
> - 审计：核心全可玩(console)；纯 C# 视图基本齐备，Unity 屏多数已建但★需编辑器验证；补三个缺口视图。
> - **GameHudView（聚合顶栏，UI 单一绑定）**：纪元/一生/生涯/君命/手令/争霸一次取全。CampaignRuntime.HudSummary。
> - **TalentRecruitView（反全知无数值）**：修红线缺口——人才只呈名/专长/招揽难度定性，不露统率武勇智略之数。TalentView。
> - **DiplomacyView**：各存续势力立场中文 + 可否径攻。至此**每个可玩系统都有干净纯 C# 视图**（多数已单测），UI 只需绑定。
> - Unity（★需验证）：GameStatusPanel.uxml/Controller 把君命/生涯/手令/人才一次 surfaced。SessionRuntime 桥全补。
> - **UI 层剩余（见 UI_STATUS.md）**：外交屏/多城屏(视图已备,缺屏)；君命结算·人才招揽·施计选城的交互屏(现 Unity 只显示、交互在 console)；战略地图场景+美术；所有★Unity 屏的编辑器验证(绑定/场景/uss/包)。

## ✅ 完成（2026-07-05 续8）— console 补全 + 行动容量节流（非体力点数）

> **提交 45c5d19（console补全）+ 88c368e（容量节流）。dotnet 1128/1128 绿。**
> - **console 补全**：出征战中微操(bview/move/posture/round，不必 auto代打)、外交(pact/breach/diplo)、多城(theater/delegate)、人才(talents/reveal/recruit)。至此 console 覆盖运行期全部可玩系统。实测:bview显5区、reveal→recruit骁将出仕。
> - **行动容量节流（用户裁定，非体力点数）**：修"一周可无限下令"缺口。CampaignRuntime.ActionCapacity(=2+官阶)/ActionsInFlight(在途侦察+在办治理)/HasFreeAgent；征粮/修工事/安抚/侦察满员则拒(NoFreeAgent)，办完腾人手。与时长制互补(时长管多久见效、容量管同时几件)，不双重节流、喂生涯(升官→更多人手)。军议/看图/出征/外交/施计不占额度。console 状态显「手令X/Y」。
>   - **诚实设计裁定**：体力点数与时长制矛盾(行动即时 vs 在途需时)、会毁"粮草先行"质感;容量制达成同样节流而不冲突。
> - **留后续**：官阶头衔已定但容量随 rank 的具体曲线(现 2+rank)可调;施计是否也占额度待定(现不占)。

## ✅ 完成（2026-07-05 续7）— 全游戏串联：文本控制台整个游戏可玩（无 UI）

> **里程碑：整个游戏在无 UI 下从头玩到尾。提交 19c208d，dotnet 1125/1125 绿。**
> - 盘点：各系统均实现+单测，但旧 CampaignDriver 只竖切；新系统全挂 CampaignRuntime，无统一驱动。
> - 新建 **GameConsole**（src/Console，引 Presentation/CampaignRuntime + MemorySaveMedium）把全部系统串成一条文本循环：
>   选年选城(starts/named/cities/gov) · 纪元推进(w/season/year) · 治理(req/repair/appease) · 情报军议(scout/council) ·
>   君命(mission/checkmission/tribute) · 出征全流程(authorize→targets→offensive→launch→auto→conclude) · 守城(defend/defauto) ·
>   施计(subvert) · 自立(rebel) · 被灭续局(fate/submit/refuse/refuge/continue) · 传承(heir) · 地图(map) · 武将录(roster) · save/load。
> - **玩法**：`dotnet run --project src/Console`（交互）或 `--script "gov city-chenliu|year|..."`（竖线分隔回放）。
> - 端到端实测通关：陈留太守→大势按年铺开(董卓焚都/桃园/连环计)→守土君命完成→出征→地图34在场武将→武将录203→施计→自立。
> - 测试 +3（完整通关/数十年寿终传承/菜单）。**"整个游戏可玩，只差 UI" 达成。**
> - **留后续**：外交/多城/人才的 console 命令（运行期已接，命令未列）；zone battle 战中微操(调动/姿态)console 未展开(现 auto代打)；美术/UI。

## ✅ 完成（2026-07-05 续6）— 君主任务两收尾 + 武将谱→203 + 战略大地图接入

> **dotnet 1119→1121 绿。提交 463fb29 / 9a215ab / 6836cf8。**
> - **君主任务两收尾（463fb29）**：失败名望罚（CareerProgressionService.ApplyRenownPenalty + 服务 PenalizeRenown，CheckMission 失败实扣）；献纳实扣粮（服务 LevyGrain + 运行期 PayLordTribute，Tribute 改判"已缴额"）。测 +2。
> - **武将谱 165→203（9a215ab）**：批3 +39（名士:徐庶/荀攸/司马徽/张松/李恢…；女性:貂蝉/大乔小乔/甄姬/蔡文姬/黄月英…；南蛮:兀突骨/木鹿/朵思;魏晋末:文鸯/诸葛诞/毌丘俭/卫瓘/邓忠;西凉;讨董副将）。羁绊+6。主干名将/名士/女性/南蛮/魏晋末皆覆盖。
> - **战略大地图接入（6836cf8）**：判定项目无大地图层→接入 scaffold。纯 C#（已测）：PlayableCampaign.AllWorldCities/IsCapitalCity + CampaignMapView（世界城归属+势力+纪元→地图单元）+ CampaignRuntime.MapView + SessionRuntime 桥（测 +2）。Unity 侧（★需编辑器验证）：scaffold 落 Assets/Scripts/Presentation/CampaignMap + Adapters（DataAdapter:ICampaignMapQueryService+ITerritoryPositionLookup·37城坐标邻接配色 / ActionAdapter / GameEventBus）+ INTEGRATION.md。
> - **留后续**：地图英雄棋子(HeroPosition 现空)/领土美术/场景搭建/坐标微调(编辑器)；南蛮女性魏晋末更多武将可续批。

## ✅ 完成（2026-07-05 续5）— W5 君主任务系统（生涯脊梁）+ 武将谱 106→165

> **dotnet 1108→1117 绿。提交 0e43af3（君主任务）+ 78ae59c（武将批2）。**
> - **君主任务系统（0e43af3）**：W5 唯一系统级缺口收口。此前君主只有完成日志、从不发话。今落地 LordMission（讨伐/守土/献纳）+ LordMissionService（种子化派发·官阶越高任越重·据世界情势评估进度，纯函数）+ CampaignRuntime.CurrentMission/CheckMission（完成计 LordMissionComplete 功绩通往晋升）+ LordMissionView + SessionRuntime 桥。测试证明数年间完成任务→功绩累积（生涯循环转起来）。留后续：失败名望罚（需生涯减项路径）、献纳实际扣粮。
> - **武将谱 106→165（78ae59c）**：批2 +55（蜀文吏后进/魏晋末/吴文武/南蛮方士:华佗左慈孟获祝融沙摩柯陆抗孙坚等），全档案+生卒+中文名+羁绊6(孙坚父子/关羽父子/诸葛父子)。仍非穷尽(女性/更多副将可续)。
> - **W5 四系统全备**：生涯(晋升+自立+传承+君主任务+头衔)/外交(4立场缔约背约)/多城(委任亲管掌管范围)/治理(征粮修工事安抚)——系统层完整,余为内容广度。

## ✅ 完成（2026-07-05 续4）— 自立无退路 + Unity 壳 + T4羁绊 + W4兵种地形 + W6引导 + W5生涯头衔

> **dotnet 1095→1108 绿。提交链 1a4b4ff→ca8ad21。**
> - **自立无退路（1a4b4ff）**：DefeatFlow +rebelled → 叛主被灭必被俘处死，Submit/Refuse 皆拒；CampaignRuntime.DeclareIndependence（接 LaunchRebellion，成功锁 _rebelled）；GDD_026 R9.1。
> - **Unity 可玩壳（7e67737，★需编辑器验证）**：SessionRuntime 扩桥（选年/选城/纪元寿命/被俘/武将录/自立）+ GameSetup/Roster/Defeat 三屏 uxml+controller + HUD 显示 公元年·季·年龄·阶段/推进改一周/覆灭转被俘屏。纯 C# DLL 不变。
> - **T4 羁绊（1f31806）**：Bond+BondEffectService（同场协同/仇怨互扣，封顶）+ GeneralBonds 谱（桃园/曹夏侯/孙周/诸葛姜维/吕布董卓等）+ 接入 FromOffensive 攻方士气。崩解（阵亡狂怒）留后续。
> - **W4 兵种×地形（8c445eb）**：ZoneBattleConfig.TroopTerrainMul（骑利平原/水利渡口/步利隘口坚城，杠杆非克制）接入 SidePower。（#9战场模板=#3、#10战法条件已有富集）
> - **W6 引导（b68a176）**：OnboardingHints +6 新系统 cue（纪元一生/选城/兵种地形/羁绊/被灭续局/自立无退路）。DisplayNames/UI草稿已前批/Unity 壳承接。
> - **W5 生涯（ca8ad21）**：CareerView 官阶八阶中文头衔（太守…大都督/继业之主）+功名+在野。外交/多城/治理系统前已实现接入；此补用户可见头衔层。
> - **留后续**：羁绊崩解动态（阵亡狂怒）；W5 外交/多城/治理内容深度（系统已在，待具体内容方向）；Unity 壳编辑器验证 + uss 样式。

## ✅ 完成（2026-07-05 续3）— 争霸放慢（涌现非照史）+ 被灭走向（被俘→…→投奔，唯身死才终）

> **用户定两事，全落地，dotnet 1081→1091 绿，push 13cc571 / d1d2eca。**
> - **#1 争霸放慢 + 涌现（13cc571）**：运行期兼并由"每季"改"每年至多一次" + 缓和权重 0.4（Domain 默认 0.8 仍供争霸单测）。(gap/sum) 自刹车 → 三/四国鼎立可久持，非必然收敛一家；走向 AI 涌现自掌、不照搬演义；大事件仍条件触发（ADR-0007）。测试锁：过 20 年天下仍未定 + ≥5 家。
> - **#2 被灭走向（d1d2eca）**：势力覆灭（末城破）→ 太守被俘 → AI 判生死 →（不杀）归顺? →（不归顺）释放? →（获释）投奔他主（AI 判收不收，收则复为太守）；**唯身死才终**；收留/归顺一律太守（普通武将路线预留暂不做）。
>   - Domain/Defeat：CaptivityService（种子化+名声调制：不轻杀名将/怕放虎归山/名过顶招功高之忌/小势力容不下太守）+ DefeatFlow 状态机。CampaignRuntime.BeginDefeat/IsPlayerEliminated 入口。GDD_026 R9。测试 +10。
>   - **✅ 活世界续局已接通（3f2a498，dotnet 1094 绿）**：CampaignSessionService.ReseatGovernor（新主割城予玩家=控制权变更 HistoricalDivergence + 争霸态玩家+1/新主-1 + 城市治理重置于新城，世界/时钟不动）；CampaignRuntime.ContinueUnderNewLord（DefeatFlow.CanPlayOn→取新主非治所城复位；_capitalOverride 覆盖治所；CurrentSuzerain）。端到端测试:被灭→归顺→同世界同年(191)同龄复位、天下续推进、新主被割一城。**#2 全链闭合。**

## ✅ 完成（2026-07-05 续2）— 时间尺度定案 + ① 事件按公元年（用户定周/季/年）

> **用户定时间尺度**：世界步=周、季=13周、年=52周=4季、战斗仍用日内时段、争霸/事件/寿命按季/年、加跳时、行动时长按周。dotnet 1080/1080 绿。提交 f72abab。
> - EraCalendar 改周基；CampaignRuntime.AdvanceWeek/Season/Year + CurrentSeason；争霸由"每日一步"改"每季一步"；行动时长按周（探子2/征粮2/修工事3/安抚1周）。
> - **① 历史事件按公元年铺开**：BuildHistory 事件挂真实史年（赤壁208/官渡200/夷陵222…，早于锚点归拢开局），沿一生逐年遇之。事件通报测试重写为"按年沿一生触发"。
> - **⚠ 新发现的平衡风险（待调参）**：一生~48年=~192季=至多192次争霸兼并 → AI 很可能在 200 年前就统一天下，导致"活到晚年天下已定、无仗可打"。**争霸每季力度须调参**（EndgameConfig/ContentionConfig 联调 + 手感），否则"活过整段历史"的前提被破。属平衡/手感，建议与用户定或做一版保守调参 + 玩测。
> - **② 多锚点年（200/208/220/184）仍未做**：纯内容大工程——每年一张不同归属的世界盘 + 该年布防表。infra（AnchorYear/按年事件/GeneralsAt(city,year)）已就位，待逐年填盘。

## ✅ 完成（2026-07-05 续）— GDD_026 余下切片 + 武将谱大扩（用户授权全推进）

> **续做：武将谱 48→106、④开局串联、③传承。dotnet 1072→1078 绿。①事件按年/②多锚点年因耦合"年↔回合节奏"平衡决策，暂缓待与用户定。**
> - **武将谱扩充批1（d6d2b1b）**：+58 名演义名将（魏21/蜀14/吴14/群雄董卓黄巾11），全档案（战阵/谋略/心/标签）+ 生卒年 + 中文名 + 部分190布防。生卒驱动登场退场（董卓190在200亡、邓艾197生190未出、马谡入谱）。→ 谱 106 人。仍为批1，次要人物续批。
> - **④开局串联（7921376）**：`GameLauncher`（选锚点年→选城/选剧本→开局）纯 C# 流程，AnchorYears/NamedStarts/GovernorCities/StartNamed/StartGovernor → 产出已开新局 CampaignRuntime。MVP 可从头玩一遍。
> - **③传承（0b28c5c）**：`CampaignRuntime.SucceedHeir`——寿终后子嗣续局，同世界同治所生涯延续，新一世自当前公元年弱冠起、寿命另掷（世代数扰种子）。IsLifeOver/Generation。一生闭环。
> - **暂缓（需用户定"年↔回合节奏"再做）**：① 历史事件改按公元年——现事件按日窗早触发；若直接改真实年（赤壁~208=约第216日），而争霸每日推进一步会在数十日内统一，事件将永不触发 → 须先定 SegmentsPerYear + 争霸推进节奏（平衡/手感，属用户裁定）。② 多锚点年 200/208/220/184 世界快照+布防（各是一整块内容），infra 已就位（AnchorYear/快照按年），待锚点年内容分批填。⑤ 谱扩向~250（续批）。
> - **仍留**：宗主俸禄/生涯深度整合；Unity 选年/选城/一生HUD/武将录 veneer（★需编辑器验证）。

## ✅ 完成（2026-07-05）— GDD_026 空降者·纪元开局与一生 · MVP 三切片（用户全权授权自主实现）

> **新大方向（用户定）**：玩家是与三国无关的空降者，任选年份×城池活出一生。设计→实现全套，dotnet 1059→1071 绿（+12），每片同步 3 DLL 并 push tk。设计先出 GDD_026 + ADR-0015（e8018ee），再实现。
> - **三决策（用户拍板）**：① 时代广义 184–280，真实公元锚点年（命名剧本，首发 190 讨董）；② 空降者有寿命（一世→寿终/退隐→传承或重开）；③ 任选非君主治所城做太守，该年该城武将归你。
> - **切片①纪元+寿命（b42a373）**：`EraCalendar`（公元年由抽象日-段纯函数派生，当前年=锚点年+Day/DaysPerYear，无新权威态/无随机）；`ArrivalLife`（入场年龄+确定性寿命 base48±7，寿终=可续自然落幕，人生阶段只给定性档 春秋鼎盛/年事渐高/风烛残年，不给倒计时）；`PlayableStart.AnchorYear`；`CampaignRuntime.CurrentYear/LifeView`；`ArrivalLifeView`。
> - **切片②武将生卒+190布防（71a1cbb）**：`GeneralDossiers` 加 48 将生卒年（史载近似）+ 190 讨董布防表（部将→任职城）。`AvailableAt(year)` 由生卒判在世出仕（诸葛/司马 190 尚幼，华雄 190 在·208 亡，关张随刘备）；`GeneralsAt(city,190)`。名将随时代登场/老去。
> - **切片③任选城太守（71a1cbb）**：`PlayableCampaign.GovernorStartOf(city)` 程序化开局——空降为任一非君主治所城太守（`IsRulerSeat` 拒），治所自宗主 carve（BuildStartConfig/InitialContention 一致，城数守恒·仅易主），该年该城在职武将归 `DeputyRoster`。`SelectableGovernorCities` + `GovernorCityChoiceView`（选城屏，中文名/宗主/部将数，反全知只给数目）。默认汜水关剧本字节级不变。
> - **留后续切片**（未做，报告已列）：历史事件改按公元年门（现仍按日-段）；多锚点年 200/208/220/184 世界快照+布防；传承子嗣谱系（现最小=重开/单继承占位）；宗主关系/俸禄的深度生涯整合（现太守=独立席+宗主记名）；运行期"选年→选城→开局"入口串起 + Unity 选城/一生 HUD veneer（★需编辑器验证）；武将谱扩向 ~250 全生卒/历年布防。


## ✅ 完成（2026-07-05）— 谋略档 + #1 多剧本 + #2 武将目录 + #3 逐城地形战场（用户授权自主做完）

> **谋略档→#1→#2/#3 三连，dotnet 1047→1059 绿（+12），3 DLL 每步同步，各自提交并 push tk。**
> - **谋略档（7cab9c2）**：与战阵档对称的第二条隐秘量级档。StrategyTier（愚钝/寻常/机敏/智略/经天纬地）+ 系数区间 + 右偏抽取（k=2），乘进已成型兵法的战力加成（ConditionBonusEach×成型数）。攻方取在场最高谋略档为主谋，每回合种子化抽取计谋系数放大条件加成——诸葛之计比马谡狠，偶有"一计定乾坤"。与战阵档独立（吕布绝世武·愚钝谋）。48 将评定。RollStrategy 复用 CombatProwess 区间数学。RoundResolutionService.StrategyMultiplier 接入。
> - **#1 多剧本/势力选择（364717e）**：CampaignRuntime 不再硬编码 PlayableCampaign.Player/Fanshui 静态身份，改读注入场景实例席位（PlayerFaction/PlayerCapital/PlayerLord/OffensiveTarget）。天下大盘抽为共享 World 表（17 席），玩家席位由 PlayableStart 指定 → 争霸/终局/出征目标随所选势力自然重定向。PlayableStartCatalog 3 开局（汜水关太守/刘备·小沛/孙策·江东）。默认剧本字节级不变。ScenarioChoiceView 选择屏投影。**注：与既有 Application.Session.ScenarioCatalog（服务级配置注册，另一层）区分，故命名 PlayableStartCatalog。**
> - **#2 武将目录 infra（6f16516）**：GeneralDossiers.All 遍历全体档案（稳定序）；反全知 GeneralRosterView/GeneralCardView 只呈中文名 + 气质性情文字，绝不投影数值/隐藏档/隐秘心（GDD_025 R1）。DisplayNames 补全 30 名将中文名。GeneralTagText 17 标签→中文。
> - **#3 逐城/地形战场（6f16516）**：BattleFieldCatalog.ForTerrain 按目标城地形选战场模板——正面区地形随之变（唯坚城得工事加成；隘口设伏/渡口水火/平原骑冲/高地夜袭皆无），侧翼/粮道/掩护/预备四区共用骨架（区 id 不变→引擎/planner 零改动）。BattleField 重构为 Compose+SideZones+StandardAdjacency（Default 输出不变）。PlayableStart.TargetTerrain 数据驱动，FromOffensive 按 prep.Terrain 选场。
> - **残留**：T4 羁绊系统（GDD_025 设计但未实现）；谋略档/战阵档更深平衡打磨；武将录/势力选择/逐地形战场的 Unity 屏 veneer（★需编辑器验证）；roster 由 48 扩向 ~250。


## ✅ 完成（2026-07-04）— epic-031 战斗核心 + 战役接线(#1) + 敌AI深度(#2)（用户授权自主做完）

> **续做 #1 接线 + #2 敌AI深度，dotnet 927/927 绿，DLL 同步。epic-031 → Complete。**
> - **#1 接线**：CampaignRuntime.LaunchOffensive 改为 授权门→进入区域战斗（多回合）→ ConcludeOffensive 终局接占城C（ResolveConquest 权威）/退兵可继续；新增 StartDefenseBattle 守城入区域防御战（玩家守方 vs 敌AI）。OffensiveResultView 加 Started/Victorious/Defeated 工厂 + BattleInProgress。SessionRuntime 加战斗驱动透传。Application 一击结算+脚本战斗保留（既有测试不破）。OffensiveRuntimeTests 更新为新流 + 守城测试。
> - **#2 敌AI深度**：EnemyZoneAiService 角色感知（攻方 ObjectivePush 压目标/乘虚·守方巩固）+ 姿态决策（守/主攻/侧翼佯攻·低士气转守）+ 优势/退避因子（EnemyAiConfig 扩）。反全知/种子/记忆/不作弊不变。+2 测。
> - **平衡**：引入士气崩溃溃散（IsBroken 含 morale≤0）+ 减员/掉士气随战力比放大封顶 → 强攻方 6 回合内可破正面。TotalStrength 只计未溃散。
> - 残留：HUD 出征面板→战斗屏场景路由（Unity veneer）；敌AI 更深迭代（ADR-0013 D8）。

## （历史）✅ 完成（2026-07-04）— epic-031 战斗核心层 S1-S7 实现

> 用户裁定：替换脚本战斗 · 敌AI一步到位最完善 · 战斗=与其他三国演义差异点须最溢满。**S1-S7 全实现+测试，dotnet 924/924 绿(+32)，3 DLL 同步。**
> - Domain/ZoneBattle：ZoneId/DetachmentId/BattleSide/Posture · Zone/BattleField(5区+邻接,非坐标) · Detachment(减员按比例缩放编成) · ZoneEngagementState/BattleClock · ZoneBattleState(确定性哈希+记忆) · ZoneBattleConfig/Context · ZoneConditionService(按区按回合涌现门) · RoundResolutionService(交战/减员随战力比封顶/涌现冲击/优先序) · ZoneCommandService(调动邻接+在途/姿态/稳定错误码) · EnemyAiMemory/AiWorldView(反全知)/EnemyAiConfig/EnemyZoneAiService(种子化整数加权,饱和Pow,同规则不作弊,落地GDD_016) · ZoneBattleOutcome(破正面即破城)。
> - Application/Battle：ZoneBattleService(回合编排:玩家命令前置→敌AI→结算→终局) · OffensiveDeploymentPlanner(六维→分区部署桥/守方布防/上下文)。
> - Presentation：ZoneBattleView(各区态势+涌现+MoveOptions排兵布阵) · ZoneBattleRuntime(可玩:调动/姿态/推进/投影 + Demo + FromOffensive)。
> - Unity 壳：ZoneBattle.uxml + ZoneBattleSession + ZoneBattleController（只 Unity 编译，未 dotnet 验证）。
> - 锚点：GDD_021 Implemented · ADR-0012/0013 Accepted · TR-zone-001~010 · epic-031 EPIC 7/7 Core Complete。
> - **迁移待办（接线，非新能力）**：把 CampaignRuntime 守城脚本战斗 + LaunchOffensive 一击结算改为进入 ZoneBattleRuntime → 终局接占城C/后果（单独 story 谨慎替换，勿破既有 892 出征/HUD 测试）。敌AI 深度按 ADR-0013 D8 迭代。
> - 提交链见下（此前 d322714 出征六维 / 631d925 GDD_021 / 63f7206 ADR-0012/0013，均 push）。

## （历史）新方向（2026-07-04）— 战斗核心层：战场区域部署与区域战斗（用户定为核心玩法，须最完善）

> 用户澄清「排兵布阵是重点」：不是纯角色布阵（嫌单薄）、也不是三国志坐标微操——要**区域布阵**（指范围非坐标 + 军师按区给方法）。三点拍板：① **战中可调整**（战争核心好玩点）；② 先固定几类区域，**预留每城场景自定义**（数据驱动 Future）；③ **敌方AI区域博弈必做**（战争核心，须最完善）。
> - 已建 **GDD_021 战场区域部署与区域战斗**（Draft，18 段）：区域部署 + 回合制战中调整 + 敌方区域AI + 攻守统一；实现 GDD_010 执行层、**首次落地 GDD_016 敌AI**。登记 gdd-index。
> - 核心模型：战场=~5 命名区域（地形/条件禀赋/邻接）；支队(将+兵种+兵力+姿态)派到区；回合循环=玩家调整→敌AI(反全知种子softmax)→同步结算→按区条件涌现；复用 TacticCondition/六维准备/士气/情报/天气；全确定性(ADR-0004/0006)。
> - 待补 ADR：拟 ADR-0012（确定性区域战斗引擎）+ ADR-0013（敌方区域AI效用模型，或扩 0006）。
> - **规模=一个战斗核心 epic（epic-031-zone-battle-core）**，非小加法。设计先行，模型确认后逐 story 建。
> - **▶ 待用户确认**：① 模型方向对不对；② 迁移：区域引擎**替换**竖切 scripted 战斗（推荐）vs 并存；③ 敌AI"便宜80% MVP→迭代到完善"分期可接受否。确认后：ADR → epic/stories → Domain 引擎+敌AI+Presentation+Unity+测试。
> - 前置已完成（见下）：出征六维准备 + HUD 入口（892/892 绿，未提交）——区域战斗将把"单选布势路线"升级为"多区域部署"，六维准备作为部署输入保留复用。

---

## ✅ 完成（2026-07-04）— epic-029 续：出征准备维度重做 + Unity HUD 出征入口（用户选「档3」，自主做完）

> **已全部实现 + 测试 + 同步 DLL。dotnet 892/892 绿（-warnaserror；基线 871 + 21 新测），零回归。未提交。**
> - 设计权威：**GDD_019 → v2**（六维准备：§4a/§5 F1-F3 门表/§6/§8/§12 AC-3a~3d+7）+ **ADR-0011 Accepted**（多维确定性出征准备模型）。登记齐：adr-index/gdd-index(Revised v2)/technical-preferences/tr-registry(TR-offensive-006~010)。
> - Domain/Conquest（新）：OffensiveEnums(TroopType/GeneralSpecialty/ApproachPlan/TerrainKind) · OffensiveCommand(OffensiveGeneral 统率/武勇/智略/专长 + 副将 decay + 军师) · TroopComposition · OffensiveTiming；OffensiveSetup 重写（六维闭合因果 Derive，纯函数/整数定点/无随机/无克制/无坐标）。
> - Application：PlayableCampaign 暴露出征卫星配置（LeadGeneral/DeputyRoster/DefenseOf虎牢关/TerrainOf/OffensiveSetup/SiegeResolution/Occupation/LordFaction/OffensiveTargetCities）。LaunchOffensive 签名不变（透传 prep）。
> - Presentation：CampaignRuntime 出征编排（RequestOffensiveAuthorization/OffensiveTargets/BeginOffensive/PreviewOffensive/LaunchOffensive + TargetScouted 反全知）；OffensiveScreens（OffensiveText/OffensiveTargetsView/OffensivePlan 草稿/OffensivePlanView 预览含缺失提示无胜率/OffensiveResultView）；HudPhaseView 治理加「出征」。
> - Unity 壳：SessionRuntime 透传 + HudController 出征面板（授权/选目标/四路线/增兵加粮配骑兵/军师/副将/预览/发起/结果）+ Hud.uxml 出征卡。**Unity .cs 只 Unity 编译，未 dotnet 验证（同既有薄壳惯例）。**
> - 测试：OffensiveDomainTests（30，含单调/门/无克制/无坐标/堆将递减/端到端）· CampaignConquestTests 适配新 prep · OffensiveRuntimeTests（7，Presentation 入口端到端）。
> - **待用户回改/后续**：数值平衡（§8 Tuning 系数占位待调）；专长 GeneralSpecialty 目前仅展示未接降门逻辑；水军/火攻未入 MVP（无风/旱天气）；可选把本批正式立成 epic-029 补充 story + /code-review + commit。

## ▶▶▶（历史）进行中（2026-07-04）— epic-029 续：出征准备维度重做（用户选「档3」，授权自主做完再汇报）

> 用户要求把「出征入口」从最小接线升级为**完整重做出征准备维度**，自主实现，做完汇报。设计锁框架已达成：
> **阵型=布势路线（非坐标）· 兵种=杠杆（非克制三角）· 武将/副将/军师=派谁轴（接 GDD_014）· 加时机/天气 + 侦察情报门（反全知）**。

### 核心设计（复用最大化，零新造兵法）
- **布势路线 = 复用现有 4 个 TacticTag**：正面强攻(无兵法·纯战力) / 假退诱敌(FeintAmbush) / 长围断粮(SupplyExhaustion) / 夜袭(NightRaid)。守城待变属守方，不作进攻路线。
- **多维准备做成「条件能否成型的门」**：兵种份额/时机时段/天气/军师随军/是否侦察 → 决定所选路线的 TacticCondition 是否携入开战态。`OffensiveSetupService.Derive` 扩为多维确定性映射（整数/定点，ADR-0004）。
- **闭合因果**：force = 基 + 兵力·系数 + 主将统率 + 兵种地形契合 + 路线修正；morale = 基 + 补给档 + 主将武勇（封顶）；conditions = 路线模板 ∩ 门（兵种/时机/天气/军师/侦察）。

### 执行序（跨层大批次，逐层补 dotnet 测试）
1. **设计权威**：GDD_019 → v2（§4 六维规则 / §5 公式 / §6 数据模型 / §8 Tuning / §12 AC）+ 新 **ADR-0011**（多维确定性出征准备模型）。
2. **Domain/Conquest**：扩 OffensivePreparation（+ OffensiveCommand/OffensiveGeneral/GeneralSpecialty、TroopComposition/TroopType、ApproachPlan、OffensiveTiming、Scouted）；扩 OffensiveSetupService + OffensiveSetupConfig。+ 测试。
3. **Application**：PlayableCampaign 暴露 将领roster/兵种/路线/时机/真守备(虎牢关)/君主势力id/占城配置；CampaignSessionService 出征备战草稿态 + 组装命令 + LaunchOffensive 消费草稿。+ 测试。
4. **Presentation**：CampaignRuntime 出征组装方法 + OffensivePanelView/OffensiveTargetView/OffensiveResultView；HudPhaseView 治理相位加「出征」。+ 测试。
5. **Unity 壳**：SessionRuntime 透传 + HudController 出征面板 + UXML。
6. 验证 dotnet 全绿 + 同步 3 DLL Release + 汇报（含我替定的全部设计细节）。

### 复用锚点（已确认）
- TacticCondition 枚举（TacticEnums.cs）：假退伏击(0-2)/断粮(10-12)/守城(20-22)/夜袭(30-33) 原子齐全。
- WeatherType：Clear/Overcast/Rain/Fog（无风/旱 → 火攻不入 MVP，与既有 scope 一致）。
- RetinueMember 仅 Affinity，无战斗属性 → 武将战斗属性需新建 OffensiveGeneral（统率/武勇/智略/专长）。
- 现有 LaunchOffensive/占城C/授权门/存档已实现（epic-029 5 story Complete），本批只**扩准备维度前段** + 补 Presentation 入口。
- 基线：dotnet 871/871 绿（开工前）。**未提交。**

---


## Session Extract — /create-stories epic-029 2026-07-04
- **5 story 已写**（Ready，0 Blocked，governing ADR 全 Accepted）：001 君主授权出征入口(Int/ADR-0009) · 002 攻城战接入进攻视角(Int/ADR-0009) · 003 闭合因果准备→战果(Logic/ADR-0004) · 004 占城归属方案C(Logic/**ADR-0010**) · 005 出征后果→功绩→升官(Int/ADR-0009)。依赖链线性 001→005。覆盖 GDD_019 全 8 AC。
- **TR-offensive-001..005 已补登** tr-registry（v5，沿 epic-028 TR-ux 补登惯例——GDD_019 未过 /architecture-review）。
- EPIC.md Stories 表填充 + epics/index.md epic-029 → 🟢 Ready for Stories。lean 模式：QL-STORY-READY 跳过、inline QA 用例（未派 qa-lead）。
- **▶ 下一步（用户止于 dev-story 前）**：`/story-readiness story-001` → `/dev-story`。Advisory：ADR 落定后 `/consistency-check` 登记 conquestIndex/rebellion_lean/OwnershipVerdict/CampaignAuthorization；前向 epic-020/022/025 须先于引用 story 存在。**本批全部未提交。**

## Session Extract — /review-all-gdds（焦点 GDD_019）2026-07-04
- Verdict: **CONCERNS**（焦点审查，非全量；001–016 已 2026-06-24 审过）。报告：`design/gdd/gdd-cross-review-2026-07-04.md`。
- Flagged: gdd-019 / gdd-014 / gdd-004 / gdd-009 / gdd-010 / gdd-015。Blocking: **None**。
- **5 Warning 全就地闭合**：W1 GDD_014 自立触发条件③接 `rebellion_lean`（+Tuning `rebel_lean_min`）· W2 GDD_019 F2 改引 GDD_010 §7 条件涌现结算、与事后 TacticRecognizer 分述 · W3 GDD_014 加"出征授权"君主任务子类型（额度随 Rank）· W4 gdd-004/009/010/014/015 各加反向依赖引用 GDD_019 · W5 GDD_019 §8 加出征功绩速率反支柱护栏。
- **设计门推进**：GDD_019 → **Reviewed**；ADR-0010 → **Accepted**（文件头 + technical-preferences 日志一致）；gdd-index 已 Reviewed；epic-029 EPIC → **Ready for Stories**。
- Advisory 待办：ADR 落定后 `/consistency-check` 登记 conquestIndex/rebellion_lean/OwnershipVerdict/CampaignAuthorization；前向 epic 020/022/025 须先于引用 story 存在。
- **▶ 下一步**：`/create-stories epic-029`（细化 5 story AC/TR-ID/QA）。用户选"先补设计门再 create-stories"，停在 dev-story 前。未提交。


> **最后更新**：2026-06-27
> **语言**：全程中文（见用户偏好 memory）
> **审查模式**：lean

> **▶ 批次完成（2026-06-27）**：用户授权「把 Must 完成后继续所有 Should」——**全部 8 story（5 Must + 3 Should）已实现+审查+收尾+push**。每 story：实现+测试→inline lean review→story-done→commit+push。**全套 dotnet 536/536 绿，-warnaserror 0。**
> **提交链（push tk/main）**：11-1 `cf00edb` · 12-1 `6414b32` · 11-2 `51039f6` · 11-3 `dfed215` · 12-2 `1003b4e` · 11-4 `ff3b284` · 12-3 `43dd9af` · **11-5 生涯存档**（CareerSaveCodec 版本化 DTO+版本/指纹校验+LordMissionLog；536/536）。
> **★ Sprint 02 Must+Should 全清（8/8）。epic-011 = 5/5 Complete。** 仅剩 Nice(3)：12-4 归属投影（已被 11-4 的 ICityControlAuthority 解锁）/12-5 抽象结算/12-6 世界存档——**未做（用户未要求）**。
> **✅ Sprint 02 全部收尾完成（2026-06-28）**：11/11 story（Must 5 + Should 3 + Nice 3）实现+审查+收尾+push（556/556 绿）。`/smoke-check` PASS（smoke-2026-06-28.md）· `/team-qa` **APPROVED**（qa-signoff-sprint-02-2026-06-28.md，0 缺陷）· `/retrospective`（retro-sprint-02-2026-06-28.md）· **epic-011 5/5 + epic-012 6/6 判 Complete**（EPIC.md + epics/index.md 已标 ✅）。
> **HEAD 待推**：close-out 文档批次（smoke/qa-signoff/retro/epic 状态/index/active）。提交链见下方提交序段。
> **下一步候选**：① 敌方 AI（gdd-016/ADR-0006，仍 Reviewed）建 epic 进实现；② Presentation 把 Meta 层接 UI；③ 存档三段统一信封整合（retro 行动项 #2）；④ 统一测试路径约定（retro 行动项 #1）。
<!-- QA RUN: 2026-06-28 | Sprint: sprint-02 | Verdict: PASS（APPROVED）| Report: production/qa/qa-signoff-sprint-02-2026-06-28.md -->

## Session Extract — 全游戏 review 2026-06-28（6 层）
- 报告：`docs/reviews/full-game-review-2026-06-28.md`（未 commit）
- 核心结论：**内核质量极高、装配极薄**。代码零禁则违反/零确定性泄漏/556 绿/8 ADR 全 Accepted；但 Meta 层(011/012)+~13 Domain 系统是**孤岛**，可玩装配≈竖切守城那一局。
- 🔴 Blocking：① BLK-1 Meta 层+多数系统未装配成可玩太守循环（GameSession 只驱动竖切系统）；② BLK-2 敌方 AI（gdd-016/ADR-0006）整块未建（无 epic/无代码）。
- 🟠 Concern：CON-1 治理状态全面漂移（gdd-index/epics-index/**control-manifest 卡在 Foundation**）；CON-2 Meta 层未纳入全局结算顺序（装配后即确定性 bug 源）；CON-3 gdd-016 三处设计缺陷（悬空引用 IntelProjection/负 softmax/lerp 未 clamp）；CON-4 自立新势力创建权威 014↔015 未裁；CON-5 SliceScenario 硬编码(ADR-0003)。
- 🟡 Advisory：雪球/粮源汇/刷战斗护栏未验证/民心恢复薄；依赖双向缺口；015 抽象结算随机源代码已对但 GDD 未声明；无目标循环端到端测试；存档三段未统一。
- **建议处置序**：①廉价校准治理状态 →②编辑级补结算顺序+裁自立权威 →③**裁决方向：先装配可玩太守循环（建议）vs 继续造内核** →④裁决敌方 AI 去留。
- **下一步等用户裁决**：是否（a）先做廉价治理校准；（b）开"装配集成 epic"；（c）建敌方 AI epic。

## Session Extract — Concern/Advisory 修复批次 2026-06-28
- 用户要求"先解决所有 Concern+Advisory，再谈可玩性(Blocking)"。已完成 8/9：
  - 文档波 `4b389e4`：FIX-1 治理状态校准（gdd-index/epics-index/control-manifest v2）· FIX-2 systems-index Meta 层结算顺序+破环 · FIX-3 gdd-016 三缺陷+registry · FIX-4 自立权威裁定（registry 新增 faction_existence）· FIX-5 平衡护栏 · FIX-6 文档完整性。
  - 代码波 `e993847`：FIX-9 Meta 跨系统链端到端测试（3 测）· FIX-8 战役存档统一信封 CampaignSaveCodec（5 测）。全套 **564/564 绿**。
- **唯一待定 CON-5（FIX-7 SliceScenario 配置化）**：SliceScenario 已是集中式不可变配置（GameSession 无魔法数）；残留仅"内联字面值 vs 外部数据源"。判断=最好并入 BLK-1 装配（届时建真 SO→不可变配置管线），现在单独做有 throwaway 风险。**已挂起待用户裁决**。
- **下一步**：讨论 BLK-1/BLK-2 可玩性装配方向。

## Session Extract — 可玩装配路线图 2026-06-28
- **路线图（权威）**：`production/roadmap-playable-assembly-2026-06-28.md`——把"内核→可玩太守循环"拆为 7 模块 M1~M7（= 未来 epic-013~019），逐模块完成，M7 完成=Unity 可测整段游戏。
- 模块：M1 会话装配内核 CampaignSession（脊梁）→ M2 场景配置化+战役目录（含 CON-5 收尾）→ M3 回合间治理/推进循环 → M4 敌方AI便宜80% → M5 Meta三屏UI → M6 首个循环内容 → M7 端到端可玩验证+平衡。
- 依赖：M1→M2→M3→M6→M7；M4 在 M1 后可并行；M5 在 M3 后可并行。
- 任务清单已建 M1~M7（pending）。**建议先开 M1**（脊梁，解锁其余）。
- CON-5（SliceScenario 配置化）已并入 M2，不单独做。

## Session Extract — 合并 codex 规划 + 治理修正进 main 2026-06-28
- **FF 合并 `codex/governance-sync`**（提交 1d26d2c）进 main：修了我引入的真 bug（sprint-status 6 个 story 卡 backlog→done）+ 补 adr-index(0006-0008)/index.md 统计段/测试数精度(556 基线/564 当前) + vertical-slice skill 测试规格。
- **带进 codex 三件套**（worktree 未提交 → 复制进 main）：
  - `production/full-game-loop-module-plan-2026-06-28.md`（**权威规划**：M00–M16 循环模块 + 双层管理 + Production Gate 五裁决 + 设计文档清单 + agent 视角）——已**合成注入**我的"设计状态分层/FIX 地基(§3.5)" + 6-23 的"MVP 出关门/Kill Criteria/明确不做/竖切战斗层未接线实况(§5b)"。
  - `docs/architecture/adr-0009-campaign-session-assembly.md`（**Proposed**，CampaignSession 装配边界，模板完整、与 FIX-2 日界顺序一致）。
  - `production/epics/epic-013-campaign-session-assembly/EPIC.md`（**Draft for Review**，TR 映射完整）。
- **我的 `roadmap-playable-assembly-2026-06-28.md` 标记被取代**（内容已并入 module-plan）。
- **三份来源最优合并结论**：codex 模块框架最强（M00-M16+双层管理+Gate）；我补"设计状态分层+FIX 挂钩"；6-23 补"硬验收/Kill Criteria/范围纪律/战斗未接线"。关键修正=不把战役当黑盒。
- **下一步（用户指示）**：派子代理重新评审合并后的规划。
- **待用户裁决（codex 5 裁决 + epic-013 5 问）**：M00 第一优先 / 君主等先补 GDD-017+ / 敌方 AI 先战术层 / 城市等须证明喂战争 / ADR-0009 转 Accepted / 是否加 TR-session-*。

## Session Extract — 复审修订全落 + 进入"可开工"状态 2026-06-28
- 用户选 (a) 落修订 + 我拍板两设计裁决，推到能开工。
- **两设计裁决（已写入文档）**：① 代偿取胜路线满足 MVP 出关门，完整 GDD_010 战役命令层(B3/B4)后置 M06；② 自立新势力创建权威=GDD_015（确认 FIX-4），写进 ADR-0009 R-3 + 排进后果写回 story。
- **ADR-0009 → Accepted**（含复审修订 R-1~R-7：存档段统一/迁移粒度/势力创建权威/契约三项/可执行闸门/原子写回事务/分层）。adr-index + technical-preferences ADR 日志同步。
- **TR-session-001..005** 入 tr-registry（日界序/原子写回/统一存档/胜败可继续/确定性哈希）。
- **epic-013 → Ready**：5 open Q 全裁定 + TR-session 映射 + **§First Sprint Scope（sprint-03，~6 story、~4.5d，S1-S6 全装配）**。index.md 新增 Assembly 层登记 epic-013。
- **plan 修订**：R3 排序（M08 战术 AI 排 M06 前）+ §5b.1 代偿裁决 + §11.2 估算/容量第三轴。
- **review 报告回填**：CON-2/3/4 + 全 Advisory 已闭、Blocking 转规划。
- **★ 可开工状态达成**：ADR-0009 Accepted + epic-013 Ready + 首批 6 story 已 scoped+估算。**下一步开工 = `/create-stories epic-013-campaign-session-assembly` → /story-readiness S1 → /dev-story。**
- 注：S1/S2/S6 无 ADR 前置可先行；S3 须 R-3、S4 须 R-1/R-2 落 method spec（裁定已在 ADR-0009）。

---

## ▶▶▶ 新会话从这里读起（2026-06-27）— Sprint 02 开工：11-1 已实现，待 review/done

> 用户授权直接开工 A（不逐步确认）。已用 `/dev-story` 实现 **epic-011 story-001 CareerState 骨架**。**全套 dotnet 465/465 绿（451+14 新），-warnaserror 0**。**尚未 commit**（按惯例待用户指示）。

### 本轮产出（11-1，Status: Ready → In Progress）
- **Domain**（`src/Domain/Career/`，10 文件）：`Rank`(0–7 官阶梯队) · `OfficeRole`(城守/副将/内政主事/军师) · `CareerErrorCode`(稳定码) · `CareerState`(merit/renown int≥0 + lord_standing Q16.16∈[0,1] + rank + FactionId? + 在野标志；不可变+不变量校验+AppendTo 哈希) · `RetinueMember` · `RetinueState`(僚属+好感+官职任免，规范序哈希) · `CareerSnapshot`(Career+Retinue 组合哈希) · `CareerCommand`(4 命令：GainMerit/AdjustLordStanding/PromoteRank/AssignOffice) · `CareerCommandResult`(成功/失败+稳定码，失败=原态不变) · `CareerStateService`(Domain 解析，纯函数确定性)。
- **Application**（`src/Application/Career/CareerCommandService.cs`）：唯一写路径，前置校验(空命令→NullCommand)后委派 Domain。
- **Test**（`tests/unit/ThreeKingdom.Domain.Tests/Career/CareerStateTests.cs`，14 测）：覆盖 AC-1/5 字段类型+越界拒、AC-3 非法操作稳定码+无部分写入(负功绩/越级/空命令/非成员任免/在野晋升)、AC-4 同前态同命令流哈希一致 + 顺序敏感(同职位先后任免→哈希异)。
- **路径偏差**：测试落统一工程 `ThreeKingdom.Domain.Tests/Career/`（沿 epic-001 约定），非 story 原拟 `tests/unit/career/`（不在编译工程内）。
- **结构性边界**：晋升只校验逐级结构（不做 merit/renown/standing 门槛——属 story-002）；lord_standing 钳制非报错（N10 有源有汇）。
- **已改追踪**：story header(Status/Last Updated/Estimate/Test Evidence✓) + sprint-status.yaml(11-1 in_progress, updated 2026-06-27)。

### ▶ 下一步
1. ✅ `/code-review` APPROVED + `/story-done` **COMPLETE WITH NOTES** — 11-1 已关闭（Status: Complete；sprint-status done；EPIC.md ✅）。
2. **接着做（sprint-02 Must 链）**：12-1 WorldState 骨架（epic-012 DAG 根）或 11-2 忠臣晋升。建议先 12-1（另一 DAG 根，并行解锁 12 链）。
3. **commit 时机由用户定**（本轮全部未 commit）。建议命令：`git add src/Domain/Career src/Application/Career tests/unit/ThreeKingdom.Domain.Tests/Career production/epics/epic-011-campaign-career production/sprint-status.yaml production/session-state/active.md && git commit -m "feat: CareerState 权威状态与确定性结算骨架 (TR-career-001/005)"`

## Session Extract — /story-done 2026-06-27
- Verdict: COMPLETE WITH NOTES
- Story: epic-011 story-001 — CareerState 权威状态与确定性结算骨架
- Tech debt logged: None
- Next recommended: epic-012 story-001 WorldState 骨架（DAG 根）或 epic-011 story-002 忠臣晋升

## Session Extract — 12-1 WorldState 骨架 实现+审查+收尾 2026-06-27
- **HEAD 进度**：11-1 已 commit+push `cf00edb`（tk/main）。12-1 实现完成，待 commit+push。
- **产出**（epic-012 story-001，Status: Ready → Complete）：
  - Domain（`src/Domain/World/`，6 文件）：`SurvivalStatus`/`RelationToPlayer` 枚举 · `CityOwnership`(城归属**只读投影**，无写 API，TR-world-003/ADR-0008) · `FactionRecord`(势力 id/君主/存续/领有城池/对玩家关系，规范序+不变量) · `WorldState`(时间引用+势力+城投影+已触发/已分叉事件集合；不可变+规范序哈希+只读访问器) · `WorldProgressionService`(纯函数确定性时间推进，story-002 事件触发的单一注入点)。
  - Test（`tests/unit/ThreeKingdom.Domain.Tests/World/WorldStateTests.cs`，12 测）：AC-1/2 字段+空集合/单势力合法+存续须有君主、AC-3/4 同序列哈希一致+不同序列哈希异+输入序无关+0段恒等、AC-5 归属只读（反射断言无 setter + WorldState 无 public 变更方法）。
  - 验证：全套 **dotnet 477/477 绿**（465+12），-warnaserror 0。
- **Deviations（ADVISORY）**：① 测试路径统一工程（非 story 拟 tests/unit/world/）；② 线性时间可交换，严格非交换序敏感随 story-002 到来（已以不同序列+输入序无关证确定性）；③ Cities 城侧投影与 FactionRecord.OwnedCities 势力侧两视图未交叉校验，权威同步随 story-004 接入。
- **追踪已更**：story header + Completion Notes + sprint-status.yaml(12-1 done) + EPIC.md(✅) + active.md。
- **▶ 下一步**：commit+push 12-1 → 续 sprint-02 Must 链最后一项 **11-2 忠臣晋升**（merit/renown/standing_req 配置化门槛，接 11-1）。之后 Should 层 11-4/12-3/11-5。

---

## ▶▶▶ 新会话从这里读起（2026-06-25）— Meta 层（014/015）已走完文档→实现管线，待开工

> 上个会话从「重启 A」一路推进到 Sprint 02 + QA plan，全部已 commit+push。**HEAD = tk/main = `fddf4c3`，工作树干净。**

### 已完成的整条管线（按提交序，全部 push tk/main）
1. `fcb8e90` 重启 A — 跨系统审查（CONCERNS，5 Warning 全落，W1 城池归属裁定 004 唯一权威）+ 014/015/016 转 Reviewed + registry 首填
2. `761a4fe` 补 014/015 架构 — **ADR-0007**（条件历史世界模型）+ **ADR-0008**（城池控制权契约）均 Accepted + 审查报告 + TR v3
3. `2fe240e` 架构审查验证重跑 — 缺口全闭，裁定 **PASS**
4. `4bc397c` 014/015 → **Locked for Slice** + **epic-011**（生涯，5 story）+ **epic-012**（世界模型，6 story）= 11 story
5. `3168fc0` **Sprint 02** 排期（sprint-02.md + sprint-status.yaml）
6. `fddf4c3` **QA plan**（qa-plan-sprint-02-2026-06-24.md）

### 当前可立即做的下一步（二选一）
- **A. 直接开工**：`/dev-story production/epics/epic-011-campaign-career/story-001-career-state-skeleton.md`
  实现 11-1 CareerState 骨架（DAG 根，解锁 11-2/3/4/5）。已 /story-readiness=**READY**。
- **B. 先清理**：把估算（M/4h）回写进全部 11 个 story header（现为占位符 `[待 sprint 规划填]`，估算实际已在 sprint-02 + sprint-status.yaml）。10 分钟杂活，然后再开工。

### Sprint 02 关键事实（实现时须知）
- **Must(5)**：11-1 CareerState骨架 · 12-1 WorldState骨架 · 11-2 忠臣晋升 · 11-3 自立三分支 · 12-2 历史事件触发门。**Should(3)**：11-4 太守开局守城 · 12-3 分叉传播 · 11-5 生涯存档。**Nice(3)**：12-4 归属投影 · 12-5 抽象结算 · 12-6 世界存档。
- 全 0 Blocked，governing ADR 全 Accepted（8 份）。纯 C# Domain Logic/Integration，NUnit + dotnet test 旁路 Unity 许可。
- **两处跨 epic 接缝**（非阻断）：① 11-4 用最小 CitySeed 配置占位（CitySeed 权威在 epic-012）；② 12-4/11-4 需确认 epic-004（已 Complete）的 `CityControlChanged`/`ICityControlAuthority` 接口（ADR-0008）已落地——缺则先补最小实现。
- **确定性专项回归门**（QA plan）：状态哈希一致 / 存档 round-trip 矩阵 / 自立好感快照隔离 / 历史够不着短路 / 无旁路随机（System.Random/UnityEngine.Random/float 权威路径）。
- 既有：dotnet 451/451 绿（竖切+epic-001~010 全 Complete）；复用 epic-001 Numerics + epic-009 存档信封。

### 状态机要点
- gdd-index：014/015 = **Locked for Slice**；016 = **Reviewed**（未建 epic）。
- ADR：0001~0008 全 **Accepted**（technical-preferences 日志同步）。
- registry：entities.yaml v2（14 跨系统事实）；tr-registry v3（含 TR-career-001~005 + TR-world-001~006）。
- 验证命令：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/...csproj -warnaserror`。

---

## ▶▶ 大方向已锁定并写入文档（2026-06-24）— 历史背景

> 用户休息后重想了整个游戏完整品，经长讨论澄清了一直以来"竖切 vs 大规划"的混淆根源。**结论已落文档，防后续跑偏。**

### 锁定的游戏定位（权威见 `design/gdd/game-concept.md`）
- **固定开局身份：城池太守**。`大势随历史（演义时间线）+ 个人靠抉择`。
- **游戏三层结构**：① 历史世界模型（GDD_015）× ② 太守生涯（GDD_014）× ③ 兵法战斗（GDD_010）。前两层是此前缺失、最易跑偏处，现已补成 GDD。
- **双线**：忠臣晋升（功绩/名望/君主好感→7阶官职→继承基业）/ 自立反叛（实力+好感→自立，结局由关系网三分支：拥立/部分跟随/众叛亲离）。
- **战斗 = 条件涌现兵法沙盒，非自由摆阵 SLG**。澄清："自由布阵 = 布局条件"（位置/隐蔽/时机），**非**阵型坐标/兵种克制/实时微操。已把术语锁写进 GDD_010。
- **玩法靠"加杠杆"而非"加按钮"**：六杠杆（天气/地形/时间/补给/人心/工程）相乘涌现招式库（火攻招牌 + 水攻/隘口/分进合击/诱敌/长围/强攻 + 人心杠杆离间/策反/攻心=护城河独门）。数据驱动加条件链。
- **条件历史解矛盾**（GDD_015）：历史事件=`{时间窗+前置条件+正常结局+分叉结局}`；**玩家够不着则前置恒成立**（早期历史在轨上跑=便宜），强到改变前置才分叉（如提前灭孙权→赤壁不成立），晚期才付分叉成本，且只玩家势力圈脱稿。
- **敌方 AI = 让自由布阵有深度的必需件**（GDD_016 + ADR-0006）：战术层敌将，效用评分+种子softmax（不可预测却可复现）+反全知锁(AiWorldView不接受真值)+渐进记忆+LLM仅装饰不决胜负。"便宜80%"先做。
- **与三国志关系**：参考存在性、不抄全套深度。砍scope尺子：每加功能须答"喂给战斗/生涯抉择什么"，喂不到即砍。

### 本轮写入/改动的文档（防跑偏全套，已落盘）
- 改：`design/gdd/game-concept.md`（太守开局+三层结构+双线+三国志边界+砍scope尺子）。
- 改：`design/gdd/gdd-010-battle-tactics-sandbox.md`（战区+六杠杆+招式库+术语锁；MVP/Future/AI 章节更新）。
- 新：`design/gdd/gdd-014-campaign-and-career.md`（战役与生涯，Draft）。
- 新：`design/gdd/gdd-015-historical-world-model.md`（条件历史世界模型，Draft）。
- 新：`design/gdd/gdd-016-enemy-ai.md`（敌方 AI，Draft）。
- 新：`docs/architecture/adr-0006-deterministic-enemy-ai.md`（**Proposed** — 待 /architecture-decision 或 review 转 Accepted 后 story 方可引用）。
- 改：`design/gdd/gdd-index.md`（登记 Meta GDD 014/015/016）、`.claude/docs/technical-preferences.md`（ADR 日志加 0006）。
- **尚未 commit**：以上均在工作树，待用户指示再提交。

### ▶ 下一步（用户原话："文档更新好了，我们再来谈要做哪些事情"）
- 文档已更新完。**待与用户讨论：先做哪些事**。候选：① 把 014/015/016 三篇新 GDD 逐节细化/过 design-review；② ADR-0006 转 Accepted；③ 直接进 GDD_016 §MVP"便宜80%"敌方AI最小切片；④ 先做 GDD_010 §MVP 的"可玩战区"（火攻+三招+人心杠杆）。
- 注：014/015/016 为 Draft，**未过跨系统审查**，按 gdd-index 控制规则不得直接进实现。

### ✅ A（`/review-all-gdds`）已完成（2026-06-24 重启后跑完）
> **重启 A 已走完 Phase 1-6**。裁定 **CONCERNS**（如预期）。报告已落盘 `design/gdd/gdd-cross-review-2026-06-24.md`。
> **W1 城池归属已由用户裁定**：GDD_004 城级控制权唯一权威 + 唯一变更事件；GDD_015 战略尺度只反映、订阅 004 事件、不独立写；GDD_014 只读。
> **5 项 Warning 已全部落盘编辑（2026-06-24）**：W1 改 004（声明城级控制权唯一权威+变更事件）/015（city.owner 改只读投影、订阅 004 事件）/014（System Outputs 归属改"经 004 触发"）· W2 **反向依赖已彻底补齐**：004←014/015 · 010←016/014 · 015→004 · 007←015/016 · 006←014 · 005←014/016 · 001←015 · 003←015 · 011←016 · 012←016 · 013 显式含 014/015/016 存档边界· W3 GDD_010 命名 TacticRecognizer（§招式库 + Data Model）· W4 010 §7 + 016 StrategicAction 各加追击边界一句 · W5 014 Balancing 加反支柱护栏（非战斗源速率竞争力）+ N10 sink 说明。
> **①②③ 已全部完成（2026-06-24）**：① registry 填充（design/registry/entities.yaml v2：7 entities + 3 formulas + 4 constants——city_control/TacticRecognizer/AiWorldView/StrategicAction/OpponentModel/HistoricalEvent/FactionKnowledge + combat_power/pursue_decision/can_promote + merit/renown/lord_standing/supply_state）。② ADR-0006 → **Accepted**（§Decision 1 补 N-#5 随机源契合 ADR-0004；technical-preferences ADR 日志同步 Accepted）。③ GDD_014/015/016 → **Reviewed**（gdd-index + 各文件状态行）。
> **下一步**：④ commit 本批（进行中）；之后可 push tk/main、或进 014/015/016 §MVP 实现切片。

<!-- CONSISTENCY-CHECK: 2026-06-24 | registry 由空首次填充（v2，14 条跨系统命名事实）| Conflicts: 0（填充模式，非检查）| 来源 gdd-cross-review-2026-06-24 -->
<!-- consistency-check 说明：registry 原为空，按技能 Phase 1 应停（检查工具需已填 registry）；改走 Phase 6 新增路径直接填充真正跨系统事实 -->
> 以下为重启时的原始中断记录与已确认发现，保留备查。

### ⏸ A（`/review-all-gdds`）中断记录 + 重启指引（2026-06-24）
> 用户选择做 A（验证新文档），跑到一半被中断，**未出最终报告/未定 verdict**。新会话**重启 A**：重跑 `/review-all-gdds`，参数聚焦 014/015/016 + 改动的 game-concept/gdd-010，对照 GDD_001-013。下列**已确认发现**可直接带入加速，不必重新发现。

**审查已完成的步骤**：Phase 1（载入范围文档）+ Phase 1b（读 registry）。**未做**：Phase 2 完整一致性、Phase 3 设计理论、Phase 4 场景走查、Phase 5 报告、Phase 6 落盘、Phase 7 handoff。

**已确认发现（带入新会话）**：
1. **Registry 为空**：`design/registry/entities.yaml` 存在但 entities/items/formulas/constants 全为 `[]`。一致性只能靠全文读。**建议审查后跑 `/consistency-check` 填充**（尤其新引入的 功绩/名望/城池归属/StrategicAction 等跨系统名）。
2. **依赖单向（Warning）**：GDD_014/015/016 依赖 004/005/006/007/010/011/012/013，但**现有 001-013 无一把 014/015/016 列为 dependents**（新文档必然如此）。修：在相关旧 GDD 的 Dependencies 加反向引用；并把新跨系统名登记进 registry。
3. **城池归属权威边界（Warning，需裁定）**：GDD_004 拥有「控制权」（"失城改变控制权"、"控制权变更事件"——见 gdd-004 L27/138/147）；GDD_015 写 `city.owner ← outcome.owner_change`。**两者都在动城池归属，须明确权威**。建议：GDD_004 拥有城级控制权 + 变更事件；GDD_015 世界模型在战略尺度**反映**归属、**只经 GDD_004 的控制权变更事件**写入，不独立改。**（裁定权在用户，审查只标选项）**
4. **反支柱「最优玩法只需刷战斗」（Design Warning）**：game-pillars 反支柱之一。GDD_014 的 功绩/名望 须保住**非战斗来源**（治理/招揽/外交/平叛）有意义，否则生涯退化为刷战斗。现状 014 已含非战斗来源——**作为护栏保留并在审查中确认权重**。
5. **AI 随机与 ADR-0004 的契合（INFO）**：ADR-0006 用 `seed=Hash(worldTick,factionId,planId)`；须确认它**消费 ADR-0004 的注入式确定性流、在声明的 AI 决策检查点取值**，而非旁路随机源。建议在 ADR-0006 补一句明确。
6. **设计锁 PASS**：014/015/016 **未**引入任何「无条件计策按钮」，GDD_010 设计锁完好；016 维持条件涌现（效用评分→基础命令，非按钮）。
7. **自洽 PASS**：015「玩家触及边界」与 016 战略层（够得着才跑 AI、够不着抽象推进）、014 自立线三者一致。

**重启 A 的下一步动作**：重跑 `/review-all-gdds` → 走完 Phase 2-4 → 出报告（verdict 预期为 **CONCERNS**：无 Blocking，上述 2/3/4 为 Warning）→ 经批准写 `design/gdd/gdd-cross-review-2026-06-24.md` → 按 handoff 决定是否把 ADR-0006 转 Accepted / 进实现。

<!-- QA-PLAN: 2026-06-24 | System: sprint-02 | Plan written: production/qa/qa-plan-sprint-02-2026-06-24.md -->

## Session Extract — /sprint-plan new 2026-06-24（Sprint 02）
- 新建 `production/sprints/sprint-02.md` + `production/sprint-status.yaml`（首个 yaml；2026-06-24~07-07）
- Goal：epic-011 生涯 + epic-012 世界模型的 Domain 内核与标志性机制
- Must(5)：11-1 CareerState骨架 · 12-1 WorldState骨架 · 11-2 晋升 · 11-3 自立 · 12-2 事件触发门（~20h）
- Should(3)：11-4 太守开局守城 · 12-3 分叉传播 · 11-5 生涯存档
- Nice(3)：12-4 归属投影(需epic-004接口) · 12-5 抽象结算 · 12-6 世界存档
- lean → PR-SPRINT 跳过。**QA plan 缺**（qa-plan-sprint-02.md 未建）——Production→Polish 门需要，建议 /qa-plan sprint
- sprint-01（Foundation）随竖切已 Complete，无 carryover
- **下一步**：/qa-plan sprint（实现前）→ /story-readiness 11-1 → /dev-story。本批 sprint 文件未 commit。

## Session Extract — /create-epics + /create-stories 2026-06-24（014/015 Meta 层进实现管线）
- 014/015 → **Locked for Slice**（gdd-index + 各文件状态行；016 仍 Reviewed）
- 新建 2 epic：**epic-011-campaign-career**（生涯，5 story：3 Logic+2 Integration）+ **epic-012-historical-world-model**（世界模型，6 story：4 Logic+2 Integration）
- 全部 11 story 嵌 TR-ID + governing ADR（全 Accepted，0 Blocked）+ 控制清单规则 + inline QA 用例（lean，未派 qa-lead）
- epic-011 story：001 CareerState 骨架 · 002 忠臣晋升 · 003 自立三分支 · 004 太守开局+守城后果(跨epic软依赖 CitySeed/epic-012) · 005 生涯存档
- epic-012 story：001 WorldState 骨架 · 002 事件四元组+reachability门+配置校验 · 003 分叉传播 · 004 归属订阅004 · 005 抽象结算器 · 006 世界存档
- 跨 epic 注意：epic-011 story-004 用最小 CitySeed 配置占位（待 epic-012）；epic-012 story-004 需确认 epic-004 的 CityControlChanged 接口落地
- EPIC.md + epics index 已更新（11/12 epic 有 stories；016 epic 未建）
- **下一步**：/sprint-plan 排期 或 /story-readiness→/dev-story 从 epic-011 story-001 起实现。本批未 commit。

## Session Extract — /architecture-review 2026-06-24（验证重跑 coverage）
- Verdict: **CONCERNS → PASS**（014/015 Meta 层范围）
- 4 缺口 TR（world-002/004、career-004、world-003）全部闭合（grep 确认 ADR-0007/0008 GDD Requirements Addressed 实际命名）
- Cross-ADR 无依赖环（0007/0008 仅 Related/Ordering 互引，未互列 Depends On）；依赖链全 Accepted；W1 状态所有权冲突由 ADR-0008 消解
- 014/015/016 全覆盖；遗留 3 历史 cosmetic partial（map-001/council-002/supply-001，非阻断）
- 报告更新：docs/architecture/architecture-review-2026-06-24.md（加「验证重跑」节 + 顶部裁定上调）
- 014/015 仍为 Reviewed（未推进 Locked for Slice，留用户定）；本次仅报告 + active.md 改动，未 commit

## Session Extract — /architecture-review 2026-06-24（聚焦 014/015 Meta 层）
- Verdict: CONCERNS（3 缺口全 Meta 层，不阻断竖切 MVP）
- Requirements: 11 total — 7 covered, 0 partial, 3 gaps
- New TR-IDs registered: TR-career-001~005 + TR-world-001~006（tr-registry version→3）
- 缺口 → 已补 ADR：缺口1 条件历史触发模型 → **ADR-0007 Accepted**；缺口2 城池控制权契约（W1 裁定固化）→ **ADR-0008 Accepted**
- GDD revision flags: None；引擎专家咨询跳过（014/015 纯 Domain 无引擎面）
- Report: docs/architecture/architecture-review-2026-06-24.md
- ADR 日志：technical-preferences 已加 0007/0008（现共 8 份全 Accepted）
- **下一步**：014/015 可 Reviewed→Locked for Slice（缺口已补）；可重跑 /architecture-review 验证全覆盖；或进 016 §MVP 敌方 AI 切片。本批未 commit。

## Session Extract — /review-all-gdds 2026-06-24（重启 A 完成）
- Verdict: CONCERNS
- GDDs reviewed: 5 焦点（014/015/016 + game-concept + 010）对照 001-013 + pillars + systems-index
- Flagged for revision: gdd-004, gdd-010, gdd-014, gdd-015, gdd-016（均 Warning，无 Blocking）
- Blocking issues: None
- Key Warnings: W1 城池归属（已裁定 004 唯一权威/015 订阅/014 只读）· W2 反向依赖缺失 · W3 TacticRecognizer 未命名 · W4 追击决策边界 · W5 非战斗功绩源速率
- Recommended next: 落 W1-W5 轻量 Edit → /consistency-check 填 registry → ADR-0006 转 Accepted（确认 N-#5）
- Report: design/gdd/gdd-cross-review-2026-06-24.md

---

## ▶ 休息后接续（2026-06-24）— 新会话从这里读起

> 用户表示"有点混淆，先休息整理思路再继续"。本段是恢复入口，读完即可接续。

### 当前状态（事实）
- **HEAD = tk/main = `a1b10a3`**，工作树干净。**dotnet 451/451 全绿**（-warnaserror 0）。
- 本轮提交链（新→旧）：`a1b10a3` GDD_008边界 · `b22d0d6` 假退伏击第三胜路 · `ff4f89a` 侦察/袭扰改延迟 · `4ffaaa9` HUD双列布局修复 · `cf58dd5` MVP验收测试 · `bc4f8fb` 断粮第二胜路 · `4aadbc8` B7花名册 · `c02ffd6` B1军议 · `4d4cb66` B6外交 · `542982a` A一局闭环 · `1ec3759` 回顾+路线图。

### 一个关键澄清（用户问"竖切内容后续正式场景会用吗"）——已厘清
- **现在做的"竖切" ≠ 早先 throwaway 原型**（`prototypes/...` 那个铁律永不 import）。**现在这个是 production 代码**（src/Domain·Application·Presentation + 真 Unity 工程，复用 9 epic Domain），是"正式游戏的垂直切片"=第一战/教学战雏形。
- **会保留进正式版**：四层架构 + 全部代码；场景**内容**（汜水关/旋门关/敖仓地名、四人物、三条链）——故用原著地名、守原创红线。
- **会重构（内容迁移、承载方式换）**：`SliceScenario` 硬编码工厂 → 按 ADR-0003 改数据驱动配置（ScriptableObject→不可变配置）；占位数值 → 待平衡；占位 UXML/USS → 正式美术。

### 本轮与用户对齐的设计决定（重要，未来都要守）
1. **策略默认全开，只由硬条件（天气/地形/兵种/资源）禁用**；禁用须给 in-world 理由。我之前给假退伏击加的"一局一次"是**人为限制，应改为 diegetic**（如诈败被识破→敌将警觉再诱难）——**待办，尚未改**。
2. **袭击/伏击不能只点按钮**（用户最看重）：要"派谁 + 投多少 + 借势(地形/敌将性格) + 时机"的真实决策，投入不足则徒劳。当前是单按钮（demo 欠做，非设定）。复用 epic-006 准备 + epic-007 战役，但**竖切只做满足 mvp-scope §条件链验收 的最小版**，不做全套准备子系统（那是全局 B2/B3）——**待办，尚未做**。
3. **军师边界（已写入 GDD_008，commit a1b10a3）**：建议以「缘由（含原著地名 + 敌将性格等前提事实 + 依据情报 + 缺失情报）→ 条件 → 风险 → 免责」呈现；**可给**地点/敌将性格/条件/风险/定性置信；**不可给**派谁·多少·何时的完整组合、胜率数字；"地点已知≠此战已解"（可用性仍须侦察证实）；军师能力体现为完整度/准确度，不显胜率。
   - 与用户达成的折中："越线版+守线版综合"——军师可点名"附近某隘口适于伏击"（缘由），但不替玩家组完整计划。

### 地名落点（我已查三国演义地理 + 推荐，用户已认可这组）
- **假退伏击 → 旋门关**（成皋与汜水关之间窄关，敌东来必经；备选 广武涧）
- **断粮疲敌 → 敖仓**（黄河南岸著名粮仓 + 其往前线粮道）
- **守城待变 → 汜水关**（即虎牢关一带，切片现名正确；后备纵深 成皋）
- 火攻/水攻/暗度陈仓：地名已备（汜水谷·荥泽/鸿沟·小平津渡/轘辕关），但属 mvp-scope 之外，**切片不做**，记全局扩展。
- 注：地名为《三国演义》/汉末实有地理（非杜撰），"何处宜某计"含我的推断，用户可改。

### ▶ 下一步（休息后三选一，待用户定）
1. **把"汜水关守御战"场景落成正式设计文档**（建议 `design/levels/` 或 `design/gdd/`）：地点落点+四人物+三条链+当前数值 → 成权威来源，后续数据驱动配置照此生成（我已建议，用户未答）。
2. **做决定 1（去人为限制）+ 决定 2（袭击/伏击最小版真实决策）+ 把地名接进军师"缘由"**——即把本轮对齐的设计真正落到切片代码。
3. **用户继续 Play 实测**本轮新增（三条路线 + 延迟动作 + 双列布局都未经用户 Play），攒反馈再统一推进。

### 验证命令（恢复后自检）
- 测试：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`（451/451）。
- 改 src 后重建桥 DLL：`dotnet build src/Presentation/ThreeKingdom.Presentation.csproj -c Release` → 复制 Domain/Application/Presentation 三 DLL 到 `Assets/Plugins/`。
- Unity 校验：关闭 Editor 后 batchmode `-quit`，看无 error CS。

---

<!-- STATUS -->
Epic: Sprint 02 — epic-011 战役与生涯 + epic-012 条件历史世界模型（Meta 层 Domain 内核）
Feature: 11 story 已就绪（7 Logic + 4 Integration），QA plan 齐备；进 /dev-story 实现
Task: HEAD=fddf4c3，全部 push tk/main，工作树干净。Sprint 02 已排 + sprint-status.yaml + QA plan。11-1 已 /story-readiness=READY（仅 estimate 占位琐碎项）。★用户在新会话续接，下一步：把估算回写 11 个 story header，或直接 /dev-story 实现 11-1（CareerState 骨架，DAG 根）★。详见顶部「▶▶ 新会话从这里读起（2026-06-25）」
<!-- /STATUS -->

## ▶ Pre-Production→Production 闸门补完（2026-06-21 续）

首跑 /gate-check pre-production（Pre-Production→Production）裁定 **FAIL**：slice 已 PROCEED（乐趣已验证），但缺 art-bible 完整+签核、三关键屏 UX、epics→stories。用户拍板按 1-2-3-4 顺序补完再回报。
- **✅ Step 1** — art-bible 升 v1.0：补 §5 动效规范 / §6 音频视觉协同 / §7 无障碍视觉标准 / §9 制作管线+**AD-ART-BIBLE 签核 APPROVED**（2026-06-21）。`design/art/art-bible.md`。
- **✅ Step 2** — 三关键屏 UX 写就并 /ux-review **全 APPROVED**（Status=Approved）：`design/ux/main-menu.md`、`hud.md`、`pause-menu.md`。核心设计锁（P10 无真值/P11 无最优解/P6 不合并/P4 双态）全落入验收；无障碍对齐 WCAG 2.1 AA（Comprehensive）。
- **✅ Step 3** — 9 epics（3 Foundation: epic-001/002/009；6 Core: epic-003~008）+ **28 stories**（12 Foundation + 16 Core，17 Logic/11 Integration，0 Blocked）写入 `production/epics/*/`，含 `index.md` 总账。每 story 嵌 TR-ID + governing ADR + AC + QA Test Cases + 测试证据路径。lean 模式跳过 PR-EPIC/QL-STORY-READY 子代理门。
- **▶ Step 4 — 闸门重判：artifact+quality 全绿 → PASS-eligible**。首跑三项硬阻断全闭合（art-bible 完整+签核 / 三屏 UX APPROVED / epics→stories）。剩余皆 CONCERNS 非阻断：①entity-inventory 缺（art-bible §9.2 列为 Production 早期，非硬前置）②主架构/可追溯命名 architecture/-traceability（实质在，待改名 architecture.md/requirements-traceability.md）③playtest 报告在 prototypes/REPORT.md 非 production/playtests/ ④sprint-01.md 仍引旧 STORY_NNN id，待 /sprint-plan 刷新指向新 story 文件。
  - 进 Production 硬前置仍在（首个 Logic story Done 前）：示例测试 .cs 跑通 + CI 至少一次绿。**TD/PR 建议**：Logic story 可用纯 `dotnet test`（NUnit）旁路 Unity 许可跑 CI 绿，UNITY_LICENSE 仅 EditMode 集成测试需要 → 把「dotnet test CI 绿」设为 epic-001 S1 硬验收。

## ▶ 四总监 Panel 裁定（Pre-Production→Production，lean，2026-06-21）

- **CD=CONCERNS / TD=CONCERNS / PR=CONCERNS / AD=READY** → 升级规则取最严 → **整体闸门裁定：CONCERNS**（首跑三硬阻断全闭合；AD 由 NOT READY 升 READY；无 NOT READY 残留）。
- **用户决定（更新）：先做 fix-forward ①②③ 再推进**。三项已完成 → **stage.txt = Production**（2026-06-21）。

## ✅ Fix-forward ①②③ 完成 + 进入 Production（2026-06-21）

**① 低成本 CONCERNS 消化**
- 文档改名为闸门规范名：`architecture-overview.md`→`architecture.md`、`architecture-traceability.md`→`requirements-traceability.md`；全仓引用同步更新（含 .claude/skills adopt/propagate-design-change/quick-start，消除模板命名冲突）。
- `design/accessibility-requirements.md` 跑 /ux-review → **Approved**（与 art-bible §7 + interaction-patterns + 三屏 UX §12 双向一致）。
- `production/sprints/sprint-01.md` 重生：指向真实 story 文件路径（Foundation 5 story，依赖序）+ 容量基线（slice velocity，标定 sprint）+ CI dotnet test 旁路许可为 S1 硬验收。

**② 进 Production 硬前置（CI + 示例测试）**
- 建纯 C# Domain 工程：`src/Domain/ThreeKingdom.Domain.csproj`（netstandard2.1，Unity 兼容，禁 UnityEngine）+ `BuildInfo.cs`（框架确认占位）；测试 `tests/unit/ThreeKingdom.Domain.Tests/`（net10.0 + NUnit）+ `BuildInfoTests.cs`；`ThreeKingdom.slnx`。
- **`dotnet test` 本地绿：2/2 passed**（restore 成功，netstandard2.1 ↔ net10.0 引用通）。
- CI `tests.yml` 重写：`domain-tests` job（dotnet，无许可，gating 绿）+ `check-unity-license`/`unity-tests`（license-guarded，未配则跳过不报红）。**首次 GitHub 绿待 push 验证**（workflow 已结构性无许可依赖）。

**③ 阶段推进**
- `production/stage.txt` = **Production**；control-manifest 当前阶段→PRODUCTION、G6=Passed(CONCERNS)、G7=Unblocked(Foundation)、当前阻断项=无（CONCERNS 列为 guardrail）。

**已入库**：上述全部改动已 commit（`de86317`）+ push 至 `tk/main`（nerrazzuri/three_kingdom，`f9158cf..de86317`）。

## Session Extract — /dev-story 2026-06-21（epic-001 S1 实现）

- **Story**: `production/epics/epic-001-domain-foundation/story-001-domain-test-boundary.md` — 建立纯 C# Domain 与测试边界（Status: In Progress）
- **/story-readiness 裁定**: READY（lean，QL-STORY-READY 跳过）
- **实现方式**: inline（小型反射断言测试，省冷启动 engine-programmer/unity-specialist 子代理；框架骨架闸门②已建）
- **文件**: 新增 `tests/unit/ThreeKingdom.Domain.Tests/DomainBoundaryTests.cs`（3 测）；既有 `src/Domain/{ThreeKingdom.Domain.csproj,BuildInfo.cs}` + `BuildInfoTests.cs`（2 测）+ `ThreeKingdom.slnx`
- **AC**: 全 4 条满足（Domain 无 UnityEngine 引用[反射实证]/独立 NUnit 测试程序集/dotnet test 无 Unity 运行时/示例类型+测试）
- **测试**: `dotnet test ThreeKingdom.slnx` → **5/5 绿**（本地）
- **偏差**: 测试证据路径由 `tests/integration/foundation/...` 调整为统一测试工程 `ThreeKingdom.Domain.Tests`（CI 装配更简）；实现 inline 非子代理（已记）
- **Blockers**: None
- **Next**: `/code-review` 新增文件 → `/story-done` 关闭 S1

## Session Extract — 设计锁红线 + /code-review + /story-done 2026-06-21

- **两条红线正式写入**（约束全部后续产出）：
  - control-manifest §强制设计锁 + art-bible §9.4：①【红线】零复制现有三国游戏/作品资产（原创性，含 AI 仿制/二改/同 IP 全禁；史料+原著可作题材、表达须自创；存疑不入库、违者拒收不 commit；`/asset-spec` 须填「参考来源」字段）②所有文件中文撰写（「中文叙述 + 英文标识符」，代码命名/引擎 API/编号保留英文）。
- **/code-review（epic-001 S1 文件）**: APPROVED（inline；ADR-0002 COMPLIANT、Standards 6/6、新红线合规）。
- **/story-done（epic-001 S1）**: **裁定 COMPLETE WITH NOTES**。Status→Complete；AC 4/4 COVERED；`dotnet test` 5/5 绿；ADVISORY 2 项（测试路径统一到 ThreeKingdom.Domain.Tests / inline 实现）。EPIC.md S1→✅Complete。lean：QL-TEST-COVERAGE 跳过、LP-CODE-REVIEW 由本会话 /code-review APPROVED 充当。
- **DomainBoundaryTests 设为永久回归门**（后续 Domain 程序集禁 UnityEngine）。
- **Next**: commit+push 本批 → 解锁 epic-001 S2（定点/随机流，Depends on S1=Done）。`/story-readiness epic-001 S2` → `/dev-story`。
- **CONCERNS 汇总（均非阻断，Production 早期 guardrail）**：
  - **CD-C1** 非战斗决策空间（人物/关系/城市）仅经设计未经体验验证 → 首个非切片里程碑须体验验证「有意义机会成本」，防漂移成「薄皮战斗沙盒」。
  - **CD-C2** 降输入摩擦 vs P11 无自动化捷径张力，尤其军师层 → 所有降摩擦设计须过 P11 审查；「军师不越界」列专门验收。
  - **CD-C3 / TD** 核心幻想在 Unity 表现层未实证 + 无 player-journey → Production 早期 UI 垂直切片重验幻想；UI Toolkit + 存档平台适配打样测帧/draw-call/内存。
  - **TD/PR** CI 从未跑绿（UNITY_LICENSE 未配/无示例测试）→ 设为 Production 第一动作 + Logic story Done 硬前置；可 dotnet test 旁路许可。
  - **PR** 无里程碑日历/容量基线 → sprint 1 内回填（仅 1 个 slice velocity 数据点）。
  - **PR** sprint-01.md 引旧 story ID → fix-forward 跑 `/sprint-plan new` 重生指向新路径。
  - **PR** epic-009 S3（存档校验）实质依赖 epic-005 S1（情报四层）→ 排程归 Core 期，勿当 Foundation 早期任务（唯一跨层回指，非阻断）。
  - **AD-C1** §4 对比度估算须工具实测（朱批朱红 4.7 / 蜀汉赤金朱 4.6 余量薄）→ 首批资产入库前。
  - **AD-C2** hud.md 引用「art-bible 高对比变体」但 §7 未定义该变体配色 → 高对比功能实现前补 §7。
  - **AD-C3** accessibility-requirements.md 仍 In Review，与 art-bible §7 已对齐 → 跑 /ux-review 更新状态。
  - 次要：ADR 可追溯性列名收尾（TR-supply-001/council-002/map-001 + ADR-0005→0003 Depends On）；架构文档改名 architecture.md/requirements-traceability.md。
- **子代理 ID**（可 SendMessage 续问）：CD=ac814ade4353a5e0e / TD=a1fd5c9cb45a9006d / PR=a0e88328a77c9ff07 / AD=a7e1c42b8c75c452c

## ⏹ 本会话结束（2026-06-21）

- **已完成**：G3 跨系统审查闭环 → Vertical Slice「汜水小城守御」全程（三条兵法条件链 + 军师 GDD_008 + 双边断粮博弈 + 敌军援军 + 情报雾 GDD_007 + 存档 round-trip ADR-0005）。**33/33 测试绿**。裁定 **PROCEED（设计者亲手试玩签核）**。
- **已入库远程**：`tk/main`（nerrazzuri/three_kingdom）至 `f20da44`。工作树干净。
- **未做（有意推迟，省 token，留待干净上下文）**：`/gate-check pre-production`（opus 档，多文档综合裁定）。
- **下次入口**：新会话直接说「跑 /gate-check pre-production」。通过后再 `/create-epics layer:foundation`/`layer:core` → `/sprint-plan`（用 REPORT velocity）。
- **关键参照**：slice 代码 `prototypes/three-kingdom-siege-vertical-slice/`（运行 `cd src && dotnet run` 演示 / `dotnet run play` 交互 / `cd tests && dotnet test`）；裁定与 Lessons 见同目录 REPORT.md。铁律：production 从头重写、永不 import prototypes。

## ▶ VERTICAL SLICE 检查点（2026-06-21 启动）

- **概念名**：three-kingdom-siege（汜水小城守御战）
- **验证问题**：新玩家从守城开局出发，无引导体验到「我赢因我创造了条件，而非点了技能」，且完整循环能在 2 周内以接近量产品质实现于纯 C# 四层架构？
- **构建形态**：headless C# 控制台（.NET 10），驱动真实 Domain；文本 UI。自检 `dotnet run` / `dotnet test`。
- **范围内系统**：GDD_001/002/003/004/005/006/007+008/009/010/011/012（外交受控入口 §8）
- **3 条破局链**：假退伏击 / 断粮疲敌 / 守城待变（玩家选其一或组合）
- **品质**：占位符文本 UI，无美术资产；数值与因果可读性优先
- **目录**：`prototypes/three-kingdom-siege-vertical-slice/`
- **铁律**：production 从头重写、永不 import prototypes；每文件带 slice 头注释
- **时限**：2 周；Day 3 检查点（完整循环不可演示则停）
- **当前阶段**：Phase 4 — Implement（搭骨架 → 模块 0/1）
- **Velocity Log**：
  - Day 1（2026-06-21）：✅ 项目骨架（.NET 10 控制台 + NUnit 测试项目）；模块 0（Fixed Q16.16 / DetRng 确定性流）；模块 1（Time）；Config（数据驱动）；Forces（断粮唯一施加点 + 溃逃）；BattleResolver（≤5 决定性因素、确定性）；SiegeState 聚合 + Application Service（Command 路径）；SiegeScenario 工厂。**断粮疲敌链端到端跑通**：对照组正面硬守战力比 1.43→城破；实验组断粮疲敌 0.59→击退守城成功。**13/13 测试绿**（Fixed/DetRng 确定性/断粮单点/时间/翻盘不变量/状态哈希复现/命令校验）。`dotnet run` + `dotnet test` 均自检通过。
  - Day 2（2026-06-21 同会话续）：✅ 链 2 假退伏击（Commander 性格 + AmbushResolver 三分支：弄假成真/敌不追/伏击成立；突然性压制守方 ×0.55 + 伏兵 ×1.6）；链 3 守城待变（Weather 确定性天气 + DiplomaticPledge/DiplomacyEvaluator 外交受控入口 GDD_012 §8：grant_score 判定/延迟交付/可背约/代价；援军→兵力、补给→后勤、时限→压力）；**交互式 harness**（InteractiveSession：状态面板 + 命令菜单 + 因果流 + 胜负判定，`dotnet run play`）。三条链脚本演示 + 交互模式均跑通。**22/22 测试绿**（新增伏击三分支、外交 grant/拒绝/延迟/必结算/援军击退）。
  - 三链结果：对照组正面硬守 1.43→城破；链1 断粮 0.59→击退；链2 伏击 1.42→追击支队重创(敌1200→826)；链3 守城待援 0.74→击退。
  - Day 2 续（用户反馈）：✅ **军师建议层（GDD_008）** Advisor — 观察 + 候选路线(所需/风险/操作) + 缺失情报 + 置信 + 「不替你定计/不保证」免责；harness 前期 3 回合自动显示（新手上手），之后 `?` 随时调出。守住设计锁：建议不排优劣、不选最优、不暴露真值。22/22 测试仍绿。
  - Day 2 收尾：✅ **存档 round-trip**（SiegeState.Capture/Restore memento 含 RNG 状态 + Infrastructure SiegeSaveSerializer 版本化信封/原子写/迁移链占位，ADR-0005）；4 项存档测试（哈希保持/读档后续推进确定性/在途外援存活/未来版本拒绝）。**26/26 测试绿**。✅ **REPORT.md 写就 — 裁定 PROCEED**（三条链体验成立 + 架构可行性验证 + 速度远超预算；限定：未验证 Unity 表现层/序列化适配）。prototypes/index.md 建立。
  - Day 2 playtest 迭代（用户反馈「断粮为何单边必胜？敌军也该有补给/援军，且须靠侦察判断」）：✅ **断粮改双边博弈**（RaidStrengthPerUnit vs EnemyEscortStrength 拉锯 + ApplyResupplyPush 敌补给车队回补；投入不足则徒劳）；✅ **敌军自身援军**（EnemyReinforceSegment=14 定时抵达，消耗赛时间压力）；✅ **情报雾 EnemyIntel（GDD_007）**——玩家只见估计值/置信/时效，新增侦察命令(7)刷新、随时间衰减；军师改读知识投影；面板敌军行改显探报。memento 扩展含 intel/scout-rng/reinforced。**33/33 测试绿**（新增双边博弈/敌援军/情报雾 7 测）。REPORT 更新含此关键设计修正。
  - **设计者试玩签核（2026-06-21）**：用户亲手 `dotnet run play` 后确认「不确定性会给玩家更好的一线体验」→ PROCEED 由「实现者自测」升级为「设计者验证」。REPORT playtest 段已记。
- **★ Vertical Slice 完成（lean 模式，CD-PLAYTEST 跳过）。裁定 PROCEED（设计者签核）。**
- **下一步（Phase 8 PROCEED 路径）**：`/gate-check pre-production` 正式推进 Production → `/create-epics layer:foundation` / `layer:core` → `/sprint-plan`（用 REPORT velocity）。注意 G6 门禁，control-manifest 待 slice 完成回填。
- **未提交 git**：本 slice 全部改动待用户指示 commit/push。
- **关键结构**：`src/Domain/{Numerics,Time,Config,Forces,Battle,Siege}` + `src/Application` + `src/Program.cs` + `tests/`
- **构建命令**：`cd prototypes/three-kingdom-siege-vertical-slice/src && dotnet run`；测试 `cd ../tests && dotnet test`

---


## 当前任务

★ 阶段已推进：**Pre-Production**（production/stage.txt 已更新）★
`/gate-check pre-production` 重跑（2026-06-21）→ **裁定 CONCERNS**（首跑 FAIL，补 art-bible 后升级），用户接受 CONCERNS 推进。

**当前正在做：G3 跨系统审查（/review-all-gdds）** — vertical slice 已暂停，待 G3 完成后恢复。
理由：control-manifest（v1）明文「下一门禁为 G3」，且 G3 真实未跑（无 cross-GDD review 报告）；
vertical slice 属 G6，不应抢在 G3 前搭在未对账的跨系统契约上（PR 总监建议一致）。用户 2026-06-21 拍板先跑 G3。

**Pre-Production 推荐顺序（修正）**：
1. ★进行中★ `/review-all-gdds`（G3 跨系统矛盾审查）→ 回写阻断项 → 刷新 control-manifest 门禁状态。
2. `/vertical-slice` — 恢复，前置验证乐趣，再写大量 story（勿先连跑两个地基 sprint）。
3. 首段 Domain 代码（EPIC_001，如 ADR-0004 FixedPoint/状态哈希）落地时**同步补示例测试 + 配 UNITY_LICENSE 跑通 CI**。

**进入 Production 的硬前置（务必在首个 Logic story 标 Done 前关闭）**：
- 示例测试文件（.cs）跑通 + CI 至少一次绿灯（需先配 `UNITY_LICENSE` secret，许可激活有外部延迟风险，宜并行处理）。

**Pre-Production 内管理项（CD/TD/PR 的 CONCERNS，非阻断）**：
- CD：支柱4「外交/天下局势」无 GDD 归属 → story 拆分前指派单一受控外交接口（落点 GDD_003 或 GDD_009）；010/008/007 补 CD-GDD-ALIGN 签核。
- PR：无时间线/容量基线 → 出粗里程碑日历 + EPIC_001 容量探针标定 solo 速度；control-manifest 门禁状态刷新。
- 次要清理项：主架构文档命名 architecture.md（闸门期望 architecture.md）；可追溯索引命名 requirements-traceability.md（期望 requirements-traceability.md）。实质存在、TD 已审稳健，可日后改名。

注意：用户偏好——公式/草稿提炼完直接写入，不再逐份询问是否可写入。

## 项目背景

- 项目：《三国演义：兵法沙盒》— 离线单机三国沙盒战略 RPG
- 引擎：Unity 6.3 LTS + C#（ADR-0001 锁定，已配置）
- 阶段：Technical Setup（`production/stage.txt`）
- 原始文档来源：`D:\Projects\三国演义\docs`（外部，已复制迁移，原件保留作备份）

## 采纳计划进度

### ✅ 已完成
- **Step 1a** — 全部外部文档迁移至模板路径（design/gdd/、design/concept/、docs/architecture/、production/）
- **Step 1b** — 引擎配置：CLAUDE.md + technical-preferences.md 写入 Unity 6.3 LTS + C#，@import 改为 unity
- **Step 1c** — ADR-0001 添加 `## Status` = Accepted
- **Step 2a** — 全部 13 份 GDD 移除节标题编号前缀（`## 1. System Purpose` → `## System Purpose`）
- **Step 2c** — ADR-0001 添加 `## Engine Compatibility`（HIGH 风险标注）+ `## ADR Dependencies`
- **Step 4c** — ADR-0001 添加 `## GDD Requirements Addressed`

### ✅ Step 2b — GDD Formulas 节（13/13 全部完成）
全部 13 份 GDD 已写入 `## Formulas` 节，位置在 Main Rules 之后、Data Model 之前。
已用脚本验证 13 份均含 `## Formulas` 标题。每份均遵循 design-docs.md 强制要求
（变量定义表 + 编号公式 + 约束 + 数值示例），并贯彻设计锁（无条件计策按钮禁止、
确定性可复盘、资源守恒、三维状态不合并、不完全信息四层分离）。

### ✅ 已完成（续）
- **Step 3b** — control-manifest.md 添加 `Manifest Version: 1`
- **Step 4b** — 全部 13 份 GDD 添加 `**Status**: Draft`（保留中文行双语并存）
- **Step 4e** — systems-index.md 无括号状态值（概念地图）；附带修复 gdd-index.md 13 个断链

### ⏳ 待办（计划剩余项，均为技能驱动或设计驱动）
- **Step 2d / 3a** — 运行 `/architecture-review` 初始化 tr-registry.yaml（重头戏，读全部 GDD+ADR）
- **Step 3c** — 运行 `/sprint-plan update` 创建 sprint-status.yaml
- **Step 3d** — 运行 `/gate-check Technical Setup`（验证能否进入 Pre-Production）
- **Step 4a** — GDD 节名调整：用户已选"最小改动"保留英文节名（System Purpose 等），
  故**不**重命名；接受部分技能可能用内容匹配而非精确标题匹配的折中。可日后按需调整。
- **Step 4d** — 创建 ADR-0002~0005：
  - [x] ADR-0002 架构分层 — Accepted（注册表 4 项 stance）
  - [x] ADR-0004 确定性战斗模拟 — Accepted（注册表 3 项 stance：定点策略/状态哈希/float 禁止）
  - [x] ADR-0005 存档版本与迁移 — Accepted（注册表 3 项 stance：save_format/save_repository/Unity 序列化禁止）
  - [x] ADR-0003 数据驱动配置 — Accepted（注册表 2 项 stance：config_pipeline/config_loader）
  ★ 4 份必需 ADR 全部完成（含 ADR-0001 共 5 份 Accepted）★

### 架构审查后的覆盖率变化（供下次 /architecture-review 参考）
ADR-0002 + ADR-0004 已覆盖原 17 缺口中的约 19 条 TR（含原部分覆盖项的正式锁定）。
剩余主要缺口指向 ADR-0005（存档序列化：TR-time-003/intel-003/save-003）与
ADR-0003（数据驱动配置的正式锁定）。

### 注册表状态（docs/registry/architecture.yaml，version 5）
- state_ownership: all_authoritative_gameplay_state → domain-layer
- interfaces: player_command_path、battle_state_hash、save_repository、config_loader
- api_decisions: battle_numeric_strategy（整数/定点）、save_format（DTO+JSON）、config_pipeline（SO→不可变）
- forbidden_patterns: domain_depends_on_unity、direct_cross_system_state_write、
  scriptableobject_as_runtime_authority、implicit_global_random、
  float_in_domain_authoritative_path、unity_serialization_of_domain

### 下一步（下次会话）
1. ★开新会话★ 跑 `/architecture-review` 验证覆盖率（5 份 ADR 后应接近全覆盖）——
   不可在本撰写会话内跑，审查 agent 须独立。
2. gate-check 前置仍缺：/test-setup（tests 目录+CI）、/ux-design（无障碍+交互模式）。
3. 之后 /gate-check Technical Setup 验证能否进入 Pre-Production。

## Formulas 节起草方法（保持一致风格）

每份 Formulas 节遵循 `.claude/rules/design-docs.md` 强制要求：
1. 顶部引言：声明数值来自版本化配置、不硬编码、确定性
2. 「变量定义」表格：变量 | 含义 | 范围/单位 | 来源
3. 编号公式块：每个含约束说明 + 具体数值示例计算
4. 回应该 GDD 的 §Test Requirements（尤其确定性/重放要求）
5. 插入位置：`## Main Rules` 之后、`## Data Model` 之前（模板 Detailed Rules → Formulas 排序）

## 下一步

继续 Step 2b：从 gdd-004-city-economy.md 起草 Formulas 节，逐份给用户审阅后写入。
读取该 GDD 全文 → 提炼公式草稿 → 展示 → 批准后 Edit 插入 → 更新本状态文件。

## 恢复指引

新会话开始时：
1. 读本文件
2. 读 `docs/adoption-plan-2026-06-21.md` 查看完整计划与勾选状态
3. 从「进行中：Step 2b」的下一个未勾选 GDD 继续

## Session Extract — /architecture-review 2026-06-21（初版）
- Verdict: CONCERNS
- Requirements: 30 total — 1 covered, 12 partial, 17 gaps
- New TR-IDs registered: 30
- GDD revision flags: None
- Top ADR gaps: ADR-0002 架构分层, ADR-0004 确定性战斗模拟, ADR-0005 存档版本与迁移
- Report: docs/architecture/architecture-review-2026-06-21.md

## Session Extract — /test-setup 2026-06-21
- Verdict: COMPLETE — Unity Test Framework 脚手架 + CI 接通
- 创建: tests/{README,unit,integration,smoke,evidence,EditMode,PlayMode} + .github/workflows/tests.yml
- 待办: 配置 UNITY_LICENSE secret；随首段 Domain 代码补 1 个示例测试（gate 要求）

## Session Extract — accessibility-requirements 2026-06-21
- Verdict: COMPLETE（In Review）— accessibility-specialist 起草，本会话写入
- File: design/accessibility-requirements.md（路径选 design/ 根，匹配 gate 检查；非 design/ux/）
- 基线: WCAG 2.1 AA；MVP 15 项必达 / 应达 6 / Future 7；10 条验收；6 开放问题
- 与模式库交叉引用 P1/P3/P5/P7/P9/P12，无矛盾
- 开放问题重点: OQ-03 须开 Spike 实测 Unity 6.3 UI Toolkit AT API 能力
- Next: /ux-review 验证两份 UX 文档

## Session Extract — /ux-design patterns 2026-06-21
- Verdict: COMPLETE（In Review）— 交互模式库 12 条，从 13 份 GDD UI Requirements 种子化
- File: design/ux/interaction-patterns.md
- 模式: P1-P12（数据显示/反馈模态/HUD/全局约束）；P3/P11/P12 为横切约束
- 缺口记录: 无 player-journey、无 accessibility-requirements.md（建议 accessibility-specialist 补）
- 平台确认: PC（键鼠为主、无触控）— 用户 /btw 确认
- /ux-review interaction-patterns: APPROVED（补 Standard Controls/Animation/Sound 三节后）；Status=Approved
- 全部 4 项 architecture-review pre-gate 缺口已补齐

## Session Extract — /architecture-review 2026-06-21（复审）
- Verdict: CONCERNS（极轻微，0 阻断项）
- Requirements: 31 total — 28 covered, 3 partial, 0 gaps（初版「30」系少计 1 条，已更正）
- New TR-IDs registered: None（31 条已全部登记，注册表无变更）
- GDD revision flags: None
- Cross-ADR conflicts: None（依赖图无环，5 份 ADR 全 Accepted）
- 部分覆盖项（仅需可选可追踪性列名补充）: TR-map-001, TR-council-002, TR-supply-001
- Pre-gate 缺失: /test-setup（tests+CI）、/ux-design（无障碍+交互模式）→ 阻断 /gate-check pre-production
- Report: docs/architecture/architecture-review-2026-06-21.md（已覆盖初版）

## Session Extract — /gate-check pre-production 2026-06-21
- 闸门: Technical Setup → Pre-Production；审查模式 lean（四总监 PHASE-GATE 全跑）
- **裁定: FAIL**（Chain-of-Verification 5 问已核，含 2 项工具复核，裁定不变）
- 必备制品 11/13；硬阻断 = 美术圣经缺失
- Director Panel: CD=CONCERNS / TD=CONCERNS / PR=CONCERNS / **AD=NOT READY**（升级规则→整体最低 FAIL）
  - CD: 支柱4外交无 GDD 归属（影响"守城待变"条件链）；010/008/007 未留 CD-GDD-ALIGN 签核
  - TD: 架构本体 READY；无示例测试+CI 未跑通=进 Production 硬前置（非进 Pre-Production 阻断）
  - PR: 无时间线/容量基线 → solo 范围现实性无法证伪；建议 vertical slice 前置验证乐趣；
        control-manifest(v1) 门禁状态脱节，需澄清 G3 是否补跑 /review-gdds
  - AD: art-bible 缺失；art-direction.md 覆盖 60-70%，缺色彩量化锚/字体排版/资产技术规格（越过最晚责任时刻）
- 用户决定: ①现在运行 /art-bible ②本闸门报告只更新 active.md，不单独存档
- 最小路径转 PASS: /art-bible（Section 1-4）+ 补一示例测试（或显式签字后置）→ 重跑 gate
- 落盘: 仅本 active.md（用户选项）；未写 production/gate-checks/

## Session Extract — /gate-check pre-production（重跑）2026-06-21
- 触发变化: art-bible 已补 → 美术总监三项具名硬阻断（色彩量化锚/字体排版/资产规格）全部解除
- **裁定: CONCERNS（首跑 FAIL → 升级）**；无阻断残留
- Director Panel: CD/TD/PR=CONCERNS（不变，沿用）/ **AD=READY**（art-bible Section 4+8 客观满足）
- 成本审慎: 未重复冷启动 CD/TD/PR 子代理（结论无变化变量）；AD 阻断按文件客观核验解除
- 唯一剩余必备制品缺口: 示例测试文件 — TD 背书后置（进 Production 硬前置，非进 Pre-Production 阻断）
- **用户决定: 推进阶段 → production/stage.txt = Pre-Production**
- Chain-of-Verification: 5 问已核（含 grep art-bible 章节头 / find *.cs 两项工具复核）— 裁定不变 CONCERNS

## Session Extract — 状态盘点 + 全量入库 GitHub 2026-06-21

- **触发**：用户「check latest status … record everything before stop … 所有内容先 git 到 github 上」
- **当前阶段**：Pre-Production（`production/stage.txt`）；审查模式 lean（`production/review-mode.txt`）
- **git 起始状态**：分支 main（与 origin/main 同步、无未推送提交）；5 个已跟踪文件被修改 + 30 项未跟踪 = 35 项
  - 修改：`.claude/docs/technical-preferences.md`、`CLAUDE.md`、`docs/CLAUDE.md`、`docs/architecture/tr-registry.yaml`、`docs/registry/architecture.yaml`
  - 新增（未跟踪重点）：`design/`（gdd/art/concept/ux + accessibility-requirements.md）、`docs/architecture/`（ADR-0001~0005 + index + overview + traceability + control-manifest + review）、`docs/`（project-brief/test-strategy/balance-strategy/ux-accessibility/adoption-plan）、`production/`（epics/sprints/story-backlog/stage/review-mode/project-stage-report/session-state）、`tests/`、`.github/workflows/`、`.claude/agent-memory/`（art-director + lead-programmer）
- **入库结果**：
  - `origin`（Donchitos/Claude-Code-Game-Studios，上游模板）→ **推送被拒 403**，nerrazzuri 无写权限
  - 用户指定目标 `nerrazzuri/three_kingdom`（私有，default=main）→ 新增 remote `tk`
  - 发现 `three_kingdom` 是**原始源文档仓库**（旧结构 docs/01_concept… 共 32 文件，即 D:\三国演义\docs 备份），与本地模板化工作仓**历史无关**
  - 未覆盖其 main；将本地提交以独立分支 `chore/pre-production-content-snapshot-2026-06-21` 推送到 `tk` ✓
  - 提交 SHA：见 `git log -1`；全部 62 文件已入 GitHub
- **已决定并执行**：用户选「提升为新 main」→ force-push `chore/...` → `tk:main`（覆盖旧的原始文档备份）
  - 旧 `three_kingdom/main` SHA = `d5973d1`（原始源文档备份，**可经此 SHA 恢复**，未删除只是不再被 main 引用）
  - 新 `tk/main` = `32b7174`（含 README 重写 + 全量快照）
  - 本地 `main` 已快进至 `32b7174` 并改跟踪 `tk/main`（以后直接 `git push` 走 three_kingdom）
- **README**：已从模板框架介绍重写为《三国演义：兵法沙盒》项目说明（commit 32b7174）
- **备注**：`.gitignore` 未忽略 `production/session-state/active.md`（与 directory-structure.md 注释「gitignored」不符）；本次按用户「记录全部」意图一并入库，作为会话记录留痕
- **下一步（恢复后）**：跑 `/review-all-gdds`（G3 跨系统审查）→ 回写阻断项 → 刷新 control-manifest 门禁状态

## Session Extract — /art-bible 2026-06-21

- **状态: COMPLETE** — design/art/art-bible.md v0.1 写入
- 覆盖范围: Section 1–4（视觉身份基础）+ Section 8（资产标准）
- Section 5–7（动效/音频/无障碍视觉）骨架已占位，待后续阶段补充
- 关键量化锚:
  - 核心视觉规则: 军府案上的活图卷（三层：地貌底/批注中/情绪顶）
  - 三支柱原则: 信息即美术 / 笔迹分层语气 / 历史质感承载演义气势
  - 五种游戏状态（生活观察/判断布局/行动承诺/战争应变/战果延续）逐一定义光照+情绪+视觉元素
  - 色彩系统: 宣纸白 H35S12L90 / 山水墨 H25S8L18 / 朱批朱红 H12S72L40 / 推测蓝灰 H215S18L55
  - WCAG-AA 对比度: 山水墨~11∶1 ✓；朱批朱红~4.7∶1 ✓（须实测）；推测蓝灰~4.1∶1（仅图形元素用）
  - 三势力旗色: 曹魏铁苍蓝 H210S28L32 / 蜀汉赤金朱 H18S58L40 / 东吴碧江青 H188S42L34
  - 色盲安全: 4 组高风险色对 + 纹样/形状冗余方案 + 强制验证流程
  - 资产分辨率层级: 地图底图 2048→4096 / 人物肖像 256×340 @1x / UI 图标 32 @1x
  - 字号体系: T1-T3 / B1-B2 / C1 / N1-N2，1080p→1440p→4K 三档，最小 11px
  - 命名规范: 12 个 category 前缀（map/mapnode/maproute/char/charfx/ui/uiicon/faction/overlay/event/vfx/bg）
  - 内存预算: @1x ~154MB 压缩后；@2x 全启 ~616MB，总占 ~2.9GB（8GB 内无冲突，真实瓶颈=制作成本）
- 4K vs 内存冲突：**无实质冲突**（技术约束不是瓶颈，制作成本才是）；MVP 阶段 @1x 优先
- 下一步: 重跑 /gate-check pre-production（art-bible 硬阻断已解除；示例测试仍待处理）

## Session Extract — /review-all-gdds 2026-06-21（G3 跨系统审查）

- **执行方式**: inline 重做综合（首轮 3 路子代理产出未落盘，恢复会话后主审查全量重读 13 份 GDD 重综合）
- **裁定: FAIL（1 项阻断）** — 支柱4「外交与天下局势」零 GDD 归属，且「守城待变」核心 slice 条件链依赖未规约外交输入
  - 阻断狭窄可廉价闭合：仅需「一个受控外交入口」附录，非新系统；另两条链（假退伏击/断粮疲敌）已完整规约
- **GDDs reviewed**: 13（+ concept/pillars/systems-index）
- **Flagged for revision**: GDD_012(指派外交入口-Blocking)、010/011/012(断粮传导三处重复-W)、003/007(知识TTL重复-W)、004/011(morale变量名碰撞-W)、001/003(mod_weather范围不一致-W)
- **Blocking issues**: 1 — 外交受控入口缺失（C-BLOCK-1 = D-BLOCK-1 = S-BLOCK-1，三面同根）
- **用户决定（2026-06-21）**: ①写独立报告 + 更新 active.md ②外交受控入口**指派进 GDD_012 后勤**
- **正向强项**: 资源守恒(004↔012)全corpus最强一致点；无竞争进程循环；无反支柱违反；玩家幻想高度一致
- **Report**: design/gdd/gdd-cross-review-2026-06-21.md
- **实体注册表为空**: 建议补跑 /consistency-check 填充

## Session Extract — G3 阻断闭合 + 重判 2026-06-21

- **阻断已闭合**: 外交受控入口写入 **GDD_012 §8**（求援/求粮/求时限三选一作用于 slice，外势力静态背景、非完整天下外交）
  - GDD_012 同步补：Main Rules 条目、§8 公式块（响应判定/交付时间/兑现背约/slice作用/代价兑付）、DiplomaticPledge 数据模型、Dependencies(加 006)、Player Inputs、Failure Cases、Balancing、Test Requirements(支柱4 验收)、MVP Scope
  - 设计锁守住：延迟交付 + 条件判定 + 代价 + 可背约失败 + 确定性可重放 + 守恒（绝非即到保证按钮）
  - systems-index §GDD责任分派 加「外交受控入口：GDD_012 §8」
- **G3 重判: FAIL → CONCERNS**（剩余 5 项 Warning 皆非阻断）
- **control-manifest 刷新**: G3 行 → Passed（CONCERNS）；当前阻断项段更新；下一门禁标注为 G6（恢复 /vertical-slice）
- **5 项 Warning 已全部回写修复（2026-06-21）**:
  - S-W1 断粮传导单点: 012持supply_state发事件 / 011唯一施加morale·fatigue / 010只读（GDD_010§8、011§2、012§5）
  - C-W2 知识TTL: 时效权威归007，003只记observed_time复用007降级（GDD_003§6）
  - C-W1 morale命名: 城市民心 civ_morale / 部队士气 unit_morale 消歧（GDD_004§4·§5、011）
  - C-W3 mod_weather: 天气只减速≥1.0对齐003，地形保留0.5–2.0（GDD_001变量表）
  - C-W4 破环/对称消费者: 补005/003消费者 + 新增 systems-index §跨系统结算顺序（含日界顺序，收S-I1）
- **G3 实质问题全部清零**（阻断闭合 + 5 Warning 全修）；control-manifest G3=Passed(CONCERNS)，下一门禁 G6
- **Recommended next**: 恢复 `/vertical-slice` 前置验证乐趣（active.md 既定）；或先补跑 /consistency-check 填充实体注册表

## Session Extract — /story-done 2026-06-21
- Verdict: COMPLETE WITH NOTES
- Story: production/epics/epic-001-domain-foundation/story-002-fixedpoint-rng.md — 定点数值与确定性随机流底座
- Test Evidence: 30/30 passed, 0 warnings（dotnet test ThreeKingdom.slnx）
- Code Review: APPROVED（ADR-0004 COMPLIANT / Standards 6/6 / SOLID / 红线合规）
- Tech debt logged: None
- Next recommended: epic-001 S3 版本化配置加载与校验（ADR-0003）

## ✅ 今日收尾 — 2026-06-22（休息存档）

**本次完成**：epic-001 S2 全链路收口并 push。
- S2 实现 ADR-0004 确定性数值三件套（Domain 权威路径禁 float）：
  - `FixedPoint`（Q16.16，checked 溢出 / decimal 显示 / 向零截断）
  - `IDeterministicRandom` + `DeterministicRandom`（SplitMix64 注入流，position 为权威状态可重建续抽）
  - `StateHasher`（FNV-1a 64 位，显式小端 / 顺序敏感）
- 测试 25 测（FixedPoint 10 / DetRng 8 / StateHasher 6）+ S1 共 **30/30 全绿，0 warning**。
- `/code-review` = **APPROVED**；`/story-done` = **COMPLETE WITH NOTES**（story-002 Status=Complete，EPIC.md S2=✅）。
- **push 成功**：`tk/main` = 本地 `HEAD` = commit **ed1ba8a**（工作区干净，已同步）。
- **顺手修隐患**：手写权威 `.csproj` 此前被 .gitignore 的 Unity 模板规则忽略且从未入库 → slnx 指向未跟踪工程会让干净克隆上 CI 构建失败。已加反忽略例外并纳入 `ThreeKingdom.Domain.csproj` + `ThreeKingdom.Domain.Tests.csproj`（prototype 的 csproj 维持忽略，守铁律）。

### ▶ 明天入口（epic-001 S3）
- **目标**：S3 版本化配置加载与校验（ADR-0003 数据驱动配置；SO 编辑期 → 不可变 Domain 配置 + 配置指纹）。
- **story 文件**：`production/epics/epic-001-domain-foundation/story-003-config-loading.md`
- **建议命令链**（沿用 S1/S2 节奏，lean 模式）：
  `/story-readiness production/epics/epic-001-domain-foundation/story-003-config-loading.md` → `/dev-story` → `/code-review` → `/story-done` → commit+push tk/main
- **注意**：ADR-0003 验收要点 — 非法范围 / 缺失引用被明确拒绝且**无部分写入**；配置指纹确定性。新增 .cs 落 `src/Domain/`，测试落 `tests/unit/ThreeKingdom.Domain.Tests/`（同 S2 结构）。
- **待办（非阻断 guardrail，源自闸门 CONCERNS）**：CI 首次 GitHub 绿仍待验证（push 已多次，可顺带去 Actions 页确认 domain-tests job 跑绿）；entity-inventory、sprint-01 旧 id 刷新等仍挂账。

---

## 🌙 远程自主执行 — 2026-06-22 06:00 起（用户授权离线自动跑）

> 背景：3:05AM 定时器未触发（机器睡眠，ScheduleWakeup 需会话存活）。用户 06:00 回来后改为「现在直接开干」，授权连续跑到 epic-002 完成。

### ✅ epic-001 已完全关闭
- **S3 版本化配置加载与校验**（ADR-0003）：`src/Domain/Configuration/`（ConfigIds/ConfigResult/ConfigSchema/ConfigDraft/ValidatedConfig/ConfigValidator/IConfigLoader）+ 18 测。两阶段校验、错误聚合、**无部分写入**、配置指纹（复用 StateHasher，规范化排序顺序无关）。`/code-review`=APPROVED，`/story-done`=COMPLETE WITH NOTES。commit `cc4605d` → push tk/main。
- **S4 SaveVersion 值对象**（ADR-0005）：`src/Domain/Persistence/SaveVersion.cs`（SaveVersion + SaveCompatibility 三类）+ 26 测。解析/比较/兼容（同主版本可迁移、存档高于当前不兼容不静默降级）、非法版本拒绝、不可变值相等。`/code-review`=APPROVED，`/story-done`=COMPLETE WITH NOTES。commit `ae34330` → push tk/main。
- **全套测试 74/74 全绿，`-warnaserror` 0 warning**。EPIC.md 已标全部 ✅ 并核对 DoD。
- **偏差（ADVISORY）**：两个 story 的测试路径从 `tests/unit/foundation/*.cs` 归一到真实可编译测试工程 `tests/unit/ThreeKingdom.Domain.Tests/`（foundation/ 不在任何 csproj）。

### ✅ epic-002 世界基底 — 全部 5 story 完成（2026-06-22）
- **S1 确定性时间推进**（GDD_001/ADR-0004）：`src/Domain/Time/`（WorldTime+DaySegment、WorldClock+AdvanceTimeCommand、DayBoundaryStage、ScheduledEventOrder）。commit `c595be5`。
- **S2 嵌套战斗时段预算**（ADR-0004+0003）：BattleClock floor(phases/budget) 跨段+天气/补给/疲劳结算、TimedAction/ActionCost、Deadline、CancellationPolicy。commit `a1cb999`。
- **S3 天气/风向确定性解析**（GDD_002/ADR-0004）：`src/Domain/Environment/`（WeatherTransitionTable、WeatherResolver 注入流加权选择、EnvironmentModifierSet、Wind）。commit `027bcf4`。
- **S4 拓扑与确定性寻路**（GDD_003/ADR-0004）：`src/Domain/Map/`（WorldMap、Pathfinder 整数代价+RouteId 字典序平局、RouteCost、Region 容量门控、RouteContact）。commit `98f259f`。
- **S5 真值/知识分离**（ADR-0002，Integration）：MapTruth、FactionKnowledge+只读投影、ScoutingService。commit 见下。
- **测试累计 148/148 全绿，`-warnaserror` 0 warning**。每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。
- **共性偏差（ADVISORY）**：各 story 测试路径从故事原写的 `tests/unit/<sys>/*.cs`、`tests/integration/...` 归一到唯一可编译测试工程 `tests/unit/ThreeKingdom.Domain.Tests/`。

### ✅ epic-003 人物与关系 — 全部 3 story 完成（2026-06-22）
- **S1 人物核心状态**（GDD_005/ADR-0002）：`src/Domain/Characters/`（CharacterState、CapabilitySet、PersonalityProfile、HealthState、TaskCapabilityWeights）。能力→质量系数非解锁。commit `36f0e72`。
- **S2 职责权限与意愿**（GDD_005 §2/§4/ADR-0004）：AuthorityRegistry（能力不绕过授权）、TaskConflictPolicy、WillingnessCalculator（读已结算 coop_score）。commit `17f7adc`。
- **S3 方向性多维关系**（GDD_006/ADR-0004）：`src/Domain/Relationships/`（RelationshipState 四维方向性+事件幂等、CooperationEvaluator coop_score、AuthorityGrant 有效性）。commit 见下。
- **测试累计 181/181 全绿，0 warning**。

### 📌 会话收尾交接（2026-06-22）— 新会话从此处接续

**已完成并全部 push tk/main（本地 HEAD = tk/main = `14f60ce`，工作区干净）**：
- epic-001（S1–S4）✅ 关闭 · epic-002（S1–S5）✅ 关闭 · epic-003（S1–S3）✅ 关闭
- 全套测试 **181/181 全绿，`-warnaserror` 0 warning**
- 自主任务 10 提交：`cc4605d ae34330`（E1 S3/S4）· `c595be5 a1cb999 027bcf4 98f259f 0f79e6b`（E2 S1–S5）· `36f0e72 17f7adc 14f60ce`（E3 S1–S3）

**▶ 下一模块：epic-004-city-logistics（城市与后勤）** — 尚未动工（仅读过 GDD_004，无代码）
- 入口：`/story-readiness production/epics/epic-004-city-logistics/story-001-city-daily-settlement.md` → `/dev-story`
- 3 个 story：
  1. S1 城市日界产耗结算与资源守恒（Logic, ADR-0004；GDD_004：守恒恒等、日界顺序 承诺→产入→消耗→短缺后果→工事/治安、stock≥FLOOR、军粮移交后勤不双计）
  2. S2 三持有者补给守恒与路线断粮传导（Logic, ADR-0004；GDD_012）
  3. S3 外交受控入口 求援/求粮/求时限（Integration, ADR-0002；GDD_012 §8）

**接续约定（沿用本轮节奏，lean 模式）**：
- 每 story 链路：`/story-readiness → /dev-story → /code-review(须 APPROVED) → /story-done → commit+push tk/main`
- 新增 .cs 落 `src/Domain/<模块>/`；测试落 `tests/unit/ThreeKingdom.Domain.Tests/<模块>/`（故事里写的 `tests/unit/<sys>/` 等路径需归一到此唯一可编译测试工程 —— 共性 ADVISORY 偏差）
- 红线：Domain 纯 C# 无 UnityEngine；权威路径禁 float（用 `FixedPoint` Q16.16 / 整数）；确定性同输入同结果；平衡值数据驱动不硬编码；构造校验不变量、失败无部分写入
- 复用底座：`Numerics`（FixedPoint/DeterministicRandom/StateHasher）、`Time`（WorldTime/日界编排）、`Configuration`、`Map`、`Characters`、`Relationships`
- 提交信息体含 `Story: EPIC-004-S0X` + CLAUDE.md 要求的 Co-Authored-By / Claude-Session 尾注；push 目标 remote `tk`（nerrazzuri/three_kingdom），非 origin
- 验证命令：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`

**挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## Session Extract — /dev-story 2026-06-22（epic-004 S1）
- Story: production/epics/epic-004-city-logistics/story-001-city-daily-settlement.md — 城市日界产耗结算与资源守恒
- 实现方式: inline（lean，沿用本轮节奏）
- 新增 `src/Domain/City/`：CityId、CityEconomyState（不可变聚合+不变量）、CitySettlementConfig（数据驱动+范围校验）、CityDaySettlementStage(+CanonicalOrder)、CitySettlementResult(+LedgerEntry+ConservationHolds)、CityDaySettlementService（纯函数确定性五阶段结算）
- 测试 `tests/unit/ThreeKingdom.Domain.Tests/City/CityDaySettlementTests.cs`（13 测）
- AC: 4/4 覆盖（AC-1 同源库存+移交无双计 / AC-2 稳定顺序 / AC-3 下限夹取不出负 / AC-4 守恒恒等）
- 测试: 194/194 全绿，0 warning（-warnaserror）
- 偏差(ADVISORY): 测试路径 tests/unit/city/*.cs → 归一到 ThreeKingdom.Domain.Tests/City/；消耗下限夹取改用 min(demand, max(0, stock−FLOOR)) 修正 GDD 字面 max() 在 stock<FLOOR 时会凭空补齐的边界 bug（更严守「不凭空补齐」）
- Blockers: None
- Next: /code-review src/Domain/City/*.cs → /story-done

## ✅ epic-004 城市与后勤 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 城市日界产耗结算与资源守恒**（GDD_004/ADR-0004+0003）：`src/Domain/City/`（CityEconomyState、CitySettlementConfig、CityDaySettlementStage 五阶段固定顺序、CitySettlementResult+ConservationHolds、CityDaySettlementService 纯函数）。13 测。commit `96b90eb`。
- **S2 三持有者补给守恒与路线断粮传导**（GDD_012/ADR-0004）：`src/Domain/Supply/`（SupplyChainState GrandTotal 守恒、SupplyConfig、RouteSupplyLink 拓扑切断派生非按钮、SupplyCutoffEvent 单一权威只发事件、SupplySettlementService 逐时段先携行后交付）。13 测。commit `e9c8d37`。
- **S3 外交受控入口（求援/求粮/求时限 §8）**（GDD_012 §8/ADR-0002+0004，Integration）：`src/Domain/Diplomacy/`（DiplomaticRequest/Pledge/Outcome、DiplomacyConfig、DiplomacyService Evaluate+Resolve+ApplyFulfilledSupply）。延迟交付非即到、可背约失败、代价不返还、交付守恒、随机流仅兑现检查点消费。14 测。commit 见下。
- **测试累计 221/221 全绿，`-warnaserror` 0 warning**（181 基线 + 40 新增）。
- 每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。
- **共性偏差（ADVISORY）**：测试路径归一到 `tests/unit/ThreeKingdom.Domain.Tests/<模块>/`；S1 消耗夹取 `min(demand, max(0, stock−FLOOR))` 修正 GDD 字面式边界 bug。

### 📌 epic-004 收尾交接 — 新会话从此处接续
- **已完成并 push tk/main**：epic-001/002/003/004 全关闭；本会话 epic-004 三提交 `96b90eb e9c8d37` + S3（待本次 commit）。
- **▶ 下一模块**：见 `production/epics/index.md`（epic-005~009 Core/Foundation 余项）。沿用 lean 链路与红线（Domain 纯 C# 禁 UnityEngine、权威路径禁 float、确定性、数据驱动、构造校验无部分写入）。
- **复用底座新增**：`City`（日界结算）、`Supply`（三持有者守恒+断粮事件）、`Diplomacy`（受控外交入口）。
- **挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## ✅ epic-005 情报与军议 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 情报四层分离与只读投影**（GDD_007/ADR-0002，Integration）：`src/Domain/Intel/`（IntelSubjectId、IntelSource、WorldTruthLedger/TruthRecord、Observation、IntelReport、FactionIntel+IntelKnowledgeEntry+IntelProjection、IntelService）。四层分离、投影不含真值、单向流转。8 测。commit `0487100`。
- **S2 报告置信/时效/区间与确定性暴露**（GDD_007/ADR-0004，Logic）：IntelConfig、ConfidenceSignals（多信号非单一百分比）、EstimateInterval、IntelAssessment/Service、ScoutingExposureService（注入随机流）。11 测。commit `e36d0b2`。
- **S3 军师条件化建议**（GDD_008/ADR-0002，Logic）：`src/Domain/Council/`（AdvisorPerspective、AdviceTemplate、CouncilConfig、AdviceStatement、CouncilAdviceSet、WarCouncilService）。读只读投影、过时标记、置信=最弱依据×能力、结构性+反射负向断言（无成功率/最优解/命令）。10 测。commit 见下。
- **测试累计 250/250 全绿，0 warning**（181 基线 + 69 新增：epic-004 40 + epic-005 29）。
- 每 story 走完整 readiness→dev-story→code-review(APPROVED)→story-done(COMPLETE WITH NOTES)→commit+push tk/main。

### 📌 接续交接 — 新会话从此处接续
- **已完成并 push tk/main**：epic-001/002/003/004/005 全关闭。
- **▶ 下一模块候选**（见 `production/epics/index.md`）：epic-006 战前准备（gdd-009，2 story，依赖 epic-005）/ epic-007 兵法沙盒结算（gdd-010/011，依赖 epic-004 supply 事件 + epic-006）/ epic-008 后果 / epic-009 存档（Foundation，S3 现已解锁）。
- **复用底座新增**：`City`、`Supply`、`Diplomacy`、`Intel`（四层+评估+暴露）、`Council`（条件化建议）。
- 红线与 lean 链路同前。验证：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`。

## ✅ epic-006 战前准备 — 全部 2 story 完成（2026-06-22 连续会话）
- **S2 硬冲突校验与 DAG 依赖图**（GDD_009/ADR-0004，Logic）：`src/Domain/Preparation/`（OrderId/ResourceKey、TimeWindow、PreparedOrder、PreparationContext、PreparationConfig、PlanValidationResult、PlanValidator）。五类硬冲突聚合 + Kahn 拓扑检环 + 错误/风险区分。12 测。commit 见下（先于 S1，依赖方向）。
- **S1 PlanDraft 零副作用与原子提交**（GDD_009/ADR-0002，Integration）：ResourcePool（StateHasher 哈希）、PlanDraft、CommittedPlan、SubmitPlanResult、PlanCommitService。草稿零副作用（哈希不变）、提交全有或全无、失败稳定错误码零部分写入。6 测。commit 见下。
- **测试累计 268/268 全绿，0 warning**（181 基线 + 87 新增：E4 40 + E5 29 + E6 18）。

### 📌 接续交接
- **已完成并 push tk/main**：epic-001/002/003/004/005/006 全关闭。
- **▶ 下一模块候选**：epic-007 兵法沙盒结算（gdd-010/011，3 story，依赖 epic-004 supply 事件 + epic-006 CommittedPlan）/ epic-008 后果 / epic-009 存档（Foundation，S3 已解锁）。
- **复用底座新增**：`Preparation`（计划草稿/校验/原子提交）。

## ✅ epic-007 兵法沙盒结算 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 确定性战役解析管线与状态哈希**（GDD_010/ADR-0004，Logic）：`src/Domain/Battle/`（BattleUnitState、八步管线 BattleResolver、CombatMath 有效战斗力/突然性/伤亡、DetectionState、状态哈希、原子回滚）。7 测。commit `cb...`（见 git log）。
- **S2 条件链涌现与复盘标签**（GDD_010/ADR-0004，Logic）：TacticTag/TacticCondition、TacticChainConfig（slice 三链+夜袭）、RetrospectiveContext、TacticRecognizer（事后打标签、反射断言无执行按钮）。10 测。
- **S3 士气/疲劳/军纪三维与阈值检查**（GDD_011/ADR-0004，Logic）：`src/Domain/Cohesion/`（CohesionState 三维独立、MoraleEvent 幂等聚合、多输入阈值 Steady/Wavering/Routed、人数加权 Merge、ApplySupplyCutoff 消费 epic-004 SupplyCutoffEvent 单一施加）。9 测。
- **测试累计 294/294 全绿，0 warning**（181 基线 + 113 新增：E4 40 + E5 29 + E6 18 + E7 26）。

### 📌 接续交接
- **已完成并 push tk/main**：epic-001~007 全关闭。
- **▶ 下一模块候选**：epic-008 后果与可玩失败（gdd-010 §后果，2 story，依赖 epic-007）/ epic-009 存档与复现（Foundation，3 story，S3 已由 epic-005 解锁）。
- **复用底座新增**：`Battle`（确定性管线+复盘标签）、`Cohesion`（士气三维）。

## ✅ epic-008 后果与可玩失败 — 全部 2 story 完成（2026-06-22 连续会话）
- **S1 跨系统变更集校验与原子写回**（gdd-010 §后果/ADR-0004+0002，Integration）：`src/Domain/Outcome/`（OutcomeChange/ConsequenceSet 意图、OutcomeWorld 四类权威状态不可变快照+确定性 ComputeHash、OutcomeWritebackService 聚合校验→任一失败整批回滚零部分写入→全通过原子构造新快照）。守恒分组净额=0、各权威系统独占写（城市 With/关系刻度 clamp/名声带符号/人物非负）。8 测。commit `965ecb7`。
- **S2 可玩失败延续分支**（ADR-0002+强制设计锁，Integration）：OutcomeBranch 胜/撤退/失城/败北四分支、FailureContinuationService 各生成不同变更集共用 S1 写回、OutcomeConsequenceConfig 数据驱动损失（按当前值上限夹取不写负）、OutcomeContinuation 构造断言 Options 非空（败局不切死局，极端失城+主将被俘仍可问责/重整）。7 测。commit `d161f43`。

## ✅ epic-009 存档与复现 — 全部 3 story 完成（2026-06-22 连续会话）
- **S1 版本化 DTO + 原子写 + 迁移链**（ADR-0005，Integration）：`src/Domain/Persistence/`（SaveSnapshot 版本+指纹+随机流位置+真值/知识分段、RngStreamState Capture/Rebuild、ISaveSerializer+CanonicalSaveSerializer 纯 BCL 确定性文本编解码禁 Unity、ISaveMedium+SaveRepository 临时写→原子改名失败保留旧档、ISaveMigration+SaveMigrator 逐版迁移只操作不可变副本失败保留原档）。7 测。commit `b1307c1`。
- **S2 Round-trip 一致性与随机流位置**（ADR-0005+0004，Integration）：load(save(s)) 经介质往返哈希≡s、(seed,position) 读档续抽与未存档逐项一致不重抽、事件序一致、在途外援/空集合存活。5 测。commit `0256947`。
- **S3 加载校验与不兼容拒绝**（ADR-0005+TR-intel-003，Logic）：SaveLoadService 顺序校验（结构→版本→指纹→迁移），不兼容/指纹不符/损坏拒绝零部分载入、纯函数零副作用、LoadResult.Reason 可行动原因、真值/知识不交叉污染（知识段缺失拒绝非真值回填）。8 测。commit `c470fd1`（关闭 epic-009）。
- **测试累计 329/329 全绿，`-warnaserror` 0 warning**（294 基线 + 35 新增：E8 15 + E9 20）。

### 📌 全部 epic 收尾
- **已完成并 push tk/main**（HEAD=`c470fd1`）：epic-001~009 全 9 epics（28 stories）✅ 关闭。
- **复用底座新增**：`Outcome`（跨系统原子写回 + 可玩失败延续）、`Persistence`（版本化存档 + 原子写 + 迁移链 + round-trip + 加载校验）。
- **▶ 下一阶段候选**：Presentation 层 EPIC_010（Slice UX，规格已 Approved）→ `/create-epics layer:presentation`；或 Unity 表现层垂直切片重验核心幻想（CD-C3/TD CONCERNS 未实证）。
- **挂账 guardrail（非阻断）**：GitHub Actions 首次绿待确认；entity-inventory、sprint-01 旧 id 刷新。

## ▶ EPIC_010 Presentation 启动（2026-06-22 连续会话）
- **Unity 验证路径确认**：本机 `C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe`（=Unity 6.3 LTS，匹配项目 pin）batchmode 可用，有效 license（LicenseClient-Liang Kai Feng），`return code 0`。→ UI 故事可测表现逻辑走 dotnet/EditMode BLOCKING，视觉走 Editor 截图 ADVISORY。
- **一次性探针残留**：`tools/_unity_probe/`（已 .gitignore + untrack，但物理文件夹仍在磁盘；`rm -rf` 被权限策略拒，待用户手动删）。
- **/create-epics + /create-stories（lean）**：`epic-010-slice-ux`（EPIC.md + 5 stories）。commit `f03bdb5`，push tk/main `82f818f`。
- **✅ S1 投影→展示模型 + 意图→Command 底座**（ADR-0002，Logic）：新 `src/Presentation/`（netstandard2.1，禁 MonoBehaviour/UnityEngine）——`Projections/`（EnemyIntelPanelView 仅探报无真值 / CohesionView 三维分列 / RelationshipView 四维方向性 / CouncilView 并列+过时+定性置信）、`Intents/`（IntentTranslator 意图→命令载荷纯映射）、`Display`（定点→展示 decimal）。设计锁反射固化 P10/P6/P11 + 不依赖 UnityEngine 边界回归。16 测。commit `7b9de4a`，push tk/main。
- **测试累计 345/345 全绿，`-warnaserror` 0 warning**（329 基线 + 16 Presentation）。
- **工程接线**：src/Presentation 入 `ThreeKingdom.slnx` + `.gitignore` csproj 例外 + 测试工程 ProjectReference。

### 📌 EPIC_010 接续交接
- **▶ 剩余 4 story（002 主菜单 / 003 HUD 五态 / 004 暂停 / 005 无障碍）均为 UI 型**：各含「可测 ViewModel 逻辑（dotnet/EditMode BLOCKING）」+「UXML/USS/Scene 视觉外壳（需 Unity Editor，ADVISORY 截图签核）」。
- **待决结构**：UI 视觉外壳需在 repo 根建真实 Unity 6.3 项目（Assets/ProjectSettings/Packages）引用 Domain DLL + asmdef 引用 Presentation。建议先把 002–005 的可测 ViewModel 逻辑在 src/Presentation 落地（dotnet 验证），再一次性建 Unity 项目搭全部 UXML 外壳并 batchmode 跑 EditMode 验证。
- 复用底座：`Presentation`（Projections/Intents/Display + 设计锁反射回归）。

### ▶ EPIC_010 S2–S5 可测 ViewModel 逻辑完成（2026-06-23）
- **S2 主菜单**：`MainMenuViewModel`（5 态）+ `SaveSlotView`；读档错误态消费 LoadResult。6 测。
- **S3 HUD**：`HudContextView`（情境→元素集 + 模态隐去）+ `CausalChainView`（跳过终值不变）+ `NotificationFeed`（500ms 合并/临界绕队/并发≤3）。8 测。
- **S4 暂停**：`PauseMenuViewModel`（5 态 + 保存失败错误态 + 草稿 P9 门控）+ `ContinuationPromptView`（消费 epic-008 OutcomeContinuation，败局仍可继续）。6 测。
- **S5 无障碍**：`AccessibilitySettings`（校验 + 序列化 round-trip）+ `StatusChannels`（去色冗余）。5 测。
- **测试累计 370/370 全绿，0 warning**（345 + 25 新；其中含 S1 16）。commit `d51c413`，push tk/main。
- **各 UI 故事状态**：可测逻辑 BLOCKING 完成；**UXML/USS/Scene 视觉壳待 Unity 项目**（ADVISORY 截图签核）。

### 📌 待决：EPIC_010 视觉壳的 Unity 项目搭建
- 剩余只差 4 屏的 UXML/USS/Scene + asmdef 引用 Presentation。需在 repo 根建真实 Unity 6.3 项目（Assets/ProjectSettings/Packages）。
- 验证：batchmode 可编译 asmdef + 跑 EditMode（BLOCKING 编译级）；视觉截图需 graphics 模式（较重）。
- 结构性改动（reshape repo 根 + CI），待用户拍板再搭。

### ▶ EPIC_010 Unity 视觉壳（2026-06-23）
- **repo 根 Unity 6000.3.18f1 项目**（batchmode 建，matches CI 默认 projectPath）。`Assets/Plugins` 经 ThreeKingdom.{Domain,Presentation}.dll 桥引用 src/ 权威逻辑（权威源仍 src/，dotnet 测试对源编译；重建步骤见 Assets/Plugins/README.md；tech-debt：未来可改 UPM 包 asmdef 或 CI dotnet build 注入）。
- **三屏 UXML/USS/Controller**（MainMenu/Hud/PauseMenu，Assets/UI）：薄壳绑定只读 ViewModel + 按钮意图经 IntentTranslator → 命令载荷。**batchmode 编译通过**（Assembly-CSharp.dll 产出，无 error CS）= 视觉壳正确引用 Presentation DLL（compile-verified BLOCKING 级）。
- **Editor 预览窗** `Assets/Editor/UxmlPreviewWindow.cs`（菜单「三国/UXML 视觉壳预览」）：无需 Play/Scene 即可加载三屏 UXML+USS 供截图签核（编译通过）。
- commit `69f57a8`（Unity 项目+三屏壳）+ 本次（Editor 预览+文档）。push tk/main。

### 📌 EPIC_010 收尾状态
- **全部 BLOCKING 完成**：5 story 可测逻辑 dotnet 370/370 绿；3 屏 UXML 壳 batchmode 编译通过。
- **剩余皆 ADVISORY（须 graphics 模式 Editor，用户侧）**：三屏视觉/无障碍截图签核（对比度实测/文本150%/键鼠焦点/色盲冗余）；可选 Scene+PanelSettings 进 Play；S5 无障碍设置面板 + 各屏挂接。
- **挂账**：`tools/_unity_probe/` 物理文件夹待用户删（rm 被权限拒）；Assets/Plugins DLL 改 src/ 后须重建。

### ▶ EPIC_010 可 Play 场景搭建（2026-06-23，选项 a）
- **三屏 Scene 已建**：`Assets/Scenes/{MainMenu,Hud,PauseMenu}.unity`，各含 UIDocument（→ 对应 UXML + 共享 `Assets/UI/SlicePanelSettings.asset`，主题 `Assets/UI/SliceTheme.tss` = `@import unity-theme://default`）+ 对应 Controller + EventSystem（StandaloneInputModule；activeInputHandler=0 旧输入，已加 `com.unity.ugui@2.0.0`）。
- **生成器** `Assets/Editor/SliceSceneBuilder.cs`：菜单「三国/构建 Slice 场景」或 batchmode `-executeMethod ThreeKingdom.Unity.EditorTools.SliceSceneBuilder.BuildAll`，程序化建场景/PanelSettings/BuildSettings（避免手写 YAML/GUID）。
- batchmode 已跑通：ugui 解析、生成器编译、三场景生成（含 EventSystem）、无 error CS、退出码 0。MainMenu 为 BuildSettings 首场景。
- **用户侧（ADVISORY，graphics Editor）**：打开任一场景进 Play → 看渲染 + 点击 → 截图签核。
- commit 见下；push tk/main。

## ⏹ 会话收尾 — 2026-06-23（用户确认 Play OK，新会话接续）

- **用户已确认**：Unity Hub 6000.3.18f1 打开项目根 → `Assets/Scenes/MainMenu.unity` → Play **可正常运行**（视觉签核第一步通过）。
- **当前 HEAD = tk/main = `baa77d3`**，工作区干净。测试 **370/370 全绿**（dotnet，BLOCKING）。

### ▶ 新会话入口（EPIC_010 续）
按优先级三选一（lean 节奏，沿用红线）：
1. **(b) Story 005 无障碍设置面板 + 挂接**：建一个无障碍设置屏（UXML/USS/Controller）绑定 `AccessibilitySettings`（已测：缩放/色盲/减动/HUD 可见性 + 序列化 round-trip），并把这些设置挂到 MainMenu/Hud/PauseMenu 三屏（文本缩放应用、reduceMotion 关动效、HUD 元素可见性切换、色盲冗余通道）。+ 可加 `ISettingsStore` 端口持久（复用 epic-009 原子写模式）。逻辑走 dotnet BLOCKING，UXML 壳走 batchmode 编译 + 可选场景。
2. **三屏视觉/无障碍截图签核（ADVISORY，须 graphics Editor）**：打开三场景进 Play，逐项核 hud §12 / 三屏 §12（对比度实测、文本 150% 无溢出、键鼠焦点环可见、色盲去色可辨、点击交互）。证据落 `production/qa/evidence/{main-menu,hud,pause-menu,accessibility}-evidence.md` + 截图。通过后各 story 由 In Progress → Complete。
3. **EPIC_010 收尾判定**：5 story BLOCKING 全绿后，视 ADVISORY 签核进度，决定 epic 关闭或挂 ADVISORY 尾。

### 关键路径与命令
- **跑测试（BLOCKING）**：`dotnet test tests/unit/ThreeKingdom.Domain.Tests/ThreeKingdom.Domain.Tests.csproj -warnaserror`（370/370）。
- **改 src/ 后重建 Unity 桥 DLL**：`dotnet build src/Presentation/ThreeKingdom.Presentation.csproj -c Release` → 复制两 DLL 到 `Assets/Plugins/`（见该目录 README）。
- **重建/新增 Unity 场景**：菜单「三国/构建 Slice 场景」或 batchmode `-executeMethod ThreeKingdom.Unity.EditorTools.SliceSceneBuilder.BuildAll`。
- **batchmode 编译校验**：`Unity.exe -batchmode -nographics -quit -projectPath . -logFile -`，看无 `error CS` + `Library/ScriptAssemblies/Assembly-CSharp*.dll` 产出。
- **Presentation 源**：`src/Presentation/`（Projections/Intents/Screens/Accessibility/Display）；测试 `tests/.../Presentation/`。Unity 壳：`Assets/UI`（UXML/USS/Controller）+ `Assets/Scenes` + `Assets/Editor`。

### 挂账（非阻断）
- `tools/_unity_probe/` 物理文件夹待用户删（`rm -rf` 被权限策略拒）。
- Assets/Plugins 两 DLL 是 src/ 构建产物桥，改 src/ 须重建（tech-debt：未来可改 UPM 包 asmdef 或 CI dotnet build 注入）。
- GitHub Actions 首次绿待确认（Unity job license-gated；domain-tests job 无许可应可绿）。

## ▶ EPIC_010 S5 无障碍面板 + 三屏挂接完成（2026-06-23 本会话）

**可测逻辑（BLOCKING，dotnet 379/379 绿，+9 新测）—— 已落 src/Presentation/Accessibility/**：
- `ISettingsMedium`（命名键读写 + 原子改名原语端口，与存档 ISaveMedium 分离）。
- `ISettingsStore` + `SettingsStore`（临时键写→原子改名编排，镜像 epic-009 SaveRepository；加载时损坏文本回落默认不砸档，区别于存档拒绝语义）。
- `AccessibilitySettingsViewModel`（不可变 with 变换：文本缩放循环档位 100/125/150/175/200、色盲设定、减少动态翻转、HUD 元素可见性翻转；persist/load 经 store）。
- 测试：`AccessibilitySettingsStoreTests`（4：round-trip / 缺失回落 / 损坏回落 / 写失败保留旧 + 临时键清理）+ `AccessibilitySettingsViewModelTests`（5：缩放循环回环 / 变换不可变 / 色盲 / HUD 翻转 / persist-load）。

**Unity 视觉壳（ADVISORY，batchmode 编译干净，227 节点，Assembly-CSharp 产出，无 error CS）—— Assets/UI/**：
- `AccessibilityRuntime`（进程内单一来源；首访从 store 加载，面板提交即写回刷新 Current；默认 PlayerPrefs 介质）。
- `AccessibilityApplier`（把设置应用到任一屏 root：text-scale-*/cb-*/reduce-motion 经 USS class + HUD 元素显隐；**与情境可见性复合——只额外隐藏用户关闭的元素，绝不强制显示**）。
- `AccessibilitySettingsController` + `AccessibilitySettings.uxml/.uss`（自我演示面板：改设置即时应用到本屏）。
- `PlayerPrefsSettingsMedium`（ISettingsMedium 的 Unity 侧 PlayerPrefs 实现；原子改名经键值搬移）。
- `SliceTheme.tss` 增全局 class（text-scale 百分比 / reduce-motion 关过渡 / cb-* slice 阶段仅留钩子不改色）。
- **三屏挂接**：MainMenu/Hud/PauseMenu 的 Controller `OnEnable` 调 `AccessibilityApplier.Apply(root, AccessibilityRuntime.Current)`（HUD 额外含元素可见性复合）。
- **场景**：`SliceSceneBuilder` 增 AccessibilitySettings 屏 → `Assets/Scenes/AccessibilitySettings.unity` 已建（共 4 屏可 Play，batchmode 生成干净）。

**验证**：dotnet test 379/379 -warnaserror 0 warning；batchmode 编译 exit 0 无 error CS；6 个新 Assets/UI .meta + 1 新场景 + .meta 已生成。

### 🐞 修复：SliceSceneBuilder PanelSettings 引用失效（2026-06-23）
- **症状**（用户截图 `D:\Projects\三国演义\UI Test\AccessibilitySettings.png`）：AccessibilitySettings 屏进 Editor 只见空天空、UIDocument 报错。
- **根因**：`BuildAll` 在循环**外**只捕获一次 `PanelSettings panel`；`NewScene(Single)` 每次卸载场景域使该引用在第 2+ 迭代失效 → **只有首屏 MainMenu 序列化到 PanelSettings，其余三屏 m_PanelSettings=None**（不渲染）。上一轮重生连带把 Hud/PauseMenu 也改坏（之前只 Play 过首屏故未暴露）。
- **修复**：PanelSettings 改为**循环内逐场景 `LoadAssetAtPath` 重新加载** + 非空守卫（`Assets/Editor/SliceSceneBuilder.cs`）。
- **验证**（Unity 关闭后 batchmode 重生，exit 0 无 error CS）：四屏 m_PanelSettings 均指向 `guid e8806f1c…`；各屏 sourceAsset 指向各自 UXML；Build Settings 4 屏。Hud/PauseMenu/MainMenu.unity 一并修回（git 显示 M）。

**状态**：S5 BLOCKING（可测逻辑 + 编译级壳）完成 + 四屏场景 PanelSettings 修复并验证。
- **✅ 用户 Play 签核通过（2026-06-23）**：四屏渲染正常 + 按钮可点击 + 功能全测 OK（文本缩放/色盲/减少动态/HUD 可见性即时生效；三屏挂接生效）。视觉+交互 ADVISORY 签核第一步通过。
- **✅ 已入库 push tk/main**（HEAD=`ffad0fc`，工作树干净）：两提交 `0e986d5`（feat S5 面板+挂接）+ `ffad0fc`（fix PanelSettings 引用失效）。
- **▶ 下一步：EPIC_010 收尾判定**。5 story BLOCKING 全绿（dotnet 379/379 + batchmode 编译干净 + 四屏 Play 可交互签核）。可选 ADVISORY 尾：把四屏截图/逐项核对（对比度实测/文本150%无溢出/色盲去色可辨）落 `production/qa/evidence/` → 各 story In Progress→Complete → 决定 epic 关闭。
**剩余 ADVISORY（用户侧 graphics Editor）**：打开 4 屏进 Play 截图签核（文本 150% 无溢出 / 色盲冗余 / 减少动态生效 / HUD 可见性切换）→ 证据落 production/qa/evidence/ → 各 story In Progress→Complete → EPIC_010 收尾判定。
**改动文件清单（待 commit）**：M Assets/Plugins/{Domain,Presentation}.dll、Assets/UI/{Hud,MainMenu,PauseMenu}Controller.cs、Assets/UI/SliceTheme.tss、Assets/Editor/SliceSceneBuilder.cs；?? Assets/UI/Accessibility*{.cs,.uss,.uxml}、Assets/UI/PlayerPrefsSettingsMedium.cs、各 .meta、Assets/Scenes/AccessibilitySettings.unity(.meta)、src/Presentation/Accessibility/{ISettingsMedium,SettingsStore,AccessibilitySettingsViewModel}.cs、tests/.../Presentation/AccessibilitySettings{Store,ViewModel}Tests.cs。

## Session Extract — CD 复审裁决1 + 护栏落盘 2026-06-28
- creative-director 子代理审裁决1（代偿路线满足出关门/B3后置）：**APPROVE WITH GUARDRAILS**。
- 核心判断：工程分期稳健（Domain已完成、BattleOutcome是稳定接缝、后置仅Presentation、零返工）；两代偿路线确是合规非按钮条件链。但只是兵法沙盒**底线**（2消耗路线），非**支柱**（杠杆相乘火×夜×伏）——过MVP门≠展示护城河。
- 6条护栏已落 module-plan §5b.5 + epic-013 验收：①复盘穿透CausalTrace ②两路线非同质单变量可翻盘 ③非战斗状态作二元成立门非百分比 ④暴露真实可败 ⑤🔴M06硬退出门(宣称兵法沙盒MVP完成前必接≥1机动招式:假退伏击/火攻) ⑥BattleOutcome契约冻结。
- **★ 全部评审与修订完成，可开工。** 等用户启动：/create-stories epic-013 → /story-readiness S1 → /dev-story。

## Session Extract — epic-013 CampaignSession 装配完成 2026-06-28
- ★ **开炮成功，epic-013 全 6/6 Complete**（M00 脊梁达成）。提交链：S1 `ac273be`→S2 `2ebff22`→S3 `d6c846d`→S4 `aff7ca2`→S5 `2bc14ef`→S6 `6ff110f`。全套 **593/593 绿**，-warnaserror 0。
- S1 CampaignSession 骨架+配置驱动开局（R-5 闸门）· S2 日界推进复用全局序 · S3 后果原子写回 ConsequenceTransaction（R-3 势力创建经015、归属经004）· S4 统一会话存档 round-trip（复用 FIX-8）· S5 目标循环 E2E+确定性哈希（CD护栏①⑥ BattleOutcomeSummary）· S6 新旧会话共存（YAGNI 延后实质共享抽取至 M03+）。
- 端到端贯通：开局→推进→战果→后果(004/015/014)→存档round-trip→续推，确定性+失败可继续。
- 保留 GameSession 为 slice fixture；新建 CampaignSession（src/Application/Session/Campaign*）。
- **▶ 下一步（按 full-game-loop-module-plan）**：在 M00 脊梁上叠加——M01 场景目录(epic-014)/M03 治理循环(epic-015)/M08 敌方AI战术层(epic-021)。可选先走 sprint-03 收尾门（smoke/team-qa/retro）。

---

## ▶▶▶ 新会话从这里读起（2026-06-28 末次）

**一句话**：已完成 全游戏 review → 修全部 Concern/Advisory → 合并 codex 完整游戏模块规划（含子代理复审+修订）→ ADR-0009 Accepted → **开工装配**：epic-013（M00 CampaignSession 脊梁）6/6 完成 + epic-014（M01 场景目录）story-001 完成。**HEAD=`54eb514`，工作树干净，全套 dotnet 598/598 绿（-warnaserror 0）。**

### 权威规划文档（必读）
- `production/full-game-loop-module-plan-2026-06-28.md` — **完整游戏 M00–M16 模块路线图**（codex 骨架 + 我注入 §3.5 FIX地基/§5b MVP出关门+Kill Criteria+CD护栏§5b.5）。这是从内核到可玩游戏的总图。
- `docs/reviews/full-game-review-2026-06-28.md` — 6 层 review（Concern/Advisory 已全闭、回填）。
- `docs/architecture/adr-0009-campaign-session-assembly.md` — **Accepted**，CampaignSession 装配边界 + R-1~R-7 修订 + 两设计裁决。
- `production/roadmap-playable-assembly-2026-06-28.md` — 已被上面 module-plan 取代（备查）。

### 已完成（push tk/main）
- **epic-013 CampaignSession 装配 = M00 脊梁 6/6 ✅**：S1 骨架+配置驱动开局 · S2 日界推进复用全局序 · S3 后果原子写回 ConsequenceTransaction（势力创建经015/归属经004）· S4 统一会话存档 round-trip · S5 目标循环 E2E+确定性哈希（BattleOutcomeSummary 携≤5因素 CausalTrace）· S6 新旧会话共存（YAGNI 延后实质共享抽取）。代码在 `src/Application/Session/Campaign*`。
- **epic-014 M01 场景目录**：story-001 ✅（ScenarioCatalog 多场景按id开局+校验，`src/Application/Session/ScenarioCatalog.cs`）。

### ▶ 下一步（按 module-plan 依赖序）
1. **epic-014 story-002（待做）**：SliceScenario → 数据驱动 SliceScenarioData，**收尾 CON-5**（竖切硬编码工厂→不可变配置数据源）。文件 `src/Application/Session/SliceScenario.cs`（274 行硬编码），改为 SliceScenarioData 持字面值 + SliceScenario 读之。slice 回归须全绿。story 文件已建：`production/epics/epic-014-scenario-catalog/story-002-...md`（注：story-002 文件尚未创建，需 /create-stories 或手写）。
2. **epic-015 = M03 回合间治理/推进循环**（最大"可玩性"缺口）：城市治理跨日+君主任务+招揽+晋升申请+campaign loop 触发历史事件(12-2)/任务→下一情境。依赖 M00✅/M01。
3. **epic-021 = M08 敌方AI便宜80%**：gdd-016 §MVP（CON-3 缺陷已修），可与 M03 并行。
4. 其余 M04~M16 见 module-plan §6 epic 切分表（epic-014~028）。

### 关键约定/裁定（实现时遵守）
- 保留 GameSession 为 slice fixture；CampaignSession 是新建脊梁，达内容平价前不停 slice。
- 装配层只编排不拥规则（ADR-0009 R-5 闸门：不算 FixedPoint 公式、不直接写 city.owner/势力存续、不引用 *Service 内部、不 new SliceScenario.Default 作唯一源）。
- 代偿取胜路线满足 MVP 出关门；完整 GDD_010 战役命令层(B3/B4)后置 M06。**CD 硬退出门**：M06 宣称"兵法沙盒MVP完成"前必接≥1机动招式(假退伏击/火攻)。
- 君主/争霸/统一(M13/M14)**必须先补 GDD_017+/ADR** 再实现。
- 测试统一落 `tests/unit/ThreeKingdom.Domain.Tests/{Session,Career,World,...}/`；纯 Domain，dotnet test 旁路 Unity 许可。
- 提交：feat/docs 规范 + Co-Authored-By + Claude-Session trailer；push 到 remote `tk`（origin 无写权限403）。
- 全程中文；零复制现有三国游戏资产红线。

### 数字
- 13 epics + epic-014(进行中)；测试 598/598 绿；ADR-0001~0009 全 Accepted；TR-session-001~005 已登记。

---

## ▶ epic-014 story-002 完成（2026-06-28 本会话）—— CON-5 收尾

**一句话**：竖切硬编码场景工厂 → 数据/逻辑分离。新增不可变 `SliceScenarioData`（slice 全部字面值单一来源 + 嵌套 AdviceSpec/CharacterSpec），`SliceScenario` 改为读数据源组装 Domain 聚合，构造方法体内**无魔法数字**。`Default()` → `new SliceScenario(SliceScenarioData.Default)`。**行为逐字保持，确定性不变**。

- 公共 API 不变 → 消费方（SessionService/SaveCoordinator）与既有测试零改动。
- 新测 `SliceScenarioDataTests`（4：Default 单一实例 / 标量+聚合取自数据 / 集合数量+主题注入 / Default() 经数据源）。
- 健康→战力因子映射保留为组装器内领域规则（非每场景平衡值，不属 CON-5）。
- **测试 602/602 绿（-warnaserror 0，+4 新测）**。文件：`src/Application/Session/SliceScenarioData.cs`（新）+ `SliceScenario.cs`（重构）+ `tests/.../Application/SliceScenarioDataTests.cs`（新）。
- **epic-014（M01 场景目录）2/2 Complete**：story-001 ScenarioCatalog ✅ + story-002 SliceScenarioData ✅。CON-5 闭。

### ▶ epic-015 stories 已创建（2026-06-28 本会话）

**epic-015 = M02 太守开局循环**，4 story 全部 Ready：

| # | Story | 测试文件 |
|---|-------|---------|
| S001 | 开局围城续局可用性——胜败两支 Advance 均可执行 | `CampaignOpeningContinuabilityTests.cs` |
| S002 | 胜支后果——配置驱动生涯初值 + 胜支存读档 | `CampaignOpeningVictoryBranchTests.cs` |
| S003 | 败支后果——在野延续存读档 + 部曲保留验证 | `CampaignOpeningDefeatBranchTests.cs` |
| S004 | 两支 E2E 确定性——同种子同 hash + 两结果不同 hash | `CampaignOpeningDeterminismTests.cs` |

全部为**纯测试 story（零新生产代码）**，复用既有 `CampaignSessionService.ResolveSiege` 两支实装。

**核心 gap 填补**：
- S001：败局可继续性（advance-after-defeat，control-manifest 强制设计锁）
- S002：胜支存读档（SaveTests 只有败支 round-trip）
- S003：defeat-then-advance 存读档（SaveTests 仅 advance-before-defeat）
- S004：TR-session-005 Session 层确定性验证（含胜/败两支，两结果 hash 不同）

### ▶ epic-015 story-001 实现完成（2026-06-29）

- 新增 `tests/unit/ThreeKingdom.Domain.Tests/Session/CampaignOpeningContinuabilityTests.cs`（4 测试）
- **606/606 全绿，`-warnaserror` 0**（+4 新测）
- 覆盖全部 5 条 AC：胜/败支 Advance 不抛、时间递增、在野态不自动复职
- 零生产代码变更（复用既有 `CampaignSessionService.Advance`）
- 下一步：`/code-review` → `/story-done` story-001，再继续 story-002

- **story-001 ✅ Complete**（2026-06-29）：/code-review APPROVED → /story-done COMPLETE；606/606 全绿
- **story-002 ✅ Complete**（2026-06-29）：胜支配置驱动生涯初值 + 胜支存读档；7 测试；613/613 全绿；内联 review APPROVED
- **story-003 ✅ Complete**（2026-06-29）：败支在野延续存读档 + 部曲保留；5 测试；618/618 全绿；内联 review APPROVED
- **story-004 ✅ Complete**（2026-06-29）：两支 E2E 确定性（胜/败各自重放同哈希 + 两支不同哈希 + 存档切割不破链）；4 测试；622/622 全绿；内联 review APPROVED

### ✅ epic-015（M02 太守开局循环）全部完成（2026-06-29）
- 4/4 stories Complete；新增 20 测试（4+7+5+4）；**622/622 全绿**
- M02 达成：开局守城胜/败两支均可继续+存读档+部曲保留+城权经004易主+两支确定性哈希

### ▶ M03 epic-016 已建（2026-06-29）
- 补登 TR-city-003（治理命令经会话）/004（治理改变战役条件）/005（治理存读档确定性）
- 写 `epic-016-city-governance-loop/EPIC.md`：Feature 层，治理 ADR 0009/0008/0003/0004 全 Accepted
- **关键发现**：M03 不同于 M01/M02——`CampaignSession` 当前不持城市经济态，`Advance` 仅推世界模型（源码注释预留"M03 接入城市日结"）；会话无治理命令入口。**M03 需新生产代码**。
- 护栏：单城 MVP + 喂给战争的筛选尺子（不变三国志全量经营）

### ▶ epic-016 stories 已创建（2026-06-29）
4 story 全部 Ready：
| # | Story | Type | 测试文件 |
|---|-------|------|---------|
| S001 | 城市治理态接入会话 + Advance 日结 | Integration | `CampaignCityGovernanceTests.cs` |
| S002 | 治理命令入口（征用/修工事/安抚）+ 非法稳定错误码 | Integration | `CampaignGovernanceCommandTests.cs` |
| S003 | 治理选择改变战役条件（≥3 差异派生 + 可解释代价） | Logic | `City/GovernanceWarConditionTests.cs` |
| S004 | 治理态存读档 round-trip + 日界确定性 | Integration | `CampaignCityGovernanceSaveTests.cs` |

**关键**：S001~S003 含**新生产代码**（CampaignSession 持城市态 / Advance 叠城市日结 / 治理命令入口 / 治理→战役条件派生）；S004 主要测试。
**裁断**：征募移出 MVP（GDD_004 MVP 不含，待扩）；S003 只做治理→战役条件输入派生（不接完整战斗，留 M05 epic-018 消费）。

### ✅ epic-016（M03 城市治理循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 35 测试（10+11+7+7）；**657/657 全绿，-warnaserror 0**
- **新生产代码**（区别于 M01/M02 纯测试）：
  - S001：CityEconomyState.AppendTo；CampaignSession 持城市态 + Advance 按日界叠 CityDaySettlementService；CampaignStartConfig 加城市参数
  - S002：CityGovernanceConfig；CampaignErrorCode +4 码；征用/修工事/安抚命令
  - S003：WarCondition.cs + WarConditionProjection（治理→战役条件派生纯函数 + 可解释账本）
  - S004：CaptureSnapshot/Restore 加城市段（配置数据驱动不入存档体，载入方按指纹提供）
- 设计：城市态**可选**（向后兼容现有测试）；征募移出 MVP；S003 只派生战役条件输入不接完整战斗（留 M05）

### ▶ M04 epic-017 已建（2026-06-30）
- 写 `epic-017-intel-council-loop/EPIC.md`：Feature 层，治理 ADR-0009/0004/0005 全 Accepted
- 复用现有 TR-intel-001~003/council-001~002（无 untraced，像 epic-015 复用既有 TR）
- ADR-0006 不挂（治敌方 AI/epic-021）；反全知由 GDD-007 四层分离保证（已实装）
- **M04 含新生产代码**（同 M03）：会话持 WorldTruth+FactionKnowledge、侦察命令、军议快照、情报存读档
- 护栏：军师只条件化建议（无成功率/唯一推荐/自动命令）+ 情报不泄真值（UI 只读阵营知识）

### ✅ epic-017（M04 情报与军议循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 23 测试（6+6+5+6）；**680/680 全绿，-warnaserror 0**
- **新生产代码**：
  - S001：WorldTruthLedger.Records；CampaignSession 持真值+知识、PlayerKnowledge 只读出口（反全知）、ComputeHash 含情报；CampaignStartConfig 加情报参数
  - S002：CampaignErrorCode +2；Scout 命令（Observe→ToReport→ApplyReport）
  - S003：SessionCouncilSetup；CurrentKnowledgeSnapshotId；ConveneCouncil（同快照确定 + IsStaleAgainst 过时）
  - S004：CaptureSnapshot/Restore 加情报段（真值/知识分别序列化不交叉污染）
- 护栏达成：军师只条件化建议（AdviceStatement 无成功率字段）+ 情报真值仅 internal（反全知）

### ✅ epic-018（M05 战役准备循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 23 测试（8+5+5+5）；**703/703 全绿，-warnaserror 0**
- **新生产代码**：
  - S001：CampaignSession 持 Pool/Draft/PrepConfig/可达/授权/CommittedPlan + AppendPreparation 哈希；AddPlanOrder/RemovePlanOrder 命令；CampaignErrorCode +PreparationDisabled
  - S002：SubmitPlan（经 PlanCommitService.Submit 原子提交；成功才写回承诺+扣减池）
  - S003：冲突拒绝测试（ResourceShortage/Unreachable/NoAuthority/CyclicDependency + 原子性 + 可继续）
  - S004：CaptureSnapshot/Restore 加准备段（pool/draftorder/committed；配置数据驱动不入存档体）
- 护栏达成：不自动布阵（系统只校验+原子提交，玩家手动构造计划）；失败无部分写入（资源池不变）

### ✅ epic-019（M06 兵法沙盒战役循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 20 测试（6+4+5+5）；**723/723 全绿，-warnaserror 0**
- **新生产代码**：
  - S001：DetectionState.Entries；CampaignSession 持战斗态 + AppendBattle 哈希；StartBattle（从 CommittedPlan 开战，敌方确定性预设）
  - S002：ResolveBattlePhase（经 BattleResolver.ResolvePhase，原子回滚）
  - S003：MarkTacticCondition + RecognizeTactics（经 TacticRecognizer，全条件涌现无按钮）
  - S004：CaptureSnapshot/Restore 加战斗段（battle/battleunit/detection/battlecond）
- **CD 硬退出门 ✅ 满足**：接入 FeintAmbush（假退伏击）机动招式，非薄皮战斗沙盒
- **关键裁断**：敌方智能 AI（EnemyAiDecision）留 M08/epic-021（敌方 AI Domain 尚未实装）；M06 用确定性预设敌方命令。M06 测试证据（同种子同 hash + 兵法事后标签）不要求智能 AI，裁断不阻塞。

### ✅ epic-020（M07 后果与恢复循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 20 测试（5+6+4+5）；**743/743 全绿，-warnaserror 0**
- 补登 TR-outcome-001（原子整批回滚）/002（四分支都有续局，败局必非空）
- **新生产代码**：
  - S001：CampaignSession outcome 字段 + 哈希；ResolveBattleOutcome（经 FailureContinuationService，OutcomeWorld.WithCity 写回城市态）
  - S002：SetLastOutcome + LastOutcomeBranch/LastContinuationOptions（续局供 UI「继续」）
  - S003：原子契约（成功全应用 + 夹取不越界 + Domain 回滚机制确认）
  - S004：CaptureSnapshot/Restore 加后果段（outcome/outcomeopt）
- **裁断**：reputation/relationship/vitality 在 OutcomeWorld 计算暴露，不入会话独立态（避免写回过宽难测）

### ✅ epic-021（M08 敌方 AI 循环）全部完成（2026-06-30）—— 从零 Domain 内核
- 4/4 stories Complete；新增 20 测试（6+5+6+4）；**764/764 全绿，-warnaserror 0**
- 补登 TR-ai-001~004（反全知/种子 softmax/可行性门/接入战区）
- **新 Domain 内核 src/Domain/EnemyAI/**（区别于前 M03~M07 装配）：
  - S001：AiWorldView（反全知锁，构造拒真值）+ OwnForceSnapshot/ObjectivePressure + StrategicAction/AiReasonCode
  - S002：ActionScorer（效用 + 硬可行性门，性格调权重，全定点）+ ScoredAction/ScorerConfig
  - S003：SoftmaxActionSelector（定点温度加权抽样，种子可复现 + 温度单调）+ DecisionRecord + EnemyAiService
  - S004：EnemyAiBattleAdapter（StrategicAction→BattleOrder，AI 与战斗同源确定性）
- **裁断（GDD-016 §MVP 便宜 80%）**：不做 OpponentModel 记忆 / StrategicPlan 战略 / ILlmNarrator 装饰（ADR-0006 已留接口，后续 epic）
- softmax 用定点温度加权抽样（避免定点 exp，保留温度单调 + 可复现）

### ✅ epic-022（M09 生涯与权限循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 18 测试（4+4+5+5）；**782/782 全绿，-warnaserror 0**
- 复用 TR-career-001~005（无新 TR）；**轻量装配**——命令接受配置参数，career 态已在存档段无新存档代码
- 新生产代码：CampaignSessionService.ApplyCareerGain/RequestPromotion/CheckRebellionEligibility/LaunchRebellion（复用 epic-011 CareerProgressionService/RebellionService）
- 护栏：非战斗功绩源竞争力（TR-career-002）；门槛/资格不足稳定错误码无写入；失败不切死局

### ✅ epic-023（M10 历史世界与势力循环）全部完成（2026-06-30）
- 4/4 stories Complete；新增 15 测试（3+4+4+4）；**797/797 全绿，-warnaserror 0**
- 复用 TR-world-001~006（无新 TR）；新生产代码：CampaignSession 持 catalog/reach/config + AdvanceHistory 命令（HistoryAdvancer + DivergencePropagation）
- 历史态(triggered/diverged)在 world 段（epic-012），无新存档代码；catalog/reach/config 数据驱动开局注入
- 验证：够不着前置短路恒成立(reachability)；够得着+前置破坏→分叉+下游传播；同序列同走向；存读档一致

### ▶ 下一步（17 模块进度：M00~M10 完成，共 11/17）
**用户授权的 M09+M10 已连做完成。** 剩余 M11~M16 进入"需补设计/受阻"区：
- 🟡 M11 外交（epic-024）/ M12 多城（epic-025）：需补设计 GDD/ADR（协作 user-driven，不能闷头做）
- 🔴 M13 君主（epic-026，缺 GDD_017）/ M14 终局（epic-027，缺 GDD_018）：硬红线，须先建 GDD+ADR
- 🟣 M15 表现/UX（epic-028）/ M16 内容平衡发布：Unity 表现层 + 内容
- 整合验证可选：/team-qa 全量回归
- 累计：23 epics 全 Complete，88 stories，797 测试全绿
2. 依序 S002 → S003 → S004（每个 story 有 `Depends on` 前置）。
3. epic-015 全部 Complete 后进入 M03（epic-016 城市治理循环）。

---

# ⭐ 会话 HANDOFF — 2026-07-01（新会话从这里恢复，覆盖以上所有历史条目）

## 全局状态
- **代码进度**：17 模块中 **M00~M10 完成（11/17）**；23 epics 全 Complete，88 stories，**797/797 单测全绿，`-warnaserror` 0**。
- **分支**：main；**推送目标 remote `tk`**（nerrazzuri/three_kingdom；origin 无写权限 403）。
- **最近 commit**：`19c85ea` M13/M14 终盘方向正式修订进 module-plan + 决策二落定。
- **已贯通的玩家循环**：太守开局→城市治理→情报→军议→战役准备→兵法战斗→后果续局（胜败都有后续）→生涯（晋升/自立）→历史世界（够不着继续/够得着分叉）→敌方 AI（可骗的战术对手）。

## ✅ 本会话重大成果：两个终盘方向决策已定并固化
1. **决策一（终盘）= 选 C「一段三国人生」**：主角是**人**（会死/可传承/多结局/失败不删档）；**争霸、战略 AI、问鼎天下全部保留**，作为人生后期走向与最难结局；**只去掉 4X 框架**（主角是不死势力/统一唯一目的/输了删档）。
2. **决策二（科技政策）= 君主全局科技树 Reject**；只做"治理方针"（重农/重商/重武/安民等数据驱动方向）**并入 M03/M12 城市治理**，不设独立模块。
3. **已正式修订 `production/full-game-loop-module-plan-2026-06-28.md`**：M13=生涯结局与传承（Career Endings & Succession）、M14=人生落幕与史册（Life Closure & Chronicle）；§6 切分表 epic-026/027 同步；GDD_017/018 重命名为 career-endings-succession / life-closure-chronicle（待后期写）。
- **三份决策文档（已 push，自洽）**：`docs/reviews/rotk-feature-gap-reference.md`（对照）、`rotk-feature-gap-verdict-2026-06-30.md`（裁定 v3，两决策已定 + 7 条评审边界）、`endgame-direction-decision-2026-06-30.md`（终盘决策记录）。

## 剩余 M11~M16（优先序）
- 🟡 **M11 外交**（epic-024，design-first）：核心支柱4；底座 `DiplomacyService` + GDD_012 §8 已有；**先补外交 GDD/ADR 再实现**。先做同盟/求援/破盟/劝降 + 通行权/后勤通道（Should）。
- 🟡 **M12 多城/军团委任**（epic-025，design-first）：中后期核心；**红线：委任不做全自动黑箱**（可解释授权）；含治理方针、多城粮道。
- 🟢 **M15 表现/UX/可进入性**（epic-028）：⚠️ **11 个 Domain 循环已贯通但无人能玩——当前边际收益最高**。范围=onboarding/可读性/反馈（非仅首小时教程）；完整难度矩阵归 M16。
- 🔵 **M13/M14 终盘**：方向已定，但属**后期内容**，待游戏能玩、有人玩过后再做（信息更充分）。
- ⚪ **M16 内容/平衡/发布** + 各 Should 内容包（策反离间⭐、战法火攻水攻诈降⭐、招揽、忠诚、声望恶名、俘虏、宝物轻量）。

## ▶ 下一步（新会话可直接选）
1. **M15 让它能玩**（建议优先）——盘点离"能玩"差什么；11 循环已贯通但无 UI/入口。
2. **M11 外交 design-first**——写外交 GDD/ADR 草稿（不碰实现）。
3. **`/team-qa` 全量回归**——确认 M00~M10 协同无回归。

## 关键约束（新会话必须遵守）
- **全程中文**交流与文档；代码标识符英文；**绝不用韩文**。
- **零复制**现有三国题材游戏/作品专有资产红线（机制可借鉴，表达须自创；原著地名/史实可用）。
- 所有**强制设计锁**保留：兵法是条件组合非按钮、军师不自动布阵、战斗不压倒人物/关系/城市/外交/政治、失败可继续、反全知、确定性、数据驱动、城市治理喂给战争。
- **装配模式**（M03~M10 已验证）：既有 Domain 内核接入 CampaignSession + 可选态向后兼容 + 配置数据驱动不入存档体 + 确定性哈希 + round-trip + 复用既有 TR。
- **无用户指令不 commit**；commit 尾注 Co-Authored-By + Claude-Session；push 到 `tk`。
- M11+ 进入"需补设计区"——设计须 **user-driven**（协作），不闷头做。

---

## ✅ M15 交付物① 完成（2026-07-01）—— CampaignSession 交互控制台 harness

**一句话**：M00~M10 建的 11 循环此前活在 `CampaignSessionService`（797 测试证明可跑）但**无人类入口**——Unity 壳只驱动旧竖切 `GameSession`。本会话建了一个纯 C# 文本控制台，把完整脊梁接到可操作表面，**11 循环现已端到端可玩**。

**用户决策**：M15 第一步走「先做 CampaignSession 交互控制台」（A 路径，非直接 Unity UI，非先写设计文档）。理由=最快让人玩到、产出真实反馈再投 Unity。

**新增（未 commit）**：`src/Console/`（新可执行工程，首个 Exe）
- `ThreeKingdom.Console.csproj`（net10.0，引用 Application+Domain；登记进 slnx + gitignore csproj 例外）
- `PlayableCampaign.cs`——**全 11 循环启用**的确定性默认场景「汜水关太守」（数值取自各循环已验证测试夹具，合并为一个连贯场景）+ 卫星配置（晋升梯队/叛乱/战斗/兵法链/后果）
- `CampaignTextView.cs`——纯函数状态渲染（只读 public 投影，反全知：情报只读玩家知识）
- `CampaignDriver.cs`——输入符→命令分派（只经 `CampaignSessionService`，不碰可变 Domain）；含 `--script` 回放
- `Program.cs`——I/O 薄壳

**测试**：`tests/.../Console/CampaignConsoleTests.cs`（8 测）。**全套 805/805 绿**（797+8），`-warnaserror` 0。

**已验证可玩路径**（`dotnet run --project src/Console`，或 `-- --script "5 6 7 8 m m m 9 t o c r h 1 s l q"`）：
侦察→军议→治理→备战设伏→开战→标记伏击条件（条件涌现非按钮）→解析战斗→复盘识别「假退伏击」→结算战果（胜/败均可续局）→记功晋升→自立检定（门槛）→推进历史（赤壁正常结局触发）→推进时段→存读档 round-trip。
设计锁在反馈中显形：军师不报胜率/不替定计、兵法条件组合、失败不删档、反全知、确定性。

**架构边界守住**：控制台只读 public 投影 + 经用例命令（`BattleConditions`/`HistoryCatalog` 等 internal 成员跨程序集不可见，正确挡住）。

## ✅ M15 设计层完成（2026-07-01，承上）—— UX 设计文档 + epic-028

**用户不在电脑前、让我在「2 写设计文档 / 3 接 Unity」中选优先级并继续。我选 2（理由：3 我无法验证——Unity 只能 batchmode、盲改 UI 风险高、违背验证驱动；且 2 是 3 的前置，epic-028 本不存在；趁建 harness 记忆最清晰）。**

**新增（未 commit，Draft）**：
- `design/ux/m15-campaign-loop-ux.md`——M15 UX 设计文档（**Draft**）。不重复既有 hud.md 单屏规范，而定义**整条战役循环的理解与反馈层**：全循环信息架构（察→谋→备→战→果→长）、**四个跨循环反馈契约**（因果链 / 风险无胜率 / 情报置信时效 / 失败可继续续局）、新手循环序、表现层硬约束、7 条 AC、5 个待实玩 Open Questions。harness 已是每个契约的首个参考实现。
- `production/epics/epic-028-presentation-ux-feedback/EPIC.md`——**Draft for Review**。Presentation 层；治理 ADR-0002/0009/0004；拟登记 TR-ux-001~005（/create-stories 时补登）；交付物① harness 已完成、交付物② Unity 接线为后续 story。
- `production/epics/index.md`——登记 epic-028 行（🟡 Draft）+ 统计改 24 epics（23✅+1 Draft）。

**▶ 下一步候选**：
1. 你**亲自 `dotnet run --project src/Console`** 实玩，回填 `m15-campaign-loop-ux.md` §7 Open Questions（置信显示小数 vs 定性档、onboarding 卡点、因果链默认展开等）。
2. `/ux-review design/ux/m15-campaign-loop-ux.md` → 转 Approved → `/create-stories epic-028`（补登 TR-ux-*）。
3. **M15 交付物②（Unity 接线）**：重写 `Assets/UI/SessionRuntime.cs` 指向 `CampaignSessionService`（现指旧竖切 `SessionService`/`GameSession`），4 场景 ViewModel/投影重定向。建议首屏=战果复盘（最吃因果契约 UI）。**注：我无法验证 Unity 运行，此项宜你在场时做或接受 batchmode-only 验证。**
4. 本批（harness + 设计文档 + epic-028）全部**未 commit**（无用户指令不 commit）。gap reference 已入 gitignore（按用户指示，不提交）。

---

# ⭐ 会话 HANDOFF — 2026-07-01（M15 交付物①+设计层已 push；新会话从这里恢复）

## 已完成并 push tk/main（本会话）
- **`459cfae` feat(console)**：M15 CampaignSession 交互控制台 harness（`src/Console/` 5 文件）——11 循环端到端可玩；8 新测；**全套 805/805 绿，-warnaserror 0**。
- **`f277aa6` docs(m15)**：M15 UX 设计文档（`design/ux/m15-campaign-loop-ux.md`，Draft）+ epic-028（`production/epics/epic-028-presentation-ux-feedback/`，Draft for Review）+ epics/index 登记（24 epics：23✅+1 Draft）。
- **`/ux-review` 已跑**：裁定 **NEEDS REVISION（0 BLOCKING / 3 ADVISORY）**。3 条 advisory（本地化/文本膨胀约束、"本文是跨循环契约、单屏细节随 story 交付"声明、pattern 库引用）**已补进文档**并 commit（`4e...` 见下）。文档在其高度上达 APPROVED 水准；Status 仍留 Draft，因 5 个 Open Questions 需**用户实玩**才能闭。

## ▶ 新会话下一步（用户决策点，勿擅自推进）
`/create-stories epic-028` **有意暂停等用户**。两个未决方向决定 story 内容：
- **方向 A（console 可读性打磨）**：Claude 能做能验证。
- **方向 B（交付物② Unity 接线）**：重写 `Assets/UI/SessionRuntime.cs`（现指旧竖切 `SessionService`/`GameSession`）→ 指向 `CampaignSessionService`，4 场景 ViewModel/投影重定向。**Claude 无法验证 Unity 运行（仅 batchmode），宜用户在场。建议首屏=战果复盘。**

**建议序**：用户先 `dotnet run --project src/Console` 实玩一局 → 回填 `m15-campaign-loop-ux.md` §7 Open Questions（尤其军师"置信"用小数 vs 高/中/低档——小数易误读为胜率）→ 再 `/create-stories epic-028`（届时补登 TR-ux-001~005）。信息最全、不返工。

## 关键事实（新会话须知）
- 分支 main，push 到 `tk`（origin 403 无写权限）。ADR-0001~0009 全 Accepted。
- 全循环脚本自检：`dotnet run --project src/Console -- --script "5 6 7 8 m m m 9 t o c r h 1 s l q"`。
- harness 架构边界：只读 public 投影 + 只提交 Command（ADR-0002/0009）；internal 成员跨程序集编译期挡住。
- M11~M14/M16 仍在"需补设计/受阻"区（见上「剩余 M11~M16」段）；本会话只推进了 M15。

---

# ⭐ 会话 2026-07-03 — §7 五问裁定 + epic-028 拆 story + story-001 实现

## ✅ 用户已裁定 §7 全部 5 个 Open Questions（2026-07-03）
1. 置信显示 = **定性档（高/中/低）**（小数易误读为胜率）；2. onboarding 卡点 = **果·长线**（战果后不知下一步/晋升历史意义不明）；3. 因果链 = **默认折叠一键展开**；4. console harness = **仅内部验证工具**（方向 A 关闭，冻结为回归载体）；5. Unity 切入屏 = **战果复盘**。
- 已回填 `design/ux/m15-campaign-loop-ux.md`（§2.2/§3/§5/§7）并转 **Approved**；epic-028 转 **Ready**（范围收敛=方向 B Unity 接线）；index.md 同步。

## ✅ /create-stories epic-028 完成（5 story，lean inline QA）
- TR-ux-001~005 补登 tr-registry **v4**（2026-07-03）。
- 依赖链：**001 会话接缝 → 002 战果复盘屏 →（003 军议敌情 ∥ 004 HUD 主循环）→ 005 新手序+无障碍**。

## ▶ story-001（会话接缝）实现完成，待 /code-review + /story-done
- **/story-readiness = READY（17/17）** → /dev-story inline 实现（lean 惯例）。
- **改动**：
  - `git mv src/Console/PlayableCampaign.cs → src/Application/Scenarios/`（namespace `ThreeKingdom.Application.Scenarios`，console/Unity 单一场景源）；CampaignDriver + CampaignConsoleTests 补 using。
  - 新 `src/Presentation/Runtime/CampaignRuntime.cs`（纯 C# 生命周期接缝：NewGame/Advance/Status/HasSave/Save/Load；ISaveMedium 端口注入；临时槽+原子改名；槽名 `campaign-session` 与旧竖切区分；Restore 卫星配置从场景源取——数据驱动）。
  - 重写 `Assets/UI/SessionRuntime.cs`（薄静态壳→CampaignRuntime；只剩 6 方法；**零 GameSession/SessionService 类型引用**）。
  - 改写 `Assets/UI/HudController.cs`（时间/推进/存档走战役会话；**未接线面板显式「接入战役会话中……」占位+按钮禁用**——story-003/004 逐屏恢复；⚠️ 已知临时退化：旧竖切 HUD 面板不再显示旧数据）。
  - 新测 `tests/.../PresentationRuntime/CampaignRuntimeTests.cs`（8 测：生命周期/自动开局/round-trip 哈希/恢复后推进确定性/损坏信封不部分载入/渲染纯函数/HasSave/写失败保留上一份）。
- **验证**：dotnet 全套 **813/813 绿**（805+8），`-warnaserror` 0；console 脚本自检通过；**Plugins DLL 已重建同步**（旧 DLL 停在 6-24 无 CampaignSession——本次 Release 重建，Application 39K→100K）；Unity batchmode 编译验证进行中。
- **发现的既有缺口（非本 story 引入）**：`CampaignSessionService.Restore` 不接收 historyCatalog/playerReach/divergenceConfig → 恢复后的会话 `HasHistory=false`（历史**状态**在 world 段完整恢复，但读档后 AdvanceHistory 变 no-op）。console harness 同样受影响。**建议后续补 Application 小修**（Restore 增 3 个可选配置参数），归 epic-028 之外的技术债或 M16 前清理。
- **✅ 已收尾（2026-07-03）**：Unity batchmode **0 error 通过** · inline lean review 通过（advisory：Load 只捕 SaveFormatException，魔数合法但数值损坏的信封可能漏 FormatException——低风险，随存档演进收紧）· story-001 判 **Complete**（EPIC 表已更）· 走查证据骨架 `production/qa/evidence/story-001-campaign-runtime-seam-evidence.md` 待用户签核。

## ▶ story-002（战果复盘屏）实现完成（2026-07-03，承上）
- **/story-readiness READY**（修正 engine note：本项目 UI 是 UI Toolkit 非 UGUI）→ inline 实现。
- **改动**：`src/Presentation/Screens/BattleReviewView.cs`（不可变 VM：默认折叠一句话/展开 ≤5 因素/兵法/续局中文映射/长线记功；`BattleReviewTuning` 承载 Q3 裁决）· `src/Presentation/Runtime/BattleReviewDemo.cs`（临时演示战局）· `Hud.uxml` outcome-chain 扩展 + `HudController` 渲染 + `SessionRuntime.RunDemoBattle` · 新测 `Presentation/BattleReviewViewModelTests.cs`（8 测）。
- **验证**：**821/821 绿**（813+8），`-warnaserror` 0；batchmode 0 error ×2（最终产物复验）；DLL 已同步。
- **关键 deviation**（详见 story Completion Notes）：①弃用 CausalChainView（跨维度求和违反 P6）②续局按钮=记录选择（真实命令分派归 story-004）③「演示一局」临时按钮（story-004 接真实流程后移除）。

---

# ⭐ 会话 HANDOFF — 2026-07-03（epic-028 story-001/002 已完成并 push；新会话从这里恢复）

## 全局状态
- **17 模块：M00~M10 完成 + M15 进行中（epic-028 = 2/5 story Complete）**。24 epics（23✅+1 进行中）。
- **测试基线：821/821 全绿**（805 + story-001 的 8 + story-002 的 8），`-warnaserror` 0。Unity batchmode 编译 0 error。
- 分支 main，push 到 **tk**（origin 403 无写权限）。ADR-0001~0009 全 Accepted。tr-registry **v4**（TR-ux-001~005）。

## 本会话完成（2026-07-03，三个 commit）
1. **§7 五问全裁定**（用户实玩后逐条回答）：置信=定性档 / 卡点=果·长线 / 因果链=默认折叠一键展开 / console=仅内部工具（方向 A 关闭）/ Unity 首屏=战果复盘。UX 契约 `design/ux/m15-campaign-loop-ux.md` 转 **Approved**；epic-028 转 Ready→拆 **5 story**（001 接缝→002 复盘→003∥004→005）。
2. **story-001 会话接缝 Complete**：`CampaignRuntime`（纯 C# 生命周期：新局/推进/统一信封存读档，原子写回）+ `SessionRuntime` 重写（零旧竖切类型引用）+ `PlayableCampaign` 迁 `src/Application/Scenarios/`（console/Unity 单一场景源）+ **Plugins DLL 重建同步**（旧 DLL 停在 6-24 不含 CampaignSession）。⚠️ HUD 未接线面板暂「接入中」占位（story-003/004 逐屏恢复）。
3. **story-002 战果复盘屏 Complete**：`BattleReviewView`（默认折叠一句话/展开 ≤5 因素/兵法复盘/续局中文映射/长线记功；`BattleReviewTuning` 承载裁决）+ Hud outcome-chain 区 + **临时**「演示一局·胜/败」按钮（evidence 入口）。

## ▶ 新会话下一步（按优先）
1. **story-003（军议敌情屏：定性置信/时效/无胜率）∥ story-004（HUD 主循环：治理/备战/条件/可做动作）**——互相独立可任选；004 完成后移除 story-002 的临时演示按钮与 `BattleReviewDemo`/`RunDemoBattle`。
2. **用户 Unity 走查**两份 evidence（`production/qa/evidence/story-001-*-evidence.md` / `story-002-*-evidence.md`，各有核对清单+签核位）。
3. story-005（新手序+无障碍）依赖 002+003+004。

## 技术债（本会话新记）
- `CampaignSessionService.Restore` 不接收 historyCatalog/playerReach/divergenceConfig → 读档后 `HasHistory=false`（历史**状态**恢复完整但 AdvanceHistory 变 no-op；console 同样受影响，非新引入）。建议 Application 小修：Restore 增 3 个可选配置参数。
- `CampaignRuntime.Load` 只捕 `SaveFormatException`——魔数合法但数值损坏的信封可能漏 FormatException（低风险）。

## 关键约束（不变）
- 全程中文；代码标识符英文；零复制三国题材资产红线；全部强制设计锁（无胜率/反全知/失败可继续/确定性/数据驱动/兵法条件涌现）。
- 无用户指令不 commit；commit 尾注 Co-Authored-By；push 到 tk。
- **DLL 同步纪律**：改 src/ 纯 C# 层后须 `dotnet build -c Release` 并 cp 三个 DLL 到 `Assets/Plugins/`，否则 Unity 用旧码。
- M11~M14/M16 仍在「需补设计/受阻」区，设计须 user-driven。

## Session Extract — /dev-story 2026-07-04（epic-028 story-003 军议敌情屏）
- Story: production/epics/epic-028-presentation-ux-feedback/story-003-council-intel-screen.md — 军议与敌情屏（定性置信+时效+无胜率）
- 新建 ViewModel `src/Presentation/Screens/CouncilIntelView.cs`（CouncilIntelTuning 档阈值配置驱动 0.4/0.7→低/中/高 · CampaignCouncilView 并列建议+快照过时 · CampaignEnemyIntelPanelView 估计值+来源+「N段前」+ttl 过时，结构无真值）
- 改 `src/Presentation/Runtime/CampaignRuntime.cs`（ConveneCouncil/CurrentCouncilView/EnemyIntel/ScoutEnemy；ttl 取 scenario IntelConfig 单一源）· `Assets/UI/SessionRuntime.cs`（暴露四法）· `Assets/UI/HudController.cs`（军议/敌情/侦察接线，移出占位集，过时【过时】文本冗余编码）
- Test: tests/unit/ThreeKingdom.Domain.Tests/Presentation/CouncilIntelViewModelTests.cs（8 测，AC-1/2/3/4/5/6）
- 验证: dotnet 829/829 绿（-warnaserror 0）· Unity batchmode 6000.3.18f1 编译 0 error CS · 3 DLL Release 同步 Assets/Plugins/
- Evidence: production/qa/evidence/story-003-council-intel-evidence.md（自动证据已填 + 人工走查清单待用户签核）
- 范围: 侦察为即时（story-004 换延迟派出循环 + 移除 story-002 演示按钮）
- Next: /code-review then /story-done

## Session Extract — /story-done 2026-07-04
- Verdict: COMPLETE WITH NOTES
- Story: production/epics/epic-028-presentation-ux-feedback/story-003-council-intel-screen.md — 军议与敌情屏（定性置信+时效+无胜率）
- AC: 6/6 全覆盖（自动 8 测）；全套 829/829 绿；Unity batchmode 0 error CS；3 DLL 已同步
- Deviations(ADVISORY): ①新建 VM 非改旧 Projections（避免破锁测试/污染 console）②侦察即时（story-004 换延迟循环）
- Tech debt logged: None（deviation 记入 story Completion Notes）
- EPIC 表 003 → ✅ Complete；sprint-status.yaml 无 epic-028 条目（跳过）
- Next recommended: story-004（HUD 战役主循环——治理/备战/条件/可做动作 + 移除 story-002 演示按钮）

## Session Extract — /dev-story 2026-07-04（epic-028 story-004 HUD 战役主循环）
- Story: production/epics/epic-028-presentation-ux-feedback/story-004-hud-campaign-loop.md — HUD 战役主循环（治理/备战/战斗条件/可做动作）
- 新建 ViewModel `src/Presentation/Screens/HudCampaignView.cs`（HudPhaseView 四相位+可做动作 · GovernanceActionView 多维账本+三动作因果 · PrepPanelView 草稿vs承诺 · BattleConditionProgressView 还差N条·非按钮 · CampaignErrorText 错误码→文案）
- 新建 `src/Presentation/Runtime/ScriptedBattle.cs`（BattleReviewDemo 迁为永久分支参数化脚本战斗工具）；删 BattleReviewDemo.cs
- 改 `CampaignRuntime.cs`（治理/备战/相位/开战两步 StartBattle→ResolveOutcome）· `SessionRuntime.cs`（暴露循环命令，删 RunDemoBattle）· `HudController.cs`（治理/备战/战斗/相位接线，删演示按钮，账本移出占位）· `Hud.uxml`（相位横幅+治理三键+备战面板+战况条件区+结算按钮，删 demo 按钮）
- Application 一行：`CampaignSession.BattleConditions` internal→public（供条件进度视图）
- Test: tests/unit/.../Presentation/HudCampaignViewModelTests.cs（7 测 AC-2~7）；story-002 测试 4 处 BattleReviewDemo.Run→ScriptedBattle.Run
- 验证: dotnet 836/836 绿（-warnaserror 0）· Unity batchmode 6000.3.18f1 编译 0 error CS · 3 DLL Release 同步
- Evidence: production/qa/evidence/story-004-hud-loop-evidence.md（自动证据已填 + 人工走查清单待用户签核）
- Next: /code-review then /story-done

## Session Extract — /story-done 2026-07-04
- Verdict: COMPLETE WITH NOTES
- Story: production/epics/epic-028-presentation-ux-feedback/story-004-hud-campaign-loop.md — HUD 战役主循环
- AC: 7/7 全覆盖（自动 7 测）；全套 836/836 绿；Unity batchmode 0 error CS；3 DLL 已同步
- Deviations(ADVISORY): ①BattleReviewDemo→ScriptedBattle 永久化（保 story-002 测试）②开战复用脚本战斗拆两步③征用固定量④BattleConditions internal→public
- 移除 story-002 临时演示按钮 + RunDemoBattle（story-004 明列任务）
- Tech debt logged: None（deviation 记入 story Completion Notes）
- EPIC 表 004 → ✅ Complete；sprint-status.yaml 无 epic-028 条目（跳过）
- Next recommended: story-005（新手循环序 + 无障碍关键项，依赖 002+003+004——现均 Complete，可开工）

## Session Extract — /dev-story 2026-07-04（epic-028 story-005 新手引导 + 无障碍）
- Story: production/epics/epic-028-presentation-ux-feedback/story-005-onboarding-accessibility.md — 新手循环序 + 无障碍关键项对齐
- 新建 ViewModel `src/Presentation/Screens/OnboardingHints.cs`（OnboardingConfig 前N回合配置驱动 · OnboardingCue 察谋备战+果·长线+可继续 · OnboardingHints 纯函数：自动展开判定/未见筛选/中文文案）
- 新建 `Assets/UI/OnboardingRuntime.cs`（PlayerPrefs 已见集+关闭偏好，不进权威存档）
- 改 `CampaignRuntime.cs`（Round）· `SessionRuntime.cs`（Round）· `Hud.uxml`（onboarding-hint + 关闭引导按钮）· `HudController.cs`（前N回合自动展开军议 + RenderOnboarding 情境提示 + 关闭）
- Test: tests/unit/.../Presentation/OnboardingViewModelTests.cs（7 测 AC-1/3/6 + 配置校验 + 引导不改会话哈希）
- 验证: dotnet 843/843 绿（-warnaserror 0）· Unity batchmode 6000.3.18f1 编译 0 error CS · 3 DLL Release 同步
- 无障碍：AC-7 走查/字符预算落 evidence 检查表（03/04 屏已有冗余文本/符号编码）
- Evidence: production/qa/evidence/story-005-onboarding-a11y-evidence.md（自动+无障碍表+字符预算已填 + 人工走查待用户签核）
- Next: /code-review then /story-done（epic-028 收尾）

## Session Extract — /story-done 2026-07-04
- Verdict: COMPLETE WITH NOTES
- Story: production/epics/epic-028-presentation-ux-feedback/story-005-onboarding-accessibility.md — 新手循环序 + 无障碍
- AC: 6/6（AC-1/3/6+配置校验自动测试；AC-2 in-world 提示编译+走查；AC-4 无障碍结构+检查表；AC-5 字符预算 evidence）；全套 843/843 绿；Unity batchmode 0 error CS；3 DLL 同步
- Deviations(ADVISORY): ①引导用专用 Label 非 NotificationFeed 队列 ②DefeatCanContinue 备而胜局常态不触发 ③新增 Round 接口
- Tech debt logged: None（deviation 记入 story Completion Notes）
- ★ epic-028 全 5/5 Complete（EPIC.md Status→✅ Complete）；M15 表现与理解循环收尾
- Next recommended: 用户 Unity 走查 003/004/005 三份 evidence 并签核 + 提交本轮；之后 /smoke-check + /team-qa 收尾 M15，或转 M11~M14/M16 需补设计区

## Session Extract — 用户实玩修复 2026-07-04（A 可见性 + B 延迟侦察 + 情报隔离）
- 用户发现 Unity Hub 开的是 C:\Users\Liang Kai Feng\three_kingdom（旧副本 @baa77d3），非 D 盘工作仓库 → 已改开 D 盘项目（同一 git 历史，C 盘落后且无独有改动）。
- 修 A（可见性 bug，HudController）：竖切「情境显隐」默认隐藏 outcome-chain，导致开战后战况/条件/结算/复盘看不见 → 战中/战后强制显示 outcome-chain。
- 补 B（延迟侦察，GDD_007 派出→在途→返报）：新增 PendingScout + CampaignSession 在途态 + CampaignSessionService.DispatchScout + Advance 解析返报 + 存档 round-trip；ViewModel InTransit「约第X日返报」；CampaignRuntime.ScoutEnemy 改 DispatchScout（lead=ScoutLeadSegments，默认 1 日=4 时段）；HUD 敌情显示 ⏳在途 + 推进返报刷新。5 测。
- 修技术债（情报隔离）：StartCampaign 复用配置可变 FactionIntel → 多局串知识；改为 CloneInitialIntel 每局播种全新情报层。1 回归测。
- 验证：dotnet 849/849 绿（-warnaserror 0）；3 DLL Release 已同步 Assets/Plugins/。Unity batchmode 未跑（用户开着 D 盘 Editor）——用户 Ctrl+R 重编译验证。
- 用户已实玩确认 A、B 通。待提交这批修复。

<!-- QA RUN: 2026-07-04 | Sprint: M15 epic-028 | Verdict: APPROVED WITH CONDITIONS | Report: production/qa/qa-signoff-m15-2026-07-04.md -->

## Session Extract — 实玩打磨批次 2 2026-07-04（HUD 可读性 + 延迟治理）
- HUD 可读性:战况/复盘互斥显示(消除开战后重叠)+底部整条宽栏+内部id中文化(DisplayNames)+去「接入中」刷屏(顶栏真实目标)。
- 延迟治理(GDD_004 派人处理→需时见效):PendingGovernanceTask + DispatchRequisition/Repair/Appease + Advance 到点应用 + 存档 round-trip;时长数据驱动(安抚半日/征用·修工事约1日);治理面板显「⏳处理中」。6 测。
- dotnet 855/855 绿;3 DLL 同步;commit 2c493fd push tk。
- 用户实玩已确认界面清爽、治理延迟合理。
- ▶ 待接通的核心命门:玩家选择(工事/补给/情报/设伏)目前仍不改变脚本战斗结果——下一步最高价值=闭合"选择→战果"因果。

## Session Extract — 方向设计对话落 story 2026-07-04
- 用户实玩后设计对话裁定"让游戏不无聊"方向,落成 3 个 Draft epic(决策嵌 EPIC.md + story 拆分):
  - epic-029 出征攻城循环(最高优先):君主授权出征 + 闭合因果(准备决定胜负,取代脚本固定胜局) + 占城归属方案C(前2默认归玩家/后续君主种子化随机取舍→喂自立张力) + 升官联动。5 story。
  - epic-030 人才招揽循环:出现随历史(GDD_015)/知晓靠情报(反全知)/入伙靠条件+种子化随机(ADR-0006)。4 story。
  - epic-025 多城战区(M12):占城C→玩家直辖多城→委任下属打理、只碰关键决策、不全知。4 story。
- index.md 已登记三 epic(Draft)。推荐序:029→025→030。
- ⚠️ 均 Draft:实现前须补 GDD/ADR 锚点(各 EPIC "实现前置"列明);出征需扩 GDD_004 或新 GDD_019 + ADR;人才需 GDD_019-talent + ADR;多城需先定委任/权限机制。
- 未提交。

## Session Extract — epic-029 补设计锚点 2026-07-04
- 起草 GDD_019 出征攻城(Draft,12段:授权/闭合因果/占城C/回报/失败可继续/公式/边界/验收8条) + ADR-0010 占城归属契约(Proposed:复用004控制权 + 归属方案C种子化判定 + 自立倾向累积)。
- 登记:gdd-index(GDD_019 Draft, GDD_020 未创建) · adr-index(ADR-0010 Proposed) · technical-preferences ADR日志 · epic-030 GDD引用改GDD_020。
- epic-029 EPIC 更新:Governing ADR加0010、GDD_019、实现前置改"已起草待审"。
- ▶ epic-029 可开工路径:/review-all-gdds(GDD_019) → /architecture-decision(ADR-0010转Accepted) → /create-stories epic-029 → /dev-story。
- 未提交。

## Session Extract — epic-029 出征攻城 实现完成 2026-07-04
- 5 story Domain+Application 核心全实现+测试（S1授权门/S2攻城接入LaunchOffensive/S3闭合因果OffensiveSetup/S4占城C OccupationOwnership+ResolveConquest/S5升官联动）。
- Domain/Conquest：OccupationOwnership(占城C种子化) · OffensiveAuthorization · OffensiveSetup · SiegeResolution。Application：AuthorizeOffensive/CheckOffensiveTarget/ResolveConquest/LaunchOffensive + ConquestResult/OffensiveResult + 会话态(ConquestCount/RebellionLean/OffensiveAuthorization)+存读档。PlayableCampaign 增敌城(虎牢关)。StartCampaign 登记非开局初始城。
- 测试：OffensiveDomainTests(9) + CampaignConquestTests(8含端到端强准备胜/裸战败/未授权拒)。dotnet 871/871 绿(-warnaserror 0)；3 DLL Release 同步。
- 5 story → Complete + 完成说明；EPIC → ✅ Complete。GDD_019 Reviewed·ADR-0010 Accepted·TR-offensive-001..005·跨系统审查5 Warning闭(外部流程)。
- commit链：2502183(Domain)→4324e8b(Application)→17add6b(端到端)。
- ⚠️ 待接：Unity HUD「出征」入口(选目标+发起)属 Presentation 层，随后续 UI 批做（epic-029 Logic/Integration 核心已完）。

## Session Extract — 全系统平衡打磨（战斗核心 + 争霸收敛） 2026-07-04
- **战斗核心（最高优先，战斗=差异化卖点）**：铲除"坚守恒占优"退化解 + 激活死数值"疲劳"。
  - `ZoneBattleConfig`：姿态从纯战力乘数升级为**速攻 vs 久持权衡**——AssaultMod 1.25 / HoldMod 1.10 / FeintMod 0.75；疲劳倍率 Assault 2.0 / Hold 0.5 / Feint 1.0；FatiguePowerWeight 0.5（有效战力=名义×(1−疲劳×0.5)）；新增 FortifiedDefenseBonus +35%（坚固地形守方城防之利）。
  - `RoundResolutionService`：SidePower 纳入 FatiguePowerMul（久战侵蚀战力）；ResolveZoneCombat 对 TerrainKind.Fortified 守方施城防加成；NextFatigue 按姿态差异化累积。修复：去 Hold 独大暴露"城门无工事优势"→ 补 FortifiedDefenseBonus（真机制非补丁）令 test_defense_battle...holds_the_gate 复绿。
- **测试 +7**：ZoneBattleBalanceTests(5：主攻>坚守/坚守省力/疲劳翻盘/城防反胜/**同守备唯准备决定胜负**) + ContentionTests(2：兼并守恒/争霸有限步收敛单一残余)。
- **次级系统校核**：出征/占城/人才/外交默认值经现有单调性+边界+决定性用例确认**合理无需改值**（OffensiveDomainTests 兵力/补给/统率/勇武单调 + decay + 占城C频谱；TalentTests/StrategicDiplomacyTests 单调+必入/必拒边界+决定性）。争霸唯一真缺口（收敛性）已补：等强停滞=设计正确（僵局由玩家出征打破，非靠AI自兼并）。
- GDD_021 §11 落定值表+理据 + W5验证映射；GDD_019 §8 补打磨确认（默认值经测试锁定、W5端到端映射）。
- dotnet **970/970 绿**（-warnaserror 0，963→970）；3 DLL Release 重建同步 Assets/Plugins。
- 待提交 → tk/main。

## Session Extract — 人心杠杆核心（护城河）Domain+接缝+W5 2026-07-04
- 用户裁定：不砍人心杠杆，连同 P0/P1/P2 全做（任务 #6-#11）。终局由玩家推进（已定）。
- **人心杠杆（GDD_024 Draft）**：离间/策反/攻心三计，复用 Intel/Relationships/Cohesion 零件，做成"施计→改变战斗条件"闭环。
  - Domain/Subversion：SubversionScheme/TargetProfile(反全知投影)/Effect(战斗接缝)/Config/Outcome/Service。纯函数、种子化(ADR-0006)、反全知门(未侦察折扣)、可反噬(暴露+守方士气反升)、策反门(低忠诚∧高怨恨)、重复递减。
  - 接缝：OffensiveDeploymentPlanner.PlanDefender 重载 + ZoneBattleRuntime.FromOffensive 可选 SubversionEffect → 有效守军×(1−倒戈比)、守方士气+=士气/军纪delta（区域引擎无独立军纪项，MVP 折进士气，GDD_024 §17 后续）。
  - 测试 +10：SubversionTests(8：反全知门/策反门/确定性/三计效果映射/反噬暴露/无效/递减/强度单调) + SubversionBattleIntegrationTests(2 W5：施计降门槛 garrison800 t500 无施计败·施计胜；施计不单独决定 t100 施计仍败)。
- dotnet 980/980 绿（970→980，-warnaserror 0）；3 DLL Release 同步。
- ⏳ 人心杠杆待续：Application 战役接线（施计命令 + 会话态待生效效果 + 存档 + 喂 LaunchOffensive）。
- ▶ 后续任务 #7 生涯纵深 / #8 事件分级+心里话 / #9 战法内容包+失败路径 / #10 全局装配+ViewModel / #11 Unity UI(需编辑器验证)。

## Session Extract — 人心杠杆战役闭环收尾 2026-07-04
- Application 接线：CampaignSessionService.AttemptSubversion（反全知门→种子结算→成功累积待生效效果到城）+ LaunchOffensive 消费待生效效果削弱守备（有效守军×(1−倒戈比)、工事因子按士气/军纪 delta 减，下限0.1）。
- 会话态：CampaignSession 每城 PendingSubversion + SubversionAttempts（含只读映射 + restore 构造参数）。
- 存档：CaptureSnapshot 加 subversion 段 + Restore 解析（照 conquest 段模式，往返一致，GDD_024 §14）。
- 测试 +3：SubversionCampaignTests（施计降门槛 400兵无施计败·施计胜 / 施计次数累计 / 待生效存读档往返）。
- GDD_024 登记 gdd-index（Implemented，epic-032）。dotnet 983/983 绿（980→983）；3 DLL Release 同步。
- ✅ 任务 #6 人心杠杆全线完成（Domain+接缝+战役+存档+16测试）。转 #7 生涯纵深。

## Session Extract — 任务#7 生涯纵深：忠诚经营+被挖角 2026-07-04
- 发现自立结局分支(FullSupport/PartialFollow/Abandoned by loyal_ratio)、晋升(Rank/Promotion)、官职任免(AssignOffice)**本已存在且测试**。#7 真缺口=忠诚经营+被挖角。
- Domain/Career 新增：RetinueState.WithMemberAffinity/WithoutMember(内部变更器) + RetinueLoyaltyConfig + PoachResult + RetinueLoyaltyService(Reward升忠诚/Decay久疏衰减不越下限/AttemptPoach敌挖角种子化·忠者不可挖·带走官职)。
- 被挖角与 GDD_024 人心杠杆对玩家守将策反**对称**；维持的忠诚自然喂已有自立结局(loyal_ratio)。
- 测试 +6：RetinueLoyaltyTests(赏赐升/衰减不越下限/忠者免挖/低忠诚被挖失官职/确定性/非成员不变)。
- dotnet 989/989 绿(983→989)；3 DLL 同步。Application 日界 tick 接线(衰减/挖角触发)归 #10 全局装配。
- ✅ 任务 #7 完成。转 #8 事件分级+心里话。

## Session Extract — 任务#8 事件分级通报 + 主角人设心里话 2026-07-04
- 复用既有 FireReason（NormalUnreachable=够不着 / Diverged+PreconditionsHeld=够得着）做分级——不改事件四元组。
- **用户裁定修正**：心里话不埋自立种子；改为开局**随机主角人设**（Ambitious/Loyalist/Pragmatist/Cautious，种子化），心里话口吻随人设而异，纯丰富体验不改状态。
- Domain/World 新增：ProtagonistPersona + ProtagonistPersonas.Roll（开局随机）· NoticeTier{Personal|Notable|Background} · MonologueRule(按人设给台词，缺回退通用)/MonologueCatalog(Default含袁术称帝等,分人设口吻) · EventReflection · EventReflectionService(纯函数，取人设台词)。
- 测试 +8：EventReflectionTests（够得着=Personal/分叉=Personal/够不着可述=Notable带心里话/口吻随人设异/无专属回退/够不着未知=Background/未触发=null/人设Roll确定性）。
- GDD_015 §1b 升级（分级+人设心里话，去种子）。dotnet 997/997 绿(989→997)；3 DLL 同步。
- Application 接线（开局赋人设存会话 + 推进产通报流）归 #10 全局装配。
- ✅ 任务 #8 完成。转 #9 战法内容包 + 失败可继续路径。

## Session Extract — 任务#9 战法内容包(火攻)+ 失败可继续红线 2026-07-04
- **火攻**作为条件涌现内容包典范（非按钮）：TacticTag.FireAttack + 3 条件(DryField干燥/EnemyExposedToFire敌暴露/FireIgnited智将纵火)。三门齐方成型；条件计入 attacker formed count 经现有 condMul 自然增威。
- 天时：ZoneBattleContext +IsDry（Default false 保护平衡套件；ContextFrom 晴天→dry）。禀赋：BattleField Supply(乌巢/赤壁烧船)+Cover(火烧连营)区。修正 DryField 须攻方在场。
- 证明内容包架构——水攻/诈降/围点打援为同机制纯内容扩展（新条件+禀赋，代码不变），作为快随内容。
- 失败可继续红线：验证出征败后同会话可重整再战胜（败非死局）。众叛流浪续局(RebellionTests AC-6)/区域战超时退兵(平衡套件)已有覆盖。
- 测试 +6：FireAttackTests(5:三门齐成型/雨天不成/无智将不纵火/无敌不暴露/非易燃区不成) + FailureContinuableTests(1 败后再战)。
- dotnet 1003/1003 绿(997→1003)；3 DLL 同步。
- ✅ 任务 #9 完成。转 #10 全局循环装配 + ViewModel。

## Session Extract — 任务#10 全局装配(既存)+ 人设接线 2026-07-04
- 发现 CampaignRuntime 已装配绝大部分全局循环（NewGame/Advance/Council/Scout/Governance/Prep/Battle/Offensive/Defense + 大量 *View ViewModel）。#10 真缺口=把新系统接进运行期。
- 本次：主角人设接入运行期——CampaignRuntime.Persona（由 Session.Id FNV-1a 确定性派生 → 存读档一致，无需新存档字段）+ PersonaView(中文名+性情描述)。完成 #8 的 Presentation 侧。
- 测试 +2：PersonaWiringTests(赋人设+展示视图 / 存读档人设稳定)。dotnet 1005/1005 绿(1003→1005)；3 DLL 同步。
- 遗留（归 #11 屏幕层随用随接）：施计命令/忠诚 tick/事件通报流的运行期专用命令方法——待具体 UI 屏调用时接。
- ✅ 任务 #10（装配既存 + 人设接线）。转 #11 Unity UI 骨架（需编辑器验证）。

## Session Extract — 任务#11 onboarding新引导点 + 人设入HUD 2026-07-04
- 发现 Unity UI 已有框架（Hud/MainMenu/ZoneBattle/PauseMenu/Accessibility + OnboardingRuntime）。核心机制引导（"兵法条件组合非按钮"/"敌情只给估计"）本已存在且测。
- 可验证（纯C#）：OnboardingHints 加 3 引导点——PreparationDecides(六维备足决定胜负)/MindLever(攻心是撬动非替代)/PersonaIntro(人设)。测试 +1（OnboardingViewModelTests：新引导点有原创文案）。
- Unity 侧（★需编辑器验证，无法无头编译）：SessionRuntime.Persona 委托 + Hud.uxml hud-persona 标签 + HudController 绑定人设标签。约定一致但未经我验证。
- dotnet 1006/1006 绿(1005→1006)；3 DLL 同步。
- ✅ 任务 #11（可验证部分完成；Unity 视觉待编辑器验证）。全部 6 任务完成。
