using System;
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
    /// epic-013 story-006：新旧会话共存 + 共享服务（Integration / Assembly）。
    /// 治理 ADR：ADR-0002（四层）+ ADR-0009（GameSession 保留为 slice fixture、新建 CampaignSession）。
    /// <para>
    /// <b>工程裁定（YAGNI，2026-06-28）</b>：S1-S5 的 CampaignSession 用生涯+世界（各自存档），与竖切 GameSession
    /// 的 RngStreamState/SaveMapper/情报捕获<b>尚无实质重叠</b>——共享服务抽取属过早抽象。本 story 验证：
    /// ① 两会话独立共存、互不干扰；② 竖切回归不破（slice 全套测试仍绿）。<b>实质共享服务（RngStreamState/SaveMapper
    /// 捕获）延后到 CampaignSession 纳入竖切 RNG/情报状态时（M03+）再抽取</b>，届时才有真正可复用件。
    /// </para>
    /// </summary>
    [TestFixture]
    public class SessionCoexistenceTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig CampaignConfig()
            => new CampaignStartConfig(
                "scenario-fanshui-siege", new ConfigFingerprint(0xCA11AB1EUL),
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) });

        [Test]
        public void test_slice_and_campaign_sessions_coexist_independently()
        {
            // 竖切 GameSession（slice fixture，保留）。
            var sliceService = new SessionService();
            GameSession slice = sliceService.NewGame();
            WorldStatusProjection sliceView0 = sliceService.Project(slice);

            // 新建 CampaignSession（装配脊梁）。
            var campaignService = new CampaignSessionService();
            CampaignSession campaign = campaignService.StartCampaign(CampaignConfig()).Session!;

            // 推进 CampaignSession 不影响竖切 GameSession（独立状态）。
            campaignService.Advance(campaign, 5);
            WorldStatusProjection sliceView1 = sliceService.Project(slice);

            Assert.That(slice, Is.Not.Null);
            Assert.That(campaign, Is.Not.Null);
            Assert.That(sliceView1.AbsoluteIndex, Is.EqualTo(sliceView0.AbsoluteIndex), "Campaign 推进不应改竖切时间。");
            Assert.That(campaign.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn).Advance(5)));
        }

        [Test]
        public void test_slice_session_still_advances_after_campaign_exists()
        {
            // 竖切回归未破：CampaignSession 存在时，竖切自身推进仍正常。
            var sliceService = new SessionService();
            GameSession slice = sliceService.NewGame();
            _ = new CampaignSessionService().StartCampaign(CampaignConfig());

            WorldStatusProjection before = sliceService.Project(slice);
            WorldStatusProjection after = sliceService.Advance(slice, 1);
            Assert.That(after.AbsoluteIndex, Is.GreaterThan(before.AbsoluteIndex));
        }
    }
}
