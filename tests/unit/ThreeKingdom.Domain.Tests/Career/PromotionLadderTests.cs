using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// epic-011 story-002：忠臣晋升逐级门槛与功绩/名望累积。
    /// 治理 ADR：ADR-0003（数据驱动配置 + 指纹）+ ADR-0004（确定性）。GDD_014 §Formula 1 / TR-career-002。
    /// 覆盖 AC-1/2/3 三项独立门槛、AC-4/5 来源累积+W5 非战斗护栏、AC-6 前 2-3 阶端到端确定性。
    /// </summary>
    [TestFixture]
    public class PromotionLadderTests
    {
        private static readonly FactionId Lord = new FactionId("faction-cao");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        // 长度 = 官阶数（8）；按阶非递减。索引 = 目标官阶。
        private static readonly int[] MeritReq = { 0, 100, 250, 450, 700, 1000, 1400, 2000 };
        private static readonly int[] RenownReq = { 0, 50, 150, 300, 500, 800, 1200, 1800 };

        private static FixedPoint[] StandingReq() => new[]
        {
            FixedPoint.Zero, Frac(3, 10), Frac(5, 10), Frac(6, 10),
            Frac(7, 10), Frac(8, 10), Frac(9, 10), FixedPoint.One,
        };

        private static Dictionary<CareerGainSource, CareerGain> Gains() => new Dictionary<CareerGainSource, CareerGain>
        {
            [CareerGainSource.CombatVictory] = new CareerGain(40, 10, Frac(2, 100)),
            [CareerGainSource.MajorBattleVictory] = new CareerGain(80, 50, Frac(5, 100)),
            [CareerGainSource.LordMissionComplete] = new CareerGain(50, 20, Frac(6, 100)),   // 非战斗，merit 竞争力
            [CareerGainSource.CityGovernance] = new CareerGain(45, 15, Frac(3, 100)),
            [CareerGainSource.RebellionSuppressed] = new CareerGain(60, 40, Frac(4, 100)),
            [CareerGainSource.TalentRecruited] = new CareerGain(20, 35, Frac(2, 100)),
        };

        private static PromotionLadderConfig Config()
            => new PromotionLadderConfig(MeritReq, RenownReq, StandingReq(), Gains());

        private static CareerSnapshot SnapshotAt(int merit, int renown, FixedPoint standing, Rank rank)
            => new CareerSnapshot(
                new CareerState(merit, renown, standing, rank, Lord, isUnaffiliated: false),
                RetinueState.Empty);

        private static CareerSnapshot Governor()
            => new CareerSnapshot(CareerState.NewGovernor(Lord, FixedPoint.Zero), RetinueState.Empty);

        private static readonly CareerProgressionService Service = new CareerProgressionService();

        // ---- AC-1 / AC-2 / AC-3：三项门槛独立判定 ----

        [Test]
        public void test_promote_requires_all_three_criteria()
        {
            PromotionLadderConfig cfg = Config();
            // 目标 rank1 门槛：merit 100, renown 50, standing 0.3。
            CareerState atThreshold = new CareerState(100, 50, Frac(3, 10), Rank.CityGovernor, Lord, false);
            PromotionCheck ok = Service.Check(cfg, new CareerSnapshot(atThreshold, RetinueState.Empty));
            Assert.That(ok.CanPromote, Is.True, "恰好等于门槛应通过（≥ 边界）。");
        }

        [Test]
        public void test_each_single_criterion_shortfall_blocks_promotion()
        {
            PromotionLadderConfig cfg = Config();

            PromotionCheck meritShort = Service.Check(cfg, SnapshotAt(99, 50, Frac(3, 10), Rank.CityGovernor));
            Assert.That(meritShort.CanPromote, Is.False);
            Assert.That(meritShort.MeritMet, Is.False);
            Assert.That(meritShort.RenownMet, Is.True);
            Assert.That(meritShort.StandingMet, Is.True);
            Assert.That(meritShort.MeritShortfall, Is.EqualTo(1));

            PromotionCheck renownShort = Service.Check(cfg, SnapshotAt(100, 49, Frac(3, 10), Rank.CityGovernor));
            Assert.That(renownShort.CanPromote, Is.False);
            Assert.That(renownShort.RenownMet, Is.False);
            Assert.That(renownShort.RenownShortfall, Is.EqualTo(1));

            PromotionCheck standingShort = Service.Check(cfg, SnapshotAt(100, 50, Frac(29, 100), Rank.CityGovernor));
            Assert.That(standingShort.CanPromote, Is.False);
            Assert.That(standingShort.StandingMet, Is.False);
            Assert.That(standingShort.StandingShortfall, Is.GreaterThan(FixedPoint.Zero));
        }

        [Test]
        public void test_request_promotion_below_threshold_returns_stable_code_unchanged()
        {
            PromotionLadderConfig cfg = Config();
            CareerSnapshot before = SnapshotAt(50, 20, Frac(1, 10), Rank.CityGovernor);
            StateHash hashBefore = before.ComputeHash();

            CareerCommandResult result = Service.RequestPromotion(cfg, before);

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.PromotionThresholdNotMet));
            Assert.That(result.Snapshot.ComputeHash(), Is.EqualTo(hashBefore));
            Assert.That(result.Snapshot.Career.Rank, Is.EqualTo(Rank.CityGovernor));
        }

        // ---- 配置校验：按阶递增 + 长度 ----

        [Test]
        public void test_config_rejects_non_monotonic_thresholds()
        {
            int[] badMerit = { 0, 100, 80, 450, 700, 1000, 1400, 2000 }; // rank2 < rank1
            Assert.Throws<ArgumentException>(
                () => new PromotionLadderConfig(badMerit, RenownReq, StandingReq(), Gains()));
        }

        [Test]
        public void test_config_rejects_wrong_length()
        {
            int[] shortArr = { 0, 100, 250 };
            Assert.Throws<ArgumentException>(
                () => new PromotionLadderConfig(shortArr, RenownReq, StandingReq(), Gains()));
        }

        [Test]
        public void test_config_fingerprint_is_deterministic_and_sensitive()
        {
            Assert.That(Config().ConfigFingerprint, Is.EqualTo(Config().ConfigFingerprint));

            int[] tweaked = (int[])MeritReq.Clone();
            tweaked[2] += 1;
            var other = new PromotionLadderConfig(tweaked, RenownReq, StandingReq(), Gains());
            Assert.That(other.ConfigFingerprint, Is.Not.EqualTo(Config().ConfigFingerprint));
        }

        // ---- AC-4 / AC-5：来源累积 + 非战斗护栏 ----

        [Test]
        public void test_gain_accumulates_by_configured_weight()
        {
            PromotionLadderConfig cfg = Config();
            CareerGain combat = cfg.GainFor(CareerGainSource.CombatVictory)!;

            CareerCommandResult r = Service.ApplyGain(cfg, Governor(), CareerGainSource.CombatVictory);
            Assert.That(r.Applied, Is.True);
            Assert.That(r.Snapshot.Career.Merit, Is.EqualTo(combat.Merit));
            Assert.That(r.Snapshot.Career.Renown, Is.EqualTo(combat.Renown));
            Assert.That(r.Snapshot.Career.LordStanding, Is.EqualTo(combat.Standing));
        }

        [Test]
        public void test_gain_scales_with_count()
        {
            PromotionLadderConfig cfg = Config();
            CareerCommandResult r = Service.ApplyGain(cfg, Governor(), CareerGainSource.LordMissionComplete, count: 3);
            Assert.That(r.Snapshot.Career.Merit, Is.EqualTo(150)); // 50×3
            Assert.That(r.Snapshot.Career.Renown, Is.EqualTo(60)); // 20×3
        }

        [Test]
        public void test_unconfigured_source_contributes_nothing()
        {
            var partial = new Dictionary<CareerGainSource, CareerGain>(Gains());
            partial.Remove(CareerGainSource.TalentRecruited);
            var cfg = new PromotionLadderConfig(MeritReq, RenownReq, StandingReq(), partial);

            CareerSnapshot before = Governor();
            CareerCommandResult r = Service.ApplyGain(cfg, before, CareerGainSource.TalentRecruited);
            Assert.That(r.Applied, Is.True);
            Assert.That(r.Snapshot.ComputeHash(), Is.EqualTo(before.ComputeHash()));
        }

        [Test]
        public void test_non_combat_sources_are_rate_competitive_guardrail()
        {
            // W5：非战斗最大单次 merit(60) ≥ 战斗最大单次 merit(80) × 0.5 = 40。
            Assert.That(Config().SatisfiesNonCombatGuardrail(Frac(1, 2)), Is.True);
        }

        // ---- AC-6：前 2-3 阶端到端 + 确定性 ----

        [Test]
        public void test_three_tier_promotion_end_to_end_is_deterministic()
        {
            CareerSnapshot a = RunCareerScript();
            CareerSnapshot b = RunCareerScript();

            Assert.That(a.Career.Rank, Is.EqualTo(Rank.ProvincialInspector)); // 太守(0)→资深太守(1)→州刺史(2)
            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));        // 同输入同结果
        }

        [Test]
        public void test_promotion_stops_when_tier_threshold_unmet()
        {
            PromotionLadderConfig cfg = Config();
            // 仅够到 rank1，不够 rank2。
            CareerSnapshot s = SnapshotAt(120, 60, Frac(35, 100), Rank.CityGovernor);
            CareerCommandResult toRank1 = Service.RequestPromotion(cfg, s);
            Assert.That(toRank1.Applied, Is.True);
            Assert.That(toRank1.Snapshot.Career.Rank, Is.EqualTo(Rank.SeniorGovernor));

            CareerCommandResult toRank2 = Service.RequestPromotion(cfg, toRank1.Snapshot);
            Assert.That(toRank2.Applied, Is.False);
            Assert.That(toRank2.Error, Is.EqualTo(CareerErrorCode.PromotionThresholdNotMet));
            Assert.That(toRank2.Snapshot.Career.Rank, Is.EqualTo(Rank.SeniorGovernor));
        }

        private static CareerSnapshot RunCareerScript()
        {
            PromotionLadderConfig cfg = Config();
            var advancement = new LoyalistAdvancementService(cfg);

            CareerSnapshot s = Governor();
            s = Apply(advancement.RecordGain(s, CareerGainSource.LordMissionComplete, 6)); // merit300 renown120 standing0.36
            s = Apply(advancement.RecordGain(s, CareerGainSource.MajorBattleVictory, 3));  // +240/+150/+0.15
            s = Apply(advancement.RecordGain(s, CareerGainSource.CityGovernance, 6));      // +270/+90/+0.18
            // 现 merit 810, renown 360, standing 0.69 — 远超 rank1/rank2 门槛。
            s = Apply(advancement.RequestPromotion(s)); // → 资深太守
            s = Apply(advancement.RequestPromotion(s)); // → 州刺史
            return s;
        }

        private static CareerSnapshot Apply(CareerCommandResult r)
        {
            Assert.That(r.Applied, Is.True, $"步骤失败：{r.Error} {r.Detail}");
            return r.Snapshot;
        }
    }
}
