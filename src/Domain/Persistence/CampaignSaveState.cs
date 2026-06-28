using System;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 战役存档统一信封（GDD_013 同一存档边界 / TR-career-003 + TR-world-006 / ADR-0005）。不可变。
    /// 把生涯段（<see cref="Career"/>）与世界段（<see cref="World"/>）纳入<b>单一信封</b>，统一 schema
    /// <see cref="Version"/> 与配置 <see cref="Fingerprint"/>——一处校验、整体载入或整体拒绝（无部分载入）。
    /// 战役段（GDD_010）随其落地后以同样方式并入本信封。
    /// </summary>
    public sealed class CampaignSaveState
    {
        /// <summary>统一 schema 版本。</summary>
        public SaveVersion Version { get; }

        /// <summary>统一配置指纹。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>生涯段（epic-011）。</summary>
        public CareerSaveState Career { get; }

        /// <summary>世界段（epic-012）。</summary>
        public WorldSaveState World { get; }

        public CampaignSaveState(SaveVersion version, ConfigFingerprint fingerprint, CareerSaveState career, WorldSaveState world)
        {
            Version = version;
            Fingerprint = fingerprint;
            Career = career ?? throw new ArgumentNullException(nameof(career));
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>整存档权威态的确定性哈希（生涯段 ⊕ 世界段）。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            hasher.Append(Career.ComputeHash().Value);
            hasher.Append(World.ComputeHash().Value);
            return hasher.ToHash();
        }
    }
}
