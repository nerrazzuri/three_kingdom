using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>后果写回错误码（稳定枚举，供 UI/复盘据此显示可行动原因）。</summary>
    public enum OutcomeErrorCode
    {
        /// <summary>变更目标在世界快照中不存在。</summary>
        UnknownTarget = 0,

        /// <summary>写回后城市/人物计量将为负（违反非负不变量）。</summary>
        NegativeResult = 1,

        /// <summary>工事写回后越界（&lt;0 或 &gt;最大值）。</summary>
        FortificationOutOfRange = 2,

        /// <summary>守恒分组之 delta 之和非 0（凭空增减）。</summary>
        ConservationViolation = 3,
    }

    /// <summary>单条写回校验错误（稳定码 + 可读细节）。</summary>
    public readonly struct OutcomeError
    {
        /// <summary>稳定错误码。</summary>
        public OutcomeErrorCode Code { get; }

        /// <summary>可读细节（定位用，不参与控制流）。</summary>
        public string Detail { get; }

        public OutcomeError(OutcomeErrorCode code, string detail)
        {
            Code = code;
            Detail = detail ?? string.Empty;
        }

        public override string ToString() => $"{Code}: {Detail}";
    }

    /// <summary>
    /// 后果写回结果（gdd-010 §后果 / TR-battle-003 原子性同源）。
    /// 成功：<see cref="ResultingWorld"/> 为写回后的新快照，<see cref="ResultHash"/> 为其确定性哈希。
    /// 失败：<see cref="ResultingWorld"/> 为<b>原样未变</b>的输入快照，<see cref="Errors"/> 聚合全部稳定错误码
    /// （零部分写入，全有或全无）。不可变。
    /// </summary>
    public sealed class OutcomeWritebackResult
    {
        /// <summary>是否成功写回。</summary>
        public bool Committed { get; }

        /// <summary>结果世界（成功=新快照；失败=原快照未变）。</summary>
        public OutcomeWorld ResultingWorld { get; }

        /// <summary>聚合的校验错误（成功为空）。</summary>
        public IReadOnlyList<OutcomeError> Errors { get; }

        /// <summary>结果状态哈希（成功非空；失败为原快照哈希）。</summary>
        public StateHash ResultHash { get; }

        private OutcomeWritebackResult(bool committed, OutcomeWorld world, IReadOnlyList<OutcomeError> errors)
        {
            Committed = committed;
            ResultingWorld = world;
            Errors = errors;
            ResultHash = world.ComputeHash();
        }

        internal static OutcomeWritebackResult Success(OutcomeWorld newWorld)
            => new OutcomeWritebackResult(true, newWorld, Array.Empty<OutcomeError>());

        internal static OutcomeWritebackResult Failure(OutcomeWorld unchanged, IReadOnlyList<OutcomeError> errors)
            => new OutcomeWritebackResult(false, unchanged, errors);

        /// <summary>是否含某错误码。</summary>
        public bool HasError(OutcomeErrorCode code)
        {
            foreach (var e in Errors) if (e.Code == code) return true;
            return false;
        }
    }
}
