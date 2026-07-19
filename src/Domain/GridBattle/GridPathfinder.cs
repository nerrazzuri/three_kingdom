using System.Collections.Generic;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>
    /// 确定性整数寻路（ADR-0018 D3 / GDD-028 §3.4，TR-grid-002）：4 向 BFS，避开不可通行（山）与被占格，
    /// 返回从 <paramref name="from"/> 朝 <paramref name="to"/> 的下一步。无路径返回 null（原地待命，不穿障/不瞬移）。
    /// 邻居按固定顺序展开（右/左/下/上）保证同态同解。
    /// </summary>
    public static class GridPathfinder
    {
        // 固定展开顺序 → 确定性。
        private static readonly (int dx, int dy)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        /// <summary>下一步坐标；已在目的地或无路径返回 null。目标格可作为终点即使被占（停在其相邻由调用方处理）。</summary>
        public static GridCoord? NextStep(GridBattleState state, GridCoord from, GridCoord to)
        {
            if (from == to) return null;
            BattleGrid grid = state.Grid;
            if (!grid.Passable(to) && to != from) { /* 目标不可通行：仍尝试逼近其可达邻格 */ }

            var prev = new Dictionary<GridCoord, GridCoord>();
            var seen = new HashSet<GridCoord> { from };
            var queue = new Queue<GridCoord>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                GridCoord cur = queue.Dequeue();
                foreach ((int dx, int dy) in Dirs)
                {
                    var next = new GridCoord(cur.X + dx, cur.Y + dy);
                    if (seen.Contains(next)) continue;
                    if (!grid.Passable(next)) continue;
                    // 被占格不可穿越；但目标格即使被占也允许作为终点（用于逼近）。
                    bool occupied = state.UnitAt(next) != null;
                    if (occupied && next != to) { seen.Add(next); continue; }

                    seen.Add(next);
                    prev[next] = cur;
                    if (next == to)
                        return FirstStep(from, to, prev);
                    queue.Enqueue(next);
                }
            }
            return null;
        }

        private static GridCoord FirstStep(GridCoord from, GridCoord to, Dictionary<GridCoord, GridCoord> prev)
        {
            GridCoord cur = to;
            while (prev.TryGetValue(cur, out GridCoord p) && p != from)
                cur = p;
            return cur; // prev[cur] == from → cur 是第一步
        }
    }
}
