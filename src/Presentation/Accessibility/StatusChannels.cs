using System;

namespace ThreeKingdom.Presentation.Accessibility
{
    /// <summary>
    /// 关键状态的多通道编码（accessibility-requirements / hud.md §10.1：<b>信息不靠颜色单一通道</b>）。
    /// 每个状态携带颜色 + <b>冗余</b>的形状与文字 token——去色后仅凭形状/文字仍可唯一区分。
    /// 纯展示元数据，不可变。
    /// </summary>
    public sealed class StatusChannels
    {
        /// <summary>颜色 token（可被色盲/去色削弱）。</summary>
        public string ColorToken { get; }
        /// <summary>形状 token（冗余通道，不依赖颜色）。</summary>
        public string ShapeToken { get; }
        /// <summary>文字 token（冗余通道，不依赖颜色）。</summary>
        public string TextToken { get; }

        public StatusChannels(string colorToken, string shapeToken, string textToken)
        {
            ColorToken = Require(colorToken, nameof(colorToken));
            ShapeToken = Require(shapeToken, nameof(shapeToken));
            TextToken = Require(textToken, nameof(textToken));
        }

        /// <summary>去色后的可区分签名（形状 + 文字），用于断言「不靠颜色亦可区分」。</summary>
        public string NonColorSignature => ShapeToken + "|" + TextToken;

        private static string Require(string v, string n)
            => string.IsNullOrWhiteSpace(v) ? throw new ArgumentException($"{n} 不可为空。", n) : v;
    }
}
