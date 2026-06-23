using System;
using System.Collections.Generic;

namespace ThreeKingdom.Application.Session
{
    /// <summary>单个关键人物的只读投影（GDD_005 / ADR-0002）。能力为 0..100 量表读值；健康/职责为标签。不可变。</summary>
    public sealed class CharacterProjection
    {
        /// <summary>身份（姓名/称号）。</summary>
        public string Identity { get; }
        /// <summary>职责标识。</summary>
        public string Role { get; }
        /// <summary>统御。</summary>
        public int Command { get; }
        /// <summary>武勇。</summary>
        public int Valor { get; }
        /// <summary>谋略。</summary>
        public int Strategy { get; }
        /// <summary>治理。</summary>
        public int Governance { get; }
        /// <summary>交涉。</summary>
        public int Diplomacy { get; }
        /// <summary>健康等级（0 健康 / 1 轻伤 / 2 失能）。</summary>
        public int HealthLevel { get; }

        public CharacterProjection(
            string identity, string role, int command, int valor, int strategy,
            int governance, int diplomacy, int healthLevel)
        {
            Identity = identity;
            Role = role;
            Command = command;
            Valor = valor;
            Strategy = strategy;
            Governance = governance;
            Diplomacy = diplomacy;
            HealthLevel = healthLevel;
        }
    }

    /// <summary>人物花名册只读投影（GDD_005 §3 关键人物）。不可变集合。</summary>
    public sealed class RosterProjection
    {
        /// <summary>关键人物（场景给定顺序）。</summary>
        public IReadOnlyList<CharacterProjection> Characters { get; }

        public RosterProjection(IReadOnlyList<CharacterProjection> characters)
            => Characters = characters ?? throw new ArgumentNullException(nameof(characters));
    }
}
