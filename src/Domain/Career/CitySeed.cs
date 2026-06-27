using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 太守开局禀赋配置（GDD_014 §Main Rules 开局；TR-career-001 / ADR-0003）。不可变；构造校验范围，非法即抛。
    /// <para>
    /// <b>跨 epic 占位</b>：CitySeed 权威终将在 epic-012 世界模型（GDD_015）。本 story 以最小配置占位、以
    /// 配置接口隔离，待世界模型落地后切换来源——开局禀赋数值<b>不硬编码</b>（来自此配置）。
    /// </para>
    /// </summary>
    public sealed class CitySeed
    {
        private readonly RetinueMember[] _retinue;

        /// <summary>所属君主势力。</summary>
        public FactionId Faction { get; }

        /// <summary>开局城池。</summary>
        public CityId City { get; }

        /// <summary>开局守备（≥0）。</summary>
        public int Garrison { get; }

        /// <summary>开局城防/工事（≥0）。</summary>
        public int Fortification { get; }

        /// <summary>开局产出（≥0）。</summary>
        public int Output { get; }

        /// <summary>开局核心部曲/僚属（含好感）。</summary>
        public IReadOnlyList<RetinueMember> Retinue => _retinue;

        public CitySeed(
            FactionId faction, CityId city, int garrison, int fortification, int output,
            IReadOnlyList<RetinueMember> retinue)
        {
            if (garrison < 0) throw new ArgumentOutOfRangeException(nameof(garrison), "守备不可为负。");
            if (fortification < 0) throw new ArgumentOutOfRangeException(nameof(fortification), "城防不可为负。");
            if (output < 0) throw new ArgumentOutOfRangeException(nameof(output), "产出不可为负。");
            if (retinue is null) throw new ArgumentNullException(nameof(retinue), "开局部曲列表不可缺失。");

            var arr = new RetinueMember[retinue.Count];
            for (int i = 0; i < retinue.Count; i++)
                arr[i] = retinue[i] ?? throw new ArgumentException("开局部曲不可含 null。", nameof(retinue));
            _retinue = arr;

            Faction = faction;
            City = city;
            Garrison = garrison;
            Fortification = fortification;
            Output = output;
        }
    }
}
