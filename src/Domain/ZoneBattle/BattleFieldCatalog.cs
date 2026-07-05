using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 逐城/地形战场目录（#3 / ADR-0012 D2「预留每城场景自定义」落地）：按<b>目标城地形</b>选战场模板。
    /// 侧翼/粮道/掩护/预备四区共用（<see cref="BattleField.SideZones"/>），仅<b>正面区</b>随地形而异——
    /// 决定攻坚难度（地形 Fortified → 守方得工事加成）与正面可涌现的兵法（隘口设伏 / 渡口水火 / 平原骑冲 / 坚城诈降）。
    /// 全确定性、数据驱动；新增地形只需加一条正面区（引擎/planner 零改动，因区 id 骨架不变）。
    /// </summary>
    public static class BattleFieldCatalog
    {
        /// <summary>按地形取战场模板（未知地形回退坚城模板）。</summary>
        public static BattleField ForTerrain(TerrainKind terrain) => terrain switch
        {
            TerrainKind.Fortified => BattleField.Default(),                 // 坚城：城门硬碰 + 诈降（含工事加成）
            TerrainKind.Pass => BattleField.Compose(PassFront()),           // 隘口：正面即设伏之地
            TerrainKind.Ford => BattleField.Compose(FordFront()),           // 渡口：正面水火可施（水淹/火船）
            TerrainKind.Plain => BattleField.Compose(PlainFront()),         // 平原：正面骑冲，无工事之利
            TerrainKind.Cover => BattleField.Compose(CoverFront()),         // 高地林：正面隐蔽夜袭
            _ => BattleField.Default(),
        };

        /// <summary>隘口正面区：地形 Pass（无工事加成），正面即可设伏诱敌（假退伏击禀赋）。</summary>
        private static Zone PassFront()
            => new Zone(BattleField.Front, TerrainKind.Pass, new[]
            {
                TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued, TacticCondition.AmbushSurprise,
            }, softCapacity: 600);

        /// <summary>渡口正面区：地形 Ford（无工事加成），临水可施水攻/火烧战船（赤壁式）。</summary>
        private static Zone FordFront()
            => new Zone(BattleField.Front, TerrainKind.Ford, new[]
            {
                TacticCondition.EnemyInLowGround, TacticCondition.WaterworksHeld, TacticCondition.FloodReleased,
                TacticCondition.DryField, TacticCondition.EnemyExposedToFire, TacticCondition.FireIgnited,
            }, softCapacity: 500);

        /// <summary>平原正面区：地形 Plain（无工事加成，利骑冲），正面硬撼——但敌溃散更快（无险可守）。</summary>
        private static Zone PlainFront()
            => new Zone(BattleField.Front, TerrainKind.Plain, new[]
            {
                TacticCondition.EnemyCohesionCrossedThreshold,
            }, softCapacity: 800);

        /// <summary>高地/林正面区：地形 Cover（无工事加成），正面即可隐蔽夜袭。</summary>
        private static Zone CoverFront()
            => new Zone(BattleField.Front, TerrainKind.Cover, new[]
            {
                TacticCondition.IsNight, TacticCondition.StealthSuccess, TacticCondition.DefenderUnaware, TacticCondition.RaiderDisciplineMet,
            }, softCapacity: 500);
    }
}
