using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 配置校验框架（ADR-0003 §2）。两阶段校验后整体接受或整体拒绝：
    /// <list type="number">
    ///   <item>schema/范围：schema 版本、必填字段、未知字段、数值范围</item>
    ///   <item>引用完整性：交叉引用指向集合内存在的 stable_id</item>
    /// </list>
    /// <b>错误聚合</b>：收集全部错误后一次返回（非首错即返），便于一次修齐。
    /// <b>无部分写入</b>（ADR-0003）：只要有任一错误，绝不构造任何 <see cref="ValidatedConfig"/>——
    /// 不可变条目仅在零错误后才建立，失败路径返回的 <see cref="Result{T}"/> 无 Value。
    /// </summary>
    public static class ConfigValidator
    {
        /// <summary>
        /// 校验一份草稿。成功返回不可变 <see cref="ValidatedConfig"/>；失败返回聚合错误且无 Value。
        /// </summary>
        /// <param name="draft">待校验草稿。</param>
        /// <param name="schema">权威 schema。</param>
        public static Result<ValidatedConfig> Validate(ConfigDraft draft, ConfigSchema schema)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (schema == null) throw new ArgumentNullException(nameof(schema));

            var errors = new List<ConfigError>();
            var allIds = CollectIds(draft, errors);

            foreach (var entry in draft.Entries)
                ValidateEntry(entry, schema, allIds, errors);

            // 无部分写入：有任一错误即整体拒绝，不构造任何不可变条目。
            if (errors.Count > 0)
                return Result<ValidatedConfig>.Failure(errors);

            return Result<ValidatedConfig>.Success(Build(draft));
        }

        /// <summary>收集全部 stable_id 并记录重复（引用完整性的判定基准）。</summary>
        private static HashSet<StableId> CollectIds(ConfigDraft draft, List<ConfigError> errors)
        {
            var allIds = new HashSet<StableId>();
            foreach (var entry in draft.Entries)
            {
                if (!allIds.Add(entry.Id))
                {
                    errors.Add(new ConfigError(
                        ConfigErrorCode.DuplicateStableId,
                        $"重复 stable_id：{entry.Id}",
                        entry.Id.Value));
                }
            }
            return allIds;
        }

        /// <summary>逐条目校验 schema 版本、必填/未知字段、数值范围与引用完整性。</summary>
        private static void ValidateEntry(
            ConfigEntryDraft entry, ConfigSchema schema, HashSet<StableId> allIds, List<ConfigError> errors)
        {
            if (entry.SchemaVersion != schema.SchemaVersion)
            {
                errors.Add(new ConfigError(
                    ConfigErrorCode.SchemaVersionMismatch,
                    $"schema 版本不符：期望 {schema.SchemaVersion}，实际 {entry.SchemaVersion}",
                    entry.Id.Value));
            }

            foreach (var required in schema.RequiredFields)
            {
                if (!entry.Fields.ContainsKey(required))
                {
                    errors.Add(new ConfigError(
                        ConfigErrorCode.MissingRequiredField,
                        $"缺失必填字段：{required}",
                        entry.Id.Value, required));
                }
            }

            foreach (var field in entry.Fields)
                ValidateField(entry, schema, field.Key, field.Value, errors);

            foreach (var reference in entry.References)
            {
                if (!allIds.Contains(reference))
                {
                    errors.Add(new ConfigError(
                        ConfigErrorCode.MissingReference,
                        $"缺失引用：{reference}",
                        entry.Id.Value));
                }
            }
        }

        /// <summary>校验单个数值字段：schema 须声明，且值须落在合法闭区间。</summary>
        private static void ValidateField(
            ConfigEntryDraft entry, ConfigSchema schema, string name, Numerics.FixedPoint value, List<ConfigError> errors)
        {
            if (!schema.FieldRanges.TryGetValue(name, out var range))
            {
                errors.Add(new ConfigError(
                    ConfigErrorCode.UnknownField,
                    $"未知字段（schema 未声明）：{name}",
                    entry.Id.Value, name));
            }
            else if (!range.Contains(value))
            {
                errors.Add(new ConfigError(
                    ConfigErrorCode.ValueOutOfRange,
                    $"值越界：raw={value.Raw} 不在 [raw {range.Min.Raw}, raw {range.Max.Raw}]",
                    entry.Id.Value, name));
            }
        }

        /// <summary>零错误后才建立不可变配置。</summary>
        private static ValidatedConfig Build(ConfigDraft draft)
        {
            var built = new Dictionary<StableId, ConfigEntry>();
            foreach (var entry in draft.Entries)
            {
                built[entry.Id] = new ConfigEntry(entry.Id, entry.SchemaVersion, entry.Fields, entry.References);
            }
            return new ValidatedConfig(built);
        }
    }
}
