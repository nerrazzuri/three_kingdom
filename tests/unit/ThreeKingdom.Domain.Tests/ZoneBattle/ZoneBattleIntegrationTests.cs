using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// S6 攻守统一编排端到端（GDD_021 R3/R7 / ADR-0012/0013）：完整回合循环（敌AI + 结算 + 终局）+
    /// 六维准备→区域部署桥 + 涌现兵法 + 确定性 + 攻守对称。
    /// </summary>
    [TestFixture]
    public class ZoneBattleIntegrationTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private readonly ZoneBattleService _service = new ZoneBattleService();
        private readonly OffensiveDeploymentPlanner _planner = new OffensiveDeploymentPlanner();
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;
        private static readonly EnemyAiConfig Ai = EnemyAiConfig.Default;

        private static OffensivePreparation Prep(
            int troops, long supply, ApproachPlan approach, int cavalry = 0, bool scouted = true,
            bool night = false, bool fog = false)
        {
            TroopComposition comp = cavalry > 0
                ? new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = cavalry, [TroopType.Infantry] = Math.Max(0, troops - cavalry) })
                : TroopComposition.AllInfantry(troops);
            var lead = new OffensiveGeneral(new CharacterId("lead"), F(7, 10), F(7, 10), F(8, 10));
            var timing = new OffensiveTiming(night ? ThreeKingdom.Domain.Time.DaySegment.Night : ThreeKingdom.Domain.Time.DaySegment.Day,
                fog ? ThreeKingdom.Domain.Environment.WeatherType.Fog : ThreeKingdom.Domain.Environment.WeatherType.Clear);
            return new OffensivePreparation(troops, supply, new OffensiveCommand(lead), comp, approach, timing,
                TerrainKind.Pass, scouted, siegeSegmentsCommitted: 0);
        }

        private ZoneBattleState StartBattle(OffensivePreparation prep, int garrison, int maxRounds = 6, ulong seed = 123UL)
        {
            FixedPoint morale = new OffensiveSetupService().Derive(prep, OffensiveSetupConfig.Default).Morale;
            var dets = new List<Detachment>();
            dets.AddRange(_planner.PlanAttacker(prep, morale, BattleField.Default()));
            dets.AddRange(_planner.PlanDefender(new SiegeDefense(garrison, F(12, 10)), F(7, 10), BattleField.Default()));
            return _service.Start(BattleField.Default(), dets, BattleSide.Attacker, maxRounds, seed);
        }

        private (ZoneBattleOutcome outcome, ZoneBattleState state, List<string> emergences) RunToEnd(
            ZoneBattleState start, ZoneBattleContext ctx, int hardCap = 12)
        {
            ZoneBattleState s = start;
            var emergences = new List<string>();
            ZoneBattleOutcome outcome = ZoneBattleOutcome.Ongoing;
            for (int i = 0; i < hardCap; i++)
            {
                ZoneBattleRoundResult r = _service.ResolveRound(s, ctx, Cfg, Ai);
                s = r.State;
                emergences.AddRange(r.Emergences);
                outcome = r.Outcome;
                if (outcome != ZoneBattleOutcome.Ongoing) break;
            }
            return (outcome, s, emergences);
        }

        [Test]
        public void test_strong_frontal_attacker_breaks_weak_defender()
        {
            OffensivePreparation prep = Prep(900, 300, ApproachPlan.FrontalAssault);
            var (outcome, _, _) = RunToEnd(StartBattle(prep, garrison: 150), _planner.ContextFrom(prep));
            Assert.That(outcome, Is.EqualTo(ZoneBattleOutcome.AttackerVictory), "强攻方压垮弱守方 → 破城。");
        }

        [Test]
        public void test_weak_attacker_fails_and_defender_holds()
        {
            OffensivePreparation prep = Prep(60, 0, ApproachPlan.FrontalAssault);
            var (outcome, _, _) = RunToEnd(StartBattle(prep, garrison: 800), _planner.ContextFrom(prep));
            Assert.That(outcome, Is.EqualTo(ZoneBattleOutcome.DefenderVictory), "弱攻方攻不破/超时 → 守方胜（失败可继续）。");
        }

        [Test]
        public void test_full_battle_loop_is_deterministic()
        {
            OffensivePreparation prep = Prep(500, 200, ApproachPlan.FeintLure, cavalry: 300);
            ZoneBattleContext ctx = _planner.ContextFrom(prep);
            var a = RunToEnd(StartBattle(prep, 300, seed: 555UL), ctx);
            var b = RunToEnd(StartBattle(prep, 300, seed: 555UL), ctx);
            Assert.That(b.outcome, Is.EqualTo(a.outcome), "同种子+同部署 → 同终局。");
            Assert.That(b.state.Hash(), Is.EqualTo(a.state.Hash()), "整局确定性哈希一致（可回放）。");
        }

        [Test]
        public void test_feint_lure_ambush_emerges_hidden_from_ai()
        {
            // 攻方侧翼设伏（骑兵+智将+已侦察），守方AI 看不见蓄势伏兵、且守方只在正面（无兵可探侧翼）→ 伏兵突然性成型。
            var guile = new OffensiveGeneral(new CharacterId("cunning"), F(6, 10), F(7, 10), F(8, 10));
            var cav = new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 300 });
            var dets = new List<Detachment>
            {
                new Detachment(new DetachmentId("atk-front"), BattleSide.Attacker, null, TroopComposition.AllInfantry(400), 400, F(7, 10), F(2, 10), Posture.Assault, BattleField.Front),
                new Detachment(new DetachmentId("atk-flank"), BattleSide.Attacker, guile, cav, 300, F(7, 10), F(2, 10), Posture.Feint, BattleField.Flank),
                new Detachment(new DetachmentId("def-front"), BattleSide.Defender, null, TroopComposition.AllInfantry(500), 500, F(7, 10), F(1, 10), Posture.Hold, BattleField.Front),
            };
            ZoneBattleState start = _service.Start(BattleField.Default(), dets, BattleSide.Attacker, 6, 909UL);
            var (_, _, emergences) = RunToEnd(start, new ZoneBattleContext(false, false, true));
            Assert.That(emergences, Has.Some.Contains("AmbushSurprise"), "侧翼伏兵在敌不知情下蓄势成型（反全知涌现）。");
            Assert.That(emergences, Has.Some.Contains("zone-flank"), "涌现发生在侧翼隘口。");
        }

        [Test]
        public void test_attack_defense_symmetry_player_as_defender_resolves()
        {
            // 玩家守方、攻方由AI驱动：同一引擎跑一回合无异常、终局可判。
            var dets = new List<Detachment>
            {
                new Detachment(new DetachmentId("atk-ai"), BattleSide.Attacker, null, TroopComposition.AllInfantry(400), 400, F(7, 10), F(2, 10), Posture.Assault, BattleField.Front),
                new Detachment(new DetachmentId("def-p"), BattleSide.Defender, null, TroopComposition.AllInfantry(500), 500, F(7, 10), F(1, 10), Posture.Hold, BattleField.Front),
            };
            ZoneBattleState s = _service.Start(BattleField.Default(), dets, BattleSide.Defender, 6, 77UL);
            ZoneBattleRoundResult r = _service.ResolveRound(s, ZoneBattleContext.Default, Cfg, Ai);
            Assert.That(r.State.Clock.Round, Is.EqualTo(2), "攻守统一：玩家守方一回合正常推进。");
            Assert.That(r.Outcome, Is.AnyOf(ZoneBattleOutcome.Ongoing, ZoneBattleOutcome.AttackerVictory, ZoneBattleOutcome.DefenderVictory));
        }

        [Test]
        public void test_planner_places_feint_cavalry_at_flank()
        {
            OffensivePreparation prep = Prep(500, 100, ApproachPlan.FeintLure, cavalry: 300);
            IReadOnlyList<Detachment> atk = _planner.PlanAttacker(prep, F(7, 10), BattleField.Default());
            Detachment? flank = null;
            foreach (Detachment d in atk) if (d.Location == BattleField.Flank) flank = d;
            Assert.That(flank, Is.Not.Null, "假退诱敌 → 骑兵支队部署于侧翼隘口。");
            Assert.That(flank!.Composition.Count(TroopType.Cavalry), Is.EqualTo(300));
            Assert.That(flank.Posture, Is.EqualTo(Posture.Feint));
        }
    }
}
