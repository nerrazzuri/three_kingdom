using System.Collections.Generic;

namespace ThreeKingdom.Application.GridBattle
{
    /// <summary>格子战斗单位存档 DTO（显式版本化 DTO，ADR-0005）。</summary>
    public sealed class GridUnitDto
    {
        public string Id { get; set; } = "";
        public int Side { get; set; }
        public int Kind { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Strength { get; set; }
        public int MaxStrength { get; set; }
        public int Attack { get; set; }
        public bool HasDestination { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// 格子战斗态存档信封 DTO（ADR-0005 显式版本化 DTO，非 Unity 序列化）。捕获权威态全部字段——
    /// 战中存档续战 round-trip 一致、确定性哈希不变（GDD-028 AC-10）。经 codec 与 <c>GridBattleState</c> 互转。
    /// </summary>
    public sealed class GridBattleSnapshot
    {
        /// <summary>DTO 版本（迁移链用，ADR-0005）。</summary>
        public int Version { get; set; } = 1;

        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>行优先地形（(int)TerrainKind）。</summary>
        public int[] Terrain { get; set; } = System.Array.Empty<int>();
        public int PlayerGranaryX { get; set; }
        public int PlayerGranaryY { get; set; }
        public int EnemyGranaryX { get; set; }
        public int EnemyGranaryY { get; set; }

        public int Day { get; set; }
        public int Segment { get; set; }
        public int PlayerSupply { get; set; }
        public int EnemySupply { get; set; }
        public bool PlayerGranaryBurned { get; set; }
        public bool EnemyGranaryBurned { get; set; }

        public List<GridUnitDto> Units { get; set; } = new List<GridUnitDto>();
    }
}
