using ThreeKingdom.Domain.Life;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 空降者一生视图（GDD_026 §13 HUD）：当前公元年 + 年龄 + <b>人生阶段定性档</b> + 是否寿终。
    /// 反全知延伸至己身——只给"春秋鼎盛/年事渐高/风烛残年"，<b>不给精确寿数倒计时</b>（§11）。纯只读投影。
    /// </summary>
    public sealed class ArrivalLifeView
    {
        /// <summary>当前公元年。</summary>
        public int Year { get; }
        /// <summary>当前年龄。</summary>
        public int Age { get; }
        /// <summary>人生阶段（定性）。</summary>
        public LifePhase Phase { get; }
        /// <summary>人生阶段中文。</summary>
        public string PhaseLabel { get; }
        /// <summary>是否已寿终（一世落幕，走结算+传承）。</summary>
        public bool IsOver { get; }

        internal ArrivalLifeView(int year, ArrivalLife life)
        {
            Year = year;
            Age = life.AgeAt(year);
            Phase = life.PhaseAt(year);
            IsOver = life.IsOver(year);
            PhaseLabel = Label(Phase);
        }

        private static string Label(LifePhase p) => p switch
        {
            LifePhase.Vigorous => "春秋鼎盛",
            LifePhase.Aging => "年事渐高",
            LifePhase.Twilight => "风烛残年",
            _ => "—",
        };
    }
}
