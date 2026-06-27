using System;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯存档段的权威可保存状态（GDD_014 §Save/Load / TR-career-003 / ADR-0005）。不可变。
    /// 含 <see cref="Snapshot"/>（CareerState+RetinueState）、可选 <see cref="Rebellion"/>、<see cref="Missions"/>，
    /// 以及 schema <see cref="Version"/> 与配置 <see cref="Fingerprint"/>。纳入既有 GDD_013 存档边界（生涯段）。
    /// </summary>
    public sealed class CareerSaveState
    {
        /// <summary>存档 schema 版本。</summary>
        public SaveVersion Version { get; }

        /// <summary>配置指纹（载入时校验一致，ADR-0003/0005）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>生涯快照（CareerState + RetinueState 好感快照）。</summary>
        public CareerSnapshot Snapshot { get; }

        /// <summary>自立状态（未自立为 null）。</summary>
        public RebellionState? Rebellion { get; }

        /// <summary>君主任务日志。</summary>
        public LordMissionLog Missions { get; }

        public CareerSaveState(
            SaveVersion version, ConfigFingerprint fingerprint,
            CareerSnapshot snapshot, RebellionState? rebellion, LordMissionLog missions)
        {
            Version = version;
            Fingerprint = fingerprint;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Rebellion = rebellion;
            Missions = missions ?? throw new ArgumentNullException(nameof(missions));
        }

        /// <summary>生涯段权威态的确定性哈希（含快照/自立/任务日志；不含版本与指纹元数据）。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            Snapshot.AppendTo(hasher);
            hasher.Append(Rebellion != null);
            Rebellion?.AppendTo(hasher);
            Missions.AppendTo(hasher);
            return hasher.ToHash();
        }
    }
}
