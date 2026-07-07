using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>守将进战斗（GDD_027 #3）：守方城册按标签择位——善守/铁骨镇正面，诡谋/远图护粮道；守将携标签/档入结算。</summary>
    [TestFixture]
    public class DefenderGeneralsTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private readonly OffensiveDeploymentPlanner _planner = new OffensiveDeploymentPlanner();
        private static BattleField Field => BattleFieldCatalog.ForTerrain(TerrainKind.Pass);

        private static OffensiveGeneral G(string id, GeneralTag[] tags, CombatTier? prow = null, StrategyTier? strat = null)
            => new OffensiveGeneral(new CharacterId(id), F(6, 10), F(7, 10), F(6, 10), GeneralSpecialty.None, tags, prow, strat);

        private static Detachment? Find(IReadOnlyList<Detachment> dets, string id)
        {
            foreach (Detachment d in dets) if (d.Id.Value == id) return d;
            return null;
        }

        [Test]
        public void test_defender_front_prefers_defensive_tag_over_prowess()
        {
            // Arrange：一莽勇之将（战阵绝世但无善守）+ 一善守之将。
            OffensiveGeneral brawler = G("g-brawler", new[] { GeneralTag.Reckless }, CombatTier.Peerless);
            OffensiveGeneral warden = G("g-warden", new[] { GeneralTag.Defender }, CombatTier.Valiant);
            // Act
            var dets = _planner.PlanDefender(new SiegeDefense(600, F(12, 10)), F(7, 10), Field, SubversionEffect.None,
                new[] { brawler, warden });
            // Assert
            Detachment? front = Find(dets, "def-front");
            Assert.That(front, Is.Not.Null);
            Assert.That(front!.General, Is.Not.Null, "守将进战斗：正面支队须带将（不再 null）。");
            Assert.That(front.General!.Character.Value, Is.EqualTo("g-warden"), "善守优先镇正面，压过纯战阵档。");
        }

        [Test]
        public void test_defender_supply_prefers_cunning_general()
        {
            // Arrange
            OffensiveGeneral warden = G("g-warden", new[] { GeneralTag.Defender }, CombatTier.Valiant);
            OffensiveGeneral schemer = G("g-schemer", new[] { GeneralTag.Cunning }, null, StrategyTier.Master);
            // Act
            var dets = _planner.PlanDefender(new SiegeDefense(600, F(12, 10)), F(7, 10), Field, SubversionEffect.None,
                new[] { warden, schemer });
            // Assert
            Detachment? supply = Find(dets, "def-supply");
            Detachment? front = Find(dets, "def-front");
            Assert.That(supply!.General, Is.Not.Null, "粮道支队须带将。");
            Assert.That(supply.General!.Character.Value, Is.EqualTo("g-schemer"), "诡谋护粮道（反伏击/识破）。");
            Assert.That(front!.General!.Character.Value, Is.EqualTo("g-warden"), "善守镇正面，与粮道不重复用人。");
        }

        [Test]
        public void test_no_defenders_yields_null_generals_backward_compat()
        {
            // Act：无名守军（旧路径）。
            var dets = _planner.PlanDefender(new SiegeDefense(600, F(12, 10)), F(7, 10), Field);
            // Assert
            foreach (Detachment d in dets) Assert.That(d.General, Is.Null, "无守将册 → 守方 null（向后兼容）。");
        }

        [Test]
        public void test_defenders_for_real_city_has_named_generals()
        {
            // Arrange / Act：邺城（袁绍）190 具名守将册 → 出征将投影。
            var defenders = PlayableCampaign.DefendersFor(new ThreeKingdom.Domain.City.CityId("city-ye"), 190);
            // Assert
            Assert.That(defenders.Count, Is.GreaterThan(0), "邺城190有具名守将（张郃/田丰/沮授等），非无名守军。");
        }
    }
}
