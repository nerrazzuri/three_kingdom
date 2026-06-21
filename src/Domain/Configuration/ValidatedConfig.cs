using System;
using System.Collections.Generic;
using System.Text;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 已校验的不可变配置条目（ADR-0003 §3：运行期只读，无任何 Set/Mutate 路径）。
    /// 仅由 <see cref="ConfigValidator"/> 在全量校验通过后构造（internal 构造函数）。
    /// </summary>
    public sealed class ConfigEntry
    {
        private readonly Dictionary<string, FixedPoint> _fields;
        private readonly StableId[] _references;

        /// <summary>条目稳定标识。</summary>
        public StableId Id { get; }

        /// <summary>schema 版本。</summary>
        public int SchemaVersion { get; }

        /// <summary>数值字段（只读）。</summary>
        public IReadOnlyDictionary<string, FixedPoint> Fields => _fields;

        /// <summary>交叉引用目标（只读）。</summary>
        public IReadOnlyList<StableId> References => _references;

        internal ConfigEntry(
            StableId id,
            int schemaVersion,
            IReadOnlyDictionary<string, FixedPoint> fields,
            IReadOnlyList<StableId> references)
        {
            Id = id;
            SchemaVersion = schemaVersion;
            _fields = new Dictionary<string, FixedPoint>(StringComparer.Ordinal);
            foreach (var kv in fields) _fields[kv.Key] = kv.Value;
            var refs = new StableId[references.Count];
            for (int i = 0; i < references.Count; i++) refs[i] = references[i];
            _references = refs;
        }

        /// <summary>取数值字段；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public FixedPoint GetField(string name)
        {
            if (_fields.TryGetValue(name, out var v)) return v;
            throw new KeyNotFoundException($"ConfigEntry「{Id}」无字段「{name}」。");
        }

        /// <summary>尝试取数值字段。</summary>
        public bool TryGetField(string name, out FixedPoint value) => _fields.TryGetValue(name, out value);
    }

    /// <summary>
    /// 配置指纹（ADR-0003 §4）：对一份已校验配置内容的确定性哈希。
    /// 纳入战役状态哈希（ADR-0004）与存档头兼容判定（ADR-0005）。
    /// </summary>
    public readonly struct ConfigFingerprint : IEquatable<ConfigFingerprint>
    {
        /// <summary>64 位指纹值。</summary>
        public ulong Value { get; }

        public ConfigFingerprint(ulong value) => Value = value;

        public bool Equals(ConfigFingerprint other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is ConfigFingerprint other && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(ConfigFingerprint a, ConfigFingerprint b) => a.Value == b.Value;
        public static bool operator !=(ConfigFingerprint a, ConfigFingerprint b) => a.Value != b.Value;
        public override string ToString() => "0x" + Value.ToString("X16");
    }

    /// <summary>
    /// 已校验的不可变配置集（ADR-0003 §3）。Domain 只读消费，无可变接口。
    /// 仅由 <see cref="ConfigValidator"/> 构造。提供确定性 <see cref="ComputeFingerprint"/>。
    /// </summary>
    public sealed class ValidatedConfig
    {
        private readonly Dictionary<StableId, ConfigEntry> _entries;

        internal ValidatedConfig(Dictionary<StableId, ConfigEntry> entries)
        {
            _entries = entries;
        }

        /// <summary>条目数量。</summary>
        public int Count => _entries.Count;

        /// <summary>是否含指定 stable_id。</summary>
        public bool Contains(StableId id) => _entries.ContainsKey(id);

        /// <summary>取条目；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public ConfigEntry Get(StableId id)
        {
            if (_entries.TryGetValue(id, out var e)) return e;
            throw new KeyNotFoundException($"ValidatedConfig 无条目「{id}」。");
        }

        /// <summary>尝试取条目。</summary>
        public bool TryGet(StableId id, out ConfigEntry entry) => _entries.TryGetValue(id, out entry!);

        /// <summary>
        /// 计算配置指纹（ADR-0003 §4：H(stable_id ‖ schema_version ‖ 规范化值)）。
        /// <b>确定性</b>：条目按 stable_id 序数升序、字段按名序数升序、引用按值序数升序遍历，
        /// 字符串以「长度前缀 + UTF-8 字节」写入避免拼接歧义；定点值取权威 Raw。
        /// 故同一内容（与插入顺序无关）→ 同一指纹；任一值变更 → 指纹变化（复用 ADR-0004 哈希底座）。
        /// </summary>
        public ConfigFingerprint ComputeFingerprint()
        {
            var hasher = new StateHasher();

            // 条目按 stable_id 序数升序——与构造/插入顺序无关，保证规范化。
            var ids = new List<StableId>(_entries.Keys);
            ids.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));

            hasher.Append(ids.Count);
            foreach (var id in ids)
            {
                var entry = _entries[id];
                AppendString(hasher, id.Value);
                hasher.Append(entry.SchemaVersion);

                // 字段按名序数升序。
                var fieldNames = new List<string>(entry.Fields.Keys);
                fieldNames.Sort(StringComparer.Ordinal);
                hasher.Append(fieldNames.Count);
                foreach (var name in fieldNames)
                {
                    AppendString(hasher, name);
                    hasher.Append(entry.Fields[name]); // FixedPoint → Raw（权威字节）
                }

                // 引用按值序数升序。
                var refs = new List<StableId>(entry.References);
                refs.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
                hasher.Append(refs.Count);
                foreach (var r in refs) AppendString(hasher, r.Value);
            }

            return new ConfigFingerprint(hasher.ToHash().Value);
        }

        /// <summary>以「长度前缀(int) + UTF-8 字节」确定性写入字符串。</summary>
        private static void AppendString(StateHasher hasher, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            hasher.Append(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) hasher.Append(bytes[i]);
        }
    }
}
