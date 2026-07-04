using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;

namespace ThreeKingdom.Application.Battle
{
    /// <summary>
    /// 守将施计画像工厂（GDD_024 §6，Application 装配）：把<b>反全知情报信号</b>（是否侦察 / 情报质量）
    /// 与<b>种子化守将性情</b>（按城 id 确定性生成——每座敌城守将性情固定但须侦察才可读）合成
    /// <see cref="SubversionTargetProfile"/>。暴露（曾被识破）→ 守将警觉↑。纯确定性，不读世界真值。
    /// </summary>
    public static class SubversionTargetProfileFactory
    {
        /// <summary>
        /// 生成目标守将画像。<paramref name="scouted"/>/<paramref name="intelQuality"/> 由运行期从情报派生（反全知门）；
        /// 性情由 <paramref name="city"/> + <paramref name="worldSeed"/> 种子化；<paramref name="exposed"/> 提升警觉。
        /// </summary>
        public static SubversionTargetProfile Build(
            CityId city, bool scouted, FixedPoint intelQuality, bool exposed, ulong worldSeed)
            => Build(new CharacterId("def-" + city.Value), scouted, intelQuality, exposed, worldSeed);

        /// <summary>
        /// 同上，但目标为<b>世界模型的真实守将/君主</b>（<paramref name="general"/>）——性情随此人确定性生成
        /// （同一人物性情一致，跨城可复现），施计指向具名武将而非合成守将。
        /// </summary>
        public static SubversionTargetProfile Build(
            CharacterId general, bool scouted, FixedPoint intelQuality, bool exposed, ulong worldSeed)
        {
            ulong baseSeed = worldSeed ^ Fnv(general.Value);
            FixedPoint loyalty = Unit(baseSeed, 0x11);
            FixedPoint resentment = Unit(baseSeed, 0x22);
            FixedPoint greed = Unit(baseSeed, 0x33);
            FixedPoint charm = Unit(baseSeed, 0x44);
            FixedPoint alertness = Unit(baseSeed, 0x55);
            if (exposed)   // 曾被识破 → 警觉大增（后续更易反噬）
                alertness = (alertness + FixedPoint.FromFraction(4, 10)).Clamp(FixedPoint.Zero, FixedPoint.One);

            // 反全知：未侦察 → 情报质量归 0（守将弱点读不清，SubversionService 内亦以 0 处理）。
            FixedPoint quality = scouted ? intelQuality.Clamp(FixedPoint.Zero, FixedPoint.One) : FixedPoint.Zero;

            return new SubversionTargetProfile(
                general, loyalty, resentment, greed, charm, alertness, scouted, quality);
        }

        /// <summary>由种子 + 盐派生 [0,1] 定点（确定性，均匀）。</summary>
        private static FixedPoint Unit(ulong seed, ulong salt)
            => new DeterministicRandom(seed, salt).NextUnit();

        private static ulong Fnv(string s)
        {
            ulong h = 1469598103934665603UL;
            if (s != null) foreach (char c in s) { h ^= c; h *= 1099511628211UL; }
            return h;
        }
    }
}
