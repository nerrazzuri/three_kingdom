using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 待校验的单个配置条目（解析产出、未校验）。承载 stable_id、schema 版本、数值字段与交叉引用。
    /// 构造时对集合做防御性拷贝——草稿一经构造即不可变，校验不会被外部并发修改干扰。
    /// </summary>
    public sealed class ConfigEntryDraft
    {
        private readonly Dictionary<string, FixedPoint> _fields;
        private readonly StableId[] _references;

        /// <summary>条目稳定标识。</summary>
        public StableId Id { get; }

        /// <summary>条目自报的 schema 版本。</summary>
        public int SchemaVersion { get; }

        /// <summary>数值字段（字段名序数比较）。</summary>
        public IReadOnlyDictionary<string, FixedPoint> Fields => _fields;

        /// <summary>交叉引用的目标 stable_id 列表。</summary>
        public IReadOnlyList<StableId> References => _references;

        /// <param name="id">条目稳定标识。</param>
        /// <param name="schemaVersion">条目自报 schema 版本。</param>
        /// <param name="fields">数值字段。null 视为空。字段名不可为空。</param>
        /// <param name="references">交叉引用目标。null 视为空。</param>
        public ConfigEntryDraft(
            StableId id,
            int schemaVersion,
            IReadOnlyDictionary<string, FixedPoint>? fields = null,
            IReadOnlyList<StableId>? references = null)
        {
            Id = id;
            SchemaVersion = schemaVersion;

            _fields = new Dictionary<string, FixedPoint>(StringComparer.Ordinal);
            if (fields != null)
            {
                foreach (var kv in fields)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key))
                        throw new ArgumentException("ConfigEntryDraft：字段名不可为空。", nameof(fields));
                    _fields[kv.Key] = kv.Value;
                }
            }

            if (references == null)
            {
                _references = Array.Empty<StableId>();
            }
            else
            {
                var refs = new StableId[references.Count];
                for (int i = 0; i < references.Count; i++) refs[i] = references[i];
                _references = refs;
            }
        }
    }

    /// <summary>
    /// 待校验的一组配置条目（一个配置集）。构造时防御性拷贝条目列表。
    /// </summary>
    public sealed class ConfigDraft
    {
        private readonly ConfigEntryDraft[] _entries;

        /// <summary>条目列表（保持输入顺序，仅作枚举；权威标识为各条目 <see cref="ConfigEntryDraft.Id"/>）。</summary>
        public IReadOnlyList<ConfigEntryDraft> Entries => _entries;

        /// <param name="entries">条目集合。null 视为空。元素不可为 null。</param>
        public ConfigDraft(IReadOnlyList<ConfigEntryDraft>? entries)
        {
            if (entries == null)
            {
                _entries = Array.Empty<ConfigEntryDraft>();
                return;
            }

            var copy = new ConfigEntryDraft[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                copy[i] = entries[i] ?? throw new ArgumentException("ConfigDraft：条目不可为 null。", nameof(entries));
            }
            _entries = copy;
        }
    }
}
