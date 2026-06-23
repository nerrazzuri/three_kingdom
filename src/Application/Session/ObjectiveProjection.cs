namespace ThreeKingdom.Application.Session
{
    /// <summary>一局的胜负状态（守城待变：守至援军 = 胜；民心崩溃失城 = 败）。</summary>
    public enum GameOutcome
    {
        /// <summary>进行中。</summary>
        Ongoing = 0,
        /// <summary>胜（守至援军抵达日）。</summary>
        Victory = 1,
        /// <summary>败（民心崩溃，城池陷落）。</summary>
        Defeat = 2,
    }

    /// <summary>
    /// 一局目标与胜负的<b>只读投影 DTO</b>（ADR-0002）。承载当前日 / 援军日 / 胜负态 / 败因，
    /// 供 HUD 显示「受命目标」与局终结算。不可变；不泄露可变聚合。
    /// </summary>
    public sealed class ObjectiveProjection
    {
        /// <summary>当前世界日（0 基）。</summary>
        public int CurrentDay { get; }
        /// <summary>援军抵达日（0 基；守至此日为胜）。</summary>
        public int ReliefDay { get; }
        /// <summary>当前胜负态。</summary>
        public GameOutcome Outcome { get; }
        /// <summary>败因（仅 Defeat 非空）。</summary>
        public string DefeatReason { get; }

        /// <summary>胜利方式文案（仅 Victory 非空：断粮退兵 / 守至援军）。</summary>
        public string VictoryReason { get; }

        public ObjectiveProjection(int currentDay, int reliefDay, GameOutcome outcome, string defeatReason, string victoryReason = "")
        {
            CurrentDay = currentDay;
            ReliefDay = reliefDay;
            Outcome = outcome;
            DefeatReason = defeatReason ?? string.Empty;
            VictoryReason = victoryReason ?? string.Empty;
        }
    }
}
