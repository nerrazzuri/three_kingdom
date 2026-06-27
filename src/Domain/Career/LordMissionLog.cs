using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>君主任务结果（GDD_014 §Main Rules：任务奖惩）。</summary>
    public enum MissionResult
    {
        /// <summary>完成。</summary>
        Completed = 0,

        /// <summary>失败。</summary>
        Failed = 1,
    }

    /// <summary>一条君主任务记录（GDD_014 §Data Model：LordMissionLog）。不可变。</summary>
    public sealed class LordMissionRecord
    {
        /// <summary>任务稳定 id。</summary>
        public string MissionId { get; }

        /// <summary>结果。</summary>
        public MissionResult Result { get; }

        public LordMissionRecord(string missionId, MissionResult result)
        {
            if (string.IsNullOrWhiteSpace(missionId))
                throw new ArgumentException("任务 id 不可为空或空白。", nameof(missionId));
            if (!Enum.IsDefined(typeof(MissionResult), result))
                throw new ArgumentOutOfRangeException(nameof(result), "未定义的任务结果。");
            MissionId = missionId;
            Result = result;
        }
    }

    /// <summary>
    /// 君主任务日志（GDD_014 §Data Model：LordMissionLog / TR-career-003）。不可变。
    /// 按记录顺序保存（顺序是历史，存档须保序）。纳入存档哈希（round-trip 一致）。
    /// </summary>
    public sealed class LordMissionLog
    {
        private readonly LordMissionRecord[] _records;

        /// <summary>任务记录（保序）。</summary>
        public IReadOnlyList<LordMissionRecord> Records => _records;

        /// <summary>空日志。</summary>
        public static LordMissionLog Empty { get; } = new LordMissionLog(Array.Empty<LordMissionRecord>());

        public LordMissionLog(IReadOnlyList<LordMissionRecord> records)
        {
            if (records is null) throw new ArgumentNullException(nameof(records));
            var arr = new LordMissionRecord[records.Count];
            for (int i = 0; i < records.Count; i++)
                arr[i] = records[i] ?? throw new ArgumentException("任务记录不可含 null。", nameof(records));
            _records = arr;
        }

        /// <summary>追加一条记录，产出新日志（不可变）。</summary>
        public LordMissionLog Append(LordMissionRecord record)
        {
            if (record is null) throw new ArgumentNullException(nameof(record));
            var list = new List<LordMissionRecord>(_records) { record };
            return new LordMissionLog(list);
        }

        /// <summary>以保序规范追加到状态哈希。</summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append(_records.Length);
            foreach (LordMissionRecord r in _records)
            {
                hasher.Append(r.MissionId.Length);
                foreach (char c in r.MissionId) hasher.Append((int)c);
                hasher.Append((int)r.Result);
            }
        }
    }
}
