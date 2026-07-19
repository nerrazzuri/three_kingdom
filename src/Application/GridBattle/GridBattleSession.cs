using System;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;

namespace ThreeKingdom.Application.GridBattle
{
    /// <summary>
    /// 格子战斗会话编排（ADR-0009 装配脊梁——只编排不拥规则）：持当前权威态 + 配置 + 敌方AI。
    /// 玩家经 <see cref="SetPlayerDestination"/> 下令（仅己方，防越权）；<see cref="Advance"/> 每段：
    /// 敌AI规划（走同一命令契约、不作弊）→ 引擎纯函数推进 → 更新态/终局。存读档经显式 DTO（ADR-0005）。
    /// </summary>
    public sealed class GridBattleSession
    {
        private readonly EnemyGridAi _ai = new EnemyGridAi();
        private GridBattleState _state;

        /// <summary>当前权威态。</summary>
        public GridBattleState State => _state;
        /// <summary>战斗配置。</summary>
        public GridBattleConfig Config { get; }
        /// <summary>当前终局。</summary>
        public GridBattleOutcome Outcome { get; private set; }
        /// <summary>是否已终局。</summary>
        public bool IsOver => Outcome != GridBattleOutcome.Ongoing;

        public GridBattleSession(GridBattleState initial, GridBattleConfig? config = null)
        {
            _state = initial ?? throw new ArgumentNullException(nameof(initial));
            Config = config ?? GridBattleConfig.Default();
            Outcome = GridBattleEngine.EvaluateOutcome(_state);
        }

        /// <summary>玩家设目的地（命令契约）：仅允许己方存活单位，敌方/未知返回 UnitNotFound（防越权）。</summary>
        public GridCommandResult SetPlayerDestination(BattleUnitId unitId, GridCoord dest)
        {
            foreach (GridUnit u in _state.Units)
                if (u.Alive && u.Id == unitId && u.Side != GridSide.Player)
                    return GridCommandResult.UnitNotFound; // 不得指挥敌军
            return GridBattleEngine.SetDestination(_state, unitId, dest);
        }

        /// <summary>推进一个时间段：敌AI规划下达 → 引擎推进 → 更新态与终局。返回段结果（含半路遭遇）。终局后调用抛异常（调用方须先查 <see cref="IsOver"/>）。</summary>
        public SegmentResult Advance()
        {
            if (IsOver) throw new InvalidOperationException("战斗已终局，不可再推进。");
            _ai.Apply(_state, GridSide.Enemy, Config);           // 敌方AI（反全知投影→效用→同命令契约）
            SegmentResult r = GridBattleEngine.Advance(_state, Config);
            _state = r.State;
            Outcome = r.Outcome;
            return r;
        }

        /// <summary>
        /// 应用半路遭遇的临机抉择（GDD-028 §3.5，仅己方）：继续=保持目的地 / 据守=目的地设为当前格 /
        /// 后撤=朝远离最近敌退一格。经命令契约（设目的地，下段推进生效）。
        /// </summary>
        public GridCommandResult ApplyEncounterChoice(BattleUnitId unitId, EncounterChoice choice)
        {
            GridUnit unit = null;
            foreach (GridUnit u in _state.Units)
                if (u.Alive && u.Id == unitId) { unit = u; break; }
            if (unit == null || unit.Side != GridSide.Player) return GridCommandResult.UnitNotFound;

            switch (choice)
            {
                case EncounterChoice.Continue:
                    return GridCommandResult.Ok; // 保持原目的地
                case EncounterChoice.Hold:
                    return GridBattleEngine.SetDestination(_state, unitId, unit.Position);
                case EncounterChoice.Retreat:
                    GridCoord? back = StepAwayFromNearestFoe(unit);
                    return GridBattleEngine.SetDestination(_state, unitId, back ?? unit.Position);
                default:
                    return GridCommandResult.Ok;
            }
        }

        private GridCoord? StepAwayFromNearestFoe(GridUnit unit)
        {
            GridUnit nearest = null;
            int bestD = int.MaxValue;
            foreach (GridUnit e in _state.Units)
            {
                if (!e.Alive || e.Side == unit.Side) continue;
                int d = GridCoord.Manhattan(unit.Position, e.Position);
                if (d < bestD) { bestD = d; nearest = e; }
            }
            if (nearest == null) return null;
            int dx = Math.Sign(unit.Position.X - nearest.Position.X);
            int dy = Math.Sign(unit.Position.Y - nearest.Position.Y);
            var diag = new GridCoord(unit.Position.X + dx, unit.Position.Y + dy);
            if (diag != unit.Position && _state.Grid.Passable(diag) && _state.UnitAt(diag) == null) return diag;
            if (dx != 0)
            {
                var ax = new GridCoord(unit.Position.X + dx, unit.Position.Y);
                if (_state.Grid.Passable(ax) && _state.UnitAt(ax) == null) return ax;
            }
            if (dy != 0)
            {
                var ay = new GridCoord(unit.Position.X, unit.Position.Y + dy);
                if (_state.Grid.Passable(ay) && _state.UnitAt(ay) == null) return ay;
            }
            return null;
        }

        /// <summary>战中存档（显式 DTO，ADR-0005）。</summary>
        public GridBattleSnapshot Snapshot() => GridBattleSnapshotCodec.Capture(_state);

        /// <summary>读档续战：由 DTO 复原态与终局。</summary>
        public static GridBattleSession Restore(GridBattleSnapshot snap, GridBattleConfig? config = null)
            => new GridBattleSession(GridBattleSnapshotCodec.Restore(snap), config);
    }
}
