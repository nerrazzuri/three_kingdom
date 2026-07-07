using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 人才知晓簿持久化编解码（GDD_027 #2 / ADR-0005）：把 <see cref="TalentKnowledgeBook"/> 无损序列化为确定性文本，
    /// 供伴生存档槽持久化——反全知发觉进度跨存读档存活（此前会话内，读档重发觉）。纯 BCL、按 id 稳定序、空/null→空簿（向后兼容）。
    /// </summary>
    public static class TalentKnowledgeCodec
    {
        private const string Magic = "TALENTKNOW/1";
        private const char Sep = '\t';

        public static string Serialize(TalentKnowledgeBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            var ids = new List<string>(book.Entries.Keys);
            ids.Sort(StringComparer.Ordinal);

            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append(ids.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (string id in ids)
            {
                TalentKnowledgeBook.Entry e = book.Entries[id];
                sb.Append("T").Append(Sep).Append(id)
                  .Append(Sep).Append((int)e.Discovery)
                  .Append(Sep).Append(e.Attempts.ToString(CultureInfo.InvariantCulture))
                  .Append(Sep).Append((int)e.LastOutcome)
                  .Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        public static TalentKnowledgeBook Deserialize(string? text)
        {
            var book = new TalentKnowledgeBook();
            if (string.IsNullOrWhiteSpace(text)) return book;
            string[] lines = text!.Split('\n');
            int idx = 0;
            if (lines[idx++].Trim() != Magic) throw new FormatException("人才知晓簿魔数不符。");
            if (!int.TryParse(lines[idx++], out int count)) throw new FormatException("人才知晓簿计数非法。");

            for (int i = 0; i < count; i++)
            {
                if (idx >= lines.Length) throw new FormatException("人才知晓簿截断。");
                string[] p = lines[idx++].Split(Sep);
                if (p.Length < 5 || p[0] != "T") throw new FormatException($"人才知晓簿期望 T 行（5 字段），实得「{string.Join("|", p)}」。");
                book.Restore(new CharacterId(p[1]), (TalentKnowledge)I(p[2]), I(p[3]), (RecruitOutcome)I(p[4]));
            }
            return book;
        }

        private static int I(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }
}
