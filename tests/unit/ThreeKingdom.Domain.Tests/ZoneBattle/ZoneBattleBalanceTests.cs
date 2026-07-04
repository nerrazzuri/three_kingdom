using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// 区域战斗<b>平衡不变量</b>（GDD_021 §11 / GDD_019 §8 W5，2026-07-04 打磨）：
    /// ① 姿态是真权衡（主攻早期高战力、坚守省疲劳）——无单一姿态占优；
    /// ② 疲劳侵蚀有效战力，久战可翻盘（疲劳非死数值）；
    /// ③ 城防之利——守方据坚固地形得工事加成，破坚城须真优势（W5）；
    /// ④ 闭合因果——同守备下唯"准备"决定胜负（备足破城 / 裸战退兵）。
    /// </summary>
    [TestFixture]
    public class ZoneBattleBalanceTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;
        private readonly RoundResolutionService _rounds = new RoundResolutionService();

        private static Detachment Det(string id, BattleSide side, ZoneId at, int strength,
            FixedPoint morale, FixedPoint fatigue, Posture posture)
            => new Detachment(new DetachmentId(id), side, general: null,
                TroopComposition.AllInfantry(strength), strength, morale, fatigue, posture, at);

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed: 7UL);

        private static int Str(ZoneBattleState s, string id) => s.TryGet(new DetachmentId(id))!.Strength;

        // ---- ① 姿态权衡：主攻在开阔交战中战力高于坚守（同兵同气同疲劳） ----
        [Test]
        public void test_assault_outpowers_hold_in_open_exchange()
        {
            // 平原区（无城防加成）隔离姿态：同 500 兵、同士气、同疲劳，攻主攻 vs 守坚守。
            ZoneBattleState s = State(
                Det("a", BattleSide.Attacker, BattleField.Reserve, 500, F(7, 10), F(2, 10), Posture.Assault),
                Det("d", BattleSide.Defender, BattleField.Reserve, 500, F(7, 10), F(2, 10), Posture.Hold));
            ZoneBattleState after = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State;
            Assert.That(Str(after, "d"), Is.LessThan(Str(after, "a")), "开阔地主攻战力更高 → 坚守方减员更多（主攻不再被坚守占优）。");
        }

        // ---- ① 姿态权衡：坚守累积疲劳慢于主攻（久持省力，代价对称） ----
        [Test]
        public void test_hold_accrues_less_fatigue_than_assault()
        {
            // 各自独占空区（仅累积疲劳，无交战）。
            ZoneBattleState s = State(
                Det("assault", BattleSide.Attacker, BattleField.Reserve, 300, F(7, 10), F(2, 10), Posture.Assault),
                Det("hold", BattleSide.Attacker, BattleField.Cover, 300, F(7, 10), F(2, 10), Posture.Hold));
            for (int i = 0; i < 3; i++) s = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State;
            FixedPoint assaultFatigue = s.TryGet(new DetachmentId("assault"))!.Fatigue;
            FixedPoint holdFatigue = s.TryGet(new DetachmentId("hold"))!.Fatigue;
            Assert.That(assaultFatigue.Raw, Is.GreaterThan(holdFatigue.Raw), "主攻耗力快于坚守 → 速攻 vs 久持成真权衡。");
        }

        // ---- ② 疲劳侵蚀有效战力：久战/疲兵可被翻盘（疲劳非死数值） ----
        [Test]
        public void test_fatigue_erodes_power_and_flips_outcome()
        {
            // 生力军：攻 500（疲劳低）主攻 vs 守 480 坚守 → 攻胜（守减员更多）。
            ZoneBattleState fresh = State(
                Det("a", BattleSide.Attacker, BattleField.Reserve, 500, F(7, 10), F(1, 10), Posture.Assault),
                Det("d", BattleSide.Defender, BattleField.Reserve, 480, F(7, 10), F(1, 10), Posture.Hold));
            ZoneBattleState freshAfter = _rounds.ResolveRound(fresh, ZoneBattleContext.Default, Cfg).State;
            Assert.That(Str(freshAfter, "d"), Is.LessThan(Str(freshAfter, "a")), "生力军主攻胜过等量坚守。");

            // 疲兵：同 500 主攻但疲劳高 → 有效战力跌破守方 → 守胜（攻减员更多）。翻盘。
            ZoneBattleState tired = State(
                Det("a", BattleSide.Attacker, BattleField.Reserve, 500, F(7, 10), F(9, 10), Posture.Assault),
                Det("d", BattleSide.Defender, BattleField.Reserve, 480, F(7, 10), F(1, 10), Posture.Hold));
            ZoneBattleState tiredAfter = _rounds.ResolveRound(tired, ZoneBattleContext.Default, Cfg).State;
            Assert.That(Str(tiredAfter, "a"), Is.LessThan(Str(tiredAfter, "d")), "疲兵有效战力被侵蚀 → 反被等量坚守压制（疲劳决定）。");
        }

        // ---- ③ 城防之利：同等兵力下，攻方在开阔地能胜、在坚固城门前反败 ----
        [Test]
        public void test_fortified_gate_favors_defender_against_equal_force()
        {
            Detachment Atk(ZoneId at) => Det("a", BattleSide.Attacker, at, 520, F(7, 10), F(2, 10), Posture.Assault);
            Detachment Def(ZoneId at) => Det("d", BattleSide.Defender, at, 500, F(7, 10), F(2, 10), Posture.Hold);

            // 开阔地（平原·无工事）：攻略多兵 → 攻胜。
            ZoneBattleState open = _rounds.ResolveRound(
                State(Atk(BattleField.Reserve), Def(BattleField.Reserve)), ZoneBattleContext.Default, Cfg).State;
            Assert.That(Str(open, "d"), Is.LessThan(Str(open, "a")), "开阔地：略优攻方胜。");

            // 坚固城门（正面·工事加成）：同等兵力 → 守方凭城防反胜。
            ZoneBattleState gate = _rounds.ResolveRound(
                State(Atk(BattleField.Front), Def(BattleField.Front)), ZoneBattleContext.Default, Cfg).State;
            Assert.That(Str(gate, "a"), Is.LessThan(Str(gate, "d")), "坚城前：城防之利令守方反胜（破坚城须真优势，W5）。");
        }

        // ---- ④ 闭合因果 W5：同守备（400）下，唯"准备"决定胜负 ----
        [Test]
        public void test_preparation_decides_outcome_against_same_garrison()
        {
            const int garrison = 400;
            ZoneBattleOutcome prepared = OffensiveOf(900).AutoResolve().Outcome;
            ZoneBattleOutcome bare = OffensiveOf(120).AutoResolve().Outcome;
            Assert.That(prepared, Is.EqualTo(ZoneBattleOutcome.AttackerVictory), "备足（900）→ 破城。");
            Assert.That(bare, Is.EqualTo(ZoneBattleOutcome.DefenderVictory), "裸战（120）→ 同守备下退兵（失败可继续）。");

            ZoneBattleRuntime OffensiveOf(int troops)
            {
                var lead = new OffensiveGeneral(new CharacterId("lead"), F(7, 10), F(7, 10), F(8, 10));
                var prep = new OffensivePreparation(troops, 300, new OffensiveCommand(lead),
                    TroopComposition.AllInfantry(troops), ApproachPlan.FrontalAssault,
                    new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Fortified, scouted: true);
                FixedPoint morale = new OffensiveSetupService().Derive(prep, OffensiveSetupConfig.Default).Morale;
                return ZoneBattleRuntime.FromOffensive(prep, morale, garrison, seed: 4321UL);
            }
        }
    }
}
