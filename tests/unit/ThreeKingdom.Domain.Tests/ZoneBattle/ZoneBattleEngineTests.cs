using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// S2/S3/S4 区域战斗引擎（GDD_021 / ADR-0012）：按区条件涌现（含累积）+ 回合交战结算 + 确定性 + 战中调整命令契约。
    /// </summary>
    [TestFixture]
    public class ZoneBattleEngineTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;

        private static OffensiveGeneral Gen(FixedPoint guile, FixedPoint command)
            => new OffensiveGeneral(new CharacterId("g-lead"), command, F(6, 10), guile);

        private static Detachment Det(string id, BattleSide side, ZoneId at, int strength,
            int cavalry = 0, OffensiveGeneral? general = null, Posture posture = Posture.Assault)
        {
            TroopComposition comp = cavalry > 0
                ? new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = cavalry, [TroopType.Infantry] = Math.Max(0, strength - cavalry) })
                : TroopComposition.AllInfantry(strength);
            return new Detachment(new DetachmentId(id), side, general, comp, strength, F(7, 10), F(2, 10), posture, at);
        }

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new BattleClock(1, 6), BattleSide.Attacker, seed: 7UL);

        private readonly RoundResolutionService _rounds = new RoundResolutionService();
        private readonly ZoneCommandService _cmd = new ZoneCommandService();

        [Test]
        public void test_round_resolution_is_deterministic()
        {
            ZoneBattleState s = State(
                Det("a1", BattleSide.Attacker, BattleField.Front, 600),
                Det("d1", BattleSide.Defender, BattleField.Front, 400));
            StateHash h1 = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State.Hash();
            StateHash h2 = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State.Hash();
            Assert.That(h2, Is.EqualTo(h1), "同态+同上下文+同配置 → 同结果（确定性哈希）。");
        }

        [Test]
        public void test_stronger_side_wins_zone_and_weaker_takes_attrition()
        {
            ZoneBattleState s = State(
                Det("a1", BattleSide.Attacker, BattleField.Front, 800),
                Det("d1", BattleSide.Defender, BattleField.Front, 300));
            ZoneBattleState after = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State;
            int defAfter = after.TryGet(new DetachmentId("d1"))!.Strength;
            int attAfter = after.TryGet(new DetachmentId("a1"))!.Strength;
            Assert.That(defAfter, Is.LessThan(300), "弱守方减员。");
            // 败方按比例减员多于胜方（交叉相乘避浮点）：(300-def)/300 > (800-att)/800。
            Assert.That((long)(300 - defAfter) * 800, Is.GreaterThan((long)(800 - attAfter) * 300), "败方减员比例高于胜方。");
            Assert.That(after.TryGet(new DetachmentId("d1"))!.Morale, Is.LessThan(after.TryGet(new DetachmentId("a1"))!.Morale), "败方士气跌、胜方不跌。");
        }

        [Test]
        public void test_supply_cut_and_starve_accumulate_over_rounds()
        {
            // 攻方独占敌粮道区（无守方）→ 切断补给瞬时成型；持续 → 断粮达宽限（StarveRounds=2）。
            ZoneBattleState s = State(Det("a1", BattleSide.Attacker, BattleField.Supply, 300));
            ZoneBattleState r1 = _rounds.ResolveRound(s, ZoneBattleContext.Default, Cfg).State;
            Assert.That(r1.EngagementOf(BattleField.Supply).HasFormed(TacticCondition.SupplyLineCut), Is.True, "占据敌粮道→切断补给。");
            Assert.That(r1.EngagementOf(BattleField.Supply).HasFormed(TacticCondition.ShortageReachedGrace), Is.False, "尚未撑够回合。");
            ZoneBattleState r2 = _rounds.ResolveRound(r1, ZoneBattleContext.Default, Cfg).State;
            Assert.That(r2.EngagementOf(BattleField.Supply).HasFormed(TacticCondition.ShortageReachedGrace), Is.True, "累计达门槛→断粮达宽限。");
            Assert.That(r2.EngagementOf(BattleField.Supply).HasFormed(TacticCondition.EnemyCohesionCrossedThreshold), Is.True, "依赖条件随之成型。");
        }

        [Test]
        public void test_ambush_charges_when_hidden_and_resets_when_contested()
        {
            OffensiveGeneral cunning = Gen(F(8, 10), F(6, 10));
            // 隘口 + 骑兵 + 智将 + 已侦察 + 无敌接触 → 蓄势 2 回合成伏。
            ZoneBattleState hidden = State(Det("a1", BattleSide.Attacker, BattleField.Flank, 300, cavalry: 150, general: cunning));
            ZoneBattleState r1 = _rounds.ResolveRound(hidden, ZoneBattleContext.Default, Cfg).State;
            Assert.That(r1.EngagementOf(BattleField.Flank).AmbushCharge, Is.EqualTo(1));
            Assert.That(r1.EngagementOf(BattleField.Flank).HasFormed(TacticCondition.AmbushSurprise), Is.False);
            ZoneBattleState r2 = _rounds.ResolveRound(r1, ZoneBattleContext.Default, Cfg).State;
            Assert.That(r2.EngagementOf(BattleField.Flank).HasFormed(TacticCondition.AmbushSurprise), Is.True, "蓄势达门槛→伏兵突然性。");
            Assert.That(r2.EngagementOf(BattleField.Flank).HasFormed(TacticCondition.EnemyPursued), Is.True, "骑兵份额→追击条件。");
        }

        [Test]
        public void test_ambush_does_not_form_without_scout()
        {
            OffensiveGeneral cunning = Gen(F(8, 10), F(6, 10));
            ZoneBattleState s = State(Det("a1", BattleSide.Attacker, BattleField.Flank, 300, cavalry: 150, general: cunning));
            var blind = new ZoneBattleContext(isNight: false, isFoggy: false, attackerScouted: false);
            ZoneBattleState r1 = _rounds.ResolveRound(s, blind, Cfg).State;
            ZoneBattleState r2 = _rounds.ResolveRound(r1, blind, Cfg).State;
            Assert.That(r2.EngagementOf(BattleField.Flank).HasFormed(TacticCondition.AmbushSurprise), Is.False, "未侦察→伏兵不成型（反全知门）。");
        }

        [Test]
        public void test_move_command_requires_adjacency_and_rejects_transit_and_nonowner()
        {
            ZoneBattleState s = State(
                Det("a1", BattleSide.Attacker, BattleField.Front, 300),
                Det("d1", BattleSide.Defender, BattleField.Front, 300));

            // 相邻 OK：正面→侧翼。
            ZoneCommandResult ok = _cmd.MoveDetachment(s, BattleSide.Attacker, new DetachmentId("a1"), BattleField.Flank);
            Assert.That(ok.Applied, Is.True);
            Assert.That(ok.State.TryGet(new DetachmentId("a1"))!.InTransit, Is.True, "调动→在途。");

            // 非相邻：正面→敌粮道。
            ZoneCommandResult far = _cmd.MoveDetachment(s, BattleSide.Attacker, new DetachmentId("a1"), BattleField.Supply);
            Assert.That(far.Error, Is.EqualTo(ZoneCommandError.NotAdjacent));

            // 在途再调 → 拒。
            ZoneCommandResult again = _cmd.MoveDetachment(ok.State, BattleSide.Attacker, new DetachmentId("a1"), BattleField.Cover);
            Assert.That(again.Error, Is.EqualTo(ZoneCommandError.AlreadyInTransit));

            // 调敌方支队 → 拒。
            ZoneCommandResult notOwner = _cmd.MoveDetachment(s, BattleSide.Attacker, new DetachmentId("d1"), BattleField.Flank);
            Assert.That(notOwner.Error, Is.EqualTo(ZoneCommandError.NotOwner));
        }

        [Test]
        public void test_in_transit_detachment_does_not_fight_this_round()
        {
            // 攻方在正面被调走（在途）→ 该回合正面只剩守方，攻方该支队不参与结算、不减员。
            ZoneBattleState s = State(
                Det("a1", BattleSide.Attacker, BattleField.Front, 300),
                Det("d1", BattleSide.Defender, BattleField.Front, 300));
            ZoneBattleState moved = _cmd.MoveDetachment(s, BattleSide.Attacker, new DetachmentId("a1"), BattleField.Flank).State;
            ZoneBattleState after = _rounds.ResolveRound(moved, ZoneBattleContext.Default, Cfg).State;
            Detachment a1 = after.TryGet(new DetachmentId("a1"))!;
            Assert.That(a1.Strength, Is.EqualTo(300), "在途支队本回合不减员（失位代价另计）。");
            Assert.That(a1.Location, Is.EqualTo(BattleField.Flank), "在途 1 回合后到位。");
        }
    }
}
