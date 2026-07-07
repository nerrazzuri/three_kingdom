using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 演义覆盖层 ↔ 运行时人生态桥（GDD_027 #1 / ADR-0017 + ADR-0016 D6）：把演义事件产生的斩杀/移籍效果
    /// <b>幂等</b>同步进 <see cref="GeneralLedger"/>——斩杀者标重创（陨落代理）、移籍者换主。仅在与现态<b>有别</b>时施加，
    /// 故可每推进/读取安全重放（不重复重置忠诚）。纯函数演化，确定性。
    /// </summary>
    public static class GeneralLifeReconciler
    {
        /// <summary>依当前演义覆盖态同步台账（幂等）。<paramref name="anchorYear"/> 供惰性铸初态。</summary>
        public static void ApplyLore(GeneralLedger ledger, LoreOverrides overrides, int anchorYear)
        {
            if (ledger == null || overrides == null || overrides.IsEmpty) return;

            // 移籍：换主君（忠诚重置为中性起点），仅当现主与目标不同（幂等）。
            foreach (var kv in overrides.Reassigned)
            {
                var g = new CharacterId(kv.Key);
                FactionId? target = kv.Value;
                GeneralState s = ledger.GetOrSeed(g, x => GeneralLifeSeeding.Seed(x, anchorYear));
                if (!FactionEquals(s.Faction, target))
                    ledger.Set(GeneralLifeService.Reassign(s, target, null));
            }

            // 斩杀：标重创（陨落代理；GeneralState 无独立死亡位，重创=不可受命）。幂等。
            foreach (string id in overrides.Slain)
            {
                var g = new CharacterId(id);
                GeneralState s = ledger.GetOrSeed(g, x => GeneralLifeSeeding.Seed(x, anchorYear));
                if (s.Health != GeneralHealth.Grave)
                    ledger.Set(s.WithHealth(GeneralHealth.Grave));
            }
        }

        private static bool FactionEquals(FactionId? a, FactionId? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Value.Equals(b.Value);
        }
    }
}
