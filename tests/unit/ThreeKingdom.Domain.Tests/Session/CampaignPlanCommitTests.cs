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
    /// epic-018 story-002：合法计划原子提交（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（提交经 PlanCommitService 原子）+ ADR-0004（确定性）。TR-prep-001。
    /// 覆盖：合法草稿 → CommittedPlan + 资源原子扣减；承诺内容；确定性。
    /// </summary>
    [TestFixture]
    public class CampaignPlanCommitTests
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

        // ---- AC-1: 合法计划原子提交生成 CommittedPlan ----

        [Test]
        public void test_legal_plan_commits_atomically()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order());

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.True);
            Assert.That(s.CommittedPlan, Is.Not.Null);
            Assert.That(s.CommittedPlan!.Orders.Count, Is.EqualTo(1));
        }

        // ---- AC-2: 合法提交原子扣减资源 ----

        [Test]
        public void test_commit_deducts_resources()
        {
            CampaignSession s = NewSession(Pool(100));
            Service.AddPlanOrder(s, Order(grainNeed: 40));

            Service.SubmitPlan(s);

            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(60), "100 − 40 锁定");
        }

        [Test]
        public void test_commit_exact_resource_leaves_zero()
        {
            CampaignSession s = NewSession(Pool(40));
            Service.AddPlanOrder(s, Order(grainNeed: 40));

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.True, "恰好够可提交");
            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(0));
        }

        // ---- AC-3: 提交返回承诺计划内容 ----

        [Test]
        public void test_committed_plan_contents()
        {
            CampaignSession s = NewSession(Pool(100));
            Service.AddPlanOrder(s, Order("order-ambush", 40));

            Service.SubmitPlan(s);

            Assert.That(s.CommittedPlan!.Orders[0].Id, Is.EqualTo(new OrderId("order-ambush")));
            Assert.That(s.CommittedPlan!.CommittedResources[Grain], Is.EqualTo(40), "总需求锁定");
        }

        // ---- AC-4: 提交确定性 ----

        [Test]
        public void test_commit_is_deterministic()
        {
            CampaignSession a = NewSession(Pool(100));
            CampaignSession b = NewSession(Pool(100));
            Service.AddPlanOrder(a, Order());
            Service.AddPlanOrder(b, Order());

            Service.SubmitPlan(a);
            Service.SubmitPlan(b);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同草稿同资源 → 同哈希");
        }
    }
}
