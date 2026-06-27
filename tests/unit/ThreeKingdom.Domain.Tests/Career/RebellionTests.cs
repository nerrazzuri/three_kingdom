using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// epic-011 story-003：自立触发判定与三分支结局。
    /// 治理 ADR：ADR-0004（确定性 + 好感快照隔离）+ ADR-0003（配置阈值）。GDD_014 §Formula 2/3 / TR-career-002。
    /// 覆盖 AC-1/2 三组触发独立、AC-3/4 三分支由快照确定性+隔离、AC-5 阈值配置化、AC-6 众叛可继续+N=0 不除零。
    /// </summary>
    [TestFixture]
    public class RebellionTests
    {
        private static readonly FactionId OldLord = new FactionId("faction-cao");
        private static readonly FactionId NewState = new FactionId("faction-player-new");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static RebellionConfig Config()
            => new RebellionConfig(
                rebelCityMin: 3,
                rebelRenownMin: 400,
                rebelAffinityMin: Frac(5, 10),
                defectThreshold: Frac(5, 10),
                loyalRatioHi: Frac(7, 10),
                loyalRatioMid: Frac(4, 10));

        private static readonly RebellionService Service = new RebellionService();

        private static RetinueState Retinue(int loyal, int disloyal)
        {
            var members = new List<RetinueMember>();
            for (int i = 0; i < loyal; i++)
                members.Add(new RetinueMember(new CharacterId($"char-L{i}"), Frac(9, 10)));   // ≥ defect 0.5
            for (int i = 0; i < disloyal; i++)
                members.Add(new RetinueMember(new CharacterId($"char-D{i}"), Frac(1, 10)));   // < defect
            return new RetinueState(members, Array.Empty<KeyValuePair<OfficeRole, CharacterId>>());
        }

        private static RetinueState UniformRetinue(int n, FixedPoint affinity)
        {
            var members = new List<RetinueMember>();
            for (int i = 0; i < n; i++)
                members.Add(new RetinueMember(new CharacterId($"char-{i}"), affinity));
            return new RetinueState(members, Array.Empty<KeyValuePair<OfficeRole, CharacterId>>());
        }

        private static CareerSnapshot Snapshot(int renown, RetinueState retinue)
            => new CareerSnapshot(new CareerState(0, renown, Frac(2, 10), Rank.CityGovernor, OldLord, false), retinue);

        // ---- AC-1 / AC-2：三组触发条件独立 ----

        [Test]
        public void test_military_group_alone_enables_rebellion()
        {
            // cities≥3 ∧ supply ∧ troops；renown=0 使第 2 组失效；无压迫。
            var ctx = new RebellionContext(3, supplyReady: true, troopsReady: true, lordOppression: false, NewState);
            RebellionEligibility e = Service.CheckEligibility(Config(), Snapshot(0, Retinue(0, 5)).Career, Retinue(0, 5), ctx);

            Assert.That(e.CanRebel, Is.True);
            Assert.That(e.MilitaryGroupMet, Is.True);
            Assert.That(e.PopularGroupMet, Is.False);
            Assert.That(e.OppressionMet, Is.False);
        }

        [Test]
        public void test_popular_group_alone_enables_rebellion()
        {
            // renown≥400 ∧ avg(affinity)≥0.5；cities=0 使第 1 组失效；无压迫。
            RetinueState ret = UniformRetinue(4, Frac(6, 10));
            var ctx = new RebellionContext(0, supplyReady: false, troopsReady: false, lordOppression: false, NewState);
            RebellionEligibility e = Service.CheckEligibility(Config(), Snapshot(400, ret).Career, ret, ctx);

            Assert.That(e.CanRebel, Is.True);
            Assert.That(e.PopularGroupMet, Is.True);
            Assert.That(e.MilitaryGroupMet, Is.False);
        }

        [Test]
        public void test_oppression_flag_alone_enables_rebellion()
        {
            var ctx = new RebellionContext(0, false, false, lordOppression: true, NewState);
            RebellionEligibility e = Service.CheckEligibility(Config(), Snapshot(0, Retinue(0, 3)).Career, Retinue(0, 3), ctx);

            Assert.That(e.CanRebel, Is.True);
            Assert.That(e.OppressionMet, Is.True);
            Assert.That(e.MilitaryGroupMet, Is.False);
            Assert.That(e.PopularGroupMet, Is.False);
        }

        [Test]
        public void test_no_group_met_cannot_rebel()
        {
            var ctx = new RebellionContext(2, supplyReady: true, troopsReady: false, lordOppression: false, NewState);
            RebellionEligibility e = Service.CheckEligibility(Config(), Snapshot(100, Retinue(0, 3)).Career, Retinue(0, 3), ctx);
            Assert.That(e.CanRebel, Is.False);
        }

        [Test]
        public void test_military_group_city_boundary_is_inclusive()
        {
            // cities 恰等于 rebel_city_min(3) → 第 1 组成立（≥ 边界）。
            var ctx = new RebellionContext(3, true, true, false, NewState);
            Assert.That(Service.CheckEligibility(Config(), Snapshot(0, Retinue(0, 1)).Career, Retinue(0, 1), ctx).MilitaryGroupMet, Is.True);
        }

        [Test]
        public void test_launch_when_ineligible_returns_stable_code_unchanged()
        {
            CareerSnapshot before = Snapshot(100, Retinue(0, 3));
            StateHash hashBefore = before.ComputeHash();
            var ctx = new RebellionContext(0, false, false, false, NewState);

            RebellionResult r = Service.Launch(Config(), before, ctx);

            Assert.That(r.Launched, Is.False);
            Assert.That(r.Error, Is.EqualTo(CareerErrorCode.RebellionConditionNotMet));
            Assert.That(r.Rebellion, Is.Null);
            Assert.That(r.Snapshot.ComputeHash(), Is.EqualTo(hashBefore));
        }

        // ---- AC-3 / AC-4：三分支由好感快照确定性 ----

        [Test]
        public void test_full_support_when_ratio_at_or_above_hi()
        {
            // 7 忠 / 3 叛 → ratio = 0.7 = hi → 全员拥立。
            RebellionResult r = Launch(Retinue(7, 3));
            Assert.That(r.Launched, Is.True);
            Assert.That(r.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.FullSupport));
            Assert.That(r.Snapshot.Career.IsUnaffiliated, Is.False);
            Assert.That(r.Snapshot.Career.Faction, Is.EqualTo(NewState));
        }

        [Test]
        public void test_partial_follow_when_ratio_in_mid_band()
        {
            // 4 忠 / 6 叛 → ratio = 0.4 = mid → 部分跟随。
            RebellionResult r = Launch(Retinue(4, 6));
            Assert.That(r.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.PartialFollow));
            Assert.That(r.Snapshot.Career.Faction, Is.EqualTo(NewState));
        }

        [Test]
        public void test_abandoned_when_ratio_below_mid()
        {
            // 2 忠 / 8 叛 → ratio = 0.2 < mid → 众叛亲离。
            RebellionResult r = Launch(Retinue(2, 8));
            Assert.That(r.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.Abandoned));
        }

        [Test]
        public void test_outcome_is_deterministic_for_same_snapshot()
        {
            StateHash a = Launch(Retinue(7, 3)).Rebellion!.ComputeHashOf();
            StateHash b = Launch(Retinue(7, 3)).Rebellion!.ComputeHashOf();
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void test_snapshot_isolation_captures_affinity_at_launch()
        {
            // 发动时好感快照固化：返回的 RebellionState 持发动瞬间的 10 个好感值，
            // 之后另建不同好感的 retinue 不影响已产出的结局（对象不可变）。
            RebellionResult r = Launch(Retinue(7, 3));
            Assert.That(r.Rebellion!.AffinitySnapshot.Count, Is.EqualTo(10));
            Assert.That(r.Rebellion.Outcome, Is.EqualTo(RebellionOutcome.FullSupport));

            // 用全叛逆 retinue 重新发动得不同结局，但不改前一次已定结局。
            RebellionResult r2 = Launch(Retinue(0, 10));
            Assert.That(r2.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.Abandoned));
            Assert.That(r.Rebellion.Outcome, Is.EqualTo(RebellionOutcome.FullSupport)); // 隔离
        }

        // ---- AC-6：众叛可继续 + N=0 不除零 ----

        [Test]
        public void test_abandoned_produces_legal_wandering_state()
        {
            RebellionResult r = Launch(Retinue(0, 10));
            Assert.That(r.Launched, Is.True);
            Assert.That(r.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.Abandoned));
            Assert.That(r.Snapshot.Career.IsUnaffiliated, Is.True);   // 流浪势力，合法可继续
            Assert.That(r.Snapshot.Career.Faction, Is.Null);
            Assert.That(r.Rebellion.NewFaction, Is.Null);
        }

        [Test]
        public void test_zero_retinue_does_not_divide_by_zero()
        {
            // N=0：loyal_ratio 取 0 → Abandoned；经第 1 组发动条件成立。
            RetinueState empty = RetinueState.Empty;
            var before = new CareerSnapshot(new CareerState(0, 0, Frac(2, 10), Rank.CityGovernor, OldLord, false), empty);
            var ctx = new RebellionContext(3, true, true, false, NewState);

            RebellionResult r = Service.Launch(Config(), before, ctx);
            Assert.That(r.Launched, Is.True);
            Assert.That(r.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.Abandoned));
            Assert.That(r.Rebellion.LoyalRatio, Is.EqualTo(FixedPoint.Zero));
        }

        [Test]
        public void test_launch_without_new_faction_id_throws()
        {
            var before = Snapshot(0, Retinue(7, 3));
            var ctx = new RebellionContext(3, true, true, false, newFactionId: null);
            Assert.Throws<ArgumentException>(() => Service.Launch(Config(), before, ctx));
        }

        // 用第 1 组（城池+补给+兵力）保证可发动，从而隔离测试结局分支。
        private static RebellionResult Launch(RetinueState retinue)
        {
            var before = new CareerSnapshot(new CareerState(0, 0, Frac(2, 10), Rank.CityGovernor, OldLord, false), retinue);
            var ctx = new RebellionContext(3, supplyReady: true, troopsReady: true, lordOppression: false, NewState);
            return Service.Launch(Config(), before, ctx);
        }
    }

    internal static class RebellionStateTestExtensions
    {
        /// <summary>测试辅助：取 RebellionState 的确定性哈希。</summary>
        public static StateHash ComputeHashOf(this RebellionState state)
        {
            var hasher = new StateHasher();
            state.AppendTo(hasher);
            return hasher.ToHash();
        }
    }
}
