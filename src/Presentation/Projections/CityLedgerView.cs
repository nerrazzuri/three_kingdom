using System;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 己方城市账本展示视图（ADR-0002：Presentation 把 Application 只读投影翻译为 UI 文案）。
    /// 把 <see cref="CityLedgerProjection"/> 映射为中文展示串（粮草/民心/治安/工事 + 短缺/骚乱警示）。
    /// 多维状态<b>分列不合并</b>（P6 设计锁）。不可变、纯映射，无规则。逻辑由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    public sealed class CityLedgerView
    {
        /// <summary>粮草标签（如「粮草 300」）。</summary>
        public string StockLabel { get; }
        /// <summary>民心标签（分列，不与治安合并）。</summary>
        public string MoraleLabel { get; }
        /// <summary>治安标签。</summary>
        public string SecurityLabel { get; }
        /// <summary>工事标签（当前/最大）。</summary>
        public string FortificationLabel { get; }
        /// <summary>是否有警示（短缺或高骚乱风险）。</summary>
        public bool HasWarning { get; }
        /// <summary>警示文案（无警示为空串）。</summary>
        public string WarningLabel { get; }

        public CityLedgerView(CityLedgerProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            StockLabel = "粮草 " + projection.Stock.ToString(CultureInfo.InvariantCulture);
            MoraleLabel = "民心 " + projection.CivMorale.ToString(CultureInfo.InvariantCulture);
            SecurityLabel = "治安 " + projection.Security.ToString(CultureInfo.InvariantCulture);
            FortificationLabel = "工事 "
                + projection.Fortification.ToString(CultureInfo.InvariantCulture) + "/"
                + projection.FortificationMax.ToString(CultureInfo.InvariantCulture);

            if (projection.LastDayShortage > 0)
            {
                HasWarning = true;
                WarningLabel = "粮草短缺 " + projection.LastDayShortage.ToString(CultureInfo.InvariantCulture)
                    + "，民心受损" + (projection.HighUnrestRisk ? "，骚乱风险高" : string.Empty);
            }
            else if (projection.HighUnrestRisk)
            {
                HasWarning = true;
                WarningLabel = "骚乱风险高";
            }
            else
            {
                HasWarning = false;
                WarningLabel = string.Empty;
            }
        }
    }
}
