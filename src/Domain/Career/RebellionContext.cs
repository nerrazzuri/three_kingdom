using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立发动的外部输入（GDD_014 §Formula 2 的非生涯侧事实）。不可变值。
    /// supply_ready/troops_ready 由 GDD_012 后勤状态投影而来，本层只消费布尔（story Out of Scope）。
    /// </summary>
    public readonly struct RebellionContext
    {
        /// <summary>自主掌控城池数（来自世界模型）。</summary>
        public int CitiesOwned { get; }

        /// <summary>补给就绪（GDD_012 投影布尔）。</summary>
        public bool SupplyReady { get; }

        /// <summary>兵力就绪（GDD_012 投影布尔）。</summary>
        public bool TroopsReady { get; }

        /// <summary>君主压迫剧情触发标志（连续贬谪/苛刻任务）。</summary>
        public bool LordOppression { get; }

        /// <summary>自立成立后新势力 id（全员拥立/部分跟随时采用；众叛亲离沦为流浪不采用，可为 null）。</summary>
        public FactionId? NewFactionId { get; }

        public RebellionContext(
            int citiesOwned, bool supplyReady, bool troopsReady, bool lordOppression, FactionId? newFactionId)
        {
            CitiesOwned = citiesOwned;
            SupplyReady = supplyReady;
            TroopsReady = troopsReady;
            LordOppression = lordOppression;
            NewFactionId = newFactionId;
        }
    }

    /// <summary>自立可发动判定结果（三组条件独立可验，GDD_014 §Formula 2）。不可变。</summary>
    public sealed class RebellionEligibility
    {
        /// <summary>是否可发动（三组任一成立）。</summary>
        public bool CanRebel { get; }

        /// <summary>第 1 组成立：城池 + 补给 + 兵力。</summary>
        public bool MilitaryGroupMet { get; }

        /// <summary>第 2 组成立：名望 + 平均好感。</summary>
        public bool PopularGroupMet { get; }

        /// <summary>第 3 组成立：君主压迫剧情。</summary>
        public bool OppressionMet { get; }

        public RebellionEligibility(bool militaryGroupMet, bool popularGroupMet, bool oppressionMet)
        {
            MilitaryGroupMet = militaryGroupMet;
            PopularGroupMet = popularGroupMet;
            OppressionMet = oppressionMet;
            CanRebel = militaryGroupMet || popularGroupMet || oppressionMet;
        }
    }
}
