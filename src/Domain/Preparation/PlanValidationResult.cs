using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>硬冲突错误码（GDD_009 §Formula 1；稳定码，提交前置校验返回）。</summary>
    public enum PlanErrorCode
    {
        /// <summary>资源不足：need &gt; avail。</summary>
        ResourceShortage = 1,

        /// <summary>占用冲突：同一执行者时间窗重叠。</summary>
        TimeOverlap = 2,

        /// <summary>目标不可达。</summary>
        Unreachable = 3,

        /// <summary>执行者无权限。</summary>
        NoAuthority = 4,

        /// <summary>命令依赖图存在环（非 DAG）。</summary>
        CyclicDependency = 5,
    }

    /// <summary>软风险码（GDD_009 §UI：风险只警告不阻断，P7）。</summary>
    public enum PlanRiskCode
    {
        /// <summary>资源紧张：可提交但余量薄（含恰好够）。</summary>
        TightResource = 1,
    }

    /// <summary>一条硬冲突错误（稳定码 + 可解释明细）。不可变。</summary>
    public sealed class PlanError
    {
        public PlanErrorCode Code { get; }
        public string Detail { get; }
        public PlanError(PlanErrorCode code, string detail) { Code = code; Detail = detail ?? string.Empty; }
        public override string ToString() => $"{Code}: {Detail}";
    }

    /// <summary>一条软风险（不阻断提交）。不可变。</summary>
    public sealed class PlanRisk
    {
        public PlanRiskCode Code { get; }
        public string Detail { get; }
        public PlanRisk(PlanRiskCode code, string detail) { Code = code; Detail = detail ?? string.Empty; }
        public override string ToString() => $"{Code}: {Detail}";
    }

    /// <summary>
    /// 计划校验结果（GDD_009 §Data Model：PlanValidationResult / TR-prep-002）。
    /// <b>区分错误与风险</b>：硬冲突（Errors）阻断提交；软风险（Risks）只警告（P7）。
    /// 错误<b>一次性聚合返回</b>（列全部硬冲突，非首个即停）。确定性（同计划同结论）。不可变。
    /// </summary>
    public sealed class PlanValidationResult
    {
        /// <summary>硬冲突（阻断提交）。</summary>
        public IReadOnlyList<PlanError> Errors { get; }

        /// <summary>软风险（不阻断）。</summary>
        public IReadOnlyList<PlanRisk> Risks { get; }

        public PlanValidationResult(IReadOnlyList<PlanError> errors, IReadOnlyList<PlanRisk> risks)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
            Risks = risks ?? throw new ArgumentNullException(nameof(risks));
        }

        /// <summary>是否可提交（无任何硬冲突）。</summary>
        public bool CanCommit => Errors.Count == 0;

        /// <summary>是否含指定错误码。</summary>
        public bool HasError(PlanErrorCode code)
        {
            foreach (PlanError e in Errors) if (e.Code == code) return true;
            return false;
        }
    }
}
