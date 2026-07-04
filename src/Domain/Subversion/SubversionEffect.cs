using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 施计的<b>战斗接缝修正</b>（GDD_024 F3 / 拟 ADR-0014）：出征发起时并入守方（GDD_021）。
    /// 不可变、可存档、确定性哈希（ADR-0004）。多次施计经 <see cref="Combine"/> 累积（守方 delta 求和、
    /// 倒戈比取"补数相乘"避免超 1），最终在 <see cref="ZoneBattle"/> 守方生成处消费。
    /// </summary>
    public sealed class SubversionEffect
    {
        /// <summary>守方开战士气增量（攻心为负、反噬为正）。</summary>
        public FixedPoint DefenderMoraleDelta { get; }
        /// <summary>守军倒戈比例（有效守军 = 守军×(1−此值)，[0,1]）。</summary>
        public FixedPoint GarrisonDefectRatio { get; }
        /// <summary>守方军纪增量（离间为负）。</summary>
        public FixedPoint DefenderDisciplineDelta { get; }

        public SubversionEffect(FixedPoint defenderMoraleDelta, FixedPoint garrisonDefectRatio, FixedPoint defenderDisciplineDelta)
        {
            DefenderMoraleDelta = defenderMoraleDelta;
            GarrisonDefectRatio = garrisonDefectRatio.Clamp(FixedPoint.Zero, FixedPoint.One);
            DefenderDisciplineDelta = defenderDisciplineDelta;
        }

        /// <summary>无效果（无待生效施计时的中性元素）。</summary>
        public static SubversionEffect None { get; } =
            new SubversionEffect(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);

        /// <summary>是否有任何非零修正。</summary>
        public bool IsNone =>
            DefenderMoraleDelta == FixedPoint.Zero
            && GarrisonDefectRatio == FixedPoint.Zero
            && DefenderDisciplineDelta == FixedPoint.Zero;

        /// <summary>累积另一次施计效果：士气/军纪 delta 求和，倒戈比按补数相乘（1−(1−a)(1−b)）。</summary>
        public SubversionEffect Combine(SubversionEffect other)
        {
            if (other == null || other.IsNone) return this;
            FixedPoint keep = (FixedPoint.One - GarrisonDefectRatio) * (FixedPoint.One - other.GarrisonDefectRatio);
            return new SubversionEffect(
                DefenderMoraleDelta + other.DefenderMoraleDelta,
                FixedPoint.One - keep,
                DefenderDisciplineDelta + other.DefenderDisciplineDelta);
        }

        internal void AppendTo(StateHasher hasher)
            => hasher.Append(DefenderMoraleDelta).Append(GarrisonDefectRatio).Append(DefenderDisciplineDelta);
    }
}
