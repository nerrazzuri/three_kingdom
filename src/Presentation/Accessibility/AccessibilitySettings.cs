using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ThreeKingdom.Presentation.Accessibility
{
    /// <summary>色盲模式（hud.md §10.1）。</summary>
    public enum ColorblindMode
    {
        /// <summary>无。</summary>
        None = 0,
        /// <summary>红色盲。</summary>
        Protanopia = 1,
        /// <summary>绿色盲。</summary>
        Deuteranopia = 2,
        /// <summary>蓝色盲。</summary>
        Tritanopia = 3,
    }

    /// <summary>
    /// 无障碍设置（accessibility-requirements WCAG 2.1 AA / hud.md §10 / ADR-0002）。
    /// 纯表现层关注点，<b>不</b>改 gameplay 规则。构造校验范围；提供确定性
    /// <see cref="Serialize"/>/<see cref="Parse"/> 以经端口跨会话持久（round-trip 一致）。不可变。
    /// </summary>
    public sealed class AccessibilitySettings
    {
        /// <summary>文本缩放下限（%）。</summary>
        public const int MinTextScale = 100;
        /// <summary>文本缩放上限（%）。</summary>
        public const int MaxTextScale = 200;

        /// <summary>文本缩放百分比 [100,200]。</summary>
        public int TextScalePercent { get; }

        /// <summary>色盲模式。</summary>
        public ColorblindMode Colorblind { get; }

        /// <summary>减少动态（停用动效）。</summary>
        public bool ReduceMotion { get; }

        /// <summary>HUD 各元素可见性（缺省视为可见）。</summary>
        public IReadOnlyDictionary<string, bool> HudVisibility { get; }

        public AccessibilitySettings(int textScalePercent, ColorblindMode colorblind, bool reduceMotion,
            IReadOnlyDictionary<string, bool>? hudVisibility = null)
        {
            if (textScalePercent < MinTextScale || textScalePercent > MaxTextScale)
                throw new ArgumentOutOfRangeException(nameof(textScalePercent), $"文本缩放须在 [{MinTextScale},{MaxTextScale}]。");
            if (!Enum.IsDefined(typeof(ColorblindMode), colorblind))
                throw new ArgumentOutOfRangeException(nameof(colorblind), "非法色盲模式。");

            TextScalePercent = textScalePercent;
            Colorblind = colorblind;
            ReduceMotion = reduceMotion;
            var map = new Dictionary<string, bool>(StringComparer.Ordinal);
            if (hudVisibility != null) foreach (var kv in hudVisibility) map[kv.Key] = kv.Value;
            HudVisibility = map;
        }

        /// <summary>默认设置（100% / 无色盲 / 不减动 / 全可见）。</summary>
        public static AccessibilitySettings Default => new AccessibilitySettings(100, ColorblindMode.None, false);

        /// <summary>动效是否启用（减少动态 → false）。信息字段不依赖动效呈现。</summary>
        public bool AnimationEnabled => !ReduceMotion;

        /// <summary>某 HUD 元素是否可见（未记录视为可见）。</summary>
        public bool IsHudElementVisible(string element)
            => !HudVisibility.TryGetValue(element, out bool v) || v;

        /// <summary>确定性序列化为持久文本（HUD 可见性按键序数升序）。</summary>
        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.Append("textScale=").Append(TextScalePercent.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("colorblind=").Append((int)Colorblind).Append('\n');
            sb.Append("reduceMotion=").Append(ReduceMotion ? '1' : '0').Append('\n');
            var keys = new List<string>(HudVisibility.Keys);
            keys.Sort(StringComparer.Ordinal);
            sb.Append("hud=").Append(keys.Count).Append('\n');
            foreach (var k in keys)
                sb.Append("h\t").Append(k).Append('\t').Append(HudVisibility[k] ? '1' : '0').Append('\n');
            return sb.ToString();
        }

        /// <summary>解析持久文本；非法抛 <see cref="FormatException"/>（不产出部分对象）。</summary>
        public static AccessibilitySettings Parse(string text)
        {
            if (text == null) throw new FormatException("设置文本为 null。");
            var lines = text.Split('\n');
            int idx = 0;
            int textScale = ParseInt(Field("textScale", Next(lines, ref idx)));
            var colorblind = (ColorblindMode)ParseInt(Field("colorblind", Next(lines, ref idx)));
            bool reduceMotion = Field("reduceMotion", Next(lines, ref idx)) == "1";
            int hudCount = ParseInt(Field("hud", Next(lines, ref idx)));
            var hud = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < hudCount; i++)
            {
                var parts = Next(lines, ref idx).Split('\t');
                if (parts.Length != 3 || parts[0] != "h") throw new FormatException("HUD 可见性记录格式错。");
                hud[parts[1]] = parts[2] == "1";
            }
            return new AccessibilitySettings(textScale, colorblind, reduceMotion, hud);
        }

        private static string Next(string[] lines, ref int idx)
            => idx < lines.Length ? lines[idx++] : throw new FormatException("设置文本截断。");

        private static string Field(string name, string line)
        {
            int eq = line.IndexOf('=');
            if (eq < 0 || line.Substring(0, eq) != name) throw new FormatException($"字段「{name}」缺失或错位。");
            return line.Substring(eq + 1);
        }

        private static int ParseInt(string s)
            => int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int v)
                ? v : throw new FormatException($"非法整数：「{s}」。");
    }
}
