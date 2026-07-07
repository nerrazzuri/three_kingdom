using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 外部武将档案加载器（GDD_027 #5 数据外部化）：把外部数据文本解析并<b>重建为 <see cref="GeneralDossier"/> 运行期对象</b>
    /// + 生卒映射。证明外部文件可作运行期权威源（H 管线的落地端）。运行期真正切换权威源到外部文件（须 Unity
    /// StreamingAssets 装载）为 ★平台/编辑器步；此提供纯 C# 的加载路径 + 保真保证。
    /// </summary>
    public static class GeneralDossierLoader
    {
        /// <summary>由外部数据文本重建档案花名册（对象级；与硬编码 <see cref="GeneralDossiers.All"/> 等价）。</summary>
        public static IReadOnlyList<GeneralDossier> LoadRoster(string tkdata)
        {
            var list = new List<GeneralDossier>();
            foreach (GeneralDossierRecord r in GeneralDossierCodec.Parse(tkdata))
                list.Add(new GeneralDossier(new CharacterId(r.Id), r.Tags, r.Leaning, r.Ambition, r.Prowess, r.Strategy, r.Stage));
            return list;
        }

        /// <summary>由外部数据文本重建生卒映射（将 id → (生, 卒)；无生卒者不入）。</summary>
        public static IReadOnlyDictionary<string, (int Birth, int Death)> LoadLifeYears(string tkdata)
        {
            var map = new Dictionary<string, (int, int)>(System.StringComparer.Ordinal);
            foreach (GeneralDossierRecord r in GeneralDossierCodec.Parse(tkdata))
                if (r.Birth.HasValue && r.Death.HasValue) map[r.Id] = (r.Birth.Value, r.Death.Value);
            return map;
        }
    }
}
