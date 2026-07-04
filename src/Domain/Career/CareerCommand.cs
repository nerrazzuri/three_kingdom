using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯变更命令基类（ADR-0002 玩家操作唯一写路径）。不可变意图数据；
    /// 由 <see cref="CareerStateService"/> 解析为新 <see cref="CareerSnapshot"/> 或稳定错误码。
    /// 命令本身不含结算逻辑——结算确定性集中于服务（ADR-0004）。
    /// </summary>
    public abstract class CareerCommand
    {
        private protected CareerCommand() { }
    }

    /// <summary>累积功绩 / 名望（来自作战、任务、治理；来源权重在 story-002）。增量须非负。</summary>
    public sealed class GainMeritCommand : CareerCommand
    {
        /// <summary>功绩增量（须 ≥0）。</summary>
        public int MeritDelta { get; }

        /// <summary>名望增量（须 ≥0）。</summary>
        public int RenownDelta { get; }

        public GainMeritCommand(int meritDelta, int renownDelta)
        {
            MeritDelta = meritDelta;
            RenownDelta = renownDelta;
        }
    }

    /// <summary>调整君主好感（可升可降，结算后钳制到 [0,1]）。在野时非法。</summary>
    public sealed class AdjustLordStandingCommand : CareerCommand
    {
        /// <summary>好感差量（定点，可正可负）。</summary>
        public FixedPoint Delta { get; }

        public AdjustLordStandingCommand(FixedPoint delta) => Delta = delta;
    }

    /// <summary>
    /// 晋升官阶。本骨架只校验<b>逐级</b>结构（目标须为当前阶+1）与上限；
    /// 功绩/名望/好感门槛判定属 story-002，不在此实现。
    /// </summary>
    public sealed class PromoteRankCommand : CareerCommand
    {
        /// <summary>目标官阶（须为当前阶的紧邻上一阶）。</summary>
        public Rank TargetRank { get; }

        public PromoteRankCommand(Rank targetRank) => TargetRank = targetRank;
    }

    /// <summary>任免僚属担任某官职位（城守/副将/内政主事/军师）。持有者须为现有僚属。</summary>
    public sealed class AssignOfficeCommand : CareerCommand
    {
        /// <summary>官职位。</summary>
        public OfficeRole Role { get; }

        /// <summary>受任僚属 ID。</summary>
        public CharacterId Holder { get; }

        public AssignOfficeCommand(OfficeRole role, CharacterId holder)
        {
            Role = role;
            Holder = holder;
        }
    }

    /// <summary>撤职命令（GDD_014 官职任免）：撤销某官职位；前任因失位而<b>派系不满</b>——好感降 <see cref="Discontent"/>。</summary>
    public sealed class DismissOfficeCommand : CareerCommand
    {
        /// <summary>被撤的官职位。</summary>
        public OfficeRole Role { get; }

        /// <summary>前任因撤职的好感降幅（派系不满，数据驱动由 Application 提供，[0,1]）。</summary>
        public FixedPoint Discontent { get; }

        public DismissOfficeCommand(OfficeRole role, FixedPoint discontent)
        {
            Role = role;
            Discontent = discontent;
        }
    }
}
