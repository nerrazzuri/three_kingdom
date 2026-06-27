using System;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立发动结果（GDD_014 §Failure Cases / TR-career-005）。不可变。
    /// 成功：<see cref="Launched"/>=true，<see cref="Snapshot"/> 为自立后新态，<see cref="Rebellion"/> 为自立状态。
    /// 失败：<see cref="Launched"/>=false，<see cref="Snapshot"/> 原样不变、<see cref="Rebellion"/> 为 null，携稳定错误码。
    /// </summary>
    public sealed class RebellionResult
    {
        /// <summary>是否成功发动。</summary>
        public bool Launched { get; }

        /// <summary>结算后生涯快照（成功为新态；失败为原态，未变）。</summary>
        public CareerSnapshot Snapshot { get; }

        /// <summary>自立状态（成功时非空）。</summary>
        public RebellionState? Rebellion { get; }

        /// <summary>错误码（成功为 <see cref="CareerErrorCode.None"/>）。</summary>
        public CareerErrorCode Error { get; }

        /// <summary>可解释明细。</summary>
        public string Detail { get; }

        private RebellionResult(bool launched, CareerSnapshot snapshot, RebellionState? rebellion, CareerErrorCode error, string detail)
        {
            Launched = launched;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Rebellion = rebellion;
            Error = error;
            Detail = detail ?? string.Empty;
        }

        public static RebellionResult Success(CareerSnapshot newSnapshot, RebellionState rebellion)
            => new RebellionResult(true, newSnapshot, rebellion, CareerErrorCode.None, string.Empty);

        public static RebellionResult Failure(CareerSnapshot unchanged, CareerErrorCode error, string detail)
            => new RebellionResult(false, unchanged, null, error, detail);
    }
}
