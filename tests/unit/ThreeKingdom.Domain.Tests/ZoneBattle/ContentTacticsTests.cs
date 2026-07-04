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
    /// 战法内容包（GDD_010/021 D12）：水攻/诈降/围点打援皆<b>多条件涌现</b>非按钮，各有区分门
    /// （水攻需湿润、诈降需佯攻姿态、围点打援需骑兵机动），门不齐不成型。
    /// </summary>
    [TestFixture]
    public class ContentTacticsTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;
        private readonly RoundResolutionService _rounds = new RoundResolutionService();

        private static ZoneBattleContext Ctx(bool dry) => new ZoneBattleContext(false, false, true, isDry: dry);

        private static OffensiveGeneral Gen() => new OffensiveGeneral(new CharacterId("g"), F(7, 10), F(7, 10), F(7, 10));

        private static Detachment Atk(string id, ZoneId at, Posture posture, TroopComposition comp, int str)
            => new Detachment(new DetachmentId(id), BattleSide.Attacker, Gen(), comp, str, F(7, 10), F(2, 10), posture, at);

        private static Detachment Def(string id, ZoneId at)
            => new Detachment(new DetachmentId(id), BattleSide.Defender, null, TroopComposition.AllInfantry(300), 300, F(7, 10), F(1, 10), Posture.Hold, at);

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed: 7UL);

        private IReadOnlyList<string> Em(ZoneBattleState s, ZoneBattleContext ctx) => _rounds.ResolveRound(s, ctx, Cfg).Emergences;
        private static bool Has(IReadOnlyList<string> em, string c)
        {
            foreach (string e in em) if (e.EndsWith(":" + c, StringComparison.Ordinal)) return true;
            return false;
        }

        private static TroopComposition Inf(int n) => TroopComposition.AllInfantry(n);
        private static TroopComposition Cav(int n) => new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = n });

        // ---- 水攻：湿润 + 低地粮道 + 智将控水 ----
        [Test]
        public void test_flood_forms_when_wet_with_cunning_general()
        {
            IReadOnlyList<string> em = Em(State(
                Atk("a", BattleField.Supply, Posture.Assault, Inf(300), 300), Def("d", BattleField.Supply)), Ctx(dry: false));
            Assert.That(Has(em, "FloodReleased"), Is.True, "湿润天时 → 决堤门。");
            Assert.That(Has(em, "WaterworksHeld"), Is.True, "智将控水利门。");
            Assert.That(Has(em, "EnemyInLowGround"), Is.True, "敌处低地门。");
        }

        [Test]
        public void test_flood_does_not_form_when_dry()
        {
            IReadOnlyList<string> em = Em(State(
                Atk("a", BattleField.Supply, Posture.Assault, Inf(300), 300), Def("d", BattleField.Supply)), Ctx(dry: true));
            Assert.That(Has(em, "FloodReleased"), Is.False, "干燥 → 无水攻（与火攻同一天时轴之反面）。");
        }

        // ---- 诈降：佯攻姿态 + 敌开门 + 军纪突袭 ----
        [Test]
        public void test_feigned_surrender_needs_feint_posture()
        {
            IReadOnlyList<string> feint = Em(State(
                Atk("a", BattleField.Front, Posture.Feint, Inf(300), 300), Def("d", BattleField.Front)), Ctx(dry: true));
            Assert.That(Has(feint, "SurrenderFeigned"), Is.True, "佯攻示弱 → 诈降门。");
            Assert.That(Has(feint, "EnemyLuredOpen"), Is.True, "敌中计门。");

            IReadOnlyList<string> assault = Em(State(
                Atk("a", BattleField.Front, Posture.Assault, Inf(300), 300), Def("d", BattleField.Front)), Ctx(dry: true));
            Assert.That(Has(assault, "SurrenderFeigned"), Is.False, "强攻无示弱 → 诈降不成型。");
        }

        // ---- 围点打援：骑兵机动 ----
        [Test]
        public void test_besiege_relief_needs_cavalry_mobility()
        {
            IReadOnlyList<string> cav = Em(State(
                Atk("a", BattleField.Cover, Posture.Assault, Cav(300), 300), Def("d", BattleField.Cover)), Ctx(dry: true));
            Assert.That(Has(cav, "PointBesieged"), Is.True, "骑兵机动 → 围点门。");
            Assert.That(Has(cav, "AmbushOnRoute"), Is.True, "途伏门。");

            IReadOnlyList<string> inf = Em(State(
                Atk("a", BattleField.Cover, Posture.Assault, Inf(300), 300), Def("d", BattleField.Cover)), Ctx(dry: true));
            Assert.That(Has(inf, "PointBesieged"), Is.False, "纯步兵无机动 → 围点打援不成型。");
        }

        // ---- 三战法均已注册复盘链 ----
        [Test]
        public void test_new_tactics_registered_in_chains()
        {
            var tags = new HashSet<TacticTag>();
            foreach (TacticChainDefinition d in TacticChainConfig.SliceDefault().Chains) tags.Add(d.Tag);
            Assert.That(tags, Does.Contain(TacticTag.FloodAttack));
            Assert.That(tags, Does.Contain(TacticTag.FeignedSurrender));
            Assert.That(tags, Does.Contain(TacticTag.BesiegeRelief));
        }
    }
}
