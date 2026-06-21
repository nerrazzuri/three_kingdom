// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 锁定确定性、断粮单点、2倍劣势翻盘等核心不变量
// Date: 2026-06-21

using NUnit.Framework;
using TkSlice.Application;
using TkSlice.Domain.Battle;
using TkSlice.Domain.Config;
using TkSlice.Domain.Diplomacy;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Siege;
using TkSlice.Domain.Time;
using TkSlice.Infrastructure.Save;

namespace TkSlice.Tests
{
    public class FixedTests
    {
        [Test]
        public void Add_Mul_AreApproximatelyCorrect()
        {
            var a = Fixed.FromFraction(7, 10);
            var b = Fixed.FromFraction(3, 10);
            // 0.7+0.3 ≈ 1（量化误差 ≤ 1 raw）
            Assert.That(System.Math.Abs((a + b).Raw - Fixed.One), Is.LessThanOrEqualTo(1));
            // 0.7*0.3 = 0.21 → raw ≈ 0.21*65536 = 13762
            Assert.That(System.Math.Abs((a * b).Raw - 13762), Is.LessThanOrEqualTo(2));
        }

        [Test]
        public void Clamp_BoundsValue()
        {
            var v = Fixed.FromFraction(15, 10);
            Assert.That(Fixed.Clamp(v, Fixed.Zero, Fixed.OneValue), Is.EqualTo(Fixed.OneValue));
        }
    }

    public class DetRngTests
    {
        [Test]
        public void SameSeedAndName_ProduceSameSequence()
        {
            var a = DetRng.Fork(20260621UL, "x");
            var b = DetRng.Fork(20260621UL, "x");
            for (int i = 0; i < 50; i++)
                Assert.That(a.NextUInt64(), Is.EqualTo(b.NextUInt64()));
        }

        [Test]
        public void DifferentNames_Diverge()
        {
            var a = DetRng.Fork(20260621UL, "x");
            var b = DetRng.Fork(20260621UL, "y");
            Assert.That(a.NextUInt64(), Is.Not.EqualTo(b.NextUInt64()));
        }

        [Test]
        public void NextInt_StaysInRange()
        {
            var r = DetRng.Fork(1, "r");
            for (int i = 0; i < 1000; i++)
            {
                int v = r.NextInt(7);
                Assert.That(v, Is.InRange(0, 6));
            }
        }
    }

    public class StarvationSinglePointTests
    {
        [Test]
        public void NoStarvation_BeforeGracePeriod()
        {
            var cfg = SiegeConfig.Default();
            var f = new ForceState("e", Side.Attacker, 1000,
                Fixed.FromFraction(70, 100), Fixed.FromFraction(25, 100),
                Fixed.FromFraction(60, 100), Fixed.OneValue);

            // 短缺 1、2 段（< grace 3）→ 不施加后果
            f.ApplySupplyCut(cfg);
            Assert.That(f.ApplyStarvationConsequence(cfg).Applied, Is.False);
            f.ApplySupplyCut(cfg);
            Assert.That(f.ApplyStarvationConsequence(cfg).Applied, Is.False);
            // 士气未变（后果未施加）
            Assert.That(f.UnitMorale, Is.EqualTo(Fixed.FromFraction(70, 100)));
        }

        [Test]
        public void Starvation_AppliesOnceAtOrAfterGrace()
        {
            var cfg = SiegeConfig.Default();
            var f = new ForceState("e", Side.Attacker, 1000,
                Fixed.FromFraction(70, 100), Fixed.FromFraction(25, 100),
                Fixed.FromFraction(60, 100), Fixed.OneValue);

            for (int i = 0; i < cfg.SupplyGracePeriod; i++) f.ApplySupplyCut(cfg);
            var before = f.UnitMorale;
            var eff = f.ApplyStarvationConsequence(cfg);
            Assert.That(eff.Applied, Is.True);
            // 恰好下降一次惩罚量（幂等单点，未被重复施加）
            Assert.That(before.Raw - f.UnitMorale.Raw, Is.EqualTo(cfg.StarveMoralePenalty.Raw));
        }

        [Test]
        public void SupplyRestore_ResetsShortageCounter()
        {
            var cfg = SiegeConfig.Default();
            var f = new ForceState("e", Side.Attacker, 1000,
                Fixed.FromFraction(70, 100), Fixed.FromFraction(25, 100),
                Fixed.FromFraction(60, 100), Fixed.OneValue);
            f.ApplySupplyCut(cfg); f.ApplySupplyCut(cfg);
            Assert.That(f.ShortageSegments, Is.EqualTo(2));
            f.ApplySupplyRestore(cfg);
            Assert.That(f.ShortageSegments, Is.EqualTo(0));
        }
    }

    public class TimeTests
    {
        [Test]
        public void Advance_DerivesDayAndSegment()
        {
            var d = WorldDay.Start;
            Assert.That(d.Day, Is.EqualTo(1));
            Assert.That(d.Segment, Is.EqualTo(DaySegment.Dawn));
            var d2 = d.Advance(4);
            Assert.That(d2.Day, Is.EqualTo(2));
            Assert.That(d2.Segment, Is.EqualTo(DaySegment.Dawn));
        }
    }

    public class SiegeScenarioTests
    {
        // 核心翻盘不变量：正面硬守必败；断粮疲敌可翻盘
        [Test]
        public void Baseline_FrontalDefense_CityFalls()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            var r = svc.ResolveAssault(new ResolveAssaultCommand());
            Assert.That(r.Outcome, Is.EqualTo(BattleOutcome.AttackerDecisive),
                "2 倍兵力正面对抗应城破——验证「必须创造条件」的前提");
        }

        [Test]
        public void CutSupplyChain_RepelsAssault()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            Assert.That(svc.CommitRaid(new CommitRaidCommand(2)).Ok, Is.True);
            Assert.That(svc.Advance(new AdvanceSegmentCommand(10)).Ok, Is.True);
            var r = svc.ResolveAssault(new ResolveAssaultCommand());
            Assert.That(r.Outcome, Is.EqualTo(BattleOutcome.AttackerRepelled),
                "断粮疲敌后应击退强攻——验证「创造条件取胜」");
        }

        [Test]
        public void Determinism_SameSeed_SameFinalHash()
        {
            long Run()
            {
                var state = SiegeScenario.CreateXishuiSiege();
                var svc = new SiegeService(state);
                svc.CommitRaid(new CommitRaidCommand(2));
                svc.Advance(new AdvanceSegmentCommand(10));
                svc.ResolveAssault(new ResolveAssaultCommand());
                return state.StateHash();
            }
            Assert.That(Run(), Is.EqualTo(Run()), "相同种子与命令流 → 相同最终状态哈希（ADR-0004）");
        }

        [Test]
        public void CommandValidation_RejectsNonPositiveRaid()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            var r = svc.CommitRaid(new CommitRaidCommand(0));
            Assert.That(r.Ok, Is.False);
            Assert.That(r.Code, Is.EqualTo("ERR_RAID_UNITS_NONPOSITIVE"));
        }

        // 链 2：假退伏击
        [Test]
        public void FeignedRetreat_RecklessEnemy_SpringsAmbush()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            int before = state.Attacker.Troops;
            var r = svc.FeignedRetreat(new FeignedRetreatCommand(150, 350), out var chk);
            Assert.That(chk.Ok, Is.True);
            Assert.That(r.Outcome, Is.EqualTo(FeignOutcome.AmbushSprung));
            Assert.That(r.Ambush!.Outcome, Is.EqualTo(BattleOutcome.AttackerDecisive),
                "鲁莽敌将追击落伏 → 伏兵突然性重创追击支队");
            Assert.That(state.Attacker.Troops, Is.LessThan(before), "敌军应因伏击减员");
        }

        [Test]
        public void FeignedRetreat_RejectsForceExceedingGarrison()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.FeignedRetreat(new FeignedRetreatCommand(500, 500), out var chk);
            Assert.That(chk.Ok, Is.False);
            Assert.That(chk.Code, Is.EqualTo("ERR_FEIGN_FORCE_EXCEEDS_GARRISON"));
        }
    }

    // AmbushResolver 纯函数：三种条件分支
    public class AmbushResolverTests
    {
        private static ForceState Enemy() => new("e", Side.Attacker, 1200,
            Fixed.FromFraction(70, 100), Fixed.FromFraction(25, 100),
            Fixed.FromFraction(60, 100), Fixed.OneValue);

        [Test]
        public void LowDiscipline_BecomesRealRout()
        {
            var cfg = SiegeConfig.Default();
            var r = AmbushResolver.Resolve(
                Fixed.FromFraction(30, 100), 150, 350,
                Fixed.FromFraction(60, 100), Fixed.FromFraction(30, 100),
                Enemy(), Fixed.FromFraction(65, 100), Fixed.FromFraction(10, 100), cfg);
            Assert.That(r.Outcome, Is.EqualTo(FeignOutcome.RoutFailure));
        }

        [Test]
        public void CautiousEnemy_DoesNotPursue()
        {
            var cfg = SiegeConfig.Default();
            var r = AmbushResolver.Resolve(
                Fixed.FromFraction(65, 100), 150, 350,
                Fixed.FromFraction(60, 100), Fixed.FromFraction(65, 100),
                Enemy(), Fixed.FromFraction(-50, 100), Fixed.FromFraction(10, 100), cfg);
            Assert.That(r.Outcome, Is.EqualTo(FeignOutcome.NotPursued));
        }
    }

    // 链 3：守城待变（外交受控入口 GDD_012 §8）
    public class DiplomacyTests
    {
        [Test]
        public void GrantScore_AcceptsWithGoodStandingAndCost()
        {
            var cfg = SiegeConfig.Default();
            var g = DiplomacyEvaluator.GrantScore(cfg,
                Fixed.FromFraction(55, 100), Fixed.FromFraction(60, 100), Fixed.Zero);
            Assert.That(DiplomacyEvaluator.Respond(cfg, g), Is.EqualTo(PledgeResponse.Accept));
        }

        [Test]
        public void GrantScore_RejectsWithLowStandingAndPressure()
        {
            var cfg = SiegeConfig.Default();
            var g = DiplomacyEvaluator.GrantScore(cfg,
                Fixed.FromFraction(10, 100), Fixed.FromFraction(10, 100), Fixed.FromFraction(50, 100));
            Assert.That(DiplomacyEvaluator.Respond(cfg, g), Is.EqualTo(PledgeResponse.Reject));
        }

        [Test]
        public void Relief_IsDelayed_NotInstant()
        {
            var cfg = SiegeConfig.Default();
            var pledge = DiplomacyEvaluator.Create(cfg, PledgeType.Relief,
                Fixed.FromFraction(55, 100), Fixed.FromFraction(60, 100), Fixed.Zero, WorldDay.Start);
            // 交付时刻必须在 commit_lead 之后，绝非即到
            Assert.That(pledge.ArrivalT.TotalSegments, Is.EqualTo(cfg.DiplomacyCommitLead));
        }

        [Test]
        public void Pledge_ResolvesByArrival_NotPendingForever()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            var (chk, pledge) = svc.RequestDiplomacy(new RequestDiplomacyCommand(PledgeType.Relief, 60));
            Assert.That(chk.Ok, Is.True);
            Assert.That(pledge!.Response, Is.EqualTo(PledgeResponse.Accept));
            svc.Advance(new AdvanceSegmentCommand(SiegeConfig.Default().DiplomacyCommitLead + 1));
            Assert.That(pledge.Status, Is.Not.EqualTo(PledgeStatus.Pending),
                "过交付时刻后须结算为 Fulfilled 或 Betrayed，不能永远 Pending");
            Assert.That(state.PendingPledge, Is.Null);
        }

        [Test]
        public void HoldAndAwait_ReliefRepelsAssault()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.RequestDiplomacy(new RequestDiplomacyCommand(PledgeType.Relief, 60));
            svc.Advance(new AdvanceSegmentCommand(6));
            var r = svc.ResolveAssault(new ResolveAssaultCommand());
            // 该种子下援军如约抵达 → 击退（验证守城待变可成立）
            Assert.That(r.Outcome, Is.EqualTo(BattleOutcome.AttackerRepelled));
        }
    }

    // 存档 round-trip（ADR-0005）
    public class SaveLoadTests
    {
        private static readonly SiegeConfig Cfg = SiegeConfig.Default();

        [Test]
        public void RoundTrip_PreservesStateHash()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.CommitRaid(new CommitRaidCommand(2));
            svc.Advance(new AdvanceSegmentCommand(5));

            string json = SiegeSaveSerializer.Serialize(state);
            var restored = SiegeSaveSerializer.Deserialize(json, Cfg);
            Assert.That(restored.StateHash(), Is.EqualTo(state.StateHash()));
        }

        [Test]
        public void RoundTrip_ContinuationStaysDeterministic()
        {
            var a = SiegeScenario.CreateXishuiSiege();
            var sa = new SiegeService(a);
            sa.CommitRaid(new CommitRaidCommand(2));
            sa.Advance(new AdvanceSegmentCommand(5));

            string json = SiegeSaveSerializer.Serialize(a);
            var b = SiegeSaveSerializer.Deserialize(json, Cfg);

            // 读档后继续推进，须与原局推进完全一致（证明 RNG 内部状态也已恢复）
            sa.Advance(new AdvanceSegmentCommand(4));
            new SiegeService(b).Advance(new AdvanceSegmentCommand(4));
            Assert.That(b.StateHash(), Is.EqualTo(a.StateHash()));
        }

        [Test]
        public void RoundTrip_PendingPledgeSurvives()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.RequestDiplomacy(new RequestDiplomacyCommand(PledgeType.Relief, 60));
            svc.Advance(new AdvanceSegmentCommand(1));   // 仍在途（commit_lead=4）

            var restored = SiegeSaveSerializer.Deserialize(SiegeSaveSerializer.Serialize(state), Cfg);
            Assert.That(restored.PendingPledge, Is.Not.Null);
            Assert.That(restored.PendingPledge!.Type, Is.EqualTo(PledgeType.Relief));
        }

        [Test]
        public void FutureSchemaVersion_IsRejected()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            string json = SiegeSaveSerializer.Serialize(state)
                .Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 99");
            Assert.Throws<System.IO.InvalidDataException>(
                () => SiegeSaveSerializer.Deserialize(json, Cfg));
        }

        [Test]
        public void RoundTrip_PreservesIntelAndReinforcement()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.Scout(new ScoutCommand());                       // 刷新情报
            svc.Advance(new AdvanceSegmentCommand(SiegeConfig.Default().EnemyReinforceSegment)); // 触发敌援军

            var restored = SiegeSaveSerializer.Deserialize(SiegeSaveSerializer.Serialize(state), Cfg);
            Assert.That(restored.EnemyReinforced, Is.EqualTo(state.EnemyReinforced));
            Assert.That(restored.Intel.EstTroops, Is.EqualTo(state.Intel.EstTroops));
            Assert.That(restored.Intel.Confidence, Is.EqualTo(state.Intel.Confidence));
        }
    }

    // 双边补给博弈 + 敌军援军（回应「敌军也有补给/援军」）
    public class TwoSidedSupplyTests
    {
        [Test]
        public void UnderInvestedRaid_FailsToStarveEnemy()
        {
            // 仅 1 支袭扰队 → 压不过敌护卫，敌补给车队反复回补 → 补给保持充足
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.CommitRaid(new CommitRaidCommand(1));
            svc.Advance(new AdvanceSegmentCommand(8));
            Assert.That(state.Attacker.SupplyState, Is.GreaterThan(Fixed.FromFraction(60, 100)),
                "投入不足时敌补给应保持充足——断粮是双边博弈，非单边必成");
        }

        [Test]
        public void WellInvestedRaid_DoesStarveEnemy()
        {
            // 3 支袭扰队 → 压过护卫 → 补给真正枯竭
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.CommitRaid(new CommitRaidCommand(3));
            svc.Advance(new AdvanceSegmentCommand(8));
            Assert.That(state.Attacker.SupplyState, Is.LessThan(Fixed.FromFraction(40, 100)),
                "投入压过护卫时补给应真正枯竭");
        }

        [Test]
        public void EnemyReinforcement_ArrivesOnSchedule()
        {
            var cfg = SiegeConfig.Default();
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            int before = state.Attacker.Troops;
            svc.Advance(new AdvanceSegmentCommand(cfg.EnemyReinforceSegment));
            Assert.That(state.EnemyReinforced, Is.True);
            Assert.That(state.Attacker.Troops, Is.EqualTo(before + cfg.EnemyReinforceTroops),
                "拖过时间窗，敌军自己的援军抵达（两边都有援军）");
        }
    }

    // 情报雾（GDD_007 真值/知识分离）
    public class IntelFogTests
    {
        [Test]
        public void Intel_StartsLowConfidence_ScoutRaisesIt()
        {
            var cfg = SiegeConfig.Default();
            var state = SiegeScenario.CreateXishuiSiege();
            Assert.That(state.Intel.Confidence, Is.EqualTo(cfg.IntelInitialConfidence));
            new SiegeService(state).Scout(new ScoutCommand());
            Assert.That(state.Intel.Confidence, Is.EqualTo(cfg.ScoutConfidence));
        }

        [Test]
        public void Intel_DecaysWithoutScouting()
        {
            var state = SiegeScenario.CreateXishuiSiege();
            var svc = new SiegeService(state);
            svc.Scout(new ScoutCommand());
            var fresh = state.Intel.Confidence;
            svc.Advance(new AdvanceSegmentCommand(3));
            Assert.That(state.Intel.Confidence, Is.LessThan(fresh), "不侦察则情报置信随时间衰减");
        }

        [Test]
        public void Intel_EstimateStaysWithinErrorBand()
        {
            var cfg = SiegeConfig.Default();
            var state = SiegeScenario.CreateXishuiSiege();
            new SiegeService(state).Scout(new ScoutCommand());
            // 补给估计与真值之差应在误差带内（不是真值，但有界）
            int diff = System.Math.Abs(state.Intel.EstSupply.Raw - state.Attacker.SupplyState.Raw);
            Assert.That(diff, Is.LessThanOrEqualTo(cfg.ScoutErrorBand.Raw));
        }
    }
}
