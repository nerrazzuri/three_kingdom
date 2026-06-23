using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// EPIC_010 竖切续：己方城市账本展示视图（BLOCKING）。治理 ADR：ADR-0002。
    /// 覆盖中文标签、多维分列（P6 不合并）、短缺/骚乱警示。
    /// </summary>
    [TestFixture]
    public class CityLedgerViewTests
    {
        private static CityLedgerProjection Proj(long stock, int morale, int sec, int fort, int fortMax, long shortage, bool unrest)
            => new CityLedgerProjection("汜水关", stock, stock, morale, sec, fort, fortMax, shortage, unrest);

        [Test]
        public void test_labels_render_chinese_and_dimensions_are_separate()
        {
            var view = new CityLedgerView(Proj(270, 70, 55, 65, 100, 0, false));

            Assert.That(view.StockLabel, Is.EqualTo("粮草 270"));
            Assert.That(view.MoraleLabel, Is.EqualTo("民心 70"));
            Assert.That(view.SecurityLabel, Is.EqualTo("治安 55"));
            Assert.That(view.FortificationLabel, Is.EqualTo("工事 65/100"));
            Assert.That(view.HasWarning, Is.False);
            Assert.That(view.WarningLabel, Is.Empty);
        }

        [Test]
        public void test_shortage_produces_warning()
        {
            var view = new CityLedgerView(Proj(80, 50, 55, 100, 100, 20, false));

            Assert.That(view.HasWarning, Is.True);
            Assert.That(view.WarningLabel, Does.Contain("粮草短缺 20"));
            Assert.That(view.WarningLabel, Does.Contain("民心受损"));
            Assert.That(view.WarningLabel, Does.Not.Contain("骚乱"));
        }

        [Test]
        public void test_high_unrest_without_shortage_produces_warning()
        {
            var view = new CityLedgerView(Proj(200, 40, 30, 80, 100, 0, true));

            Assert.That(view.HasWarning, Is.True);
            Assert.That(view.WarningLabel, Is.EqualTo("骚乱风险高"));
        }
    }
}
