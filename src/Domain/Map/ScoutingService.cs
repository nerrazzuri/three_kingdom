using System;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 侦察服务（地图层；侦察机制本体在 epic-005）。这是真值 → 阵营知识的<b>唯一</b>合法跨越点：
    /// 一次观察读取当前真值生成知识条目，并只写入该阵营知识——真值不变（AC-2）。
    /// 控制权变更等其它路径绝不经此，故不会自动揭示敌情。
    /// </summary>
    public static class ScoutingService
    {
        /// <summary>
        /// 以 <paramref name="faction"/> 在 <paramref name="now"/> 侦察 <paramref name="region"/>：
        /// 读真值生成知识并写入 <paramref name="knowledge"/>。返回写入的知识条目。
        /// </summary>
        public static RegionKnowledge Observe(
            MapTruth truth, FactionKnowledge knowledge, RegionId region, WorldTime now, KnowledgeSource source)
        {
            if (truth == null) throw new ArgumentNullException(nameof(truth));
            if (knowledge == null) throw new ArgumentNullException(nameof(knowledge));

            var t = truth.Region(region); // 读真值（侦察的合法跨越）
            var observation = new RegionKnowledge(region, t.Controller, t.Garrison, now, source);
            knowledge.ApplyObservation(observation); // 只写知识，不写真值
            return observation;
        }
    }
}
