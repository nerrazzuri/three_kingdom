using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-013 story-001：CampaignSession 骨架 + 配置驱动开局（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配边界）+ ADR-0002（四层）+ ADR-0003（数据驱动配置）。TR-session-001/003。
    /// 覆盖 AC-1/3/5 层级与 R-5 闸门（反射）、AC-2/4 配置驱动开局 + 指纹 + 非法拒。
    /// </summary>
    [TestFixture]
    public class CampaignSessionSkeletonTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig Config()
            => new CampaignStartConfig(
                scenarioConfigId: "scenario-fanshui-siege",
                fingerprint: new ConfigFingerprint(0xCA11AB1EUL),
                governorSeed: new CitySeed(Player, Fanshui, garrison: 800, fortification: 60, output: 20,
                    new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                startTime: new WorldTime(0, DaySegment.Dawn),
                initialFactions: new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                initialCities: new[] { new CityOwnership(Fanshui, Player, 800) });

        private static readonly CampaignSessionService Service = new CampaignSessionService();

        // ---- AC-1/3/5：层级 + R-5 闸门（反射）----

        [Test]
        public void test_campaign_session_lives_in_application_layer()
        {
            Assert.That(typeof(CampaignSession).Namespace, Is.EqualTo("ThreeKingdom.Application.Session"));
            Assert.That(typeof(CampaignSessionService).Namespace, Is.EqualTo("ThreeKingdom.Application.Session"));
            // Application 程序集引用 Domain（非反向）：CareerState 在不同程序集。
            Assert.That(typeof(CampaignSession).Assembly, Is.Not.EqualTo(typeof(CareerState).Assembly));
        }

        [Test]
        public void test_campaign_assembly_does_not_reference_unityengine()
        {
            // ADR-0002/0009：装配层纯 C#，不依赖 UnityEngine。
            bool refsUnity = typeof(CampaignSession).Assembly.GetReferencedAssemblies()
                .Any(a => a.Name != null && a.Name.IndexOf("UnityEngine", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(refsUnity, Is.False, "CampaignSession 所在程序集不应引用 UnityEngine。");
        }

        [Test]
        public void test_campaign_session_exposes_no_public_rule_mutator()
        {
            // R-5：装配类只编排，不暴露 Set*/Compute*/Write* 玩法写方法（仅只读 getter + 服务入口）。
            foreach (Type t in new[] { typeof(CampaignSession), typeof(CampaignSessionService) })
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (m.IsSpecialName) continue; // 属性 getter
                    string n = m.Name;
                    bool ruleMutator = n.StartsWith("Set", StringComparison.Ordinal)
                        || n.StartsWith("Compute", StringComparison.Ordinal)
                        || n.StartsWith("Write", StringComparison.Ordinal)
                        || n.StartsWith("Apply", StringComparison.Ordinal);
                    Assert.That(ruleMutator, Is.False, $"{t.Name} 不应暴露玩法写方法：{n}（R-5 闸门）。");
                }
            }
        }

        // ---- AC-2/4：配置驱动开局 ----

        [Test]
        public void test_start_campaign_builds_session_from_config()
        {
            CampaignStartConfig config = Config();
            CampaignStartResult r = Service.StartCampaign(config);

            Assert.That(r.Started, Is.True);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.None));
            CampaignSession s = r.Session!;
            Assert.That(s.Fingerprint, Is.EqualTo(config.Fingerprint));          // 指纹进会话
            Assert.That(s.ScenarioConfigId, Is.EqualTo("scenario-fanshui-siege"));
            Assert.That(s.Career.Career.Faction, Is.EqualTo(Player));            // 生涯绑太守
            Assert.That(s.Career.Career.Rank, Is.EqualTo(Rank.CityGovernor));
            Assert.That(s.Career.Retinue.IsMember(Aide), Is.True);
            Assert.That(s.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player)); // 世界归属投影
            Assert.That(s.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)));
        }

        [Test]
        public void test_start_campaign_null_config_returns_stable_code()
        {
            CampaignStartResult r = Service.StartCampaign(null!);
            Assert.That(r.Started, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.NullConfig));
            Assert.That(r.Session, Is.Null);
        }

        [Test]
        public void test_config_rejects_empty_scenario_id()
        {
            Assert.Throws<ArgumentException>(() => new CampaignStartConfig(
                "  ", new ConfigFingerprint(1UL),
                new CitySeed(Player, Fanshui, 800, 60, 20, Array.Empty<RetinueMember>()),
                new WorldTime(0, DaySegment.Dawn),
                Array.Empty<FactionRecord>(), Array.Empty<CityOwnership>()));
        }
    }
}
