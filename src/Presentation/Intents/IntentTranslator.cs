using System;

namespace ThreeKingdom.Presentation.Intents
{
    /// <summary>
    /// UI 意图 → Application 命令的<b>显式映射</b>（ADR-0002 状态变更协议 1：Presentation 构造意图 Command）。
    /// <b>纯映射</b>：只把意图翻译为命令载荷，<b>不执行任何规则</b>、不触达 Domain、不验证身份/时机
    /// （验证是 Application 层职责，§协议 2）。同一意图 → 同一命令（确定性）。
    /// </summary>
    public sealed class IntentTranslator
    {
        /// <summary>把一个 UI 意图翻译为对应的 Application 命令。</summary>
        public IApplicationCommand Translate(IUiIntent intent)
        {
            switch (intent)
            {
                case NewGameIntent _: return new StartNewGameCommand();
                case LoadGameIntent g: return new LoadGameCommand(g.Slot);
                case SaveGameIntent g: return new SaveGameCommand(g.Slot);
                case ScoutIntent s: return new ScoutCommand(s.Subject);
                case SubmitPlanIntent p: return new SubmitPlanCommand(p.PlanId);
                case null: throw new ArgumentNullException(nameof(intent));
                default: throw new NotSupportedException($"未支持的 UI 意图类型：{intent.GetType().Name}。");
            }
        }
    }
}
