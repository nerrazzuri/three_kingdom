using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Contention
{
    /// <summary>E4.1 势力战略意图 + 威胁评估（GDD_017/018）：由争霸态派生每势力意图（可解释反馈大脑第一层）。</summary>
    [TestFixture]
    public class FactionStrategyTests
    {
        private static FactionId F(string id) => new FactionId(id);
        private static ContentionState State(params (string, int)[] powers)
        {
            var list = new List<PowerStanding>();
            foreach (var (f, c) in powers) list.Add(new PowerStanding(F(f), c));
            return new ContentionState(list);
        }

        [Test]
        public void test_strongest_expands()
        {
            var s = State(("cao", 6), ("yuan", 4), ("player", 2));
            Assert.That(FactionStrategy.Assess(s, F("cao"), F("player"), null, false), Is.EqualTo(StrategicIntent.Expansion));
        }

        [Test]
        public void test_single_city_collapses_or_revenges()
        {
            var s = State(("cao", 6), ("weak", 1), ("player", 3));
            Assert.That(FactionStrategy.Assess(s, F("weak"), F("player"), null, false), Is.EqualTo(StrategicIntent.Collapse));
            Assert.That(FactionStrategy.Assess(s, F("weak"), F("player"), null, wrongedByPlayer: true), Is.EqualTo(StrategicIntent.Revenge));
        }

        [Test]
        public void test_recent_loss_triggers_recovery()
        {
            var prev = State(("cao", 6), ("sun", 4), ("player", 2));
            var now = State(("cao", 6), ("sun", 3), ("player", 3));   // 孙失一城
            Assert.That(FactionStrategy.Assess(now, F("sun"), F("player"), prev, false), Is.EqualTo(StrategicIntent.Recovery));
        }

        [Test]
        public void test_wronged_by_player_seeks_revenge()
        {
            var s = State(("cao", 6), ("sun", 4), ("player", 3));
            Assert.That(FactionStrategy.Assess(s, F("sun"), F("player"), null, wrongedByPlayer: true), Is.EqualTo(StrategicIntent.Revenge));
        }

        [Test]
        public void test_opportunist_when_weak_rival_exists()
        {
            var s = State(("cao", 6), ("sun", 4), ("weak", 1), ("player", 3));
            // 孙 4 ≥ 均(3.5) 且有弱邻(weak=1) → 趁火打劫。
            Assert.That(FactionStrategy.Assess(s, F("sun"), F("player"), null, false), Is.EqualTo(StrategicIntent.Opportunist));
        }

        [Test]
        public void test_weak_faction_seeks_diplomacy()
        {
            var s = State(("cao", 8), ("yuan", 6), ("small", 2), ("player", 4));
            // small=2 < 均(5) → 求存外交。
            Assert.That(FactionStrategy.Assess(s, F("small"), F("player"), null, false), Is.EqualTo(StrategicIntent.Diplomacy));
        }

        [Test]
        public void test_threat_to_player_scales_with_relative_strength()
        {
            var s = State(("mighty", 8), ("even", 4), ("weak", 2), ("player", 4));
            Assert.That(FactionStrategy.ThreatToPlayer(s, F("mighty"), F("player")), Is.EqualTo(3), "两倍领城 → 大威胁。");
            Assert.That(FactionStrategy.ThreatToPlayer(s, F("even"), F("player")), Is.EqualTo(2), "势均 → 中威胁。");
            Assert.That(FactionStrategy.ThreatToPlayer(s, F("weak"), F("player")), Is.EqualTo(1), "半数 → 小威胁。");
        }

        [Test]
        public void test_assess_all_excludes_player_and_dead()
        {
            var s = State(("cao", 6), ("dead", 0), ("player", 3));
            var views = FactionStrategy.AssessAll(s, F("player"), null, null);
            Assert.That(views.Count, Is.EqualTo(1), "只列存续非玩家势力。");
            Assert.That(views[0].Faction.Value, Is.EqualTo("cao"));
        }
    }
}
