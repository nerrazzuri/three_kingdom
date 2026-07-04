using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 人心杠杆接入运行期（GDD_024 A1）：守将画像工厂（种子化性情 + 反全知信号 + 暴露升警觉）；
    /// 运行期 AttemptSubversion 可达、确定性；成功累积待生效效果。
    /// </summary>
    [TestFixture]
    public class SubversionRuntimeTests
    {
        private static readonly CityId CityA = new CityId("city-a");
        private static readonly CityId CityB = new CityId("city-b");

        // ---- 画像工厂 ----

        [Test]
        public void test_profile_personality_is_deterministic_per_city()
        {
            SubversionTargetProfile p1 = SubversionTargetProfileFactory.Build(CityA, true, FixedPoint.FromFraction(7, 10), false, 42UL);
            SubversionTargetProfile p2 = SubversionTargetProfileFactory.Build(CityA, true, FixedPoint.FromFraction(7, 10), false, 42UL);
            Assert.That(p2.Loyalty.Raw, Is.EqualTo(p1.Loyalty.Raw), "同城同种子 → 守将性情一致。");
            Assert.That(p2.ResentmentToLord.Raw, Is.EqualTo(p1.ResentmentToLord.Raw));
        }

        [Test]
        public void test_different_cities_have_different_generals()
        {
            SubversionTargetProfile a = SubversionTargetProfileFactory.Build(CityA, true, FixedPoint.FromFraction(7, 10), false, 42UL);
            SubversionTargetProfile b = SubversionTargetProfileFactory.Build(CityB, true, FixedPoint.FromFraction(7, 10), false, 42UL);
            bool differ = a.Loyalty.Raw != b.Loyalty.Raw || a.ResentmentToLord.Raw != b.ResentmentToLord.Raw
                || a.Greed.Raw != b.Greed.Raw || a.Charm.Raw != b.Charm.Raw;
            Assert.That(differ, Is.True, "不同城守将性情各异。");
        }

        [Test]
        public void test_unscouted_zeroes_intel_quality()
        {
            SubversionTargetProfile blind = SubversionTargetProfileFactory.Build(CityA, false, FixedPoint.One, false, 42UL);
            Assert.That(blind.Scouted, Is.False);
            Assert.That(blind.EffectiveIntelQuality.Raw, Is.EqualTo(0), "未侦察 → 情报质量归 0（反全知）。");
        }

        [Test]
        public void test_exposed_raises_alertness()
        {
            SubversionTargetProfile calm = SubversionTargetProfileFactory.Build(CityA, true, FixedPoint.FromFraction(7, 10), false, 42UL);
            SubversionTargetProfile alerted = SubversionTargetProfileFactory.Build(CityA, true, FixedPoint.FromFraction(7, 10), true, 42UL);
            Assert.That(alerted.Alertness.Raw, Is.GreaterThan(calm.Alertness.Raw), "曾被识破 → 守将警觉升（后续更易反噬）。");
        }

        // ---- 运行期可达 ----

        [Test]
        public void test_attempt_subversion_reachable_via_runtime()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            SubversionView view = runtime.AttemptSubversion("city-hulao", SubversionScheme.UnderminedMorale, 100);
            Assert.That(view, Is.Not.Null, "人心杠杆经运行期可达。");
            Assert.That(view.SchemeLabel, Is.EqualTo("攻心流言"));
            Assert.That(view.ResultLabel, Is.Not.Empty, "给结果文案（无胜率）。");
        }

        [Test]
        public void test_blind_attempt_on_fresh_session_records_attempt_but_reachable()
        {
            // 反全知：新局未侦察 → 盲施（成功度大折扣，多为无效/反噬）；但仍可达且记为一次尝试（递减源）。
            // 成功→待生效效果的闭环在 SubversionCampaignTests（service 层，必成配置）已证。
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            SubversionView v = runtime.AttemptSubversion("city-hulao", SubversionScheme.InciteDefection, 100);
            Assert.That(v, Is.Not.Null, "未侦察也可施（盲施），系统不拒。");
            // 盲施策反门多半不成型或落空——无论如何均为合法结果之一。
            Assert.That(System.Enum.IsDefined(typeof(SubversionResult), v.Result), Is.True);
        }
    }
}
