using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 一员武将的<b>档案</b>（GDD_025 §6）：具名气质标签集 + 隐秘心（忠诚倾向/野心，定性档，不显示）+ 人生阶段。
    /// <b>取代任何数值 stat 卡</b>。玩家侧只见名声+已知标签（反全知投影，另见展示层）；此为权威档案。
    /// 隐秘心经确定性映射（<see cref="LoyaltyScore"/> 等）喂人心杠杆（GDD_024）——映射<b>内部</b>，非玩家可见数。不可变。
    /// </summary>
    public sealed class GeneralDossier
    {
        private readonly HashSet<GeneralTag> _tags;

        public CharacterId Id { get; }
        public IReadOnlyCollection<GeneralTag> Tags => _tags;
        public LoyaltyLeaning Leaning { get; }
        public Ambition Ambition { get; }
        public EraStage Stage { get; }

        public GeneralDossier(CharacterId id, IReadOnlyList<GeneralTag> tags, LoyaltyLeaning leaning, Ambition ambition, EraStage stage = EraStage.Prime)
        {
            if (tags is null) throw new ArgumentNullException(nameof(tags));
            _tags = new HashSet<GeneralTag>(tags);
            Id = id;
            Leaning = leaning;
            Ambition = ambition;
            Stage = stage;
        }

        /// <summary>是否带某标签。</summary>
        public bool HasTag(GeneralTag tag) => _tags.Contains(tag);

        // ---- 隐秘心 → 人心杠杆内部映射（GDD_025 F2 / GDD_024）。定点 [0,1]，供 SubversionTargetProfile 消费；不显示给玩家。----

        private static FixedPoint F(int n) => FixedPoint.FromFraction(n, 10);

        /// <summary>忠诚度（对主君）：忠义近乎不可策反、怀贰极易；【反复】标签再降。</summary>
        public FixedPoint LoyaltyScore
        {
            get
            {
                FixedPoint b = Leaning switch
                {
                    LoyaltyLeaning.Loyal => F(9),
                    LoyaltyLeaning.Content => F(7),
                    LoyaltyLeaning.Wavering => F(4),
                    _ => FixedPoint.FromFraction(15, 100),   // Disloyal
                };
                if (HasTag(GeneralTag.Fickle)) b = (b - F(2)).Clamp(FixedPoint.Zero, FixedPoint.One);
                return b;
            }
        }

        /// <summary>对主君怨恨（可乘度）：野心越大而忠诚越低越怀怨；狼顾/雄图 + 怀贰最盛。</summary>
        public FixedPoint ResentmentScore
        {
            get
            {
                FixedPoint amb = Ambition switch
                {
                    Ambition.None => F(1),
                    Ambition.Aspiring => F(3),
                    Ambition.Grand => F(5),
                    _ => F(7),   // Wolfish
                };
                // 忠诚低则怨怼放大（不忠 + 有志 = 可乘）。
                FixedPoint disloyalBoost = (FixedPoint.One - LoyaltyScore) * F(4);
                return (amb + disloyalBoost).Clamp(FixedPoint.Zero, FixedPoint.One);
            }
        }

        /// <summary>贪财（受许诺影响）：默认中偏低；【反复】者更易被利诱。</summary>
        public FixedPoint GreedScore
            => (F(3) + (HasTag(GeneralTag.Fickle) ? F(2) : FixedPoint.Zero)).Clamp(FixedPoint.Zero, FixedPoint.One);

        /// <summary>魅力（攻心抵抗）：仁德者高、傲物者低。</summary>
        public FixedPoint CharmScore
        {
            get
            {
                if (HasTag(GeneralTag.Benevolent)) return F(8);
                if (HasTag(GeneralTag.Arrogant)) return FixedPoint.FromFraction(25, 100);
                return F(4);
            }
        }

        /// <summary>警觉（识破反噬）：诡谋/远图/狼顾者机敏。</summary>
        public FixedPoint AlertnessScore
            => (HasTag(GeneralTag.Cunning) || HasTag(GeneralTag.Strategist) || HasTag(GeneralTag.Wolflook)) ? F(7) : F(4);
    }
}
