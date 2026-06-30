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
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;
using WTimeWindow = ThreeKingdom.Domain.World.TimeWindow;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-023 共享夹具 + story-001：历史世界态接入会话（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配）+ ADR-0007（条件历史世界）。TR-world-001。
    /// </summary>
    [TestFixture]
    public class CampaignHistoryStateTests
    {
        internal static readonly FactionId Player = new FactionId("faction-player");
        internal static readonly FactionId Sun = new FactionId("faction-sun");
        internal static readonly CharacterId Lord = new CharacterId("char-player-lord");
        internal static readonly CharacterId Aide = new CharacterId("char-aide");
        internal static readonly CharacterId SunQuan = new CharacterId("char-sunquan");
        internal static readonly CityId Fanshui = new CityId("city-fanshui");
        internal static readonly EventId Chibi = new EventId("evt-chibi");
        internal static readonly EventId Yiling = new EventId("evt-yiling");   // 下游
        internal static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        internal static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        // 赤壁事件：前置=孙存活；窗 D0–D5；下游=夷陵。够不着→正常结局短路；够得着+前置破坏→分叉。
        internal static HistoricalEvent ChibiEvent()
            => new HistoricalEvent(
                Chibi,
                new WTimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("historical-chibi"),
                new HistoricalOutcome("sun-fell-early"),
                new[] { Yiling });

        internal static HistoricalEvent YilingEvent()
            => new HistoricalEvent(
                Yiling,
                new WTimeWindow(new WorldTime(1, DaySegment.Dawn), new WorldTime(8, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("historical-yiling"),
                new HistoricalOutcome("yiling-diverged"),
                Array.Empty<EventId>());

        internal static HistoricalEventCatalog Catalog()
            => HistoricalEventCatalog.TryCreate(new[] { ChibiEvent(), YilingEvent() }).Value!;

        internal static PlayerReach ReachTouchingSun() => new PlayerReach(new[] { Sun }, Array.Empty<CityId>());

        internal static CampaignStartConfig Config(bool sunAlive, PlayerReach? reach = null, HistoricalEventCatalog? catalog = null)
        {
            FactionRecord sun = sunAlive
                ? new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>())
                : new FactionRecord(Sun, null, SurvivalStatus.Destroyed, RelationToPlayer.Neutral, Array.Empty<CityId>());
            return new CampaignStartConfig(
                "scenario-fanshui-history", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }), sun },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                historyCatalog: catalog ?? Catalog(),
                playerReach: reach ?? PlayerReach.None,
                divergenceConfig: new DivergencePropagationConfig(2));
        }

        internal static readonly CampaignSessionService Service = new CampaignSessionService();
        internal static CampaignSession NewSession(bool sunAlive = true, PlayerReach? reach = null)
            => Service.StartCampaign(Config(sunAlive, reach)).Session!;

        // ---- AC-1: 会话持历史态 ----

        [Test]
        public void test_session_holds_history_state()
        {
            CampaignSession s = NewSession();
            Assert.That(s.HasHistory, Is.True);
        }

        [Test]
        public void test_session_without_history_config()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;
            Assert.That(s.HasHistory, Is.False);
            Assert.That(Service.AdvanceHistory(s).Count, Is.EqualTo(0), "无历史目录 → 空结果");
        }

        // ---- AC-2: 开局历史事件未触发 ----

        [Test]
        public void test_events_initially_untriggered()
        {
            CampaignSession s = NewSession();
            Assert.That(s.World.IsTriggered(Chibi.Value), Is.False);
        }
    }
}
