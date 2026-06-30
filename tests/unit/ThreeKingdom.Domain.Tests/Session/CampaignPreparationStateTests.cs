using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
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
    /// epic-018 story-001：准备态接入会话 + 计划草稿编辑（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配，草稿不改权威态）+ ADR-0004（确定性）。TR-prep-001。
    /// 覆盖：会话持准备态；编辑命令改草稿；草稿不改权威态；准备态入哈希；可选向后兼容。
    /// </summary>
    [TestFixture]
    public class CampaignPreparationStateTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly RegionId Pass = new RegionId("region-pass");
        private static readonly ResourceKey Grain = new ResourceKey("res-grain");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static ResourcePool Pool(long grain = 100)
            => new ResourcePool(new Dictionary<ResourceKey, long> { [Grain] = grain });

        private static PreparedOrder Order(string id = "order-ambush", long grainNeed = 40)
            => new PreparedOrder(
                new OrderId(id), Aide, Pass, new TimeWindow(0, 2),
                new Dictionary<ResourceKey, long> { [Grain] = grainNeed }, null);

        private static CampaignStartConfig Config(ResourcePool? pool = null)
            => new CampaignStartConfig(
                "scenario-fanshui-prep", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                resourcePool: pool ?? Pool(),
                preparationConfig: new PreparationConfig(tightResourceMargin: 10),
                reachableRegions: new[] { Pass },
                authorizedOrders: new[] { new OrderId("order-ambush"), new OrderId("order-raid") });

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession(ResourcePool? pool = null)
            => Service.StartCampaign(Config(pool)).Session!;

        // ---- AC-1: 会话持有准备态 ----

        [Test]
        public void test_session_holds_preparation_state()
        {
            CampaignSession s = NewSession();
            Assert.That(s.HasPreparation, Is.True);
            Assert.That(s.PlanOrders, Is.Not.Null);
            Assert.That(s.PlanOrders!.Count, Is.EqualTo(0), "开局空草稿");
            Assert.That(s.CommittedPlan, Is.Null);
        }

        // ---- AC-2: 编辑命令修改草稿 ----

        [Test]
        public void test_add_and_remove_plan_order()
        {
            CampaignSession s = NewSession();

            CampaignCommandResult add = Service.AddPlanOrder(s, Order());
            Assert.That(add.Applied, Is.True);
            Assert.That(s.PlanOrders!.Count, Is.EqualTo(1));

            CampaignCommandResult rem = Service.RemovePlanOrder(s, new OrderId("order-ambush"));
            Assert.That(rem.Applied, Is.True);
            Assert.That(s.PlanOrders!.Count, Is.EqualTo(0));
        }

        [Test]
        public void test_remove_nonexistent_order_fails_without_throw()
        {
            CampaignSession s = NewSession();
            CampaignCommandResult rem = Service.RemovePlanOrder(s, new OrderId("order-ghost"));
            Assert.That(rem.Applied, Is.False, "删不存在命令返回失败但不抛");
        }

        [Test]
        public void test_add_duplicate_order_id_rejected()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order());
            CampaignCommandResult dup = Service.AddPlanOrder(s, Order());
            Assert.That(dup.Applied, Is.False, "同 id 命令拒绝");
        }

        // ---- AC-3: 草稿编辑不改权威态（TR-prep-001）----

        [Test]
        public void test_draft_edit_does_not_change_authoritative_state()
        {
            CampaignSession s = NewSession(Pool(100));

            Service.AddPlanOrder(s, Order(grainNeed: 40));

            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(100), "草稿不扣资源");
            Assert.That(s.CommittedPlan, Is.Null, "草稿非承诺");
        }

        // ---- AC-4: 准备态纳入会话哈希 ----

        [Test]
        public void test_resource_pool_enters_session_hash()
        {
            CampaignSession a = NewSession(Pool(100));
            CampaignSession b = NewSession(Pool(200));
            Assert.That(a.ComputeHash(), Is.Not.EqualTo(b.ComputeHash()), "资源池进哈希");
        }

        [Test]
        public void test_draft_order_enters_session_hash()
        {
            CampaignSession withOrder = NewSession();
            Service.AddPlanOrder(withOrder, Order());
            CampaignSession empty = NewSession();

            Assert.That(withOrder.ComputeHash(), Is.Not.EqualTo(empty.ComputeHash()), "草稿命令进哈希");
        }

        // ---- AC-5: 无准备配置向后兼容 ----

        [Test]
        public void test_session_without_preparation_config_has_no_preparation()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            Assert.That(s.HasPreparation, Is.False);
            Assert.That(s.PlanOrders, Is.Null);
        }
    }
}
