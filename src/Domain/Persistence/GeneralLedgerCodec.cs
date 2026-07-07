using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 武将运行时人生台账编解码（ADR-0017 / ADR-0005）：把 <see cref="GeneralLedger"/> 无损序列化为确定性文本，供伴生存档槽持久化。
    /// 纯 BCL、禁 Unity 序列化。行/制表符分隔、版本化头；按将 id 稳定排序 → 规范输出（同态同串）。空台账→仅头。
    /// 未来折入统一信封（ADR-0009）时复用本编解码，届时改由信封段承载。
    /// </summary>
    public sealed class GeneralLedgerCodec
    {
        private const string Magic = "GENLEDGER/1";
        private const char Sep = '\t';

        /// <summary>序列化台账为规范文本（按 id 稳定序）。</summary>
        public string Serialize(GeneralLedger ledger)
        {
            if (ledger == null) throw new ArgumentNullException(nameof(ledger));
            var ids = new List<string>(ledger.Entries.Keys);
            ids.Sort(StringComparer.Ordinal);

            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append(ids.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (string id in ids)
            {
                GeneralState s = ledger.Entries[id];
                sb.Append("G").Append(Sep).Append(id)
                  .Append(Sep).Append(s.Faction?.Value ?? "")
                  .Append(Sep).Append(s.Location?.Value ?? "")
                  .Append(Sep).Append(s.Loyalty.ToString(CultureInfo.InvariantCulture))
                  .Append(Sep).Append(((int)s.Health).ToString(CultureInfo.InvariantCulture))
                  .Append(Sep).Append(s.Fatigue.ToString(CultureInfo.InvariantCulture))
                  .Append(Sep).Append(s.CaptiveOf?.Value ?? "")
                  .Append(Sep).Append(s.Memories.Count.ToString(CultureInfo.InvariantCulture))
                  .Append('\n');
                foreach (MemoryEvent m in s.Memories)
                    sb.Append("M").Append(Sep).Append(((int)m.Kind).ToString(CultureInfo.InvariantCulture))
                      .Append(Sep).Append(m.Counterpart.Value ?? "")
                      .Append(Sep).Append(m.Year.ToString(CultureInfo.InvariantCulture))
                      .Append(Sep).Append(m.Weight.ToString(CultureInfo.InvariantCulture))
                      .Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>反序列化文本为台账；空/null 文本 → 空台账（向后兼容无此段的旧存档）。</summary>
        public GeneralLedger Deserialize(string? text)
        {
            var ledger = new GeneralLedger();
            if (string.IsNullOrWhiteSpace(text)) return ledger;
            string[] lines = text!.Split('\n');
            int idx = 0;
            if (idx >= lines.Length || lines[idx++].Trim() != Magic)
                throw new SaveFormatException("武将台账魔数不符。");
            if (idx >= lines.Length || !int.TryParse(lines[idx++], out int count))
                throw new SaveFormatException("武将台账计数非法。");

            for (int i = 0; i < count; i++)
            {
                string[] g = Row(lines, ref idx, "G", 9);
                var id = new CharacterId(g[1]);
                FactionId? faction = g[2].Length == 0 ? (FactionId?)null : new FactionId(g[2]);
                CityId? city = g[3].Length == 0 ? (CityId?)null : new CityId(g[3]);
                int loyalty = ParseInt(g[4]);
                var health = (GeneralHealth)ParseInt(g[5]);
                int fatigue = ParseInt(g[6]);
                FactionId? captive = g[7].Length == 0 ? (FactionId?)null : new FactionId(g[7]);
                int memCount = ParseInt(g[8]);

                var mems = new List<MemoryEvent>(memCount);
                for (int j = 0; j < memCount; j++)
                {
                    string[] m = Row(lines, ref idx, "M", 5);
                    mems.Add(new MemoryEvent((MemoryKind)ParseInt(m[1]), new CharacterId(m[2]), ParseInt(m[3]), ParseInt(m[4])));
                }
                ledger.Set(new GeneralState(id, faction, city, loyalty, health, fatigue, captive, mems));
            }
            return ledger;
        }

        private static string[] Row(string[] lines, ref int idx, string tag, int minFields)
        {
            if (idx >= lines.Length) throw new SaveFormatException("武将台账截断。");
            string[] parts = lines[idx++].Split(Sep);
            if (parts.Length < minFields || parts[0] != tag)
                throw new SaveFormatException($"武将台账期望「{tag}」行（{minFields} 字段），实得「{string.Join("|", parts)}」。");
            return parts;
        }

        private static int ParseInt(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }
}
