// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 天气由种子+时间确定，只减速≥1.0（GDD_002，与 GDD_003 对齐）
// Date: 2026-06-21

using TkSlice.Domain.Numerics;
using TkSlice.Domain.Time;

namespace TkSlice.Domain.Environment
{
    public enum WeatherKind { Clear, Rain, Storm }

    /// <summary>
    /// 确定性天气：由世界种子 + 时段数派生，玩家只能预测与等待，不能直接操控。
    /// 暴雨/风暴提高敌军行军与强攻耗时修正（mod_weather ≥ 1.0，绝不加速）。
    /// </summary>
    public static class Weather
    {
        public static WeatherKind At(ulong worldSeed, WorldDay day)
        {
            var rng = DetRng.Fork(worldSeed, "weather#" + day.TotalSegments);
            int roll = rng.NextInt(100);
            // 70% Clear / 22% Rain / 8% Storm（数据驱动可调；此处为 slice 常量）
            if (roll < 70) return WeatherKind.Clear;
            if (roll < 92) return WeatherKind.Rain;
            return WeatherKind.Storm;
        }

        /// <summary>天气对敌方强攻/行军的耗时修正（≥1.0）。</summary>
        public static Fixed AssaultTimeMod(WeatherKind w) => w switch
        {
            WeatherKind.Clear => Fixed.OneValue,
            WeatherKind.Rain => Fixed.FromFraction(130, 100),
            WeatherKind.Storm => Fixed.FromFraction(170, 100),
            _ => Fixed.OneValue,
        };

        /// <summary>暴雨/风暴会延阻敌军强攻发起（守城待变的「拖延窗口」）。</summary>
        public static bool DelaysAssault(WeatherKind w) => w != WeatherKind.Clear;

        public static string Name(WeatherKind w) => w switch
        {
            WeatherKind.Clear => "晴",
            WeatherKind.Rain => "雨",
            WeatherKind.Storm => "风暴",
            _ => "?"
        };
    }
}
