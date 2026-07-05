using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Characters
{
    /// <summary>
    /// 羁绊系统（GDD_025 R4 / T4）：同场协同（血脉/师徒/知己→士气升）/互扣（仇怨→士气损）。确定性纯函数。
    /// </summary>
    [TestFixture]
    public class BondTests
    {
        private static readonly BondEffectService Svc = new BondEffectService();
        private static CharacterId C(string id) => new CharacterId(id);

        [Test]
        public void test_no_bond_is_neutral()
        {
            var present = new List<CharacterId> { C("char-x"), C("char-y") };
            Assert.That(Svc.SideBondMorale(present, GeneralBonds.Among(present), BondConfig.Default),
                Is.EqualTo(FixedPoint.One), "互不相干 → 中性 1.0。");
            Assert.That(Svc.SideBondMorale(new List<CharacterId> { C("char-guanyu") }, GeneralBonds.All, BondConfig.Default),
                Is.EqualTo(FixedPoint.One), "独一人 → 无同场羁绊。");
        }

        [Test]
        public void test_kindred_same_field_synergizes()
        {
            // 刘关张同场（三对知己）→ 士气协同上升。
            var present = new List<CharacterId> { C("char-liubei"), C("char-guanyu"), C("char-zhangfei") };
            var bonds = GeneralBonds.Among(present);
            Assert.That(bonds.Count, Is.EqualTo(3), "刘关张三对知己。");
            FixedPoint mul = Svc.SideBondMorale(present, bonds, BondConfig.Default);
            Assert.That(mul, Is.GreaterThan(FixedPoint.One), "并肩之将协同 → 士气升。");
        }

        [Test]
        public void test_feud_same_field_penalizes()
        {
            // 关羽与吕蒙同场（宿敌）→ 士气互扣。
            var present = new List<CharacterId> { C("char-guanyu"), C("char-lvmeng") };
            FixedPoint mul = Svc.SideBondMorale(present, GeneralBonds.Among(present), BondConfig.Default);
            Assert.That(mul, Is.LessThan(FixedPoint.One), "仇怨同场貌合神离 → 士气损。");
        }

        [Test]
        public void test_synergy_is_capped()
        {
            // 大量协同羁绊叠加仍封顶，不失衡。
            var present = new List<CharacterId>
            {
                C("char-caocao"), C("char-xiahoudun"), C("char-caoren"), C("char-caohong"),
                C("char-xiahouyuan"), C("char-caopi"), C("char-caozhi"),
            };
            FixedPoint mul = Svc.SideBondMorale(present, GeneralBonds.Among(present), BondConfig.Default);
            Assert.That(mul, Is.LessThanOrEqualTo(FixedPoint.One + BondConfig.Default.Cap), "封顶 ±Cap。");
            Assert.That(mul, Is.GreaterThan(FixedPoint.One));
        }

        [Test]
        public void test_among_filters_to_both_present()
        {
            // 只在场一端 → 不计。
            var present = new List<CharacterId> { C("char-liubei"), C("char-caocao") };
            Assert.That(GeneralBonds.Among(present).Count, Is.EqualTo(0), "刘备与曹操之间无登记羁绊。");
        }
    }
}
