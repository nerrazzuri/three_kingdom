using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Defeat
{
    /// <summary>
    /// 被灭之后的处境阶段（GDD_026 补：势力覆灭≠game over，唯身死才终）。
    /// 流程：被俘 → 胜者判生死（<see cref="Executed"/>/存）→ 存则可归顺（<see cref="Submitted"/>）；
    /// 不归顺 → 胜者判释放（<see cref="Released"/>）/囚禁（<see cref="Imprisoned"/>）；释放 → 投奔他主，
    /// 被收留则复为太守（<see cref="Reseated"/>）。身死（被杀/死节/寿终）→ <see cref="LifeEnded"/>。
    /// </summary>
    public enum DefeatStage
    {
        /// <summary>被俘（待胜者发落）。</summary>
        Captured = 0,
        /// <summary>被处死（身死 → 一世终）。</summary>
        Executed = 1,
        /// <summary>已归顺（事新主，复为其太守）。</summary>
        Submitted = 2,
        /// <summary>不归顺、被囚禁（待后续；未获释）。</summary>
        Imprisoned = 3,
        /// <summary>获释（流亡之身，可投奔他主）。</summary>
        Released = 4,
        /// <summary>被某势力收留、复为太守（东山再起）。</summary>
        Reseated = 5,
        /// <summary>一世终（身死）。</summary>
        LifeEnded = 6,
    }

    /// <summary>被俘/流亡的 AI 判定配置（GDD_026 补，可版本化）。名声调制：有用之才不轻杀，然名高则怕放虎、亦招猜忌。</summary>
    public sealed class CaptivityConfig
    {
        /// <summary>名声归一化基准（renown / 此 → [0,~1]）。</summary>
        public int RenownReference { get; }
        /// <summary>基础不杀概率。</summary>
        public FixedPoint BaseSpare { get; }
        /// <summary>名声对"不杀"的加成（越有名越想留用）。</summary>
        public FixedPoint SpareRenownAppeal { get; }
        /// <summary>基础释放概率（不归顺时）。</summary>
        public FixedPoint BaseRelease { get; }
        /// <summary>名声对"释放"的惩罚（越有名越怕放虎归山）。</summary>
        public FixedPoint ReleaseRenownFear { get; }
        /// <summary>基础收留概率（投奔时）。</summary>
        public FixedPoint BaseRecruit { get; }
        /// <summary>名声对"收留"的加成（名将谁不想要）。</summary>
        public FixedPoint RecruitRenownAppeal { get; }
        /// <summary>名声高于此归一化阈值 → 招猜忌，削收留（功高震主之虑）。</summary>
        public FixedPoint RecruitSuspicionThreshold { get; }
        /// <summary>猜忌对"收留"的惩罚强度。</summary>
        public FixedPoint RecruitSuspicion { get; }
        /// <summary>可收留太守的势力最少领城数（太小则自顾不暇，无以安置）。</summary>
        public int RefugeMinLordCities { get; }

        public CaptivityConfig(
            int renownReference, FixedPoint baseSpare, FixedPoint spareRenownAppeal,
            FixedPoint baseRelease, FixedPoint releaseRenownFear,
            FixedPoint baseRecruit, FixedPoint recruitRenownAppeal,
            FixedPoint recruitSuspicionThreshold, FixedPoint recruitSuspicion, int refugeMinLordCities)
        {
            RenownReference = renownReference <= 0 ? 1 : renownReference;
            BaseSpare = baseSpare;
            SpareRenownAppeal = spareRenownAppeal;
            BaseRelease = baseRelease;
            ReleaseRenownFear = releaseRenownFear;
            BaseRecruit = baseRecruit;
            RecruitRenownAppeal = recruitRenownAppeal;
            RecruitSuspicionThreshold = recruitSuspicionThreshold;
            RecruitSuspicion = recruitSuspicion;
            RefugeMinLordCities = refugeMinLordCities;
        }

        /// <summary>默认（GDD_026 补）。</summary>
        public static CaptivityConfig Default { get; } = new CaptivityConfig(
            renownReference: 800,
            baseSpare: F(45), spareRenownAppeal: F(45),
            baseRelease: F(60), releaseRenownFear: F(45),
            baseRecruit: F(40), recruitRenownAppeal: F(45),
            recruitSuspicionThreshold: F(75), recruitSuspicion: F(80),
            refugeMinLordCities: 2);

        private static FixedPoint F(int n) => FixedPoint.FromFraction(n, 100);
    }

    /// <summary>
    /// 被俘/流亡的 AI 判定（GDD_026 补，<b>确定性纯函数</b>，ADR-0004/0006）：胜者杀不杀、放不放，他主收不收——
    /// 皆种子化 + 名声调制，不掷骰、可复现。<b>撬动叙事</b>：名将不轻杀（有用），然名高则怕放虎、亦招功高之忌。
    /// </summary>
    public sealed class CaptivityService
    {
        /// <summary>名声归一化到 [0,1]（超过基准按 1 计）。</summary>
        private static FixedPoint RenownNorm(int renown, CaptivityConfig cfg)
        {
            if (renown <= 0) return FixedPoint.Zero;
            FixedPoint n = FixedPoint.FromFraction(renown, cfg.RenownReference);
            return n > FixedPoint.One ? FixedPoint.One : n;
        }

        /// <summary>胜者是否<b>不杀</b>（存主角）：名声越高越想留用 → 越可能不杀。种子化。</summary>
        public bool CaptorSpares(int renown, ulong seed, CaptivityConfig cfg)
        {
            FixedPoint p = (cfg.BaseSpare + cfg.SpareRenownAppeal * RenownNorm(renown, cfg))
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            return new DeterministicRandom(seed).NextUnit() < p;
        }

        /// <summary>不归顺时胜者是否<b>释放</b>：名声越高越怕放虎归山 → 越不肯放。种子化。</summary>
        public bool CaptorReleases(int renown, ulong seed, CaptivityConfig cfg)
        {
            FixedPoint p = (cfg.BaseRelease - cfg.ReleaseRenownFear * RenownNorm(renown, cfg))
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            return new DeterministicRandom(seed).NextUnit() < p;
        }

        /// <summary>
        /// 投奔某势力是否被<b>收留</b>（复为其太守）：名声助力，然过阈招猜忌反削；且该势力须够大（≥ 最少领城）方能安置。
        /// 种子化确定性。
        /// </summary>
        public bool LordAcceptsRefuge(int renown, int lordCities, ulong seed, CaptivityConfig cfg)
        {
            if (lordCities < cfg.RefugeMinLordCities) return false;   // 自顾不暇，无以安置太守
            FixedPoint norm = RenownNorm(renown, cfg);
            FixedPoint suspicion = norm > cfg.RecruitSuspicionThreshold
                ? (norm - cfg.RecruitSuspicionThreshold) * cfg.RecruitSuspicion
                : FixedPoint.Zero;
            FixedPoint p = (cfg.BaseRecruit + cfg.RecruitRenownAppeal * norm - suspicion)
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            return new DeterministicRandom(seed).NextUnit() < p;
        }
    }
}
