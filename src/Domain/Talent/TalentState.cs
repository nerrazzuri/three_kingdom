using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Talent
{
    /// <summary>
    /// 人才循环权威态（GDD_020 §14）：已知晓人才（id→渠道，反全知）+ 各人招揽尝试次数 + 已入伙集。不可变；纳入存档、确定性哈希。
    /// </summary>
    public sealed class TalentState
    {
        private readonly Dictionary<string, TalentChannel> _known;
        private readonly Dictionary<string, int> _attempts;
        private readonly HashSet<string> _recruited;

        public TalentState(
            IReadOnlyDictionary<string, TalentChannel>? known,
            IReadOnlyDictionary<string, int>? attempts,
            IReadOnlyCollection<string>? recruited)
        {
            _known = new Dictionary<string, TalentChannel>(StringComparer.Ordinal);
            if (known != null) foreach (KeyValuePair<string, TalentChannel> kv in known) _known[kv.Key] = kv.Value;
            _attempts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (attempts != null) foreach (KeyValuePair<string, int> kv in attempts) if (kv.Value != 0) _attempts[kv.Key] = kv.Value;
            _recruited = new HashSet<string>(StringComparer.Ordinal);
            if (recruited != null) foreach (string r in recruited) _recruited.Add(r);
        }

        public static TalentState Empty { get; } = new TalentState(null, null, null);

        public bool Knows(TalentId id) => _known.ContainsKey(id.Value ?? "");
        public bool IsRecruited(TalentId id) => _recruited.Contains(id.Value ?? "");
        public int Attempts(TalentId id) => _attempts.TryGetValue(id.Value ?? "", out int v) ? v : 0;
        public IReadOnlyDictionary<string, TalentChannel> Known => _known;
        public IReadOnlyCollection<string> Recruited => _recruited;

        /// <summary>知晓某人才（经渠道进入视野，反全知）；已知则保留首个渠道。</summary>
        public TalentState Reveal(TalentId id, TalentChannel channel)
        {
            if (_known.ContainsKey(id.Value)) return this;
            var known = new Dictionary<string, TalentChannel>(_known, StringComparer.Ordinal) { [id.Value] = channel };
            return new TalentState(known, _attempts, _recruited);
        }

        /// <summary>记一次招揽尝试（attemptIndex+1，供种子分流使多次尝试各有独立确定性结果）。</summary>
        public TalentState RecordAttempt(TalentId id)
        {
            var attempts = new Dictionary<string, int>(_attempts, StringComparer.Ordinal);
            attempts[id.Value] = (attempts.TryGetValue(id.Value, out int v) ? v : 0) + 1;
            return new TalentState(_known, attempts, _recruited);
        }

        /// <summary>入伙。</summary>
        public TalentState Recruit(TalentId id)
        {
            var recruited = new HashSet<string>(_recruited, StringComparer.Ordinal) { id.Value };
            return new TalentState(_known, _attempts, recruited);
        }

        internal void AppendTo(StateHasher hasher)
        {
            AppendMap(hasher, _known, ch => (int)ch);
            AppendIntMap(hasher, _attempts);
            var recruited = new List<string>(_recruited);
            recruited.Sort(StringComparer.Ordinal);
            hasher.Append(recruited.Count);
            foreach (string r in recruited) AppendString(hasher, r);
        }

        private static void AppendMap(StateHasher h, Dictionary<string, TalentChannel> map, Func<TalentChannel, int> val)
        {
            var keys = new List<string>(map.Keys);
            keys.Sort(StringComparer.Ordinal);
            h.Append(keys.Count);
            foreach (string k in keys) { AppendString(h, k); h.Append(val(map[k])); }
        }

        private static void AppendIntMap(StateHasher h, Dictionary<string, int> map)
        {
            var keys = new List<string>(map.Keys);
            keys.Sort(StringComparer.Ordinal);
            h.Append(keys.Count);
            foreach (string k in keys) { AppendString(h, k); h.Append(map[k]); }
        }

        private static void AppendString(StateHasher h, string s)
        {
            h.Append(s?.Length ?? 0);
            if (s != null) foreach (char c in s) h.Append((int)c);
        }

        /// <summary>本态确定性哈希（存读档校验）。</summary>
        public StateHash Hash()
        {
            var h = new StateHasher();
            AppendTo(h);
            return h.ToHash();
        }
    }
}
