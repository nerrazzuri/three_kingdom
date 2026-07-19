using ThreeKingdom.Application.GridBattle;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 格子战斗运行期桥（Presentation 薄壳与 Application 会话之间）。持一局 <see cref="GridBattleSession"/>，
    /// 供 UI 只读投影 <see cref="View"/> 与下令 <see cref="SetDestination"/>/<see cref="Advance"/>。
    /// 无外部注入时构建 demo 局（复用原型地图：隘口/粮仓/林地），供独立场景可玩验证。
    /// </summary>
    public static class GridBattleRuntime
    {
        private static GridBattleSession _session;

        /// <summary>战斗配置（原型验证默认值）。</summary>
        public static GridBattleConfig Config { get; } = GridBattleConfig.Default();

        /// <summary>当前会话（无则建 demo）。</summary>
        public static GridBattleSession Session
        {
            get { if (_session == null) _session = BuildDemo(); return _session; }
        }

        /// <summary>注入一局（战役接线用）。</summary>
        public static void Begin(GridBattleSession session) => _session = session;

        /// <summary>玩家视角投影。</summary>
        public static GridBattleView View() => GridBattleView.From(Session.State, Session.Outcome);

        /// <summary>玩家设目的地（仅己方；命令契约）。</summary>
        public static GridCommandResult SetDestination(string unitId, int x, int y)
            => Session.SetPlayerDestination(new BattleUnitId(unitId), new GridCoord(x, y));

        /// <summary>推进一个时间段（终局后为空操作，返回 null）。</summary>
        public static SegmentResult Advance() => Session.IsOver ? null : Session.Advance();

        /// <summary>是否终局。</summary>
        public static bool IsOver => Session.IsOver;

        /// <summary>重开 demo 局。</summary>
        public static void Reset() => _session = BuildDemo();

        private static GridBattleSession BuildDemo()
        {
            var grid = BattleGrid.FromRows(new[]
            {
                ".......^......",
                ".......^......",
                ".......^......",
                "......FPF.....",
                ".G.....P....g.",
                "......FPF.....",
                ".......^......",
                ".......^......",
                ".......^......",
            });
            var units = new[]
            {
                Unit("guan",  GridSide.Player, TroopKind.Cavalry, 3, 2, 100, 24, "关羽"),
                Unit("zhang", GridSide.Player, TroopKind.Infantry, 3, 6, 130, 22, "张飞"),
                Unit("huang", GridSide.Player, TroopKind.Archer, 2, 4, 80, 23, "黄忠"),
                Unit("e-cav", GridSide.Enemy, TroopKind.Cavalry, 11, 2, 94, 23, "敌骑", 4, 3),
                Unit("e-inf", GridSide.Enemy, TroopKind.Infantry, 11, 6, 120, 20, "敌步", 4, 5),
                Unit("e-arc", GridSide.Enemy, TroopKind.Archer, 11, 4, 76, 22, "敌弓", 9, 4),
            };
            var state = new GridBattleState(grid, units, new WorldTime(0, DaySegment.Dawn), 100, 100);
            return new GridBattleSession(state, Config);
        }

        private static GridUnit Unit(string id, GridSide side, TroopKind kind, int x, int y, int str, int atk, string name, int? dx = null, int? dy = null)
        {
            GridCoord? dest = dx.HasValue && dy.HasValue ? new GridCoord(dx.Value, dy.Value) : (GridCoord?)null;
            return new GridUnit(new BattleUnitId(id), side, kind, new GridCoord(x, y), str, str, atk, dest, name);
        }
    }
}
