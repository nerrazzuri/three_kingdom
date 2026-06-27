namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 功绩/名望/君主好感的具名累积来源（GDD_014 §Detailed Rules：merit/renown/lord_standing 各来源）。
    /// 各来源的增益权重由 <see cref="PromotionLadderConfig"/> 配置化（ADR-0003），不在代码硬编码。
    /// <para>
    /// <b>反支柱护栏（W5）</b>：非战斗来源（治理/任务/招揽）须在配置上与战斗来源速率有竞争力，
    /// 防"最优玩法只需刷战斗"。<see cref="IsCombatSource"/> 标注来源类别，供护栏校验。
    /// </para>
    /// </summary>
    public enum CareerGainSource
    {
        /// <summary>作战胜利（战斗源）。</summary>
        CombatVictory = 0,

        /// <summary>大型战役胜利（战斗源，名望权重高）。</summary>
        MajorBattleVictory = 1,

        /// <summary>完成君主任务（非战斗源）。</summary>
        LordMissionComplete = 2,

        /// <summary>治理城池 / 治理盛世（非战斗源）。</summary>
        CityGovernance = 3,

        /// <summary>平定叛乱（非战斗源，名望显著）。</summary>
        RebellionSuppressed = 4,

        /// <summary>招揽贤才（非战斗源，名望源）。</summary>
        TalentRecruited = 5,
    }

    /// <summary>来源类别工具。</summary>
    public static class CareerGainSources
    {
        /// <summary>该来源是否为战斗来源（用于 W5 非战斗速率护栏校验）。</summary>
        public static bool IsCombatSource(CareerGainSource source)
            => source == CareerGainSource.CombatVictory || source == CareerGainSource.MajorBattleVictory;
    }
}
