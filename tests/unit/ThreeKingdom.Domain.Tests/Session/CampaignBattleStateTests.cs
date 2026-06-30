using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;
using TimeWindow = ThreeKingdom.Domain.Preparation.TimeWindow;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-019 story-001：战斗态接入会话 + 从 CommittedPlan 开战（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配）+ ADR-0004（确定性战斗）。TR-battle-001。
    /// 覆盖：会话持战斗态；从 CommittedPlan 开战；战斗态入哈希；可选向后兼容。
    /// </summary>
    [TestFixture]
    public class CampaignBattleStateTests
    {
        internal static readonly FactionId Player = new FactionId("faction-player");
        internal static readonly FactionId Enemy = new FactionId("faction-yuan");
        internal static readonly CharacterId Lord = new CharacterId("char-player-lord");
        internal static readonly CharacterId Aide = new CharacterId("char-aide");
        internal static readonly CityId Fanshui = new CityId("city-fanshui");
        internal static readonly RegionId Pass = new RegionId("region-pass");
        internal static readonly ResourceKey Grain = new ResourceKey("res-grain");
        internal static readonly BattleUnitId PlayerUnit = new BattleUnitId("unit-player-1");
        internal static readonly BattleUnitId EnemyUnit = new BattleUnitId("unit-enemy-1");
        internal static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        internal static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        internal static BattleConfig BattleCfg() => new BattleConfig(Frac(15, 10), Frac(1, 1));
        internal static TacticChainConfig TacticChains() => TacticChainConfig.SliceDefault();

        internal static BattleUnitState Unit(BattleUnitId id, FactionId faction, int force)
            => new BattleUnitState(id, faction, Pass, force,
                morale: Frac(7, 10), fatigue: Frac(2, 10), discipline: Frac(6, 10),
                terrainMod: Frac(1, 1), postureMod: Frac(1, 1), support: Frac(0, 1));

        internal static IReadOnlyList<BattleUnitState> Units(int playerForce = 1000, int enemyForce = 800)
            => new[] { Unit(PlayerUnit, Player, playerForce), Unit(EnemyUnit, Enemy, enemyForce) };

        internal static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-battle", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                resourcePool: new ResourcePool(new Dictionary<ResourceKey, long> { [Grain] = 100 }),
                preparationConfig: new PreparationConfig(tightResourceMargin: 10),
                reachableRegions: new[] { Pass },
                authorizedOrders: new[] { new OrderId("order-ambush") });

        internal static readonly CampaignSessionService Service = new CampaignSessionService();

        /// <summary>建立已提交计划（满足开战前提）的会话。</summary>
        internal static CampaignSession SessionWithCommittedPlan()
        {
            CampaignSession s = Service.StartCampaign(Config()).Session!;
            Service.AddPlanOrder(s, new PreparedOrder(
                new OrderId("order-ambush"), Aide, Pass, new TimeWindow(0, 2),
                new Dictionary<ResourceKey, long> { [Grain] = 40 }, null));
            Service.SubmitPlan(s);
            return s;
        }

        // ---- AC-1: 会话持有战斗态 ----

        [Test]
        public void test_start_battle_holds_battle_state()
        {
            CampaignSession s = SessionWithCommittedPlan();

            CampaignCommandResult r = Service.StartBattle(s, Units(), BattleCfg(), seed: 42, TacticChains());

            Assert.That(r.Applied, Is.True);
            Assert.That(s.HasBattle, Is.True);
            Assert.That(s.Battle!.Has(PlayerUnit), Is.True);
            Assert.That(s.Battle!.Has(EnemyUnit), Is.True);
        }

        [Test]
        public void test_start_battle_without_committed_plan_rejected()
        {
            CampaignSession s = Service.StartCampaign(Config()).Session!;   // 未提交计划

            CampaignCommandResult r = Service.StartBattle(s, Units(), BattleCfg(), 42, TacticChains());

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.PreparationDisabled));
            Assert.That(s.HasBattle, Is.False);
        }

        // ---- AC-2: 战斗态纳入哈希 ----

        [Test]
        public void test_battle_state_enters_session_hash()
        {
            CampaignSession a = SessionWithCommittedPlan();
            CampaignSession b = SessionWithCommittedPlan();
            Service.StartBattle(a, Units(playerForce: 1000), BattleCfg(), 42, TacticChains());
            Service.StartBattle(b, Units(playerForce: 2000), BattleCfg(), 42, TacticChains());

            Assert.That(a.ComputeHash(), Is.Not.EqualTo(b.ComputeHash()), "战斗单位兵力进哈希");
        }

        [Test]
        public void test_identical_battle_yields_same_hash()
        {
            CampaignSession a = SessionWithCommittedPlan();
            CampaignSession b = SessionWithCommittedPlan();
            Service.StartBattle(a, Units(), BattleCfg(), 42, TacticChains());
            Service.StartBattle(b, Units(), BattleCfg(), 42, TacticChains());

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));
        }

        // ---- AC-3: 开战单位可读 ----

        [Test]
        public void test_battle_units_readable()
        {
            CampaignSession s = SessionWithCommittedPlan();
            Service.StartBattle(s, Units(playerForce: 1000, enemyForce: 800), BattleCfg(), 42, TacticChains());

            Assert.That(s.Battle!.Unit(PlayerUnit).Force, Is.EqualTo(1000));
            Assert.That(s.Battle!.Unit(EnemyUnit).Force, Is.EqualTo(800));
            Assert.That(s.Battle!.Unit(EnemyUnit).Faction, Is.EqualTo(Enemy));
        }

        // ---- AC-4: 无战斗向后兼容 ----

        [Test]
        public void test_session_without_battle_has_no_battle()
        {
            CampaignSession s = Service.StartCampaign(Config()).Session!;
            Assert.That(s.HasBattle, Is.False);
            Assert.That(s.Battle, Is.Null);
        }
    }
}
