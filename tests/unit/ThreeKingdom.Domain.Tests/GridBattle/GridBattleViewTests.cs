using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.GridBattle
{
    /// <summary>格子战斗展示视图（Presentation 投影）：地形/部队/补给/时钟投影 + 反全知藏兵排除。</summary>
    [TestFixture]
    public class GridBattleViewTests
    {
        private static BattleUnitId U(string s) => new BattleUnitId(s);
        private static GridCoord C(int x, int y) => new GridCoord(x, y);
        private static GridUnit Unit(string id, GridSide side, TroopKind kind, int x, int y)
            => new GridUnit(U(id), side, kind, C(x, y), 100, 100, 10);
        // G(0,0) / g(4,0) / 林 F(1,1)
        private static BattleGrid Grid() => BattleGrid.FromRows(new[] { "G...g", ".F...", "....." });

        private static GridBattleState State(params GridUnit[] u)
            => new GridBattleState(Grid(), u, new WorldTime(1, DaySegment.Dusk), 60, 30);

        [Test]
        public void test_projects_grid_dimensions_and_terrain()
        {
            var view = GridBattleView.From(State(), GridBattleOutcome.Ongoing);
            Assert.That(view.Width, Is.EqualTo(5));
            Assert.That(view.Height, Is.EqualTo(3));
            Assert.That(view.Cells, Has.Count.EqualTo(15));
            var forest = view.Cells.First(c => c.X == 1 && c.Y == 1);
            Assert.That(forest.Terrain, Is.EqualTo(TerrainKind.Forest));
            Assert.That(forest.Glyph, Is.EqualTo("林"));
            Assert.That(view.Cells.First(c => c.X == 0 && c.Y == 0).IsPlayerGranary, Is.True);
        }

        [Test]
        public void test_anti_omniscient_hides_enemy_in_forest_shows_own()
        {
            var view = GridBattleView.From(State(
                Unit("own-cover", GridSide.Player, TroopKind.Archer, 1, 1),  // 己方林中——应可见
                Unit("enemy-cover", GridSide.Enemy, TroopKind.Archer, 1, 1), // 敌方同格林中——应隐藏（占位不同格）
                Unit("enemy-open", GridSide.Enemy, TroopKind.Infantry, 3, 2)), GridBattleOutcome.Ongoing);
            Assert.That(view.Units.Any(u => u.Id == "own-cover"), Is.True, "己方藏兵应可见");
            Assert.That(view.Units.Any(u => u.Id == "enemy-cover"), Is.False, "敌方林中藏兵应隐藏（反全知）");
            Assert.That(view.Units.Any(u => u.Id == "enemy-open"), Is.True, "敌方开阔地应可见");
        }

        [Test]
        public void test_clock_and_supply_and_outcome_projection()
        {
            var view = GridBattleView.From(State(), GridBattleOutcome.PlayerVictory);
            Assert.That(view.ClockLabel, Is.EqualTo("第 2 日 · 黄昏"), "时钟标签（Day 从 0 起→显示+1）");
            Assert.That(view.PlayerSupply, Is.EqualTo(60));
            Assert.That(view.EnemySupply, Is.EqualTo(30));
            Assert.That(view.IsOver, Is.True);
            Assert.That(view.OutcomeLabel, Is.EqualTo("全胜！"));
        }
    }
}
