using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Environment
{
    /// <summary>
    /// epic-002 story-003：配置驱动天气/风向确定性解析。
    /// 治理 ADR：ADR-0004（注入确定性随机流、整数/定点、禁 float）+ ADR-0003（权重配置驱动）。GDD_002 / TR-weather-001/002。
    /// 覆盖 AC-1 配置权重+注入流转移、AC-2 同流位置+同前态→同结果、AC-3 具名修正不触发计策、AC-4 天气只减速≥1.0+夹取。
    /// </summary>
    [TestFixture]
    public class WeatherDeterministicTests
    {
        /// <summary>固定 NextUnit 的随机流桩，用于精确验证阈值选择逻辑（不依赖具体 PRNG 序列）。</summary>
        private sealed class FixedUnitRandom : IDeterministicRandom
        {
            private readonly FixedPoint _unit;
            public FixedUnitRandom(FixedPoint unit) { _unit = unit; }
            public ulong Position { get; private set; }
            public ulong NextBits() { Position++; return 0UL; }
            public FixedPoint NextUnit() { Position++; return _unit; }
            public int NextInt(int minInclusive, int maxExclusive) { Position++; return minInclusive; }
        }

        private static WeatherTransitionTable ClearWeightedTable()
        {
            // from=Clear：{晴:6, 阴:3, 雨:1}（GDD §Formula 1 示例）
            var weights = new Dictionary<WeatherType, IReadOnlyDictionary<WeatherType, int>>
            {
                [WeatherType.Clear] = new Dictionary<WeatherType, int>
                {
                    [WeatherType.Clear] = 6,
                    [WeatherType.Overcast] = 3,
                    [WeatherType.Rain] = 1,
                },
            };
            return new WeatherTransitionTable(weights);
        }

        // ---- AC-1 / AC-2：确定性加权转移 ----

        [Test]
        public void ResolveNext_matches_gdd_cumulative_example()
        {
            // r=0.72，total=10，阈值=7.2；累积 晴6→阴9>7.2 → 阴
            var resolver = new WeatherResolver(ClearWeightedTable());
            var result = resolver.ResolveNext(WeatherType.Clear, new FixedUnitRandom(FixedPoint.FromFraction(72, 100)));
            Assert.That(result, Is.EqualTo(WeatherType.Overcast));
        }

        [Test]
        public void ResolveNext_low_r_picks_first_weighted()
        {
            var resolver = new WeatherResolver(ClearWeightedTable());
            Assert.That(resolver.ResolveNext(WeatherType.Clear, new FixedUnitRandom(FixedPoint.Zero)),
                Is.EqualTo(WeatherType.Clear));
        }

        [Test]
        public void ResolveNext_high_r_picks_last_weighted()
        {
            // r=0.95 → 阈值 9.5；累积 6,9,10 → 仅 10>9.5 → 雨
            var resolver = new WeatherResolver(ClearWeightedTable());
            Assert.That(resolver.ResolveNext(WeatherType.Clear, new FixedUnitRandom(FixedPoint.FromFraction(95, 100))),
                Is.EqualTo(WeatherType.Rain));
        }

        [Test]
        public void ResolveNext_single_weight_is_forced()
        {
            var weights = new Dictionary<WeatherType, IReadOnlyDictionary<WeatherType, int>>
            {
                [WeatherType.Rain] = new Dictionary<WeatherType, int> { [WeatherType.Overcast] = 5 },
            };
            var resolver = new WeatherResolver(new WeatherTransitionTable(weights));
            // 任意 r 都只能选唯一有权重态
            Assert.That(resolver.ResolveNext(WeatherType.Rain, new FixedUnitRandom(FixedPoint.Zero)), Is.EqualTo(WeatherType.Overcast));
            Assert.That(resolver.ResolveNext(WeatherType.Rain, new FixedUnitRandom(FixedPoint.FromFraction(99, 100))), Is.EqualTo(WeatherType.Overcast));
        }

        [Test]
        public void ResolveNext_same_seed_and_state_yields_same_sequence()
        {
            var resolver = new WeatherResolver(ClearWeightedTable());

            WeatherType[] Run()
            {
                var rng = new DeterministicRandom(seed: 0xC0FFEE);
                var seq = new WeatherType[5];
                for (int i = 0; i < seq.Length; i++) seq[i] = resolver.ResolveNext(WeatherType.Clear, rng);
                return seq;
            }

            Assert.That(Run(), Is.EqualTo(Run()));
        }

        [Test]
        public void ResolveNext_stream_position_resume_continues_identically()
        {
            var resolver = new WeatherResolver(ClearWeightedTable());

            var full = new DeterministicRandom(seed: 42);
            var reference = new List<WeatherType>();
            for (int i = 0; i < 5; i++) reference.Add(resolver.ResolveNext(WeatherType.Clear, full));

            // 抽 3 个后保存 position，新流从该 position 续抽 2 个
            var partial = new DeterministicRandom(seed: 42);
            for (int i = 0; i < 3; i++) resolver.ResolveNext(WeatherType.Clear, partial);
            var resumed = new DeterministicRandom(seed: 42, position: partial.Position);
            var tail = new[] { resolver.ResolveNext(WeatherType.Clear, resumed), resolver.ResolveNext(WeatherType.Clear, resumed) };

            Assert.That(tail, Is.EqualTo(reference.Skip(3).ToArray()));
        }

        [Test]
        public void Table_rejects_zero_sum_and_negative_weights()
        {
            var zeroSum = new Dictionary<WeatherType, IReadOnlyDictionary<WeatherType, int>>
            {
                [WeatherType.Clear] = new Dictionary<WeatherType, int> { [WeatherType.Clear] = 0 },
            };
            Assert.Throws<ArgumentException>(() => new WeatherTransitionTable(zeroSum));

            var negative = new Dictionary<WeatherType, IReadOnlyDictionary<WeatherType, int>>
            {
                [WeatherType.Clear] = new Dictionary<WeatherType, int> { [WeatherType.Rain] = -1 },
            };
            Assert.Throws<ArgumentException>(() => new WeatherTransitionTable(negative));
        }

        [Test]
        public void ResolveNext_missing_from_row_is_rejected()
        {
            var resolver = new WeatherResolver(ClearWeightedTable());
            Assert.Throws<ArgumentException>(() => resolver.ResolveNext(WeatherType.Fog, new FixedUnitRandom(FixedPoint.Zero)));
        }

        // ---- AC-3：具名修正（不触发计策）----

        [Test]
        public void ModifierSet_exposes_named_modifiers_per_consumer()
        {
            var set = new EnvironmentModifierSet(new Dictionary<EnvironmentConsumer, EnvironmentModifier>
            {
                [EnvironmentConsumer.Movement] = new EnvironmentModifier(FixedPoint.FromFraction(3, 2), FixedPoint.Zero), // 雨：移动变慢
                [EnvironmentConsumer.Stealth] = new EnvironmentModifier(FixedPoint.FromFraction(1, 2), FixedPoint.Zero),  // 雨：隐蔽更好
            });

            Assert.That(set.Apply(EnvironmentConsumer.Movement, FixedPoint.FromInt(10)), Is.EqualTo(FixedPoint.FromInt(15)));
            Assert.That(set.Apply(EnvironmentConsumer.Stealth, FixedPoint.FromInt(10)), Is.EqualTo(FixedPoint.FromInt(5)));
            // 未声明消费者 → 中性
            Assert.That(set.Apply(EnvironmentConsumer.Supply, FixedPoint.FromInt(10)), Is.EqualTo(FixedPoint.FromInt(10)));
        }

        [Test]
        public void Consumers_are_only_the_five_named_no_stratagem()
        {
            // 结构性保证：环境不直接产出「计策成功」——消费者仅五类
            Assert.That(Enum.GetValues(typeof(EnvironmentConsumer)).Length, Is.EqualTo(5));
        }

        // ---- AC-4：天气只减速 ≥1.0 + 极端夹取 ----

        [Test]
        public void Movement_modifier_below_one_is_rejected()
        {
            Assert.Throws<ArgumentException>(() => new EnvironmentModifierSet(new Dictionary<EnvironmentConsumer, EnvironmentModifier>
            {
                [EnvironmentConsumer.Movement] = new EnvironmentModifier(FixedPoint.FromFraction(8, 10), FixedPoint.Zero), // 0.8 < 1.0
            }));
        }

        [Test]
        public void ClampSlowdown_clamps_into_one_to_cap()
        {
            var cap = FixedPoint.FromInt(2);
            Assert.That(EnvironmentModifierSet.ClampSlowdown(FixedPoint.FromInt(3), cap), Is.EqualTo(cap));                       // 超上限夹取
            Assert.That(EnvironmentModifierSet.ClampSlowdown(FixedPoint.FromFraction(8, 10), cap), Is.EqualTo(FixedPoint.One));  // 低于 1 提到 1
            Assert.That(EnvironmentModifierSet.ClampSlowdown(FixedPoint.FromFraction(3, 2), cap), Is.EqualTo(FixedPoint.FromFraction(3, 2))); // 区间内不变
            Assert.Throws<ArgumentOutOfRangeException>(() => EnvironmentModifierSet.ClampSlowdown(FixedPoint.FromInt(3), FixedPoint.FromFraction(1, 2)));
        }

        // ---- 风门控（GDD §Formula 4 零风）----

        [Test]
        public void Wind_zero_strength_has_no_effect()
        {
            Assert.That(new Wind(WindDirection.East, 0).HasEffect, Is.False);
            Assert.That(new Wind(WindDirection.East, 3).HasEffect, Is.True);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Wind(WindDirection.North, -1));
        }
    }
}
