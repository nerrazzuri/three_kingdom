using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.GridBattle
{
    /// <summary>敌方格子AI（ADR-0013 特化）：反全知投影 · 确定性效用 · 同规则不作弊。</summary>
    [TestFixture]
    public class EnemyGridAiTests
    {
        private static readonly GridBattleConfig Cfg = GridBattleConfig.Default();
        private static readonly EnemyGridAi Ai = new EnemyGridAi();

        private static BattleUnitId U(string s) => new BattleUnitId(s);
        private static GridCoord C(int x, int y) => new GridCoord(x, y);

        private static GridUnit Unit(string id, GridSide side, TroopKind kind, int x, int y, int str)
            => new GridUnit(U(id), side, kind, C(x, y), str, str, 10);

        // G(0,0) 玩家仓 / g(4,0) 敌仓 / F(1,1) 林地
        private static BattleGrid Grid() => BattleGrid.FromRows(new[] { "G...g", ".F...", "....." });

        private static GridBattleState State(int pSup, int eSup, params GridUnit[] units)
            => new GridBattleState(Grid(), units, new WorldTime(0, DaySegment.Dawn), pSup, eSup);

        [Test]
        public void test_ai_view_hides_units_in_forest_and_shows_own_truthfully()
        {
            var s = State(50, 50,
                Unit("e1", GridSide.Enemy, TroopKind.Infantry, 3, 0, 100),
                Unit("hidden", GridSide.Player, TroopKind.Archer, 1, 1, 80),  // 林中藏兵
                Unit("open", GridSide.Player, TroopKind.Infantry, 2, 2, 90));
            var view = AiGridView.BuildFor(s, GridSide.Enemy);
            Assert.That(view.OwnUnits, Has.Count.EqualTo(1), "AI 只见自身单位");
            Assert.That(view.VisibleEnemies, Has.Count.EqualTo(1), "林中藏兵不入可见面");
            Assert.That(view.VisibleEnemies[0].Id, Is.EqualTo(U("open")), "只见非林地的敌");
        }

        [Test]
        public void test_cavalry_targets_enemy_granary_to_cut_supply()
        {
            var s = State(50, 50,
                Unit("ecav", GridSide.Enemy, TroopKind.Cavalry, 4, 2, 100),
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 2, 100));
            var orders = Ai.Plan(AiGridView.BuildFor(s, GridSide.Enemy), Cfg);
            Assert.That(orders, Has.Count.EqualTo(1));
            Assert.That(orders[0].Destination, Is.EqualTo(C(0, 0)), "骑兵应扑向玩家粮仓断粮");
        }

        [Test]
        public void test_infantry_targets_nearest_visible_enemy()
        {
            var s = State(50, 50,
                Unit("einf", GridSide.Enemy, TroopKind.Infantry, 4, 2, 100),
                Unit("near", GridSide.Player, TroopKind.Infantry, 2, 2, 100),
                Unit("far", GridSide.Player, TroopKind.Infantry, 0, 0, 100));
            var orders = Ai.Plan(AiGridView.BuildFor(s, GridSide.Enemy), Cfg);
            Assert.That(orders[0].Destination, Is.EqualTo(C(2, 2)), "步兵应逼近最近可见敌");
        }

        [Test]
        public void test_starving_ai_retreats_to_own_granary()
        {
            var s = State(50, 10, // 敌补给告急
                Unit("einf", GridSide.Enemy, TroopKind.Infantry, 2, 2, 100),
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 2, 100));
            var orders = Ai.Plan(AiGridView.BuildFor(s, GridSide.Enemy), Cfg);
            Assert.That(orders[0].Destination, Is.EqualTo(C(4, 0)), "补给告急应退守己仓(敌仓 g@4,0)");
        }

        [Test]
        public void test_plan_is_deterministic()
        {
            var s = State(50, 50,
                Unit("ecav", GridSide.Enemy, TroopKind.Cavalry, 4, 2, 100),
                Unit("einf", GridSide.Enemy, TroopKind.Infantry, 3, 1, 100),
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 1, 100));
            var v = AiGridView.BuildFor(s, GridSide.Enemy);
            var a = Ai.Plan(v, Cfg);
            var b = Ai.Plan(v, Cfg);
            Assert.That(a.Count, Is.EqualTo(b.Count));
            for (int i = 0; i < a.Count; i++)
                Assert.That(a[i].Destination, Is.EqualTo(b[i].Destination), "同视图应产出同规划");
        }

        [Test]
        public void test_apply_sets_destinations_via_command_contract()
        {
            var s = State(50, 50,
                Unit("ecav", GridSide.Enemy, TroopKind.Cavalry, 4, 2, 100),
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 2, 100));
            Ai.Apply(s, GridSide.Enemy, Cfg);
            var cav = s.UnitAt(C(4, 2));
            Assert.That(cav!.Destination, Is.EqualTo(C(0, 0)), "Apply 应经 SetDestination 派目的地（玩家仓）");
        }
    }
}
