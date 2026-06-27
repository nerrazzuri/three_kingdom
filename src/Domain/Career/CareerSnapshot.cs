using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯权威状态快照（CareerState + RetinueState），命令路径的输入与输出单元。
    /// 不可变；组合哈希顺序固定（先 Career 后 Retinue），为「同一前态 + 同一命令流 → 同一生涯结算」
    /// （TR-career-001）与存档 round-trip（story-005）提供单一确定性指纹。
    /// </summary>
    public sealed class CareerSnapshot
    {
        /// <summary>生涯数值状态。</summary>
        public CareerState Career { get; }

        /// <summary>部曲/僚属状态。</summary>
        public RetinueState Retinue { get; }

        public CareerSnapshot(CareerState career, RetinueState retinue)
        {
            Career = career ?? throw new ArgumentNullException(nameof(career));
            Retinue = retinue ?? throw new ArgumentNullException(nameof(retinue));
        }

        /// <summary>以规范顺序追加两聚合到状态哈希（先 Career 后 Retinue）。</summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            Career.AppendTo(hasher);
            Retinue.AppendTo(hasher);
        }

        /// <summary>计算本快照的确定性组合哈希。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            AppendTo(hasher);
            return hasher.ToHash();
        }
    }
}
