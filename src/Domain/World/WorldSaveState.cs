using System;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 世界存档段的权威可保存状态（GDD_015 §Save/Load / TR-world-006 / ADR-0005）。不可变。
    /// 含 <see cref="World"/>（时间/势力/城池归属投影/已触发·已分叉事件集合）与 schema <see cref="Version"/>、
    /// 配置 <see cref="Fingerprint"/>。纳入既有 GDD_013 存档边界（世界段，与生涯/战役段共存）。
    /// </summary>
    public sealed class WorldSaveState
    {
        /// <summary>存档 schema 版本。</summary>
        public SaveVersion Version { get; }

        /// <summary>配置指纹（载入时校验一致）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>世界权威态。</summary>
        public WorldState World { get; }

        public WorldSaveState(SaveVersion version, ConfigFingerprint fingerprint, WorldState world)
        {
            Version = version;
            Fingerprint = fingerprint;
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>世界段权威态的确定性哈希（即 WorldState 哈希，含 diverged 标志集合）。</summary>
        public StateHash ComputeHash() => World.ComputeHash();
    }
}
