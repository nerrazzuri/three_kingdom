using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// epic-009 story-001：版本化 DTO + 原子写 + 迁移链。
    /// 治理 ADR：ADR-0005（版本化 DTO、临时文件原子写、逆序逐版迁移链、只操作副本、失败保留上一份）。
    /// 覆盖 AC-1 原子写不破坏现有档（临时写/改名失败均保留旧档）、AC-2 迁移链逆序应用 + 中途失败保留原档。
    /// </summary>
    [TestFixture]
    public class VersionedAtomicSaveTests
    {
        private static readonly ISaveSerializer Serializer = new CanonicalSaveSerializer();
        private const string Slot = "campaign";

        private static SaveSnapshot Snapshot(int major, int minor, long troopCount = 3000)
            => new SaveSnapshot(
                new SaveVersion(major, minor),
                new ConfigFingerprint(0xABCDEF1234567890UL),
                new[] { new RngStreamState("battle", 42UL, 7UL) },
                new Dictionary<string, long> { ["enemy.actual_strength"] = 1200, ["troops"] = troopCount },
                new Dictionary<string, long> { ["enemy.estimate"] = 1000 });

        // ---- DTO round-trip 映射（序列化↔反序列化等价）----

        [Test]
        public void test_dto_serialize_deserialize_preserves_snapshot_hash()
        {
            var s = Snapshot(1, 0);
            var restored = Serializer.Deserialize(Serializer.Serialize(s));
            Assert.That(restored.ComputeHash(), Is.EqualTo(s.ComputeHash()));
            Assert.That(restored.Version, Is.EqualTo(s.Version));
        }

        // ---- AC-1: 原子写成功 ----

        [Test]
        public void test_save_commits_via_temp_then_rename()
        {
            var medium = new InMemorySaveMedium();
            var repo = new SaveRepository(medium, Serializer);

            var result = repo.Save(Slot, Snapshot(1, 0));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(medium.Exists(Slot), Is.True);
            Assert.That(medium.Exists(Slot + ".tmp"), Is.False, "临时槽改名后不残留。");
            Assert.That(repo.ReadRaw(Slot)!.ComputeHash(), Is.EqualTo(Snapshot(1, 0).ComputeHash()));
        }

        // ---- AC-1: 临时写失败 → 正式存档保留上一份 ----

        [Test]
        public void test_temp_write_failure_preserves_existing_valid_save()
        {
            var medium = new InMemorySaveMedium();
            var repo = new SaveRepository(medium, Serializer);
            repo.Save(Slot, Snapshot(1, 0)); // 既有有效存档（troops=3000）
            var goodHash = repo.ReadRaw(Slot)!.ComputeHash();

            medium.FailWriteOn.Add(Slot + ".tmp"); // 模拟磁盘写满
            var result = repo.Save(Slot, Snapshot(1, 0, troopCount: 9999));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(SaveErrorCode.TempWriteFailed));
            // 原存档完好（仍是 troops=3000 的旧档），未被半写破坏。
            Assert.That(repo.ReadRaw(Slot)!.ComputeHash(), Is.EqualTo(goodHash));
            Assert.That(medium.Exists(Slot + ".tmp"), Is.False, "失败后临时槽被清理。");
        }

        // ---- AC-1: 原子改名失败 → 正式存档保留上一份 ----

        [Test]
        public void test_rename_failure_preserves_existing_valid_save()
        {
            var medium = new InMemorySaveMedium();
            var repo = new SaveRepository(medium, Serializer);
            repo.Save(Slot, Snapshot(1, 0));
            var goodHash = repo.ReadRaw(Slot)!.ComputeHash();

            medium.FailMove = true;
            var result = repo.Save(Slot, Snapshot(1, 0, troopCount: 9999));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(SaveErrorCode.CommitFailed));
            Assert.That(repo.ReadRaw(Slot)!.ComputeHash(), Is.EqualTo(goodHash), "正式存档保持上一份有效内容。");
        }

        // ---- AC-2: 迁移链逐版应用 ----

        [Test]
        public void test_migration_chain_applies_each_version_step()
        {
            var migrator = new SaveMigrator(new ISaveMigration[]
            {
                new BumpMigration(new SaveVersion(1, 0), new SaveVersion(1, 1)),
                new BumpMigration(new SaveVersion(1, 1), new SaveVersion(1, 2)),
            });

            var result = migrator.Migrate(Snapshot(1, 0), new SaveVersion(1, 2));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Result!.Version, Is.EqualTo(new SaveVersion(1, 2)));
            // 每步累加一个迁移标记字段 → 证明逐版应用。
            Assert.That(result.Result!.WorldTruth["migrated.steps"], Is.EqualTo(2));
        }

        [Test]
        public void test_migration_step_failure_preserves_original_snapshot()
        {
            var original = Snapshot(1, 0);
            var migrator = new SaveMigrator(new ISaveMigration[]
            {
                new BumpMigration(new SaveVersion(1, 0), new SaveVersion(1, 1)),
                new ThrowingMigration(new SaveVersion(1, 1), new SaveVersion(1, 2)),
            });

            var result = migrator.Migrate(original, new SaveVersion(1, 2));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(MigrationErrorCode.StepFailed));
            // 原快照不可变，未被触碰（仍是 1.0、无迁移标记）。
            Assert.That(original.Version, Is.EqualTo(new SaveVersion(1, 0)));
            Assert.That(original.WorldTruth.ContainsKey("migrated.steps"), Is.False);
        }

        [Test]
        public void test_missing_migration_step_reports_broken_chain()
        {
            var migrator = new SaveMigrator(new ISaveMigration[]
            {
                new BumpMigration(new SaveVersion(1, 0), new SaveVersion(1, 1)),
                // 缺 1.1→1.2 步骤
            });

            var result = migrator.Migrate(Snapshot(1, 0), new SaveVersion(1, 2));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(MigrationErrorCode.NoMigrationPath));
        }

        // —— 测试用迁移步骤 ——

        private sealed class BumpMigration : ISaveMigration
        {
            public SaveVersion From { get; }
            public SaveVersion To { get; }
            public BumpMigration(SaveVersion from, SaveVersion to) { From = from; To = to; }
            public SaveSnapshot Apply(SaveSnapshot s)
            {
                var truth = new Dictionary<string, long>();
                foreach (var kv in s.WorldTruth) truth[kv.Key] = kv.Value;
                truth["migrated.steps"] = (truth.TryGetValue("migrated.steps", out long n) ? n : 0) + 1;
                return s.With(version: To, worldTruth: truth);
            }
        }

        private sealed class ThrowingMigration : ISaveMigration
        {
            public SaveVersion From { get; }
            public SaveVersion To { get; }
            public ThrowingMigration(SaveVersion from, SaveVersion to) { From = from; To = to; }
            public SaveSnapshot Apply(SaveSnapshot s) => throw new System.InvalidOperationException("模拟迁移失败");
        }
    }
}
