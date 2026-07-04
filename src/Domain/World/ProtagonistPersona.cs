using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 主角人设/性情（GDD_015 事件分级：每次开新游戏<b>随机</b>赋予，给主角一个"性格底色"）。
    /// 决定同一天下事件下主角的<b>心里话口吻</b>——纯为丰富体验/代入感，非机械种子。
    /// </summary>
    public enum ProtagonistPersona
    {
        /// <summary>雄心：更向往自立门户、问鼎天下。</summary>
        Ambitious = 0,

        /// <summary>忠义：食君之禄忠君之事，鄙弃僭越背主。</summary>
        Loyalist = 1,

        /// <summary>务实：审时度势、利害为先，不为虚名所动。</summary>
        Pragmatist = 2,

        /// <summary>谨慎：明哲保身、静观其变，恶乱世凶险。</summary>
        Cautious = 3,
    }

    /// <summary>主角人设的确定性生成（开新游戏时按种子随机，ADR-0006；同种子同人设，可复现）。</summary>
    public static class ProtagonistPersonas
    {
        private static readonly ProtagonistPersona[] All =
        {
            ProtagonistPersona.Ambitious, ProtagonistPersona.Loyalist,
            ProtagonistPersona.Pragmatist, ProtagonistPersona.Cautious,
        };

        /// <summary>按种子确定性抽取一个人设（开局赋予主角）。</summary>
        public static ProtagonistPersona Roll(ulong seed)
            => All[new DeterministicRandom(seed).NextInt(0, All.Length)];
    }
}
