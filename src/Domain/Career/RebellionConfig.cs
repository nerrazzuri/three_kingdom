using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立触发与结局分支的版本化配置（GDD_014 §Formula 2/3 / §Tuning Knobs / TR-career-002 / ADR-0003）。
    /// 不可变；构造时校验范围与 mid≤hi，非法即抛、无部分写入。阈值<b>来自配置</b>，判定不硬编码。
    /// </summary>
    public sealed class RebellionConfig
    {
        /// <summary>自立第 1 组：自主掌控城池数下限（≥0）。</summary>
        public int RebelCityMin { get; }

        /// <summary>自立第 2 组：名望下限（≥0）。</summary>
        public int RebelRenownMin { get; }

        /// <summary>自立第 2 组：核心文武平均好感下限（定点 ∈[0,1]）。</summary>
        public FixedPoint RebelAffinityMin { get; }

        /// <summary>分支判定：单个僚属计入"忠诚"的好感阈值（定点 ∈[0,1]）。</summary>
        public FixedPoint DefectThreshold { get; }

        /// <summary>分支判定：全员拥立的 loyal_ratio 下限 hi（定点 ∈[0,1]）。</summary>
        public FixedPoint LoyalRatioHi { get; }

        /// <summary>分支判定：部分跟随的 loyal_ratio 下限 mid（定点 ∈[0,1]，≤hi）。</summary>
        public FixedPoint LoyalRatioMid { get; }

        /// <summary>配置指纹（确定性哈希，ADR-0003）。</summary>
        public StateHash ConfigFingerprint { get; }

        public RebellionConfig(
            int rebelCityMin,
            int rebelRenownMin,
            FixedPoint rebelAffinityMin,
            FixedPoint defectThreshold,
            FixedPoint loyalRatioHi,
            FixedPoint loyalRatioMid)
        {
            if (rebelCityMin < 0) throw new ArgumentOutOfRangeException(nameof(rebelCityMin), "自立城池下限不可为负。");
            if (rebelRenownMin < 0) throw new ArgumentOutOfRangeException(nameof(rebelRenownMin), "自立名望下限不可为负。");
            CheckUnit(rebelAffinityMin, nameof(rebelAffinityMin));
            CheckUnit(defectThreshold, nameof(defectThreshold));
            CheckUnit(loyalRatioHi, nameof(loyalRatioHi));
            CheckUnit(loyalRatioMid, nameof(loyalRatioMid));
            if (loyalRatioMid > loyalRatioHi)
                throw new ArgumentException("mid 不可大于 hi（部分跟随门槛 ≤ 全员拥立门槛）。", nameof(loyalRatioMid));

            RebelCityMin = rebelCityMin;
            RebelRenownMin = rebelRenownMin;
            RebelAffinityMin = rebelAffinityMin;
            DefectThreshold = defectThreshold;
            LoyalRatioHi = loyalRatioHi;
            LoyalRatioMid = loyalRatioMid;

            var hasher = new StateHasher();
            hasher.Append(rebelCityMin);
            hasher.Append(rebelRenownMin);
            hasher.Append(rebelAffinityMin);
            hasher.Append(defectThreshold);
            hasher.Append(loyalRatioHi);
            hasher.Append(loyalRatioMid);
            ConfigFingerprint = hasher.ToHash();
        }

        private static void CheckUnit(FixedPoint v, string name)
        {
            if (v < FixedPoint.Zero || v > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "须在 [0,1]。");
        }
    }
}
