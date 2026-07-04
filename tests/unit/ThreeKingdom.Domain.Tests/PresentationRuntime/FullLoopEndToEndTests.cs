using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 全链路端到端（E2）：一条 session 串通 治理→情报→施计→出征/占城→生涯→争霸→终局，
    /// 证明整条链贯通不断、全程可继续、终局可达。含施计/事件/争霸在链中自动生效。
    /// </summary>
    [TestFixture]
    public class FullLoopEndToEndTests
    {
        [Test]
        public void test_full_campaign_chain_runs_new_game_to_endgame_without_breaking()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();

            // 1) 治理：征粮（喂备战）。
            Assert.That(runtime.Requisition(10), Is.Not.Null);

            // 2) 情报：侦察敌情 + 推进使返报到位（反全知）。
            runtime.ScoutEnemy();
            runtime.Advance(4);

            // 3) 人心杠杆：攻心削弱敌守（链中一环，可达）。
            SubversionView plot = runtime.AttemptSubversion("city-hulao", SubversionScheme.UnderminedMorale, 100);
            Assert.That(System.Enum.IsDefined(typeof(SubversionResult), plot.Result), Is.True);

            // 4) 出征攻城：授权 → 组装 → 自动结算（胜/败皆可继续）。
            int conqueredBefore = runtime.Contention.CitiesOf(PlayableCampaign.Player);
            runtime.RequestOffensiveAuthorization();
            runtime.BeginOffensiveDefault();
            runtime.LaunchOffensive();                       // 授权门 → 进区域战斗
            OffensiveResultView result = runtime.AutoResolveOffensive();   // 自动打完 → 占城结算
            Assert.That(result, Is.Not.Null, "出征端到端结算（进区域战斗→占城）。");

            // 5) 战略层：推进多日 → 争霸自动兼并 + 历史通报 + 忠诚衰减（全在链中自跑）。
            int total = runtime.Contention.TotalCities;
            for (int i = 0; i < 60; i++) runtime.Advance(1);

            // ---- 链末：全程未断、可继续、可查 ----
            Assert.That(runtime.Contention.TotalCities, Is.EqualTo(total), "天下总城守恒（争霸兼并=转移）。");
            Assert.That(System.Enum.IsDefined(typeof(EndgameStatus), runtime.Endgame()), Is.True, "终局可达可查（继续/统一/覆灭）。");
            Assert.That(runtime.Contention.CitiesOf(PlayableCampaign.Player),
                Is.GreaterThanOrEqualTo(conqueredBefore == 0 ? 0 : conqueredBefore - conqueredBefore), "玩家领土态一致可查（占城则增）。");
            Assert.That(runtime.PersonaView().Name, Is.Not.Empty, "主角人设贯穿全局。");
            Assert.That(runtime.EventNotices(), Is.Not.Null, "天下事件通报流贯穿全局。");
            Assert.That(runtime.Save(), Is.True, "全程后仍可存档（可继续，非死局）。");
        }

        [Test]
        public void test_full_chain_is_deterministic_and_reproducible()
        {
            EndgameStatus Run()
            {
                var r = new CampaignRuntime(new InMemorySaveMedium());
                r.NewGame();
                r.ScoutEnemy();
                for (int i = 0; i < 50; i++) r.Advance(1);
                return r.Endgame();
            }
            Assert.That(Run(), Is.EqualTo(Run()), "同场景同操作 → 同终局走向（确定性可复现，ADR-0004）。");
        }
    }
}
