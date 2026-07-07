using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Contention
{
    /// <summary>E5 外交 AI（ADR-0013）：据玩家支配度+背信记录派生各势力对玩家立场；玩家太强/太反复 → 合纵围之。</summary>
    [TestFixture]
    public class DiplomaticAITests
    {
        private static FactionId F(string id) => new FactionId(id);
        private static ContentionState State(params (string, int)[] powers)
        {
            var list = new List<PowerStanding>();
            foreach (var (f, c) in powers) list.Add(new PowerStanding(F(f), c));
            return new ContentionState(list);
        }

        [Test]
        public void test_wronged_faction_is_hostile()
        {
            var s = State(("player", 4), ("sun", 4));
            Assert.That(DiplomaticAI.StanceToPlayer(s, F("sun"), F("player"), wrongedByPlayer: true, 1),
                Is.EqualTo(PlayerStance.Hostile));
        }

        [Test]
        public void test_dominant_player_triggers_coalition_among_threatened()
        {
            // 玩家据 8/16=0.5（支配），诸鄰弱（≤半数）→ 合纵。
            var s = State(("player", 8), ("a", 3), ("b", 3), ("c", 2));
            Assert.That(DiplomaticAI.StanceToPlayer(s, F("a"), F("player"), false, 0), Is.EqualTo(PlayerStance.Coalition));
            Assert.That(DiplomaticAI.CoalitionForming(s, F("player"), null, 0), Is.True, "多家受威胁 → 合纵成立。");
        }

        [Test]
        public void test_weak_faction_seeks_peace_when_player_not_dominant()
        {
            // 玩家不支配（4/12=0.33 边界？取 3/12=0.25 不支配），弱势小势力求和。
            var s = State(("player", 3), ("big", 6), ("tiny", 1), ("mid", 2));
            Assert.That(DiplomaticAI.StanceToPlayer(s, F("tiny"), F("player"), false, 0), Is.EqualTo(PlayerStance.Submissive));
        }

        [Test]
        public void test_no_coalition_when_player_modest()
        {
            var s = State(("player", 3), ("a", 4), ("b", 4), ("c", 4));
            Assert.That(DiplomaticAI.CoalitionForming(s, F("player"), null, 0), Is.False, "玩家不强 → 无合纵。");
        }

        [Test]
        public void test_betrayals_provoke_coalition_even_without_weakness()
        {
            // 玩家支配(6/12=0.5)且屡屡背信(≥2) → 势均势力也合纵。
            var s = State(("player", 6), ("a", 3), ("b", 3));
            Assert.That(DiplomaticAI.StanceToPlayer(s, F("a"), F("player"), false, playerBetrayals: 3),
                Is.EqualTo(PlayerStance.Coalition), "太反复 → 合纵。");
        }
    }
}
