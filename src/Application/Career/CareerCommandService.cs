using System;
using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Application.Career
{
    /// <summary>
    /// 生涯命令的 Application 写路径（ADR-0002：玩家操作经 Command/Application Service 唯一写路径）。
    /// 本层负责命令的<b>前置校验</b>（存在性 / 身份 / 时机），随后委派 Domain
    /// <see cref="CareerStateService"/> 做状态结算。UI 不得绕过本路径直接改 Domain 状态。
    /// <para>
    /// 骨架阶段前置校验为：命令非空、快照非空。身份/时机门（任务时段、君主在世等）随
    /// story-002/004 接入世界态势后充实——此处保留单一切入点，避免后续散落写路径。
    /// </para>
    /// </summary>
    public sealed class CareerCommandService
    {
        private readonly CareerStateService _domain;

        /// <summary>注入 Domain 结算服务（依赖注入，便于测试，ADR-0002）。</summary>
        public CareerCommandService(CareerStateService domain)
        {
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        }

        /// <summary>默认构造：自建 Domain 结算服务。</summary>
        public CareerCommandService() : this(new CareerStateService()) { }

        /// <summary>
        /// 提交一条生涯命令。前置校验失败返回稳定错误码、快照原样不变（无部分写入）；
        /// 否则委派 Domain 结算。
        /// </summary>
        /// <param name="current">当前生涯快照（非空）。</param>
        /// <param name="command">玩家命令（空命令返回 <see cref="CareerErrorCode.NullCommand"/>）。</param>
        public CareerCommandResult Submit(CareerSnapshot current, CareerCommand command)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            if (command is null)
                return CareerCommandResult.Failure(current, CareerErrorCode.NullCommand, "命令为空。");

            return _domain.Apply(current, command);
        }
    }
}
