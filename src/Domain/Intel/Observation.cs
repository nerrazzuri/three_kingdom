using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报第 2 层——观察（GDD_007 / TR-intel-001）。
    /// 某阵营在某时段对某主题的<b>一次原始观察</b>。独立于真值层：真值此后变化不改既有观察
    /// （观察是历史快照）。S1 观察值取观察时的真值副本；噪声/估计区间由 Story 002 派生。不可变。
    /// </summary>
    public sealed class Observation
    {
        /// <summary>主题。</summary>
        public IntelSubjectId Subject { get; }

        /// <summary>观察阵营。</summary>
        public FactionId Observer { get; }

        /// <summary>观察到的兵力（观察时刻的快照值，非实时真值）。</summary>
        public int ObservedStrength { get; }

        /// <summary>观察时间（时效衰减以此为基，Story 002 复用）。</summary>
        public WorldTime ObservedAt { get; }

        public Observation(IntelSubjectId subject, FactionId observer, int observedStrength, WorldTime observedAt)
        {
            Subject = subject;
            Observer = observer;
            ObservedStrength = observedStrength;
            ObservedAt = observedAt;
        }
    }
}
