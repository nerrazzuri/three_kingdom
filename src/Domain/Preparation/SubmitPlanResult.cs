using System;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 提交计划结果（GDD_009 §Formula 1/3 / TR-prep-001）。
    /// 成功：携带不可变 <see cref="CommittedPlan"/> 与扣减后的新资源池。
    /// 失败：携带聚合的校验结果（稳定错误码），资源池<b>原样不变</b>（零部分写入，全有或全无）。不可变。
    /// </summary>
    public sealed class SubmitPlanResult
    {
        /// <summary>是否提交成功。</summary>
        public bool Committed { get; }

        /// <summary>已承诺计划（成功时非空）。</summary>
        public CommittedPlan? Plan { get; }

        /// <summary>提交后资源池（成功为扣减后新池；失败为原池，未变）。</summary>
        public ResourcePool ResultingPool { get; }

        /// <summary>校验结果（成功也含软风险列表）。</summary>
        public PlanValidationResult Validation { get; }

        private SubmitPlanResult(bool committed, CommittedPlan? plan, ResourcePool resultingPool, PlanValidationResult validation)
        {
            Committed = committed;
            Plan = plan;
            ResultingPool = resultingPool;
            Validation = validation;
        }

        public static SubmitPlanResult Success(CommittedPlan plan, ResourcePool resultingPool, PlanValidationResult validation)
            => new SubmitPlanResult(true, plan, resultingPool, validation);

        public static SubmitPlanResult Failure(ResourcePool unchangedPool, PlanValidationResult validation)
            => new SubmitPlanResult(false, null, unchangedPool, validation);
    }
}
