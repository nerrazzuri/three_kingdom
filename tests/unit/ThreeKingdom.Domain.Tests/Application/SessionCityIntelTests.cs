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

        /// <summary>派出侦察并推进至返报（非即时）；返回返报后的情报投影。</summary>
        private static ThreeKingdom.Domain.Intel.IntelProjection ScoutAndResolve(SessionService service, GameSession session)
        {
            service.DispatchScout(session);
            service.Advance(session, Scenario.ScoutLeadSegments);
            return service.ProjectIntel(session);
        }

        [Test]
        public void test_scout_is_not_instant_intel_only_arrives_after_return()
        {
            var service = new SessionService();
            var session = service.NewGame();

            service.DispatchScout(session);
            Assert.That(service.ProjectIntel(session).Count, Is.EqualTo(0), "派出当时尚无情报——返报需时（非即时暴露）。");
            Assert.That(service.ProjectScout(session).InFlight, Is.True);

            var intel = ScoutAndResolve(new SessionService(), service.NewGame()); // 另证：返报后有情报
            Assert.That(intel.Count, Is.GreaterThan(0));
        }

        [Test]
        public void test_scout_records_truth_at_return_time()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var intel = ScoutAndResolve(service, session); // 派出 + 推进 ScoutLead 返报

            Assert.That(intel.TryGet(Scenario.EnemySubject, out var entry), Is.True);
            // 返报在同日内（ScoutLead=2<一日），未跨日界 → 敌真值仍为初值。
            Assert.That(entry.KnownStrength, Is.EqualTo(Scenario.EnemyInitialStrength));
            Assert.That(entry.ObservedAt, Is.EqualTo(new WorldTime(0, DaySegment.Dawn).Advance(Scenario.ScoutLeadSegments)));
            Assert.That(entry.Source, Is.EqualTo(IntelSource.Scouting));
        }

        [Test]
        public void test_intel_grows_stale_when_time_passes_without_rescouting()
        {
            var service = new SessionService();
            var session = service.NewGame();
            ScoutAndResolve(service, session);                      // 侦察返报，留下情报基准
            var atScout = service.ProjectIntel(session);
            atScout.TryGet(Scenario.EnemySubject, out var baseline);

            service.Advance(session, WorldTime.SegmentsPerDay * 2);  // 过两日，敌真值已漂移

            var intel = service.ProjectIntel(session);
            intel.TryGet(Scenario.EnemySubject, out var entry);
            Assert.That(entry.KnownStrength, Is.EqualTo(baseline.KnownStrength), "不再侦察 → 所持估计值停在旧值（过时）。");
            Assert.That(entry.ObservedAt, Is.EqualTo(baseline.ObservedAt), "观察时间停在上次返报时刻。");
        }

        [Test]
        public void test_rescout_after_days_reflects_grown_truth()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.Advance(session, WorldTime.SegmentsPerDay * 2); // 敌真值 +2 日增援
            var intel = ScoutAndResolve(service, session);          // 重新侦察返报

            intel.TryGet(Scenario.EnemySubject, out var entry);
            int expected = Scenario.EnemyInitialStrength + Scenario.EnemyReinforcePerDay * 2;
            Assert.That(entry.KnownStrength, Is.EqualTo(expected), "重新侦察反映漂移后的真值。");
        }
    }
}
