using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 世界存档段的版本化 DTO 编解码（ADR-0005 + ADR-0002）。纯 BCL、确定性、制表符分隔文本；<b>禁</b> Unity 序列化。
    /// Serialize 同态→同文本；Deserialize(Serialize(s)) 还原等价世界态（含 diverged 标志）。
    /// 载入校验 schema 版本不兼容（更新）或配置指纹不符 → 抛 <see cref="SaveFormatException"/>，<b>不部分载入</b>。
    /// </summary>
    public sealed class WorldSaveCodec
    {
        private const string Magic = "TKWORLD/1";
        private const char Sep = '\t';

        public string Serialize(WorldSaveState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append("version").Append(Sep).Append(state.Version.ToString()).Append('\n');
            sb.Append("fingerprint").Append(Sep).Append(state.Fingerprint.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');

            WorldState w = state.World;
            sb.Append("time").Append(Sep).Append(w.CurrentTime.Day).Append(Sep).Append((int)w.CurrentTime.Segment).Append('\n');

            sb.Append("factions").Append(Sep).Append(w.Factions.Count).Append('\n');
            foreach (FactionRecord f in w.Factions)
            {
                sb.Append("f").Append(Sep).Append(Escape(f.Id.Value)).Append(Sep)
                  .Append(f.Lord.HasValue ? 1 : 0).Append(Sep)
                  .Append(f.Lord.HasValue ? Escape(f.Lord.Value.Value) : string.Empty).Append(Sep)
                  .Append((int)f.Survival).Append(Sep).Append((int)f.Relation).Append(Sep)
                  .Append(f.OwnedCities.Count).Append('\n');
                foreach (CityId c in f.OwnedCities)
                    sb.Append("fo").Append(Sep).Append(Escape(c.Value)).Append('\n');
            }

            sb.Append("cities").Append(Sep).Append(w.Cities.Count).Append('\n');
            foreach (CityOwnership c in w.Cities)
                sb.Append("c").Append(Sep).Append(Escape(c.City.Value)).Append(Sep)
                  .Append(c.Owner.HasValue ? 1 : 0).Append(Sep)
                  .Append(c.Owner.HasValue ? Escape(c.Owner.Value.Value) : string.Empty).Append(Sep)
                  .Append(c.Garrison).Append('\n');

            AppendStringList(sb, "triggered", "tg", w.TriggeredEvents);
            AppendStringList(sb, "diverged", "dv", w.DivergedEvents);
            return sb.ToString();
        }

        public WorldSaveState Deserialize(string text, SaveVersion currentVersion, ConfigFingerprint expectedFingerprint)
        {
            if (text is null) throw new SaveFormatException("世界存档文本为 null。");
            string[] lines = text.Split('\n');
            int idx = 0;
            string Next() => idx < lines.Length ? lines[idx++] : throw new SaveFormatException("世界存档截断（缺行）。");

            if (Next() != Magic) throw new SaveFormatException("世界存档魔数不符。");

            SaveVersion version = SaveVersion.Parse(Field("version", Next()));
            if (!version.CanLoadInto(currentVersion))
                throw new SaveFormatException($"世界存档版本 {version} 高于当前 {currentVersion}，拒绝载入（不静默降级）。");

            if (!ulong.TryParse(Field("fingerprint", Next()), NumberStyles.None, CultureInfo.InvariantCulture, out ulong fpVal))
                throw new SaveFormatException("配置指纹字段非法。");
            var fingerprint = new ConfigFingerprint(fpVal);
            if (fingerprint != expectedFingerprint)
                throw new SaveFormatException("配置指纹不符，拒绝载入。");

            string[] timeParts = Next().Split(Sep);
            if (timeParts.Length != 3 || timeParts[0] != "time") throw new SaveFormatException("time 段格式错。");
            var time = new WorldTime(ParseInt(timeParts[1]), (DaySegment)ParseInt(timeParts[2]));

            int factionCount = ParseCount(Field("factions", Next()));
            var factions = new List<FactionRecord>(factionCount);
            for (int i = 0; i < factionCount; i++)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 7 || p[0] != "f") throw new SaveFormatException("faction 段格式错。");
                var id = new FactionId(Unescape(p[1]));
                CharacterId? lord = ParseInt(p[2]) != 0 ? new CharacterId(Unescape(p[3])) : (CharacterId?)null;
                var survival = (SurvivalStatus)ParseInt(p[4]);
                var relation = (RelationToPlayer)ParseInt(p[5]);
                int ownedCount = ParseCount(p[6]);
                var owned = new List<CityId>(ownedCount);
                for (int j = 0; j < ownedCount; j++)
                {
                    string[] op = Next().Split(Sep);
                    if (op.Length != 2 || op[0] != "fo") throw new SaveFormatException("faction 领有城格式错。");
                    owned.Add(new CityId(Unescape(op[1])));
                }
                factions.Add(new FactionRecord(id, lord, survival, relation, owned));
            }

            int cityCount = ParseCount(Field("cities", Next()));
            var cities = new List<CityOwnership>(cityCount);
            for (int i = 0; i < cityCount; i++)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 5 || p[0] != "c") throw new SaveFormatException("city 段格式错。");
                FactionId? owner = ParseInt(p[2]) != 0 ? new FactionId(Unescape(p[3])) : (FactionId?)null;
                cities.Add(new CityOwnership(new CityId(Unescape(p[1])), owner, ParseInt(p[4])));
            }

            List<string> triggered = ReadStringList("triggered", "tg", Next, Next());
            List<string> diverged = ReadStringList("diverged", "dv", Next, Next());

            var world = new WorldState(time, factions, cities, triggered, diverged);
            return new WorldSaveState(version, fingerprint, world);
        }

        private static List<string> ReadStringList(string key, string tag, Func<string> next, string headerLine)
        {
            int count = ParseCount(Field(key, headerLine));
            var list = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                string[] p = next().Split(Sep);
                if (p.Length != 2 || p[0] != tag) throw new SaveFormatException($"{key} 记录格式错。");
                list.Add(Unescape(p[1]));
            }
            return list;
        }

        private static void AppendStringList(StringBuilder sb, string key, string tag, IReadOnlyList<string> items)
        {
            sb.Append(key).Append(Sep).Append(items.Count).Append('\n');
            foreach (string s in items) sb.Append(tag).Append(Sep).Append(Escape(s)).Append('\n');
        }

        private static string Field(string key, string line)
        {
            string[] parts = line.Split(Sep);
            if (parts.Length < 2 || parts[0] != key) throw new SaveFormatException($"期望字段「{key}」，实得「{line}」。");
            return parts[1];
        }

        private static int ParseInt(string s)
        {
            if (!int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int v))
                throw new SaveFormatException($"整数字段非法：「{s}」。");
            return v;
        }

        private static int ParseCount(string s)
        {
            int v = ParseInt(s);
            if (v < 0) throw new SaveFormatException("计数不可为负。");
            return v;
        }

        private static string Escape(string s)
            => s.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n");

        private static string Unescape(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    sb.Append(n == 't' ? '\t' : n == 'n' ? '\n' : n);
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
