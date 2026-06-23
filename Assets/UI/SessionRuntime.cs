using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Presentation.Projections;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// slice 运行期当前游戏会话的进程内单一来源（跨场景存活的静态状态）。把 Unity 壳接到真实
    /// Application 用例：MainMenu「新游戏」开局、HUD 读真实世界状态投影并推进时段。
    /// <b>不持有可变 Domain 对象</b>——只持有 Application 的 <see cref="GameSession"/> 句柄，
    /// 一切推进经 <see cref="SessionService"/>（ADR-0002 接缝的执行端）；UI 只拿只读 <see cref="WorldStatusView"/>。
    /// 无 MonoBehaviour 依赖（纯静态），便于被三屏 Controller 共读。
    /// </summary>
    public static class SessionRuntime
    {
        private static readonly SessionService _service = new SessionService();
        private static readonly SaveCoordinator _saves = new SaveCoordinator(new PlayerPrefsSaveMedium());
        private const string DefaultSlot = "campaign";
        private static GameSession _session;

        /// <summary>当前会话（首访自动开局，保证 HUD 单独打开也可玩）。</summary>
        public static GameSession Current => _session ??= _service.NewGame();

        /// <summary>开新局（MainMenu「新游戏」）：重置会话至第 0 日黎明，返回初始世界状态视图。</summary>
        public static WorldStatusView NewGame()
        {
            _session = _service.NewGame();
            return new WorldStatusView(_service.Project(_session));
        }

        /// <summary>推进一个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public static WorldStatusView Advance()
            => new WorldStatusView(_service.Advance(Current, 1));

        /// <summary>取当前世界状态视图（不推进）。</summary>
        public static WorldStatusView Status()
            => new WorldStatusView(_service.Project(Current));

        /// <summary>取一局目标/胜负视图（守城待变）。</summary>
        public static ObjectiveView Objective()
            => new ObjectiveView(_service.ProjectObjective(Current));

        /// <summary>取己方城市账本视图（GDD_004）。</summary>
        public static CityLedgerView Ledger()
            => new CityLedgerView(_service.ProjectCity(Current));

        /// <summary>取敌情探报视图（GDD_007；时效以当前世界时间计）。</summary>
        public static EnemyReportView Enemy()
            => new EnemyReportView(_service.ProjectIntel(Current), Current.CurrentTime);

        /// <summary>侦察敌方并返回更新后的探报视图。</summary>
        public static EnemyReportView Scout()
            => new EnemyReportView(_service.Scout(Current), Current.CurrentTime);

        /// <summary>取外交求粮视图（GDD_012 §8）。</summary>
        public static DiplomacyView Diplomacy()
            => new DiplomacyView(_service.ProjectDiplomacy(Current));

        /// <summary>求粮（受控一局一次）并返回更新后的外交视图。</summary>
        public static DiplomacyView RequestAid()
            => new DiplomacyView(_service.RequestAid(Current));

        // ---- 存档 / 读档（ADR-0005，经真实持久栈）----

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public static bool HasSave() => new PlayerPrefsSaveMedium().Exists(DefaultSlot);

        /// <summary>原子存档当前会话到默认槽；返回是否成功。</summary>
        public static bool Save() => _saves.Save(DefaultSlot, Current).Succeeded;

        /// <summary>读取默认槽恢复会话；成功则切换当前会话并返回 true，失败返回 false 且不动当前会话。</summary>
        public static bool Load(out string reason)
        {
            SessionLoadResult result = _saves.Load(DefaultSlot);
            if (result.Succeeded)
            {
                _session = result.Session;
                reason = string.Empty;
                return true;
            }
            reason = result.Reason;
            return false;
        }
    }
}
