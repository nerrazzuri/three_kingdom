using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 多剧本/势力选择（#1 运行期解耦）：天下大盘共享，玩家席位由 <see cref="PlayableStart"/> 指定 →
    /// 争霸/终局/出征目标随所选势力自然重定向，运行期不再硬编码单一场景。默认「汜水关太守」保持原样（既有测试为证）。
    /// </summary>
    [TestFixture]
    public class PlayableStartCatalogTests
    {
        [Test]
        public void test_catalog_offers_multiple_starts_default_is_fanshui()
        {
            Assert.That(PlayableStartCatalog.All.Count, Is.GreaterThanOrEqualTo(2), "至少两种开局（多剧本）。");
            Assert.That(PlayableStartCatalog.Default, Is.SameAs(PlayableStartCatalog.FanshuiGovernor), "缺省=汜水关太守。");
            Assert.That(PlayableStartCatalog.ById("liubei-xiaopei"), Is.SameAs(PlayableStartCatalog.LiubeiXiaopei));
            Assert.That(PlayableStartCatalog.ById("no-such"), Is.Null);
        }

        [Test]
        public void test_default_start_reproduces_fanshui_governor_board()
        {
            // 默认剧本世界大盘与原竖切一致（第 18 独立席 + 汜水关 1 城）。
            ContentionState c = PlayableCampaign.Default().InitialContention();
            Assert.That(c.CitiesOf(PlayableCampaign.Player), Is.EqualTo(1), "太守据一城。");
            Assert.That(c.CitiesOf(PlayableCampaign.Cao), Is.EqualTo(4));
            Assert.That(c.TotalCities, Is.EqualTo(37), "17 席共 37 城（含太守专属席）。");
        }

        [Test]
        public void test_playing_as_liubei_redirects_player_seat_and_drops_bespoke_seat()
        {
            PlayableCampaign camp = PlayableCampaign.ForStart(PlayableStartCatalog.LiubeiXiaopei);

            Assert.That(camp.PlayerFaction, Is.EqualTo(PlayableCampaign.LiuBei), "玩家即刘备势力。");
            Assert.That(camp.PlayerCapital, Is.EqualTo(PlayableCampaign.Xiaopei), "治所小沛。");
            Assert.That(camp.PlayerOffensiveTarget, Is.EqualTo(PlayableCampaign.Xiapi), "首锋指下邳。");
            Assert.That(camp.DefendingFactionOf(PlayableCampaign.Xiapi), Is.EqualTo(PlayableCampaign.LuBu), "下邳属吕布。");

            ContentionState c = camp.InitialContention();
            Assert.That(c.CitiesOf(PlayableCampaign.LiuBei), Is.EqualTo(1), "刘备起于小沛一城。");
            // 扮演既有诸侯 → 不含汜水关太守专属席。
            bool hasBespoke = false;
            foreach (FactionId f in c.AlivePowers()) if (f == PlayableCampaign.Player) hasBespoke = true;
            Assert.That(hasBespoke, Is.False, "扮演诸侯时无太守专属席。");
            Assert.That(c.TotalCities, Is.EqualTo(36), "去掉汜水关 → 36 城。");
        }

        [Test]
        public void test_runtime_runs_full_loop_under_alternative_start()
        {
            // 运行期以所选开局驱动：新局/推进/终局/出征目标皆键于玩家所选势力（刘备），非硬编码汜水关。
            var runtime = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.LiubeiXiaopei));
            runtime.NewGame();
            runtime.Advance(1);

            Assert.That(runtime.Scenario.PlayerFaction, Is.EqualTo(PlayableCampaign.LiuBei));
            Assert.That(runtime.Endgame(), Is.EqualTo(EndgameStatus.Ongoing), "刘备支配度低 → 开局即争霸。");
            Assert.That(runtime.Contention.CitiesOf(PlayableCampaign.LiuBei), Is.GreaterThanOrEqualTo(1),
                "争霸态键于刘备（运行期读场景身份，非静态默认）。");

            // 出征目标重定向为下邳（吕布），授权门流程照常。
            runtime.RequestOffensiveAuthorization();
            OffensiveTargetsView t = runtime.OffensiveTargets();
            Assert.That(t.Targets[0].CityId, Is.EqualTo(PlayableCampaign.Xiapi.Value));
            Assert.That(t.Authorized, Is.True);
        }

        [Test]
        public void test_scenario_choice_view_lists_all_starts_with_chinese_labels()
        {
            ScenarioChoiceView v = ScenarioChoiceView.FromCatalog();
            Assert.That(v.Choices.Count, Is.EqualTo(PlayableStartCatalog.All.Count), "选择屏列全部开局。");
            Assert.That(v.DefaultId, Is.EqualTo(PlayableStartCatalog.Default.Id));

            ScenarioChoiceLine liubei = null!;
            foreach (ScenarioChoiceLine l in v.Choices) if (l.Id == "liubei-xiaopei") liubei = l;
            Assert.That(liubei, Is.Not.Null);
            Assert.That(liubei.Name, Is.EqualTo("刘玄德·小沛"));
            Assert.That(liubei.CapitalName, Is.EqualTo("小沛"), "治所中文名经 DisplayNames。");
            Assert.That(liubei.TargetName, Is.EqualTo("下邳"), "目标城中文名经 DisplayNames。");
        }

        [Test]
        public void test_multi_anchor_starts_assemble_their_era_board()
        {
            // 每个纪元盘同 36 城骨架，归属随纪元重绘（ADR-0015 离散快照）。
            foreach (PlayableStart s in new[] { PlayableStartCatalog.CaocaoGuandu, PlayableStartCatalog.SunquanChibi, PlayableStartCatalog.LiubeiShu,
                                                PlayableStartCatalog.HejinHuangjin, PlayableStartCatalog.ZhangjiaoUprising, PlayableStartCatalog.ZhugeliangWuzhang })
            {
                ContentionState c = PlayableCampaign.ForStart(s).InitialContention();
                Assert.That(c.TotalCities, Is.EqualTo(36), $"{s.Id} 纪元盘 36 城（无太守专属席）。");
            }

            // 184 黄巾：汉庭据中原（13城），黄巾起河北（5城），二势力并存。
            ContentionState huangjin = PlayableCampaign.ForStart(PlayableStartCatalog.HejinHuangjin).InitialContention();
            Assert.That(huangjin.CitiesOf(PlayableCampaign.Han), Is.EqualTo(13), "184 汉庭据中原 13 城。");
            Assert.That(huangjin.CitiesOf(PlayableCampaign.Huangjin), Is.EqualTo(5), "184 黄巾据河北 5 城。");

            // 234 五丈原：仍是魏蜀吴三分（同 220 归属，异纪元/君主）。
            ContentionState wuzhang = PlayableCampaign.ForStart(PlayableStartCatalog.ZhugeliangWuzhang).InitialContention();
            Assert.That(wuzhang.AlivePowers().Count, Is.EqualTo(3), "234 天下三分。");
            Assert.That(wuzhang.CitiesOf(PlayableCampaign.LiuBei), Is.EqualTo(4), "季汉据益州汉中 4 城。");

            // 208 赤壁：曹操并北取荆 → 独大（≥20 城）；孙权据江东联刘抗曹。
            ContentionState chibi = PlayableCampaign.ForStart(PlayableStartCatalog.SunquanChibi).InitialContention();
            Assert.That(chibi.CitiesOf(PlayableCampaign.Cao), Is.GreaterThanOrEqualTo(20), "208 曹操独大。");
            Assert.That(chibi.CitiesOf(PlayableCampaign.Sun), Is.GreaterThanOrEqualTo(4), "208 孙权据江东。");

            // 220 三分：魏蜀吴三家分 36 城，别无他势力存续。
            ContentionState sanfen = PlayableCampaign.ForStart(PlayableStartCatalog.LiubeiShu).InitialContention();
            Assert.That(sanfen.AlivePowers().Count, Is.EqualTo(3), "220 天下三分。");
            Assert.That(sanfen.CitiesOf(PlayableCampaign.LiuBei), Is.EqualTo(4), "季汉据益州汉中 4 城。");
        }

        [Test]
        public void test_era_start_projects_its_anchor_year_and_target_defender()
        {
            // 208 孙权开局：纪元起点公元 208；目标江陵守方为曹操（据显式 TargetFaction，非 190 归属）。
            PlayableCampaign camp = PlayableCampaign.ForStart(PlayableStartCatalog.SunquanChibi);
            Assert.That(camp.DefendingFactionOf(PlayableCampaign.Jiangling), Is.EqualTo(PlayableCampaign.Cao));

            var runtime = new CampaignRuntime(new InMemorySaveMedium(), camp);
            runtime.NewGame();
            Assert.That(runtime.HudSummary().Year, Is.EqualTo(208), "纪元起点随锚点年 = 公元 208。");

            // 184 黄巾起义与 234 五丈原纪元年份亦正确投影。
            var r184 = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.ZhangjiaoUprising));
            r184.NewGame();
            Assert.That(r184.HudSummary().Year, Is.EqualTo(184), "黄巾起义 = 公元 184。");

            var r234 = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.ZhugeliangWuzhang));
            r234.NewGame();
            Assert.That(r234.HudSummary().Year, Is.EqualTo(234), "五丈原北伐 = 公元 234。");
            Assert.That(r234.Scenario.DefendingFactionOf(PlayableCampaign.Changan), Is.EqualTo(PlayableCampaign.Cao), "长安守方为魏。");
        }
    }
}
