using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Presentation
{
    /// <summary>
    /// 表现层显示辅助（ADR-0004：float/decimal <b>仅限非权威显示</b>，不参与权威结算/状态哈希）。
    /// 把权威定点值转为展示用 decimal——只读、单向，不回写 Domain。
    /// </summary>
    public static class Display
    {
        private const decimal OneRaw = 65536m; // 2^16（FixedPoint.FractionalBits=16）

        /// <summary>定点 → 展示 decimal（非权威，仅供 UI 呈现）。</summary>
        public static decimal ToDecimal(FixedPoint value) => value.Raw / OneRaw;
    }
}
