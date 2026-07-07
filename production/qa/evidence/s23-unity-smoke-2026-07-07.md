# s23 · Unity 冒烟验证报告（武将全局融入 A-H 批 DLL 更新）

时间：2026-07-07（约09:23-09:45，北京时间）
是否跑通：基本是——步骤1/2/3/5均PASS；步骤4在可用回合内未能从UI找到入口，标记BLOCKED
说明：仓库 D:\Projects\三国演义\Claude-Code-Game-Studios，未执行git pull，未改代码，未commit/push。

## 步骤1：打开项目+编译检查
PASS。Console显示0 error / 0 warning。截图：step1-compile-console.png

## 步骤2：三国→构建Slice场景
PASS。日志显示完成：11 场景 + PanelSettings + Build Settings。12条info、0 warning、0 error。截图：step2-sliceScene-generator.png

## 步骤3：Play模式HUD
PASS。MainMenu→新游戏→选择刘玄德·小沛开局→就此起家，进入HUD。纪元行、君命行、争霸行均正常显示且无叠加遮挡；Console 0 error。截图：step3-hud-clean.png

## 步骤4：出征/守城战斗
BLOCKED（尽力探索但未达成，非报错崩溃）。
1) 相位行文字列出出征为可做行为之一，但确认为纯文本展示，点击无反应，未发现可点击入口；
2) 敌情探报面板中仅派出侦察按钮可用，派出袭扰/设伏诱敌持续为禁用状态，多次推进时段后仍未解锁攻守选项；
3) 战略图/外交/武将录/多城战区四个附属屏均可正常进入，列表与返回按钮渲染正常，但均未提供直接开战入口；
4) 全程Console保持0 error/0 warning，未见异常崩溃或红色报错；
5) 观察到一处非阻塞性小问题：点击结算君命后出现未本地化占位符文案君命: Pending（非报错，仅记录）。
结论：受限于可用回合数/时间，未能实际验证ZoneBattle战斗结算流程；但期间大量UI交互均运行正常、无异常，可作为DLL更新未破坏现有UI流程的旁证。截图：step4-battle-not-reached.png

## 步骤5：战略图屏
PASS。标题战略图·天下大势，纪元行公元190·夏 — 天下36城·16家逐鹿，反全知说明行正常显示。群雄割据列表显示曹操—4城、刘表—4城；在场武将列表含具名条目及未探明之将匿名条目，反全知机制运作正常；返回按钮正常。Console 0 error。截图：step5-campaignmap.png

总判定：核心冒烟目标（编译、场景生成、HUD、战略图）全部PASS，DLL更新未见破坏现有可玩场景/UI；战斗结算环节因未能在可用时间内从现有UI找到出征入口而未验证，标记BLOCKED（非崩溃/报错所致）。全程Console保持0 error。
