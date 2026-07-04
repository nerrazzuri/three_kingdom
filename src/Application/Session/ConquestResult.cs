using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Application.Session
{
    /// <summary>占城结算结果（GDD_019 §占城 C / ADR-0010）：归属判定 + 占城计数 + 自立倾向 + 是否记功。不可变。</summary>
    public sealed class ConquestResult
    {
        /// <summary>占领归属判定（归玩家/归君主）。</summary>
        public OwnershipVerdict Verdict { get; }

        /// <summary>结算后累计占城数。</summary>
        public int ConquestCount { get; }

        /// <summary>结算后累计自立倾向量。</summary>
        public int RebellionLean { get; }

        /// <summary>是否应用了出征战功（给梯队且达成）。</summary>
        public bool CareerApplied { get; }

        public ConquestResult(OwnershipVerdict verdict, int conquestCount, int rebellionLean, bool careerApplied)
        {
            Verdict = verdict;
            ConquestCount = conquestCount;
            RebellionLean = rebellionLean;
            CareerApplied = careerApplied;
        }
    }
}
