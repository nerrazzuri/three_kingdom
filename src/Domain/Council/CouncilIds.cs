using System;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>军师/参与者 ID（GDD_008：AdvisorPerspective 人物）。序数比较，非空。</summary>
    public readonly struct AdvisorId : IEquatable<AdvisorId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public AdvisorId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("AdvisorId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(AdvisorId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is AdvisorId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(AdvisorId a, AdvisorId b) => a.Equals(b);
        public static bool operator !=(AdvisorId a, AdvisorId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>
    /// 知识快照 ID（GDD_008：CouncilSession 冻结召开时合法知识快照）。序数比较，非空。
    /// 建议绑定快照 ID；当前知识快照 ID 变化即建议过时（不静默更新）。
    /// </summary>
    public readonly struct KnowledgeSnapshotId : IEquatable<KnowledgeSnapshotId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public KnowledgeSnapshotId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("KnowledgeSnapshotId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(KnowledgeSnapshotId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is KnowledgeSnapshotId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(KnowledgeSnapshotId a, KnowledgeSnapshotId b) => a.Equals(b);
        public static bool operator !=(KnowledgeSnapshotId a, KnowledgeSnapshotId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
