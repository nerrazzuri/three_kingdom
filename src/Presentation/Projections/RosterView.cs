using System;
using System.Collections.Generic;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 人物花名册展示视图（GDD_005 §3 / ADR-0002）。把 <see cref="RosterProjection"/> 翻译为中文人物行
    /// （身份 · 职责 · 健康 + 能力五域）。能力为过程质量量表读值，<b>非</b>解锁门槛（P11 无技能解锁）。
    /// 不可变、纯映射。逻辑由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    public sealed class RosterView
    {
        private static readonly string[] HealthLabels = { "健康", "轻伤", "失能" };

        /// <summary>每个关键人物一段中文摘要（多行：标题行 + 能力行）。</summary>
        public IReadOnlyList<CharacterLine> Characters { get; }

        public RosterView(RosterProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            var list = new List<CharacterLine>();
            foreach (var c in projection.Characters)
            {
                string health = (c.HealthLevel >= 0 && c.HealthLevel < HealthLabels.Length)
                    ? HealthLabels[c.HealthLevel] : "未知";
                string title = c.Identity + "（" + c.Role + " · " + health + "）";
                string caps = "统 " + N(c.Command) + " 武 " + N(c.Valor) + " 谋 " + N(c.Strategy)
                    + " 治 " + N(c.Governance) + " 交 " + N(c.Diplomacy);
                list.Add(new CharacterLine(title, caps));
            }
            Characters = list;
        }

        private static string N(int v) => v.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>单个人物的两行展示（标题 + 能力）。不可变。</summary>
    public sealed class CharacterLine
    {
        /// <summary>标题行（身份 · 职责 · 健康）。</summary>
        public string Title { get; }
        /// <summary>能力行（统/武/谋/治/交）。</summary>
        public string Capabilities { get; }

        public CharacterLine(string title, string capabilities)
        {
            Title = title;
            Capabilities = capabilities;
        }
    }
}
