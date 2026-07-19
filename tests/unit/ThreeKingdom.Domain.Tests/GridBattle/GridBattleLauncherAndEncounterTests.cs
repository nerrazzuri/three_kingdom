using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.GridBattle;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.GridBattle
{
    /// <summary>装配入口（编成→一局）与半路遭遇抉择（继续/据守/后撤）。</summary>
    [TestFixture]
    public class GridBattleLauncherAndEncounterTests
    {
        private static BattleUnitId U(string s) => new BattleUnitId(s);
        private static GridCoord C(int x, int y) => new GridCoord(x, y);

        // ---- 装配入口 ----
        private static BattleGrid WideGrid() => BattleGrid.FromRows(new[] { "G.......g", ".........", "........." });

        [Test]
        public void test_launcher_places_forces_near_own_granary_with_enemy_destination()
        {
            var grid = WideGrid();
            var player = new List<ForceUnit> { new ForceUnit("p1", TroopKind.Cavalry, 100, 20), new ForceUnit("p2", TroopKind.Infantry, 120, 18) };
            var enemy = new List<ForceUnit> { new ForceUnit("e1", TroopKind.Infantry, 110, 18) };
            var session = GridBattleLauncher.Create(grid, player, enemy);

            Assert.That(session.State.Units.Count, Is.EqualTo(3), "双方编成全部入场");
            Assert.That(session.IsOver, Is.False, "双方在场，非终局");

            foreach (var u in session.State.AliveOf(GridSide.Player))
                Assert.That(GridCoord.Manhattan(u.Position, grid.PlayerGranary),
                    Is.LessThan(GridCoord.Manhattan(u.Position, grid.EnemyGranary)), "玩家部队应靠己方粮仓");

            foreach (var u in session.State.AliveOf(GridSide.Enemy))
            {
                Assert.That(GridCoord.Manhattan(u.Position, grid.EnemyGranary),
                    Is.LessThan(GridCoord.Manhattan(u.Position, grid.PlayerGranary)), "敌军应靠敌方粮仓");
                Assert.That(u.Destination, Is.EqualTo(grid.PlayerGranary), "敌军初始目的地=玩家粮仓（推进/断粮）");
            }
        }

        [Test]
        public void test_launcher_placement_is_deterministic()
        {
            var p = new List<ForceUnit> { new ForceUnit("p1", TroopKind.Cavalry, 100, 20) };
            var e = new List<ForceUnit> { new ForceUnit("e1", TroopKind.Infantry, 110, 18) };
            long h1 = GridBattleLauncher.Create(WideGrid(), p, e).State.Hash();
            long h2 = GridBattleLauncher.Create(WideGrid(), p, e).State.Hash();
            Assert.That(h1, Is.EqualTo(h2), "同编成同地图应确定性摆位");
        }

        // ---- 半路遭遇抉择 ----
        private static GridUnit Unit(string id, GridSide side, int x, int y, GridCoord? dest = null)
            => new GridUnit(U(id), side, TroopKind.Infantry, C(x, y), 100, 100, 10, dest);

        private static GridBattleSession Session(params GridUnit[] units)
            => new GridBattleSession(new GridBattleState(
                BattleGrid.FromRows(new[] { "G.....g", ".......", "......." }),
                units, new WorldTime(0, DaySegment.Dawn), 40, 40));

        [Test]
        public void test_encounter_hold_sets_destination_to_current_cell()
        {
            var s = Session(Unit("p", GridSide.Player, 2, 1, C(5, 1)), Unit("e", GridSide.Enemy, 3, 1));
            Assert.That(s.ApplyEncounterChoice(U("p"), EncounterChoice.Hold), Is.EqualTo(GridCommandResult.Ok));
            Assert.That(s.State.UnitAt(C(2, 1))!.Destination, Is.EqualTo(C(2, 1)), "据守=目的地设为当前格");
        }

        [Test]
        public void test_encounter_retreat_steps_away_from_nearest_foe()
        {
            var s = Session(Unit("p", GridSide.Player, 2, 1, C(5, 1)), Unit("e", GridSide.Enemy, 3, 1)); // 敌在东
            s.ApplyEncounterChoice(U("p"), EncounterChoice.Retreat);
            Assert.That(s.State.UnitAt(C(2, 1))!.Destination, Is.EqualTo(C(1, 1)), "后撤=朝远离敌方向退一格（西）");
        }

        [Test]
        public void test_encounter_continue_keeps_destination()
        {
            var s = Session(Unit("p", GridSide.Player, 2, 1, C(5, 1)), Unit("e", GridSide.Enemy, 3, 1));
            s.ApplyEncounterChoice(U("p"), EncounterChoice.Continue);
            Assert.That(s.State.UnitAt(C(2, 1))!.Destination, Is.EqualTo(C(5, 1)), "继续=保持原目的地");
        }

        [Test]
        public void test_encounter_choice_rejects_enemy_unit()
        {
            var s = Session(Unit("p", GridSide.Player, 2, 1), Unit("e", GridSide.Enemy, 3, 1));
            Assert.That(s.ApplyEncounterChoice(U("e"), EncounterChoice.Hold), Is.EqualTo(GridCommandResult.UnitNotFound), "不得替敌军抉择");
        }
    }
}
