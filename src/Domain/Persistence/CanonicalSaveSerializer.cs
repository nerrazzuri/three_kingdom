using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Configuration;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// <see cref="ISaveSerializer"/> 的参考实现（ADR-0005）：纯 BCL、确定性、行/制表符分隔的版本化 DTO 文本编解码。
    /// <b>不</b>使用任何 Unity 序列化。字符串字段以反斜杠转义（<c>\\ \t \n</c>）避免分隔符歧义；
    /// 随机流按名、真值/知识按键<b>序数升序</b>写入（同内容 → 同文本）。
    /// 量产期可由 Infrastructure 的 JSON 库实现替换，契约不变。
    /// </summary>
    public sealed class CanonicalSaveSerializer : ISaveSerializer
    {
        private const string Magic = "TKSAVE/1";
        private const char Sep = '\t';

        /// <inheritdoc/>
        public string Serialize(SaveSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append("version").Append(Sep).Append(snapshot.Version.ToString()).Append('\n');
            sb.Append("fingerprint").Append(Sep).Append(snapshot.Fingerprint.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');

            var streams = new List<RngStreamState>(snapshot.RngStreams);
            streams.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            sb.Append("rng").Append(Sep).Append(streams.Count).Append('\n');
            foreach (var s in streams)
                sb.Append("r").Append(Sep).Append(Escape(s.Name)).Append(Sep)
                  .Append(s.Seed.ToString(CultureInfo.InvariantCulture)).Append(Sep)
                  .Append(s.Position.ToString(CultureInfo.InvariantCulture)).Append('\n');

            AppendMap(sb, "truth", "t", snapshot.WorldTruth);
            AppendMap(sb, "knowledge", "k", snapshot.FactionKnowledge);
            return sb.ToString();
        }

        /// <inheritdoc/>
        public SaveSnapshot Deserialize(string text)
        {
            if (text == null) throw new SaveFormatException("存档文本为 null。");
            var lines = text.Split('\n');
            int idx = 0;

            string Header() => idx < lines.Length ? lines[idx++] : throw new SaveFormatException("存档截断（缺行）。");

            if (Header() != Magic) throw new SaveFormatException("存档魔数/版本头不符。");

            var version = SaveVersion.Parse(Field("version", Header()));
            var fpText = Field("fingerprint", Header());
            if (!ulong.TryParse(fpText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong fpVal))
                throw new SaveFormatException("配置指纹字段非法。");
            var fingerprint = new ConfigFingerprint(fpVal);

            int rngCount = ParseCount(Field("rng", Header()));
            var streams = new List<RngStreamState>(rngCount);
            for (int i = 0; i < rngCount; i++)
            {
                var parts = Header().Split(Sep);
                if (parts.Length != 4 || parts[0] != "r") throw new SaveFormatException("随机流记录格式错。");
                if (!ulong.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed) ||
                    !ulong.TryParse(parts[3], NumberStyles.None, CultureInfo.InvariantCulture, out ulong pos))
                    throw new SaveFormatException("随机流 seed/position 非法。");
                streams.Add(new RngStreamState(Unescape(parts[1]), seed, pos));
            }

            var truth = ReadMap("truth", "t", lines, ref idx);
            var knowledge = ReadMap("knowledge", "k", lines, ref idx);

            return new SaveSnapshot(version, fingerprint, streams, truth, knowledge);
        }

        private static void AppendMap(StringBuilder sb, string header, string tag, IReadOnlyDictionary<string, long> map)
        {
            var keys = new List<string>(map.Keys);
            keys.Sort(StringComparer.Ordinal);
            sb.Append(header).Append(Sep).Append(keys.Count).Append('\n');
            foreach (var k in keys)
                sb.Append(tag).Append(Sep).Append(Escape(k)).Append(Sep)
                  .Append(map[k].ToString(CultureInfo.InvariantCulture)).Append('\n');
        }

        private static Dictionary<string, long> ReadMap(string header, string tag, string[] lines, ref int idx)
        {
            if (idx >= lines.Length) throw new SaveFormatException($"存档截断（缺 {header} 段）。");
            int count = ParseCount(Field(header, lines[idx++]));
            var map = new Dictionary<string, long>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                if (idx >= lines.Length) throw new SaveFormatException($"{header} 段截断。");
                var parts = lines[idx++].Split(Sep);
                if (parts.Length != 3 || parts[0] != tag) throw new SaveFormatException($"{header} 记录格式错。");
                if (!long.TryParse(parts[2], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long v))
                    throw new SaveFormatException($"{header} 值非法。");
                map[Unescape(parts[1])] = v;
            }
            return map;
        }

        private static string Field(string name, string line)
        {
            int sep = line.IndexOf(Sep);
            if (sep < 0 || line.Substring(0, sep) != name) throw new SaveFormatException($"字段「{name}」缺失或错位。");
            return line.Substring(sep + 1);
        }

        private static int ParseCount(string s)
        {
            if (!int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out int n))
                throw new SaveFormatException("计数字段非法。");
            return n;
        }

        private static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\n': sb.Append("\\n"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private static string Unescape(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != '\\') { sb.Append(c); continue; }
                if (++i >= s.Length) throw new SaveFormatException("转义序列截断。");
                switch (s[i])
                {
                    case '\\': sb.Append('\\'); break;
                    case 't': sb.Append('\t'); break;
                    case 'n': sb.Append('\n'); break;
                    default: throw new SaveFormatException("未知转义序列。");
                }
            }
            return sb.ToString();
        }
    }
}
