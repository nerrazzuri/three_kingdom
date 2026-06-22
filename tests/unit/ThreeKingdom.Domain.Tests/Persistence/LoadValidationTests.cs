using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// epic-009 story-003：加载校验与不兼容拒绝。
    /// 治理 ADR：ADR-0005（先校验后载入；不兼容不部分载入）+ epic-005 真值/知识分离（TR-intel-003）。
    /// 覆盖 AC-1 不兼容/指纹不符/损坏拒绝且零部分载入 + 可行动原因、AC-2 真值/知识不交叉污染。
    /// </summary>
    [TestFixture]
    public class LoadValidationTests
    {
        private static readonly ISaveSerializer Serializer = new CanonicalSaveSerializer();
        private const string Slot = "campaign";

        private static readonly SaveVersion Current = new SaveVersion(1, 0);
        private static readonly ConfigFingerprint CurrentFp = new ConfigFingerprint(0xDEADBEEFUL);

        private static SaveSnapshot Snapshot(SaveVersion version, ConfigFingerprint fp,
            Dictionary<string, long>? truth = null, Dictionary<string, long>? knowledge = null)
            => new SaveSnapshot(version, fp,
                new[] { new RngStreamState("battle", 1UL, 2UL) },
                truth ?? new Dictionary<string, long> { ["troops"] = 3000 },
                knowledge ?? new Dictionary<string, long> { ["enemy.estimate"] = 1000 });

        private static InMemorySaveMedium MediumWith(string content)
        {
            var m = new InMemorySaveMedium();
            m.Write(Slot, content);
            return m;
        }

        private static SaveLoadService Service(InMemorySaveMedium medium, params ISaveMigration[] migrations)
            => new SaveLoadService(medium, Serializer, new SaveMigrator(migrations));

        // ---- AC-1: 兼容 + 指纹一致 → 成功 ----

        [Test]
        public void test_compatible_save_with_matching_fingerprint_loads()
        {
            var medium = MediumWith(Serializer.Serialize(Snapshot(Current, CurrentFp)));
            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Snapshot!.Version, Is.EqualTo(Current));
        }

        // ---- AC-1: 版本高于当前 → 拒绝，零部分载入 ----

        [Test]
        public void test_newer_version_save_is_rejected_without_partial_load()
        {
            var newer = Serializer.Serialize(Snapshot(new SaveVersion(2, 0), CurrentFp));
            var medium = MediumWith(newer);
            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.IncompatibleNewer));
            Assert.That(result.Snapshot, Is.Null, "零部分载入。");
            Assert.That(result.Reason, Is.Not.Empty, "返回可行动原因。");
            Assert.That(medium.Peek(Slot), Is.EqualTo(newer), "拒绝不改动磁盘存档（当前会话状态不变）。");
        }

        [Test]
        public void test_version_exactly_one_minor_higher_is_incompatible()
        {
            // 边界：版本恰高一位（minor）。
            var medium = MediumWith(Serializer.Serialize(Snapshot(new SaveVersion(1, 1), CurrentFp)));
            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.IncompatibleNewer));
        }

        // ---- AC-1: 指纹不符 → 拒绝 ----

        [Test]
        public void test_fingerprint_mismatch_is_rejected()
        {
            // 边界：指纹仅差一字段。
            var medium = MediumWith(Serializer.Serialize(Snapshot(Current, new ConfigFingerprint(0xDEADBEEEUL))));
            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.FingerprintMismatch));
            Assert.That(result.Snapshot, Is.Null);
        }

        // ---- AC-1: 损坏 → 拒绝 ----

        [Test]
        public void test_corrupted_save_is_rejected()
        {
            var medium = MediumWith("not-a-valid-save\nrandom garbage");
            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.Corrupted));
        }

        [Test]
        public void test_missing_knowledge_segment_is_rejected_not_backfilled_from_truth()
        {
            // 截断掉知识段 → 结构不完整 → 拒绝（而非用真值回填，TR-intel-003 edge）。
            string full = Serializer.Serialize(Snapshot(Current, CurrentFp));
            int kIdx = full.IndexOf("knowledge\t", System.StringComparison.Ordinal);
            string truncated = full.Substring(0, kIdx); // 去掉 knowledge 段
            var medium = MediumWith(truncated);

            var result = Service(medium).Load(Slot, Current, CurrentFp);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.Corrupted));
        }

        // ---- AC-1: 可迁移旧档 → 迁移后成功 ----

        [Test]
        public void test_migratable_old_save_is_upgraded_and_loaded()
        {
            var oldSave = Serializer.Serialize(Snapshot(new SaveVersion(1, 0), CurrentFp));
            var medium = MediumWith(oldSave);
            var service = Service(medium, new VersionBump(new SaveVersion(1, 0), new SaveVersion(1, 1)));

            var result = service.Load(Slot, new SaveVersion(1, 1), CurrentFp);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Snapshot!.Version, Is.EqualTo(new SaveVersion(1, 1)));
        }

        // ---- AC-2: 真值/知识不交叉污染 ----

        [Test]
        public void test_truth_and_knowledge_segments_do_not_cross_contaminate()
        {
            // 同名键在两段取不同值：真值=实际 1200，知识=估计 1000。加载后各归各位，知识不含真值。
            var snapshot = Snapshot(Current, CurrentFp,
                truth: new Dictionary<string, long> { ["enemy.strength"] = 1200 },
                knowledge: new Dictionary<string, long> { ["enemy.strength"] = 1000 });
            var medium = MediumWith(Serializer.Serialize(snapshot));

            var loaded = Service(medium).Load(Slot, Current, CurrentFp).Snapshot!;

            Assert.That(loaded.WorldTruth["enemy.strength"], Is.EqualTo(1200), "真值段保留实际值。");
            Assert.That(loaded.FactionKnowledge["enemy.strength"], Is.EqualTo(1000), "知识段保留估计值，未被真值污染。");
        }

        private sealed class VersionBump : ISaveMigration
        {
            public SaveVersion From { get; }
            public SaveVersion To { get; }
            public VersionBump(SaveVersion from, SaveVersion to) { From = from; To = to; }
            public SaveSnapshot Apply(SaveSnapshot s) => s.With(version: To);
        }
    }
}
