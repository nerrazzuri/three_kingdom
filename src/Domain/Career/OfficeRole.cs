namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 可任免官职位（GDD_014 §Data Model：RetinueState「可任免官职（城守/副将/内政主事/军师）」）。
    /// 本骨架仅定义职位与任免路径；各职位的权限/加成差异在后续 story 配置化。
    /// </summary>
    public enum OfficeRole
    {
        /// <summary>城守（守备主官）。</summary>
        CityWarden = 0,

        /// <summary>副将（军务副官）。</summary>
        DeputyGeneral = 1,

        /// <summary>内政主事（治理主官）。</summary>
        InternalAffairs = 2,

        /// <summary>军师（参谋/建议，见 GDD_008）。</summary>
        Strategist = 3,
    }
}
