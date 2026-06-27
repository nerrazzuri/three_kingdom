namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 官阶（GDD_014 §官职晋升梯队 / Data Model `rank` 0–7）。枚举序数即 GDD 的官阶值。
    /// 逐级解锁带兵上限 / 治理范围 / 战略参与权（具体映射在 story-002 配置化，本骨架仅定序数）。
    /// <para>
    /// 序数严格递增，<see cref="CityGovernor"/>=0 为太守开局阶，<see cref="Successor"/>=7 为忠臣线终极结局。
    /// 晋升须<b>逐级</b>（rank→rank+1），不可越级（见 <see cref="CareerStateService"/>）。
    /// </para>
    /// </summary>
    public enum Rank
    {
        /// <summary>城池太守（单城，基础兵权/内政权）——开局阶。</summary>
        CityGovernor = 0,

        /// <summary>资深太守（多城治理，可调配相邻城池兵力）。</summary>
        SeniorGovernor = 1,

        /// <summary>州刺史（一州数城，自主编制中型军团）。</summary>
        ProvincialInspector = 2,

        /// <summary>四方中郎将（势力核心将领，参与战略决策）。</summary>
        RegionalGeneral = 3,

        /// <summary>镇国将军（独领战区，独立作战调度权）。</summary>
        GuardianGeneral = 4,

        /// <summary>副都督（执掌部分主力军团）。</summary>
        DeputyCommander = 5,

        /// <summary>大都督（总领全军，军政二把手）。</summary>
        GrandCommander = 6,

        /// <summary>继承基业（功绩/名望/民心/全军好感拉满后继承势力）——忠臣线终极结局。</summary>
        Successor = 7,
    }
}
