using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// epic-001 story-004：SaveVersion 值对象。
    /// 治理 ADR：ADR-0005（存档版本与迁移——SaveVersion 表达 schema 兼容关系；不静默降级）。
    /// 覆盖 AC-1 解析/比较/兼容判断（含相等、主版本差、存档高于当前）、AC-2 非法版本被拒绝、不可变+值相等。
    /// </summary>
    [TestFixture]
    public class SaveVersionTests
    {
        // ---- AC-1：解析 ----

        [Test]
        public void Parse_valid_string_yields_components()
        {
            var v = SaveVersion.Parse("3.7");
            Assert.That(v.Major, Is.EqualTo(3));
            Assert.That(v.Minor, Is.EqualTo(7));
            Assert.That(v.ToString(), Is.EqualTo("3.7"));
        }

        [Test]
        public void Parse_round_trips_through_ToString()
        {
            var v = SaveVersion.Parse("12.0");
            Assert.That(SaveVersion.Parse(v.ToString()), Is.EqualTo(v));
        }

        // ---- AC-1：比较 ----

        [Test]
        public void Compare_orders_by_major_then_minor()
        {
            Assert.That(new SaveVersion(1, 9) < new SaveVersion(2, 0), Is.True);
            Assert.That(new SaveVersion(2, 1) > new SaveVersion(2, 0), Is.True);
            Assert.That(new SaveVersion(2, 0) <= new SaveVersion(2, 0), Is.True);
            Assert.That(new SaveVersion(2, 0) >= new SaveVersion(2, 0), Is.True);
            Assert.That(new SaveVersion(1, 0).CompareTo(new SaveVersion(1, 0)), Is.EqualTo(0));
        }

        // ---- AC-1：兼容判断 ----

        [Test]
        public void Classify_equal_version_is_compatible()
        {
            var current = new SaveVersion(2, 3);
            Assert.That(new SaveVersion(2, 3).ClassifyForLoad(current), Is.EqualTo(SaveCompatibility.Compatible));
        }

        [Test]
        public void Classify_same_major_lower_minor_is_migratable()
        {
            var current = new SaveVersion(2, 5);
            Assert.That(new SaveVersion(2, 1).ClassifyForLoad(current), Is.EqualTo(SaveCompatibility.Migratable));
        }

        [Test]
        public void Classify_lower_major_is_migratable()
        {
            var current = new SaveVersion(3, 0);
            Assert.That(new SaveVersion(1, 9).ClassifyForLoad(current), Is.EqualTo(SaveCompatibility.Migratable));
        }

        [Test]
        public void Classify_higher_major_is_incompatible_newer()
        {
            var current = new SaveVersion(2, 0);
            Assert.That(new SaveVersion(3, 0).ClassifyForLoad(current), Is.EqualTo(SaveCompatibility.IncompatibleNewer));
        }

        [Test]
        public void Classify_same_major_higher_minor_is_incompatible_newer()
        {
            var current = new SaveVersion(2, 2);
            Assert.That(new SaveVersion(2, 9).ClassifyForLoad(current), Is.EqualTo(SaveCompatibility.IncompatibleNewer));
        }

        [Test]
        public void CanLoadInto_rejects_only_newer_saves()
        {
            var current = new SaveVersion(2, 2);
            Assert.That(new SaveVersion(2, 2).CanLoadInto(current), Is.True);  // compatible
            Assert.That(new SaveVersion(1, 0).CanLoadInto(current), Is.True);  // migratable
            Assert.That(new SaveVersion(2, 5).CanLoadInto(current), Is.False); // newer → 拒绝
        }

        // ---- AC-2：非法版本被拒绝 ----

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("1")]
        [TestCase("1.2.3")]
        [TestCase("a.b")]
        [TestCase("1.x")]
        [TestCase("-1.0")]
        [TestCase("1.-2")]
        [TestCase("+1.0")]
        [TestCase("1. 2")]
        [TestCase(" 1.2")]
        [TestCase("1..2")]
        [TestCase("99999999999.0")] // 溢出 int
        public void Parse_illegal_string_throws_format_exception(string text)
        {
            Assert.Throws<FormatException>(() => SaveVersion.Parse(text));
        }

        [Test]
        public void Parse_null_throws_format_exception()
        {
            Assert.Throws<FormatException>(() => SaveVersion.Parse(null!));
        }

        [Test]
        public void TryParse_illegal_returns_false_without_object()
        {
            Assert.That(SaveVersion.TryParse("nope", out var v), Is.False);
            Assert.That(v, Is.EqualTo(default(SaveVersion)));
        }

        [Test]
        public void Constructor_rejects_negative_components()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaveVersion(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaveVersion(0, -1));
        }

        // ---- 不可变 + 值相等 ----

        [Test]
        public void Equality_is_by_value()
        {
            Assert.That(new SaveVersion(4, 2), Is.EqualTo(new SaveVersion(4, 2)));
            Assert.That(new SaveVersion(4, 2) == new SaveVersion(4, 2), Is.True);
            Assert.That(new SaveVersion(4, 2) != new SaveVersion(4, 3), Is.True);
            Assert.That(new SaveVersion(4, 2).GetHashCode(), Is.EqualTo(new SaveVersion(4, 2).GetHashCode()));
        }
    }
}
