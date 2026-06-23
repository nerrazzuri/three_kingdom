using ThreeKingdom.Application.Session;
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

        /// <summary>取己方城市账本视图（GDD_004）。</summary>
        public static CityLedgerView Ledger()
            => new CityLedgerView(_service.ProjectCity(Current));

        /// <summary>取敌情探报视图（GDD_007；时效以当前世界时间计）。</summary>
        public static EnemyReportView Enemy()
            => new EnemyReportView(_service.ProjectIntel(Current), Current.CurrentTime);

        /// <summary>侦察敌方并返回更新后的探报视图。</summary>
        public static EnemyReportView Scout()
            => new EnemyReportView(_service.Scout(Current), Current.CurrentTime);
    }
}
