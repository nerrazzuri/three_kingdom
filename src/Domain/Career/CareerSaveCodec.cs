using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯存档段的版本化 DTO 编解码（ADR-0005 + ADR-0002）。纯 BCL、确定性、制表符分隔文本；<b>禁</b> Unity 序列化。
    /// <see cref="Serialize"/> 同状态→同文本；<see cref="Deserialize"/>(Serialize(s)) 还原等价状态（round-trip）。
    /// 载入校验：schema 版本不兼容（更新）或配置指纹不符 → 抛 <see cref="SaveFormatException"/>，<b>不部分载入</b>。
    /// 量产期可由 Infrastructure 的 JSON 实现替换，契约不变。
    /// </summary>
    public sealed class CareerSaveCodec
    {
        private const string Magic = "TKCAREER/1";
        private const char Sep = '\t';

        /// <summary>序列化生涯存档段为确定性文本。</summary>
        public string Serialize(CareerSaveState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            var sb = new StringBuilder();
            sb.Append(Magic).Append('\n');
            sb.Append("version").Append(Sep).Append(state.Version.ToString()).Append('\n');
            sb.Append("fingerprint").Append(Sep).Append(state.Fingerprint.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');

            CareerState c = state.Snapshot.Career;
            sb.Append("career").Append(Sep)
              .Append(c.Merit).Append(Sep).Append(c.Renown).Append(Sep)
              .Append(c.LordStanding.Raw).Append(Sep).Append((int)c.Rank).Append(Sep)
              .Append(c.IsUnaffiliated ? 1 : 0).Append(Sep)
              .Append(c.Faction.HasValue ? Escape(c.Faction.Value.Value) : string.Empty).Append('\n');

            RetinueState r = state.Snapshot.Retinue;
            sb.Append("retinue").Append(Sep).Append(r.Members.Count).Append('\n');
            foreach (RetinueMember m in r.Members)
                sb.Append("m").Append(Sep).Append(Escape(m.Character.Value)).Append(Sep).Append(m.Affinity.Raw).Append('\n');

            IReadOnlyList<KeyValuePair<OfficeRole, CharacterId>> offices = r.Assignments();
            sb.Append("offices").Append(Sep).Append(offices.Count).Append('\n');
            foreach (KeyValuePair<OfficeRole, CharacterId> kv in offices)
                sb.Append("o").Append(Sep).Append((int)kv.Key).Append(Sep).Append(Escape(kv.Value.Value)).Append('\n');

            RebellionState? reb = state.Rebellion;
            sb.Append("rebellion").Append(Sep).Append(reb != null ? 1 : 0).Append('\n');
            if (reb != null)
            {
                sb.Append("reb").Append(Sep).Append(reb.LoyalRatio.Raw).Append(Sep).Append((int)reb.Outcome).Append(Sep)
                  .Append(reb.NewFaction.HasValue ? 1 : 0).Append(Sep)
                  .Append(reb.NewFaction.HasValue ? Escape(reb.NewFaction.Value.Value) : string.Empty).Append(Sep)
                  .Append(reb.AffinitySnapshot.Count).Append('\n');
                foreach (FixedPoint a in reb.AffinitySnapshot)
                    sb.Append("ra").Append(Sep).Append(a.Raw).Append('\n');
            }

            sb.Append("missions").Append(Sep).Append(state.Missions.Records.Count).Append('\n');
            foreach (LordMissionRecord rec in state.Missions.Records)
                sb.Append("ms").Append(Sep).Append(Escape(rec.MissionId)).Append(Sep).Append((int)rec.Result).Append('\n');

            return sb.ToString();
        }

        /// <summary>
        /// 反序列化并校验：版本不兼容（更新）或指纹不符 → 拒绝（抛，不部分载入）。
        /// </summary>
        public CareerSaveState Deserialize(string text, SaveVersion currentVersion, ConfigFingerprint expectedFingerprint)
        {
            if (text is null) throw new SaveFormatException("生涯存档文本为 null。");
            string[] lines = text.Split('\n');
            int idx = 0;
            string Next() => idx < lines.Length ? lines[idx++] : throw new SaveFormatException("生涯存档截断（缺行）。");

            if (Next() != Magic) throw new SaveFormatException("生涯存档魔数不符。");

            SaveVersion version = SaveVersion.Parse(Field("version", Next()));
            if (!version.CanLoadInto(currentVersion))
                throw new SaveFormatException($"生涯存档版本 {version} 高于当前 {currentVersion}，拒绝载入（不静默降级）。");

            string fpText = Field("fingerprint", Next());
            if (!ulong.TryParse(fpText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong fpVal))
                throw new SaveFormatException("配置指纹字段非法。");
            var fingerprint = new ConfigFingerprint(fpVal);
            if (fingerprint != expectedFingerprint)
                throw new SaveFormatException("配置指纹不符，拒绝载入（防配置漂移，ADR-0003/0005）。");

            // career
            string[] careerParts = Next().Split(Sep);
            if (careerParts.Length != 7 || careerParts[0] != "career") throw new SaveFormatException("career 段格式错。");
            int merit = ParseInt(careerParts[1]);
            int renown = ParseInt(careerParts[2]);
            FixedPoint standing = FixedPoint.FromRaw(ParseInt(careerParts[3]));
            var rank = (Rank)ParseInt(careerParts[4]);
            bool unaffiliated = ParseInt(careerParts[5]) != 0;
            FactionId? faction = unaffiliated ? (FactionId?)null : new FactionId(Unescape(careerParts[6]));
            var career = new CareerState(merit, renown, standing, rank, faction, unaffiliated);

            // retinue
            int memberCount = ParseCount(Field("retinue", Next()));
            var members = new List<RetinueMember>(memberCount);
            for (int i = 0; i < memberCount; i++)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 3 || p[0] != "m") throw new SaveFormatException("retinue 成员格式错。");
                members.Add(new RetinueMember(new CharacterId(Unescape(p[1])), FixedPoint.FromRaw(ParseInt(p[2]))));
            }
            int officeCount = ParseCount(Field("offices", Next()));
            var offices = new List<KeyValuePair<OfficeRole, CharacterId>>(officeCount);
            for (int i = 0; i < officeCount; i++)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 3 || p[0] != "o") throw new SaveFormatException("offices 记录格式错。");
                offices.Add(new KeyValuePair<OfficeRole, CharacterId>((OfficeRole)ParseInt(p[1]), new CharacterId(Unescape(p[2]))));
            }
            var retinue = new RetinueState(members, offices);

            // rebellion
            RebellionState? rebellion = null;
            int hasReb = ParseInt(Field("rebellion", Next()));
            if (hasReb != 0)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 6 || p[0] != "reb") throw new SaveFormatException("rebellion 段格式错。");
                FixedPoint ratio = FixedPoint.FromRaw(ParseInt(p[1]));
                var outcome = (RebellionOutcome)ParseInt(p[2]);
                bool hasNew = ParseInt(p[3]) != 0;
                FactionId? newFaction = hasNew ? new FactionId(Unescape(p[4])) : (FactionId?)null;
                int affCount = ParseCount(p[5]);
                var affinities = new List<FixedPoint>(affCount);
                for (int i = 0; i < affCount; i++)
                {
                    string[] ap = Next().Split(Sep);
                    if (ap.Length != 2 || ap[0] != "ra") throw new SaveFormatException("rebellion 好感快照格式错。");
                    affinities.Add(FixedPoint.FromRaw(ParseInt(ap[1])));
                }
                rebellion = new RebellionState(affinities, ratio, outcome, newFaction);
            }

            // missions
            int missionCount = ParseCount(Field("missions", Next()));
            var missions = new List<LordMissionRecord>(missionCount);
            for (int i = 0; i < missionCount; i++)
            {
                string[] p = Next().Split(Sep);
                if (p.Length != 3 || p[0] != "ms") throw new SaveFormatException("mission 记录格式错。");
                missions.Add(new LordMissionRecord(Unescape(p[1]), (MissionResult)ParseInt(p[2])));
            }

            return new CareerSaveState(version, fingerprint, new CareerSnapshot(career, retinue), rebellion, new LordMissionLog(missions));
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
