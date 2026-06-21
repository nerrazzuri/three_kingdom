using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 路线通行耗时（GDD_003 §Formula 1：<c>route_time = ceil(base × mod_unit × mod_weather × mod_load)</c>，≥1）。
    /// 定点乘法（ADR-0004 禁 float 权威路径）；向上取整 <c>ceil(x) = -floor(-x)</c>。天气修正衔接 Story 003 已结算修正。
    /// </summary>
    public static class RouteCost
    {
        /// <param name="baseTime">路线基础耗时（≥1，配置）。</param>
        /// <param name="unitMod">单位速度修正（&gt;0）。</param>
        /// <param name="weatherMod">天气通行修正（&gt;0，只减速即 ≥1，来自 GDD_002 已结算修正）。</param>
        /// <param name="loadMod">负载修正（&gt;0，≥1）。</param>
        public static int Compute(int baseTime, FixedPoint unitMod, FixedPoint weatherMod, FixedPoint loadMod)
        {
            if (baseTime < 1) throw new ArgumentOutOfRangeException(nameof(baseTime), "基础耗时须 ≥ 1。");
            if (unitMod <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(unitMod), "单位修正须 > 0。");
            if (weatherMod <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(weatherMod), "天气修正须 > 0。");
            if (loadMod <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(loadMod), "负载修正须 > 0。");

            FixedPoint product = FixedPoint.FromInt(baseTime) * unitMod * weatherMod * loadMod;
            int cost = -((-product).FloorToInt()); // ceil
            return cost < 1 ? 1 : cost;
        }
    }

    /// <summary>
    /// 相向移动接触判定（GDD_003 §Formula 5：<c>contact = 双向 ∧ (progress_A + progress_B ≥ 1.0)</c>）。
    /// progress 为 [0,1] 路线行进比例（定点，确定性）。
    /// </summary>
    public static class RouteContact
    {
        /// <summary>判定两单位在同一路线相向行进是否接触。单向路线恒不接触。</summary>
        public static bool Occurs(Route route, FixedPoint progressA, FixedPoint progressB)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            Validate(progressA, nameof(progressA));
            Validate(progressB, nameof(progressB));
            if (!route.Bidirectional) return false;
            return progressA + progressB >= FixedPoint.One;
        }

        private static void Validate(FixedPoint progress, string name)
        {
            if (progress < FixedPoint.Zero || progress > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "行进比例须 ∈ [0,1]。");
        }
    }
}
