using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// epic-014 story-002（收尾 CON-5）：SliceScenario 数据驱动——字面值来自不可变 <see cref="SliceScenarioData"/>，
    /// 组装器 <see cref="SliceScenario"/> 只做数据→Domain 聚合映射，无硬编码（ADR-0003）。
    /// </summary>
    [TestFixture]
    public class SliceScenarioDataTests
    {
        [Test]
        public void test_default_data_is_single_shared_instance()
        {
            // 数据源单一来源：Default 是同一不可变实例（非每次新建）。
            Assert.That(SliceScenarioData.Default, Is.SameAs(SliceScenarioData.Default));
        }

        [Test]
        public void test_scenario_reads_values_from_injected_data()
        {
            // 组装器映射数据源：场景标量/聚合取自数据，非内联常量。
            SliceScenarioData data = SliceScenarioData.Default;
            SliceScenario scenario = new SliceScenario(data);

            Assert.That(scenario.ReliefDay, Is.EqualTo(data.ReliefDay));
            Assert.That(scenario.EnemyInitialStrength, Is.EqualTo(data.EnemyInitialStrength));
            Assert.That(scenario.EnemyWithdrawThreshold, Is.EqualTo(data.EnemyWithdrawThreshold));
            Assert.That(scenario.InitialCity.Id.Value, Is.EqualTo(data.CityId));
            Assert.That(scenario.InitialCity.Stock, Is.EqualTo(data.CityStock));
            Assert.That(scenario.PlayerFaction.Value, Is.EqualTo(data.PlayerFactionId));
            Assert.That(scenario.EnemyFaction.Value, Is.EqualTo(data.EnemyFactionId));
            Assert.That(scenario.DiplomacyPower.Value, Is.EqualTo(data.DiplomacyPowerId));
            Assert.That(scenario.PopulationPressure, Is.EqualTo(data.PopulationPressure));
        }

        [Test]
        public void test_data_drives_collections_count_and_subject_injection()
        {
            // 集合（花名册/建议）数量来自数据；建议主题由组装器统一注入 EnemySubject。
            SliceScenarioData data = SliceScenarioData.Default;
            SliceScenario scenario = new SliceScenario(data);

            Assert.That(scenario.Roster.Count, Is.EqualTo(data.CharacterSpecs.Count));
            Assert.That(scenario.AdviceTemplates.Count, Is.EqualTo(data.AdviceSpecs.Count));
            foreach (var advice in scenario.AdviceTemplates)
            {
                Assert.That(advice.ReferencedSubjects, Has.Count.EqualTo(1));
                Assert.That(advice.ReferencedSubjects[0], Is.EqualTo(scenario.EnemySubject));
            }
            // 花名册按 spec 顺序映射身份（数据驱动，无内联人物）。
            for (int i = 0; i < scenario.Roster.Count; i++)
                Assert.That(scenario.Roster[i].Identity, Is.EqualTo(data.CharacterSpecs[i].Identity));
        }

        [Test]
        public void test_default_factory_uses_default_data_source()
        {
            // SliceScenario.Default() 从 SliceScenarioData.Default 组装：不再以内联工厂为唯一源。
            SliceScenario fromFactory = SliceScenario.Default();
            SliceScenario fromData = new SliceScenario(SliceScenarioData.Default);

            Assert.That(fromFactory.ReliefDay, Is.EqualTo(fromData.ReliefDay));
            Assert.That(fromFactory.EnemyInitialStrength, Is.EqualTo(fromData.EnemyInitialStrength));
            Assert.That(fromFactory.Roster.Count, Is.EqualTo(fromData.Roster.Count));
            Assert.That(fromFactory.DiplomacyRngSeed, Is.EqualTo(fromData.DiplomacyRngSeed));
        }
    }
}
