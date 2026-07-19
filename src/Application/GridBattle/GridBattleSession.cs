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

        /// <summary>战中存档（显式 DTO，ADR-0005）。</summary>
        public GridBattleSnapshot Snapshot() => GridBattleSnapshotCodec.Capture(_state);

        /// <summary>读档续战：由 DTO 复原态与终局。</summary>
        public static GridBattleSession Restore(GridBattleSnapshot snap, GridBattleConfig? config = null)
            => new GridBattleSession(GridBattleSnapshotCodec.Restore(snap), config);
    }
}
