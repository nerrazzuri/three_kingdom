using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 一条确定性随机流的可存档位置状态（ADR-0004 + ADR-0005 / TR-save-002）。
    /// 携带流名、种子与<b>已抽取位置</b>。读档<b>不重抽</b>已发生结果——以 (seed, position) 精确重建后续序列。
    /// 不可变值。
    /// </summary>
    public readonly struct RngStreamState
    {
        /// <summary>流名（区分多条命名流；序列化键）。</summary>
        public string Name { get; }

        /// <summary>种子（权威状态的一半）。</summary>
        public ulong Seed { get; }

        /// <summary>已抽取位置（权威状态的另一半；读档据此续抽）。</summary>
        public ulong Position { get; }

        public RngStreamState(string name, ulong seed, ulong position)
        {
            Name = name;
            Seed = seed;
            Position = position;
        }

        /// <summary>从一条具体随机流抓取当前位置状态。</summary>
        public static RngStreamState Capture(string name, DeterministicRandom rng)
            => new RngStreamState(name, rng.Seed, rng.Position);

        /// <summary>以保存的 (seed, position) 重建随机流，续抽与未存档继续一致。</summary>
        public DeterministicRandom Rebuild() => new DeterministicRandom(Seed, Position);
    }
}
