using System.Collections.Generic;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>格子战斗展示文本（表现层，不影响权威）。</summary>
    public static class GridBattleText
    {
        public static string Terrain(TerrainKind t) => t switch
        {
            TerrainKind.Mountain => "山",
            TerrainKind.Pass => "隘",
            TerrainKind.Forest => "林",
            TerrainKind.Granary => "仓",
            _ => "",
        };

        public static string Troop(TroopKind k) => k switch
        {
            TroopKind.Cavalry => "骑",
            TroopKind.Infantry => "步",
            TroopKind.Archer => "弓",
            _ => "?",
        };

        public static string Segment(DaySegment s) => s switch
        {
            DaySegment.Dawn => "黎明",
            DaySegment.Day => "白昼",
            DaySegment.Dusk => "黄昏",
            DaySegment.Night => "夜",
            _ => s.ToString(),
        };

        public static string Outcome(GridBattleOutcome o) => o switch
        {
            GridBattleOutcome.PlayerVictory => "全胜！",
            GridBattleOutcome.EnemyVictory => "我军覆没……",
            _ => "鏖战中",
        };
    }

    /// <summary>一格的展示投影（地形 + 字形）。</summary>
    public sealed class GridCellView
    {
        public int X { get; }
        public int Y { get; }
        public TerrainKind Terrain { get; }
        public string Glyph { get; }
        /// <summary>是否玩家/敌方粮仓（供 UI 标阵营边）。</summary>
        public bool IsPlayerGranary { get; }
        public bool IsEnemyGranary { get; }

        internal GridCellView(int x, int y, TerrainKind terrain, bool playerGran, bool enemyGran)
        {
            X = x; Y = y; Terrain = terrain; Glyph = GridBattleText.Terrain(terrain);
            IsPlayerGranary = playerGran; IsEnemyGranary = enemyGran;
        }
    }

    /// <summary>一支部队的展示投影（反全知：敌方林中藏兵不入本视图）。</summary>
    public sealed class GridUnitView
    {
        public string Id { get; }
        public bool IsPlayer { get; }
        public string KindGlyph { get; }
        public int X { get; }
        public int Y { get; }
        public int Strength { get; }
        public int MaxStrength { get; }
        public bool HasDestination { get; }
        public int DestX { get; }
        public int DestY { get; }
        public bool InCover { get; }
        public string Name { get; }

        internal GridUnitView(GridUnit u, bool inCover)
        {
            Id = u.Id.Value; IsPlayer = u.Side == GridSide.Player; KindGlyph = GridBattleText.Troop(u.Kind);
            X = u.Position.X; Y = u.Position.Y; Strength = u.Strength; MaxStrength = u.MaxStrength;
            HasDestination = u.Destination.HasValue; DestX = u.Destination?.X ?? 0; DestY = u.Destination?.Y ?? 0;
            InCover = inCover; Name = u.Name;
        }
    }

    /// <summary>
    /// 格子战斗展示视图（GDD-028 / ADR-0018）：网格地形 + 部队投影 + 补给 + 时钟 + 终局。不可变、纯投影。
    /// <b>反全知</b>：敌方处林地（藏兵）不入本视图（玩家不见未侦破伏兵）；己方全见。
    /// </summary>
    public sealed class GridBattleView
    {
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<GridCellView> Cells { get; }
        public IReadOnlyList<GridUnitView> Units { get; }
        public int PlayerSupply { get; }
        public int EnemySupply { get; }
        public string ClockLabel { get; }
        public string OutcomeLabel { get; }
        public bool IsOver { get; }

        private GridBattleView(int w, int h, IReadOnlyList<GridCellView> cells, IReadOnlyList<GridUnitView> units,
            int pSup, int eSup, string clock, string outcome, bool isOver)
        {
            Width = w; Height = h; Cells = cells; Units = units;
            PlayerSupply = pSup; EnemySupply = eSup; ClockLabel = clock; OutcomeLabel = outcome; IsOver = isOver;
        }

        /// <summary>由战斗态构造玩家视角投影（敌方林中藏兵排除）。</summary>
        public static GridBattleView From(GridBattleState state, GridBattleOutcome outcome)
        {
            BattleGrid grid = state.Grid;
            var cells = new List<GridCellView>(grid.Width * grid.Height);
            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    var c = new GridCoord(x, y);
                    cells.Add(new GridCellView(x, y, grid.TerrainAt(c),
                        grid.IsGranaryOf(GridSide.Player, c), grid.IsGranaryOf(GridSide.Enemy, c)));
                }

            var units = new List<GridUnitView>();
            foreach (GridUnit u in state.OrderedAlive())
            {
                bool inCover = grid.TerrainAt(u.Position) == TerrainKind.Forest;
                if (u.Side == GridSide.Enemy && inCover) continue; // 反全知：敌方藏兵不见
                units.Add(new GridUnitView(u, inCover));
            }

            string clock = $"第 {state.Clock.Day + 1} 日 · {GridBattleText.Segment(state.Clock.Segment)}";
            return new GridBattleView(grid.Width, grid.Height, cells, units,
                state.PlayerSupply, state.EnemySupply, clock,
                GridBattleText.Outcome(outcome), outcome != GridBattleOutcome.Ongoing);
        }
    }
}
