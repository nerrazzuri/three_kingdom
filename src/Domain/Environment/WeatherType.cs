namespace ThreeKingdom.Domain.Environment
{
    /// <summary>
    /// 天气类型（GDD_002）。枚举序数即天气转移加权选择的规范遍历顺序（确定性，ADR-0004）。
    /// MVP 四类；新增类型须追加在末尾以保转移遍历顺序稳定。
    /// </summary>
    public enum WeatherType
    {
        /// <summary>晴。</summary>
        Clear = 0,

        /// <summary>阴。</summary>
        Overcast = 1,

        /// <summary>雨。</summary>
        Rain = 2,

        /// <summary>雾。</summary>
        Fog = 3,
    }
}
