using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 敌方AI渐进记忆（GDD_021 R6 / ADR-0013 D6）：记上一回合各区<b>可见</b>敌情（玩家兵力投影），
    /// 供检测玩家意图趋势（如"连续增援正面"）。不可变；入 <see cref="ZoneBattleState"/> 随存档、确定性哈希。
    /// <b>只记可见</b>（反全知：隐蔽伏兵不入记忆）。
    /// </summary>
    public sealed class EnemyAiMemory
    {
        private readonly Dictionary<string, int> _lastVisible;

        /// <summary>上回合各区可见敌兵力（key=zone id）。</summary>
        public IReadOnlyDictionary<string, int> LastVisibleEnemyStrength => _lastVisible;

        public EnemyAiMemory(IReadOnlyDictionary<string, int>? lastVisible)
        {
            _lastVisible = new Dictionary<string, int>(StringComparer.Ordinal);
            if (lastVisible != null)
                foreach (KeyValuePair<string, int> kv in lastVisible)
                    if (kv.Value != 0) _lastVisible[kv.Key] = kv.Value;
        }

        /// <summary>空记忆（战斗初始）。</summary>
        public static EnemyAiMemory Empty { get; } = new EnemyAiMemory(null);

        /// <summary>某区上回合可见敌兵力（缺省 0）。</summary>
        public int LastVisible(ZoneId zone) => _lastVisible.TryGetValue(zone.Value ?? string.Empty, out int v) ? v : 0;

        internal void AppendTo(StateHasher hasher)
        {
            var keys = new List<string>(_lastVisible.Keys);
            keys.Sort(StringComparer.Ordinal);
            hasher.Append(keys.Count);
            foreach (string k in keys)
            {
                ZoneHashing.AppendString(hasher, k);
                hasher.Append(_lastVisible[k]);
            }
        }
    }
}
