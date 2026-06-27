using System;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Application.Career
{
    /// <summary>守城开局事件结算结果（GDD_014）。不可变。携结算后生涯快照、是否解锁全城权限、可能的归属变更事件。</summary>
    public sealed class SiegeResolutionResult
    {
        /// <summary>战役胜负。</summary>
        public SiegeOutcome Outcome { get; }

        /// <summary>结算后生涯快照。</summary>
        public CareerSnapshot Snapshot { get; }

        /// <summary>是否解锁全城权限（守城胜）。</summary>
        public bool FullAuthorityUnlocked { get; }

        /// <summary>失守时由 GDD_004 发布的控制权变更事件（守城胜为 null）。</summary>
        public CityControlChanged? ControlChange { get; }

        public SiegeResolutionResult(SiegeOutcome outcome, CareerSnapshot snapshot, bool fullAuthorityUnlocked, CityControlChanged? controlChange)
        {
            Outcome = outcome;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            FullAuthorityUnlocked = fullAuthorityUnlocked;
            ControlChange = controlChange;
        }
    }

    /// <summary>
    /// 太守开局与守城后果的 Application 编排（GDD_014 / TR-career-001/004 / ADR-0008 + ADR-0002）。
    /// 跨系统集成：开局从 <see cref="CitySeed"/> 绑定生涯 + 登记城池归属；守城后果据 BattleOutcome 调生涯结算，
    /// <b>失城归属经 GDD_004 <see cref="ICityControlAuthority"/> 发起</b>（生涯层只读、不直接写 city.owner，ADR-0008）。
    /// </summary>
    public sealed class GovernorCampaignService
    {
        private readonly ICityControlAuthority _control;
        private readonly GovernorOutcomeService _outcome = new GovernorOutcomeService();
        private CityControlChanged? _lastControlChange;

        /// <summary>注入城池控制权权威（ADR-0008：仅 GDD_004 实现）。订阅一次以捕获本服务发起的归属变更。</summary>
        public GovernorCampaignService(ICityControlAuthority cityControlAuthority)
        {
            _control = cityControlAuthority ?? throw new ArgumentNullException(nameof(cityControlAuthority));
            _control.Subscribe(e => _lastControlChange = e);
        }

        /// <summary>
        /// 太守开局绑定：生涯绑为该势力某城太守，禀赋来自 <see cref="CitySeed"/>；登记开局城池归属（权威态）。
        /// </summary>
        public CareerSnapshot BeginGovernorStart(CitySeed seed)
        {
            if (seed is null) throw new ArgumentNullException(nameof(seed));

            // 登记开局城池归属：势力据有该城（权威态在 GDD_004）。
            if (_control is CityControlAuthority authority)
                authority.RegisterInitial(seed.City, seed.Faction, new Garrison(seed.Garrison));

            var career = CareerState.NewGovernor(seed.Faction, FixedPoint.Zero);
            var retinue = new RetinueState(seed.Retinue, Array.Empty<System.Collections.Generic.KeyValuePair<OfficeRole, Domain.Characters.CharacterId>>());
            return new CareerSnapshot(career, retinue);
        }

        /// <summary>
        /// 守城开局事件后果：胜→初始功绩/信任 + 解锁权限；败→转在野（保留部曲）+ 经 GDD_004 发起失城归属变更。
        /// 确定性：同一前态 + 同一 BattleOutcome → 同一结果。
        /// </summary>
        public SiegeResolutionResult ResolveSiege(
            CareerSnapshot before, SiegeOutcome outcome, GovernorStartConfig config, SiegeContext context)
        {
            if (before is null) throw new ArgumentNullException(nameof(before));
            if (config is null) throw new ArgumentNullException(nameof(config));

            if (outcome == SiegeOutcome.Defended)
            {
                CareerCommandResult win = _outcome.ResolveDefended(before, config);
                return new SiegeResolutionResult(SiegeOutcome.Defended, win.Snapshot, fullAuthorityUnlocked: true, controlChange: null);
            }

            // 失守：先生涯转在野（合法可继续），再经 GDD_004 发起失城归属变更（生涯层不直接写）。
            CareerSnapshot wandering = _outcome.ResolveFallen(before);

            _lastControlChange = null;
            _control.RequestControlChange(context.City, context.EnemyFaction, context.EnemyGarrison, ChangeCause.SiegeDefenseLost);

            return new SiegeResolutionResult(SiegeOutcome.Fallen, wandering, fullAuthorityUnlocked: false, controlChange: _lastControlChange);
        }
    }
}
