using System.Collections.Generic;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Talent;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 一名可招人才的展示卡（GDD_020 / GDD_025 R1 无数值）：中文名 + 专长文字 + 招揽难度<b>定性档</b>——
    /// <b>不投影统率/武勇/智略之数</b>，亦不露确切志向/阻力数值（反全知：靠名声与打听识才，非读数据）。
    /// </summary>
    public sealed class TalentRecruitLine
    {
        public string TalentId { get; }
        public string Name { get; }
        public string SpecialtyLabel { get; }
        /// <summary>招揽难度定性：易招 / 尚可 / 难招（由志向-阻力定性化，无数字）。</summary>
        public string DifficultyLabel { get; }

        internal TalentRecruitLine(TalentProfile t)
        {
            TalentId = t.Id.Value;
            Name = DisplayNames.Of(t.Character.Value);
            SpecialtyLabel = SpecialtyText(t.Specialty);
            DifficultyLabel = Difficulty(t.Willingness, t.Reluctance);
        }

        private static string SpecialtyText(GeneralSpecialty s) => s switch
        {
            GeneralSpecialty.Ambush => "善奇袭",
            GeneralSpecialty.Siege => "善攻坚",
            GeneralSpecialty.Cavalry => "善骑战",
            GeneralSpecialty.Logistics => "善辎重",
            _ => "未显专长",
        };

        private static string Difficulty(FixedPoint willingness, FixedPoint reluctance)
        {
            FixedPoint net = willingness - reluctance;   // 高=易招
            if (net >= FixedPoint.FromFraction(3, 10)) return "易招";
            if (net <= -FixedPoint.FromFraction(2, 10)) return "难招";
            return "尚可";
        }
    }

    /// <summary>
    /// 可招人才录（GDD_020）：列玩家已知晓且已登场的人才（反全知——未知晓者不入）。只呈名/专长/难度定性，无数值。
    /// </summary>
    public sealed class TalentRecruitView
    {
        public IReadOnlyList<TalentRecruitLine> Talents { get; }

        private TalentRecruitView(IReadOnlyList<TalentRecruitLine> talents) => Talents = talents;

        public static TalentRecruitView From(IReadOnlyList<TalentProfile> visible)
        {
            var lines = new List<TalentRecruitLine>();
            if (visible != null)
                foreach (TalentProfile t in visible) lines.Add(new TalentRecruitLine(t));
            return new TalentRecruitView(lines);
        }
    }
}
