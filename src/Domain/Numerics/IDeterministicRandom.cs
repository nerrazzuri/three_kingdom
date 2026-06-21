namespace ThreeKingdom.Domain.Numerics
{
    /// <summary>
    /// 注入式确定性随机流（ADR-0004 支柱 2）。随机性只能经此接口在规则声明的检查点消费——
    /// 禁止隐式全局随机源（control-manifest 禁则 implicit_global_random）。
    /// <para>
    /// <see cref="Position"/>（已抽取序号）是**权威状态**，纳入存档与状态哈希（epic-009 序列化）；
    /// 同一 (seed, position) 必定重建同一后续序列——这是回放与 round-trip 一致性的根。
    /// </para>
    /// <para>
    /// 注：ADR-0004 示意的 <c>Next(CheckpointId)</c> 命名流/声明式检查点语义在有消费者的系统
    /// （战斗/天气/情报）落地时再分层封装于本原语之上；本接口提供确定性流 + 位置的底座。
    /// </para>
    /// </summary>
    public interface IDeterministicRandom
    {
        /// <summary>已抽取的值的数量（权威状态；(seed, Position) 唯一决定后续序列）。</summary>
        ulong Position { get; }

        /// <summary>抽取下一个 64 位无符号随机值，并前进 <see cref="Position"/>。</summary>
        ulong NextBits();

        /// <summary>抽取下一个定点单位值，落在 [0, 1)。</summary>
        FixedPoint NextUnit();

        /// <summary>抽取 [minInclusive, maxExclusive) 内的整数。maxExclusive&lt;=minInclusive 抛 ArgumentException。</summary>
        int NextInt(int minInclusive, int maxExclusive);
    }
}
