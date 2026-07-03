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

        /// <summary>
        /// 【临时·story-002 evidence】在当前会话演示一局并返回战果复盘模型（确定性序列，只经用例命令）。
        /// story-004 HUD 接真实「备战→开战」流程后移除。
        /// </summary>
        public static BattleReviewView RunDemoBattle(ThreeKingdom.Domain.Outcome.OutcomeBranch branch)
            => BattleReviewDemo.Run(_runtime.Session, _runtime.Scenario, branch, BattleReviewTuning.Default);
    }
}
