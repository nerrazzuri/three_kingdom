using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// S1 战场区域模型（GDD_021 / ADR-0012 D1/D2）：战场图 + 支队 + 交战态 + 确定性哈希 + 无坐标负向不变量。
    /// </summary>
    [TestFixture]
    public class ZoneBattleModelTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static Detachment Det(string id, BattleSide side, ZoneId at, int strength = 300)
            => new Detachment(new DetachmentId(id), side, null, TroopComposition.AllInfantry(strength),
                strength, F(6, 10), F(2, 10), Posture.Assault, at);

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new BattleClock(1, 6), BattleSide.Attacker, seed: 42UL);

        [Test]
        public void test_default_field_has_five_zones_and_symmetric_adjacency()
        {
            BattleField f = BattleField.Default();
            Assert.That(f.Zones.Count, Is.EqualTo(5));
            Assert.That(f.AreAdjacent(BattleField.Front, BattleField.Flank), Is.True);
            Assert.That(f.AreAdjacent(BattleField.Flank, BattleField.Front), Is.True, "邻接无向对称。");
            Assert.That(f.AreAdjacent(BattleField.Front, BattleField.Supply), Is.False, "正面与敌粮道不相邻。");
            Assert.That(f.AreAdjacent(BattleField.Front, BattleField.Front), Is.False, "自邻为假。");
        }

        [Test]
        public void test_zone_lookup_and_neighbors()
        {
            BattleField f = BattleField.Default();
            Assert.That(f.ZoneOf(BattleField.Flank).Terrain, Is.EqualTo(TerrainKind.Pass));
            Assert.That(f.Neighbors(BattleField.Front), Contains.Item(BattleField.Flank));
            Assert.That(f.Contains(new ZoneId("nope")), Is.False);
            Assert.Throws<KeyNotFoundException>(() => f.ZoneOf(new ZoneId("nope")));
        }

        [Test]
        public void test_field_and_zone_have_no_coordinate_fields()
        {
            // ADR-0012 D2 负向不变量：区域=命名单位非坐标格子。
            string[] subSpecific = { "grid", "facing", "coordinate", "hexcol", "hexrow", "pixel" };
            string[] exact = { "x", "y", "row", "col", "position", "posx", "posy", "tile" };
            foreach (Type t in new[] { typeof(Zone), typeof(BattleField), typeof(Detachment) })
            foreach (string n in t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name.ToLowerInvariant()))
            {
                foreach (string bad in subSpecific) Assert.That(n.Contains(bad), Is.False, $"{t.Name}.{n} 不得含网格/坐标词");
                foreach (string bad in exact) Assert.That(n, Is.Not.EqualTo(bad), $"{t.Name}.{n} 不得为坐标字段");
            }
        }

        [Test]
        public void test_state_hash_is_deterministic_and_order_independent()
        {
            Detachment a = Det("d-a", BattleSide.Attacker, BattleField.Front);
            Detachment b = Det("d-b", BattleSide.Defender, BattleField.Front);
            StateHash h1 = State(a, b).Hash();
            StateHash h2 = State(b, a).Hash();   // 输入序不同
            Assert.That(h2, Is.EqualTo(h1), "同态、输入序无关 → 同哈希（规范序）。");
        }

        [Test]
        public void test_state_hash_sensitive_to_strength_change()
        {
            StateHash before = State(Det("d-a", BattleSide.Attacker, BattleField.Front, 300)).Hash();
            StateHash after = State(Det("d-a", BattleSide.Attacker, BattleField.Front, 250)).Hash();
            Assert.That(after, Is.Not.EqualTo(before), "兵力变 → 哈希变。");
        }

        [Test]
        public void test_detachment_transit_advances_and_lands()
        {
            Detachment d = Det("d-a", BattleSide.Attacker, BattleField.Front).MoveTo(BattleField.Flank, transit: 2);
            Assert.That(d.InTransit, Is.True);
            Detachment step1 = d.AdvanceTransit();
            Assert.That(step1.InTransit, Is.True, "在途 2 回合，推进 1 次仍在途。");
            Assert.That(step1.Location, Is.EqualTo(BattleField.Front), "未到位前所在区不变。");
            Detachment landed = step1.AdvanceTransit();
            Assert.That(landed.InTransit, Is.False, "第二次推进到位。");
            Assert.That(landed.Location, Is.EqualTo(BattleField.Flank), "落位目标区。");
        }

        [Test]
        public void test_state_rejects_detachment_in_nonexistent_zone()
        {
            Assert.Throws<ArgumentException>(() => State(Det("d-a", BattleSide.Attacker, new ZoneId("ghost"))),
                "支队所在区不在战场 → 拒。");
        }

        [Test]
        public void test_totals_and_rout()
        {
            ZoneBattleState s = State(
                Det("d-a", BattleSide.Attacker, BattleField.Front, 300),
                Det("d-b", BattleSide.Attacker, BattleField.Flank, 200));
            Assert.That(s.TotalStrength(BattleSide.Attacker), Is.EqualTo(500));
            Assert.That(s.IsRouted(BattleSide.Defender), Is.True, "守方无支队 → 溃。");
            Assert.That(s.DetachmentsIn(BattleField.Front).Count, Is.EqualTo(1));
        }
    }
}
