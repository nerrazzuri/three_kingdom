using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市治理命令的代价/增益配置（GDD_004 §Balancing / M03 / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围，失败即抛、无部分写入。所有系数版本化，方法体内不得硬编码。
    /// <para>
    /// 与 <see cref="CitySettlementConfig"/>（日界自动结算参数）职责分离：本类型只描述
    /// <b>玩家主动治理命令</b>（征用/修工事/安抚）的代价与增益。
    /// </para>
    /// </summary>
    public sealed class CityGovernanceConfig
    {
        /// <summary>征用军粮每单位的城市民心代价系数 k_morale_req（GDD_004 §Formula 5，建议 0..1）。</summary>
        public FixedPoint RequisitionMoralePenalty { get; }

        /// <summary>单次安抚命令的民心增益（≥0；夹至 CivMoraleMax）。</summary>
        public int AppeaseMoraleGain { get; }

        /// <summary>单次修工事命令的投入修复量（≥0；夹至工事上限）。</summary>
        public int FortRepairPerOrder { get; }

        public CityGovernanceConfig(FixedPoint requisitionMoralePenalty, int appeaseMoraleGain, int fortRepairPerOrder)
        {
            if (requisitionMoralePenalty < FixedPoint.Zero)
                throw new ArgumentOutOfRangeException(nameof(requisitionMoralePenalty), "征用民心代价系数不可为负。");
            if (appeaseMoraleGain < 0)
                throw new ArgumentOutOfRangeException(nameof(appeaseMoraleGain), "安抚民心增益不可为负。");
            if (fortRepairPerOrder < 0)
                throw new ArgumentOutOfRangeException(nameof(fortRepairPerOrder), "修工事投入量不可为负。");

            RequisitionMoralePenalty = requisitionMoralePenalty;
            AppeaseMoraleGain = appeaseMoraleGain;
            FortRepairPerOrder = fortRepairPerOrder;
        }
    }
}
