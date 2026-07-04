using System.Collections.Generic;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 主角人设展示视图（GDD_015 事件分级：开局随机人设，给心里话着色）。
    /// 把 <see cref="ProtagonistPersona"/> 映射为中文名 + 一句性情描述。不可变、纯映射。
    /// </summary>
    public sealed class PersonaView
    {
        private static readonly IReadOnlyDictionary<ProtagonistPersona, (string Name, string Desc)> Labels =
            new Dictionary<ProtagonistPersona, (string, string)>
            {
                [ProtagonistPersona.Ambitious] = ("雄心", "胸怀大志，向往自立门户、问鼎天下。"),
                [ProtagonistPersona.Loyalist] = ("忠义", "食君之禄忠君之事，鄙弃僭越背主。"),
                [ProtagonistPersona.Pragmatist] = ("务实", "审时度势、利害为先，不为虚名所动。"),
                [ProtagonistPersona.Cautious] = ("谨慎", "明哲保身、静观其变，恶乱世凶险。"),
            };

        /// <summary>人设枚举。</summary>
        public ProtagonistPersona Persona { get; }
        /// <summary>中文名（雄心/忠义/务实/谨慎）。</summary>
        public string Name { get; }
        /// <summary>一句性情描述。</summary>
        public string Description { get; }

        public PersonaView(ProtagonistPersona persona)
        {
            Persona = persona;
            (string name, string desc) = Labels.TryGetValue(persona, out var l) ? l : ("未知", string.Empty);
            Name = name;
            Description = desc;
        }
    }
}
