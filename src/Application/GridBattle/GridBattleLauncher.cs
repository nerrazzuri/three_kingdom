using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.GridBattle
{
    /// <summary>一支入场部队的编成（兵种 + 兵力 + 攻 + 名）——战役侧输入（如出征六维准备折算，GDD-019）。</summary>
    public sealed class ForceUnit
    {
        public string Id { get; }
        public TroopKind Kind { get; }
        public int Strength { get; }
        public int Attack { get; }
        public string Name { get; }

        public ForceUnit(string id, TroopKind kind, int strength, int attack, string name = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ForceUnit.Id 不可为空。", nameof(id));
            Id = id; Kind = kind; Strength = strength; Attack = attack; Name = string.IsNullOrEmpty(name) ? id : name;
        }
    }

    /// <summary>
    /// 格子战斗装配入口（ADR-0009 装配脊梁 seam）：由双方兵力编成 + 地图建一局 <see cref="GridBattleSession"/>。
    /// 玩家部队部署在己方粮仓周边可通行格，敌方部署在敌仓周边并以趋玩家粮仓为初始目的地。确定性布放（同输入同摆位）。
    /// 供战役接线（从出征/守城把编成喂进来）复用；深度接入 CampaignSession 存档信封为后续。
    /// </summary>
    public static class GridBattleLauncher
    {
        public static GridBattleSession Create(
            BattleGrid grid,
            IReadOnlyList<ForceUnit> playerForce,
            IReadOnlyList<ForceUnit> enemyForce,
            int playerSupply = 100,
            int enemySupply = 100,
            GridBattleConfig config = null)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (playerForce == null) throw new ArgumentNullException(nameof(playerForce));
            if (enemyForce == null) throw new ArgumentNullException(nameof(enemyForce));

            var units = new List<GridUnit>();
            var occupied = new HashSet<GridCoord>();
            PlaceForce(grid, playerForce, GridSide.Player, grid.PlayerGranary, destination: null, units, occupied);
            // 敌方初始目的地=玩家粮仓（推进/断粮方向）；后续敌AI每段自行重规划。
            PlaceForce(grid, enemyForce, GridSide.Enemy, grid.EnemyGranary, destination: grid.PlayerGranary, units, occupied);

            var state = new GridBattleState(grid, units, new WorldTime(0, DaySegment.Dawn), playerSupply, enemySupply);
            return new GridBattleSession(state, config);
        }

        // 在 anchor 周边按距离/规范序确定性铺放（避开山与已占）。
        private static void PlaceForce(BattleGrid grid, IReadOnlyList<ForceUnit> force, GridSide side,
            GridCoord anchor, GridCoord? destination, List<GridUnit> units, HashSet<GridCoord> occupied)
        {
            var candidates = CellsByDistance(grid, anchor);
            int ci = 0;
            foreach (ForceUnit f in force)
            {
                GridCoord? cell = null;
                while (ci < candidates.Count)
                {
                    GridCoord c = candidates[ci++];
                    if (!occupied.Contains(c)) { cell = c; break; }
                }
                if (cell == null) throw new InvalidOperationException("地图可通行格不足以容纳编成。");
                occupied.Add(cell.Value);
                units.Add(new GridUnit(new BattleUnitId(f.Id), side, f.Kind, cell.Value,
                    f.Strength, f.Strength, f.Attack, destination, f.Name));
            }
        }

        // 全部可通行格，按 (到 anchor 的曼哈顿距离, 坐标规范序) 排序——确定性。
        private static List<GridCoord> CellsByDistance(BattleGrid grid, GridCoord anchor)
        {
            var cells = new List<GridCoord>();
            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    var c = new GridCoord(x, y);
                    if (grid.Passable(c)) cells.Add(c);
                }
            cells.Sort((a, b) =>
            {
                int da = GridCoord.Manhattan(a, anchor), db = GridCoord.Manhattan(b, anchor);
                int cmp = da.CompareTo(db);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });
            return cells;
        }
    }
}
