using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>招揽统一·运行期权威路径（GDD_027 #2）：反全知知晓门 + 招揽入伙写麾下 → 可任用。</summary>
    [TestFixture]
    public class RuntimeRecruitmentTests
    {
        private static CharacterId C(string id) => new CharacterId(id);

        [Test]
        public void test_discover_recruit_then_appointable()
        {
            // Arrange
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            var sage = C("char-simahui");   // 在野名士

            // 未发觉 → 不在已知池（反全知）。
            Assert.That(rt.KnownTalents().Count, Is.EqualTo(0), "未发觉 → 招揽列表空。");

            // Act：发觉（访贤=接触）→ 招揽（厚待易招者）。
            rt.DiscoverTalent(sage, RecruitChannel.Visit);
            Assert.That(rt.KnownTalents().Count, Is.GreaterThan(0), "发觉后入已知池。");
            RecruitAttemptResult r = rt.RecruitGeneral(sage, offerTier: 9, seed: 777UL);
            Assert.That(r.Outcome, Is.EqualTo(RecruitOutcome.Joined), "厚待招易招者 → 入伙。");

            // Assert：入伙 → 归麾下人生态 + 可任用（此前 recruited 者会被"非你麾下"拒）。
            Assert.That(rt.Generals.Get(sage), Is.Not.Null, "入伙 → 人生态铸入麾下。");
            AppointGate gate = rt.AppointGeneral(new CityId("city-xiaopei"), sage);
            Assert.That(gate, Is.EqualTo(AppointGate.Ok), "已招揽入伙者可任用。");
        }
    }
}
