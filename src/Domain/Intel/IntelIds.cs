using System;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报主题 ID（GDD_007：情报针对的对象——敌军单位/城池/运输等）。序数比较，非空。
    /// 情报四层（真值/观察/报告/知识）均以本 ID 关联同一主题。
    /// </summary>
    public readonly struct IntelSubjectId : IEquatable<IntelSubjectId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public IntelSubjectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("IntelSubjectId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(IntelSubjectId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is IntelSubjectId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(IntelSubjectId a, IntelSubjectId b) => a.Equals(b);
        public static bool operator !=(IntelSubjectId a, IntelSubjectId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
