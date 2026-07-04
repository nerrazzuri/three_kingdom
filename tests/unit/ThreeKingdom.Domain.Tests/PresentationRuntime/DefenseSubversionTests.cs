using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 敌方对玩家守城施人心杠杆（GDD_024 §13 对称威胁 B7）：守城开战时敌方种子化攻心，
    /// 成功则削弱玩家守方 + 预警（<see cref="CampaignRuntime.WasDefenseSubverted"/>）。确定性、非无解（仅削弱）。
    /// </summary>
    [TestFixture]
    public class DefenseSubversionTests
    {
        [Test]
        public void test_defense_battle_starts_with_deterministic_subversion_warning()
        {
            var a = new CampaignRuntime(new InMemorySaveMedium());
            a.NewGame();
            ZoneBattleView view = a.StartDefenseBattle();
            Assert.That(view, Is.Not.Null, "守城战照常开始（施计只削弱、不阻断）。");

            // 同场景种子 → 敌方施计结果确定性可复现。
            var b = new CampaignRuntime(new InMemorySaveMedium());
            b.NewGame();
            b.StartDefenseBattle();
            Assert.That(b.WasDefenseSubverted, Is.EqualTo(a.WasDefenseSubverted), "同种子敌方攻心结果一致（可复现，不作弊）。");
        }
    }
}
