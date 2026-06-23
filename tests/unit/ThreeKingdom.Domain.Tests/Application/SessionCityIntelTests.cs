using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续：会话编排城市日界结算（GDD_004）+ 敌情侦察/时效（GDD_007）。BLOCKING。
    /// 治理 ADR：ADR-0002（用例编排，UI 只拿投影）+ ADR-0004（确定性）+ ADR-0003（数值来自配置）。
    /// </summary>
    [TestFixture]
    public class SessionCityIntelTests
    {
        private static readonly SliceScenario Scenario = SliceScenario.Default();

        // ---- 城市账本（GDD_004）----

        [Test]
        public void test_new_game_city_matches_scenario_initial_state()
        {
            var service = new SessionService();

            var ledger = service.ProjectCity(service.NewGame());

            Assert.That(ledger.Stock, Is.EqualTo(Scenario.InitialCity.Stock));
            Assert.That(ledger.CivMorale, Is.EqualTo(Scenario.InitialCity.CivMorale));
            Assert.That(ledger.Security, Is.EqualTo(Scenario.InitialCity.Security));
            Assert.That(ledger.Fortification, Is.EqualTo(Scenario.InitialCity.FortificationCurrent));
            Assert.That(ledger.FortificationMax, Is.EqualTo(Scenario.InitialCity.FortificationMax));
            Assert.That(ledger.LastDayShortage, Is.EqualTo(0));
        }

        [Test]
        public void test_advancing_one_day_settles_city_by_yield_minus_consumption()
        {
            var service = new SessionService();
            var session = service.NewGame();

            service.Advance(session, WorldTime.SegmentsPerDay); // 跨一日 → 结算一次
            var ledger = service.ProjectCity(session);

            // 净额 = 基础产入 − 民用需求（库存充足、无短缺）。
            long expected = Scenario.InitialCity.Stock + Scenario.CityConfig.BaseYield - Scenario.CityConfig.BaseCivConsume;
            Assert.That(ledger.Stock, Is.EqualTo(expected));
            Assert.That(ledger.LastDayShortage, Is.EqualTo(0), "库存充足时不应短缺。");
        }

        [Test]
        public void test_advancing_within_a_day_does_not_settle_city()
        {
            var service = new SessionService();
            var session = service.NewGame();

            service.Advance(session, 1); // 黎明→白昼，未跨日
            var ledger = service.ProjectCity(session);

            Assert.That(ledger.Stock, Is.EqualTo(Scenario.InitialCity.Stock), "未跨日界不触发城市结算。");
        }

        [Test]
        public void test_prolonged_advance_clamps_stock_at_floor_and_triggers_shortage()
        {
            var service = new SessionService();
            var session = service.NewGame();

            service.Advance(session, WorldTime.SegmentsPerDay * 12); // 推进足够多日
            var ledger = service.ProjectCity(session);

            Assert.That(ledger.Stock, Is.EqualTo(Scenario.CityConfig.StockFloor), "库存夹至下限不出负、不凭空补齐。");
            Assert.That(ledger.LastDayShortage, Is.GreaterThan(0), "库存触底后民用需求无法满足 → 短缺。");
            Assert.That(ledger.CivMorale, Is.LessThan(Scenario.InitialCity.CivMorale), "短缺应损耗民心。");
        }

        // ---- 敌情侦察与时效（GDD_007）----

        [Test]
        public void test_before_scouting_no_intel_known()
        {
            var service = new SessionService();

            var intel = service.ProjectIntel(service.NewGame());

            Assert.That(intel.Count, Is.EqualTo(0), "未侦察前阵营知识为空（不泄露真值）。");
        }

        [Test]
        public void test_scout_records_current_truth_at_current_time()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var intel = service.Scout(session);

            Assert.That(intel.TryGet(Scenario.EnemySubject, out var entry), Is.True);
            Assert.That(entry.KnownStrength, Is.EqualTo(Scenario.EnemyInitialStrength));
            Assert.That(entry.ObservedAt, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)));
            Assert.That(entry.Source, Is.EqualTo(IntelSource.Scouting));
        }

        [Test]
        public void test_intel_grows_stale_when_time_passes_without_rescouting()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.Scout(session);                              // 第 0 日黎明侦察
            service.Advance(session, WorldTime.SegmentsPerDay * 2); // 过两日，敌真值已漂移

            var intel = service.ProjectIntel(session);
            intel.TryGet(Scenario.EnemySubject, out var entry);

            Assert.That(entry.KnownStrength, Is.EqualTo(Scenario.EnemyInitialStrength), "不再侦察 → 所持估计值停在旧值（过时）。");
            Assert.That(entry.ObservedAt, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)), "观察时间停在上次侦察时刻。");
        }

        [Test]
        public void test_rescout_after_days_reflects_grown_truth_and_new_time()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.Advance(session, WorldTime.SegmentsPerDay * 2); // 敌真值 +2 日增援
            var intel = service.Scout(session);                    // 重新侦察

            intel.TryGet(Scenario.EnemySubject, out var entry);
            int expected = Scenario.EnemyInitialStrength + Scenario.EnemyReinforcePerDay * 2;
            Assert.That(entry.KnownStrength, Is.EqualTo(expected), "重新侦察反映漂移后的真值。");
            Assert.That(entry.ObservedAt, Is.EqualTo(new WorldTime(2, DaySegment.Dawn)));
        }
    }
}
