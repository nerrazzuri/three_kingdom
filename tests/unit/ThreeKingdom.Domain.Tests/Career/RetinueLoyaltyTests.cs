using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// 忠诚经营（GDD_014）：赏赐升忠诚 · 久疏衰减（不越下限）· 忠者不可挖 · 低忠诚可被挖角（种子化）·
    /// 挖角带走其官职 · 确定性。挖角与 GDD_024 人心杠杆对玩家守将策反对称。
    /// </summary>
    [TestFixture]
    public class RetinueLoyaltyTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CharacterId Warden = new CharacterId("char-warden");
        private readonly RetinueLoyaltyService _svc = new RetinueLoyaltyService();

        private static RetinueState Retinue(int aideAff, int wardenAff)
            => new RetinueState(
                new[] { new RetinueMember(Aide, F(aideAff, 10)), new RetinueMember(Warden, F(wardenAff, 10)) },
                new[] { new KeyValuePair<OfficeRole, CharacterId>(OfficeRole.CityWarden, Warden) });

        private static FixedPoint Aff(RetinueState s, CharacterId c)
        {
            foreach (RetinueMember m in s.Members) if (m.Character == c) return m.Affinity;
            return FixedPoint.FromInt(-1);
        }

        [Test]
        public void test_reward_raises_affinity()
        {
            RetinueState after = _svc.Reward(Retinue(3, 5), Aide, FixedPoint.One, RetinueLoyaltyConfig.Default);
            Assert.That(Aff(after, Aide).Raw, Is.GreaterThan(F(3, 10).Raw), "赏赐升忠诚。");
        }

        [Test]
        public void test_reward_non_member_is_unchanged()
        {
            RetinueState after = _svc.Reward(Retinue(3, 5), new CharacterId("char-stranger"), FixedPoint.One, RetinueLoyaltyConfig.Default);
            Assert.That(Aff(after, Aide).Raw, Is.EqualTo(F(3, 10).Raw), "非成员赏赐 → 原成员好感不变。");
            Assert.That(Aff(after, Warden).Raw, Is.EqualTo(F(5, 10).Raw), "非成员赏赐 → 其余不变。");
        }

        [Test]
        public void test_decay_lowers_affinity_but_not_below_floor()
        {
            var cfg = RetinueLoyaltyConfig.Default;   // decay 0.03, floor 0.1
            RetinueState once = _svc.Decay(Retinue(5, 1), cfg);
            Assert.That(Aff(once, Aide).Raw, Is.LessThan(F(5, 10).Raw), "久疏 → 忠诚衰减。");
            Assert.That(Aff(once, Warden).Raw, Is.EqualTo(cfg.LoyaltyFloor.Raw), "已在下限（0.1）者不再降。");
        }

        [Test]
        public void test_loyal_member_is_immune_to_poaching()
        {
            // 好感 0.7 ≥ 阈值 0.4 → 不可挖，任何种子都不叛。
            PoachResult r = _svc.AttemptPoach(Retinue(7, 5), Aide, FixedPoint.One, 12345UL, RetinueLoyaltyConfig.Default);
            Assert.That(r.Left, Is.False, "忠者不叛。");
            Assert.That(r.Chance.Raw, Is.EqualTo(0), "忠者被挖角概率为 0。");
        }

        [Test]
        public void test_low_loyalty_member_can_be_poached_and_loses_office()
        {
            // Warden 好感 0.1（<0.4）且持 CityWarden 官职；强拉拢 → 高概率叛离。
            // 用必成配置（base=1）确保确定性：任何 roll<1 → 叛离。
            var sure = new RetinueLoyaltyConfig(F(2, 10), F(3, 100), F(1, 10),
                poachThreshold: F(4, 10), poachBase: FixedPoint.One, poachPullWeight: F(4, 10), poachVulnerabilityWeight: F(6, 10));
            PoachResult r = _svc.AttemptPoach(Retinue(7, 1), Warden, FixedPoint.One, 1UL, sure);
            Assert.That(r.Left, Is.True, "低忠诚 + 强拉拢 → 叛离。");
            Assert.That(r.State.IsMember(Warden), Is.False, "叛离者移出部曲。");
            Assert.That(r.State.Holder(OfficeRole.CityWarden), Is.Null, "叛离带走其官职（职位空缺）。");
        }

        [Test]
        public void test_poach_is_deterministic()
        {
            var s = Retinue(7, 2);
            PoachResult a = _svc.AttemptPoach(s, Warden, F(5, 10), 99UL, RetinueLoyaltyConfig.Default);
            PoachResult b = _svc.AttemptPoach(s, Warden, F(5, 10), 99UL, RetinueLoyaltyConfig.Default);
            Assert.That(b.Left, Is.EqualTo(a.Left));
            Assert.That(b.Chance.Raw, Is.EqualTo(a.Chance.Raw), "同种子挖角结果一致。");
        }
    }
}
