using System;
using NUnit.Framework;
using ThreeKingdom.Application.GridBattle;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.GridBattle
{
    /// <summary>格子战斗会话编排（ADR-0009/0005）：敌AI驱动推进 · 胜负 · 存档 round-trip · 防越权指挥。</summary>
    [TestFixture]
    public class GridBattleSessionTests
    {
        private static BattleUnitId U(string s) => new BattleUnitId(s);
        private static GridCoord C(int x, int y) => new GridCoord(x, y);
        private static GridUnit Unit(string id, GridSide side, TroopKind kind, int x, int y, int str, int atk)
            => new GridUnit(U(id), side, kind, C(x, y), str, str, atk);
        private static BattleGrid Grid() => BattleGrid.FromRows(new[] { "G...g", ".....", "....." });
        private static GridBattleState State(int p, int e, params GridUnit[] u)
            => new GridBattleState(Grid(), u, new WorldTime(0, DaySegment.Dawn), p, e);

        [Test]
        public void test_advance_drives_enemy_ai_movement()
        {
            var s = State(40, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 2, 300, 10),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 4, 2, 300, 10));
            var session = new GridBattleSession(s);
            session.Advance();
            Assert.That(session.State.UnitAt(C(4, 2)), Is.Null, "敌AI应自行朝玩家推进，离开起点");
        }

        [Test]
        public void test_session_reaches_player_victory()
        {
            var s = State(40, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 0, 300, 50),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 5, 1));
            var session = new GridBattleSession(s);
            session.Advance();
            Assert.That(session.Outcome, Is.EqualTo(GridBattleOutcome.PlayerVictory));
            Assert.That(session.IsOver, Is.True);
        }

        [Test]
        public void test_advance_after_over_throws()
        {
            var s = State(40, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 0, 300, 50),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 5, 1));
            var session = new GridBattleSession(s);
            session.Advance();
            Assert.That(() => session.Advance(), Throws.InstanceOf<InvalidOperationException>(), "终局后推进应抛异常");
        }

        [Test]
        public void test_player_cannot_command_enemy_unit()
        {
            var s = State(40, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 0, 100, 10),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 4, 0, 100, 10));
            var session = new GridBattleSession(s);
            Assert.That(session.SetPlayerDestination(U("e"), C(1, 1)), Is.EqualTo(GridCommandResult.UnitNotFound), "不得指挥敌军");
            Assert.That(session.SetPlayerDestination(U("p"), C(1, 1)), Is.EqualTo(GridCommandResult.Ok), "己方合法目的地应接受");
        }

        [Test]
        public void test_save_snapshot_roundtrip_preserves_hash()
        {
            var s = State(60, 45,
                Unit("p", GridSide.Player, TroopKind.Cavalry, 0, 2, 200, 20),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 4, 1, 220, 18));
            var session = new GridBattleSession(s);
            session.SetPlayerDestination(U("p"), C(3, 0));
            session.Advance();
            session.Advance();

            long before = session.State.Hash();
            GridBattleSnapshot snap = session.Snapshot();
            GridBattleSession restored = GridBattleSession.Restore(snap);

            Assert.That(restored.State.Hash(), Is.EqualTo(before), "存读档 round-trip 须哈希一致（战中续战）");
            Assert.That(restored.Outcome, Is.EqualTo(session.Outcome));
        }

        [Test]
        public void test_restored_session_continues_deterministically()
        {
            GridBattleState Fresh() => State(60, 45,
                Unit("p", GridSide.Player, TroopKind.Cavalry, 0, 2, 200, 20),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 6, 1, 220, 18));

            // 直连推进 4 段。
            var direct = new GridBattleSession(Fresh());
            direct.SetPlayerDestination(U("p"), C(6, 0));
            for (int i = 0; i < 4 && !direct.IsOver; i++) direct.Advance();

            // 推进 2 段→存→读→再推 2 段，结果须与直连一致。
            var via = new GridBattleSession(Fresh());
            via.SetPlayerDestination(U("p"), C(6, 0));
            via.Advance(); via.Advance();
            var reloaded = GridBattleSession.Restore(via.Snapshot());
            for (int i = 0; i < 2 && !reloaded.IsOver; i++) reloaded.Advance();

            Assert.That(reloaded.State.Hash(), Is.EqualTo(direct.State.Hash()), "读档续战应与不间断推进同结果（确定性）");
        }
    }
}
