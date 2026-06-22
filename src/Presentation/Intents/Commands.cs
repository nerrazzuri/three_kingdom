using System;

namespace ThreeKingdom.Presentation.Intents
{
    /// <summary>
    /// Application 命令的表现层契约（ADR-0002：Presentation 构造意图 Command 交 Application 执行）。
    /// 这是表现层与 Application 之间的<b>接缝</b>——表现层<b>只</b>产出这些不可变命令载荷，
    /// <b>不</b>自行执行规则、不触达 Domain。Application 层订阅并验证/解析它们。
    /// </summary>
    public interface IApplicationCommand { }

    /// <summary>开始新游戏。</summary>
    public sealed class StartNewGameCommand : IApplicationCommand { }

    /// <summary>读取存档槽。</summary>
    public sealed class LoadGameCommand : IApplicationCommand
    {
        /// <summary>存档槽名。</summary>
        public string Slot { get; }
        public LoadGameCommand(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            Slot = slot;
        }
    }

    /// <summary>保存到存档槽。</summary>
    public sealed class SaveGameCommand : IApplicationCommand
    {
        /// <summary>存档槽名。</summary>
        public string Slot { get; }
        public SaveGameCommand(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            Slot = slot;
        }
    }

    /// <summary>侦察某主题（刷新情报）。</summary>
    public sealed class ScoutCommand : IApplicationCommand
    {
        /// <summary>侦察主题标识。</summary>
        public string Subject { get; }
        public ScoutCommand(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("主题不可为空。", nameof(subject));
            Subject = subject;
        }
    }

    /// <summary>提交战前计划。</summary>
    public sealed class SubmitPlanCommand : IApplicationCommand
    {
        /// <summary>计划标识。</summary>
        public string PlanId { get; }
        public SubmitPlanCommand(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("计划标识不可为空。", nameof(planId));
            PlanId = planId;
        }
    }
}
