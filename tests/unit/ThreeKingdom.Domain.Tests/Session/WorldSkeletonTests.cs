using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 群雄割据世界骨架（W1 内容扩充）：天下多势力多城，玩家为一方太守（远未统一）——
    /// 争霸→终局成真实战略盘（修正竖切"一开局即统一"）。争霸盘城数与世界模型城归属一致。
    /// </summary>
    [TestFixture]
    public class WorldSkeletonTests
    {
        [Test]
        public void test_world_is_a_real_multi_power_board_not_instant_unify()
        {
            ContentionState c = PlayableCampaign.Default().InitialContention();
            int rivals = 0;
            foreach (FactionId f in c.AlivePowers()) if (f != PlayableCampaign.Player) rivals++;
            Assert.That(rivals, Is.GreaterThanOrEqualTo(8), "天下群雄并起（≥8 家对手），非单一小场景。");
            Assert.That(c.TotalCities, Is.GreaterThanOrEqualTo(15), "天下多城（≥15），玩家仅据其一。");

            var svc = new EndgameService();
            Assert.That(svc.Evaluate(c, PlayableCampaign.Player, EndgameConfig.Default),
                Is.EqualTo(EndgameStatus.Ongoing), "玩家支配度低 → 开局即争霸，绝非一上来就统一。");
        }

        [Test]
        public void test_contention_matches_world_model_city_counts()
        {
            // 争霸盘每方城数应与世界模型城归属一致（数据自洽）。
            ContentionState c = PlayableCampaign.Default().InitialContention();
            Assert.That(c.CitiesOf(PlayableCampaign.Cao), Is.EqualTo(3), "曹操 3 城（许昌/濮阳/陈留）。");
            Assert.That(c.CitiesOf(PlayableCampaign.Enemy), Is.EqualTo(2), "袁术 2 城（虎牢关/寿春）。");
            Assert.That(c.CitiesOf(PlayableCampaign.Player), Is.EqualTo(1), "玩家太守 1 城（汜水关）。");
        }
    }
}
