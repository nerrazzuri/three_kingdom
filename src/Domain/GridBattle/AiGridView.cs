using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>敌AI可见的一个单位投影（位置/兵力/兵种；不含目的地等意图）。</summary>
    public sealed class AiUnitView
    {
        public BattleUnitId Id { get; }
        public TroopKind Kind { get; }
        public GridCoord Position { get; }
        public int Strength { get; }

        internal AiUnitView(BattleUnitId id, TroopKind kind, GridCoord position, int strength)
        {
            Id = id; Kind = kind; Position = position; Strength = strength;
        }
    }

    /// <summary>
    /// 敌AI世界观投影（ADR-0013 D2 反全知锁，特化到格子）：AI 只见<b>自身</b>单位 + <b>可见</b>敌方单位。
    /// <b>结构上拒绝真值</b>——AI 决策只读本视图，不读 <see cref="GridBattleState"/>。林中未被侦破的藏兵不入可见面
    /// （MVP：见 <see cref="BuildFor"/> 的可见性过滤——当前为"非林地即可见"的占位反全知，完整情报时效投影后续接 GDD-007）。
    /// </summary>
    public sealed class AiGridView
    {
        /// <summary>AI 阵营。</summary>
        public GridSide AiSide { get; }
        /// <summary>AI 自身存活单位。</summary>
        public IReadOnlyList<AiUnitView> OwnUnits { get; }
        /// <summary>AI 可见的敌方单位（藏兵排除）。</summary>
        public IReadOnlyList<AiUnitView> VisibleEnemies { get; }
        /// <summary>AI 自身补给度（用于攻守取舍）。</summary>
        public int OwnSupply { get; }
        /// <summary>AI 自身粮仓坐标（守/补给点）。</summary>
        public GridCoord OwnGranary { get; }
        /// <summary>敌方粮仓坐标（断粮目标，属公开地图信息）。</summary>
        public GridCoord EnemyGranary { get; }

        private AiGridView(GridSide aiSide, IReadOnlyList<AiUnitView> own, IReadOnlyList<AiUnitView> enemies,
            int ownSupply, GridCoord ownGranary, GridCoord enemyGranary)
        {
            AiSide = aiSide; OwnUnits = own; VisibleEnemies = enemies;
            OwnSupply = ownSupply; OwnGranary = ownGranary; EnemyGranary = enemyGranary;
        }

        /// <summary>从战斗态为 <paramref name="aiSide"/> 构造反全知投影（过滤真值，藏兵排除）。</summary>
        public static AiGridView BuildFor(GridBattleState state, GridSide aiSide)
        {
            var own = new List<AiUnitView>();
            var enemies = new List<AiUnitView>();
            foreach (GridUnit u in state.OrderedAlive())
            {
                if (u.Side == aiSide)
                    own.Add(new AiUnitView(u.Id, u.Kind, u.Position, u.Strength));
                else if (state.Grid.TerrainAt(u.Position) != TerrainKind.Forest) // 藏兵（林中）不可见
                    enemies.Add(new AiUnitView(u.Id, u.Kind, u.Position, u.Strength));
            }
            GridCoord ownGran = aiSide == GridSide.Player ? state.Grid.PlayerGranary : state.Grid.EnemyGranary;
            GridCoord foeGran = aiSide == GridSide.Player ? state.Grid.EnemyGranary : state.Grid.PlayerGranary;
            return new AiGridView(aiSide, own, enemies, state.SupplyOf(aiSide), ownGran, foeGran);
        }
    }
}
