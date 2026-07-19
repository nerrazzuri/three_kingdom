using NUnit.Framework;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// 反全知敌军兵力区间估计（visual-battle-scene GDD §4 F2）：已侦察 → 真值精确显示；
    /// 未侦察 → 确定性区间中点 + "约 X–Y" 标签，视图不暴露真值；真值 0 → "未见敌踪"，不造假非零区间。
    /// FogBandWidth=0.20（5 档），边界值取自 GDD F2 示例。
    /// </summary>
    [TestFixture]
    public class ZoneBattleViewFogTests
    {
        [Test]
        public void test_fogband_revealed_shows_exact_value()
        {
            var (display, label) = ZoneBattleView.FogBand(300, 500, revealed: true);
            Assert.That(display, Is.EqualTo(300));
            Assert.That(label, Is.EqualTo("300"));
        }

        [Test]
        public void test_fogband_unrevealed_zero_shows_no_enemy_sighted()
        {
            var (display, label) = ZoneBattleView.FogBand(0, 500, revealed: false);
            Assert.That(display, Is.EqualTo(0));
            Assert.That(label, Is.EqualTo("未见敌踪"));
        }

        [Test]
        public void test_fogband_unrevealed_low_strength_lands_in_band_zero()
        {
            // GDD 边界：capacity 500，trueStrength 1 → band 0 → "约 0–100"，中点 50。
            var (display, label) = ZoneBattleView.FogBand(1, 500, revealed: false);
            Assert.That(display, Is.EqualTo(50));
            Assert.That(label, Is.EqualTo("约 0–100"));
        }

        [Test]
        public void test_fogband_unrevealed_mid_strength_bands_deterministically()
        {
            // ratio 0.6 → band 3 → lower 300 / upper 400 → "约 300–400"，中点 350。
            var (display, label) = ZoneBattleView.FogBand(300, 500, revealed: false);
            Assert.That(display, Is.EqualTo(350));
            Assert.That(label, Is.EqualTo("约 300–400"));
        }

        [Test]
        public void test_fogband_unrevealed_top_band_caps_with_plus()
        {
            // ratio 1.0 → band 封顶 4 → lower 400 / upper 500 → "约 400–500+"，中点 450。
            var (display, label) = ZoneBattleView.FogBand(500, 500, revealed: false);
            Assert.That(display, Is.EqualTo(450));
            Assert.That(label, Is.EqualTo("约 400–500+"));
        }

        [Test]
        public void test_fogband_unrevealed_overflow_strength_still_capped_no_leak()
        {
            // 真值远超容量仍封顶档，绝不因未侦察泄露真实兵力。
            var (display, label) = ZoneBattleView.FogBand(900, 500, revealed: false);
            Assert.That(display, Is.EqualTo(450));
            Assert.That(label, Is.EqualTo("约 400–500+"));
        }

        [Test]
        public void test_fogband_deterministic_same_input_same_output()
        {
            var a = ZoneBattleView.FogBand(273, 500, revealed: false);
            var b = ZoneBattleView.FogBand(273, 500, revealed: false);
            Assert.That(a, Is.EqualTo(b));
        }
    }
}
