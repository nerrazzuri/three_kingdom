using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>三维细化（GDD_027 #5）：统率/武勇/智略从战阵档/谋略档+标签确定性派生，名将差异分明——不再恒 0.6/0.7/0.8。</summary>
    [TestFixture]
    public class GeneralStatDerivationTests
    {
        private static OffensiveGeneral P(string id) => PlayableCampaign.GeneralProjection(new CharacterId(id));

        [Test]
        public void test_lubu_high_valor_low_guile_vs_kongming_inverse()
        {
            // Arrange：吕布=绝世武·愚钝谋；诸葛亮=羸弱武·经天纬地谋。
            OffensiveGeneral lubu = P("char-lubu");
            OffensiveGeneral kongming = P("char-zhugeliang");
            // Assert：武勇吕布 > 诸葛；智略诸葛 > 吕布（维度不再压平）。
            Assert.That(lubu.Valor > kongming.Valor, Is.True, "吕布武勇高于诸葛。");
            Assert.That(kongming.Guile > lubu.Guile, Is.True, "诸葛智略高于吕布。");
        }

        [Test]
        public void test_stats_are_not_flat_across_generals()
        {
            // Arrange：一勇将 vs 一谋士 vs 一守将。
            OffensiveGeneral lubu = P("char-lubu");
            OffensiveGeneral guojia = P("char-guojia");
            // Assert：至少一维明显不同（不再人人 0.6/0.7/0.8）。
            bool differ = lubu.Command != guojia.Command || lubu.Valor != guojia.Valor || lubu.Guile != guojia.Guile;
            Assert.That(differ, Is.True, "不同档武将三维应有差异。");
            Assert.That(lubu.Valor > guojia.Valor, Is.True, "猛将武勇高于谋士。");
            Assert.That(guojia.Guile > lubu.Guile, Is.True, "谋士智略高于猛将。");
        }

        [Test]
        public void test_derivation_is_deterministic()
        {
            // Act：两次投影同将。
            OffensiveGeneral a = P("char-guanyu");
            OffensiveGeneral b = P("char-guanyu");
            // Assert：确定性（同将同结果）。
            Assert.That(a.Command == b.Command && a.Valor == b.Valor && a.Guile == b.Guile, Is.True, "派生确定性。");
        }
    }
}
