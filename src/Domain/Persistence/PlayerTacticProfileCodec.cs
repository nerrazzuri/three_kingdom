using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>玩家战术倾向档持久化（ADR-0013 E3 / ADR-0005）：路线计数 + 末次 + 连击无损 round-trip；空/null→空档（向后兼容）。</summary>
    public sealed class PlayerTacticProfileCodec
    {
        private const string Magic = "PLAYERTACTIC/1";
        private const char Sep = '\t';

        public string Serialize(PlayerTacticProfile p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            var keys = new List<int>(p.Counts.Keys);
            keys.Sort();
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append(p.LastApproach.ToString(CultureInfo.InvariantCulture)).Append(Sep)
              .Append(p.Streak.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append(keys.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (int k in keys)
                sb.Append("A").Append(Sep).Append(k).Append(Sep).Append(p.Counts[k]).Append('\n');
            return sb.ToString().TrimEnd('\n');
        }

        public PlayerTacticProfile Deserialize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return PlayerTacticProfile.Empty;
            string[] lines = text!.Split('\n');
            int idx = 0;
            if (lines[idx++].Trim() != Magic) throw new SaveFormatException("玩家战术档魔数不符。");
            string[] head = lines[idx++].Split(Sep);
            if (head.Length < 2) throw new SaveFormatException("玩家战术档头非法。");
            int last = I(head[0]); int streak = I(head[1]);
            if (!int.TryParse(lines[idx++], out int count)) throw new SaveFormatException("玩家战术档计数非法。");

            var counts = new Dictionary<int, int>();
            for (int i = 0; i < count; i++)
            {
                if (idx >= lines.Length) throw new SaveFormatException("玩家战术档截断。");
                string[] p = lines[idx++].Split(Sep);
                if (p.Length < 3 || p[0] != "A") throw new SaveFormatException($"玩家战术档期望 A 行，实得「{string.Join("|", p)}」。");
                counts[I(p[1])] = I(p[2]);
            }
            return new PlayerTacticProfile(counts, last, streak);
        }

        private static int I(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }
}
