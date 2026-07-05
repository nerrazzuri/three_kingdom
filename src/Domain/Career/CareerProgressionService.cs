using System;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 忠臣晋升与生涯累积的配置化结算服务（GDD_014 §Formula 1 / TR-career-002 / ADR-0003 + ADR-0004）。
    /// 在 story-001 命令路径之上叠加配置门槛：来源增益按 <see cref="PromotionLadderConfig"/> 累积，
    /// 晋升须三项门槛达标方放行。纯函数、确定性；复用 <see cref="CareerStateService"/> 做底层不变量结算与单一写路径。
    /// </summary>
    public sealed class CareerProgressionService
    {
        private readonly CareerStateService _career = new CareerStateService();
        private readonly PromotionGate _gate = new PromotionGate();

        /// <summary>
        /// 应用一次具名来源累积（merit/renown 经 GainMerit、lord_standing 经 AdjustLordStanding，均走命令路径）。
        /// 来源未配置 → 视为零增益恒等返回成功；<paramref name="count"/> ≥1。
        /// </summary>
        public CareerCommandResult ApplyGain(
            PromotionLadderConfig config, CareerSnapshot snapshot, CareerGainSource source, int count = 1)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "来源次数须 ≥1。");

            CareerGain? baseGain = config.GainFor(source);
            if (baseGain is null) return CareerCommandResult.Success(snapshot); // 零增益来源：恒等

            CareerGain gain = baseGain.Scale(count);

            // 1) merit/renown 经 GainMerit 命令。
            CareerCommandResult meritResult = _career.Apply(snapshot, new GainMeritCommand(gain.Merit, gain.Renown));
            if (!meritResult.Applied) return meritResult;

            // 2) lord_standing 经 AdjustLordStanding 命令（在野则该命令自身返回稳定错误码）。
            if (gain.Standing != Numerics.FixedPoint.Zero)
            {
                CareerCommandResult standingResult =
                    _career.Apply(meritResult.Snapshot, new AdjustLordStandingCommand(gain.Standing));
                // 在野时 standing 调整失败：回退到累积前态（无部分写入），返回该错误。
                if (!standingResult.Applied)
                    return CareerCommandResult.Failure(snapshot, standingResult.Error, standingResult.Detail);
                return standingResult;
            }
            return meritResult;
        }

        /// <summary>
        /// 名望惩罚（GDD_014 / W5：君主任务失败/逾期损名望）：名望减 <paramref name="amount"/>（下限 0），
        /// merit/standing/rank 不动。纯函数，返回新快照。
        /// </summary>
        public CareerSnapshot ApplyRenownPenalty(CareerSnapshot snapshot, int amount)
        {
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
            if (amount <= 0) return snapshot;
            CareerState c = snapshot.Career;
            return new CareerSnapshot(c.With(renown: Math.Max(0, c.Renown - amount)), snapshot.Retinue);
        }

        /// <summary>判定能否晋升（不改状态），供 UI「距下一阶差距」与申请前校验。</summary>
        public PromotionCheck Check(PromotionLadderConfig config, CareerSnapshot snapshot)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
            return _gate.Evaluate(snapshot.Career, config);
        }

        /// <summary>
        /// 申请晋升（GDD_014：门槛达标才晋级）。三项达标 → 逐级晋升至下一阶（复用 story-001 结构性校验）；
        /// 未达 → 返回 <see cref="CareerErrorCode.PromotionThresholdNotMet"/> 稳定错误码、状态不变（无部分写入）。
        /// </summary>
        public CareerCommandResult RequestPromotion(PromotionLadderConfig config, CareerSnapshot snapshot)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));

            PromotionCheck check = _gate.Evaluate(snapshot.Career, config);
            if (!check.CanPromote)
            {
                CareerErrorCode code = check.Blocked
                    ? (snapshot.Career.IsUnaffiliated ? CareerErrorCode.Unaffiliated : CareerErrorCode.AlreadyAtMaxRank)
                    : CareerErrorCode.PromotionThresholdNotMet;
                return CareerCommandResult.Failure(
                    snapshot, code,
                    $"晋升门槛未达：merit缺{check.MeritShortfall} renown缺{check.RenownShortfall} standing缺{check.StandingShortfall}。");
            }

            // 门槛达标 → 走 story-001 逐级晋升命令（结构性 +1、最高阶/在野再校验一次）。
            return _career.Apply(snapshot, new PromoteRankCommand(check.TargetRank));
        }
    }
}
