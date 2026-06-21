// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 军师给观察/候选路线/风险/置信，但不替玩家定计、不保证结果（GDD_008 设计锁）
// Date: 2026-06-21

using System.Collections.Generic;
using TkSlice.Domain.Environment;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Siege;

namespace TkSlice.Domain.Advisory
{
    /// <summary>一条候选兵法路线的军师评估（含是否可行、所需条件、主要风险）。</summary>
    public readonly struct RouteAdvice
    {
        public readonly string Name;
        public readonly bool Available;
        public readonly string Needs;
        public readonly string Risk;
        public readonly string How;   // 大致操作提示（仍由玩家决定是否、如何投入）
        public RouteAdvice(string name, bool available, string needs, string risk, string how)
        { Name = name; Available = available; Needs = needs; Risk = risk; How = how; }
    }

    /// <summary>军师建议包：观察 / 候选路线 / 缺失情报 / 置信度 / 免责声明（GDD_008）。</summary>
    public sealed class AdvicePacket
    {
        public List<string> Observations { get; } = new();
        public List<RouteAdvice> Routes { get; } = new();
        public List<string> MissingIntel { get; } = new();
        public string Confidence { get; set; } = "中";
        public string Disclaimer => "以上为军师之见，仅供参考——不替你定计，不保证结果，最终决断在你。";
    }

    /// <summary>
    /// 军师（精简）。只读当前可见状态，产出建议；不输出隐藏真值、不自动选最优方案。
    /// 前期为生疏玩家提供观察与候选路线，帮助理解「该考虑什么」，而非代替决策。
    /// </summary>
    public static class Advisor
    {
        public static AdvicePacket Advise(SiegeState s)
        {
            var cfg = s.Config;
            var a = new AdvicePacket();

            // ── 观察（军师只读知识投影 Intel，非真值；GDD_008）──
            var intel = s.Intel;
            int seg = s.Clock.TotalSegments;
            bool stale = intel.Confidence < Fixed.FromFraction(25, 100);

            if (stale)
            {
                a.Observations.Add($"敌情已陈旧（{intel.Describe(seg)}）——所报兵力/补给未必可信，**宜先侦察再做判断**。");
            }
            else
            {
                if (intel.EstTroops >= s.Defender.Troops)
                {
                    int ratioX10 = s.Defender.Troops > 0 ? intel.EstTroops * 10 / s.Defender.Troops : 99;
                    a.Observations.Add($"据探报敌约 ~{intel.EstTroops} 兵、约 {ratioX10 / 10.0:0.0} 倍于我——正面凶多吉少，须先创造条件。");
                }
                if (intel.EstSupply < Fixed.FromFraction(50, 100))
                    a.Observations.Add($"据探报敌补给告急（约 ~{intel.EstSupply}）——断粮之计似已奏效，可待其士气崩。");
                else
                    a.Observations.Add($"据探报敌补给尚足（约 ~{intel.EstSupply}）——若在断粮，恐未压过其护粮，须加力或另谋。");
                if (intel.EstMorale < Fixed.FromFraction(40, 100))
                    a.Observations.Add("据探报敌士气濒崩，随时可能溃逃——决战窗口将至。");
            }
            if (intel.ReinforcementRumor)
                a.Observations.Add("⚠ 探报：敌军援军将至！消耗赛时间将尽，宜速战速决或改弦更张。");
            if (s.EnemyCommander.Recklessness > Fixed.FromFraction(50, 100))
                a.Observations.Add($"敌将性情鲁莽（推测 {s.EnemyCommander.Recklessness}），贪功冒进，可以诱敌之计破之。");
            if (Weather.DelaysAssault(s.CurrentWeather))
                a.Observations.Add($"天候{Weather.Name(s.CurrentWeather)}，敌军强攻受阻——利于拖延待变。");
            if (s.PendingPledge != null)
                a.Observations.Add($"我之外援在途，约 {s.PendingPledge.ArrivalT} 抵达（但可能迟到或背约，勿全押）。");

            // ── 候选兵法路线（不排序、不指定唯一最优）──
            a.Routes.Add(new RouteAdvice(
                "断粮疲敌",
                available: true,
                needs: "袭扰强度须压过敌军护粮（投入越多越易得手），且赶在敌援军抵达前拖垮其补给与士气。",
                risk: "袭扰队太少则敌补给车队反复回补、徒劳无功；抽调兵力会削弱工事；拖太久敌援军将至。",
                how: "命令 1（如 `1 3` 多投以压过护卫），4 推进数段，**7 侦察确认是否真断粮**，敌虚弱后 6 决战。"));

            bool disciplineOk = s.Defender.Discipline >= cfg.FeignDisciplineThreshold;
            a.Routes.Add(new RouteAdvice(
                "假退伏击",
                available: !s.AmbushChainSpent && disciplineOk,
                needs: $"军纪须≥{cfg.FeignDisciplineThreshold}（当前 {s.Defender.Discipline}，{(disciplineOk ? "满足" : "不足")}）压住佯退，且敌将贪追。",
                risk: "军纪不足则佯退弄假成真、诱饵折损；诱敌为一次性，敌将一旦警觉不可再用。",
                how: "命令 2（如 `2 150 350`：诱饵 150 + 伏兵 350）。"));

            a.Routes.Add(new RouteAdvice(
                "守城待变",
                available: s.PendingPledge == null,
                needs: "遣使向外势力求援/求粮/求时限，以声望与承诺代价换取，再扛住围城等其交付。",
                risk: "外援延迟交付、可能背约，且代价照付——绝非即到的保证。",
                how: "命令 3（`3 a 60` 求援 / `3 b` 求粮 / `3 c` 求时限），再 4 守城待援。"));

            // ── 缺失情报 / 置信（取决于侦察新鲜度）──
            a.MissingIntel.Add("敌军兵力、补给、援军动向均为侦察推测，非确证——情报会过时，须复探。");
            int confPct = (intel.Confidence * Fixed.FromInt(100)).FloorToInt();
            a.Confidence = confPct >= 70 ? "高" : confPct >= 40 ? "中" : "低（情报陈旧）";
            return a;
        }
    }
}
