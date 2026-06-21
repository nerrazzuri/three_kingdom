// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 敌将性格（鲁莽/谨慎）决定是否中诱敌之计——可解释，非随机（GDD_005）
// Date: 2026-06-21

using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Characters
{
    /// <summary>
    /// 指挥官（精简）。性格倾向影响判断（如是否追击），不提供任意战斗光环。
    /// 能力（统御）影响过程质量（如能否压住佯退军纪）。
    /// </summary>
    public sealed class Commander
    {
        public string Id { get; }
        public string Name { get; }
        /// <summary>鲁莽倾向 [-1,1]：越高越易贸然追击/冒进。</summary>
        public Fixed Recklessness { get; }
        /// <summary>统御 [0,1]：影响压住军纪、组织佯退的过程质量。</summary>
        public Fixed Command { get; }

        public Commander(string id, string name, Fixed recklessness, Fixed command)
        {
            Id = id; Name = name; Recklessness = recklessness; Command = command;
        }
    }
}
