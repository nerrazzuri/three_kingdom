using ThreeKingdom.Application.Session;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 运行期当前<b>战役会话</b>的进程内单一来源（跨场景存活的静态状态，epic-028 story-001）。
    /// 把 Unity 壳接到完整 <c>CampaignSession</c> 脊梁（M00~M10 全 11 循环，ADR-0009）——不再指旧竖切
    /// <c>SessionService</c>/<c>GameSession</c>。生命周期逻辑在纯 C# <see cref="CampaignRuntime"/>
    /// （dotnet 已单测）；本类只是 Unity 侧薄静态壳 + <see cref="PlayerPrefsSaveMedium"/> 介质注入。
    /// <b>不持有可变 Domain 对象</b>；UI 只拿只读 <see cref="WorldStatusView"/>（ADR-0002）。
    /// 其余面板投影（账本/敌情/军议/花名册等）随 story-003/004 逐屏接入。
    /// </summary>
    public static class SessionRuntime
    {
        private static readonly CampaignRuntime _runtime =
            new CampaignRuntime(new PlayerPrefsSaveMedium());

        /// <summary>开新局（MainMenu「新游戏」）：以共享场景配置（汜水关太守）开局，返回初始世界状态视图。</summary>
        public static WorldStatusView NewGame() => _runtime.NewGame();

        /// <summary>推进一个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public static WorldStatusView Advance() => _runtime.Advance(1);

        /// <summary>取当前世界状态视图（不推进；纯函数渲染恒等）。</summary>
        public static WorldStatusView Status() => _runtime.Status();

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public static bool HasSave() => _runtime.HasSave();

        /// <summary>原子存档当前会话（统一信封；失败保留上一份有效存档）；返回是否成功。</summary>
        public static bool Save() => _runtime.Save();

        /// <summary>读取默认槽恢复会话；成功切换当前会话返回 true，失败返回 false 与原因且不动当前会话。</summary>
        public static bool Load(out string reason) => _runtime.Load(out reason);

        // --- 军议/敌情屏（story-003 / TR-ux-002/003）：UI 只拿只读展示模型，反全知只经玩家知识投影。---

        /// <summary>会话是否启用军议/情报循环（面板可用性）。</summary>
        public static bool HasIntel() => _runtime.HasIntel;

        /// <summary>召开军议（HUD「召开军议」），返回军议屏展示模型（并列建议 + 定性置信档，无成功率/唯一推荐）。</summary>
        public static CampaignCouncilView ConveneCouncil() => _runtime.ConveneCouncil();

        /// <summary>取最近军议对当前知识快照的展示模型（不重开；侦察后其 IsStale 变真）。未召开过为 null。</summary>
        public static CampaignCouncilView CurrentCouncil() => _runtime.CurrentCouncilView();

        /// <summary>取敌情面板展示模型（GDD_007，仅估计值/来源/时效，无真值）。</summary>
        public static CampaignEnemyIntelPanelView EnemyIntel() => _runtime.EnemyIntel();

        /// <summary>【story-003 最小重定向·story-004 换延迟派出】即时侦察敌军主力并入知识；返回是否成功。</summary>
        public static bool Scout() => _runtime.ScoutEnemy().Applied;

        // --- 战役主循环（story-004 / TR-ux-001/005）：治理/备战/相位/战斗，UI 只读投影 + 命令结果。---

        /// <summary>当前回合数（1 起；新手引导前 N 回合判定，story-005）。</summary>
        public static int Round() => _runtime.Round;

        /// <summary>当前相位 + 该相位合法可做动作集（AC-5）。</summary>
        public static HudPhaseView Phase() => _runtime.Phase();

        /// <summary>治理面板（多维账本 + 三动作因果说明）。</summary>
        public static GovernanceActionView Governance() => _runtime.Governance();

        /// <summary>征用军粮；返回命令结果（失败含稳定错误码）。</summary>
        public static CampaignCommandResult Requisition(long amount) => _runtime.Requisition(amount);

        /// <summary>修工事；返回命令结果。</summary>
        public static CampaignCommandResult Repair() => _runtime.Repair();

        /// <summary>安抚民心；返回命令结果。</summary>
        public static CampaignCommandResult Appease() => _runtime.Appease();

        /// <summary>备战面板（草稿 vs 已提交）。</summary>
        public static PrepPanelView Prep() => _runtime.Prep();

        /// <summary>加入设伏草稿命令。</summary>
        public static CampaignCommandResult AddAmbushOrder() => _runtime.AddAmbushOrder();

        /// <summary>移除草稿命令。</summary>
        public static CampaignCommandResult RemoveOrder(string orderId) => _runtime.RemoveOrder(orderId);

        /// <summary>提交计划（原子承诺，不可反悔）；返回是否成功。</summary>
        public static bool SubmitPlan() => _runtime.SubmitPlan();

        /// <summary>兵法条件进度（战中相位）。</summary>
        public static BattleConditionProgressView BattleConditionProgress() => _runtime.BattleConditionProgress();

        /// <summary>开战（复用脚本战斗，要求已提交计划）；返回命令结果。</summary>
        public static CampaignCommandResult StartBattle() => _runtime.StartBattle();

        /// <summary>结算战果，返回战后复盘展示模型。</summary>
        public static BattleReviewView ResolveOutcome() => _runtime.ResolveOutcome();
    }
}
