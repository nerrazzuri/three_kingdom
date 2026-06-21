// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 守城聚合根——三条条件链经多时段承诺形成，战果可解释（GDD_010/011/012）
// Date: 2026-06-21

using System.Collections.Generic;
using TkSlice.Domain.Battle;
using TkSlice.Domain.Characters;
using TkSlice.Domain.Config;
using TkSlice.Domain.Diplomacy;
using TkSlice.Domain.Environment;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Intel;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Time;

namespace TkSlice.Domain.Siege
{
    /// <summary>
    /// 「汜水小城守御」战役聚合根。持有权威状态，仅经 Domain 方法变更。
    /// 因果日志记录「来源→修正→结果」，供解释链与复盘。
    /// 支持三条兵法条件链：断粮疲敌 / 假退伏击 / 守城待变（外交）。
    /// </summary>
    public sealed class SiegeState
    {
        public WorldDay Clock { get; private set; }
        public ForceState Defender { get; }     // 玩家守军
        public ForceState Attacker { get; }     // 敌先锋
        public Commander EnemyCommander { get; }
        public Fixed Fortification { get; private set; }
        public int CityFood { get; private set; }
        public bool EnemyRaidCutActive { get; private set; }
        public int RaidUnits { get; private set; }

        public Fixed Standing { get; }                  // 玩家外部声望（外交用）
        public Fixed DiplPressure { get; private set; } // 守城累积的外交/民心压力
        public WeatherKind CurrentWeather { get; private set; }
        public DiplomaticPledge? PendingPledge { get; private set; }
        public bool AmbushChainSpent { get; private set; }  // 假退伏击为一次性诱敌
        public bool EnemyReinforced { get; private set; }   // 敌军自身援军是否已抵达
        public EnemyIntel Intel { get; private set; }       // 玩家对敌的知识投影（非真值）

        private readonly SiegeConfig _cfg;
        private readonly DetRng _raidRng;
        private readonly DetRng _diploRng;
        private readonly DetRng _scoutRng;
        private readonly ulong _worldSeed;
        private readonly List<string> _log = new();

        public IReadOnlyList<string> CausalLog => _log;
        public SiegeConfig Config => _cfg;

        public SiegeState(SiegeConfig cfg, ulong worldSeed,
            ForceState defender, ForceState attacker, Commander enemyCommander,
            Fixed fortification, int cityFood, Fixed standing)
        {
            _cfg = cfg; _worldSeed = worldSeed;
            Defender = defender; Attacker = attacker; EnemyCommander = enemyCommander;
            Fortification = fortification; CityFood = cityFood; Standing = standing;
            _raidRng = DetRng.Fork(worldSeed, "raid-contest");
            _diploRng = DetRng.Fork(worldSeed, "diplomacy-betrayal");
            _scoutRng = DetRng.Fork(worldSeed, "scout");
            Clock = WorldDay.Start;
            DiplPressure = Fixed.Zero;
            CurrentWeather = Weather.At(worldSeed, Clock);
            // 开局敌情模糊：粗略知道兵力规模，士气/补给低置信（须侦察）
            Intel = new EnemyIntel(attacker.Troops, attacker.UnitMorale, attacker.SupplyState,
                cfg.IntelInitialConfidence);
        }

        /// <summary>侦察敌军（GDD_007）：刷新知识投影；不消耗 Domain 时间，由调用方决定时间成本。</summary>
        public void ScoutEnemy()
        {
            bool reinforceSoon = !EnemyReinforced
                && _cfg.EnemyReinforceSegment - Clock.TotalSegments <= 4;
            Intel.Scout(Attacker, Clock.TotalSegments, reinforceSoon, _cfg, _scoutRng);
            Log($"侦察敌营：{Intel.Describe(Clock.TotalSegments)}");
        }

        private void Log(string s) => _log.Add($"[{Clock}] {s}");

        // ───────────────────────── 链 1：断粮疲敌 ─────────────────────────

        public void CommitRaidOnSupplyLine(int raidUnits)
        {
            EnemyRaidCutActive = true;
            RaidUnits = raidUnits;
            Fixed weaken = _cfg.RaidGarrisonWeaken * Fixed.FromInt(raidUnits);
            Fortification = Fixed.Clamp(Fortification - weaken, Fixed.Zero, Fixed.OneValue);
            Log($"投入 {raidUnits} 支袭扰队断敌粮道；守城工事削弱至 {Fortification}（代价）。");
        }

        public void StopRaid()
        {
            EnemyRaidCutActive = false;
            Log("撤回袭扰队，敌补给线恢复。");
        }

        // ───────────────────────── 链 2：假退伏击 ─────────────────────────

        /// <summary>佯退诱敌设伏（GDD_005 性格 + GDD_010 伏击 + GDD_011 军纪）。一次性诱敌。</summary>
        public FeignedRetreatResult CommitFeignedRetreat(int decoyTroops, int ambushTroops)
        {
            if (AmbushChainSpent)
                return new FeignedRetreatResult { Outcome = FeignOutcome.NotPursued, Note = "敌将已警觉，诱敌之计不可再用。" };
            AmbushChainSpent = true;

            // 伏兵新设、未暴露 → 低疑虑
            Fixed suspicion = Fixed.FromFraction(10, 100);
            var r = AmbushResolver.Resolve(
                Defender.Discipline, decoyTroops, ambushTroops,
                Defender.UnitMorale, Defender.Discipline,
                Attacker, EnemyCommander.Recklessness, suspicion, _cfg);

            switch (r.Outcome)
            {
                case FeignOutcome.RoutFailure:
                    Defender.TakeCasualties(r.DecoyLosses);
                    Log(r.Note);
                    break;
                case FeignOutcome.NotPursued:
                    Log(r.Note);
                    break;
                case FeignOutcome.AmbushSprung:
                    var b = r.Ambush!;
                    // 伏击战果作用于真实敌军：支队伤亡从敌总兵力扣除，并打击全军士气
                    Attacker.TakeCasualties(b.DefenderCasualties);
                    Defender.TakeCasualties(b.AttackerCasualties);
                    Attacker.HitMorale(_cfg.StarveMoralePenalty);  // 遭伏，全军夺气
                    Log(r.Note + $" 敌损 {b.DefenderCasualties}（余 {Attacker.Troops}），伏兵损 {b.AttackerCasualties}。");
                    break;
            }
            return r;
        }

        // ───────────────────────── 链 3：守城待变（外交） ─────────────────────────

        /// <summary>遣使发起一次外交受控请求（GDD_012 §8）。延迟交付、可背约。</summary>
        public DiplomaticPledge RequestDiplomacy(PledgeType type, Fixed pledgeCost)
        {
            var pledge = DiplomacyEvaluator.Create(_cfg, type, Standing, pledgeCost, DiplPressure, Clock);
            if (pledge.Response != PledgeResponse.Reject)
                PendingPledge = pledge;
            string respText = pledge.Response switch
            {
                PledgeResponse.Accept => $"应允，约 {pledge.ArrivalT} 交付（迟到/背约风险 {pledge.BetrayRisk}）",
                PledgeResponse.Conditional => $"附条件应允（grant {pledge.GrantScore}），约 {pledge.ArrivalT} 交付",
                _ => $"婉拒（grant {pledge.GrantScore} < 阈值 {_cfg.DiplomacyAcceptThreshold}）",
            };
            Log($"遣使{DiplomaticPledge.TypeName(type)}：{respText}。");
            return pledge;
        }

        // ───────────────────────── 时段推进 ─────────────────────────

        public void AdvanceSegment()
        {
            Clock = Clock.Advance(1);
            CurrentWeather = Weather.At(_worldSeed, Clock);

            // 断粮：双边博弈（链 1）。袭扰强度 vs 敌护卫；切断失败则敌补给车队回补。
            if (EnemyRaidCutActive)
            {
                Fixed raidStrength = _cfg.RaidStrengthPerUnit * Fixed.FromInt(RaidUnits);
                Fixed advantage = raidStrength - _cfg.EnemyEscortStrength;
                Fixed cutProb = Fixed.Clamp(
                    Fixed.FromFraction(50, 100) + _cfg.CutContestSlope * advantage,
                    Fixed.FromFraction(10, 100), Fixed.FromFraction(90, 100));
                Fixed roll = _raidRng.NextFixedUnit();
                if (roll < cutProb)
                {
                    Attacker.ApplySupplyCut(_cfg);  // 这段袭扰得手（真值；玩家未必知道）
                }
                else
                {
                    // 护卫挡住袭扰，敌补给车队推进回补（短缺计数清零，敌补给回升）
                    Attacker.ApplyResupplyPush(_cfg);
                }
            }
            else
            {
                Attacker.ApplySupplyRestore(_cfg);
            }

            // 断粮后果（GDD_011 唯一施加点）——只有净切断占优、短缺累积过宽限才生效
            Attacker.ApplyStarvationConsequence(_cfg);

            // 敌军自身援军抵达（两边都有援军）：拖过此刻敌军回血
            if (!EnemyReinforced && Clock.TotalSegments >= _cfg.EnemyReinforceSegment)
            {
                EnemyReinforced = true;
                Attacker.Reinforce(_cfg.EnemyReinforceTroops);
                Attacker.HitMorale(-_cfg.EnemyReinforceMorale);  // 负惩罚 = 士气回升
                Log($"⚠ 敌军援军抵达（+{_cfg.EnemyReinforceTroops} 兵，士气回升）——消耗赛时间窗已过！");
            }

            // 守城压力累积（链 3 代价）
            DiplPressure = Fixed.Clamp(DiplPressure + _cfg.SiegePressurePerSegment, Fixed.Zero, Fixed.OneValue);

            // 情报随时间衰减（不侦察则越来越不可信，GDD_007）
            Intel.DecayConfidence(_cfg);

            // 外交交付结算（链 3）
            ProcessPledgeArrival();
        }

        private void ProcessPledgeArrival()
        {
            if (PendingPledge == null || Clock < PendingPledge.ArrivalT) return;
            var p = PendingPledge;
            Fixed r = _diploRng.NextFixedUnit();
            bool fulfilled = r >= p.BetrayRisk;
            if (fulfilled)
            {
                p.MarkFulfilled();
                ApplyPledgeEffect(p);
                Log($"外援交付：{DiplomaticPledge.TypeName(p.Type)} 如约抵达。");
            }
            else
            {
                p.MarkBetrayed();
                // 背约后果：声誉/外交压力上升（GDD_006 引用），且无交付
                DiplPressure = Fixed.Clamp(DiplPressure + Fixed.FromFraction(10, 100), Fixed.Zero, Fixed.OneValue);
                Log($"外势力背约（roll {r} < 风险 {p.BetrayRisk}）：{DiplomaticPledge.TypeName(p.Type)} 未至，外交压力上升。代价已付，徒劳。");
            }
            PendingPledge = null;
        }

        private void ApplyPledgeEffect(DiplomaticPledge p)
        {
            switch (p.Type)
            {
                case PledgeType.Relief:
                    Defender.Reinforce(_cfg.DiplomacyReliefTroops);   // 援军 → 兵力↑（作用于 slice 条件）
                    break;
                case PledgeType.Supply:
                    Defender.ApplySupplyRestore(_cfg);
                    CityFood += 6;                                    // 补给 → 后勤↑
                    break;
                case PledgeType.Deadline:
                    DiplPressure = Fixed.Clamp(DiplPressure - Fixed.FromFraction(25, 100), Fixed.Zero, Fixed.OneValue);
                    Log("外交斡旋为守军争取了喘息时限（压力下降）。");
                    break;
            }
        }

        // ───────────────────────── 决战 ─────────────────────────

        public BattleResult ResolveEnemyAssault()
        {
            var r = BattleResolver.ResolveAssault(Attacker, Defender, Fortification, attackerAmbush: false, _cfg);
            Attacker.TakeCasualties(r.AttackerCasualties);
            Defender.TakeCasualties(r.DefenderCasualties);
            Log($"敌军强攻：{Outcome(r.Outcome)}（战力比 {r.PowerRatio}）。");
            return r;
        }

        public static string Outcome(BattleOutcome o) => o switch
        {
            BattleOutcome.AttackerDecisive => "城破，守军溃败",
            BattleOutcome.AttackerRepelled => "敌军被击退，守城成功",
            BattleOutcome.Stalemate => "僵持消耗，敌军暂退",
            _ => "?"
        };

        public long StateHash()
        {
            unchecked
            {
                long h = Clock.TotalSegments;
                h = h * 31 + Defender.StateHash();
                h = h * 31 + Attacker.StateHash();
                h = h * 31 + Fortification.Raw;
                h = h * 31 + CityFood;
                h = h * 31 + DiplPressure.Raw;
                return h;
            }
        }

        // ───────────────────────── 存档快照（ADR-0005）─────────────────────────

        /// <summary>权威状态快照（Domain 端，含 RNG 内部状态以保证读档后续推进仍确定性）。</summary>
        public sealed record Memento(
            ulong WorldSeed, int ClockSegments,
            ForceMemento Defender, ForceMemento Attacker,
            string CmdId, string CmdName, int CmdReckRaw, int CmdCommandRaw,
            int FortificationRaw, int CityFood, int StandingRaw, int DiplPressureRaw,
            bool EnemyRaidCutActive, int RaidUnits, bool AmbushChainSpent, bool EnemyReinforced,
            ulong RaidRngState, ulong DiploRngState, ulong ScoutRngState,
            IntelMemento Intel, PledgeMemento? PendingPledge);

        public sealed record ForceMemento(
            string Id, int SideValue, int Troops,
            int MoraleRaw, int FatigueRaw, int DisciplineRaw, int SupplyRaw, int ShortageSegments);

        public sealed record IntelMemento(
            int EstTroops, int EstMoraleRaw, int EstSupplyRaw, int ConfidenceRaw,
            int LastObservedSeg, bool ReinforcementRumor);

        public sealed record PledgeMemento(
            int TypeValue, int PledgeCostRaw, int GrantScoreRaw, int ResponseValue,
            int ArrivalSegments, int BetrayRiskRaw, int StatusValue);

        public Memento Capture()
        {
            return new Memento(
                _worldSeed, Clock.TotalSegments,
                CaptureForce(Defender), CaptureForce(Attacker),
                EnemyCommander.Id, EnemyCommander.Name,
                EnemyCommander.Recklessness.Raw, EnemyCommander.Command.Raw,
                Fortification.Raw, CityFood, Standing.Raw, DiplPressure.Raw,
                EnemyRaidCutActive, RaidUnits, AmbushChainSpent, EnemyReinforced,
                _raidRng.PeekState(), _diploRng.PeekState(), _scoutRng.PeekState(),
                new IntelMemento(Intel.EstTroops, Intel.EstMorale.Raw, Intel.EstSupply.Raw,
                    Intel.Confidence.Raw, Intel.LastObservedSeg, Intel.ReinforcementRumor),
                PendingPledge == null ? null : new PledgeMemento(
                    (int)PendingPledge.Type, PendingPledge.PledgeCost.Raw, PendingPledge.GrantScore.Raw,
                    (int)PendingPledge.Response, PendingPledge.ArrivalT.TotalSegments,
                    PendingPledge.BetrayRisk.Raw, (int)PendingPledge.Status));
        }

        private static ForceMemento CaptureForce(ForceState f) => new(
            f.Id, (int)f.Side, f.Troops,
            f.UnitMorale.Raw, f.Fatigue.Raw, f.Discipline.Raw, f.SupplyState.Raw, f.ShortageSegments);

        public static SiegeState Restore(Memento m, SiegeConfig cfg)
        {
            var defender = RestoreForce(m.Defender);
            var attacker = RestoreForce(m.Attacker);
            var commander = new Commander(m.CmdId, m.CmdName,
                Fixed.FromRaw(m.CmdReckRaw), Fixed.FromRaw(m.CmdCommandRaw));

            var s = new SiegeState(cfg, m.WorldSeed, defender, attacker, commander,
                Fixed.FromRaw(m.FortificationRaw), m.CityFood, Fixed.FromRaw(m.StandingRaw));

            s.Clock = new WorldDay(m.ClockSegments);
            s.CurrentWeather = Weather.At(m.WorldSeed, s.Clock);
            s.DiplPressure = Fixed.FromRaw(m.DiplPressureRaw);
            s.EnemyRaidCutActive = m.EnemyRaidCutActive;
            s.RaidUnits = m.RaidUnits;
            s.AmbushChainSpent = m.AmbushChainSpent;
            s.EnemyReinforced = m.EnemyReinforced;
            s._raidRng.SetState(m.RaidRngState);
            s._diploRng.SetState(m.DiploRngState);
            s._scoutRng.SetState(m.ScoutRngState);
            s.Intel = new EnemyIntel(m.Intel.EstTroops, Fixed.FromRaw(m.Intel.EstMoraleRaw),
                Fixed.FromRaw(m.Intel.EstSupplyRaw), Fixed.FromRaw(m.Intel.ConfidenceRaw));
            s.Intel.RestoreObservation(m.Intel.LastObservedSeg, m.Intel.ReinforcementRumor);

            if (m.PendingPledge is { } p)
            {
                var pledge = new DiplomaticPledge(
                    (PledgeType)p.TypeValue, Fixed.FromRaw(p.PledgeCostRaw), Fixed.FromRaw(p.GrantScoreRaw),
                    (PledgeResponse)p.ResponseValue, new WorldDay(p.ArrivalSegments), Fixed.FromRaw(p.BetrayRiskRaw));
                if ((PledgeStatus)p.StatusValue == PledgeStatus.Fulfilled) pledge.MarkFulfilled();
                else if ((PledgeStatus)p.StatusValue == PledgeStatus.Betrayed) pledge.MarkBetrayed();
                s.PendingPledge = pledge;
            }
            return s;
        }

        private static ForceState RestoreForce(ForceMemento f) => ForceState.Restore(
            f.Id, (Side)f.SideValue, f.Troops,
            Fixed.FromRaw(f.MoraleRaw), Fixed.FromRaw(f.FatigueRaw),
            Fixed.FromRaw(f.DisciplineRaw), Fixed.FromRaw(f.SupplyRaw), f.ShortageSegments);
    }
}
