using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>副将进区域（GDD_027 #4）：主将坐镇招牌列，副将按区适配填此前无将的次列，不再被丢弃。</summary>
    [TestFixture]
    public class AttackerDeputyDeploymentTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private readonly OffensiveDeploymentPlanner _planner = new OffensiveDeploymentPlanner();
        private static BattleField Field => BattleFieldCatalog.ForTerrain(TerrainKind.Pass);

        private static OffensiveGeneral G(string id, GeneralTag[] tags, CombatTier? prow = null)
            => new OffensiveGeneral(new CharacterId(id), F(6, 10), F(7, 10), F(6, 10), GeneralSpecialty.None, tags, prow, null);

        private static OffensivePreparation Prep(ApproachPlan approach, IReadOnlyList<OffensiveGeneral> deputies, bool withCav)
        {
            OffensiveGeneral lead = G("g-lead", new[] { GeneralTag.Cavalry }, CombatTier.Peerless);
            var dict = new Dictionary<TroopType, int> { [TroopType.Infantry] = 300 };
            if (withCav) dict[TroopType.Cavalry] = 300;
            return new OffensivePreparation(
                withCav ? 600 : 300, 300, new OffensiveCommand(lead, deputies, false),
                new TroopComposition(dict), approach,
                new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Pass, scouted: true);
        }

        private static Detachment? Find(IReadOnlyList<Detachment> dets, string id)
        {
            foreach (Detachment d in dets) if (d.Id.Value == id) return d;
            return null;
        }

        [Test]
        public void test_feint_lure_deploys_deputy_to_front_column()
        {
            // Arrange：假退诱敌（含骑）主将镇侧翼，一名副将应补正面（此前为 null）。
            OffensiveGeneral deputy = G("g-deputy", new[] { GeneralTag.Defender }, CombatTier.Valiant);
            OffensivePreparation prep = Prep(ApproachPlan.FeintLure, new[] { deputy }, withCav: true);
            // Act
            var dets = _planner.PlanAttacker(prep, F(7, 10), Field);
            // Assert
            Detachment? flank = Find(dets, "atk-flank");
            Detachment? front = Find(dets, "atk-front");
            Assert.That(flank!.General!.Character.Value, Is.EqualTo("g-lead"), "主将坐镇招牌列（侧翼）。");
            Assert.That(front!.General, Is.Not.Null, "副将补正面列（不再被丢弃）。");
            Assert.That(front.General!.Character.Value, Is.EqualTo("g-deputy"), "善守副将补正面。");
        }

        [Test]
        public void test_protracted_siege_deploys_deputy_to_front()
        {
            // Arrange
            OffensiveGeneral deputy = G("g-deputy", new[] { GeneralTag.Defender }, CombatTier.Valiant);
            OffensivePreparation prep = Prep(ApproachPlan.ProtractedSiege, new[] { deputy }, withCav: false);
            // Act
            var dets = _planner.PlanAttacker(prep, F(7, 10), Field);
            // Assert
            Detachment? front = Find(dets, "atk-front");
            Assert.That(front!.General, Is.Not.Null, "围城正面列由副将坐镇。");
            Assert.That(front.General!.Character.Value, Is.EqualTo("g-deputy"));
        }

        [Test]
        public void test_no_deputy_leaves_secondary_column_leaderless_backward_compat()
        {
            // Arrange：无副将 → 次列仍无将（向后兼容）。
            OffensivePreparation prep = Prep(ApproachPlan.ProtractedSiege, new OffensiveGeneral[0], withCav: false);
            // Act
            var dets = _planner.PlanAttacker(prep, F(7, 10), Field);
            // Assert
            Detachment? supply = Find(dets, "atk-supply");
            Detachment? front = Find(dets, "atk-front");
            Assert.That(supply!.General!.Character.Value, Is.EqualTo("g-lead"), "主将镇粮道列。");
            Assert.That(front!.General, Is.Null, "无副将 → 正面次列无将（向后兼容）。");
        }

        [Test]
        public void test_front_column_prefers_defensive_deputy()
        {
            // Arrange：两副将，一善守一莽勇 → 正面取善守。
            OffensiveGeneral brawler = G("g-brawler", new[] { GeneralTag.Reckless }, CombatTier.Peerless);
            OffensiveGeneral warden = G("g-warden", new[] { GeneralTag.Defender }, CombatTier.Valiant);
            OffensivePreparation prep = Prep(ApproachPlan.ProtractedSiege, new[] { brawler, warden }, withCav: false);
            // Act
            var dets = _planner.PlanAttacker(prep, F(7, 10), Field);
            // Assert
            Detachment? front = Find(dets, "atk-front");
            Assert.That(front!.General!.Character.Value, Is.EqualTo("g-warden"), "善守副将优先补正面。");
        }
    }
}
