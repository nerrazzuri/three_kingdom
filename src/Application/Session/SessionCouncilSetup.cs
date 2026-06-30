using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 会话军议装配配置（M04 / ADR-0003 数据驱动）。打包召开军议所需的军师视角、候选论证模板、
    /// 军议平衡配置与已知主题置信。不可变；构造校验非空。
    /// <para>
    /// 军议本身复用既有 <see cref="WarCouncilService"/>（GDD_008，军师只输出条件化建议）；
    /// 本类型只是装配层把"数据驱动的军议输入"集中传入会话，不含任何规则。
    /// </para>
    /// </summary>
    public sealed class SessionCouncilSetup
    {
        /// <summary>军师视角（能力影响缺口发现与置信）。</summary>
        public AdvisorPerspective Advisor { get; }

        /// <summary>候选论证模板（数据驱动）。</summary>
        public IReadOnlyList<AdviceTemplate> Templates { get; }

        /// <summary>军议平衡配置。</summary>
        public CouncilConfig Config { get; }

        /// <summary>已知主题的统一有效置信（GDD_007 effective_conf 的会话级简化输入）。</summary>
        public FixedPoint KnownClaimConfidence { get; }

        public SessionCouncilSetup(
            AdvisorPerspective advisor, IReadOnlyList<AdviceTemplate> templates,
            CouncilConfig config, FixedPoint knownClaimConfidence)
        {
            Advisor = advisor ?? throw new ArgumentNullException(nameof(advisor));
            Templates = templates ?? throw new ArgumentNullException(nameof(templates));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            KnownClaimConfidence = knownClaimConfidence;
        }
    }
}
