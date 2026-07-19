using System;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>交战方（GDD-028 / ADR-0018）。序数固定，用于确定性规范序遍历。</summary>
    public enum GridSide
    {
        /// <summary>玩家方。</summary>
        Player = 0,
        /// <summary>敌方。</summary>
        Enemy = 1,
    }

    /// <summary>格子地形（ADR-0018 D2）。山为不可通行，逼出隘口这一通道。</summary>
    public enum TerrainKind
    {
        /// <summary>平地：可通行，无特性。</summary>
        Plain = 0,
        /// <summary>山：不可通行。</summary>
        Mountain = 1,
        /// <summary>隘口：可通行的狭窄通道（伏击触发地形）。</summary>
        Pass = 2,
        /// <summary>林地：可通行，藏兵/伏击发起地形。</summary>
        Forest = 3,
        /// <summary>粮仓：可通行的补给源（归属见 BattleGrid 粮仓坐标）。</summary>
        Granary = 4,
    }

    /// <summary>兵种（ADR-0018 D3）。速度/射程数据驱动（见 GridBattleConfig），兵种差异非克制三角（ADR-0011）。</summary>
    public enum TroopKind
    {
        /// <summary>骑兵：速度高，近战。</summary>
        Cavalry = 0,
        /// <summary>步兵：速度低，兵厚，近战。</summary>
        Infantry = 1,
        /// <summary>弓兵：速度低，可隔格放箭（射程 &gt; 1）。</summary>
        Archer = 2,
    }

    /// <summary>格子战斗终局（GDD-028 §3.10）。</summary>
    public enum GridBattleOutcome
    {
        /// <summary>未分胜负，继续推进。</summary>
        Ongoing = 0,
        /// <summary>玩家胜（敌方全灭）。</summary>
        PlayerVictory = 1,
        /// <summary>敌方胜（玩家全灭）。</summary>
        EnemyVictory = 2,
    }

    /// <summary><see cref="GridSide"/> 辅助。</summary>
    public static class GridSideExtensions
    {
        /// <summary>对立方。</summary>
        public static GridSide Opponent(this GridSide side) => side == GridSide.Player ? GridSide.Enemy : GridSide.Player;
    }

    /// <summary>格子坐标（整数格，ADR-0004 无 float）。行列序数比较，用于确定性规范序。</summary>
    public readonly struct GridCoord : IEquatable<GridCoord>, IComparable<GridCoord>
    {
        /// <summary>列（X，≥0）。</summary>
        public int X { get; }
        /// <summary>行（Y，≥0）。</summary>
        public int Y { get; }

        public GridCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>切比雪夫距离（含斜角，用于"相邻格"交战/伏击/火攻判定）。</summary>
        public static int Chebyshev(GridCoord a, GridCoord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        /// <summary>曼哈顿距离。</summary>
        public static int Manhattan(GridCoord a, GridCoord b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        public int CompareTo(GridCoord other)
        {
            int c = Y.CompareTo(other.Y);
            return c != 0 ? c : X.CompareTo(other.X);
        }

        public bool Equals(GridCoord other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is GridCoord o && Equals(o);
        public override int GetHashCode() => (X * 397) ^ Y;
        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
        public override string ToString() => $"({X},{Y})";
    }
}
