using System;
using System.Collections.Generic;
using System.Text;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 存档快照 DTO（ADR-0005：显式版本化 DTO；<b>禁</b> Unity 序列化 Domain 权威状态）。
    /// 不可变。携带 schema 版本、配置指纹、各随机流位置，以及<b>分别序列化</b>的世界真值段与阵营知识段
    /// （TR-intel-003：加载不交叉污染）。Round-trip 一致性由 <see cref="ComputeHash"/> 校验（TR-save-002）。
    /// <para>
    /// 真值/知识此处以 <c>string→long</c> 字段映射建模权威数值状态——足以验证序列化契约
    /// （版本/指纹/随机流位置/分段隔离/round-trip 哈希）。各权威系统的完整状态在量产期由其自身 DTO 投影注入。
    /// </para>
    /// </summary>
    public sealed class SaveSnapshot
    {
        private readonly RngStreamState[] _rngStreams;
        private readonly Dictionary<string, long> _worldTruth;
        private readonly Dictionary<string, long> _factionKnowledge;

        /// <summary>schema 版本（兼容/迁移判定）。</summary>
        public SaveVersion Version { get; }

        /// <summary>配置指纹（加载时比对，ADR-0003 §4）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>各随机流位置（只读）。</summary>
        public IReadOnlyList<RngStreamState> RngStreams => _rngStreams;

        /// <summary>世界真值段（只读；与知识段隔离）。</summary>
        public IReadOnlyDictionary<string, long> WorldTruth => _worldTruth;

        /// <summary>阵营知识段（只读；与真值段隔离，不含真值）。</summary>
        public IReadOnlyDictionary<string, long> FactionKnowledge => _factionKnowledge;

        public SaveSnapshot(
            SaveVersion version,
            ConfigFingerprint fingerprint,
            IEnumerable<RngStreamState> rngStreams,
            IReadOnlyDictionary<string, long> worldTruth,
            IReadOnlyDictionary<string, long> factionKnowledge)
        {
            if (rngStreams == null) throw new ArgumentNullException(nameof(rngStreams));
            if (worldTruth == null) throw new ArgumentNullException(nameof(worldTruth));
            if (factionKnowledge == null) throw new ArgumentNullException(nameof(factionKnowledge));

            Version = version;
            Fingerprint = fingerprint;
            _rngStreams = new List<RngStreamState>(rngStreams).ToArray();
            _worldTruth = new Dictionary<string, long>(worldTruth, StringComparer.Ordinal);
            _factionKnowledge = new Dictionary<string, long>(factionKnowledge, StringComparer.Ordinal);
        }

        /// <summary>返回替换部分字段的新快照（不可变更新；供迁移链产出升级后副本）。</summary>
        public SaveSnapshot With(
            SaveVersion? version = null,
            ConfigFingerprint? fingerprint = null,
            IEnumerable<RngStreamState>? rngStreams = null,
            IReadOnlyDictionary<string, long>? worldTruth = null,
            IReadOnlyDictionary<string, long>? factionKnowledge = null)
            => new SaveSnapshot(
                version ?? Version,
                fingerprint ?? Fingerprint,
                rngStreams ?? _rngStreams,
                worldTruth ?? _worldTruth,
                factionKnowledge ?? _factionKnowledge);

        /// <summary>
        /// 确定性状态哈希（ADR-0004）：以规范顺序遍历版本、指纹、随机流（按名序数升序）、
        /// 真值段与知识段（各按键序数升序）。round-trip 后内容相同 → 哈希相等（TR-save-002）。
        /// </summary>
        public StateHash ComputeHash()
        {
            var h = new StateHasher();
            h.Append(Version.Major); h.Append(Version.Minor);
            h.Append(Fingerprint.Value);

            var streams = new List<RngStreamState>(_rngStreams);
            streams.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            h.Append(streams.Count);
            foreach (var s in streams) { AppendString(h, s.Name); h.Append(s.Seed); h.Append(s.Position); }

            AppendMap(h, _worldTruth);
            AppendMap(h, _factionKnowledge);
            return h.ToHash();
        }

        private static void AppendMap(StateHasher h, Dictionary<string, long> map)
        {
            var keys = new List<string>(map.Keys);
            keys.Sort(StringComparer.Ordinal);
            h.Append(keys.Count);
            foreach (var k in keys) { AppendString(h, k); h.Append(map[k]); }
        }

        private static void AppendString(StateHasher h, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            h.Append(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) h.Append(bytes[i]);
        }
    }
}
