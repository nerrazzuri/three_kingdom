namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 人心杠杆三计（GDD_024 R2，杠杆非技能按钮）。各计作用于不同弱点、映射不同战斗效果。
    /// </summary>
    public enum SubversionScheme
    {
        /// <summary>离间：挑守将与主君关系（怨恨↑/信任↓）→ 战斗守方军纪罚；越阈触倒戈倾向。</summary>
        SowDiscord = 0,

        /// <summary>策反：仅低忠诚+高怨恨方成型 → 守军一部倒戈（有效守军减）。</summary>
        InciteDefection = 1,

        /// <summary>攻心流言：散布流言 → 守方开战士气↓（守将魅力抵抗）。</summary>
        UnderminedMorale = 2,
    }

    /// <summary>施计结算类型（GDD_024 F2）。</summary>
    public enum SubversionResult
    {
        /// <summary>成型生效（产出 <see cref="SubversionEffect"/>）。</summary>
        Success = 0,

        /// <summary>无效：门不齐或判定落空——耗成本、无效果（非报错，失败可继续）。</summary>
        Ineffective = 1,

        /// <summary>反噬：被识破——守方士气反升 + 守将怨你 + 情报暴露（合法可继续，红线）。</summary>
        Backfired = 2,
    }
}
