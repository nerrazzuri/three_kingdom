using System;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>
    /// 战场网格（ADR-0018 D2 / GDD-028 §3.1）：宽×高整数格 + 每格地形 + 双方粮仓坐标。
    /// 不可变、引擎无关、数据驱动（ADR-0003 构建期不可变配置）。山格不可通行，逼出隘口。
    /// </summary>
    public sealed class BattleGrid
    {
        private readonly TerrainKind[] _cells; // 行优先：index = y*Width + x

        /// <summary>宽（列数，&gt;0）。</summary>
        public int Width { get; }
        /// <summary>高（行数，&gt;0）。</summary>
        public int Height { get; }
        /// <summary>玩家粮仓坐标。</summary>
        public GridCoord PlayerGranary { get; }
        /// <summary>敌方粮仓坐标。</summary>
        public GridCoord EnemyGranary { get; }

        public BattleGrid(int width, int height, TerrainKind[] cells, GridCoord playerGranary, GridCoord enemyGranary)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (cells.Length != width * height) throw new ArgumentException("cells 长度须等于 width*height。", nameof(cells));
            Width = width;
            Height = height;
            _cells = (TerrainKind[])cells.Clone();
            PlayerGranary = playerGranary;
            EnemyGranary = enemyGranary;
            if (!InBounds(playerGranary) || !InBounds(enemyGranary))
                throw new ArgumentException("粮仓坐标越界。");
        }

        /// <summary>坐标是否在场内。</summary>
        public bool InBounds(GridCoord c) => c.X >= 0 && c.Y >= 0 && c.X < Width && c.Y < Height;

        /// <summary>某格地形（越界视作山/不可通行）。</summary>
        public TerrainKind TerrainAt(GridCoord c) => InBounds(c) ? _cells[c.Y * Width + c.X] : TerrainKind.Mountain;

        /// <summary>是否可通行（非山、且在场内）。</summary>
        public bool Passable(GridCoord c) => InBounds(c) && _cells[c.Y * Width + c.X] != TerrainKind.Mountain;

        /// <summary>该坐标是否某方粮仓。</summary>
        public bool IsGranaryOf(GridSide side, GridCoord c) => c == (side == GridSide.Player ? PlayerGranary : EnemyGranary);

        /// <summary>
        /// 从字符地图构建（测试/数据便利）：每行等长字符串，行数=高、列数=宽。
        /// 字符映射：'.'平地 '^'山 'P'隘口 'F'林地 'G'玩家粮仓 'g'敌方粮仓。未知字符按平地。
        /// </summary>
        public static BattleGrid FromRows(string[] rows)
        {
            if (rows == null || rows.Length == 0) throw new ArgumentException("rows 不可为空。", nameof(rows));
            int h = rows.Length, w = rows[0].Length;
            var cells = new TerrainKind[w * h];
            GridCoord playerGran = default, enemyGran = default;
            bool hasP = false, hasE = false;
            for (int y = 0; y < h; y++)
            {
                if (rows[y].Length != w) throw new ArgumentException("所有行须等长。", nameof(rows));
                for (int x = 0; x < w; x++)
                {
                    char ch = rows[y][x];
                    TerrainKind t = ch switch
                    {
                        '^' => TerrainKind.Mountain,
                        'P' => TerrainKind.Pass,
                        'F' => TerrainKind.Forest,
                        'G' => TerrainKind.Granary,
                        'g' => TerrainKind.Granary,
                        _ => TerrainKind.Plain,
                    };
                    cells[y * w + x] = t;
                    if (ch == 'G') { playerGran = new GridCoord(x, y); hasP = true; }
                    if (ch == 'g') { enemyGran = new GridCoord(x, y); hasE = true; }
                }
            }
            if (!hasP || !hasE) throw new ArgumentException("字符地图须各含一个 'G'（玩家仓）与 'g'（敌方仓）。", nameof(rows));
            return new BattleGrid(w, h, cells, playerGran, enemyGran);
        }
    }
}
