using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 战役存档统一信封编解码（GDD_013 / ADR-0005 + ADR-0002）。纯 BCL、确定性、禁 Unity 序列化。
    /// 信封头（魔数 + 统一 version + fingerprint）<b>一处校验</b>；段间以哨兵行分隔，各段文本委派既有
    /// <see cref="CareerSaveCodec"/> / <see cref="WorldSaveCodec"/>（复用，不重写）。版本不兼容或指纹不符 →
    /// 整体抛 <see cref="SaveFormatException"/>，<b>不部分载入</b>（生涯段与世界段全有或全无）。
    /// </summary>
    public sealed class CampaignSaveCodec
    {
        private const string Magic = "TKCAMPAIGN/1";
        private const char Sep = '\t';
        private const string CareerMarker = "--SEG:career--";
        private const string WorldMarker = "--SEG:world--";

        private readonly CareerSaveCodec _careerCodec = new CareerSaveCodec();
        private readonly WorldSaveCodec _worldCodec = new WorldSaveCodec();

        /// <summary>序列化统一信封。</summary>
        public string Serialize(CampaignSaveState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append("version").Append(Sep).Append(state.Version.ToString()).Append('\n');
            sb.Append("fingerprint").Append(Sep).Append(state.Fingerprint.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append(CareerMarker).Append('\n');
            sb.Append(_careerCodec.Serialize(state.Career)).Append('\n');
            sb.Append(WorldMarker).Append('\n');
            sb.Append(_worldCodec.Serialize(state.World));
            return sb.ToString();
        }

        /// <summary>
        /// 反序列化并校验信封版本/指纹（一处）；再以同一 version/fingerprint 委派各段解码（各段二次校验，纵深防御）。
        /// </summary>
        public CampaignSaveState Deserialize(string text, SaveVersion currentVersion, ConfigFingerprint expectedFingerprint)
        {
            if (text is null) throw new SaveFormatException("战役存档文本为 null。");
            string[] lines = text.Split('\n');
            int idx = 0;
            string Next() => idx < lines.Length ? lines[idx++] : throw new SaveFormatException("战役存档截断（缺行）。");

            if (Next() != Magic) throw new SaveFormatException("战役存档魔数不符。");

            SaveVersion version = SaveVersion.Parse(Field("version", Next()));
            if (!version.CanLoadInto(currentVersion))
                throw new SaveFormatException($"战役存档版本 {version} 高于当前 {currentVersion}，拒绝载入。");

            if (!ulong.TryParse(Field("fingerprint", Next()), NumberStyles.None, CultureInfo.InvariantCulture, out ulong fpVal))
                throw new SaveFormatException("配置指纹字段非法。");
            var fingerprint = new ConfigFingerprint(fpVal);
            if (fingerprint != expectedFingerprint)
                throw new SaveFormatException("配置指纹不符，拒绝载入。");

            if (Next() != CareerMarker) throw new SaveFormatException("缺生涯段标记。");
            string careerText = CollectUntil(lines, ref idx, WorldMarker);
            string worldText = CollectRemaining(lines, idx);

            // 委派各段解码（各段头部 version/fingerprint 与信封一致，二次校验通过）。
            CareerSaveState career = _careerCodec.Deserialize(careerText, currentVersion, expectedFingerprint);
            WorldSaveState world = _worldCodec.Deserialize(worldText, currentVersion, expectedFingerprint);

            return new CampaignSaveState(version, fingerprint, career, world);
        }

        private static string CollectUntil(string[] lines, ref int idx, string marker)
        {
            var seg = new List<string>();
            while (idx < lines.Length && lines[idx] != marker) seg.Add(lines[idx++]);
            if (idx >= lines.Length) throw new SaveFormatException($"缺段标记：{marker}。");
            idx++; // 跳过 marker
            return string.Join("\n", seg);
        }

        private static string CollectRemaining(string[] lines, int idx)
        {
            var seg = new List<string>();
            while (idx < lines.Length) seg.Add(lines[idx++]);
            return string.Join("\n", seg);
        }

        private static string Field(string key, string line)
        {
            string[] parts = line.Split(Sep);
            if (parts.Length < 2 || parts[0] != key) throw new SaveFormatException($"期望字段「{key}」，实得「{line}」。");
            return parts[1];
        }
    }
}
