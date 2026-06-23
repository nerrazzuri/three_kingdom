namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 己方城市账本的<b>只读投影 DTO</b>（ADR-0002：Application → Presentation 只暴露不可变投影，
    /// 不泄露可变 Domain 聚合 <c>CityEconomyState</c>）。承载展示所需的权威读值——粮草/民心/治安/工事，
    /// 以及最近一次日界结算的短缺与骚乱风险。粮量为权威整数（ADR-0004 权威路径无 float）。不可变。
    /// </summary>
    public sealed class CityLedgerProjection
    {
        /// <summary>城市标识（展示用）。</summary>
        public string CityLabel { get; }
        /// <summary>当前粮食库存（权威整数）。</summary>
        public long Stock { get; }
        /// <summary>可自由分配量（库存 − 已承诺保留）。</summary>
        public long Available { get; }
        /// <summary>城市民心。</summary>
        public int CivMorale { get; }
        /// <summary>治安。</summary>
        public int Security { get; }
        /// <summary>工事当前值。</summary>
        public int Fortification { get; }
        /// <summary>工事最大值。</summary>
        public int FortificationMax { get; }
        /// <summary>最近一次日界结算的民用短缺量（0=无短缺）。</summary>
        public long LastDayShortage { get; }
        /// <summary>最近一次日界结算是否高骚乱风险。</summary>
        public bool HighUnrestRisk { get; }

        public CityLedgerProjection(
            string cityLabel, long stock, long available, int civMorale, int security,
            int fortification, int fortificationMax, long lastDayShortage, bool highUnrestRisk)
        {
            CityLabel = cityLabel;
            Stock = stock;
            Available = available;
            CivMorale = civMorale;
            Security = security;
            Fortification = fortification;
            FortificationMax = fortificationMax;
            LastDayShortage = lastDayShortage;
            HighUnrestRisk = highUnrestRisk;
        }
    }
}
