using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 武将内容数据完整性校验（GDD_027 / ADR-0016 内容守卫）：在测试期扫描全谱（档案/生卒/布防/羁绊/归属）查悬空引用、
    /// 非法生卒、同纪元重复布防等一类内容 bug。硬编码数据源易出静默错（如重复 key 被后者覆盖、id 拼错、羁绊单端不存在），
    /// 此校验作为 CI 守卫，把这类错在测试即拦下。纯函数，无场景依赖。数据外部化后（Batch H）由加载器承接同套规则。
    /// </summary>
    public static class GeneralDataValidation
    {
        /// <summary>有布防数据的锚点年（与 <see cref="GeneralDossiers.AllPlacements"/> 覆盖一致）。</summary>
        public static readonly IReadOnlyList<int> AnchorYears = new[] { 184, 190, 194, 200, 208, 219, 220, 234 };

        /// <summary>一条校验违规（人类可读，含类别 + 定位信息）。</summary>
        public readonly struct Violation
        {
            public string Category { get; }
            public string Detail { get; }
            public Violation(string category, string detail) { Category = category; Detail = detail; }
            public override string ToString() => $"[{Category}] {Detail}";
        }

        /// <summary>全谱扫描，返回所有违规（空 = 数据健康）。</summary>
        public static IReadOnlyList<Violation> Validate()
        {
            var v = new List<Violation>();
            Exists(v);
            Placements(v);
            Bonds(v);
            LifeYears(v);
            return v;
        }

        // 每个档案在世卒表有对应生卒（或显式无生卒＝恒在世，允许）；此处只查 id 非空唯一。
        private static void Exists(List<Violation> v)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (GeneralDossier d in GeneralDossiers.All)
            {
                string? id = d.Id.Value;
                if (string.IsNullOrEmpty(id)) { v.Add(new Violation("empty-id", "存在空 id 档案")); continue; }
                if (!seen.Add(id)) v.Add(new Violation("duplicate-dossier", $"档案 id 重复：{id}"));
            }
        }

        // 布防：每个被布防武将须有档案；同一纪元同一武将不得驻两城（AllPlacements 若源含重复即暴露）。
        private static void Placements(List<Violation> v)
        {
            foreach (int year in AnchorYears)
            {
                var placed = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach ((CharacterId general, ThreeKingdom.Domain.City.CityId city) in GeneralDossiers.AllPlacements(year))
                {
                    string gid = general.Value ?? "";
                    if (GeneralDossiers.Find(general) == null)
                        v.Add(new Violation("placement-orphan", $"公元{year} 布防了无档案武将：{gid} → {city.Value}"));
                    if (placed.TryGetValue(gid, out string? prev) && prev != city.Value)
                        v.Add(new Violation("placement-double-city", $"公元{year} 武将 {gid} 同纪元驻两城：{prev} 与 {city.Value}"));
                    else placed[gid] = city.Value ?? "";
                }
            }
        }

        // 羁绊：两端武将都须有档案（拼错 id / 单端悬空是常见静默错）。
        private static void Bonds(List<Violation> v)
        {
            foreach (Bond b in GeneralBonds.All)
            {
                if (GeneralDossiers.Find(b.A) == null) v.Add(new Violation("bond-orphan", $"羁绊端无档案：{b.A.Value}（与 {b.B.Value}）"));
                if (GeneralDossiers.Find(b.B) == null) v.Add(new Violation("bond-orphan", $"羁绊端无档案：{b.B.Value}（与 {b.A.Value}）"));
            }
        }

        // 生卒合法：生年 < 卒年；三国纪元合理区间（生 100–260，卒 150–320，寿 ≤ 100）。
        private static void LifeYears(List<Violation> v)
        {
            foreach (GeneralDossier d in GeneralDossiers.All)
            {
                (int Birth, int Death)? life = GeneralDossiers.LifeOf(d.Id);
                if (life == null) continue; // 无生卒＝该纪元恒在世（名士/中立），允许。
                (int b, int dd) = (life.Value.Birth, life.Value.Death);
                if (b >= dd) v.Add(new Violation("life-inverted", $"{d.Id.Value} 生年≥卒年：{b}≥{dd}"));
                else if (dd - b > 100) v.Add(new Violation("life-implausible", $"{d.Id.Value} 寿逾百年：{b}–{dd}"));
                if (b < 100 || b > 270) v.Add(new Violation("life-out-of-era", $"{d.Id.Value} 生年离谱：{b}"));
                if (dd < 150 || dd > 320) v.Add(new Violation("life-out-of-era", $"{d.Id.Value} 卒年离谱：{dd}"));
            }
        }
    }
}
