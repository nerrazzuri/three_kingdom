using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Defeat
{
    /// <summary>
    /// 被灭之后的处境流程（GDD_026 补，<b>确定性状态机</b>）：势力覆灭→太守被俘，按你定的次序推进——
    /// 胜者判生死 → 存则可归顺 → 不归顺则胜者判释放/囚禁 → 获释则投奔他主（AI 判收留）。
    /// 收留/归顺者复为<b>太守</b>（不做普通武将路线）。<b>唯身死才终</b>（<see cref="IsLifeEnded"/>）。
    /// 各决策种子分流（base ⊕ 常量），确定性可复现；玩家选择（归顺/不归顺/投奔谁）由外部命令驱动。
    /// </summary>
    public sealed class DefeatFlow
    {
        private readonly int _renown;
        private readonly ulong _seed;
        private readonly CaptivityConfig _cfg;
        private readonly CaptivityService _svc = new CaptivityService();

        /// <summary>擒获者（覆灭时最强之敌）。</summary>
        public FactionId Captor { get; }
        /// <summary>当前阶段。</summary>
        public DefeatStage Stage { get; private set; }
        /// <summary>若已复为太守，效力的新主（归顺=擒获者；投奔=收留者）。否则 null。</summary>
        public FactionId? NewLord { get; private set; }

        public DefeatFlow(FactionId captor, int renown, ulong seed, CaptivityConfig cfg)
        {
            Captor = captor;
            _renown = renown;
            _seed = seed;
            _cfg = cfg ?? CaptivityConfig.Default;
            Stage = DefeatStage.Captured;
        }

        /// <summary>是否一世已终（身死：被处死）。</summary>
        public bool IsLifeEnded => Stage == DefeatStage.Executed || Stage == DefeatStage.LifeEnded;
        /// <summary>是否已复位可续玩（归顺或被收留为太守）。</summary>
        public bool CanPlayOn => Stage == DefeatStage.Submitted || Stage == DefeatStage.Reseated;

        /// <summary>
        /// 胜者发落（被俘后首步）：种子化判是否处死。处死 → <see cref="DefeatStage.Executed"/>（身死）；
        /// 不杀 → 留于 <see cref="DefeatStage.Captured"/>，待玩家决定归顺与否。仅在被俘阶段有效。
        /// </summary>
        public void ResolveCaptorFate()
        {
            if (Stage != DefeatStage.Captured) return;
            bool spared = _svc.CaptorSpares(_renown, _seed ^ 0x5A17_0001UL, _cfg);
            if (!spared) Stage = DefeatStage.Executed;
        }

        /// <summary>归顺擒获者（玩家选择）：复为其太守（<see cref="DefeatStage.Submitted"/>）。须在未处死的被俘阶段。</summary>
        public void Submit()
        {
            if (Stage != DefeatStage.Captured) return;
            NewLord = Captor;
            Stage = DefeatStage.Submitted;
        }

        /// <summary>
        /// 不归顺（玩家选择）：胜者种子化判释放/囚禁。释放 → <see cref="DefeatStage.Released"/>（可投奔）；
        /// 否则 <see cref="DefeatStage.Imprisoned"/>。须在未处死的被俘阶段。返回是否获释。
        /// </summary>
        public bool Refuse()
        {
            if (Stage != DefeatStage.Captured) return false;
            bool released = _svc.CaptorReleases(_renown, _seed ^ 0x5A17_0002UL, _cfg);
            Stage = released ? DefeatStage.Released : DefeatStage.Imprisoned;
            return released;
        }

        /// <summary>
        /// 投奔某势力求收留（获释后，玩家选择投奔谁）：AI 种子化判收不收（名声调制 + 该势力须够大）。
        /// 收留 → 复为其太守（<see cref="DefeatStage.Reseated"/>，<see cref="NewLord"/>=之）；拒 → 仍流亡（可再投他家）。
        /// 须在获释阶段。返回是否被收留。
        /// </summary>
        public bool SeekRefuge(FactionId lord, int lordCities)
        {
            if (Stage != DefeatStage.Released) return false;
            ulong s = _seed ^ 0x5A17_0003UL ^ FnvId(lord.Value);
            if (!_svc.LordAcceptsRefuge(_renown, lordCities, s, _cfg)) return false;
            NewLord = lord;
            Stage = DefeatStage.Reseated;
            return true;
        }

        private static ulong FnvId(string s)
        {
            ulong h = 1469598103934665603UL;
            if (s != null) foreach (char c in s) { h ^= c; h *= 1099511628211UL; }
            return h;
        }
    }
}
