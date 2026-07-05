using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 君主任务展示投影（GDD_014 / W5）：中文任务类型 + 目标 + 期限（公元年）+ 一句情境。纯只读——供 HUD 显示君主所命。
    /// 反全知不涉（任务是君主明令）；奖惩数值不逐一列出，只述"完成累积功勋、通往晋升"。
    /// </summary>
    public sealed class LordMissionView
    {
        /// <summary>任务类型中文。</summary>
        public string TypeLabel { get; }
        /// <summary>目标城中文名（讨伐/守土）；献纳为空。</summary>
        public string TargetName { get; }
        /// <summary>期限公元年。</summary>
        public int DeadlineYear { get; }
        /// <summary>一句情境命令。</summary>
        public string Order { get; }

        internal LordMissionView(LordMission m, int currentYear)
        {
            DeadlineYear = m.DeadlineYear;
            switch (m.Type)
            {
                case MissionType.Subjugate:
                    TypeLabel = "讨伐";
                    TargetName = m.TargetCity.HasValue ? DisplayNames.Of(m.TargetCity.Value.Value) : "—";
                    Order = $"君命：限公元{m.DeadlineYear}年前，攻取{TargetName}。功成，记大功、通显达。";
                    break;
                case MissionType.Defend:
                    TypeLabel = "守土";
                    TargetName = m.TargetCity.HasValue ? DisplayNames.Of(m.TargetCity.Value.Value) : "—";
                    Order = $"君命：坚守{TargetName}至公元{m.DeadlineYear}年，勿失寸土。守成，累功勋。";
                    break;
                default:
                    TypeLabel = "献纳";
                    TargetName = string.Empty;
                    Order = $"君命：限公元{m.DeadlineYear}年前，上缴军粮{m.TributeGrain}石以充军资。缴足，计功勋。";
                    break;
            }
        }
    }
}
