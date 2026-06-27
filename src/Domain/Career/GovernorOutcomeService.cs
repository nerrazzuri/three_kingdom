using System;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 守城开局事件的生涯后果结算（GDD_014 §Main Rules / §Failure Cases / TR-career-001 / ADR-0002）。
    /// 纯函数、确定性。胜：经命令路径加初始功绩 + 君主信任；败：转在野（罢官、保留部曲），产出<b>合法可继续态</b>。
    /// <b>不</b>触碰城池归属（ADR-0008 归属唯一权威在 GDD_004，由 Application 编排发起控制权变更）。
    /// </summary>
    public sealed class GovernorOutcomeService
    {
        private readonly CareerStateService _career = new CareerStateService();

        /// <summary>守城胜后果：解锁全城权限（标志由调用方记录）+ 初始功绩 + 君主初始信任。经命令路径写入。</summary>
        public CareerCommandResult ResolveDefended(CareerSnapshot before, GovernorStartConfig config)
        {
            if (before is null) throw new ArgumentNullException(nameof(before));
            if (config is null) throw new ArgumentNullException(nameof(config));

            CareerCommandResult merit = _career.Apply(before, new GainMeritCommand(config.InitialMerit, 0));
            if (!merit.Applied) return merit;

            if (config.InitialLordStanding != Numerics.FixedPoint.Zero)
                return _career.Apply(merit.Snapshot, new AdjustLordStandingCommand(config.InitialLordStanding));
            return merit;
        }

        /// <summary>
        /// 守城败后果：失城罢官 → 转在野，保留核心部曲。产出合法可继续态（与自立众叛流浪态同构）。
        /// 不写城池归属——失城的归属变更由 Application 经 GDD_004 控制权变更发起。
        /// </summary>
        public CareerSnapshot ResolveFallen(CareerSnapshot before)
        {
            if (before is null) throw new ArgumentNullException(nameof(before));
            // 保留部曲（Retinue 不变），仅生涯转在野。
            return new CareerSnapshot(before.Career.IntoWandering(), before.Retinue);
        }
    }
}
