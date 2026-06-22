using System;

namespace ThreeKingdom.Presentation.Intents
{
    /// <summary>玩家 UI 意图（点击/按键产生的原始操作）。由 <see cref="IntentTranslator"/> 映射为 Application 命令。</summary>
    public interface IUiIntent { }

    /// <summary>「新游戏」意图。</summary>
    public sealed class NewGameIntent : IUiIntent { }

    /// <summary>「读档」意图。</summary>
    public sealed class LoadGameIntent : IUiIntent
    {
        public string Slot { get; }
        public LoadGameIntent(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            Slot = slot;
        }
    }

    /// <summary>「存档」意图。</summary>
    public sealed class SaveGameIntent : IUiIntent
    {
        public string Slot { get; }
        public SaveGameIntent(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            Slot = slot;
        }
    }

    /// <summary>「侦察」意图。</summary>
    public sealed class ScoutIntent : IUiIntent
    {
        public string Subject { get; }
        public ScoutIntent(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("主题不可为空。", nameof(subject));
            Subject = subject;
        }
    }

    /// <summary>「提交计划」意图。</summary>
    public sealed class SubmitPlanIntent : IUiIntent
    {
        public string PlanId { get; }
        public SubmitPlanIntent(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("计划标识不可为空。", nameof(planId));
            PlanId = planId;
        }
    }
}
