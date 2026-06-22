using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Relationships;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 方向性关系展示模型（P6 多维不合并 / GDD_006 / ADR-0002）。
    /// 信任/敬重/恩义/怨恨<b>四维各为独立字段</b>，本类型<b>刻意不含</b>任何单一综合好感值
    /// （负向不变量由 PresentationLockTests 反射断言）。方向性：from→to 与 to→from 不同。不可变。
    /// </summary>
    public sealed class RelationshipView
    {
        /// <summary>来源标签。</summary>
        public string FromLabel { get; }

        /// <summary>目标标签。</summary>
        public string ToLabel { get; }

        /// <summary>信任（独立维度）。</summary>
        public int Trust { get; }

        /// <summary>敬重（独立维度）。</summary>
        public int Respect { get; }

        /// <summary>恩义（独立维度）。</summary>
        public int Gratitude { get; }

        /// <summary>怨恨（独立维度）。</summary>
        public int Resentment { get; }

        private RelationshipView(string fromLabel, string toLabel, int trust, int respect, int gratitude, int resentment)
        {
            FromLabel = fromLabel;
            ToLabel = toLabel;
            Trust = trust;
            Respect = respect;
            Gratitude = gratitude;
            Resentment = resentment;
        }

        /// <summary>从关系状态读取 from→to 的四个独立维度构造（不合并）。</summary>
        public static RelationshipView FromState(RelationshipState state, CharacterId from, CharacterId to)
        {
            return new RelationshipView(
                from.ToString(),
                to.ToString(),
                state.Get(from, to, RelationshipDimension.Trust),
                state.Get(from, to, RelationshipDimension.Respect),
                state.Get(from, to, RelationshipDimension.Gratitude),
                state.Get(from, to, RelationshipDimension.Resentment));
        }
    }
}
