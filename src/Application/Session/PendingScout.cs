using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 一支在途侦察兵（GDD_007 派出→在途→返报）。派出时记录目标/方法/预计返报时刻；
    /// 推进到 <see cref="ArrivalTime"/> 后由 <see cref="CampaignSessionService"/> 解析为报告并入玩家知识——
    /// 侦察<b>非即时</b>，返报需时。不可变。
    /// </summary>
    public sealed class PendingScout
    {
        /// <summary>侦察对象。</summary>
        public IntelSubjectId Subject { get; }

        /// <summary>侦察方法（来源可靠性）。</summary>
        public IntelSource Method { get; }

        /// <summary>预计返报时刻（派出时刻 + 行程时段）。</summary>
        public WorldTime ArrivalTime { get; }

        public PendingScout(IntelSubjectId subject, IntelSource method, WorldTime arrivalTime)
        {
            Subject = subject;
            Method = method;
            ArrivalTime = arrivalTime;
        }
    }
}
