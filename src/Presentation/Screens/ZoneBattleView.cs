using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>区域战斗展示标签（表现层，不影响权威）。</summary>
    public static class ZoneBattleText
    {
        /// <summary>区域中文名。</summary>
        public static string Zone(string zoneId) => zoneId switch
        {
            "zone-front" => "正面关城",
            "zone-flank" => "侧翼隘口",
            "zone-supply" => "敌粮道",
            "zone-cover" => "遮蔽高地",
            "zone-reserve" => "预备后方",
            _ => zoneId,
        };

        /// <summary>终局中文说明。</summary>
        public static string Outcome(ZoneBattleOutcome o, bool playerIsAttacker) => o switch
        {
            ZoneBattleOutcome.Ongoing => "鏖战中",
            ZoneBattleOutcome.AttackerVictory => playerIsAttacker ? "破城取胜！" : "城破——守御失利。",
            ZoneBattleOutcome.DefenderVictory => playerIsAttacker ? "攻城未克，退兵（可再图）。" : "守土成功，退敌！",
            _ => o.ToString(),
        };

        /// <summary>姿态中文名。</summary>
        public static string Posture(Posture p) => p switch
        {
            ThreeKingdom.Domain.ZoneBattle.Posture.Assault => "主攻",
            ThreeKingdom.Domain.ZoneBattle.Posture.Feint => "佯攻",
            ThreeKingdom.Domain.ZoneBattle.Posture.Hold => "守",
            _ => p.ToString(),
        };
    }

    /// <summary>一个区域的态势行（我方/敌方投影/已成条件/我方支队）。不可变。</summary>
    public sealed class ZoneLineView
    {
        public string ZoneId { get; }
        public string ZoneLabel { get; }
        public int OwnStrength { get; }
        public int EnemyStrength { get; }
        public IReadOnlyList<string> FormedConditions { get; }
        /// <summary>我方在该区的支队（将领·id + 一句摘要，供选择调动）。</summary>
        public IReadOnlyList<string> OwnDetachments { get; }
        /// <summary>该区敌方将领（GDD_027 #3/#4 守将进战斗，反全知：已侦察目标现真名，否则「未探明之将」）。</summary>
        public IReadOnlyList<string> EnemyCommanders { get; }
        public bool IsObjective { get; }

        internal ZoneLineView(
            string zoneId, int ownStrength, int enemyStrength,
            IReadOnlyList<string> formed, IReadOnlyList<string> ownDetachments,
            IReadOnlyList<string> enemyCommanders, bool isObjective)
        {
            ZoneId = zoneId;
            ZoneLabel = ZoneBattleText.Zone(zoneId);
            OwnStrength = ownStrength;
            EnemyStrength = enemyStrength;
            FormedConditions = formed;
            OwnDetachments = ownDetachments;
            EnemyCommanders = enemyCommanders;
            IsObjective = isObjective;
        }
    }

    /// <summary>一个合法调动选项（哪支队 → 哪相邻区），供 UI 渲染「排兵布阵」按钮。不可变。</summary>
    public sealed class ZoneMoveOption
    {
        public string DetachmentId { get; }
        public string DetachmentLabel { get; }
        public string TargetZoneId { get; }
        public string TargetZoneLabel { get; }

        internal ZoneMoveOption(string detachmentId, string detachmentLabel, string targetZoneId)
        {
            DetachmentId = detachmentId;
            DetachmentLabel = detachmentLabel;
            TargetZoneId = targetZoneId;
            TargetZoneLabel = ZoneBattleText.Zone(targetZoneId);
        }
    }

    /// <summary>
    /// 区域战斗展示视图（GDD_021 §12 战中界面）：回合 + 各区态势（我/敌/条件）+ 涌现 + 终局 + 合法调动选项。不可变、纯函数。
    /// <b>兵法条件是涌现进度，非按钮</b>。玩家看得到各区可做什么（AC-5 精神）。
    /// </summary>
    public sealed class ZoneBattleView
    {
        public int Round { get; }
        public int MaxRounds { get; }
        public bool PlayerIsAttacker { get; }
        public IReadOnlyList<ZoneLineView> Zones { get; }
        public IReadOnlyList<string> Emergences { get; }
        /// <summary>当前合法调动（己方在场未在途支队 × 相邻区）——「排兵布阵」交互源。</summary>
        public IReadOnlyList<ZoneMoveOption> MoveOptions { get; }
        public ZoneBattleOutcome Outcome { get; }
        public string OutcomeLabel { get; }
        public bool IsOver => Outcome != ZoneBattleOutcome.Ongoing;

        private ZoneBattleView(
            int round, int maxRounds, bool playerIsAttacker, IReadOnlyList<ZoneLineView> zones,
            IReadOnlyList<string> emergences, IReadOnlyList<ZoneMoveOption> moveOptions, ZoneBattleOutcome outcome)
        {
            Round = round;
            MaxRounds = maxRounds;
            PlayerIsAttacker = playerIsAttacker;
            Zones = zones;
            Emergences = emergences;
            MoveOptions = moveOptions;
            Outcome = outcome;
            OutcomeLabel = ZoneBattleText.Outcome(outcome, playerIsAttacker);
        }

        /// <summary>由战斗态构造（玩家视角：己方=PlayerSide）。<paramref name="defendersRevealed"/>=已侦察目标 → 敌将现真名，否则「未探明之将」（反全知）。</summary>
        public static ZoneBattleView FromState(
            ZoneBattleState state, ZoneBattleOutcome outcome, IReadOnlyList<string>? lastEmergences, bool defendersRevealed = false)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            BattleSide own = state.PlayerSide;

            var lines = new List<ZoneLineView>();
            foreach (Zone z in state.Field.Zones)
            {
                int ownStrength = 0, enemyStrength = 0;
                var ownDets = new List<string>();
                var enemyCmd = new List<string>();
                foreach (Detachment d in state.DetachmentsIn(z.Id))
                {
                    if (d.Side == own)
                    {
                        ownStrength += d.Strength;
                        string transit = d.InTransit ? $"→在途" : "";
                        string leader = d.General != null ? DisplayNames.Of(d.General.Character.Value) + "·" : "";
                        ownDets.Add($"{leader}{d.Id.Value}（{d.Strength}·{ZoneBattleText.Posture(d.Posture)}{transit}）");
                    }
                    else
                    {
                        enemyStrength += d.Strength;
                        // 守将进战斗（B/C）：敌方支队将领投影，反全知门——已侦察现真名，否则未探明。
                        if (d.General != null)
                            enemyCmd.Add(defendersRevealed ? DisplayNames.Of(d.General.Character.Value) : "未探明之将");
                    }
                }
                var formed = new List<string>();
                foreach (TacticCondition c in state.EngagementOf(z.Id).FormedConditions) formed.Add(OffensiveText.Condition(c));
                lines.Add(new ZoneLineView(z.Id.Value, ownStrength, enemyStrength, formed, ownDets, enemyCmd, z.Id == BattleField.Front));
            }

            var emerg = new List<string>();
            if (lastEmergences != null)
                foreach (string e in lastEmergences)
                {
                    int i = e.IndexOf(':');
                    if (i > 0 && Enum.TryParse(e.Substring(i + 1), out TacticCondition c))
                        emerg.Add($"{ZoneBattleText.Zone(e.Substring(0, i))}：{OffensiveText.Condition(c)} 成型！");
                    else emerg.Add(e);
                }

            // 合法调动选项（己方在场未在途支队 × 相邻区）——排兵布阵交互源。终局后为空。
            var moves = new List<ZoneMoveOption>();
            if (outcome == ZoneBattleOutcome.Ongoing)
                foreach (Detachment d in state.DetachmentsOf(own))
                {
                    if (d.InTransit || d.IsBroken) continue;
                    string label = $"{d.Id.Value}（{d.Strength}）";
                    foreach (ZoneId n in state.Field.Neighbors(d.Location))
                        moves.Add(new ZoneMoveOption(d.Id.Value, label, n.Value));
                }

            return new ZoneBattleView(state.Clock.Round, state.Clock.MaxRounds, own == BattleSide.Attacker, lines, emerg, moves, outcome);
        }
    }
}
