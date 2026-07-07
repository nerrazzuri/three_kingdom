using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>武将档案外部化（GDD_027 #5）：导出→解析 round-trip 无损捕获全谱（外部文件由权威源生成，零转写误差）。</summary>
    [TestFixture]
    public class GeneralDossierCodecTests
    {
        [Test]
        public void test_export_parse_round_trip_matches_authoritative_roster()
        {
            // Act：从权威硬编码导出 → 解析回记录。
            string text = GeneralDossierCodec.Export();
            IReadOnlyList<GeneralDossierRecord> recs = GeneralDossierCodec.Parse(text);

            // Assert：数量一致。
            Assert.That(recs.Count, Is.EqualTo(GeneralDossiers.All.Count), "外部化捕获全部档案。");

            // 逐条比对每个字段与权威源等价（证零转写误差）。
            foreach (GeneralDossierRecord r in recs)
            {
                GeneralDossier? d = GeneralDossiers.Find(new CharacterId(r.Id));
                Assert.That(d, Is.Not.Null, $"{r.Id} 应在权威档案中。");
                Assert.That(r.Prowess, Is.EqualTo(d!.Prowess), $"{r.Id} 战阵档。");
                Assert.That(r.Strategy, Is.EqualTo(d.Strategy), $"{r.Id} 谋略档。");
                Assert.That(r.Leaning, Is.EqualTo(d.Leaning), $"{r.Id} 忠义倾向。");
                Assert.That(r.Ambition, Is.EqualTo(d.Ambition), $"{r.Id} 野心。");
                Assert.That(r.Stage, Is.EqualTo(d.Stage), $"{r.Id} 纪元段。");

                (int Birth, int Death)? life = GeneralDossiers.LifeOf(d.Id);
                Assert.That(r.Birth, Is.EqualTo(life?.Birth), $"{r.Id} 生年。");
                Assert.That(r.Death, Is.EqualTo(life?.Death), $"{r.Id} 卒年。");

                var expected = new HashSet<GeneralTag>(d.Tags);
                var actual = new HashSet<GeneralTag>(r.Tags);
                Assert.That(actual.SetEquals(expected), Is.True, $"{r.Id} 标签集一致。");
            }
        }

        [Test]
        public void test_export_is_deterministic()
        {
            Assert.That(GeneralDossierCodec.Export(), Is.EqualTo(GeneralDossierCodec.Export()), "导出确定性（按 id 稳定序）。");
        }
    }
}
