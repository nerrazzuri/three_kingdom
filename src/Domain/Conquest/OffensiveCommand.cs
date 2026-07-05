using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 出征将领（GDD_019 D3 派谁轴）：主将/副将的战斗属性。不可变。
    /// 三属性 Q16.16 ∈[0,1]（权威路径禁 float，ADR-0004）：
    /// <c>Command</c>统率（→战力/军纪）· <c>Valor</c>武勇（→士气）· <c>Guile</c>智略（→兵法条件可行性/识破）。
    /// 来源=玩家僚属花名册（GDD_014/006），但战斗属性与好感解耦（RetinueMember 只有好感，ADR-0011 D5）。
    /// </summary>
    public sealed class OffensiveGeneral
    {
        /// <summary>将领人物 ID（GDD_005 稳定身份）。</summary>
        public CharacterId Character { get; }

        /// <summary>统率（→ 战力/军纪加成）。</summary>
        public FixedPoint Command { get; }

        /// <summary>武勇（→ 士气加成）。</summary>
        public FixedPoint Valor { get; }

        /// <summary>智略（→ 兵法条件可行性/识破敌预设）。</summary>
        public FixedPoint Guile { get; }

        /// <summary>专长（降低对应路线条件门槛；展示 + 后续扩展）。</summary>
        public GeneralSpecialty Specialty { get; }

        private readonly GeneralTag[] _tags;

        /// <summary>气质标签（GDD_025）：随将带入战斗，在条件涌现中发作（如【诡谋】降智略门）。空=无标签。</summary>
        public IReadOnlyList<GeneralTag> Tags => _tags;

        /// <summary>战阵档（GDD_025，可空）：带兵杀伤强度粗档，驱动杀伤系数右偏抽取。null=无档（不加成，中性 1.0）。</summary>
        public CombatTier? Prowess { get; }

        /// <summary>是否带某气质标签（GDD_025）。</summary>
        public bool HasTag(GeneralTag tag)
        {
            foreach (GeneralTag t in _tags) if (t == tag) return true;
            return false;
        }

        /// <summary>构造并校验三属性范围 [0,1]。越界即抛，无部分写入。</summary>
        public OffensiveGeneral(
            CharacterId character, FixedPoint command, FixedPoint valor, FixedPoint guile,
            GeneralSpecialty specialty = GeneralSpecialty.None, IReadOnlyList<GeneralTag>? tags = null,
            CombatTier? prowess = null)
        {
            Require01(command, nameof(command));
            Require01(valor, nameof(valor));
            Require01(guile, nameof(guile));
            Character = character;
            Command = command;
            Valor = valor;
            Guile = guile;
            Specialty = specialty;
            _tags = tags != null ? new List<GeneralTag>(tags).ToArray() : Array.Empty<GeneralTag>();
            Prowess = prowess;
        }

        private static void Require01(FixedPoint v, string name)
        {
            if (v < FixedPoint.Zero || v > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "将领属性须在 [0,1]。");
        }
    }

    /// <summary>
    /// 出征将领编成（GDD_019 D3）：主将（必选）+ 副将（0..N，战力贡献按 decay^k 递减）+ 军师随军（可选）。不可变。
    /// 军师随军开启/强化"敌预设可信度/伏击突然性"一类需智谋才成立的条件门（GDD_008），并提供战前可行性提示。
    /// </summary>
    public sealed class OffensiveCommand
    {
        /// <summary>主将（必选）。</summary>
        public OffensiveGeneral Lead { get; }

        /// <summary>副将（按给定顺序，第 k 名贡献 decay^k 递减；护栏防堆将碾压，ADR-0011 D5 / W5）。</summary>
        public IReadOnlyList<OffensiveGeneral> Deputies { get; }

        /// <summary>军师是否随军。</summary>
        public bool AdvisorAccompanies { get; }

        /// <summary>构造。主将不可为空（缺主将不成军）。</summary>
        public OffensiveCommand(
            OffensiveGeneral lead, IReadOnlyList<OffensiveGeneral>? deputies = null, bool advisorAccompanies = false)
        {
            Lead = lead ?? throw new ArgumentNullException(nameof(lead), "出征须有主将。");
            Deputies = deputies ?? Array.Empty<OffensiveGeneral>();
            AdvisorAccompanies = advisorAccompanies;
        }
    }
}
