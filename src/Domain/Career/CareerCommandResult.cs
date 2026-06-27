using System;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯命令结算结果（GDD_014 §Failure Cases / TR-career-005）。不可变。
    /// <para>
    /// <b>成功</b>：<see cref="Applied"/>=true，<see cref="Snapshot"/> 为结算后新快照，<see cref="Error"/>=None。
    /// <b>失败</b>：<see cref="Applied"/>=false，<see cref="Snapshot"/> 为<b>原样未变</b>的前态快照（零部分写入，全有或全无），
    /// <see cref="Error"/> 为稳定错误码、<see cref="Detail"/> 为可解释明细。
    /// </para>
    /// </summary>
    public sealed class CareerCommandResult
    {
        /// <summary>命令是否被应用（成功）。</summary>
        public bool Applied { get; }

        /// <summary>结算后快照（成功为新态；失败为原态，未变）。</summary>
        public CareerSnapshot Snapshot { get; }

        /// <summary>错误码（成功为 <see cref="CareerErrorCode.None"/>）。</summary>
        public CareerErrorCode Error { get; }

        /// <summary>可解释明细（成功为空串）。</summary>
        public string Detail { get; }

        private CareerCommandResult(bool applied, CareerSnapshot snapshot, CareerErrorCode error, string detail)
        {
            Applied = applied;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Error = error;
            Detail = detail ?? string.Empty;
        }

        /// <summary>成功：携带结算后新快照。</summary>
        public static CareerCommandResult Success(CareerSnapshot newSnapshot)
            => new CareerCommandResult(true, newSnapshot, CareerErrorCode.None, string.Empty);

        /// <summary>失败：携带稳定错误码与<b>未变</b>的前态快照。</summary>
        public static CareerCommandResult Failure(CareerSnapshot unchangedSnapshot, CareerErrorCode error, string detail)
            => new CareerCommandResult(false, unchangedSnapshot, error, detail);
    }
}
