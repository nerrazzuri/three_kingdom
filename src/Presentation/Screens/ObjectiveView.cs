using System;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 一局目标/胜负展示视图（ADR-0002：Presentation 翻译 Application 只读投影为 UI 文案）。
    /// 进行中显示「受命：守城至第 N 日援军抵达（还余 M 日）」；局终显示胜/败横幅 + 败因。
    /// 不可变、纯映射。逻辑由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    public sealed class ObjectiveView
    {
        /// <summary>是否一局已结束（胜或败）。</summary>
        public bool IsOver { get; }
        /// <summary>是否胜。</summary>
        public bool IsVictory { get; }
        /// <summary>目标/进度文案（进行中显示剩余天数；局终显示空串，改看横幅）。</summary>
        public string ObjectiveLabel { get; }
        /// <summary>局终横幅文案（进行中为空串）。</summary>
        public string BannerLabel { get; }

        public ObjectiveView(ObjectiveProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            int reliefDisplay = projection.ReliefDay + 1; // 0 基 → 「第 N 日」
            IsOver = projection.Outcome != GameOutcome.Ongoing;
            IsVictory = projection.Outcome == GameOutcome.Victory;

            switch (projection.Outcome)
            {
                case GameOutcome.Victory:
                    ObjectiveLabel = string.Empty;
                    BannerLabel = "已守至援军抵达——汜水关守住了。";
                    break;
                case GameOutcome.Defeat:
                    ObjectiveLabel = string.Empty;
                    BannerLabel = "城破：" + projection.DefeatReason;
                    break;
                default:
                    int remaining = projection.ReliefDay - projection.CurrentDay;
                    if (remaining < 0) remaining = 0;
                    ObjectiveLabel = "受命：守汜水关至第 " + reliefDisplay.ToString(CultureInfo.InvariantCulture)
                        + " 日援军抵达（还余 " + remaining.ToString(CultureInfo.InvariantCulture) + " 日）。";
                    BannerLabel = string.Empty;
                    break;
            }
        }
    }
}
