using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Defeat;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 东山再起·活世界续局（GDD_026 补）：势力被灭→归顺/投奔成功→<b>同一世界、当前公元年、这一生</b>里
    /// 由新主割一城复位为太守，覆灭解除、天下照旧流转。世界/生涯/一生皆不重置。
    /// </summary>
    [TestFixture]
    public class DefeatContinuationTests
    {
        private static readonly CampaignSessionService Service = new CampaignSessionService();

        [Test]
        public void test_continue_rejected_when_not_eliminated()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            Assert.That(runtime.IsPlayerEliminated, Is.False);
            Assert.That(runtime.ContinueUnderNewLord(), Is.False, "未覆灭不进续局。");
        }

        [Test]
        public void test_submit_then_reseat_revives_in_same_world_and_year()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            runtime.AdvanceYear();                     // 到 191，以证年份延续
            int yearBefore = runtime.CurrentYear;
            int lifeAgeBefore = runtime.LifeView().Age;

            // 令玩家在争霸态覆灭（曹操吞其末城）。
            Service.RecordPlayerConquest(runtime.Session, runtime.Contention, PlayableCampaign.Cao, PlayableCampaign.Player);
            Assert.That(runtime.IsPlayerEliminated, Is.True, "领城归零 → 覆灭。");

            // 被俘 → 归顺（自被俘阶段直接归顺擒获者）。
            DefeatFlow flow = runtime.BeginDefeat();
            flow.Submit();
            Assert.That(flow.CanPlayOn, Is.True, "归顺 → 可复位。");

            bool revived = runtime.ContinueUnderNewLord();
            Assert.That(revived, Is.True, "在活世界里复位为新主太守。");
            Assert.That(runtime.IsPlayerEliminated, Is.False, "复位后覆灭解除。");
            Assert.That(runtime.Contention.CitiesOf(PlayableCampaign.Player), Is.EqualTo(1), "复得一城。");
            Assert.That(runtime.CurrentYear, Is.EqualTo(yearBefore), "同世界·当前公元年延续（非重置回锚点）。");
            Assert.That(runtime.LifeView().Age, Is.EqualTo(lifeAgeBefore), "同一人·这一生接续（年龄不重置）。");
            Assert.That(runtime.CurrentSuzerain, Is.EqualTo(PlayableCampaign.Cao), "归顺即事擒获者曹操。");

            // 复位后世界照旧可推进（东山再起，续玩）。
            runtime.AdvanceYear();
            Assert.That(runtime.CurrentYear, Is.EqualTo(yearBefore + 1), "续局后时间照旧流转。");
        }

        [Test]
        public void test_reseat_grants_a_city_owned_by_the_new_lord()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            Service.RecordPlayerConquest(runtime.Session, runtime.Contention, PlayableCampaign.Cao, PlayableCampaign.Player);

            runtime.BeginDefeat().Submit();
            Assert.That(runtime.ContinueUnderNewLord(), Is.True);

            // 复位所得之城现属玩家，且原为曹操治下（新主割让）。
            int playerCities = 0;
            foreach (PowerStanding p in runtime.Contention.Powers)
                if (p.Faction == PlayableCampaign.Player) playerCities = p.Cities;
            Assert.That(playerCities, Is.EqualTo(1));
            // 曹操被割一城（原 4，先吞玩家 +1=5，割让 −1=4）。
            Assert.That(runtime.Contention.CitiesOf(PlayableCampaign.Cao), Is.EqualTo(4), "新主割让一城予流亡太守。");
        }
    }
}
