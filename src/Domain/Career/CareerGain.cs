using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 单次来源事件的生涯增益（GDD_014：merit/renown 累积 + lord_standing 变化）。不可变值。
    /// merit/renown 为非负整数增量；standing 为定点增量（可正可负，结算时钳制到 [0,1]）。
    /// </summary>
    public sealed class CareerGain
    {
        /// <summary>功绩增量（≥0）。</summary>
        public int Merit { get; }

        /// <summary>名望增量（≥0）。</summary>
        public int Renown { get; }

        /// <summary>君主好感增量（定点，可正可负）。</summary>
        public FixedPoint Standing { get; }

        public CareerGain(int merit, int renown, FixedPoint standing)
        {
            if (merit < 0) throw new ArgumentOutOfRangeException(nameof(merit), "功绩增量不可为负（单调累积）。");
            if (renown < 0) throw new ArgumentOutOfRangeException(nameof(renown), "名望增量不可为负（单调累积）。");
            Merit = merit;
            Renown = renown;
            Standing = standing;
        }

        /// <summary>按整数倍数缩放增益（count 次同源事件）。</summary>
        public CareerGain Scale(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "倍数不可为负。");
            return new CareerGain(
                checked(Merit * count),
                checked(Renown * count),
                FixedPoint.FromInt(count) * Standing);
        }
    }
}
