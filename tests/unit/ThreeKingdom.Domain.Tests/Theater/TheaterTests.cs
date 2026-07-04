using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Theater;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Theater;

namespace ThreeKingdom.Domain.Tests.Theater
{
    /// <summary>
    /// 多城战区（GDD M12 / epic-025）：直辖多城+委任 · 委任 AI 不越权 · 跨城资源守恒 · 掌管范围随官阶 + 反全知报告。
    /// </summary>
    [TestFixture]
    public class TheaterTests
    {
        private static CityId City(string id) => new CityId(id);
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private readonly TheaterService _service = new TheaterService();

        // ---- S1 直辖多城 + 委任 + 存读档 ----

        [Test]
        public void test_hold_multiple_cities_and_delegate()
        {
            TheaterState s = TheaterState.Empty.AddCity(City("c-a")).AddCity(City("c-b"));
            Assert.That(s.Count, Is.EqualTo(2));
            Assert.That(s.SelfGovernedCount, Is.EqualTo(2), "占城默认亲管。");

            TheaterCommandResult r = _service.Delegate(s, City("c-b"), Aide);
            Assert.That(r.Applied, Is.True);
            Assert.That(r.State.Of(City("c-b"))!.Mode, Is.EqualTo(GovernanceMode.Delegated), "可委任下属打理。");
            Assert.That(r.State.Of(City("c-b"))!.Governor, Is.EqualTo(Aide));
            Assert.That(r.State.DelegatedCount, Is.EqualTo(1));
        }

        [Test]
        public void test_theater_state_hash_roundtrip_and_sensitive()
        {
            TheaterState a = TheaterState.Empty.AddCity(City("c-a")).Delegate(City("c-a"), Aide);
            TheaterState b = new TheaterState(new List<CityHolding>(a.Holdings));
            Assert.That(b.Hash(), Is.EqualTo(a.Hash()), "同态 → 同哈希（存读档一致）。");
            Assert.That(a.AddCity(City("c-z")).Hash(), Is.Not.EqualTo(a.Hash()), "多一城 → 哈希变。");
        }

        // ---- S2 委任 AI 本地自理·不越权 ----

        [Test]
        public void test_delegate_governance_chooses_local_action_by_situation()
        {
            var svc = new DelegateGovernanceService();
            var cfg = DelegateGovernanceConfig.Default;
            Assert.That(svc.ChooseAction(200, morale: 20, fortification: 80, cfg), Is.EqualTo(DelegateAction.Appease), "民心低→安抚。");
            Assert.That(svc.ChooseAction(200, morale: 80, fortification: 20, cfg), Is.EqualTo(DelegateAction.Repair), "工事低→修。");
            Assert.That(svc.ChooseAction(200, morale: 80, fortification: 80, cfg), Is.EqualTo(DelegateAction.Requisition), "粮丰→征。");
            Assert.That(svc.ChooseAction(0, morale: 80, fortification: 80, cfg), Is.EqualTo(DelegateAction.Idle), "态平→无为。");
        }

        [Test]
        public void test_delegate_action_space_has_no_strategic_action()
        {
            // 负向不变量：委任 AI 不越权——动作空间结构上仅本地治理，无出征/宣战/战区令等战略。
            string[] strategic = { "offensive", "campaign", "war", "declare", "theater", "strategic", "conquer", "siege" };
            foreach (string name in Enum.GetNames(typeof(DelegateAction)))
                foreach (string bad in strategic)
                    Assert.That(name.ToLowerInvariant().Contains(bad), Is.False, $"委任动作不得含战略动作：{name}");
        }

        // ---- S3 跨城资源守恒 ----

        [Test]
        public void test_cross_city_resource_transfer_conserves_total()
        {
            var res = new TheaterResources(new Dictionary<string, long> { ["c-a"] = 100, ["c-b"] = 50 });
            long before = res.Total;
            TheaterResources after = res.Transfer(City("c-a"), City("c-b"), 30);
            Assert.That(after.Total, Is.EqualTo(before), "跨城调粮守恒（总量不变）。");
            Assert.That(after.StockOf(City("c-a")), Is.EqualTo(70));
            Assert.That(after.StockOf(City("c-b")), Is.EqualTo(80));
        }

        [Test]
        public void test_transfer_rejects_insufficient_source()
        {
            var res = new TheaterResources(new Dictionary<string, long> { ["c-a"] = 100, ["c-b"] = 50 });
            Assert.Throws<InvalidOperationException>(() => res.Transfer(City("c-a"), City("c-b"), 1000), "源粮不足 → 拒（无凭空产出）。");
        }

        // ---- S4 掌管范围随官阶 + 反全知报告 ----

        [Test]
        public void test_self_govern_bounded_by_rank_span()
        {
            // 阶0 亲管上限 1；持 2 城、委任 1。
            TheaterState s = TheaterState.Empty.AddCity(City("c-a")).AddCity(City("c-b")).Delegate(City("c-b"), Aide);
            Assert.That(s.SelfGovernedCount, Is.EqualTo(1));

            // 阶0 收回 c-b 亲管 → 会成 2 亲管 > 上限 1 → 拒。
            TheaterCommandResult r0 = _service.SelfGovern(s, City("c-b"), rank: 0, SpanOfControlConfig.Default);
            Assert.That(r0.Error, Is.EqualTo(TheaterCommandError.ExceedsSpan), "阶0 超出亲管范围 → 拒（须委任或升官）。");

            // 阶3 亲管上限 3 → 允许。
            TheaterCommandResult r3 = _service.SelfGovern(s, City("c-b"), rank: 3, SpanOfControlConfig.Default);
            Assert.That(r3.Applied, Is.True, "升官后亲管范围放宽 → 可收回。");
        }

        [Test]
        public void test_theater_report_marks_delegated_cities_not_fresh()
        {
            // 反全知：亲管城即时准确，委任城经下属汇报（Fresh=false，非全知面板）。
            TheaterState s = TheaterState.Empty.AddCity(City("c-a")).AddCity(City("c-b")).Delegate(City("c-b"), Aide);
            var res = new TheaterResources(new Dictionary<string, long> { ["c-a"] = 100, ["c-b"] = 50 });
            var reports = new TheaterReportService().Build(s, res);

            TheaterCityReport a = reports.First(r => r.City == City("c-a"));
            TheaterCityReport b = reports.First(r => r.City == City("c-b"));
            Assert.That(a.Fresh, Is.True, "亲管城即时准确。");
            Assert.That(b.Fresh, Is.False, "委任城经下属汇报（反全知，非即时全知）。");
        }
    }
}
