using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>叛离风险定性档（GDD_027 #1，反全知无数字）。</summary>
    public enum DefectionRisk
    {
        /// <summary>忠贞：不会叛。</summary>
        Steadfast = 0,
        /// <summary>安稳。</summary>
        Settled = 1,
        /// <summary>浮动：有隐忧。</summary>
        Wavering = 2,
        /// <summary>离心：随时可叛。</summary>
        Disloyal = 3,
    }

    /// <summary>
    /// 武将人生演化服务（GDD_005 / GDD_027 #1）：纯函数演化 <see cref="GeneralState"/>——忠诚增减、记忆施加（并联动忠诚）、
    /// 负伤/将养、被俘/获释、疲劳。确定性、无随机源。记忆是"活人"的核心：被背叛/救命会长期改写向背。
    /// </summary>
    public static class GeneralLifeService
    {
        /// <summary>调整忠诚（自动 clamp）。</summary>
        public static GeneralState AdjustLoyalty(GeneralState s, int delta) => s.WithLoyalty(s.Loyalty + delta);

        /// <summary>
        /// 施加一桩记忆并联动忠诚（GDD_005）：救命/重赏/拔擢/宽宥升忠，背叛/羞辱降忠（对施与者，此处作用于对主君之忠）。
        /// 权重放大情感烈度。返新态。
        /// </summary>
        public static GeneralState Remember(GeneralState s, MemoryKind kind, CharacterId counterpart, int year, int weight = 1)
        {
            var m = new MemoryEvent(kind, counterpart, year, weight);
            int w = m.Weight;
            int loyaltyDelta = kind switch
            {
                MemoryKind.Rescued => 15 * w,
                MemoryKind.Rewarded => 8 * w,
                MemoryKind.Promoted => 6 * w,
                MemoryKind.Spared => 10 * w,
                MemoryKind.Humiliated => -10 * w,
                MemoryKind.Betrayed => -20 * w,
                _ => 0,
            };
            return s.WithMemory(m).WithLoyalty(s.Loyalty + loyaltyDelta);
        }

        /// <summary>负伤一档（康健→负伤→重创）。</summary>
        public static GeneralState Wound(GeneralState s)
            => s.WithHealth(s.Health == GeneralHealth.Hale ? GeneralHealth.Wounded : GeneralHealth.Grave);

        /// <summary>将养：疲劳下降 + 健康回复一档。</summary>
        public static GeneralState Rest(GeneralState s, int fatigueRelief = 20)
        {
            GeneralHealth h = s.Health == GeneralHealth.Grave ? GeneralHealth.Wounded
                : s.Health == GeneralHealth.Wounded ? GeneralHealth.Hale : GeneralHealth.Hale;
            return s.WithFatigue(s.Fatigue - fatigueRelief).WithHealth(h);
        }

        /// <summary>征战积劳：疲劳上升。</summary>
        public static GeneralState Tire(GeneralState s, int amount = 15) => s.WithFatigue(s.Fatigue + amount);

        /// <summary>被某势力所俘（在押不可受命；忠诚不变，去留由后续宽宥/招降决定）。</summary>
        public static GeneralState Capture(GeneralState s, FactionId captor) => s.WithCaptiveOf(captor);

        /// <summary>获释归乡（清除在押；不改归属，归属由招降/流亡另定）。</summary>
        public static GeneralState Release(GeneralState s) => s.WithCaptiveOf(null);

        /// <summary>改换门庭（移籍到新主/新驻城；忠诚重置为中性起点 50，记忆保留）。</summary>
        public static GeneralState Reassign(GeneralState s, FactionId? faction, CityId? location, int startLoyalty = 50)
            => s.WithFaction(faction, location).WithLoyalty(startLoyalty).WithCaptiveOf(null);

        /// <summary>是否可受命出征（未被俘 + 非重创）。</summary>
        public static bool CanServe(GeneralState s) => s.CaptiveOf == null && s.Health != GeneralHealth.Grave;

        /// <summary>叛离风险定性（GDD_027 #1）：由忠诚 + 被背叛记忆派生（无数字呈玩家）。</summary>
        public static DefectionRisk RiskOf(GeneralState s)
        {
            int betrayed = s.MemoryWeight(MemoryKind.Betrayed);
            int effective = s.Loyalty - betrayed * 5;
            if (effective >= 75) return DefectionRisk.Steadfast;
            if (effective >= 50) return DefectionRisk.Settled;
            if (effective >= 30) return DefectionRisk.Wavering;
            return DefectionRisk.Disloyal;
        }
    }

    /// <summary>
    /// 一局武将人生台账（GDD_027 #1）：按需登记的 per-将运行时态集合（未登记者以档案派生初态惰性铸入）。
    /// 可变容器（唯一权威运行时态处），为持久化设计（<see cref="Entries"/> 供版本化存档 DTO 读写）。
    /// </summary>
    public sealed class GeneralLedger
    {
        private readonly Dictionary<string, GeneralState> _states = new Dictionary<string, GeneralState>(StringComparer.Ordinal);

        /// <summary>取某将运行时态（无＝null）。</summary>
        public GeneralState? Get(CharacterId id)
            => id.Value != null && _states.TryGetValue(id.Value, out GeneralState? s) ? s : null;

        /// <summary>取或惰性铸入初态（首次触及时以工厂铸初态并登记）。</summary>
        public GeneralState GetOrSeed(CharacterId id, Func<CharacterId, GeneralState> seed)
        {
            GeneralState? cur = Get(id);
            if (cur != null) return cur;
            GeneralState born = seed(id);
            _states[id.Value!] = born;
            return born;
        }

        /// <summary>写回某将态（演化后持久化到台账）。</summary>
        public void Set(GeneralState state)
        {
            if (state?.Id.Value != null) _states[state.Id.Value] = state;
        }

        /// <summary>已登记态数。</summary>
        public int Count => _states.Count;

        /// <summary>持久化入口（供存档 DTO；键=将 id）。</summary>
        public IReadOnlyDictionary<string, GeneralState> Entries => _states;
    }
}
