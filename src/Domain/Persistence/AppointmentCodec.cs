using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Appointment;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 太守任用簿持久化编解码（GDD_027 P3 / ADR-0005）：把 <see cref="AppointmentBook"/>（城→已调拨将 + 城册上限）
    /// 无损序列化为确定性文本，供伴生存档槽持久化——任用态此前会话内，本编解码使其跨存读档存活。
    /// 纯 BCL、按城 id 稳定序、空/null→空簿（向后兼容）。重建经 <see cref="AppointmentBook.Assign"/>（数据合法即等价）。
    /// </summary>
    public sealed class AppointmentCodec
    {
        private const string Magic = "APPOINT/1";
        private const char Sep = '\t';

        public string Serialize(AppointmentBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append(book.CityCap.ToString(CultureInfo.InvariantCulture)).Append('\n');
            IReadOnlyList<string> cities = book.Cities();
            sb.Append(cities.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (string city in cities)
                sb.Append("C").Append(Sep).Append(city).Append(Sep)
                  .Append(string.Join(",", book.Roster(new CityId(city)))).Append('\n');
            return sb.ToString().TrimEnd('\n');
        }

        public AppointmentBook Deserialize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return AppointmentBook.Empty();
            string[] lines = text!.Split('\n');
            int idx = 0;
            if (lines[idx++].Trim() != Magic) throw new SaveFormatException("任用簿魔数不符。");
            if (!int.TryParse(lines[idx++], out int cap)) throw new SaveFormatException("任用簿上限非法。");
            if (!int.TryParse(lines[idx++], out int cityCount)) throw new SaveFormatException("任用簿城数非法。");

            AppointmentBook book = AppointmentBook.Empty(cap);
            for (int i = 0; i < cityCount; i++)
            {
                if (idx >= lines.Length) throw new SaveFormatException("任用簿截断。");
                string[] p = lines[idx++].Split(Sep);
                if (p.Length < 3 || p[0] != "C") throw new SaveFormatException($"任用簿期望 C 行，实得「{string.Join("|", p)}」。");
                var city = new CityId(p[1]);
                if (p[2].Length == 0) continue;
                foreach (string gid in p[2].Split(','))
                    book = book.Assign(city, new CharacterId(gid)).Book;   // 重建（合法数据等价）
            }
            return book;
        }
    }
}
