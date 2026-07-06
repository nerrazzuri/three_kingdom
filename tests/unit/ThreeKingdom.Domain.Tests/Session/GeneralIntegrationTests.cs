using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Appointment;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 武将全局融入 P2–P8（GDD_027）：招揽池 / 任用簿 / 内政 / 军师 / 派系 / 成军 / 历史事件。纯 C# 无场景依赖。
    /// </summary>
    [TestFixture]
    public class GeneralIntegrationTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);
        private static System.Collections.Generic.List<CharacterId> L(params string[] ids)
        {
            var l = new System.Collections.Generic.List<CharacterId>();
            foreach (string i in ids) l.Add(C(i));
            return l;
        }

        // ---- P2 招揽统一 ----
        [Test]
        public void test_p2_wandering_pool_and_difficulty()
        {
            var pool = GeneralRecruitment.PoolAt(190);
            Assert.That(pool.Count, Is.GreaterThan(0), "在野在世将成可招池。");
            bool hasSimahui = false;
            foreach (RecruitCandidate c in pool) if (c.GeneralId == "char-simahui") hasSimahui = true;
            Assert.That(hasSimahui, Is.True, "名士司马徽在野可招。");
            // 难度定性：仁德者易招、狼顾/傲物者难招。
            Assert.That(new[] { "易招", "尚可", "难招" }, Does.Contain(GeneralRecruitment.DifficultyOf(C("char-simahui"))));
        }

        // ---- P3 任用簿 ----
        [Test]
        public void test_p3_appointment_cap_transfer_and_immutability()
        {
            AppointmentBook book = AppointmentBook.Empty(cityCap: 2);
            var (r1, b1) = book.Assign(City("city-xiaopei"), C("char-guanyu"));
            var (r2, b2) = b1.Assign(City("city-xiaopei"), C("char-zhangfei"));
            Assert.That(r1, Is.EqualTo(AppointResult.Ok));
            Assert.That(r2, Is.EqualTo(AppointResult.Ok));
            Assert.That(b2.Roster(City("city-xiaopei")).Count, Is.EqualTo(2));

            // 满员拒。
            var (r3, _) = b2.Assign(City("city-xiaopei"), C("char-zhaoyun"));
            Assert.That(r3, Is.EqualTo(AppointResult.CityFull), "城册满 → 拒。");

            // 调他城 → 自原城移出（一将只在一城）。
            var (r4, b4) = b2.Assign(City("city-xiapi"), C("char-guanyu"));
            Assert.That(r4, Is.EqualTo(AppointResult.Ok));
            Assert.That(b4.CityOf(C("char-guanyu")), Is.EqualTo("city-xiapi"));
            Assert.That(b4.Roster(City("city-xiaopei")).Count, Is.EqualTo(1), "关羽已移出小沛。");

            // 不可变：原态不变。
            Assert.That(b2.Roster(City("city-xiaopei")).Count, Is.EqualTo(2), "旧态不受后续调拨影响。");
        }

        // ---- P4 内政 ----
        [Test]
        public void test_p4_administrator_and_yield_modifier()
        {
            // 刘备仁德 → 民心加成；册中点其为内政官。
            CharacterId? admin = GovernanceContribution.AdministratorOf(L("char-liubei", "char-zhangfei"));
            Assert.That(admin, Is.EqualTo(C("char-liubei")), "仁德者为内政官。");
            GovernanceModifier m = GovernanceContribution.ModifierOf(C("char-liubei"));
            Assert.That(m.MoralePercent, Is.GreaterThan(0), "仁德内政官增民心。");
            // 无内政官 → 无加成。
            Assert.That(GovernanceContribution.ForRoster(L()).MoralePercent, Is.EqualTo(0));
        }

        // ---- P5 军师 ----
        [Test]
        public void test_p5_advisor_tier_and_quality()
        {
            CharacterId? adv = CouncilCapability.AdvisorOf(L("char-guanyu", "char-zhugeliang"));
            Assert.That(adv, Is.EqualTo(C("char-zhugeliang")), "谋略最高者为军师。");
            Assert.That(CouncilCapability.AdviceTierOf(adv), Is.EqualTo(4), "经天纬地 → 4 档。");
            Assert.That(CouncilCapability.QualityLabel(adv), Is.EqualTo("神算"));
            // 诸葛可提策集为吕布(愚钝)超集。
            Assert.That(CouncilCapability.CanPropose(C("char-zhugeliang"), 3), Is.True);
            Assert.That(CouncilCapability.CanPropose(C("char-lubu"), 3), Is.False, "有勇无谋不能提高深之策。");
        }

        // ---- P6 派系凝聚 ----
        [Test]
        public void test_p6_cohesion_bonds_and_defection()
        {
            int taoyuan = RetinueCohesion.CohesionOf(L("char-liubei", "char-guanyu", "char-zhangfei"));
            int feud = RetinueCohesion.CohesionOf(L("char-lubu", "char-dongzhuo"));
            Assert.That(taoyuan, Is.GreaterThan(feud), "桃园知己 > 吕布董卓仇怨。");
            Assert.That(RetinueCohesion.DefectionRiskLabel(L("char-liubei", "char-guanyu", "char-zhangfei")), Is.EqualTo("稳固"));
        }

        // ---- P7 成军 ----
        [Test]
        public void test_p7_army_leader_deputy_and_power()
        {
            Army? two = ArmyFormation.Form(L("char-guanyu", "char-zhangfei"));
            Assert.That(two.HasValue, Is.True);
            Assert.That(two!.Value.HasDeputy, Is.True, "两将成军 → 主将+副将。");

            Army? one = ArmyFormation.Form(L("char-huangzhong"));
            Assert.That(one!.Value.HasDeputy, Is.False, "单将成军 → 无副将。");

            Assert.That(ArmyFormation.Form(new System.Collections.Generic.List<CharacterId>()), Is.Null, "空册不成军。");
            Assert.That(ArmyFormation.PowerContribution(two.Value), Is.GreaterThan(ArmyFormation.PowerContribution(one.Value)), "主将+副将战力 > 孤将。");
        }

        // ---- P8 演义事件（试水：桃园结义）----
        [Test]
        public void test_p8_taoyuan_event_fires_by_general_and_era()
        {
            var fires190 = LoreEvents.FiredAt(new LoreContext(190, 190, PlayableCampaign.LiuBei));
            bool taoyuan = false;
            foreach (LoreEvent e in fires190) if (e.Id == "event-taoyuan") taoyuan = true;
            Assert.That(taoyuan, Is.True, "刘备·讨董之世·刘关张俱在 → 桃园结义触发。");

            // 非刘备不触发。
            Assert.That(LoreEvents.FiredAt(new LoreContext(190, 190, PlayableCampaign.Cao)).Count, Is.EqualTo(0), "非刘备无桃园。");
            // 迟世桃园不触发（≤190 条件）——注：200 年可有他事（如世事类许攸夜奔），故只断言桃园缺席，不断言总数。
            bool taoyuan200 = false;
            foreach (LoreEvent e in LoreEvents.FiredAt(new LoreContext(200, 200, PlayableCampaign.LiuBei))) if (e.Id == "event-taoyuan") taoyuan200 = true;
            Assert.That(taoyuan200, Is.False, "200 已非结义之时，桃园不触发。");
        }
    }
}
