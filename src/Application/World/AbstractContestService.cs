using System;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.World
{
    /// <summary>
    /// 抽象结算的 Application 编排（GDD_015 / TR-world-004 / ADR-0007 §4 + ADR-0008）。
    /// 解算玩家不在场的势力混战，<b>若易主则经 GDD_004 <see cref="ICityControlAuthority"/> 发起</b>控制权变更
    /// （不直接写 city.owner，ADR-0008）；守住则不触发控制权事件。
    /// </summary>
    public sealed class AbstractContestService
    {
        private readonly IAbstractResolver _resolver;
        private readonly ICityControlAuthority _control;

        public AbstractContestService(IAbstractResolver resolver, ICityControlAuthority cityControlAuthority)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _control = cityControlAuthority ?? throw new ArgumentNullException(nameof(cityControlAuthority));
        }

        /// <summary>解算一场争夺并落地结局：易主经 004 控制权变更，否则归属不变。</summary>
        public AbstractOutcome ResolveAndApply(
            FactionRecord attacker, FactionRecord defender, ContestContext ctx, IDeterministicRandom rng)
        {
            if (attacker is null) throw new ArgumentNullException(nameof(attacker));
            if (defender is null) throw new ArgumentNullException(nameof(defender));

            AbstractOutcome outcome = _resolver.Resolve(attacker, defender, ctx, rng);
            if (outcome.OwnershipChanged)
                _control.RequestControlChange(ctx.ContestedCity, outcome.Winner, ctx.ResultingGarrison, ChangeCause.AbstractContest);
            return outcome;
        }
    }
}
