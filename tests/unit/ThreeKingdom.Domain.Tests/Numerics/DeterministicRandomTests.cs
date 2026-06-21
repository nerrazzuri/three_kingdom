using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Numerics
{
    /// <summary>
    /// epic-001 story-002 AC2：确定性随机流——同种子→同序列；位置可读取，(seed,position) 可重建续抽。
    /// 治理 ADR：ADR-0004（注入式确定性随机流；位置为权威状态）。
    /// </summary>
    [TestFixture]
    public class DeterministicRandomTests
    {
        [Test]
        public void Same_seed_yields_same_sequence()
        {
            var a = new DeterministicRandom(123456789UL);
            var b = new DeterministicRandom(123456789UL);
            var seqA = Enumerable.Range(0, 20).Select(_ => a.NextBits()).ToArray();
            var seqB = Enumerable.Range(0, 20).Select(_ => b.NextBits()).ToArray();
            Assert.That(seqA, Is.EqualTo(seqB));
        }

        [Test]
        public void Different_seed_yields_different_sequence()
        {
            var a = new DeterministicRandom(1UL);
            var b = new DeterministicRandom(2UL);
            Assert.That(a.NextBits(), Is.Not.EqualTo(b.NextBits()));
        }

        [Test]
        public void Position_advances_with_each_draw()
        {
            var rng = new DeterministicRandom(42UL);
            Assert.That(rng.Position, Is.EqualTo(0UL));
            rng.NextBits();
            rng.NextBits();
            Assert.That(rng.Position, Is.EqualTo(2UL));
        }

        [Test]
        public void Reconstruct_from_position_continues_identically()
        {
            const ulong seed = 0xDEADBEEFUL;
            var original = new DeterministicRandom(seed);
            for (int i = 0; i < 5; i++) original.NextBits();
            ulong savedPosition = original.Position;

            var restored = new DeterministicRandom(seed, savedPosition);

            // 续抽 10 个应与原流一致
            var contOriginal = Enumerable.Range(0, 10).Select(_ => original.NextBits()).ToArray();
            var contRestored = Enumerable.Range(0, 10).Select(_ => restored.NextBits()).ToArray();
            Assert.That(contRestored, Is.EqualTo(contOriginal));
        }

        [Test]
        public void Reconstruct_from_zero_reproduces_from_start()
        {
            const ulong seed = 777UL;
            var first = new DeterministicRandom(seed);
            var firstFive = Enumerable.Range(0, 5).Select(_ => first.NextBits()).ToArray();

            var replay = new DeterministicRandom(seed, 0UL);
            var replayFive = Enumerable.Range(0, 5).Select(_ => replay.NextBits()).ToArray();
            Assert.That(replayFive, Is.EqualTo(firstFive));
        }

        [Test]
        public void NextUnit_is_within_zero_inclusive_one_exclusive()
        {
            var rng = new DeterministicRandom(2024UL);
            for (int i = 0; i < 1000; i++)
            {
                var u = rng.NextUnit();
                Assert.That(u, Is.GreaterThanOrEqualTo(FixedPoint.Zero));
                Assert.That(u, Is.LessThan(FixedPoint.One));
            }
        }

        [Test]
        public void NextInt_stays_in_range_and_is_deterministic()
        {
            var a = new DeterministicRandom(555UL);
            var b = new DeterministicRandom(555UL);
            for (int i = 0; i < 500; i++)
            {
                int va = a.NextInt(10, 20);
                int vb = b.NextInt(10, 20);
                Assert.That(va, Is.InRange(10, 19));
                Assert.That(va, Is.EqualTo(vb)); // 确定性
            }
        }

        [Test]
        public void NextInt_rejects_empty_range()
        {
            var rng = new DeterministicRandom(1UL);
            Assert.Throws<ArgumentException>(() => rng.NextInt(5, 5));
            Assert.Throws<ArgumentException>(() => rng.NextInt(5, 4));
        }
    }
}
