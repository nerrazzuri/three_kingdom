using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 事件分级通报（GDD_015，<b>确定性纯函数</b>）：把历史推进结果按<b>可达性</b>分级——
    /// 够得着（前置成立/已分叉）走完整事件（Personal）；够不着的按心里话表为可述通报（Notable：通报 +
    /// <b>随主角人设着色的心里话</b>）或背景（Background：仅世界事实）。复用既有 <see cref="FireReason"/>，不改事件定义。
    /// 心里话纯为丰富体验/代入感，<b>不</b>机械改变生涯状态。
    /// </summary>
    public sealed class EventReflectionService
    {
        /// <summary>
        /// 由历史推进结果 + 主角人设产出通报。未触发（含幂等短路）→ null。
        /// 够得着（<see cref="FireReason.Diverged"/> / <see cref="FireReason.NormalPreconditionsHeld"/>）→ Personal；
        /// 够不着（<see cref="FireReason.NormalUnreachable"/>）→ 查 <paramref name="catalog"/>：命中则 Notable（按 <paramref name="persona"/> 取台词）、否则 Background。
        /// </summary>
        public EventReflection? Reflect(HistoryAdvanceResult result, MonologueCatalog catalog, ProtagonistPersona persona)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            if (!result.Fired || result.FiredOutcome is null) return null;

            string label = result.FiredOutcome.Label;

            bool reachable = result.Reason == FireReason.Diverged || result.Reason == FireReason.NormalPreconditionsHeld;
            if (reachable)
                return new EventReflection(label, NoticeTier.Personal, monologue: string.Empty);

            // 够不着：有心里话规则 → 通报 + 按人设着色的心里话；否则背景。
            MonologueRule? rule = catalog.Find(label);
            return rule != null
                ? new EventReflection(label, NoticeTier.Notable, rule.LineFor(persona))
                : new EventReflection(label, NoticeTier.Background, monologue: string.Empty);
        }
    }
}
