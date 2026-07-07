using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>外部化的一条武将档案记录（GDD_027 #5 数据外部化）：解析自外部数据文本，字段与 <see cref="GeneralDossier"/> + 生卒对齐。</summary>
    public readonly struct GeneralDossierRecord
    {
        public string Id { get; }
        public CombatTier Prowess { get; }
        public StrategyTier Strategy { get; }
        public LoyaltyLeaning Leaning { get; }
        public Ambition Ambition { get; }
        public EraStage Stage { get; }
        public int? Birth { get; }
        public int? Death { get; }
        public IReadOnlyList<GeneralTag> Tags { get; }

        public GeneralDossierRecord(string id, CombatTier prowess, StrategyTier strategy, LoyaltyLeaning leaning,
            Ambition ambition, EraStage stage, int? birth, int? death, IReadOnlyList<GeneralTag> tags)
        {
            Id = id; Prowess = prowess; Strategy = strategy; Leaning = leaning; Ambition = ambition;
            Stage = stage; Birth = birth; Death = death; Tags = tags;
        }
    }

    /// <summary>
    /// 武将档案外部化编解码（GDD_027 #5 / ADR-0016）：把权威硬编码档案<b>导出</b>为确定性外部数据文本、并可<b>解析</b>回记录。
    /// 外部文件由本导出<b>生成</b>（非手工转写）→ 零转写误差（不重蹈 char-jiling 类静默错）；<see cref="GeneralDataValidation"/>
    /// 承接加载校验。纯 BCL、行/制表符分隔、按 id 稳定序。<b>本阶段建管线 + 生成外部文件 + round-trip 证等价</b>；
    /// 将运行期权威源切到外部文件（须随构建/Unity 资源装载）为后续 ★编辑器步。
    /// </summary>
    public static class GeneralDossierCodec
    {
        private const string Magic = "GENDOSSIER/1";
        private const char Sep = '\t';

        /// <summary>从当前权威档案（<see cref="GeneralDossiers.All"/> + 生卒）导出规范外部数据文本。</summary>
        public static string Export()
        {
            var rows = new List<GeneralDossier>(GeneralDossiers.All);
            rows.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));

            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append(rows.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (GeneralDossier d in rows)
            {
                (int Birth, int Death)? life = GeneralDossiers.LifeOf(d.Id);
                var tagInts = new List<string>();
                foreach (GeneralTag t in d.Tags) tagInts.Add(((int)t).ToString(CultureInfo.InvariantCulture));
                tagInts.Sort(StringComparer.Ordinal);
                sb.Append("D").Append(Sep).Append(d.Id.Value)
                  .Append(Sep).Append((int)d.Prowess)
                  .Append(Sep).Append((int)d.Strategy)
                  .Append(Sep).Append((int)d.Leaning)
                  .Append(Sep).Append((int)d.Ambition)
                  .Append(Sep).Append((int)d.Stage)
                  .Append(Sep).Append(life.HasValue ? life.Value.Birth.ToString(CultureInfo.InvariantCulture) : "")
                  .Append(Sep).Append(life.HasValue ? life.Value.Death.ToString(CultureInfo.InvariantCulture) : "")
                  .Append(Sep).Append(string.Join(",", tagInts))
                  .Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>解析外部数据文本为档案记录（校验魔数/计数；字段非法即抛）。</summary>
        public static IReadOnlyList<GeneralDossierRecord> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("档案数据文本为空。", nameof(text));
            string[] lines = text.Split('\n');
            int idx = 0;
            if (lines[idx++].Trim() != Magic) throw new FormatException("武将档案魔数不符。");
            if (!int.TryParse(lines[idx++], out int count)) throw new FormatException("武将档案计数非法。");

            var recs = new List<GeneralDossierRecord>(count);
            for (int i = 0; i < count; i++)
            {
                if (idx >= lines.Length) throw new FormatException("武将档案截断。");
                string[] p = lines[idx++].Split(Sep);
                // 字段：0=D 1=id 2=prowess 3=strategy 4=leaning 5=ambition 6=stage 7=birth 8=death 9=tags
                if (p.Length < 10 || p[0] != "D") throw new FormatException($"武将档案期望 D 行（10 字段），实得「{string.Join("|", p)}」。");
                var tags = new List<GeneralTag>();
                if (p[9].Length > 0)
                    foreach (string ts in p[9].Split(','))
                        if (int.TryParse(ts, out int tv)) tags.Add((GeneralTag)tv);
                recs.Add(new GeneralDossierRecord(
                    p[1], (CombatTier)I(p[2]), (StrategyTier)I(p[3]), (LoyaltyLeaning)I(p[4]),
                    (Ambition)I(p[5]), (EraStage)I(p[6]),
                    p[7].Length == 0 ? (int?)null : I(p[7]),
                    p[8].Length == 0 ? (int?)null : I(p[8]), tags));
            }
            return recs;
        }

        private static int I(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }
}
