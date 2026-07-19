using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.GridBattle
{
    /// <summary>
    /// 格子战斗态 ⇄ 存档 DTO 互转（ADR-0005 显式 DTO；无 Unity 序列化）。
    /// <see cref="Capture"/>/<see cref="Restore"/> round-trip 保证权威态一致、确定性哈希不变（GDD-028 AC-10）。
    /// </summary>
    public static class GridBattleSnapshotCodec
    {
        /// <summary>由权威态生成存档 DTO。</summary>
        public static GridBattleSnapshot Capture(GridBattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            BattleGrid grid = state.Grid;
            var terrain = new int[grid.Width * grid.Height];
            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                    terrain[y * grid.Width + x] = (int)grid.TerrainAt(new GridCoord(x, y));

            var snap = new GridBattleSnapshot
            {
                Version = 1,
                Width = grid.Width,
                Height = grid.Height,
                Terrain = terrain,
                PlayerGranaryX = grid.PlayerGranary.X,
                PlayerGranaryY = grid.PlayerGranary.Y,
                EnemyGranaryX = grid.EnemyGranary.X,
                EnemyGranaryY = grid.EnemyGranary.Y,
                Day = state.Clock.Day,
                Segment = (int)state.Clock.Segment,
                PlayerSupply = state.PlayerSupply,
                EnemySupply = state.EnemySupply,
                PlayerGranaryBurned = state.PlayerGranaryBurned,
                EnemyGranaryBurned = state.EnemyGranaryBurned,
            };
            foreach (GridUnit u in state.Units)
            {
                snap.Units.Add(new GridUnitDto
                {
                    Id = u.Id.Value,
                    Side = (int)u.Side,
                    Kind = (int)u.Kind,
                    X = u.Position.X,
                    Y = u.Position.Y,
                    Strength = u.Strength,
                    MaxStrength = u.MaxStrength,
                    Attack = u.Attack,
                    HasDestination = u.Destination.HasValue,
                    DestX = u.Destination?.X ?? 0,
                    DestY = u.Destination?.Y ?? 0,
                    Name = u.Name,
                });
            }
            return snap;
        }

        /// <summary>由存档 DTO 复原权威态。</summary>
        public static GridBattleState Restore(GridBattleSnapshot snap)
        {
            if (snap == null) throw new ArgumentNullException(nameof(snap));
            var cells = new TerrainKind[snap.Terrain.Length];
            for (int i = 0; i < cells.Length; i++) cells[i] = (TerrainKind)snap.Terrain[i];
            var grid = new BattleGrid(snap.Width, snap.Height, cells,
                new GridCoord(snap.PlayerGranaryX, snap.PlayerGranaryY),
                new GridCoord(snap.EnemyGranaryX, snap.EnemyGranaryY));

            var units = new List<GridUnit>();
            foreach (GridUnitDto d in snap.Units)
            {
                GridCoord? dest = d.HasDestination ? new GridCoord(d.DestX, d.DestY) : (GridCoord?)null;
                units.Add(new GridUnit(new BattleUnitId(d.Id), (GridSide)d.Side, (TroopKind)d.Kind,
                    new GridCoord(d.X, d.Y), d.Strength, d.MaxStrength, d.Attack, dest, d.Name));
            }
            return new GridBattleState(grid, units, new WorldTime(snap.Day, (DaySegment)snap.Segment),
                snap.PlayerSupply, snap.EnemySupply, snap.PlayerGranaryBurned, snap.EnemyGranaryBurned);
        }
    }
}
