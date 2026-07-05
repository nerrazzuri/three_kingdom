using NUnit.Framework;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// 逐城/地形战场（#3 / ADR-0012 D2）：按目标城地形选战场模板——正面区地形随之而变（坚城得工事之利、
    /// 隘口/渡口/平原/高地则无），侧翼/粮道/掩护/预备四区共用。默认坚城模板与原 Default() 一致（既有战斗测试为证）。
    /// </summary>
    [TestFixture]
    public class BattleFieldCatalogTests
    {
        private static TerrainKind FrontTerrain(TerrainKind kind)
            => BattleFieldCatalog.ForTerrain(kind).ZoneOf(BattleField.Front).Terrain;

        [Test]
        public void test_front_zone_terrain_matches_city_terrain()
        {
            Assert.That(FrontTerrain(TerrainKind.Fortified), Is.EqualTo(TerrainKind.Fortified), "坚城：正面工事硬碰。");
            Assert.That(FrontTerrain(TerrainKind.Pass), Is.EqualTo(TerrainKind.Pass), "隘口：正面即设伏之地。");
            Assert.That(FrontTerrain(TerrainKind.Ford), Is.EqualTo(TerrainKind.Ford), "渡口：正面临水可水火。");
            Assert.That(FrontTerrain(TerrainKind.Plain), Is.EqualTo(TerrainKind.Plain), "平原：正面骑冲无险。");
            Assert.That(FrontTerrain(TerrainKind.Cover), Is.EqualTo(TerrainKind.Cover), "高地林：正面隐蔽夜袭。");
        }

        [Test]
        public void test_only_fortified_front_grants_defense_bonus()
        {
            // 唯坚城正面为 Fortified → 守方得工事加成（RoundResolutionService）；余地形正面皆无此利，攻坚较易。
            Assert.That(FrontTerrain(TerrainKind.Fortified), Is.EqualTo(TerrainKind.Fortified));
            foreach (TerrainKind k in new[] { TerrainKind.Pass, TerrainKind.Ford, TerrainKind.Plain, TerrainKind.Cover })
                Assert.That(FrontTerrain(k), Is.Not.EqualTo(TerrainKind.Fortified), $"{k} 正面非坚城 → 无工事加成。");
        }

        [Test]
        public void test_all_terrains_share_five_zone_skeleton()
        {
            // 区 id 骨架不变（Front/Flank/Supply/Cover/Reserve）→ planner/引擎零改动即支持逐地形战场。
            foreach (TerrainKind k in new[] { TerrainKind.Fortified, TerrainKind.Pass, TerrainKind.Ford, TerrainKind.Plain, TerrainKind.Cover })
            {
                BattleField f = BattleFieldCatalog.ForTerrain(k);
                Assert.That(f.Zones.Count, Is.EqualTo(5), $"{k} 战场 5 区。");
                Assert.That(f.Contains(BattleField.Front) && f.Contains(BattleField.Flank)
                    && f.Contains(BattleField.Supply) && f.Contains(BattleField.Cover) && f.Contains(BattleField.Reserve),
                    Is.True, $"{k} 含标准五区。");
            }
        }
    }
}
