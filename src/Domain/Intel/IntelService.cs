using System;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报流转服务（GDD_007 / TR-intel-001 / ADR-0002）。
    /// 驱动四层单向流转：世界真值 →（侦察）观察 →（上报）报告 →（累积）阵营知识。
    /// <b>方向不可逆</b>：阵营知识/报告/观察的变化绝不写回世界真值（UI 经投影只读，禁触真值）。
    /// S1 观察取观察时刻的真值快照；噪声/暴露判定/置信由 Story 002 引入。确定性。
    /// </summary>
    public sealed class IntelService
    {
        /// <summary>
        /// 侦察：读世界真值生成一次观察（第 1 层 → 第 2 层）。观察是时刻快照，
        /// 此后真值变化不影响既得观察。
        /// </summary>
        public Observation Observe(WorldTruthLedger truth, IntelSubjectId subject, FactionId observer, WorldTime at)
        {
            if (truth == null) throw new ArgumentNullException(nameof(truth));
            TruthRecord record = truth.Get(subject);
            return new Observation(subject, observer, record.ActualStrength, at);
        }

        /// <summary>
        /// 上报：由观察派生带来源的报告（第 2 层 → 第 3 层）。报告值来自观察，可能与当前真值冲突。
        /// </summary>
        public IntelReport ToReport(Observation observation, FactionId faction, IntelSource source)
        {
            if (observation == null) throw new ArgumentNullException(nameof(observation));
            return new IntelReport(
                observation.Subject, faction, observation.ObservedStrength, source, observation.ObservedAt);
        }
    }
}
