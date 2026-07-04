using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 武将标签体系（GDD_025）：无数值面板——武将由气质标签 + 隐秘心（忠诚倾向/野心）定义；
    /// 隐秘心确定性驱动人心杠杆，"离间忠义者难、离间怀贰者易"由标签而非数字决定。
    /// </summary>
    [TestFixture]
    public class GeneralDossierTests
    {
        private static readonly CharacterId GuanYu = new CharacterId("char-guanyu");   // 忠义 + 傲骨
        private static readonly CharacterId LuBu = new CharacterId("char-lubu");        // 怀贰 + 反复

        [Test]
        public void test_dossier_defines_general_by_tags_not_numbers()
        {
            GeneralDossier? guan = GeneralDossiers.Find(GuanYu);
            Assert.That(guan, Is.Not.Null, "名将有档案。");
            Assert.That(guan!.HasTag(GeneralTag.Awe), Is.True, "关羽【威压】。");
            Assert.That(guan.HasTag(GeneralTag.IronBones), Is.True, "关羽【傲骨】。");
            Assert.That(guan.Leaning, Is.EqualTo(LoyaltyLeaning.Loyal), "关羽忠义（隐秘心，定性档）。");
        }

        [Test]
        public void test_loyal_general_is_far_harder_to_subvert_than_disloyal()
        {
            var svc = new SubversionService();
            var cfg = SubversionConfig.Default;
            // 同等情报/强度下，比较策反成功度。
            SubversionTargetProfile guan = SubversionTargetProfileFactory.Build(GuanYu, true, FixedPoint.FromFraction(8, 10), false, 1UL);
            SubversionTargetProfile lubu = SubversionTargetProfileFactory.Build(LuBu, true, FixedPoint.FromFraction(8, 10), false, 1UL);

            // 策反门：吕布（怀贰+反复）成型；关羽（忠义）门不齐 → 无效。
            SubversionOutcome vsGuan = svc.Resolve(SubversionScheme.InciteDefection, guan, FixedPoint.One, 0, 3UL, cfg);
            SubversionOutcome vsLubu = svc.Resolve(SubversionScheme.InciteDefection, lubu, FixedPoint.One, 0, 3UL, cfg);

            Assert.That(vsGuan.Result, Is.EqualTo(SubversionResult.Ineffective), "关羽忠义 → 策反门不成型（几乎不可策反）。");
            Assert.That(vsLubu.Chance.Raw, Is.GreaterThan(vsGuan.Chance.Raw), "吕布怀贰 → 策反可乘度远高于关羽（标签驱动，非数字）。");
        }

        [Test]
        public void test_benevolent_general_resists_rumor_better()
        {
            // 仁德者魅力高 → 攻心（UnderminedMorale）抵抗更强，成功度低于寻常。
            var svc = new SubversionService();
            SubversionTargetProfile liubei = SubversionTargetProfileFactory.Build(new CharacterId("char-liubei"), true, FixedPoint.FromFraction(8, 10), false, 1UL);
            SubversionTargetProfile lubu = SubversionTargetProfileFactory.Build(LuBu, true, FixedPoint.FromFraction(8, 10), false, 1UL);
            FixedPoint vsLiubei = svc.Resolve(SubversionScheme.UnderminedMorale, liubei, FixedPoint.One, 0, 5UL, SubversionConfig.Default).Chance;
            FixedPoint vsLubu = svc.Resolve(SubversionScheme.UnderminedMorale, lubu, FixedPoint.One, 0, 5UL, SubversionConfig.Default).Chance;
            Assert.That(vsLiubei.Raw, Is.LessThan(vsLubu.Raw), "仁德者（刘备）攻心更难得手（魅力抵抗，标签驱动）。");
        }
    }
}
