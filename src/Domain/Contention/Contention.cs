using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Contention
{
    /// <summary>一方势力的争霸态势（GDD_017：领城数 + 存续）。不可变。存续 = 领城 &gt; 0。</summary>
    public sealed class PowerStanding
    {
        public FactionId Faction { get; }
        public int Cities { get; }
        public bool Alive => Cities > 0;

        public PowerStanding(FactionId faction, int cities)
        {
            if (cities < 0) throw new ArgumentOutOfRangeException(nameof(cities), "领城不可为负。");
            Faction = faction;
            Cities = cities;
        }
    }

    /// <summary>
    /// 群雄争霸态（GDD_017 §权威）：各主要势力领城/存续（建于 GDD_015 世界势力）。不可变；哈希、存档。
    /// </summary>
    public sealed class ContentionState
    {
        private readonly List<PowerStanding> _powers;             // 按 FactionId 规范序
        private readonly Dictionary<string, int> _cities;

        public IReadOnlyList<PowerStanding> Powers => _powers;

        public ContentionState(IReadOnlyList<PowerStanding> powers)
        {
            _cities = new Dictionary<string, int>(StringComparer.Ordinal);
            _powers = new List<PowerStanding>(powers ?? Array.Empty<PowerStanding>());
            foreach (PowerStanding p in _powers)
            {
                if (_cities.ContainsKey(p.Faction.Value)) throw new ArgumentException($"势力重复：{p.Faction.Value}");
                _cities[p.Faction.Value] = p.Cities;
            }
            _powers.Sort((a, b) => string.CompareOrdinal(a.Faction.Value, b.Faction.Value));
        }

        public int CitiesOf(FactionId f) => _cities.TryGetValue(f.Value ?? "", out int v) ? v : 0;
        public bool IsAlive(FactionId f) => CitiesOf(f) > 0;

        public int TotalCities
        {
            get { int t = 0; foreach (PowerStanding p in _powers) t += p.Cities; return t; }
        }

        /// <summary>某势力支配度（领城/天下总城；总城 0 → 0）。</summary>
        public FixedPoint Dominance(FactionId f)
        {
            int total = TotalCities;
            return total <= 0 ? FixedPoint.Zero : FixedPoint.FromFraction(CitiesOf(f), total);
        }

        /// <summary>存续（领城&gt;0）的势力。</summary>
        public IReadOnlyList<FactionId> AlivePowers()
        {
            var list = new List<FactionId>();
            foreach (PowerStanding p in _powers) if (p.Alive) list.Add(p.Faction);
            return list;
        }

        /// <summary>以某势力新领城数产出新态（其余不变）。</summary>
        public ContentionState WithCities(FactionId f, int cities)
        {
            var list = new List<PowerStanding>();
            bool found = false;
            foreach (PowerStanding p in _powers)
            {
                if (p.Faction == f) { list.Add(new PowerStanding(f, cities)); found = true; }
                else list.Add(p);
            }
            if (!found) list.Add(new PowerStanding(f, cities));
            return new ContentionState(list);
        }

        public StateHash Hash()
        {
            var h = new StateHasher();
            h.Append(_powers.Count);
            foreach (PowerStanding p in _powers)
            {
                h.Append(p.Faction.Value.Length);
                foreach (char c in p.Faction.Value) h.Append((int)c);
                h.Append(p.Cities);
            }
            return h.ToHash();
        }
    }

    /// <summary>争霸配置（GDD_017 §Balancing，数据驱动）。不可变。</summary>
    public sealed class ContentionConfig
    {
        /// <summary>兼并权重（实力差归一化 × 此 → 兼并概率）。</summary>
        public FixedPoint AnnexWeight { get; }

        public ContentionConfig(FixedPoint annexWeight) => AnnexWeight = annexWeight;

        public static ContentionConfig Default { get; } = new ContentionConfig(FixedPoint.FromFraction(8, 10));
    }

    /// <summary>
    /// 对手扩张（GDD_017 R3，种子化确定性）：非玩家势力中<b>最强兼并最弱</b>——实力差越大越易兼并（单调），
    /// 种子化确定性（可复现·非掷骰，ADR-0006）。每战略步至多一次兼并（弱者失一城、强者得一城）。纯函数。
    /// </summary>
    public sealed class RivalExpansionService
    {
        /// <summary>推进一战略步：非玩家最强势力以种子化概率兼并非玩家最弱存续势力一城。</summary>
        public ContentionState Step(ContentionState state, FactionId player, ulong seed, ContentionConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            PowerStanding? strongest = null, weakest = null;
            foreach (PowerStanding p in state.Powers)   // 已按 FactionId 规范序，确定性
            {
                if (p.Faction == player || !p.Alive) continue;
                if (strongest == null || p.Cities > strongest.Cities) strongest = p;
                if (weakest == null || p.Cities < weakest.Cities) weakest = p;
            }
            if (strongest == null || weakest == null || strongest.Faction == weakest.Faction) return state;
            if (strongest.Cities <= weakest.Cities) return state;

            int gap = strongest.Cities - weakest.Cities;
            int sum = strongest.Cities + weakest.Cities;
            FixedPoint p2 = (FixedPoint.FromFraction(gap, sum) * config.AnnexWeight).Clamp(FixedPoint.Zero, FixedPoint.One);
            bool annex = new DeterministicRandom(seed).NextUnit() < p2;
            if (!annex) return state;

            return state
                .WithCities(weakest.Faction, weakest.Cities - 1)
                .WithCities(strongest.Faction, strongest.Cities + 1);
        }

        /// <summary>
        /// 战略化推进（ADR-0013 E4.2）：<b>意图驱动</b>——只有侵略性势力（扩张/趁火/报复）出手；且强势报复/扩张者在玩家较弱时
        /// 可<b>夺玩家一城</b>（世界反击玩家·多线压力），否则夺最弱非玩家。种子化确定性、gap 概率。纯函数。
        /// </summary>
        public ContentionState StepStrategic(
            ContentionState state, FactionId player, ContentionState? prev,
            IReadOnlyCollection<string> wronged, ulong seed, ContentionConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            PowerStanding? aggressor = null;
            StrategicIntent aggIntent = StrategicIntent.Defense;
            foreach (PowerStanding p in state.Powers)
            {
                if (p.Faction == player || !p.Alive) continue;
                bool w = wronged != null && p.Faction.Value != null && Has(wronged, p.Faction.Value);
                StrategicIntent intent = FactionStrategy.Assess(state, p.Faction, player, prev, w);
                if (intent != StrategicIntent.Expansion && intent != StrategicIntent.Opportunist && intent != StrategicIntent.Revenge) continue;
                if (aggressor == null || p.Cities > aggressor.Cities) { aggressor = p; aggIntent = intent; }
            }
            if (aggressor == null) return state;   // 无势力有侵略意图 → 天下暂安

            int playerCities = state.CitiesOf(player);
            // 战略压力可夺玩家城，但<b>永不夺其最后一城</b>（>1）——灭国须真刀真枪攻城，非抽象骰子（避免磨死玩家）。
            bool pressurePlayer = playerCities > 1
                && aggressor.Cities > playerCities
                && (aggIntent == StrategicIntent.Revenge || aggressor.Cities >= playerCities * 2);

            PowerStanding? target = null;
            if (pressurePlayer) target = new PowerStanding(player, playerCities);
            else
                foreach (PowerStanding p in state.Powers)
                {
                    if (p.Faction == player || p.Faction == aggressor.Faction || !p.Alive) continue;
                    if (target == null || p.Cities < target.Cities) target = p;
                }
            if (target == null || target.Cities <= 0 || target.Cities >= aggressor.Cities) return state;

            int gap = aggressor.Cities - target.Cities;
            int sum = aggressor.Cities + target.Cities;
            FixedPoint p2 = (FixedPoint.FromFraction(gap, sum) * config.AnnexWeight).Clamp(FixedPoint.Zero, FixedPoint.One);
            if (!(new DeterministicRandom(seed).NextUnit() < p2)) return state;

            return state
                .WithCities(target.Faction, target.Cities - 1)
                .WithCities(aggressor.Faction, aggressor.Cities + 1);
        }

        private static bool Has(IReadOnlyCollection<string> set, string v)
        {
            foreach (string s in set) if (s == v) return true;
            return false;
        }
    }
}
