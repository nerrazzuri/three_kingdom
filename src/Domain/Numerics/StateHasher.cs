using System;

namespace ThreeKingdom.Domain.Numerics
{
    /// <summary>
    /// 确定性状态哈希值（ADR-0004 支柱 4）。状态哈希是严格相等判定，用于回放校验与差异复现。
    /// </summary>
    public readonly struct StateHash : IEquatable<StateHash>
    {
        public ulong Value { get; }
        public StateHash(ulong value) => Value = value;

        public bool Equals(StateHash other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is StateHash other && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(StateHash a, StateHash b) => a.Value == b.Value;
        public static bool operator !=(StateHash a, StateHash b) => a.Value != b.Value;
        public override string ToString() => "0x" + Value.ToString("X16");
    }

    /// <summary>
    /// 确定性哈希器（FNV-1a 64 位）。ADR-0004：H(initial_snapshot ‖ config_fingerprint ‖ seed ‖ ordered_orders)。
    /// 字节序显式小端写入，**不依赖平台字节序**（BitConverter.IsLittleEndian 因平台而异，故手写）。
    /// <para>
    /// 哈希**顺序敏感**：调用方须以规范顺序 Append（ADR-0004「哈希前对状态做规范字节序排序」）；
    /// 各聚合自身负责其字段的规范遍历顺序。无符号乘法在默认 unchecked 下按 2^64 回绕，符合 FNV 语义。
    /// </para>
    /// </summary>
    public sealed class StateHasher
    {
        private const ulong Offset = 1469598103934665603UL; // FNV offset basis (64-bit)
        private const ulong Prime = 1099511628211UL;        // FNV prime (64-bit)

        private ulong _hash = Offset;

        /// <summary>追加一个字节。</summary>
        public StateHasher Append(byte b)
        {
            _hash ^= b;
            _hash *= Prime;
            return this;
        }

        /// <summary>追加 int（小端 4 字节）。</summary>
        public StateHasher Append(int value) => AppendUInt32(unchecked((uint)value));

        /// <summary>追加 long（小端 8 字节）。</summary>
        public StateHasher Append(long value) => AppendUInt64(unchecked((ulong)value));

        /// <summary>追加 ulong（小端 8 字节）。</summary>
        public StateHasher Append(ulong value) => AppendUInt64(value);

        /// <summary>追加 FixedPoint（按其权威 Raw 值）。</summary>
        public StateHasher Append(FixedPoint value) => Append(value.Raw);

        /// <summary>追加 bool（1/0）。</summary>
        public StateHasher Append(bool value) => Append((byte)(value ? 1 : 0));

        private StateHasher AppendUInt32(uint v)
        {
            Append((byte)(v & 0xFF));
            Append((byte)((v >> 8) & 0xFF));
            Append((byte)((v >> 16) & 0xFF));
            Append((byte)((v >> 24) & 0xFF));
            return this;
        }

        private StateHasher AppendUInt64(ulong v)
        {
            for (int i = 0; i < 8; i++)
            {
                Append((byte)(v & 0xFF));
                v >>= 8;
            }
            return this;
        }

        /// <summary>取当前哈希值。</summary>
        public StateHash ToHash() => new StateHash(_hash);
    }
}
