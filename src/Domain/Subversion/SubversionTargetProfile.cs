using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 施计目标（守将）画像（GDD_024 §6）。<b>投影自 Intel + Relationships，非世界真值</b>（反全知，R1）：
    /// 未侦察（<see cref="Scouted"/>=false）时弱点读不清、<see cref="IntelQuality"/> 视为 0。
    /// 各维定点 [0,1]（ADR-0004），不可变。
    /// </summary>
    public sealed class SubversionTargetProfile
    {
        /// <summary>目标守将。</summary>
        public CharacterId General { get; }
        /// <summary>忠诚（对当前主君，越低越易策反）。</summary>
        public FixedPoint Loyalty { get; }
        /// <summary>对主君怨恨（离间/策反弱点，越高越可乘）。</summary>
        public FixedPoint ResentmentToLord { get; }
        /// <summary>贪财（受许诺/贿赂影响，策反辅助弱点）。</summary>
        public FixedPoint Greed { get; }
        /// <summary>魅力抵抗（攻心/流言的抵抗）。</summary>
        public FixedPoint Charm { get; }
        /// <summary>警觉（越高越易识破 → 反噬）。</summary>
        public FixedPoint Alertness { get; }
        /// <summary>是否已侦察（反全知门；false → 盲施，弱点不可读）。</summary>
        public bool Scouted { get; }
        /// <summary>侦察质量（[0,1]；未侦察时结算按 0 处理）。</summary>
        public FixedPoint IntelQuality { get; }

        public SubversionTargetProfile(
            CharacterId general, FixedPoint loyalty, FixedPoint resentmentToLord, FixedPoint greed,
            FixedPoint charm, FixedPoint alertness, bool scouted, FixedPoint intelQuality)
        {
            RequireUnit(loyalty, nameof(loyalty));
            RequireUnit(resentmentToLord, nameof(resentmentToLord));
            RequireUnit(greed, nameof(greed));
            RequireUnit(charm, nameof(charm));
            RequireUnit(alertness, nameof(alertness));
            RequireUnit(intelQuality, nameof(intelQuality));
            General = general;
            Loyalty = loyalty;
            ResentmentToLord = resentmentToLord;
            Greed = greed;
            Charm = charm;
            Alertness = alertness;
            Scouted = scouted;
            IntelQuality = intelQuality;
        }

        /// <summary>结算用侦察质量（反全知：未侦察恒 0）。</summary>
        public FixedPoint EffectiveIntelQuality => Scouted ? IntelQuality : FixedPoint.Zero;

        private static void RequireUnit(FixedPoint v, string n)
        { if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(n, "须在 [0,1]。"); }
    }
}
