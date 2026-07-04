using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Contention
{
    /// <summary>
    /// 君主争霸 + 统一终局（GDD_017/018 / epic-026/027）：争霸态 · 对手种子化兼并（强吞弱·确定性）· 支配度 · 终局判定。
    /// </summary>
    [TestFixture]
    public class ContentionTests
    {
        private static FactionId F(string id) => new FactionId(id);
        private static readonly FactionId Player = F("faction-player");

        private static ContentionState State(params (string, int)[] powers)
        {
            var list = new List<PowerStanding>();
            foreach ((string id, int cities) in powers) list.Add(new PowerStanding(F(id), cities));
            return new ContentionState(list);
        }

        // ---- M13 争霸态 + 对手扩张 ----

        [Test]
        public void test_contention_state_dominance_and_hash()
        {
            ContentionState s = State(("faction-player", 6), ("faction-wei", 2));
            Assert.That(s.TotalCities, Is.EqualTo(8));
            Assert.That(s.Dominance(Player), Is.EqualTo(FixedPoint.FromFraction(6, 8)), "支配度=领城/天下总城。");
            ContentionState b = State(("faction-player", 6), ("faction-wei", 2));
            Assert.That(b.Hash(), Is.EqualTo(s.Hash()), "同态同哈希（存档）。");
            Assert.That(s.WithCities(Player, 7).Hash(), Is.Not.EqualTo(s.Hash()), "领土变→哈希变。");
        }

        [Test]
        public void test_rival_expansion_is_deterministic()
        {
            ContentionState s = State(("faction-player", 3), ("faction-wei", 5), ("faction-wu", 1));
            var svc = new RivalExpansionService();
            ContentionState a = svc.Step(s, Player, 42UL, ContentionConfig.Default);
            ContentionState b = svc.Step(s, Player, 42UL, ContentionConfig.Default);
            Assert.That(b.Hash(), Is.EqualTo(a.Hash()), "同种子 → 同兼并走向（可复现）。");
        }

        [Test]
        public void test_strongest_rival_annexes_weakest_when_annex_occurs()
        {
            // 高兼并权重 → p 达 1 → 必兼并：最强(wei) 兼并最弱存续对手(wu) 一城。
            var certain = new ContentionConfig(FixedPoint.FromInt(10));
            ContentionState s = State(("faction-player", 3), ("faction-wei", 5), ("faction-wu", 2));
            ContentionState after = new RivalExpansionService().Step(s, Player, 1UL, certain);
            Assert.That(after.CitiesOf(F("faction-wei")), Is.EqualTo(6), "最强对手兼并得一城。");
            Assert.That(after.CitiesOf(F("faction-wu")), Is.EqualTo(1), "最弱对手失一城。");
            Assert.That(after.CitiesOf(Player), Is.EqualTo(3), "玩家不受对手互兼并影响。");
        }

        // ---- M14 终局判定 ----

        [Test]
        public void test_endgame_player_eliminated_when_no_cities()
        {
            var svc = new EndgameService();
            Assert.That(svc.Evaluate(State(("faction-player", 0), ("faction-wei", 5)), Player, EndgameConfig.Default),
                Is.EqualTo(EndgameStatus.PlayerEliminated), "领城归零 → 覆灭。");
        }

        [Test]
        public void test_endgame_unifies_by_dominance_threshold()
        {
            var svc = new EndgameService();
            // 支配度 6/8=0.75 ≥ 0.5 → 统一。
            Assert.That(svc.Evaluate(State(("faction-player", 6), ("faction-wei", 2)), Player, EndgameConfig.Default),
                Is.EqualTo(EndgameStatus.PlayerUnifies), "支配度达阈 → 统一。");
        }

        [Test]
        public void test_endgame_unifies_when_rivals_eliminated()
        {
            var svc = new EndgameService();
            Assert.That(svc.Evaluate(State(("faction-player", 3), ("faction-wei", 0)), Player, EndgameConfig.Default),
                Is.EqualTo(EndgameStatus.PlayerUnifies), "群雄尽灭 → 统一。");
        }

        [Test]
        public void test_endgame_ongoing_when_contested()
        {
            var svc = new EndgameService();
            // 玩家 3/7≈0.43 <0.5，群雄尚存 → 继续。
            Assert.That(svc.Evaluate(State(("faction-player", 3), ("faction-wei", 2), ("faction-wu", 2)), Player, EndgameConfig.Default),
                Is.EqualTo(EndgameStatus.Ongoing), "未达阈且群雄尚存 → 争霸继续。");
        }
    }
}
