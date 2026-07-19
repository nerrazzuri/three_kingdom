using System;
using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>
    /// 格子战斗权威态（ADR-0018 D1 / GDD-028）：网格 + 各单位位姿 + 世界钟 + 双方补给 + 粮仓焚毁标记。
    /// 纯 Domain、引擎无关。<see cref="Clone"/> 供 <see cref="GridBattleEngine.Advance"/> 于克隆态推进（原态不变）；
    /// <see cref="Hash"/> 为确定性状态哈希（同种子+同命令序→同哈希，可回放，ADR-0004）。
    /// </summary>
    public sealed class GridBattleState
    {
        private readonly List<GridUnit> _units;

        /// <summary>战场网格（不可变）。</summary>
        public BattleGrid Grid { get; }
        /// <summary>世界钟（GDD-001）。</summary>
        public WorldTime Clock { get; internal set; }
        /// <summary>玩家补给度。</summary>
        public int PlayerSupply { get; internal set; }
        /// <summary>敌方补给度。</summary>
        public int EnemySupply { get; internal set; }
        /// <summary>玩家粮仓是否已焚。</summary>
        public bool PlayerGranaryBurned { get; internal set; }
        /// <summary>敌方粮仓是否已焚。</summary>
        public bool EnemyGranaryBurned { get; internal set; }

        /// <summary>全部部队（含已溃灭？——引擎每段末剔除死者，故常态皆存活）。</summary>
        public IReadOnlyList<GridUnit> Units => _units;

        public GridBattleState(BattleGrid grid, IEnumerable<GridUnit> units, WorldTime clock,
            int playerSupply, int enemySupply, bool playerGranaryBurned = false, bool enemyGranaryBurned = false)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _units = (units ?? throw new ArgumentNullException(nameof(units))).ToList();
            Clock = clock;
            PlayerSupply = playerSupply;
            EnemySupply = enemySupply;
            PlayerGranaryBurned = playerGranaryBurned;
            EnemyGranaryBurned = enemyGranaryBurned;
        }

        /// <summary>某方补给度。</summary>
        public int SupplyOf(GridSide side) => side == GridSide.Player ? PlayerSupply : EnemySupply;
        /// <summary>某方粮仓是否已焚。</summary>
        public bool GranaryBurnedOf(GridSide side) => side == GridSide.Player ? PlayerGranaryBurned : EnemyGranaryBurned;

        /// <summary>某格上的存活单位（无则 null）。</summary>
        public GridUnit? UnitAt(GridCoord c)
        {
            foreach (GridUnit u in _units)
                if (u.Alive && u.Position == c) return u;
            return null;
        }

        /// <summary>某方存活单位。</summary>
        public IEnumerable<GridUnit> AliveOf(GridSide side) => _units.Where(u => u.Alive && u.Side == side);

        /// <summary>规范序（Side→Id 序数）的存活单位快照——确定性遍历用。</summary>
        public IReadOnlyList<GridUnit> OrderedAlive()
            => _units.Where(u => u.Alive)
                     .OrderBy(u => (int)u.Side)
                     .ThenBy(u => u.Id, Comparer<BattleUnitId>.Default)
                     .ToList();

        internal void AddUnit(GridUnit u) => _units.Add(u);
        internal void RemoveDead() => _units.RemoveAll(u => !u.Alive);

        /// <summary>深拷贝（单位深拷，网格不可变共享）。</summary>
        public GridBattleState Clone()
            => new GridBattleState(Grid, _units.Select(u => u.Clone()), Clock,
                PlayerSupply, EnemySupply, PlayerGranaryBurned, EnemyGranaryBurned);

        /// <summary>
        /// 确定性状态哈希（FNV-1a，64 位）：时钟 + 补给 + 焚毁标记 + 各存活单位（规范序）位姿/兵力/目的地。
        /// 同态→同哈希；用于回放/存档一致性断言（ADR-0004/0005）。纯整数。
        /// </summary>
        public long Hash()
        {
            unchecked
            {
                const ulong prime = 1099511628211UL;
                ulong h = 14695981039346656037UL;
                void Mix(long v) { h = (h ^ (ulong)v) * prime; }

                Mix(Clock.AbsoluteIndex);
                Mix(PlayerSupply);
                Mix(EnemySupply);
                Mix(PlayerGranaryBurned ? 1 : 0);
                Mix(EnemyGranaryBurned ? 1 : 0);
                foreach (GridUnit u in OrderedAlive())
                {
                    Mix((int)u.Side);
                    foreach (char ch in u.Id.Value) Mix(ch);
                    Mix((int)u.Kind);
                    Mix(u.Position.X); Mix(u.Position.Y);
                    Mix(u.Strength);
                    Mix(u.Destination.HasValue ? 1 : 0);
                    if (u.Destination.HasValue) { Mix(u.Destination.Value.X); Mix(u.Destination.Value.Y); }
                }
                return (long)h;
            }
        }
    }
}
