using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>武将健康态（GDD_005 / GDD_027 #1）：影响可否受命出征与战力。</summary>
    public enum GeneralHealth
    {
        /// <summary>康健。</summary>
        Hale = 0,
        /// <summary>负伤：战力/行动受损，须将养。</summary>
        Wounded = 1,
        /// <summary>重创：不可出征，长养方复。</summary>
        Grave = 2,
    }

    /// <summary>武将记忆事件类别（GDD_005 人物记忆）：塑造其对玩家/他人的向背。</summary>
    public enum MemoryKind
    {
        /// <summary>被背叛（对背叛者忠诚骤降·生仇）。</summary>
        Betrayed = 0,
        /// <summary>受重赏（忠诚升）。</summary>
        Rewarded = 1,
        /// <summary>被救命（大恩·忠诚大升）。</summary>
        Rescued = 2,
        /// <summary>受羞辱（忠诚降·记恨）。</summary>
        Humiliated = 3,
        /// <summary>被宽宥（降者获释·感念）。</summary>
        Spared = 4,
        /// <summary>获拔擢（委以要职·忠诚升）。</summary>
        Promoted = 5,
    }

    /// <summary>一桩武将记忆（GDD_005）：谁在何年对其做了什么，权重决定情感烈度。不可变。</summary>
    public readonly struct MemoryEvent
    {
        public MemoryKind Kind { get; }
        public CharacterId Counterpart { get; }   // 施与者（玩家主君/他将），可空 id
        public int Year { get; }
        public int Weight { get; }

        public MemoryEvent(MemoryKind kind, CharacterId counterpart, int year, int weight)
        {
            Kind = kind; Counterpart = counterpart; Year = year; Weight = weight < 1 ? 1 : weight;
        }
    }

    /// <summary>
    /// 武将<b>运行时人生态</b>（GDD_005 / GDD_027 #1）：区别于静态档案 <see cref="GeneralDossier"/>（"本质是什么样"），
    /// 此记录"这一局里经历了什么"——当前主君/所在/忠诚/健康/疲劳/被俘/记忆。不可变（With* 返新），确定性，无随机源。
    /// 由 <c>GeneralLifeService</c> 演化，为持久化设计（字段皆值/枚举/id，便于版本化存档 DTO）。
    /// </summary>
    public sealed class GeneralState
    {
        public CharacterId Id { get; }
        public FactionId? Faction { get; }
        public CityId? Location { get; }
        /// <summary>忠诚 0..100（对当前主君）。低于阈值有叛离风险。</summary>
        public int Loyalty { get; }
        public GeneralHealth Health { get; }
        /// <summary>疲劳 0..100（高则战力/行动受损，须歇整）。</summary>
        public int Fatigue { get; }
        /// <summary>被俘于某势力（非 null＝在押，不可受命）。</summary>
        public FactionId? CaptiveOf { get; }

        private readonly MemoryEvent[] _memories;
        public IReadOnlyList<MemoryEvent> Memories => _memories;

        public GeneralState(
            CharacterId id, FactionId? faction, CityId? location, int loyalty,
            GeneralHealth health, int fatigue, FactionId? captiveOf, IReadOnlyList<MemoryEvent>? memories)
        {
            Id = id;
            Faction = faction;
            Location = location;
            Loyalty = Clamp(loyalty, 0, 100);
            Health = health;
            Fatigue = Clamp(fatigue, 0, 100);
            CaptiveOf = captiveOf;
            _memories = memories != null ? new List<MemoryEvent>(memories).ToArray() : Array.Empty<MemoryEvent>();
        }

        /// <summary>初铸：某主君麾下、驻某城、初始忠诚（由气质派生），康健无俘无忆。</summary>
        public static GeneralState Fresh(CharacterId id, FactionId? faction, CityId? location, int loyalty)
            => new GeneralState(id, faction, location, loyalty, GeneralHealth.Hale, 0, null, null);

        public GeneralState WithLoyalty(int loyalty)
            => new GeneralState(Id, Faction, Location, loyalty, Health, Fatigue, CaptiveOf, _memories);

        public GeneralState WithHealth(GeneralHealth health)
            => new GeneralState(Id, Faction, Location, Loyalty, health, Fatigue, CaptiveOf, _memories);

        public GeneralState WithFatigue(int fatigue)
            => new GeneralState(Id, Faction, Location, Loyalty, Health, fatigue, CaptiveOf, _memories);

        public GeneralState WithFaction(FactionId? faction, CityId? location)
            => new GeneralState(Id, faction, location, Loyalty, Health, Fatigue, CaptiveOf, _memories);

        public GeneralState WithCaptiveOf(FactionId? captor)
            => new GeneralState(Id, Faction, Location, Loyalty, Health, Fatigue, captor, _memories);

        /// <summary>追记一桩记忆（返新态，记忆按时序追加）。</summary>
        public GeneralState WithMemory(MemoryEvent m)
        {
            var list = new List<MemoryEvent>(_memories) { m };
            return new GeneralState(Id, Faction, Location, Loyalty, Health, Fatigue, CaptiveOf, list);
        }

        /// <summary>某类记忆累计权重（供关系/向背判定）。</summary>
        public int MemoryWeight(MemoryKind kind)
        {
            int w = 0;
            foreach (MemoryEvent m in _memories) if (m.Kind == kind) w += m.Weight;
            return w;
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
