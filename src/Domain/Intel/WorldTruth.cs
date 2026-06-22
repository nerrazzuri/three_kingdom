using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报第 1 层——世界真值单条（GDD_007 / TR-intel-001 / ADR-0002）。
    /// 关于某主题的<b>权威事实</b>（真实兵力/控制方）。<b>仅</b>属世界真值层，
    /// <b>绝不</b>进入显示层投影——UI 只读阵营知识层（见 <see cref="IntelProjection"/>）。
    /// </summary>
    public sealed class TruthRecord
    {
        /// <summary>主题。</summary>
        public IntelSubjectId Subject { get; }

        /// <summary>真实兵力（权威整数）。</summary>
        public int ActualStrength { get; internal set; }

        /// <summary>真实控制方。</summary>
        public FactionId Owner { get; internal set; }

        public TruthRecord(IntelSubjectId subject, int actualStrength, FactionId owner)
        {
            if (actualStrength < 0) throw new ArgumentOutOfRangeException(nameof(actualStrength), "真实兵力不可为负。");
            Subject = subject;
            ActualStrength = actualStrength;
            Owner = owner;
        }
    }

    /// <summary>
    /// 世界真值账本（GDD_007 第 1 层，权威可变；同一信息唯一权威来源）。
    /// 仅 Domain 结算与侦察读取以派生观察；显示层<b>不得</b>引用本类型（control-manifest 禁则 UI 读世界真值）。
    /// </summary>
    public sealed class WorldTruthLedger
    {
        private readonly Dictionary<IntelSubjectId, TruthRecord> _records = new Dictionary<IntelSubjectId, TruthRecord>();

        /// <summary>登记/替换某主题的真值。</summary>
        public void Set(TruthRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _records[record.Subject] = record;
        }

        /// <summary>读取主题真值；不存在抛。</summary>
        public TruthRecord Get(IntelSubjectId subject)
        {
            if (!_records.TryGetValue(subject, out TruthRecord? record))
                throw new KeyNotFoundException($"无主题 {subject} 的真值记录。");
            return record;
        }

        /// <summary>是否存在主题真值。</summary>
        public bool Has(IntelSubjectId subject) => _records.ContainsKey(subject);

        /// <summary>更新真实兵力（权威路径整数）。</summary>
        public void SetStrength(IntelSubjectId subject, int strength)
        {
            if (strength < 0) throw new ArgumentOutOfRangeException(nameof(strength), "真实兵力不可为负。");
            Get(subject).ActualStrength = strength;
        }
    }
}
