using NUnit.Framework;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Numerics
{
    /// <summary>
    /// epic-001 story-002 AC3：状态哈希稳定（同输入→同哈希）、顺序敏感、任一字段变更→哈希变化。
    /// 治理 ADR：ADR-0004（状态哈希为严格相等判定，回放校验基础）。
    /// </summary>
    [TestFixture]
    public class StateHasherTests
    {
        [Test]
        public void Same_input_yields_same_hash()
        {
            var h1 = new StateHasher().Append(1).Append(2L).Append(FixedPoint.FromFraction(1, 2)).ToHash();
            var h2 = new StateHasher().Append(1).Append(2L).Append(FixedPoint.FromFraction(1, 2)).ToHash();
            Assert.That(h1, Is.EqualTo(h2));
        }

        [Test]
        public void Order_is_significant()
        {
            var ab = new StateHasher().Append(1).Append(2).ToHash();
            var ba = new StateHasher().Append(2).Append(1).ToHash();
            Assert.That(ab, Is.Not.EqualTo(ba));
        }

        [Test]
        public void Changing_any_field_changes_hash()
        {
            var baseHash = new StateHasher().Append(100).Append(200).ToHash();
            var changed = new StateHasher().Append(100).Append(201).ToHash();
            Assert.That(changed, Is.Not.EqualTo(baseHash));
        }

        [Test]
        public void FixedPoint_hashes_by_its_raw_value()
        {
            var viaFixed = new StateHasher().Append(FixedPoint.FromRaw(12345)).ToHash();
            var viaRaw = new StateHasher().Append(12345).ToHash();
            Assert.That(viaFixed, Is.EqualTo(viaRaw));
        }

        [Test]
        public void Empty_hash_is_fnv_offset_basis()
        {
            Assert.That(new StateHasher().ToHash().Value, Is.EqualTo(1469598103934665603UL));
        }

        [Test]
        public void Hash_renders_as_hex()
        {
            var hash = new StateHash(0xABCDEF0123456789UL);
            Assert.That(hash.ToString(), Is.EqualTo("0xABCDEF0123456789"));
        }
    }
}
