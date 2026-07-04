using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 忠诚经营（GDD_014，<b>确定性纯函数</b>）：赏赐升忠诚、久疏则衰减、低忠诚可被敌<b>挖角</b>叛离。
    /// 挖角与 GDD_024 人心杠杆对玩家守将的策反对称（种子化 ADR-0006、忠者不可挖的门）。定点 [0,1]（ADR-0004）。
    /// 变更经 <see cref="RetinueState"/> 内部命令路径产出新态，不就地修改。
    /// </summary>
    public sealed class RetinueLoyaltyService
    {
        /// <summary>赏赐某僚属升忠诚（intensity∈[0,1]）。非成员则原样返回。资源成本由 Application 层扣。</summary>
        public RetinueState Reward(RetinueState state, CharacterId member, FixedPoint intensity, RetinueLoyaltyConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (!state.IsMember(member)) return state;
            FixedPoint current = AffinityOf(state, member);
            FixedPoint next = (current + config.RewardPerIntensity * intensity.Clamp(FixedPoint.Zero, FixedPoint.One))
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            return state.WithMemberAffinity(member, next);
        }

        /// <summary>推进一次忠诚衰减：各僚属好感向下降 <c>DecayPerTick</c>，不低于 <c>LoyaltyFloor</c>（已在下限者不变）。</summary>
        public RetinueState Decay(RetinueState state, RetinueLoyaltyConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            RetinueState next = state;
            foreach (RetinueMember m in state.Members)
            {
                if (m.Affinity <= config.LoyaltyFloor) continue;
                FixedPoint decayed = m.Affinity - config.DecayPerTick;
                if (decayed < config.LoyaltyFloor) decayed = config.LoyaltyFloor;
                next = next.WithMemberAffinity(m.Character, decayed);
            }
            return next;
        }

        /// <summary>
        /// 敌挖角某僚属（GDD_014 被挖角，种子化确定性）：忠诚 ≥ 阈值则不可挖（返回 Left=false）；
        /// 否则 chance = base + pull·拉拢力 + vuln·(阈值−好感)，种子判定叛离则移除该员。
        /// </summary>
        public PoachResult AttemptPoach(
            RetinueState state, CharacterId member, FixedPoint poacherPull, ulong seed, RetinueLoyaltyConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (!state.IsMember(member)) return new PoachResult(member, false, state, FixedPoint.Zero);

            FixedPoint affinity = AffinityOf(state, member);
            if (affinity >= config.PoachThreshold)
                return new PoachResult(member, false, state, FixedPoint.Zero);   // 忠者不叛

            FixedPoint pull = poacherPull.Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint vulnerability = config.PoachThreshold - affinity;
            FixedPoint chance = (config.PoachBase
                + config.PoachPullWeight * pull
                + config.PoachVulnerabilityWeight * vulnerability)
                .Clamp(FixedPoint.Zero, FixedPoint.One);

            bool left = new DeterministicRandom(seed).NextUnit() < chance;
            return new PoachResult(member, left, left ? state.WithoutMember(member) : state, chance);
        }

        private static FixedPoint AffinityOf(RetinueState state, CharacterId member)
        {
            foreach (RetinueMember m in state.Members)
                if (m.Character == member) return m.Affinity;
            return FixedPoint.Zero;
        }
    }
}
