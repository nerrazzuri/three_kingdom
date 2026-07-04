using System.Collections.Generic;
using ThreeKingdom.Domain.Subversion;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 人心杠杆施计结果展示视图（GDD_024）。把 <see cref="SubversionOutcome"/> 译为中文文案——
    /// 给结果与缘由，<b>不给胜率数字</b>（承接反支柱/军师不报胜率原则）。不可变、纯映射。
    /// </summary>
    public sealed class SubversionView
    {
        private static readonly IReadOnlyDictionary<SubversionScheme, string> SchemeNames =
            new Dictionary<SubversionScheme, string>
            {
                [SubversionScheme.SowDiscord] = "离间",
                [SubversionScheme.InciteDefection] = "策反",
                [SubversionScheme.UnderminedMorale] = "攻心流言",
            };

        /// <summary>计名（离间/策反/攻心流言）。</summary>
        public string SchemeLabel { get; }
        /// <summary>结果类型。</summary>
        public SubversionResult Result { get; }
        /// <summary>结果文案（成/无效/反噬 + 缘由，无胜率）。</summary>
        public string ResultLabel { get; }
        /// <summary>是否暴露（反噬 → 守将警觉、后续更难）。</summary>
        public bool Exposed { get; }

        public SubversionView(SubversionScheme scheme, SubversionOutcome outcome)
        {
            SchemeLabel = SchemeNames.TryGetValue(scheme, out string n) ? n : scheme.ToString();
            Result = outcome.Result;
            Exposed = outcome.Exposed;
            ResultLabel = Describe(scheme, outcome);
        }

        private static string Describe(SubversionScheme scheme, SubversionOutcome o)
        {
            string s = SchemeNames.TryGetValue(scheme, out string n) ? n : scheme.ToString();
            switch (o.Result)
            {
                case SubversionResult.Success:
                    return "「" + s + "」得手——敌守方已生间隙，出征时可见其效。";
                case SubversionResult.Backfired:
                    return "「" + s + "」被识破——守将警觉、同仇敌忾，此城暂难再施；情报已暴露。";
                default:
                    return "「" + s + "」未见成效——门槛未齐或时机未到（未伤根本，可再图）。";
            }
        }
    }
}
