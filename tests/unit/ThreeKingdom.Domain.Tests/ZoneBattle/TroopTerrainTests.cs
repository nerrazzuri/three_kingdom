using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// 兵种×地形战力杠杆（W4 #11 / ADR-0011 杠杆非克制）：合地形之兵种增益、逆地形受抑——
    /// 骑利平原、水利渡口、骑不得展于隘口/林莽/坚城、步利坚城。非石头剪子布硬克制。
    /// </summary>
    [TestFixture]
    public class TroopTerrainTests
    {
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;
        private static TroopComposition All(TroopType t) => new TroopComposition(new Dictionary<TroopType, int> { [t] = 1000 });

        [Test]
        public void test_cavalry_favored_on_plain()
        {
            FixedPoint cav = Cfg.TroopTerrainMul(All(TroopType.Cavalry), TerrainKind.Plain);
            FixedPoint inf = Cfg.TroopTerrainMul(All(TroopType.Infantry), TerrainKind.Plain);
            Assert.That(cav, Is.GreaterThan(FixedPoint.One), "平原利骑冲。");
            Assert.That(cav, Is.GreaterThan(inf), "平原上骑胜于步。");
        }

        [Test]
        public void test_cavalry_hampered_in_pass_and_infantry_favored()
        {
            FixedPoint cav = Cfg.TroopTerrainMul(All(TroopType.Cavalry), TerrainKind.Pass);
            FixedPoint inf = Cfg.TroopTerrainMul(All(TroopType.Infantry), TerrainKind.Pass);
            Assert.That(cav, Is.LessThan(FixedPoint.One), "隘口骑不得展。");
            Assert.That(inf, Is.GreaterThan(cav), "隘口步战胜骑。");
        }

        [Test]
        public void test_marine_favored_at_ford()
        {
            FixedPoint marine = Cfg.TroopTerrainMul(All(TroopType.Marine), TerrainKind.Ford);
            FixedPoint inf = Cfg.TroopTerrainMul(All(TroopType.Infantry), TerrainKind.Ford);
            Assert.That(marine, Is.GreaterThan(FixedPoint.One), "渡口利水战。");
            Assert.That(marine, Is.GreaterThan(inf), "渡口水军胜步。");
        }

        [Test]
        public void test_infantry_favored_in_fortified()
        {
            FixedPoint inf = Cfg.TroopTerrainMul(All(TroopType.Infantry), TerrainKind.Fortified);
            Assert.That(inf, Is.GreaterThan(FixedPoint.One), "坚城步战守成。");
        }

        [Test]
        public void test_empty_composition_is_neutral()
        {
            Assert.That(Cfg.TroopTerrainMul(new TroopComposition(new Dictionary<TroopType, int>()), TerrainKind.Plain),
                Is.EqualTo(FixedPoint.One), "无兵种编成 → 中性。");
        }
    }
}
