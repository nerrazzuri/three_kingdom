using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>E3 反套路·渐进记忆（ADR-0013）：跨战记录玩家路线 + 连击 → AI 渐进反制（用过去战例，不作弊）。</summary>
    [TestFixture]
    public class PlayerTacticProfileTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        [Test]
        public void test_streak_accumulates_on_repeat_resets_on_change()
        {
            var p = PlayerTacticProfile.Empty
                .Record(ApproachPlan.FeintLure)
                .Record(ApproachPlan.FeintLure)
                .Record(ApproachPlan.FeintLure);
            Assert.That(p.Streak, Is.EqualTo(3), "同路线连用 → 连击累加。");
            Assert.That(p.CountOf(ApproachPlan.FeintLure), Is.EqualTo(3));

            var q = p.Record(ApproachPlan.NightRaid);
            Assert.That(q.Streak, Is.EqualTo(1), "换路线 → 连击归 1。");
        }

        [Test]
        public void test_counter_hint_is_gradual_and_targets_main_thrust()
        {
            // 无历史 → 无反制。
            Assert.That(PlayerTacticProfile.Empty.CounterHint().Weight, Is.EqualTo(0));

            // 连用夜袭 3 次 → 反制掩护区，权重渐进（3×15）。
            var raid = PlayerTacticProfile.Empty
                .Record(ApproachPlan.NightRaid).Record(ApproachPlan.NightRaid).Record(ApproachPlan.NightRaid);
            var hint = raid.CounterHint(perStreak: 15, cap: 4);
            Assert.That(hint.Zone.Value, Is.EqualTo(BattleField.Cover.Value), "夜袭 → 反制掩护区。");
            Assert.That(hint.Weight, Is.EqualTo(45), "3 连击 × 15 = 45（渐进）。");

            // 长围 → 反制粮道。
            var siege = PlayerTacticProfile.Empty.Record(ApproachPlan.ProtractedSiege);
            Assert.That(siege.CounterHint().Zone.Value, Is.EqualTo(BattleField.Supply.Value), "长围 → 反制粮道。");
        }

        [Test]
        public void test_counter_weight_caps()
        {
            var p = PlayerTacticProfile.Empty;
            for (int i = 0; i < 10; i++) p = p.Record(ApproachPlan.FrontalAssault);
            Assert.That(p.CounterHint(perStreak: 15, cap: 4).Weight, Is.EqualTo(60), "连击封顶 cap×perStreak（4×15），不无限增长（非开挂）。");
        }

        [Test]
        public void test_codec_round_trip()
        {
            var codec = new PlayerTacticProfileCodec();
            var p = PlayerTacticProfile.Empty
                .Record(ApproachPlan.FeintLure).Record(ApproachPlan.FeintLure).Record(ApproachPlan.NightRaid);
            PlayerTacticProfile back = codec.Deserialize(codec.Serialize(p));
            Assert.That(back.LastApproach, Is.EqualTo(p.LastApproach));
            Assert.That(back.Streak, Is.EqualTo(p.Streak));
            Assert.That(back.CountOf(ApproachPlan.FeintLure), Is.EqualTo(2));
            Assert.That(codec.Deserialize(null).LastApproach, Is.EqualTo(-1), "空/旧存档 → 空档。");
        }

        [Test]
        public void test_ai_reinforces_countered_zone()
        {
            var det = new Detachment(new DetachmentId("d"), BattleSide.Defender, null,
                TroopComposition.AllInfantry(300), 300, F(7, 10), F(1, 10), Posture.Hold, BattleField.Front);
            EnemyAiConfig counter = EnemyAiConfig.Default.WithCounter(BattleField.Front.Value, 30);
            int with = EnemyZoneAiService.Score(BattleField.Front, false, BattleSide.Defender, det, 200, 300, 200, counter);
            int without = EnemyZoneAiService.Score(BattleField.Front, false, BattleSide.Defender, det, 200, 300, 200, EnemyAiConfig.Default);
            Assert.That(with - without, Is.EqualTo(30), "守方在被反套路指向的区 +CounterWeight。");
        }
    }
}
