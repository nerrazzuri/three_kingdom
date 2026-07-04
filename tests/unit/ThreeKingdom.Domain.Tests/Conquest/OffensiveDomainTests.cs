using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Conquest
{
    /// <summary>
    /// 出征攻城 Domain 核心（GDD_019 v2 / ADR-0010 / ADR-0011）：占城归属 C（S4）、出征授权门（S1）、
    /// <b>六维闭合因果映射</b>（S3：兵力/补给/将领/兵种/布势/时机/侦察 → 战力/士气/条件）。全纯函数、确定性。
    /// </summary>
    [TestFixture]
    public class OffensiveDomainTests
    {
        // ---- S4 占城归属 C（ADR-0010）----

        private static readonly FixedPoint Half = FixedPoint.FromFraction(1, 2);
        private static readonly FixedPoint Zero = FixedPoint.Zero;

        private static OccupationConfig Cfg(FixedPoint baseGrant, int n = 2) =>
            new OccupationConfig(n, baseGrant, Zero, Zero, Zero, leanPerSeizure: 10);

        [Test]
        public void test_first_n_conquests_always_granted_to_player()
        {
            var svc = new OccupationOwnershipService();
            var cfg = Cfg(Zero);   // base 0 → 第3座起几乎必被君主收；但前2座恒归玩家
            Assert.That(svc.Resolve(0, Zero, Zero, Zero, 1UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
            Assert.That(svc.Resolve(1, Zero, Zero, Zero, 1UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
        }

        [Test]
        public void test_p_grant_one_always_player_zero_always_lord()
        {
            var svc = new OccupationOwnershipService();
            var always = Cfg(FixedPoint.One);   // p=1
            var never = Cfg(Zero);              // p=0
            for (ulong seed = 1; seed <= 5; seed++)
            {
                Assert.That(svc.Resolve(2, Zero, Zero, Zero, seed, always), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
                Assert.That(svc.Resolve(2, Zero, Zero, Zero, seed, never), Is.EqualTo(OwnershipVerdict.LordKeeps));
            }
        }

        [Test]
        public void test_ownership_is_deterministic_for_same_seed()
        {
            var svc = new OccupationOwnershipService();
            var cfg = Cfg(Half);   // p=0.5，取决于种子
            OwnershipVerdict a = svc.Resolve(3, Zero, Zero, Zero, 0xABCDUL, cfg);
            OwnershipVerdict b = svc.Resolve(3, Zero, Zero, Zero, 0xABCDUL, cfg);
            Assert.That(b, Is.EqualTo(a), "同 (index,因子,种子,配置) → 同结果（可复现）。");
        }

        [Test]
        public void test_renown_raises_grant_probability()
        {
            var svc = new OccupationOwnershipService();
            var cfg = new OccupationConfig(2, Zero, FixedPoint.One, Zero, Zero, 10);   // base0 + 名望权重1
            Assert.That(svc.Resolve(2, FixedPoint.One, Zero, Zero, 7UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
            Assert.That(svc.Resolve(2, Zero, Zero, Zero, 7UL, cfg), Is.EqualTo(OwnershipVerdict.LordKeeps));
        }

        // ---- S1 出征授权门（GDD_019 R1/R2）----

        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-enemy");
        private static readonly CityId Target = new CityId("city-target");

        [Test]
        public void test_authorized_enemy_city_passes_gate()
        {
            var svc = new OffensiveAuthorizationService();
            var auth = new OffensiveAuthorization(new[] { Target });
            Assert.That(svc.Check(Target, auth, Enemy, Player), Is.EqualTo(OffensiveGateResult.Authorized));
        }

        [Test]
        public void test_gate_rejects_unauthorized_own_and_ownerless()
        {
            var svc = new OffensiveAuthorizationService();
            Assert.That(svc.Check(Target, OffensiveAuthorization.None, Enemy, Player),
                Is.EqualTo(OffensiveGateResult.NotAuthorized), "不在授权集 → 拒。");

            var auth = new OffensiveAuthorization(new[] { Target });
            Assert.That(svc.Check(Target, auth, Player, Player),
                Is.EqualTo(OffensiveGateResult.OwnCity), "己方城 → 拒。");
            Assert.That(svc.Check(Target, auth, null, Player),
                Is.EqualTo(OffensiveGateResult.NotEnemyControlled), "无主/未登记 → 拒。");
        }

        // ---- S3 六维闭合因果映射（GDD_019 v2 §4a/§5 / ADR-0011）----

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static OffensiveGeneral Gen(FixedPoint command, FixedPoint? valor = null, FixedPoint? guile = null,
            string id = "char-lead", GeneralSpecialty specialty = GeneralSpecialty.None)
            => new OffensiveGeneral(new CharacterId(id), command, valor ?? F(5, 10), guile ?? F(5, 10), specialty);

        private static OffensiveCommand Command(FixedPoint? leadCmd = null, bool advisor = false,
            FixedPoint? valor = null, FixedPoint? guile = null, params OffensiveGeneral[] deputies)
            => new OffensiveCommand(Gen(leadCmd ?? F(5, 10), valor, guile), deputies, advisor);

        private static OffensivePreparation Prep(
            int troops = 0, long supply = 0, OffensiveCommand? command = null, TroopComposition? comp = null,
            ApproachPlan approach = ApproachPlan.FrontalAssault, OffensiveTiming? timing = null,
            TerrainKind terrain = TerrainKind.Fortified, bool scouted = false, long siegeSegs = 0)
            => new OffensivePreparation(troops, supply, command ?? Command(), comp ?? TroopComposition.None,
                approach, timing ?? new OffensiveTiming(DaySegment.Day, WeatherType.Clear), terrain, scouted, siegeSegs);

        private static OffensiveForce Derive(OffensivePreparation p)
            => new OffensiveSetupService().Derive(p, OffensiveSetupConfig.Default);

        [Test]
        public void test_more_troops_yields_more_force()
        {
            Assert.That(Derive(Prep(troops: 600)).Force, Is.GreaterThan(Derive(Prep(troops: 0)).Force), "兵多→战力高（单调）。");
        }

        [Test]
        public void test_more_supply_yields_more_morale_and_caps()
        {
            OffensiveForce weak = Derive(Prep(supply: 0));
            OffensiveForce strong = Derive(Prep(supply: 300));
            OffensiveForce flood = Derive(Prep(supply: 1_000_000));
            Assert.That(strong.Morale, Is.GreaterThan(weak.Morale), "粮足→士气高（单调）。");
            Assert.That(flood.Morale, Is.EqualTo(OffensiveSetupConfig.Default.MaxMorale), "补给极多→士气封顶，不溢出。");
        }

        [Test]
        public void test_higher_lead_command_yields_more_force()
        {
            OffensiveForce lowCmd = Derive(Prep(troops: 100, command: Command(leadCmd: F(2, 10))));
            OffensiveForce highCmd = Derive(Prep(troops: 100, command: Command(leadCmd: F(9, 10))));
            Assert.That(highCmd.Force, Is.GreaterThan(lowCmd.Force), "主将统率高→战力高（派谁轴）。");
        }

        [Test]
        public void test_higher_lead_valor_yields_more_morale()
        {
            OffensiveForce lowVal = Derive(Prep(command: Command(valor: F(1, 10))));
            OffensiveForce highVal = Derive(Prep(command: Command(valor: F(9, 10))));
            Assert.That(highVal.Morale, Is.GreaterThan(lowVal.Morale), "主将武勇高→士气高。");
        }

        [Test]
        public void test_deputy_force_contribution_decays()
        {
            OffensiveGeneral dep = Gen(F(10, 10), id: "char-dep");
            int f0 = Derive(Prep(troops: 100, command: Command(deputies: Array.Empty<OffensiveGeneral>()))).Force;
            int f1 = Derive(Prep(troops: 100, command: Command(deputies: new[] { dep }))).Force;
            int f2 = Derive(Prep(troops: 100, command: Command(deputies: new[] { dep, dep }))).Force;
            Assert.That(f1, Is.GreaterThan(f0), "加一名副将→战力增。");
            Assert.That(f2 - f1, Is.LessThan(f1 - f0), "第二名副将边际贡献递减（decay^k，防堆将碾压）。");
        }

        [Test]
        public void test_frontal_assault_carries_no_tactic_conditions()
        {
            Assert.That(Derive(Prep(troops: 600, approach: ApproachPlan.FrontalAssault)).Conditions, Is.Empty,
                "正面强攻无兵法条件（纯战力）。");
        }

        [Test]
        public void test_feint_lure_pursuit_gated_by_cavalry_share()
        {
            var noCav = Prep(troops: 100, approach: ApproachPlan.FeintLure, comp: TroopComposition.AllInfantry(100));
            var withCav = Prep(troops: 100, approach: ApproachPlan.FeintLure,
                comp: new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 60, [TroopType.Infantry] = 40 }));
            Assert.That(Derive(noCav).Conditions, Does.Not.Contain(TacticCondition.EnemyPursued), "无骑兵→无追击条件。");
            Assert.That(Derive(withCav).Conditions, Contains.Item(TacticCondition.EnemyPursued), "骑兵份额达门槛→追击条件成型。");
        }

        [Test]
        public void test_feint_lure_ambush_requires_pass_scout_and_guile()
        {
            OffensiveCommand cunning = Command(guile: F(8, 10));
            var formed = Prep(troops: 100, approach: ApproachPlan.FeintLure, command: cunning,
                terrain: TerrainKind.Pass, scouted: true);
            var notScouted = Prep(troops: 100, approach: ApproachPlan.FeintLure, command: cunning,
                terrain: TerrainKind.Pass, scouted: false);
            var notPass = Prep(troops: 100, approach: ApproachPlan.FeintLure, command: cunning,
                terrain: TerrainKind.Plain, scouted: true);
            Assert.That(Derive(formed).Conditions, Contains.Item(TacticCondition.AmbushSurprise), "隘口+侦察+智谋→伏兵突然性成型。");
            Assert.That(Derive(notScouted).Conditions, Does.Not.Contain(TacticCondition.AmbushSurprise), "未侦察→无突然性（反全知门）。");
            Assert.That(Derive(notPass).Conditions, Does.Not.Contain(TacticCondition.AmbushSurprise), "非隘口→无突然性。");
        }

        [Test]
        public void test_advisor_accompanies_enables_ambush_without_high_guile()
        {
            OffensiveCommand dullButAdvised = Command(guile: F(1, 10), advisor: true);
            var prep = Prep(troops: 100, approach: ApproachPlan.FeintLure, command: dullButAdvised,
                terrain: TerrainKind.Pass, scouted: true);
            Assert.That(Derive(prep).Conditions, Contains.Item(TacticCondition.AmbushSurprise),
                "军师随军→补足智谋门，伏兵突然性成型。");
        }

        [Test]
        public void test_night_raid_conditions_gated_by_night_segment()
        {
            var byDay = Prep(troops: 100, approach: ApproachPlan.NightRaid,
                timing: new OffensiveTiming(DaySegment.Day, WeatherType.Fog), command: Command(leadCmd: F(8, 10)), scouted: true);
            var byNight = Prep(troops: 100, approach: ApproachPlan.NightRaid,
                timing: new OffensiveTiming(DaySegment.Night, WeatherType.Fog), command: Command(leadCmd: F(8, 10)), scouted: true);
            Assert.That(Derive(byDay).Conditions, Is.Empty, "非夜间→夜袭条件全不成型。");
            Assert.That(Derive(byNight).Conditions, Contains.Item(TacticCondition.IsNight));
            Assert.That(Derive(byNight).Conditions, Contains.Item(TacticCondition.StealthSuccess), "夜+雾→隐蔽成功。");
            Assert.That(Derive(byNight).Conditions, Contains.Item(TacticCondition.DefenderUnaware), "夜+侦察→守方未察觉。");
        }

        [Test]
        public void test_protracted_siege_conditions_gated_by_supply_and_time()
        {
            var quick = Prep(troops: 100, supply: 300, approach: ApproachPlan.ProtractedSiege, siegeSegs: 0);
            var patient = Prep(troops: 100, supply: 300, approach: ApproachPlan.ProtractedSiege, siegeSegs: 10);
            var starved = Prep(troops: 100, supply: 50, approach: ApproachPlan.ProtractedSiege, siegeSegs: 10);
            Assert.That(Derive(quick).Conditions, Is.EqualTo(new[] { TacticCondition.SupplyLineCut }),
                "粮足但未撑够时段→只切断补给，未达断粮宽限。");
            Assert.That(Derive(patient).Conditions, Contains.Item(TacticCondition.ShortageReachedGrace), "粮足+时段足→断粮达宽限。");
            Assert.That(Derive(starved).Conditions, Is.Empty, "补给不足门槛→无法久围，条件不成型。");
        }

        [Test]
        public void test_unscouted_incurs_intel_blind_penalty()
        {
            int scouted = Derive(Prep(troops: 600, scouted: true)).Force;
            int blind = Derive(Prep(troops: 600, scouted: false)).Force;
            Assert.That(scouted - blind, Is.EqualTo(OffensiveSetupConfig.Default.IntelBlindPenalty),
                "未侦察→情报盲区战力折扣（反全知代价）。");
        }

        [Test]
        public void test_troop_composition_is_lever_not_counter()
        {
            // AC-3b 负向不变量：任何兵种组合都不因"被克"减战力——匹配加成或 0，绝无减益。
            int none = Derive(Prep(troops: 600, approach: ApproachPlan.FrontalAssault, comp: TroopComposition.None)).Force;
            int cavalry = Derive(Prep(troops: 600, approach: ApproachPlan.FrontalAssault,
                comp: TroopComposition.AllInfantry(0) /* 空 */)).Force;
            int allCav = Derive(Prep(troops: 600, approach: ApproachPlan.FrontalAssault,
                comp: new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 600 }))).Force;
            int allInf = Derive(Prep(troops: 600, approach: ApproachPlan.FrontalAssault,
                comp: TroopComposition.AllInfantry(600))).Force;
            Assert.That(allCav, Is.GreaterThanOrEqualTo(none), "不匹配路线的兵种不减战力（无克制减益）。");
            Assert.That(allInf, Is.GreaterThan(none), "匹配路线的兵种给契合加成（杠杆）。");
        }

        [Test]
        public void test_approach_plan_has_no_coordinate_fields()
        {
            // AC-3c 负向不变量：布势=路线非坐标——数据模型无网格/朝向/坐标字段。
            // 用「子串匹配网格类词 + 精确匹配坐标字段名」，避免误伤合法名（如 Composition 含 "position" 子串）。
            string[] forbiddenSubstr = { "grid", "facing", "coordinate", "hexcol", "hexrow" };
            string[] forbiddenExact = { "x", "y", "row", "col", "position", "posx", "posy", "tile" };
            IEnumerable<string> names = typeof(OffensivePreparation).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name.ToLowerInvariant())
                .Concat(Enum.GetNames(typeof(ApproachPlan)).Select(n => n.ToLowerInvariant()));
            foreach (string n in names)
            {
                foreach (string bad in forbiddenSubstr)
                    Assert.That(n.Contains(bad), Is.False, $"出征准备/布势不得含网格/坐标字段：{n}");
                foreach (string bad in forbiddenExact)
                    Assert.That(n, Is.Not.EqualTo(bad), $"出征准备/布势不得含坐标字段：{n}");
            }
        }

        [Test]
        public void test_offensive_setup_is_deterministic()
        {
            var prep = Prep(troops: 250, supply: 175, approach: ApproachPlan.FeintLure,
                comp: new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 100, [TroopType.Infantry] = 150 }),
                terrain: TerrainKind.Pass, scouted: true, command: Command(guile: F(8, 10)));
            OffensiveForce a = Derive(prep);
            OffensiveForce b = Derive(prep);
            Assert.That(b.Force, Is.EqualTo(a.Force));
            Assert.That(b.Morale, Is.EqualTo(a.Morale));
            Assert.That(b.Conditions, Is.EqualTo(a.Conditions));
        }

        [Test]
        public void test_composition_exceeding_muster_is_rejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new OffensivePreparation(100, 0, Command(), TroopComposition.AllInfantry(200),
                    ApproachPlan.FrontalAssault, new OffensiveTiming(DaySegment.Day, WeatherType.Clear)),
                "兵种编成总数超过投入兵力→拒（无部分写入）。");
        }

        [Test]
        public void test_offensive_command_requires_lead_general()
        {
            Assert.Throws<ArgumentNullException>(() => new OffensiveCommand(null!), "缺主将→拒（不成军）。");
        }
    }
}
