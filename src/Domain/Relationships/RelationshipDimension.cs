namespace ThreeKingdom.Domain.Relationships
{
    /// <summary>
    /// 关系维度（GDD_006：MVP 四个方向性维度，<b>不</b>压缩成单一好感值，P6 多维不合并）。
    /// </summary>
    public enum RelationshipDimension
    {
        /// <summary>信任。</summary>
        Trust = 0,

        /// <summary>敬重。</summary>
        Respect = 1,

        /// <summary>恩义。</summary>
        Gratitude = 2,

        /// <summary>怨恨。</summary>
        Resentment = 3,
    }

    /// <summary>关系维度取值刻度（GDD_006 §Formula：DIM_MIN..DIM_MAX，向 DIM_NEUTRAL 衰减）。</summary>
    public static class RelationshipScale
    {
        /// <summary>维度下限。</summary>
        public const int Min = -100;

        /// <summary>维度上限。</summary>
        public const int Max = 100;

        /// <summary>中性值（衰减目标）。</summary>
        public const int Neutral = 0;

        /// <summary>夹取到 [Min, Max]。</summary>
        public static int Clamp(long value)
            => value < Min ? Min : value > Max ? Max : (int)value;
    }
}
