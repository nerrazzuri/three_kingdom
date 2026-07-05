using System.Collections.Generic;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>与一方势力的外交态（GDD_023）：中文名 + 立场文字 + 是否可径攻（盟/互不侵犯须先背约）。</summary>
    public sealed class DiplomacyLine
    {
        public string FactionId { get; }
        public string FactionName { get; }
        public string StanceLabel { get; }
        /// <summary>是否可不背约径攻（敌对/中立可，盟/互不侵犯不可）。</summary>
        public bool CanAttackFreely { get; }

        internal DiplomacyLine(string factionId, string factionName, DiplomaticStance stance)
        {
            FactionId = factionId;
            FactionName = factionName;
            StanceLabel = Label(stance);
            CanAttackFreely = stance == DiplomaticStance.Hostile || stance == DiplomaticStance.Neutral;
        }

        private static string Label(DiplomaticStance s) => s switch
        {
            DiplomaticStance.Hostile => "敌对",
            DiplomaticStance.NonAggression => "互不侵犯",
            DiplomaticStance.Alliance => "盟约",
            _ => "中立",
        };
    }

    /// <summary>
    /// 外交态一览（GDD_023）：对每一存续势力列出当前立场（缔约经 pact/背约经 breach 改变）。
    /// 数据源为外交立场态 + 争霸存续势力；纯只读、无 Unity 依赖。
    /// </summary>
    public sealed class DiplomacyView
    {
        public IReadOnlyList<DiplomacyLine> Factions { get; }

        private DiplomacyView(IReadOnlyList<DiplomacyLine> factions) => Factions = factions;

        public static DiplomacyView Build(DiplomaticStanceState stances, ContentionState contention, FactionId playerFaction)
        {
            var lines = new List<DiplomacyLine>();
            if (contention != null)
                foreach (PowerStanding p in contention.Powers)
                {
                    if (!p.Alive || p.Faction == playerFaction) continue;
                    DiplomaticStance s = stances != null ? stances.StanceWith(p.Faction) : DiplomaticStance.Neutral;
                    lines.Add(new DiplomacyLine(p.Faction.Value, DisplayNames.Of(p.Faction.Value), s));
                }
            return new DiplomacyView(lines);
        }
    }
}
