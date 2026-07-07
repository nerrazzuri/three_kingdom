using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>外部档案加载器（GDD_027 #5）：export→load 对象级 round-trip 重建等价全谱（外部文件可作运行期权威源）。</summary>
    [TestFixture]
    public class GeneralDossierLoaderTests
    {
        [Test]
        public void test_load_from_exported_data_reproduces_authoritative_roster()
        {
            // Act：权威源 → 导出 → 加载回运行期对象。
            string tkdata = GeneralDossierCodec.Export();
            IReadOnlyList<GeneralDossier> loaded = GeneralDossierLoader.LoadRoster(tkdata);

            // Assert：对象级与硬编码全谱逐字段等价。
            Assert.That(loaded.Count, Is.EqualTo(GeneralDossiers.All.Count), "加载全部档案。");
            foreach (GeneralDossier d in loaded)
            {
                GeneralDossier? auth = GeneralDossiers.Find(d.Id);
                Assert.That(auth, Is.Not.Null, $"{d.Id.Value} 应在权威档案。");
                Assert.That(d.Prowess, Is.EqualTo(auth!.Prowess), $"{d.Id.Value} 战阵档。");
                Assert.That(d.Strategy, Is.EqualTo(auth.Strategy), $"{d.Id.Value} 谋略档。");
                Assert.That(d.Leaning, Is.EqualTo(auth.Leaning), $"{d.Id.Value} 忠义。");
                Assert.That(d.Ambition, Is.EqualTo(auth.Ambition), $"{d.Id.Value} 野心。");
                Assert.That(d.Stage, Is.EqualTo(auth.Stage), $"{d.Id.Value} 纪元段。");
                var expected = new HashSet<GeneralTag>(auth.Tags);
                var actual = new HashSet<GeneralTag>(d.Tags);
                Assert.That(actual.SetEquals(expected), Is.True, $"{d.Id.Value} 标签集。");
            }
        }

        [Test]
        public void test_load_life_years_matches_authoritative()
        {
            var life = GeneralDossierLoader.LoadLifeYears(GeneralDossierCodec.Export());
            // 抽验若干具名武将生卒与权威一致。
            foreach (string id in new[] { "char-guanyu", "char-zhugeliang", "char-lubu", "char-huaxiong" })
            {
                (int Birth, int Death)? auth = GeneralDossiers.LifeOf(new CharacterId(id));
                if (auth == null) continue;
                Assert.That(life.ContainsKey(id), Is.True, $"{id} 生卒应被加载。");
                Assert.That(life[id].Birth, Is.EqualTo(auth.Value.Birth), $"{id} 生年。");
                Assert.That(life[id].Death, Is.EqualTo(auth.Value.Death), $"{id} 卒年。");
            }
        }
    }
}
