using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 一员武将的目录卡（#2 / GDD_025 R1：<b>无数值面板</b>）：只呈中文名 + <b>气质标签的文字化性情</b>——
    /// 玩家凭名声与性情识人，看不到统率/武力/智略之数，更看不到隐藏的战阵档/谋略档（量级档）与隐秘的心（忠诚/野心）。
    /// 反全知：这些皆为权威内部量，绝不投影为数字（数值只是结果，机制才是灵魂）。
    /// </summary>
    public sealed class GeneralCardView
    {
        /// <summary>稳定人物 id。</summary>
        public string Id { get; }
        /// <summary>中文名。</summary>
        public string Name { get; }
        /// <summary>气质性情（各气质标签的中文短语；无标签则空——"泛泛之辈"）。</summary>
        public IReadOnlyList<string> Traits { get; }

        internal GeneralCardView(GeneralDossier d)
        {
            Id = d.Id.Value;
            Name = DisplayNames.Of(d.Id.Value);
            var traits = new List<string>();
            foreach (GeneralTag t in d.Tags) traits.Add(GeneralTagText.Of(t));
            traits.Sort(System.StringComparer.Ordinal);   // 稳定序，便于测试/渲染
            Traits = traits;
        }
    }

    /// <summary>
    /// 武将目录投影（#2）：列全体已登记武将的目录卡。数据源为 <see cref="GeneralDossiers.All"/>（稳定序）。
    /// 纯只读、无数值——供 Unity 武将录屏渲染，亦为"凭名声识人"体验的基座。
    /// </summary>
    public sealed class GeneralRosterView
    {
        /// <summary>全体武将目录卡（按 id 规范序）。</summary>
        public IReadOnlyList<GeneralCardView> Cards { get; }

        private GeneralRosterView(IReadOnlyList<GeneralCardView> cards) => Cards = cards;

        /// <summary>从武将档案目录构造。</summary>
        public static GeneralRosterView Build()
        {
            var cards = new List<GeneralCardView>();
            foreach (GeneralDossier d in GeneralDossiers.All) cards.Add(new GeneralCardView(d));
            return new GeneralRosterView(cards);
        }
    }

    /// <summary>气质标签 → 中文性情短语（#2 目录展示；与 GDD_025 §标签集一致）。</summary>
    public static class GeneralTagText
    {
        public static string Of(GeneralTag tag) => tag switch
        {
            GeneralTag.Reckless => "莽撞",
            GeneralTag.Awe => "威压",
            GeneralTag.LoneValor => "孤胆",
            GeneralTag.IronBones => "傲骨",
            GeneralTag.NightRaider => "善夜袭",
            GeneralTag.Naval => "谙水战",
            GeneralTag.Cavalry => "骑锋",
            GeneralTag.Defender => "善守",
            GeneralTag.Cunning => "诡谋",
            GeneralTag.Strategist => "远图",
            GeneralTag.Benevolent => "仁德",
            GeneralTag.Arrogant => "傲物",
            GeneralTag.Stubborn => "刚愎",
            GeneralTag.Bloodthirsty => "嗜杀",
            GeneralTag.Hesitant => "优柔",
            GeneralTag.Fickle => "反复",
            GeneralTag.Wolflook => "狼顾",
            _ => tag.ToString(),
        };
    }
}
