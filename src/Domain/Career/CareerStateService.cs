using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯命令解析服务（Domain 结算层，ADR-0002：Domain 解析 + ADR-0004：确定性）。
    /// 接受前态快照 + 一条命令，校验不变量后产出<b>新</b>快照或稳定错误码；纯函数、无随机、无时间依赖。
    /// 失败一律返回原样未变的前态（无部分写入，TR-career-005）。
    /// <para>
    /// 本骨架只实现结构性校验（非负累积 / 逐级晋升 / 在野约束 / 任免对象存在）；
    /// 晋升数值门槛（merit/renown/standing_req）与自立判定属 story-002/003，<b>不</b>在此。
    /// </para>
    /// </summary>
    public sealed class CareerStateService
    {
        private static readonly FixedPoint StandingMin = FixedPoint.Zero;
        private static readonly FixedPoint StandingMax = FixedPoint.One;

        /// <summary>解析一条命令。</summary>
        /// <param name="before">前态快照（非空）。</param>
        /// <param name="command">命令（非空——空命令前置应在 Application 层拦截）。</param>
        public CareerCommandResult Apply(CareerSnapshot before, CareerCommand command)
        {
            if (before is null) throw new ArgumentNullException(nameof(before));
            if (command is null) throw new ArgumentNullException(nameof(command));

            switch (command)
            {
                case GainMeritCommand g: return ApplyGainMerit(before, g);
                case AdjustLordStandingCommand s: return ApplyAdjustStanding(before, s);
                case PromoteRankCommand p: return ApplyPromote(before, p);
                case AssignOfficeCommand a: return ApplyAssignOffice(before, a);
                case DismissOfficeCommand d: return ApplyDismissOffice(before, d);
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), $"未知生涯命令类型：{command.GetType().Name}。");
            }
        }

        private static CareerCommandResult ApplyGainMerit(CareerSnapshot before, GainMeritCommand cmd)
        {
            if (cmd.MeritDelta < 0)
                return CareerCommandResult.Failure(before, CareerErrorCode.NegativeMeritGain, $"功绩增量为负：{cmd.MeritDelta}。");
            if (cmd.RenownDelta < 0)
                return CareerCommandResult.Failure(before, CareerErrorCode.NegativeRenownGain, $"名望增量为负：{cmd.RenownDelta}。");

            CareerState career = before.Career;
            int newMerit = checked(career.Merit + cmd.MeritDelta);
            int newRenown = checked(career.Renown + cmd.RenownDelta);
            CareerState next = career.With(merit: newMerit, renown: newRenown);
            return CareerCommandResult.Success(new CareerSnapshot(next, before.Retinue));
        }

        private static CareerCommandResult ApplyAdjustStanding(CareerSnapshot before, AdjustLordStandingCommand cmd)
        {
            CareerState career = before.Career;
            if (career.IsUnaffiliated)
                return CareerCommandResult.Failure(before, CareerErrorCode.Unaffiliated, "在野（无君主）不可调整君主好感。");

            // lord_standing 为可升可降的有源有汇值（GDD_014 N10）：结算后钳制到 [0,1]，非越界报错。
            FixedPoint adjusted = (career.LordStanding + cmd.Delta).Clamp(StandingMin, StandingMax);
            CareerState next = career.With(lordStanding: adjusted);
            return CareerCommandResult.Success(new CareerSnapshot(next, before.Retinue));
        }

        private static CareerCommandResult ApplyPromote(CareerSnapshot before, PromoteRankCommand cmd)
        {
            CareerState career = before.Career;
            if (career.IsUnaffiliated)
                return CareerCommandResult.Failure(before, CareerErrorCode.Unaffiliated, "在野（无君主）不可晋升官阶。");
            if (career.Rank == Rank.Successor)
                return CareerCommandResult.Failure(before, CareerErrorCode.AlreadyAtMaxRank, "已达最高阶，无可晋升目标。");

            var nextRank = (Rank)((int)career.Rank + 1);
            if (cmd.TargetRank != nextRank)
                return CareerCommandResult.Failure(
                    before, CareerErrorCode.RankSkipNotAllowed,
                    $"越级晋升：当前 {career.Rank}，仅可晋升至 {nextRank}，目标为 {cmd.TargetRank}。");

            // 注：merit/renown/standing 门槛达标判定属 story-002（配置阈值）；本骨架仅放行逐级结构。
            CareerState next = career.With(rank: nextRank);
            return CareerCommandResult.Success(new CareerSnapshot(next, before.Retinue));
        }

        private static CareerCommandResult ApplyAssignOffice(CareerSnapshot before, AssignOfficeCommand cmd)
        {
            RetinueState retinue = before.Retinue;
            if (!retinue.IsMember(cmd.Holder))
                return CareerCommandResult.Failure(
                    before, CareerErrorCode.UnknownRetinueMember,
                    $"任免对象不在僚属列表：{cmd.Holder}。");

            RetinueState next = retinue.WithOffice(cmd.Role, cmd.Holder);
            return CareerCommandResult.Success(new CareerSnapshot(before.Career, next));
        }

        private static CareerCommandResult ApplyDismissOffice(CareerSnapshot before, DismissOfficeCommand cmd)
        {
            RetinueState retinue = before.Retinue;
            CharacterId? holder = retinue.Holder(cmd.Role);
            if (holder == null)
                return CareerCommandResult.Failure(before, CareerErrorCode.NoOfficeHolder, $"该官职位无人在任，无可撤：{cmd.Role}。");

            // 撤职 → 前任派系不满：好感下降（钳制 [0,1]）。喂忠诚经营（可能因此跌破可挖角阈）。
            FixedPoint before2 = AffinityOf(retinue, holder.Value);
            FixedPoint discontented = (before2 - cmd.Discontent).Clamp(FixedPoint.Zero, FixedPoint.One);
            RetinueState next = retinue.WithoutOffice(cmd.Role).WithMemberAffinity(holder.Value, discontented);
            return CareerCommandResult.Success(new CareerSnapshot(before.Career, next));
        }

        private static FixedPoint AffinityOf(RetinueState retinue, CharacterId member)
        {
            foreach (RetinueMember m in retinue.Members)
                if (m.Character == member) return m.Affinity;
            return FixedPoint.Zero;
        }
    }
}
