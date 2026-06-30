using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.City
{
    /// <summary>
    /// epic-016 story-003：治理选择改变战役条件派生（Logic / Domain）。
    /// 治理 ADR：ADR-0003（系数数据驱动）+ ADR-0004（确定性纯函数）。TR-city-004。
    /// 覆盖：工事→守城强度、征用→补给（民心代价）、民心→风险三条派生；确定性；治理选择可区分；可解释账本。
    /// 边界：只派生战役条件输入（守城强度/补给/民心风险），不接完整战斗胜负（留 M05 epic-018 消费）。
    /// </summary>
    [TestFixture]
    public class GovernanceWarConditionTests
    {
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        // 守城强度系数 2.0；民心风险阈值 50。
        private static WarConditionConfig Config()
            => new WarConditionConfig(fortDefenseFactor: FixedPoint.FromInt(2), moraleRiskThreshold: 50);

        private static CityEconomyState City(int fortCur = 20, int morale = 60)
            => new CityEconomyState(Fanshui, stock: 100, reserved: 0, civMorale: morale, security: 50,
                fortificationCurrent: fortCur, fortificationMax: 100);

        private static readonly WarConditionProjection Projection = new WarConditionProjection();

        // ---- AC-1: 工事 → 守城强度（条件①）----

        [Test]
        public void test_higher_fortification_yields_higher_defense_strength()
        {
            WarConditionInputs low = Projection.Project(City(fortCur: 20), 0, Config());
            WarConditionInputs high = Projection.Project(City(fortCur: 80), 0, Config());

            Assert.That(high.DefenseStrength, Is.GreaterThan(low.DefenseStrength), "工事越高守城强度越高");
            Assert.That(low.DefenseStrength, Is.EqualTo(40), "20 × 2.0 = 40");
            Assert.That(high.DefenseStrength, Is.EqualTo(160), "80 × 2.0 = 160");
        }

        [Test]
        public void test_zero_fortification_yields_zero_defense()
        {
            WarConditionInputs r = Projection.Project(City(fortCur: 0), 0, Config());
            Assert.That(r.DefenseStrength, Is.EqualTo(0));
        }

        // ---- AC-2: 征用 → 补给能力↑ + 民心代价（条件②）----

        [Test]
        public void test_requisition_raises_supply_with_morale_cost()
        {
            // 不征用：后勤 0、民心 60；征用后：后勤 40、民心降至 40（代价）。
            WarConditionInputs noReq = Projection.Project(City(morale: 60), logisticsHolding: 0, Config());
            WarConditionInputs withReq = Projection.Project(City(morale: 40), logisticsHolding: 40, Config());

            Assert.That(withReq.SupplyCapacity, Is.GreaterThan(noReq.SupplyCapacity), "征用提升补给");
            Assert.That(withReq.SupplyCapacity, Is.EqualTo(40));
            Assert.That(withReq.UnrestRisk, Is.GreaterThan(noReq.UnrestRisk), "征用的代价：民心降→风险升");
        }

        // ---- AC-3: 民心 → 风险（条件③）----

        [Test]
        public void test_lower_morale_yields_higher_unrest_risk()
        {
            WarConditionInputs high = Projection.Project(City(morale: 60), 0, Config());   // ≥阈值50 → 0
            WarConditionInputs low = Projection.Project(City(morale: 30), 0, Config());    // 50−30=20

            Assert.That(low.UnrestRisk, Is.GreaterThan(high.UnrestRisk), "民心越低风险越高");
            Assert.That(high.UnrestRisk, Is.EqualTo(0), "民心≥阈值无风险");
            Assert.That(low.UnrestRisk, Is.EqualTo(20), "50 − 30 = 20");
        }

        // ---- AC-4: 派生确定性 ----

        [Test]
        public void test_projection_is_deterministic()
        {
            CityEconomyState c = City(fortCur: 35, morale: 45);
            WarConditionInputs a = Projection.Project(c, 25, Config());
            WarConditionInputs b = Projection.Project(c, 25, Config());

            Assert.That(a.DefenseStrength, Is.EqualTo(b.DefenseStrength));
            Assert.That(a.SupplyCapacity, Is.EqualTo(b.SupplyCapacity));
            Assert.That(a.UnrestRisk, Is.EqualTo(b.UnrestRisk));
        }

        // ---- AC-5: 三组治理选择两两可区分 ----

        [Test]
        public void test_three_governance_choices_are_distinguishable()
        {
            // 重工事：高 fort、不征用、民心高 → 高守城、低补给、低风险。
            WarConditionInputs fortFocus = Projection.Project(City(fortCur: 90, morale: 70), 0, Config());
            // 重征用：低 fort、征用、民心低 → 低守城、高补给、高风险。
            WarConditionInputs reqFocus = Projection.Project(City(fortCur: 20, morale: 30), 60, Config());
            // 重安抚：中 fort、不征用、民心满 → 中守城、低补给、零风险。
            WarConditionInputs appeaseFocus = Projection.Project(City(fortCur: 50, morale: 100), 0, Config());

            // 三组在三维度上两两可区分（证明治理选择确实改变战役条件）。
            Assert.That(fortFocus.DefenseStrength, Is.Not.EqualTo(reqFocus.DefenseStrength));
            Assert.That(reqFocus.SupplyCapacity, Is.Not.EqualTo(fortFocus.SupplyCapacity));
            Assert.That(reqFocus.UnrestRisk, Is.Not.EqualTo(appeaseFocus.UnrestRisk));
            // 各自特征
            Assert.That(fortFocus.DefenseStrength, Is.GreaterThan(reqFocus.DefenseStrength), "重工事守城最高");
            Assert.That(reqFocus.SupplyCapacity, Is.GreaterThan(appeaseFocus.SupplyCapacity), "重征用补给最高");
            Assert.That(appeaseFocus.UnrestRisk, Is.EqualTo(0), "重安抚风险为零");
        }

        // ---- 可解释账本 ----

        [Test]
        public void test_ledger_explains_every_condition()
        {
            WarConditionInputs r = Projection.Project(City(fortCur: 35, morale: 40), 25, Config());

            Assert.That(r.Ledger.Count, Is.EqualTo(3), "三条战役条件各一账本");
            Assert.That(r.Ledger.Any(e => e.Kind == WarConditionKind.DefenseStrength), Is.True);
            Assert.That(r.Ledger.Any(e => e.Kind == WarConditionKind.SupplyCapacity), Is.True);
            Assert.That(r.Ledger.Any(e => e.Kind == WarConditionKind.UnrestRisk), Is.True);
            Assert.That(r.Ledger.All(e => !string.IsNullOrWhiteSpace(e.Note)), Is.True, "每条附可解释说明");
        }
    }
}
