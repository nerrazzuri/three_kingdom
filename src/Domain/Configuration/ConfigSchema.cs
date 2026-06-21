using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 数值字段的合法范围（ADR-0003 §2：valid_range）。<b>闭区间</b>：端点视为合法。
    /// 以定点 <see cref="FixedPoint"/> 表达，权威且跨平台一致（ADR-0004，禁 float）。
    /// </summary>
    public readonly struct ConfigFieldRange
    {
        /// <summary>下限（含）。</summary>
        public FixedPoint Min { get; }

        /// <summary>上限（含）。</summary>
        public FixedPoint Max { get; }

        /// <summary>构造范围；<paramref name="max"/> 小于 <paramref name="min"/> 抛 <see cref="ArgumentException"/>。</summary>
        public ConfigFieldRange(FixedPoint min, FixedPoint max)
        {
            if (max < min)
                throw new ArgumentException("ConfigFieldRange：Max 不可小于 Min。", nameof(max));
            Min = min;
            Max = max;
        }

        /// <summary>闭区间含值判定：Min ≤ value ≤ Max。</summary>
        public bool Contains(FixedPoint value) => value >= Min && value <= Max;
    }

    /// <summary>
    /// 配置 schema（ADR-0003 §2）：声明期望 schema 版本、各字段合法范围、必填字段。
    /// schema 为校验的<b>权威</b>来源——条目含未声明字段被拒绝（<see cref="ConfigErrorCode.UnknownField"/>）。
    /// 构造时对入参做防御性拷贝，构造后不可变。
    /// </summary>
    public sealed class ConfigSchema
    {
        private readonly Dictionary<string, ConfigFieldRange> _fieldRanges;
        private readonly string[] _requiredFields;

        /// <summary>期望的 schema 版本；条目版本不符即拒绝。</summary>
        public int SchemaVersion { get; }

        /// <summary>各数值字段的合法范围（字段名序数比较）。</summary>
        public IReadOnlyDictionary<string, ConfigFieldRange> FieldRanges => _fieldRanges;

        /// <summary>必填字段名集合。</summary>
        public IReadOnlyList<string> RequiredFields => _requiredFields;

        /// <param name="schemaVersion">期望 schema 版本。</param>
        /// <param name="fieldRanges">字段名→合法范围。null 视为空。</param>
        /// <param name="requiredFields">必填字段名。null 视为空。所有必填字段必须在 <paramref name="fieldRanges"/> 中声明。</param>
        public ConfigSchema(
            int schemaVersion,
            IReadOnlyDictionary<string, ConfigFieldRange>? fieldRanges = null,
            IReadOnlyList<string>? requiredFields = null)
        {
            SchemaVersion = schemaVersion;

            _fieldRanges = new Dictionary<string, ConfigFieldRange>(StringComparer.Ordinal);
            if (fieldRanges != null)
            {
                foreach (var kv in fieldRanges)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key))
                        throw new ArgumentException("ConfigSchema：字段名不可为空。", nameof(fieldRanges));
                    _fieldRanges[kv.Key] = kv.Value;
                }
            }

            if (requiredFields == null)
            {
                _requiredFields = Array.Empty<string>();
            }
            else
            {
                var req = new string[requiredFields.Count];
                for (int i = 0; i < requiredFields.Count; i++)
                {
                    var name = requiredFields[i];
                    if (string.IsNullOrWhiteSpace(name))
                        throw new ArgumentException("ConfigSchema：必填字段名不可为空。", nameof(requiredFields));
                    if (!_fieldRanges.ContainsKey(name))
                        throw new ArgumentException($"ConfigSchema：必填字段「{name}」未在 fieldRanges 中声明。", nameof(requiredFields));
                    req[i] = name;
                }
                _requiredFields = req;
            }
        }
    }
}
