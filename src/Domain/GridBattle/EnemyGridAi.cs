using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>敌AI一步规划：给某单位派一个目的地。</summary>
    public readonly struct AiOrder
    {
        public BattleUnitId UnitId { get; }
        public GridCoord Destination { get; }
        public AiOrder(BattleUnitId unitId, GridCoord destination) { UnitId = unitId; Destination = destination; }
    }

    /// <summary>
    /// 敌方格子AI（特化 ADR-0013 到格子动作空间）：<b>只读 <see cref="AiGridView"/> 反全知投影，不读真值</b>；
    /// <b>确定性效用</b>取舍目的地；<b>同规则不作弊</b>（经 <see cref="GridBattleEngine.SetDestination"/> 下达，走同一命令契约）。
    /// MVP 为确定性 argmax（无 softmax/记忆——ADR-0013 深度分期迭代）：
    /// 补给告急→退守己仓；骑兵→突袭敌粮仓断粮；余者→逼近最近可见敌。无可见敌则趋敌粮仓（推进）。
    /// </summary>
    public sealed class EnemyGridAi
    {
        /// <summary>为 AI 各单位规划目的地（纯函数，确定性；id 序数决胜）。</summary>
        public IReadOnlyList<AiOrder> Plan(AiGridView view, GridBattleConfig config)
        {
            var orders = new List<AiOrder>();
            bool starving = view.OwnSupply < config.SupplyLowThreshold;
            foreach (AiUnitView u in view.OwnUnits)
            {
                GridCoord dest;
                if (starving)
                    dest = view.OwnGranary;                       // 补给告急：退守己仓（回补/防断粮）
                else if (u.Kind == TroopKind.Cavalry)
                    dest = view.EnemyGranary;                     // 骑兵：突袭敌粮仓，断其补给
                else
                    dest = NearestEnemy(u, view) ?? view.EnemyGranary; // 步/弓：逼近最近可见敌；无敌可见则推进
                orders.Add(new AiOrder(u.Id, dest));
            }
            return orders;
        }

        /// <summary>规划并经命令契约下达到态（同规则、前置校验；非法目的地静默跳过——不作弊绕过校验）。</summary>
        public void Apply(GridBattleState state, GridSide aiSide, GridBattleConfig config)
        {
            AiGridView view = AiGridView.BuildFor(state, aiSide);
            foreach (AiOrder o in Plan(view, config))
                GridBattleEngine.SetDestination(state, o.UnitId, o.Destination);
        }

        private static GridCoord? NearestEnemy(AiUnitView u, AiGridView view)
        {
            GridCoord? best = null;
            int bestD = int.MaxValue;
            foreach (AiUnitView e in view.VisibleEnemies)
            {
                int d = GridCoord.Manhattan(u.Position, e.Position);
                // 距离更近取之；等距按坐标规范序（Y→X）决胜，保证确定性。
                if (d < bestD || (d == bestD && best.HasValue && e.Position.CompareTo(best.Value) < 0))
                {
                    bestD = d; best = e.Position;
                }
            }
            return best;
        }
    }
}
