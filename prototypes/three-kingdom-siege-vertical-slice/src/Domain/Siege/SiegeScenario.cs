// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 「汜水小城」开局——2倍兵力劣势，正面必败，须创造条件取胜
// Date: 2026-06-21

using TkSlice.Domain.Characters;
using TkSlice.Domain.Config;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Siege
{
    /// <summary>开局场景工厂。数值来自配置/常量，便于复现与调参。</summary>
    public static class SiegeScenario
    {
        public const ulong WorldSeed = 20260621UL;

        // 开局常量（slice 场景设定；量产中来自存档/关卡配置）
        public const int DefenderTroops = 600;
        public const int AttackerTroops = 1200;   // 2 倍兵力优势

        public static SiegeState CreateXishuiSiege(SiegeConfig? cfg = null)
        {
            cfg ??= SiegeConfig.Default();

            var defender = new ForceState(
                "汜水守军", Side.Defender, DefenderTroops,
                morale: Fixed.FromFraction(60, 100),
                fatigue: Fixed.FromFraction(20, 100),
                discipline: Fixed.FromFraction(65, 100),
                supply: Fixed.OneValue);

            var attacker = new ForceState(
                "敌军先锋", Side.Attacker, AttackerTroops,
                morale: Fixed.FromFraction(70, 100),
                fatigue: Fixed.FromFraction(25, 100),
                discipline: Fixed.FromFraction(60, 100),
                supply: Fixed.OneValue);

            // 敌将鲁莽（易中诱敌之计）；统御中等
            var enemyCommander = new Commander(
                "enemy-vanguard", "敌先锋·华雄型猛将",
                recklessness: Fixed.FromFraction(65, 100),
                command: Fixed.FromFraction(55, 100));

            return new SiegeState(
                cfg, WorldSeed, defender, attacker, enemyCommander,
                fortification: Fixed.FromFraction(70, 100),
                cityFood: 12,
                standing: Fixed.FromFraction(55, 100));   // 中等声望
        }
    }
}
