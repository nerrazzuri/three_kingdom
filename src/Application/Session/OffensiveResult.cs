using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 一次出征攻城的结果（GDD_019）：授权门 → 战力 → 攻城胜负 → （胜）占城归属。不可变。
    /// 败局不占城但可继续（退兵再备战/改守，失败可继续红线）。
    /// </summary>
    public sealed class OffensiveResult
    {
        /// <summary>是否通过授权门并实际出征（false=被门拒，未开战）。</summary>
        public bool Launched { get; }
        /// <summary>授权门结果（被拒时说明原因）。</summary>
        public OffensiveGateResult Gate { get; }
        /// <summary>是否破城取胜（仅出征时有意义）。</summary>
        public bool Victory { get; }
        /// <summary>派生出的进攻方战力（出征时非空）。</summary>
        public OffensiveForce? Force { get; }
        /// <summary>占城归属结算（仅取胜时非空）。</summary>
        public ConquestResult? Conquest { get; }

        private OffensiveResult(bool launched, OffensiveGateResult gate, bool victory, OffensiveForce? force, ConquestResult? conquest)
        {
            Launched = launched;
            Gate = gate;
            Victory = victory;
            Force = force;
            Conquest = conquest;
        }

        /// <summary>授权门拒绝（未出征）。</summary>
        public static OffensiveResult Rejected(OffensiveGateResult gate)
            => new OffensiveResult(false, gate, false, null, null);

        /// <summary>出征但攻城败（不占城，可继续）。</summary>
        public static OffensiveResult Defeated(OffensiveForce force)
            => new OffensiveResult(true, OffensiveGateResult.Authorized, false, force, null);

        /// <summary>出征破城取胜（占城归属见 <see cref="Conquest"/>）。</summary>
        public static OffensiveResult Won(OffensiveForce force, ConquestResult conquest)
            => new OffensiveResult(true, OffensiveGateResult.Authorized, true, force, conquest);
    }
}
