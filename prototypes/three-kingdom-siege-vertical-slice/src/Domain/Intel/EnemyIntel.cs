// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 玩家只见敌情「估计值＋置信」，须侦察才能判断断粮是否真奏效（GDD_007 真值/知识分离）
// Date: 2026-06-21

using TkSlice.Domain.Config;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Intel
{
    /// <summary>
    /// 玩家阵营对敌军的【知识投影】（非真值）。开局模糊，须侦察刷新；不侦察则随时间衰减过时。
    /// 战斗按真值结算，但玩家的判断只能基于此估计——这就是「断粮到底成没成」需要侦察的原因。
    /// </summary>
    public sealed class EnemyIntel
    {
        public int EstTroops { get; private set; }
        public Fixed EstMorale { get; private set; }
        public Fixed EstSupply { get; private set; }
        public Fixed Confidence { get; private set; }
        public int LastObservedSeg { get; private set; }
        public bool ReinforcementRumor { get; private set; }  // 是否探得敌援军将至

        public EnemyIntel(int estTroops, Fixed estMorale, Fixed estSupply, Fixed confidence)
        {
            EstTroops = estTroops; EstMorale = estMorale; EstSupply = estSupply;
            Confidence = confidence; LastObservedSeg = 0; ReinforcementRumor = false;
        }

        private static Fixed Unit(Fixed v) => Fixed.Clamp(v, Fixed.Zero, Fixed.OneValue);

        /// <summary>
        /// 侦察刷新：把估计拉近真值，残留误差由 ScoutErrorBand 与确定性 rng 决定（可能偏高/偏低）。
        /// </summary>
        public void Scout(ForceState truth, int clockSeg, bool reinforcementIncoming, SiegeConfig cfg, DetRng rng)
        {
            // 误差：[-band, +band] 的确定性扰动
            Fixed band = cfg.ScoutErrorBand;
            EstMorale = Unit(truth.UnitMorale + Noise(band, rng));
            EstSupply = Unit(truth.SupplyState + Noise(band, rng));
            // 兵力误差按比例（±band）
            int err = (Fixed.FromInt(truth.Troops) * Noise(band, rng)).FloorToInt();
            EstTroops = truth.Troops + err;
            if (EstTroops < 0) EstTroops = 0;
            Confidence = cfg.ScoutConfidence;
            LastObservedSeg = clockSeg;
            ReinforcementRumor = reinforcementIncoming;
        }

        /// <summary>不侦察则每时段置信衰减（估计值保留但越来越不可信）。</summary>
        public void DecayConfidence(SiegeConfig cfg)
            => Confidence = Fixed.Max(Confidence - cfg.IntelDecayPerSegment, Fixed.Zero);

        /// <summary>存档恢复观察元数据。</summary>
        public void RestoreObservation(int lastObservedSeg, bool reinforcementRumor)
        {
            LastObservedSeg = lastObservedSeg;
            ReinforcementRumor = reinforcementRumor;
        }

        private static Fixed Noise(Fixed band, DetRng rng)
        {
            // [0,1) → [-band, +band]
            Fixed u = rng.NextFixedUnit();
            return band * (u + u - Fixed.OneValue);
        }

        public int StaleFor(int clockSeg) => clockSeg - LastObservedSeg;

        /// <summary>可读呈现（带置信与时效，绝不冒充真值）。</summary>
        public string Describe(int clockSeg)
        {
            int pct = (Confidence * Fixed.FromInt(100)).FloorToInt();
            if (Confidence < Fixed.FromFraction(20, 100))
                return $"敌情不明（情报陈旧 {StaleFor(clockSeg)} 段，信心 {pct}%）——宜先侦察";
            string rumor = ReinforcementRumor ? "；探报：敌援军将至！" : "";
            return $"约兵 ~{EstTroops}、士气 ~{EstMorale}、补给 ~{EstSupply}（信心 {pct}%，{StaleFor(clockSeg)} 段前所探{rumor}）";
        }
    }
}
