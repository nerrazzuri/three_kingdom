// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 断粮→士气/疲劳传导有唯一施加点（GDD_011），不被重复计算
// Date: 2026-06-21

using TkSlice.Domain.Config;
using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Forces
{
    public enum Side { Defender, Attacker }

    /// <summary>
    /// 一支部队的权威状态。所有状态用整数/定点。
    /// 士气/疲劳/军纪/补给为 [0,1] 定点。仅 Domain 服务可变更（经 Command 路径）。
    /// </summary>
    public sealed class ForceState
    {
        public string Id { get; }
        public Side Side { get; }
        public int Troops { get; private set; }
        public Fixed UnitMorale { get; private set; }   // 部队士气（区别于城市民心 civ_morale）
        public Fixed Fatigue { get; private set; }
        public Fixed Discipline { get; private set; }
        public Fixed SupplyState { get; private set; }  // 补给充足度 1=充足 0=断绝
        /// <summary>连续短缺时段计数（GDD_012 §5）。</summary>
        public int ShortageSegments { get; private set; }

        public ForceState(string id, Side side, int troops,
            Fixed morale, Fixed fatigue, Fixed discipline, Fixed supply)
        {
            Id = id; Side = side; Troops = troops;
            UnitMorale = morale; Fatigue = fatigue; Discipline = discipline; SupplyState = supply;
            ShortageSegments = 0;
        }

        private static Fixed Unit(Fixed v) => Fixed.Clamp(v, Fixed.Zero, Fixed.OneValue);

        /// <summary>补给被切断一时段：先改 supply_state 并累计短缺（GDD_012 §5，只改本系统拥有的状态）。</summary>
        public void ApplySupplyCut(SiegeConfig cfg)
        {
            SupplyState = Unit(SupplyState - cfg.SupplyDecayPerSegment);
            ShortageSegments++;
        }

        /// <summary>补给恢复一时段：短缺计数清零、补给回升（无袭扰时）。</summary>
        public void ApplySupplyRestore(SiegeConfig cfg)
        {
            SupplyState = Unit(SupplyState + cfg.SupplyDecayPerSegment);
            ShortageSegments = 0;
        }

        /// <summary>护卫挡住袭扰、敌补给车队推进回补一时段：补给部分回升、短缺缓解（非清零）。</summary>
        public void ApplyResupplyPush(SiegeConfig cfg)
        {
            SupplyState = Unit(SupplyState + cfg.EnemyResupplyRestore);
            if (ShortageSegments > 0) ShortageSegments--;
        }

        /// <summary>断粮后果施加结果（供因果解释链记录）。</summary>
        public readonly struct StarvationEffect
        {
            public readonly bool Applied;
            public readonly int Deserted;
            public StarvationEffect(bool applied, int deserted) { Applied = applied; Deserted = deserted; }
        }

        /// <summary>
        /// 断粮后果的【唯一施加点】（GDD_011）：仅当短缺超宽限期才施加，幂等按时段。
        /// 士气跌破阈值则部队溃逃减员（GDD_011 失败态）。
        /// </summary>
        public StarvationEffect ApplyStarvationConsequence(SiegeConfig cfg)
        {
            if (ShortageSegments < cfg.SupplyGracePeriod) return new StarvationEffect(false, 0);
            UnitMorale = Unit(UnitMorale - cfg.StarveMoralePenalty);
            Fatigue = Unit(Fatigue + cfg.StarveFatigueGain);

            int deserted = 0;
            if (UnitMorale < cfg.DesertMoraleThreshold)
            {
                deserted = (Fixed.FromInt(Troops) * cfg.DesertRatePerSegment).FloorToInt();
                TakeCasualties(deserted);
            }
            return new StarvationEffect(true, deserted);
        }

        public void Reinforce(int troops) => Troops += troops;
        public void TakeCasualties(int troops) => Troops = troops > Troops ? 0 : Troops - troops;

        /// <summary>直接夺气（如遭伏击全军震动）。</summary>
        public void HitMorale(Fixed amount) => UnitMorale = Unit(UnitMorale - amount);

        public ForceState Clone()
        {
            var c = new ForceState(Id, Side, Troops, UnitMorale, Fatigue, Discipline, SupplyState);
            c.ShortageSegments = ShortageSegments;
            return c;
        }

        /// <summary>从存档快照恢复（含短缺计数）。</summary>
        public static ForceState Restore(string id, Side side, int troops,
            Fixed morale, Fixed fatigue, Fixed discipline, Fixed supply, int shortageSegments)
        {
            var f = new ForceState(id, side, troops, morale, fatigue, discipline, supply);
            f.ShortageSegments = shortageSegments;
            return f;
        }

        /// <summary>状态哈希贡献（确定性回归用）。</summary>
        public long StateHash()
        {
            unchecked
            {
                long h = 17;
                h = h * 31 + Troops;
                h = h * 31 + UnitMorale.Raw;
                h = h * 31 + Fatigue.Raw;
                h = h * 31 + Discipline.Raw;
                h = h * 31 + SupplyState.Raw;
                h = h * 31 + ShortageSegments;
                return h;
            }
        }
    }
}
