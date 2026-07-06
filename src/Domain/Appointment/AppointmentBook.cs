using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Appointment
{
    /// <summary>调拨结果码（GDD_027 P3 R4）。</summary>
    public enum AppointResult { Ok, CityFull, AlreadyThere, NotFound }

    /// <summary>
    /// 太守任用簿（GDD_027 P3 / ADR-0016）：玩家把已招武将调拨入各城武将册（每城 ≤ 上限）。不可变——每次调拨返回新态。
    /// 一将同时只在一城（调入他城自动从原城移出）。消费方（内政/军师/战斗）从城册按角色点用。纳入存档（权威态）。
    /// </summary>
    public sealed class AppointmentBook
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _byCity;
        /// <summary>每城册上限（GDD_027 R2）。</summary>
        public int CityCap { get; }

        public static AppointmentBook Empty(int cityCap = 20)
            => new AppointmentBook(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal), cityCap);

        private AppointmentBook(IReadOnlyDictionary<string, IReadOnlyList<string>> byCity, int cityCap)
        {
            _byCity = byCity; CityCap = cityCap;
        }

        /// <summary>某城已调拨武将（稳定序）；空城返回空。</summary>
        public IReadOnlyList<string> Roster(CityId city)
            => city.Value != null && _byCity.TryGetValue(city.Value, out IReadOnlyList<string>? r) ? r : System.Array.Empty<string>();

        /// <summary>该将当前所在城（未调拨则 null）。</summary>
        public string? CityOf(CharacterId general)
        {
            if (general.Value == null) return null;
            foreach (KeyValuePair<string, IReadOnlyList<string>> kv in _byCity)
                foreach (string id in kv.Value)
                    if (id == general.Value) return kv.Key;
            return null;
        }

        /// <summary>调拨某将入某城（GDD_027 R4）：满员拒；已在该城拒；在他城则先移出。返回 (结果, 新态)。</summary>
        public (AppointResult Result, AppointmentBook Book) Assign(CityId city, CharacterId general)
        {
            if (city.Value == null || general.Value == null) return (AppointResult.NotFound, this);
            string? cur = CityOf(general);
            if (cur == city.Value) return (AppointResult.AlreadyThere, this);

            var next = Clone();
            if (cur != null) RemoveFrom(next, cur, general.Value);   // 从原城移出

            List<string> roster = next.TryGetValue(city.Value, out IReadOnlyList<string>? r) ? new List<string>(r) : new List<string>();
            if (roster.Count >= CityCap) return (AppointResult.CityFull, this);
            roster.Add(general.Value);
            roster.Sort(StringComparer.Ordinal);
            next[city.Value] = roster;
            return (AppointResult.Ok, new AppointmentBook(next, CityCap));
        }

        /// <summary>撤某将出其所在城。返回 (结果, 新态)。</summary>
        public (AppointResult Result, AppointmentBook Book) Remove(CharacterId general)
        {
            if (general.Value == null) return (AppointResult.NotFound, this);
            string? cur = CityOf(general);
            if (cur == null) return (AppointResult.NotFound, this);
            var next = Clone();
            RemoveFrom(next, cur, general.Value);
            return (AppointResult.Ok, new AppointmentBook(next, CityCap));
        }

        /// <summary>全部已调拨城（稳定序）——供存档/遍历。</summary>
        public IReadOnlyList<string> Cities()
        {
            var keys = new List<string>(_byCity.Keys);
            keys.Sort(StringComparer.Ordinal);
            return keys;
        }

        private Dictionary<string, IReadOnlyList<string>> Clone()
            => new Dictionary<string, IReadOnlyList<string>>(_byCity, StringComparer.Ordinal);

        private static void RemoveFrom(Dictionary<string, IReadOnlyList<string>> map, string city, string general)
        {
            if (!map.TryGetValue(city, out IReadOnlyList<string>? r)) return;
            var roster = new List<string>(r);
            roster.Remove(general);
            if (roster.Count == 0) map.Remove(city);
            else map[city] = roster;
        }
    }
}
