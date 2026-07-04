using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Theater;

namespace ThreeKingdom.Application.Theater
{
    /// <summary>多城战区命令稳定错误码（GDD M12）。</summary>
    public enum TheaterCommandError
    {
        None = 0,
        NotHeld = 1,          // 未持有该城
        ExceedsSpan = 2,      // 超出官阶亲管范围
    }

    /// <summary>战区命令结果（成功携新态；失败=原态 + 稳定错误码，无部分写入）。不可变。</summary>
    public sealed class TheaterCommandResult
    {
        public bool Applied { get; }
        public TheaterCommandError Error { get; }
        public string Detail { get; }
        public TheaterState State { get; }

        private TheaterCommandResult(bool applied, TheaterCommandError error, string detail, TheaterState state)
        {
            Applied = applied;
            Error = error;
            Detail = detail;
            State = state;
        }

        public static TheaterCommandResult Success(TheaterState s) => new TheaterCommandResult(true, TheaterCommandError.None, "", s);
        public static TheaterCommandResult Failure(TheaterCommandError e, TheaterState original, string detail = "") => new TheaterCommandResult(false, e, detail, original);
    }

    /// <summary>
    /// 多城战区编排（Application / GDD M12）：占城 C 产出的城入战区态；委任下属打理；<b>亲管范围随官阶</b>约束
    /// （超阶须委任或升官）。委任城由 <see cref="DelegateGovernanceService"/> 本地自理（不越权）；反全知报告经
    /// <see cref="TheaterReportService"/>。只编排不拥规则。
    /// </summary>
    public sealed class TheaterService
    {
        /// <summary>纳入占城 C 产出的直辖城（默认亲管）。</summary>
        public TheaterState HoldConqueredCity(TheaterState state, CityId city)
            => (state ?? throw new ArgumentNullException(nameof(state))).AddCity(city);

        /// <summary>委任某城给下属打理（须已持有）。</summary>
        public TheaterCommandResult Delegate(TheaterState state, CityId city, CharacterId governor)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!state.Holds(city)) return TheaterCommandResult.Failure(TheaterCommandError.NotHeld, state, city.ToString());
            return TheaterCommandResult.Success(state.Delegate(city, governor));
        }

        /// <summary>
        /// 收回某城亲管：受<b>官阶亲管范围</b>约束——亲管城数超上限则拒（须委任他城或升官）。
        /// 已亲管则幂等。
        /// </summary>
        public TheaterCommandResult SelfGovern(TheaterState state, CityId city, int rank, SpanOfControlConfig span)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (span == null) throw new ArgumentNullException(nameof(span));
            if (!state.Holds(city)) return TheaterCommandResult.Failure(TheaterCommandError.NotHeld, state, city.ToString());

            CityHolding holding = state.Of(city)!;
            if (holding.Mode == GovernanceMode.SelfGoverned) return TheaterCommandResult.Success(state);   // 幂等

            if (state.SelfGovernedCount + 1 > span.MaxSelfGoverned(rank))
                return TheaterCommandResult.Failure(TheaterCommandError.ExceedsSpan, state,
                    $"亲管已达官阶上限（{span.MaxSelfGoverned(rank)}），须委任他城或升官。");

            return TheaterCommandResult.Success(state.Reclaim(city));
        }

        /// <summary>是否在官阶范围内可再亲管一城（供 UI 提示）。</summary>
        public bool CanSelfGovernMore(TheaterState state, int rank, SpanOfControlConfig span)
            => state.SelfGovernedCount < span.MaxSelfGoverned(rank);
    }
}
