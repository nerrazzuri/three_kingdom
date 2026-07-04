using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Theater
{
    /// <summary>城池治理方式（GDD M12）：亲管 或 委任下属打理。</summary>
    public enum GovernanceMode
    {
        SelfGoverned = 0,   // 玩家亲管
        Delegated = 1,      // 委任下属（AI 打理，玩家只碰关键决策）
    }

    /// <summary>一座直辖城的持有信息（治理方式 + 受任下属）。不可变。</summary>
    public sealed class CityHolding
    {
        public CityId City { get; }
        public GovernanceMode Mode { get; }
        /// <summary>受任下属（Delegated 时非空——来自僚属/招揽人才）。</summary>
        public CharacterId? Governor { get; }

        public CityHolding(CityId city, GovernanceMode mode, CharacterId? governor)
        {
            if (mode == GovernanceMode.Delegated && governor == null)
                throw new ArgumentException("委任治理须指定受任下属。", nameof(governor));
            City = city;
            Mode = mode;
            Governor = governor;
        }

        internal void AppendTo(StateHasher h)
        {
            AppendString(h, City.Value);
            h.Append((int)Mode);
            h.Append(Governor != null);
            if (Governor != null) AppendString(h, Governor.Value.Value);
        }

        private static void AppendString(StateHasher h, string s)
        {
            h.Append(s?.Length ?? 0);
            if (s != null) foreach (char c in s) h.Append((int)c);
        }
    }

    /// <summary>
    /// 多城战区态（GDD M12 §权威）：玩家直辖诸城 + 各城治理方式（亲管/委任）。不可变；纳入存档、确定性哈希。
    /// 占城 C（epic-029）产出的城进此态；掌管范围受官阶约束（<see cref="SpanOfControlConfig"/>）。
    /// </summary>
    public sealed class TheaterState
    {
        private readonly List<CityHolding> _holdings;             // 按 CityId 规范序
        private readonly Dictionary<string, CityHolding> _byId;

        public IReadOnlyList<CityHolding> Holdings => _holdings;

        public TheaterState(IReadOnlyList<CityHolding> holdings)
        {
            _byId = new Dictionary<string, CityHolding>(StringComparer.Ordinal);
            _holdings = new List<CityHolding>(holdings ?? Array.Empty<CityHolding>());
            foreach (CityHolding h in _holdings)
            {
                if (_byId.ContainsKey(h.City.Value)) throw new ArgumentException($"城池重复持有：{h.City.Value}");
                _byId[h.City.Value] = h;
            }
            _holdings.Sort((a, b) => string.CompareOrdinal(a.City.Value, b.City.Value));
        }

        public static TheaterState Empty { get; } = new TheaterState(Array.Empty<CityHolding>());

        public bool Holds(CityId city) => _byId.ContainsKey(city.Value ?? "");
        public CityHolding? Of(CityId city) => _byId.TryGetValue(city.Value ?? "", out CityHolding? h) ? h : null;
        public int Count => _holdings.Count;

        public int SelfGovernedCount => CountMode(GovernanceMode.SelfGoverned);
        public int DelegatedCount => CountMode(GovernanceMode.Delegated);

        private int CountMode(GovernanceMode m)
        {
            int n = 0;
            foreach (CityHolding h in _holdings) if (h.Mode == m) n++;
            return n;
        }

        /// <summary>纳入一座新直辖城（占城 C 产出），默认亲管。已持有则原样返回。</summary>
        public TheaterState AddCity(CityId city)
        {
            if (Holds(city)) return this;
            var list = new List<CityHolding>(_holdings) { new CityHolding(city, GovernanceMode.SelfGoverned, null) };
            return new TheaterState(list);
        }

        /// <summary>委任某城给下属打理。城须已持有。</summary>
        public TheaterState Delegate(CityId city, CharacterId governor)
            => Replace(city, new CityHolding(city, GovernanceMode.Delegated, governor));

        /// <summary>收回某城亲管。城须已持有。</summary>
        public TheaterState Reclaim(CityId city)
            => Replace(city, new CityHolding(city, GovernanceMode.SelfGoverned, null));

        private TheaterState Replace(CityId city, CityHolding updated)
        {
            if (!Holds(city)) throw new InvalidOperationException($"未持有该城：{city}");
            var list = new List<CityHolding>();
            foreach (CityHolding h in _holdings) list.Add(h.City == city ? updated : h);
            return new TheaterState(list);
        }

        public StateHash Hash()
        {
            var h = new StateHasher();
            h.Append(_holdings.Count);
            foreach (CityHolding c in _holdings) c.AppendTo(h);
            return h.ToHash();
        }
    }

    /// <summary>
    /// 掌管范围配置（GDD M12：掌管范围随官阶 Rank 0–7）：每官阶可<b>亲管</b>城数上限；超出须委任（或升官）。
    /// 数据驱动。不可变。
    /// </summary>
    public sealed class SpanOfControlConfig
    {
        private readonly int[] _maxSelfByRank;   // 索引=Rank

        public SpanOfControlConfig(IReadOnlyList<int> maxSelfByRank)
        {
            if (maxSelfByRank == null || maxSelfByRank.Count == 0) throw new ArgumentException("须给各官阶上限。", nameof(maxSelfByRank));
            _maxSelfByRank = new int[maxSelfByRank.Count];
            for (int i = 0; i < maxSelfByRank.Count; i++)
            {
                if (maxSelfByRank[i] < 1) throw new ArgumentOutOfRangeException(nameof(maxSelfByRank), "亲管上限须 ≥1。");
                _maxSelfByRank[i] = maxSelfByRank[i];
            }
        }

        /// <summary>某官阶的亲管城数上限（越阶取最高档）。</summary>
        public int MaxSelfGoverned(int rank)
        {
            if (rank < 0) rank = 0;
            if (rank >= _maxSelfByRank.Length) rank = _maxSelfByRank.Length - 1;
            return _maxSelfByRank[rank];
        }

        /// <summary>默认：阶0 亲管1城，逐阶放宽到阶7 亲管8城（余城靠委任）。</summary>
        public static SpanOfControlConfig Default { get; } = new SpanOfControlConfig(new[] { 1, 1, 2, 3, 4, 5, 6, 8 });
    }
}
