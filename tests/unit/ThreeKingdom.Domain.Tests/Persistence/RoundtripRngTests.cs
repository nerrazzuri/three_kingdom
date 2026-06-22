using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// epic-009 story-002：Round-trip 一致性与随机流位置保存。
    /// 治理 ADR：ADR-0005（load(save(s))≡s）+ ADR-0004（随机流位置 + 状态哈希）。
    /// 覆盖 AC-1 round-trip 状态哈希一致（含在途行动/空集合）、AC-2 随机流位置保持（读档续抽与未存档继续一致，
    /// 不重抽已发生结果）、TR-time-003 事件序一致。
    /// </summary>
    [TestFixture]
    public class RoundtripRngTests
    {
        private static readonly ISaveSerializer Serializer = new CanonicalSaveSerializer();
        private const string Slot = "campaign";

        private static SaveSnapshot SnapshotWith(IEnumerable<RngStreamState> streams, Dictionary<string, long> truth)
            => new SaveSnapshot(new SaveVersion(1, 0), new ConfigFingerprint(0x1122334455667788UL),
                streams, truth, new Dictionary<string, long> { ["enemy.estimate"] = 1000 });

        private static SaveSnapshot SaveAndLoad(SaveSnapshot s)
        {
            var repo = new SaveRepository(new InMemorySaveMedium(), Serializer);
            Assert.That(repo.Save(Slot, s).Succeeded, Is.True);
            return repo.ReadRaw(Slot)!;
        }

        // ---- AC-1: round-trip 状态哈希一致 ----

        [Test]
        public void test_load_of_save_equals_original_state_hash()
        {
            var s = SnapshotWith(
                new[] { RngStreamState.Capture("battle", new DeterministicRandom(7, 3)), RngStreamState.Capture("weather", new DeterministicRandom(9, 11)) },
                new Dictionary<string, long> { ["troops"] = 3000, ["enemy.actual"] = 1200 });

            var loaded = SaveAndLoad(s);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(s.ComputeHash()), "load(save(s)) 状态哈希 ≡ s。");
        }

        [Test]
        public void test_inflight_action_and_empty_collections_survive_roundtrip()
        {
            // 在途外援/在途行动以权威字段建模；空随机流集合为边界。
            var s = SnapshotWith(
                new RngStreamState[0],
                new Dictionary<string, long> { ["inflight.relief.eta_segment"] = 14, ["inflight.supply.amount"] = 800 });

            var loaded = SaveAndLoad(s);

            Assert.That(loaded.RngStreams.Count, Is.EqualTo(0));
            Assert.That(loaded.WorldTruth["inflight.relief.eta_segment"], Is.EqualTo(14), "在途外援 round-trip 后存活。");
            Assert.That(loaded.ComputeHash(), Is.EqualTo(s.ComputeHash()));
        }

        // ---- AC-2: 随机流位置保持，读档续抽与未存档继续一致（不重抽已发生结果）----

        [Test]
        public void test_rng_position_preserved_continued_draws_match_unsaved()
        {
            const ulong seed = 0xC0FFEEUL;
            var live = new DeterministicRandom(seed);
            for (int i = 0; i < 5; i++) live.NextBits(); // 已发生 5 次抽取

            var snapshot = SnapshotWith(
                new[] { RngStreamState.Capture("main", live) },
                new Dictionary<string, long> { ["k"] = 1 });

            var loaded = SaveAndLoad(snapshot);
            var rebuilt = loaded.RngStreams[0].Rebuild(); // 读档重建（position=5）

            // 读档续抽 5 次 与 未存档直接继续 5 次 必须完全一致——读档不重抽已发生结果。
            for (int i = 0; i < 5; i++)
                Assert.That(rebuilt.NextBits(), Is.EqualTo(live.NextBits()), $"第 {i} 次续抽不一致。");

            Assert.That(rebuilt.Position, Is.EqualTo(live.Position), "续抽后位置一致。");
        }

        // ---- TR-time-003: round-trip 后行动耗时/事件序列一致 ----

        [Test]
        public void test_event_sequence_is_identical_after_roundtrip()
        {
            const ulong seed = 0xABCUL;
            var live = new DeterministicRandom(seed);
            // 模拟已推进若干段、产生若干「行动耗时」事件。
            var preRoll = new List<int>();
            for (int i = 0; i < 8; i++) preRoll.Add(live.NextInt(1, 6));

            var snapshot = SnapshotWith(new[] { RngStreamState.Capture("timeline", live) },
                new Dictionary<string, long> { ["segment"] = 8 });

            var rebuilt = SaveAndLoad(snapshot).RngStreams[0].Rebuild();

            // 后续事件序列：未存档继续 vs 读档继续 必须逐项一致。
            for (int i = 0; i < 10; i++)
                Assert.That(rebuilt.NextInt(1, 6), Is.EqualTo(live.NextInt(1, 6)), $"事件 {i} 序列分叉。");
        }

        // ---- 多流：各流独立保持各自位置 ----

        [Test]
        public void test_multiple_streams_each_keep_own_position()
        {
            var a = new DeterministicRandom(1); for (int i = 0; i < 3; i++) a.NextBits();
            var b = new DeterministicRandom(2); for (int i = 0; i < 7; i++) b.NextBits();

            var loaded = SaveAndLoad(SnapshotWith(
                new[] { RngStreamState.Capture("a", a), RngStreamState.Capture("b", b) },
                new Dictionary<string, long> { ["k"] = 0 }));

            RngStreamState la = loaded.RngStreams[0].Name == "a" ? loaded.RngStreams[0] : loaded.RngStreams[1];
            RngStreamState lb = loaded.RngStreams[0].Name == "b" ? loaded.RngStreams[0] : loaded.RngStreams[1];
            Assert.That(la.Position, Is.EqualTo(3UL));
            Assert.That(lb.Position, Is.EqualTo(7UL));
        }
    }
}
