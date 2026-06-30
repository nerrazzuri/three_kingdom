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
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;
using TimeWindow = ThreeKingdom.Domain.Preparation.TimeWindow;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-018 story-004：准备态存读档 + 确定性（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档 round-trip）+ ADR-0004（确定性）。TR-prep-001。
    /// 覆盖：准备态逐字段一致；提交后承诺态一致；确定性链；未提供配置整体拒绝；向后兼容。
    /// </summary>
    [TestFixture]
    public class CampaignPreparationSaveTests
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

        private static PreparedOrder Order(string id = "order-ambush", long grainNeed = 40, IReadOnlyList<OrderId>? deps = null)
            => new PreparedOrder(
                new OrderId(id), Aide, Pass, new TimeWindow(1, 3),
                new Dictionary<ResourceKey, long> { [Grain] = grainNeed }, deps);

        private static PreparationConfig PrepCfg() => new PreparationConfig(tightResourceMargin: 10);
        private static RegionId[] Reachable() => new[] { Pass };
        private static OrderId[] Authorized() => new[] { new OrderId("order-ambush"), new OrderId("order-raid") };

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
                resourcePool: Pool(),
                preparationConfig: PrepCfg(),
                reachableRegions: Reachable(),
                authorizedOrders: Authorized());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;
        private static CampaignSession Restore(string text)
            => Service.Restore(text, Fp, prepConfig: PrepCfg(), reachableRegions: Reachable(), authorizedOrders: Authorized());

        // ---- AC-1: 准备态 round-trip 逐字段一致 ----

        [Test]
        public void test_draft_roundtrip_field_for_field()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", 40, new[] { new OrderId("order-raid") }));

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.PlanOrders!.Count, Is.EqualTo(1));
            PreparedOrder o = loaded.PlanOrders![0];
            Assert.That(o.Id, Is.EqualTo(new OrderId("order-ambush")));
            Assert.That(o.Executor, Is.EqualTo(Aide));
            Assert.That(o.Target, Is.EqualTo(Pass));
            Assert.That(o.Window.Start, Is.EqualTo(1));
            Assert.That(o.Window.End, Is.EqualTo(3));
            Assert.That(o.ResourceNeeds[Grain], Is.EqualTo(40));
            Assert.That(o.Dependencies.Count, Is.EqualTo(1));
            Assert.That(o.Dependencies[0], Is.EqualTo(new OrderId("order-raid")));
            Assert.That(loaded.Pool!.Get(Grain), Is.EqualTo(100));
        }

        // ---- AC-2: 提交后承诺态 round-trip ----

        [Test]
        public void test_committed_plan_roundtrip_preserves_hash()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", 40));
            Service.SubmitPlan(s);
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
            Assert.That(loaded.CommittedPlan, Is.Not.Null);
            Assert.That(loaded.CommittedPlan!.Orders[0].Id, Is.EqualTo(new OrderId("order-ambush")));
            Assert.That(loaded.Pool!.Get(Grain), Is.EqualTo(60), "扣减后资源池恢复");
        }

        // ---- AC-3: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：编辑草稿 → 提交。
            CampaignSession direct = NewSession();
            Service.AddPlanOrder(direct, Order("order-ambush", 40));
            Service.SubmitPlan(direct);
            StateHash directHash = direct.ComputeHash();

            // 切割：编辑草稿 → 存读档 → 提交。
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order("order-ambush", 40));
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.SubmitPlan(loaded);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续提交确定性");
        }

        // ---- AC-4: 含准备态存档未提供配置 → 整体拒绝 ----

        [Test]
        public void test_restore_prep_save_without_config_is_rejected()
        {
            CampaignSession s = NewSession();
            Service.AddPlanOrder(s, Order());
            string text = Service.CaptureSnapshot(s);

            Assert.Throws<SaveFormatException>(() => Service.Restore(text, Fp), "含准备态但未提供 prepConfig 应整体拒绝");
        }

        // ---- 向后兼容：无准备的会话存读档不受影响 ----

        [Test]
        public void test_non_prep_session_roundtrip_still_works()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.HasPreparation, Is.False);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }
    }
}
