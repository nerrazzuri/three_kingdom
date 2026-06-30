using System;
using System.Collections.Generic;
using System.Linq;
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
    /// epic-018 story-003：冲突 DAG 拒绝非法计划（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（硬冲突阻止提交，无部分写入）+ ADR-0004（确定性）。TR-prep-002。
    /// 覆盖：资源不足/不可达/无权限/循环依赖 → 拒绝、资源不变、无 CommittedPlan、可继续。
    /// </summary>
    [TestFixture]
    public class CampaignPlanConflictTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly RegionId Pass = new RegionId("region-pass");
        private static readonly RegionId FarLand = new RegionId("region-far");
        private static readonly ResourceKey Grain = new ResourceKey("res-grain");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static ResourcePool Pool(long grain = 100)
            => new ResourcePool(new Dictionary<ResourceKey, long> { [Grain] = grain });

        private static PreparedOrder Order(
            string id, RegionId target, long grainNeed = 40, IReadOnlyList<OrderId>? deps = null)
            => new PreparedOrder(
                new OrderId(id), Aide, target, new TimeWindow(0, 2),
                new Dictionary<ResourceKey, long> { [Grain] = grainNeed }, deps);

        private static CampaignStartConfig Config()
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
                resourcePool: Pool(100),
                preparationConfig: new PreparationConfig(tightResourceMargin: 10),
                reachableRegions: new[] { Pass },           // 仅 Pass 可达，FarLand 不可达
                authorizedOrders: new[] { new OrderId("order-ambush"), new OrderId("order-a"), new OrderId("order-b") });
                                                            // order-unauth 不在授权集

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;

        // ---- AC-1: 资源不足拒绝 + 资源不变 ----

        [Test]
        public void test_resource_shortage_rejected_pool_unchanged()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", Pass, grainNeed: 500));   // 需 500 > 100
            StateHash before = s.ComputeHash();

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.False);
            Assert.That(r.Validation.Errors.Any(e => e.Code == PlanErrorCode.ResourceShortage), Is.True);
            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(100), "资源不变");
            Assert.That(s.CommittedPlan, Is.Null);
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "拒绝零写入");
        }

        // ---- AC-2: 目标不可达拒绝 ----

        [Test]
        public void test_unreachable_target_rejected()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", FarLand, grainNeed: 40));   // FarLand 不可达
            StateHash before = s.ComputeHash();

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.False);
            Assert.That(r.Validation.Errors.Any(e => e.Code == PlanErrorCode.Unreachable), Is.True);
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-3: 执行者无权限拒绝 ----

        [Test]
        public void test_unauthorized_order_rejected()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-unauth", Pass, grainNeed: 40));   // 不在授权集
            StateHash before = s.ComputeHash();

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.False);
            Assert.That(r.Validation.Errors.Any(e => e.Code == PlanErrorCode.NoAuthority), Is.True);
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-4: 循环依赖（非 DAG）拒绝 ----

        [Test]
        public void test_cyclic_dependency_rejected()
        {
            CampaignSession s = NewSession();
            // A 依赖 B、B 依赖 A → 环
            Service.AddPlanOrder(s, Order("order-a", Pass, 20, new[] { new OrderId("order-b") }));
            Service.AddPlanOrder(s, Order("order-b", Pass, 20, new[] { new OrderId("order-a") }));
            StateHash before = s.ComputeHash();

            SubmitPlanResult r = Service.SubmitPlan(s);

            Assert.That(r.Committed, Is.False);
            Assert.That(r.Validation.Errors.Any(e => e.Code == PlanErrorCode.CyclicDependency), Is.True);
            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(100), "资源不变");
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-5: 拒绝后会话可继续 ----

        [Test]
        public void test_session_continues_after_rejected_submit()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", Pass, grainNeed: 999));   // 超额被拒
            Service.SubmitPlan(s);

            // 修正：移除非法命令，加合法命令
            Service.RemovePlanOrder(s, new OrderId("order-ambush"));
            Service.AddPlanOrder(s, Order("order-ambush", Pass, grainNeed: 40));
            SubmitPlanResult ok = Service.SubmitPlan(s);

            Assert.That(ok.Committed, Is.True, "失败后合法提交仍成功");
            Assert.That(s.Pool!.Get(Grain), Is.EqualTo(60));
        }
    }
}
