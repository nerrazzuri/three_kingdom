using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// UI 层接入投影：聚合顶栏视图（GameHudView，UI 单一绑定）+ 反全知人才录（TalentRecruitView，无数值）。
    /// </summary>
    [TestFixture]
    public class GameHudViewTests
    {
        [Test]
        public void test_hud_summary_aggregates_top_line_state()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            GameHudView h = rt.HudSummary();

            Assert.That(h.Year, Is.EqualTo(190));
            Assert.That(h.SeasonLabel, Is.EqualTo("春"));
            Assert.That(h.Age, Is.EqualTo(20));
            Assert.That(h.LifePhaseLabel, Is.EqualTo("春秋鼎盛"));
            Assert.That(h.RankTitle, Is.EqualTo("太守"));
            Assert.That(h.ActionCapacity, Is.EqualTo(2), "太守手令 2。");
            Assert.That(h.ActionsInFlight, Is.EqualTo(0));
            Assert.That(h.PlayerCities, Is.EqualTo(1));
            Assert.That(h.AliveRivals, Is.GreaterThanOrEqualTo(8), "群雄并起。");
            Assert.That(h.IsEliminated, Is.False);
            Assert.That(h.MissionOrder, Does.Contain("君命"), "顶栏含当前君命。");
        }

        [Test]
        public void test_talent_view_is_anti_omniscient_no_numbers()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            // 探知一名人才 → 进入可见录。
            rt.RevealTalent(new ThreeKingdom.Domain.Talent.TalentId("talent-xiaojiang"), ThreeKingdom.Domain.Talent.TalentChannel.Scouting);
            // 骁将开局即登场（appearFrom 0）。
            TalentRecruitView v = rt.TalentView();
            Assert.That(v.Talents.Count, Is.GreaterThanOrEqualTo(1), "已知晓且登场者入录。");

            TalentRecruitLine line = null!;
            foreach (TalentRecruitLine l in v.Talents) if (l.TalentId == "talent-xiaojiang") line = l;
            Assert.That(line, Is.Not.Null);
            Assert.That(line.SpecialtyLabel, Is.EqualTo("善攻坚"), "只呈专长文字。");
            Assert.That(new[] { "易招", "尚可", "难招" }, Does.Contain(line.DifficultyLabel), "招揽难度为定性档，非数字。");
        }

        [Test]
        public void test_diplomacy_view_lists_faction_stances()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            DiplomacyView d = rt.DiplomacyView();
            Assert.That(d.Factions.Count, Is.GreaterThanOrEqualTo(8), "对每一存续势力列立场。");

            DiplomacyLine cao = null!;
            foreach (DiplomacyLine l in d.Factions) if (l.FactionId == "faction-cao") cao = l;
            Assert.That(cao, Is.Not.Null);
            Assert.That(cao.FactionName, Is.EqualTo("曹操"));
            Assert.That(new[] { "敌对", "中立", "互不侵犯", "盟约" }, Does.Contain(cao.StanceLabel));
            Assert.That(cao.StanceLabel, Is.EqualTo("中立"), "开局默认中立。");
            Assert.That(cao.CanAttackFreely, Is.True, "中立可径攻（无约）。");
        }
    }
}
