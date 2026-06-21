using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Configuration
{
    /// <summary>
    /// epic-001 story-003：版本化配置加载与校验。
    /// 治理 ADR：ADR-0003（数据驱动配置——进入 Domain 前完整校验、不可变运行时配置、配置指纹、无部分写入）。
    /// 覆盖 AC-1 范围校验（含边界/负值/空集）、AC-2 引用完整性、AC-3 指纹稳定/变化/顺序无关，及无部分写入与稳定错误码。
    /// </summary>
    [TestFixture]
    public class ConfigLoadValidateTests
    {
        private const int SchemaV = 3;

        // 字段 "ratio" ∈ [0,1]；"weight" ∈ [-2,2]（含负值范围）。
        private static ConfigSchema MakeSchema(bool ratioRequired = false)
        {
            var ranges = new Dictionary<string, ConfigFieldRange>
            {
                ["ratio"] = new ConfigFieldRange(FixedPoint.Zero, FixedPoint.FromInt(1)),
                ["weight"] = new ConfigFieldRange(FixedPoint.FromInt(-2), FixedPoint.FromInt(2)),
            };
            var required = ratioRequired ? new[] { "ratio" } : Array.Empty<string>();
            return new ConfigSchema(SchemaV, ranges, required);
        }

        private static ConfigEntryDraft Entry(
            string id,
            IReadOnlyDictionary<string, FixedPoint>? fields = null,
            IReadOnlyList<StableId>? refs = null,
            int schemaVersion = SchemaV)
            => new ConfigEntryDraft(new StableId(id), schemaVersion, fields, refs);

        private static ConfigDraft Draft(params ConfigEntryDraft[] entries) => new ConfigDraft(entries);

        // ---- AC-1：范围校验 ----

        [Test]
        public void Validate_in_range_values_succeeds()
        {
            var draft = Draft(Entry("a", new Dictionary<string, FixedPoint>
            {
                ["ratio"] = FixedPoint.FromFraction(1, 2),
                ["weight"] = FixedPoint.FromInt(1),
            }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.Value.Get(new StableId("a")).GetField("ratio"),
                Is.EqualTo(FixedPoint.FromFraction(1, 2)));
        }

        [Test]
        public void Validate_value_above_max_is_rejected_with_stable_code()
        {
            var draft = Draft(Entry("a", new Dictionary<string, FixedPoint>
            {
                ["ratio"] = FixedPoint.FromFraction(3, 2), // 1.5 > 1
            }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Exactly(1).Items);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.ValueOutOfRange));
            Assert.That(result.Errors[0].Field, Is.EqualTo("ratio"));
            Assert.That(result.Errors[0].EntryId, Is.EqualTo("a"));
        }

        [Test]
        public void Validate_value_below_min_is_rejected()
        {
            var draft = Draft(Entry("a", new Dictionary<string, FixedPoint>
            {
                ["weight"] = FixedPoint.FromInt(-3), // < -2
            }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.ValueOutOfRange));
        }

        [Test]
        public void Validate_boundary_values_are_accepted_inclusive()
        {
            var draft = Draft(
                Entry("lo", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.Zero }),
                Entry("hi", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromInt(1) }),
                Entry("neg", new Dictionary<string, FixedPoint> { ["weight"] = FixedPoint.FromInt(-2) }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.True, "闭区间端点（含负下限）应被接受");
        }

        [Test]
        public void Validate_entry_with_no_fields_and_no_required_succeeds()
        {
            var draft = Draft(Entry("empty"));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Get(new StableId("empty")).Fields, Is.Empty);
        }

        [Test]
        public void Validate_empty_draft_succeeds_with_empty_config()
        {
            var result = ConfigValidator.Validate(new ConfigDraft(null), MakeSchema());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_unknown_field_is_rejected()
        {
            var draft = Draft(Entry("a", new Dictionary<string, FixedPoint>
            {
                ["nope"] = FixedPoint.Zero,
            }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.UnknownField));
        }

        [Test]
        public void Validate_missing_required_field_is_rejected()
        {
            var draft = Draft(Entry("a", new Dictionary<string, FixedPoint>
            {
                ["weight"] = FixedPoint.Zero,
            }));

            var result = ConfigValidator.Validate(draft, MakeSchema(ratioRequired: true));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.MissingRequiredField));
            Assert.That(result.Errors[0].Field, Is.EqualTo("ratio"));
        }

        [Test]
        public void Validate_schema_version_mismatch_is_rejected()
        {
            var draft = Draft(Entry("a", schemaVersion: SchemaV + 1));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.SchemaVersionMismatch));
        }

        // ---- AC-2：引用完整性 ----

        [Test]
        public void Validate_reference_to_existing_id_succeeds()
        {
            var draft = Draft(
                Entry("target"),
                Entry("source", refs: new[] { new StableId("target") }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Validate_reference_to_missing_id_is_rejected()
        {
            var draft = Draft(
                Entry("source", refs: new[] { new StableId("ghost") }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.MissingReference));
            Assert.That(result.Errors[0].EntryId, Is.EqualTo("source"));
        }

        [Test]
        public void Validate_duplicate_stable_id_is_rejected()
        {
            var draft = Draft(Entry("dup"), Entry("dup"));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors[0].Code, Is.EqualTo(ConfigErrorCode.DuplicateStableId));
        }

        // ---- 无部分写入 + 错误聚合 ----

        [Test]
        public void Validate_aggregates_multiple_errors_and_yields_no_value()
        {
            var draft = Draft(
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromInt(5) }),
                Entry("b", refs: new[] { new StableId("ghost") }));

            var result = ConfigValidator.Validate(draft, MakeSchema());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Count, Is.GreaterThanOrEqualTo(2), "应聚合多条错误而非首错即返");
            Assert.Throws<InvalidOperationException>(() => { var _ = result.Value; },
                "失败结果读取 Value 须抛异常（无部分写入）");
        }

        // ---- AC-3：配置指纹 ----

        [Test]
        public void Fingerprint_is_stable_across_two_validations()
        {
            ConfigDraft Build() => Draft(
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromFraction(1, 4) }),
                Entry("b", new Dictionary<string, FixedPoint> { ["weight"] = FixedPoint.FromInt(1) },
                    new[] { new StableId("a") }));

            var fp1 = ConfigValidator.Validate(Build(), MakeSchema()).Value.ComputeFingerprint();
            var fp2 = ConfigValidator.Validate(Build(), MakeSchema()).Value.ComputeFingerprint();

            Assert.That(fp1, Is.EqualTo(fp2));
        }

        [Test]
        public void Fingerprint_is_independent_of_entry_insertion_order()
        {
            var ab = Draft(
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromFraction(1, 4) }),
                Entry("b", new Dictionary<string, FixedPoint> { ["weight"] = FixedPoint.FromInt(1) }));
            var ba = Draft(
                Entry("b", new Dictionary<string, FixedPoint> { ["weight"] = FixedPoint.FromInt(1) }),
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromFraction(1, 4) }));

            var fpAb = ConfigValidator.Validate(ab, MakeSchema()).Value.ComputeFingerprint();
            var fpBa = ConfigValidator.Validate(ba, MakeSchema()).Value.ComputeFingerprint();

            Assert.That(fpAb, Is.EqualTo(fpBa), "指纹须对插入顺序不敏感（规范化排序）");
        }

        [Test]
        public void Fingerprint_changes_when_any_value_changes()
        {
            var baseDraft = Draft(
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromFraction(1, 4) }));
            var changed = Draft(
                Entry("a", new Dictionary<string, FixedPoint> { ["ratio"] = FixedPoint.FromFraction(1, 3) }));

            var fpBase = ConfigValidator.Validate(baseDraft, MakeSchema()).Value.ComputeFingerprint();
            var fpChanged = ConfigValidator.Validate(changed, MakeSchema()).Value.ComputeFingerprint();

            Assert.That(fpChanged, Is.Not.EqualTo(fpBase));
        }

        [Test]
        public void Fingerprint_changes_when_reference_changes()
        {
            var withRef = Draft(
                Entry("t1"), Entry("t2"),
                Entry("s", refs: new[] { new StableId("t1") }));
            var withOtherRef = Draft(
                Entry("t1"), Entry("t2"),
                Entry("s", refs: new[] { new StableId("t2") }));

            var fp1 = ConfigValidator.Validate(withRef, MakeSchema()).Value.ComputeFingerprint();
            var fp2 = ConfigValidator.Validate(withOtherRef, MakeSchema()).Value.ComputeFingerprint();

            Assert.That(fp2, Is.Not.EqualTo(fp1));
        }

        // ---- 不可变配置访问 ----

        [Test]
        public void ValidatedConfig_get_missing_id_throws()
        {
            var result = ConfigValidator.Validate(Draft(Entry("a")), MakeSchema());

            Assert.That(result.Value.Contains(new StableId("a")), Is.True);
            Assert.That(result.Value.Contains(new StableId("zzz")), Is.False);
            Assert.Throws<KeyNotFoundException>(() => result.Value.Get(new StableId("zzz")));
        }
    }
}
