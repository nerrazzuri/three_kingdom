using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 受控外交请求（GDD_012 §8 / TR-supply 外交受控入口）。
    /// 玩家经 Command 发起的一次求援/求粮/求时限请求 + 承诺代价。不可变；构造校验范围。
    /// 声望/反向压力为 [0,1] 定点输入（声望来自 GDD_006 + 场景预设）。
    /// </summary>
    public sealed class DiplomaticRequest
    {
        /// <summary>请求类型（三选一作用于 slice）。</summary>
        public DiplomaticRequestType Type { get; }

        /// <summary>目标外势力。</summary>
        public ForeignPowerId Power { get; }

        /// <summary>承诺代价（资源/未来义务折算，≥0；接受时兑付，不凭空返还）。</summary>
        public long PledgeCost { get; }

        /// <summary>请求规模（援军兵力 / 补给量 / 时限缩减段数，≥0）。</summary>
        public long RequestedAmount { get; }

        /// <summary>玩家阵营对该外势力的声望/信誉 standing（[0,1]）。</summary>
        public FixedPoint Standing { get; }

        /// <summary>敌方对该外势力的反向外交压力 dipl_pressure（[0,1]）。</summary>
        public FixedPoint DiplomaticPressure { get; }

        public DiplomaticRequest(
            DiplomaticRequestType type,
            ForeignPowerId power,
            long pledgeCost,
            long requestedAmount,
            FixedPoint standing,
            FixedPoint diplomaticPressure)
        {
            if (pledgeCost < 0) throw new ArgumentOutOfRangeException(nameof(pledgeCost), "承诺代价不可为负。");
            if (requestedAmount < 0) throw new ArgumentOutOfRangeException(nameof(requestedAmount), "请求规模不可为负。");
            if (standing < FixedPoint.Zero || standing > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(standing), "声望须在 [0,1]。");
            if (diplomaticPressure < FixedPoint.Zero || diplomaticPressure > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(diplomaticPressure), "外交压力须在 [0,1]。");

            Type = type;
            Power = power;
            PledgeCost = pledgeCost;
            RequestedAmount = requestedAmount;
            Standing = standing;
            DiplomaticPressure = diplomaticPressure;
        }
    }
}
