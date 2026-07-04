using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// S7 区域战斗可玩运行期 <see cref="ZoneBattleRuntime"/>（GDD_021 §12）：玩家战中调整 + 推进回合 + 只读投影 + 终局。
    /// </summary>
    [TestFixture]
    public class ZoneBattlePresentationTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static OffensivePreparation Prep(int troops, ApproachPlan approach, int cavalry = 0)
        {
            TroopComposition comp = cavalry > 0
                ? new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = cavalry, [TroopType.Infantry] = Math.Max(0, troops - cavalry) })
                : TroopComposition.AllInfantry(troops);
            var lead = new OffensiveGeneral(new CharacterId("lead"), F(7, 10), F(7, 10), F(8, 10));
            return new OffensivePreparation(troops, 300, new OffensiveCommand(lead), comp, approach,
                new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Pass, scouted: true);
        }

        private static ZoneBattleRuntime Battle(int troops, ApproachPlan approach, int garrison, int cavalry = 0, ulong seed = 321UL)
        {
            OffensivePreparation prep = Prep(troops, approach, cavalry);
            FixedPoint morale = new OffensiveSetupService().Derive(prep, OffensiveSetupConfig.Default).Morale;
            return ZoneBattleRuntime.FromOffensive(prep, morale, garrison, seed);
        }

        [Test]
        public void test_player_drives_frontal_battle_to_victory()
        {
            ZoneBattleRuntime rt = Battle(900, ApproachPlan.FrontalAssault, garrison: 150);
            ZoneBattleView view = rt.View();
            for (int i = 0; i < 12 && !rt.IsOver; i++) view = rt.ResolveRound();
            Assert.That(rt.Outcome, Is.EqualTo(ZoneBattleOutcome.AttackerVictory));
            Assert.That(view.IsOver, Is.True);
            Assert.That(view.OutcomeLabel, Does.Contain("破城"), "玩家视角胜局文案。");
        }

        [Test]
        public void test_view_reflects_five_zones_and_own_front_presence()
        {
            ZoneBattleView view = Battle(600, ApproachPlan.FrontalAssault, garrison: 400).View();
            Assert.That(view.Zones.Count, Is.EqualTo(5));
            ZoneLineView? front = null;
            foreach (ZoneLineView z in view.Zones) if (z.ZoneId == "zone-front") front = z;
            Assert.That(front, Is.Not.Null);
            Assert.That(front!.OwnStrength, Is.GreaterThan(0), "攻方在正面有兵。");
            Assert.That(front.ZoneLabel, Is.EqualTo("正面关城"));
        }

        [Test]
        public void test_player_move_command_repositions_detachment()
        {
            ZoneBattleRuntime rt = Battle(600, ApproachPlan.FrontalAssault, garrison: 400);
            // 正面→侧翼（相邻）合法。
            ZoneCommandResult r = rt.MoveDetachment("atk-front", "zone-flank");
            Assert.That(r.Applied, Is.True, "相邻调动成功。");
            Assert.That(rt.State.TryGet(new DetachmentId("atk-front"))!.InTransit, Is.True, "调动→在途。");
            // 非相邻（正面→敌粮道）拒。
            ZoneCommandResult far = rt.MoveDetachment("atk-front", "zone-supply");
            Assert.That(far.Applied, Is.False);
        }

        [Test]
        public void test_commands_are_noop_after_battle_over()
        {
            ZoneBattleRuntime rt = Battle(900, ApproachPlan.FrontalAssault, garrison: 150);
            for (int i = 0; i < 12 && !rt.IsOver; i++) rt.ResolveRound();
            Assert.That(rt.IsOver, Is.True);
            ZoneCommandResult r = rt.MoveDetachment("atk-front", "zone-flank");
            Assert.That(r.Applied, Is.False, "战斗已终局，命令不再生效。");
        }

        [Test]
        public void test_ai_autoresolve_strong_attacker_wins()
        {
            ZoneBattleRuntime rt = Battle(900, ApproachPlan.FrontalAssault, garrison: 150);
            ZoneBattleView v = rt.AutoResolve();
            Assert.That(rt.IsOver, Is.True, "AI 代打推进至终局。");
            Assert.That(rt.Outcome, Is.EqualTo(ZoneBattleOutcome.AttackerVictory), "强部署 → 代打胜。");
        }

        [Test]
        public void test_ai_autoresolve_does_not_guarantee_victory()
        {
            // 弱部署对强守：AI 代打不作弊 → 仍会输（代打不必胜）。
            ZoneBattleRuntime rt = Battle(60, ApproachPlan.FrontalAssault, garrison: 900);
            rt.AutoResolve();
            Assert.That(rt.Outcome, Is.EqualTo(ZoneBattleOutcome.DefenderVictory), "弱部署 → 代打亦败（不保证赢）。");
        }

        [Test]
        public void test_ai_autoresolve_is_deterministic()
        {
            ZoneBattleOutcome a = Battle(500, ApproachPlan.FeintLure, garrison: 300, cavalry: 250, seed: 88UL).AutoResolve().Outcome;
            ZoneBattleOutcome b = Battle(500, ApproachPlan.FeintLure, garrison: 300, cavalry: 250, seed: 88UL).AutoResolve().Outcome;
            Assert.That(b, Is.EqualTo(a), "同种子+同部署 → 代打同终局（确定性可复现）。");
        }

        [Test]
        public void test_resolve_round_advances_clock_and_projects_view()
        {
            ZoneBattleRuntime rt = Battle(500, ApproachPlan.FeintLure, garrison: 300, cavalry: 250);
            int before = rt.View().Round;
            ZoneBattleView after = rt.ResolveRound();
            Assert.That(after.Round, Is.EqualTo(before + 1), "推进一回合。");
        }
    }
}
