using System;
using System.Collections.Generic;
using System.Text;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Defeat;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Talent;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Console
{
    /// <summary>
    /// 完整游戏文本控制台（把 <see cref="CampaignRuntime"/> 聚合的全部系统串成一条可玩循环——无 UI，纯文本可玩）：
    /// 选年选城开局 → 按周/季/年推进 → 治理/情报/军议/君主任务/出征攻城/守城/人心施计/晋升/自立 →
    /// 历经天下大势与一生 → 被灭则被俘续局、寿终则传承。命令分派为纯逻辑（可脚本回放、确定性）。
    /// </summary>
    public sealed class GameConsole
    {
        private readonly ISaveMedium _medium = new MemorySaveMedium();
        private CampaignRuntime _rt;
        public bool Quit { get; private set; }

        public GameConsole()
        {
            _rt = GameLauncher.StartNamed("scenario-fanshui-playable", _medium);
        }

        public CampaignRuntime Runtime => _rt;

        // ── 状态渲染 ──
        public string StatusText()
        {
            var sb = new StringBuilder();
            CareerView career = _rt.CareerView();
            ArrivalLifeView life = _rt.LifeView();
            sb.Append($"〖公元{_rt.CurrentYear}·{_rt.CurrentSeasonLabel}〗 ");
            sb.Append($"{career.RankTitle}（功{career.Merit}/名望{career.Renown}）");
            sb.Append($" · {life.Age}岁·{life.PhaseLabel}");
            if (_rt.HasRebelled) sb.Append(" · 【已自立·无退路】");
            if (_rt.IsPlayerEliminated) sb.Append(" · 【势力覆灭·待发落】");
            else if (_rt.IsLifeOver) sb.Append(" · 【寿终·待传承】");
            sb.AppendLine();
            LordMissionView m = _rt.CurrentMissionView();
            sb.AppendLine($"  {m.Order}");
            sb.Append($"  争霸：存续 {AliveCount()} 家 · 你据 {_rt.Contention.CitiesOf(_rt.Scenario.PlayerFaction)} 城");
            sb.Append($" · 手令 {_rt.ActionsInFlight}/{_rt.ActionCapacity}");
            return sb.ToString();
        }

        private int AliveCount()
        {
            int n = 0;
            foreach (PowerStanding p in _rt.Contention.Powers) if (p.Alive) n++;
            return n;
        }

        // ── 命令分派 ──
        public string Dispatch(string? line)
        {
            if (line == null) { Quit = true; return string.Empty; }
            string[] tok = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tok.Length == 0) return string.Empty;
            string v = tok[0].ToLowerInvariant();
            string a1 = tok.Length > 1 ? tok[1] : string.Empty;
            string a2 = tok.Length > 2 ? tok[2] : string.Empty;

            try
            {
                switch (v)
                {
                    // 开局
                    case "named": _rt = GameLauncher.StartNamed(a1, _medium); return $"✓ 开新局：{a1}";
                    case "gov": _rt = GameLauncher.StartGovernor(a1, _medium); return $"✓ 空降为 {Name(a1)} 太守";
                    case "starts": return ListStarts();
                    case "cities": return ListGovernorCities();

                    // 推进
                    case "w": case "week": _rt.AdvanceWeek(); return AfterAdvance("过一周");
                    case "season": _rt.AdvanceSeason(); return AfterAdvance("过一季");
                    case "year": _rt.AdvanceYear(); return AfterAdvance("过一年");

                    // 治理
                    case "req": return Cmd(_rt.Requisition(ParseLong(a1, 30)), "征粮");
                    case "repair": return Cmd(_rt.Repair(), "修工事");
                    case "appease": return Cmd(_rt.Appease(), "安抚民心");

                    // 情报 / 军议
                    case "scout": return Cmd(_rt.ScoutEnemy(), "派出侦察");
                    case "council": _rt.ConveneCouncil(); return "✓ 召开军议（军师陈列条件化建议，无胜率）";

                    // 君主任务
                    case "mission": return _rt.CurrentMissionView().Order;
                    case "checkmission": return $"任务评估：{_rt.CheckMission()}";
                    case "tribute": return _rt.PayLordTribute() ? "✓ 已上缴军粮" : "× 非献纳任务或库存不足";

                    // 出征攻城
                    case "authorize": _rt.RequestOffensiveAuthorization(); return "✓ 已请君主授权出征";
                    case "targets": return ListTargets();
                    case "offensive": _rt.BeginOffensiveDefault(); return "✓ 组装出征（太守亲征·正面强攻）。[launch] 发起";
                    case "launch": _rt.LaunchOffensive(); return _rt.HasOffensiveBattle ? "⚔ 出征战斗开始。[auto] AI代打 / [conclude] 结算" : "× 出征被门拒（未授权/非敌控/有盟约）";
                    case "auto": if (_rt.HasOffensiveBattle) { _rt.OffensiveBattleAutoResolve(); return "⚔ AI 代打至终局。[conclude] 结算占城"; } return "× 无进行中的出征战斗";
                    case "conclude": return ConcludeOffensive();

                    // 出征·战中微操（不代打，自己排兵布阵）
                    case "bview": return _rt.HasOffensiveBattle || _rt.OffensiveBattleOver ? RenderBattle(_rt.OffensiveBattleView()) : "× 无进行中的出征战斗";
                    case "round": return _rt.HasOffensiveBattle ? RenderBattle(_rt.OffensiveBattleResolveRound()) : "× 无进行中的出征战斗";
                    case "move": return _rt.OffensiveBattleMove(a1, a2).Applied ? $"✓ {a1}→{ZoneBattleText.Zone(a2)}" : "× 调动失败（非相邻/无此支队）";
                    case "posture": return _rt.OffensiveBattleSetPosture(a1, ParsePosture(a2)).Applied ? $"✓ {a1} 改姿态" : "× 改姿态失败";

                    // 守城
                    case "defend": _rt.StartDefenseBattle(); return "🛡 守城战开始。[defauto] AI代打";
                    case "defauto": return _rt.HasDefenseBattle ? (_rt.AutoResolveDefense() ? "🛡 守住了！退敌。" : "✗ 城破——势力受挫。") : "× 无守城战";

                    // 外交
                    case "pact": return Pact(a1);
                    case "breach": { BreachResult br = _rt.BreachPact(new FactionId(a1)); return $"背约于 {Name(a1)}——名望受损、对方转敌对。"; }
                    case "diplo": return RenderDiplomacy();

                    // 多城战区
                    case "theater": return RenderTheater();
                    case "delegate": { var r = _rt.DelegateCity(new ThreeKingdom.Domain.City.CityId(a1), new ThreeKingdom.Domain.Characters.CharacterId(a2)); return r.Applied ? $"✓ 委任 {Name(a2)} 打理 {Name(a1)}" : "× 委任失败（未持有/超掌管范围）"; }

                    // 人才招揽
                    case "talents": return RenderTalents();
                    case "reveal": _rt.RevealTalent(new TalentId(a1), TalentChannel.Scouting); return $"✓ 探得 {a1} 行踪，纳入视野";
                    case "recruit": return Recruit(a1);

                    // 武将全局融入（GDD_027 P1-P8）
                    case "affil": return RenderAffiliation(a1);
                    case "croster": return RenderCityRoster(a1);
                    case "pool": return RenderRecruitPool();
                    case "discover": return Discover(a1, a2);
                    case "hire": return Hire(a1, a2);
                    case "exportgen": System.IO.File.WriteAllText(a1, GeneralDossierCodec.Export()); return $"✓ 导出 {GeneralDossiers.All.Count} 员档案 → {a1}";
                    case "appoint": return Appoint(a1, a2);
                    case "unappoint": return Unappoint(a1);
                    case "life": return RenderLife(a1);
                    case "reward": return LifeMemo(a1, ThreeKingdom.Domain.Characters.MemoryKind.Rewarded, "重赏");
                    case "betray": return LifeMemo(a1, ThreeKingdom.Domain.Characters.MemoryKind.Betrayed, "背弃");
                    case "strategy": return RenderStrategies();
                    case "assignai": return RenderAssignment(a1);
                    case "dipai": return RenderDiplomaticAI();
                    case "lore": return RenderLoreEvents();

                    // 人心杠杆施计
                    case "subvert": return Subvert(a1, a2);

                    // 生涯
                    case "rebel": return Rebel();

                    // 被灭续局
                    case "fate": if (!_rt.IsPlayerEliminated) return "× 尚未覆灭"; _rt.BeginDefeat().ResolveCaptorFate(); return DefeatState();
                    case "submit": if (!_rt.IsPlayerEliminated) return "× 尚未覆灭"; _rt.BeginDefeat().Submit(); return DefeatState();
                    case "refuse": if (!_rt.IsPlayerEliminated) return "× 尚未覆灭"; _rt.BeginDefeat().Refuse(); return DefeatState();
                    case "refuge": return Refuge(a1);
                    case "continue": return _rt.ContinueUnderNewLord() ? "✓ 复位为新主太守——东山再起，天下照旧。" : "× 尚不可复位（未走完流程/新主无城可授）";

                    // 传承
                    case "heir": return _rt.IsLifeOver ? RenderHeir() : "× 尚未寿终";

                    // 视图
                    case "map": return RenderMap();
                    case "roster": return RenderRoster(ParseInt(a1, 0));
                    case "status": return StatusText();

                    // 存读档
                    case "save": return _rt.Save() ? "✓ 已存档" : "× 存档失败";
                    case "load": return _rt.Load(out string reason) ? "✓ 已读档" : $"× 读档失败：{reason}";

                    case "?": case "help": return Menu();
                    case "q": case "quit": Quit = true; return "再会。";
                    default: return $"× 未知命令「{v}」。[?] 看菜单。";
                }
            }
            catch (Exception ex)
            {
                return $"× 出错：{ex.Message}";
            }
        }

        private string AfterAdvance(string label)
        {
            var sb = new StringBuilder($"✓ {label}。");
            foreach (EventNoticeView n in _rt.EventNotices())
                sb.Append($"\n  〔天下事〕{n.Text}");
            if (_rt.IsPlayerEliminated) sb.Append("\n  ！势力覆灭——你为阶下之囚（[fate]听候发落 / [submit]归顺 / [refuse]不屈）。");
            else if (_rt.IsLifeOver) sb.Append("\n  ！大限已至——寿终（[heir]传承子嗣）。");
            return sb.ToString();
        }

        private string ConcludeOffensive()
        {
            if (!_rt.OffensiveBattleOver && !_rt.HasOffensiveBattle) return "× 无待结算的出征";
            int before = _rt.Contention.CitiesOf(_rt.Scenario.PlayerFaction);
            _rt.ConcludeOffensive();
            int after = _rt.Contention.CitiesOf(_rt.Scenario.PlayerFaction);
            return after > before ? "✓ 破城！占为己有，领土 +1。" : "退兵——未下城，战役可继续。";
        }

        private string Subvert(string cityId, string scheme)
        {
            SubversionScheme s = scheme switch
            {
                "1" or "discord" => SubversionScheme.SowDiscord,
                "2" or "defect" => SubversionScheme.InciteDefection,
                _ => SubversionScheme.UnderminedMorale,
            };
            SubversionView v = _rt.AttemptSubversion(string.IsNullOrEmpty(cityId) ? "city-hulao" : cityId, s, 100);
            return $"施计（{s}）：{v.ResultLabel}";
        }

        private string Rebel()
        {
            RebellionResult r = _rt.DeclareIndependence();
            return r.Launched ? "✓ 自立门户！从此无退路——他日若败，必被俘处死。" : "× 自立资格未达（需领城/名望/民心积累）";
        }

        private string Refuge(string factionId)
        {
            if (!_rt.IsPlayerEliminated) return "× 尚未覆灭";
            DefeatFlow f = _rt.BeginDefeat();
            if (f.Stage != DefeatStage.Released) return "× 须先 [refuse] 且获释，方可投奔";
            int cities = _rt.Contention.CitiesOf(new ThreeKingdom.Domain.Map.FactionId(factionId));
            bool ok = f.SeekRefuge(new ThreeKingdom.Domain.Map.FactionId(factionId), cities);
            return ok ? $"✓ {Name(factionId)} 纳你为太守。[continue] 复位续局。" : $"× {Name(factionId)} 不肯收留——另投他家。";
        }

        private string DefeatState()
        {
            DefeatFlow f = _rt.BeginDefeat();
            return f.Stage switch
            {
                DefeatStage.Executed => "身死——一世至此而终（[heir] 传承）。",
                DefeatStage.Submitted => "归顺新主。[continue] 复位为其太守。",
                DefeatStage.Released => "获释流亡。[refuge <势力id>] 投奔他主。",
                DefeatStage.Imprisoned => "被囚禁，静待时变。",
                DefeatStage.Captured => "阶下之囚（[submit]归顺 / [refuse]不屈）。",
                _ => f.Stage.ToString(),
            };
        }

        private string RenderHeir()
        {
            ArrivalLifeView h = _rt.SucceedHeir();
            return $"✓ 子嗣承业（第{_rt.Generation}世）——公元{h.Year}，弱冠{h.Age}岁接掌，天下照旧流转。";
        }

        // ── 视图渲染 ──
        private string RenderMap()
        {
            CampaignMapView map = _rt.MapView();
            var sb = new StringBuilder($"【战略图·公元{map.Year}·{map.Season}】势力（领城）：");
            foreach (MapFactionCell f in map.Factions)
                sb.Append($" {f.FactionName}{(f.IsPlayer ? "★" : "")}{f.CityCount}");
            sb.AppendLine();
            sb.Append($"  在场武将 {map.Heroes.Count} 员（例：");
            int shown = 0;
            foreach (MapHeroCell h in map.Heroes) { if (shown++ >= 8) break; sb.Append($"{h.HeroName}@{Name(h.CityId)} "); }
            sb.Append("…）");
            return sb.ToString();
        }

        private string RenderRoster(int page)
        {
            var all = GeneralRosterView.Build().Cards;
            int per = 20, start = page * per;
            var sb = new StringBuilder($"【武将录】共 {all.Count} 员（第 {page + 1} 页，[roster N] 翻页）：");
            for (int i = start; i < System.Math.Min(start + per, all.Count); i++)
            {
                GeneralCardView c = all[i];
                sb.Append($"\n  {c.Name}");
                if (c.Traits.Count > 0) sb.Append("〔" + string.Join("·", c.Traits) + "〕");
            }
            return sb.ToString();
        }

        private string ListStarts()
        {
            var sb = new StringBuilder("命名开局（named <id>）：");
            foreach (ScenarioChoiceLine s in ScenarioChoiceView.FromCatalog().Choices)
                sb.Append($"\n  {s.Id} — {s.Name}：{s.Blurb}");
            return sb.ToString();
        }

        private string ListGovernorCities()
        {
            var sb = new StringBuilder("任选城做太守（gov <cityId>）：");
            foreach (GovernorCityChoiceLine c in GovernorCityChoiceView.Build().Choices)
                sb.Append($"\n  {c.CityId} — {c.CityName}（{c.SuzerainName}·部将{c.GeneralCount}）");
            return sb.ToString();
        }

        private string ListTargets()
        {
            OffensiveTargetsView t = _rt.OffensiveTargets();
            var sb = new StringBuilder($"出征目标（授权：{(t.Authorized ? "是" : "否，先 [authorize]")}）：");
            foreach (OffensiveTargetLine l in t.Targets)
                sb.Append($"\n  {Name(l.CityId)}（{l.CityId}）门：{l.Gate}");
            return sb.ToString();
        }

        private string RenderBattle(ZoneBattleView v)
        {
            var sb = new StringBuilder($"【出征战·第{v.Round}/{v.MaxRounds}回合】{(_rt.OffensiveBattleOver ? "已分胜负（[conclude]结算）" : "鏖战中")}");
            foreach (ZoneLineView z in v.Zones)
            {
                sb.Append($"\n  {z.ZoneLabel}：我 {z.OwnStrength} vs 敌 {z.EnemyStrength}");
                if (z.IsObjective) sb.Append(" ★目标");
                if (z.FormedConditions.Count > 0) sb.Append(" 〔成型：" + string.Join("·", z.FormedConditions) + "〕");
                if (z.OwnDetachments.Count > 0) sb.Append(" 己方支队:" + string.Join(",", z.OwnDetachments));
            }
            sb.Append("\n  ([move <支队> <区>] 调动 · [posture <支队> <a攻|h守|f佯>] 改姿态 · [round] 推进一回合 · [auto] 代打)");
            return sb.ToString();
        }

        private static Posture ParsePosture(string s) => s switch
        {
            "a" or "assault" => Posture.Assault,
            "f" or "feint" => Posture.Feint,
            _ => Posture.Hold,
        };

        private FixedPoint RenownNorm() => FixedPoint.FromFraction(System.Math.Min(_rt.CareerView().Renown, 1000), 1000);

        private string Pact(string factionId)
        {
            var factors = new PactFactors(RenownNorm(), FixedPoint.FromFraction(1, 2), FixedPoint.FromFraction(1, 2));
            PactResult r = _rt.ProposePact(new FactionId(factionId), DiplomaticStance.NonAggression, factors);
            return r.Accepted ? $"✓ 与 {Name(factionId)} 缔「互不侵犯」之约" : $"× {Name(factionId)} 未允（名望/关系不足）";
        }

        private string RenderDiplomacy()
            => "【外交】pact <势力id> 缔互不侵犯 · breach <势力id> 背约（损名望·转敌对）。与盟/互不侵犯势力须先背约方可攻。";

        private string RenderTheater()
            => "【多城战区】占城归你后入战区；delegate <城id> <将id> 委任下属打理（掌管范围随官阶）。占城越多、越需委任。";

        private string RenderTalents()
        {
            var vis = _rt.VisibleTalents();
            if (vis.Count == 0)
                return "尚无可见人才（[reveal <id>] 探知；或推进时段待其登场）。可探：talent-wolong(卧龙) / talent-xiaojiang(骁将) / talent-nengli(能吏)";
            var sb = new StringBuilder("【可见人才】(recruit <id> 招揽)：");
            foreach (TalentProfile t in vis) sb.Append($"\n  {t.Id.Value} — {Name(t.Character.Value)}");
            return sb.ToString();
        }

        private string Recruit(string id)
        {
            var offer = new RecruitmentOffer(RenownNorm(), FixedPoint.FromFraction(1, 2), FixedPoint.FromFraction(1, 2), FixedPoint.FromFraction(1, 2));
            _rt.RecruitTalent(new TalentId(id), offer);
            return _rt.HasRecruited(new TalentId(id)) ? $"✓ {id} 出仕入伙！" : "× 未能招得（未登场/未知晓/志向未动）";
        }

        private static string Cmd(CampaignCommandResult r, string label)
            => r.Applied ? $"✓ {label}（已办，需时见效）" : $"× {label}失败：{r.Error}";

        private static string Name(string id) => DisplayNames.Of(id);

        // ---- 武将全局融入（GDD_027 P1-P8）console 视图 ----
        private static readonly string[] RoleText = { "内政", "守将", "先锋", "谋士", "斥候", "水军" };

        /// <summary>当前战局累积的演义覆盖态（GDD_027 R6）：由纪元盘→当前年确定性重放，透传给归属/城册使事件效果全局一致。</summary>
        private LoreOverrides CurrentOverrides()
            => LoreEvents.OverridesAt(new LoreContext(_rt.Scenario.AnchorYear, _rt.CurrentYear, _rt.Scenario.PlayerFaction));

        private string RenderAffiliation(string generalId)
        {
            int y = _rt.Scenario.AnchorYear;
            Affiliation a = GeneralAffiliations.AffiliationOf(new ThreeKingdom.Domain.Characters.CharacterId(generalId), y, CurrentOverrides());
            switch (a.Status)
            {
                case AffiliationStatus.Absent: return $"{Name(generalId)}：公元{y} 尚未及冠或已故——不在世间。";
                case AffiliationStatus.Wandering: return $"{Name(generalId)}：在野（未仕，可招揽 · 难度 {GeneralRecruitment.DifficultyOf(new ThreeKingdom.Domain.Characters.CharacterId(generalId))}）。";
                default: return $"{Name(generalId)}：事奉 {Name(a.Faction.Value)}，驻 {Name(a.City.Value)}，任 {RoleText[(int)a.Role]}。";
            }
        }

        private string RenderCityRoster(string cityId)
        {
            int y = _rt.Scenario.AnchorYear;
            var city = new ThreeKingdom.Domain.City.CityId(cityId);
            var roster = GeneralAffiliations.RosterOf(city, y, CurrentOverrides());
            if (roster.Count == 0) return $"{Name(cityId)}：此纪元无在职武将。";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【{Name(cityId)}·武将册】公元{y} · 在职 {roster.Count} 员（上限 {GeneralAffiliations.RosterCap}）：");
            var names = new System.Collections.Generic.List<string>();
            foreach (var g in roster) names.Add(Name(g.Value));
            sb.AppendLine("  " + string.Join("、", names));
            var admin = GovernanceContribution.AdministratorOf(roster);
            var advisor = CouncilCapability.AdvisorOf(roster);
            var army = ArmyFormation.Form(roster);
            sb.AppendLine($"  内政官：{(admin.HasValue ? Name(admin.Value.Value) : "无")}" +
                          (admin.HasValue ? $"（民心{GovernanceContribution.ModifierOf(admin.Value).MoralePercent:+0;-0;0}% 征粮{GovernanceContribution.ModifierOf(admin.Value).GrainPercent:+0;-0;0}%）" : ""));
            sb.AppendLine($"  军师：{(advisor.HasValue ? Name(advisor.Value.Value) : "无")}（{CouncilCapability.QualityLabel(advisor)}）");
            if (army.HasValue)
                sb.AppendLine($"  成军：主将 {Name(army.Value.LeaderId)}" + (army.Value.HasDeputy ? $" · 副将 {Name(army.Value.DeputyId!)}" : "（无副将）") + $"（战力贡献 {ArmyFormation.PowerContribution(army.Value)}）");
            sb.Append($"  凝聚：{RetinueCohesion.CohesionOf(roster)}（{RetinueCohesion.DefectionRiskLabel(roster)}）");
            return sb.ToString();
        }

        private string RenderRecruitPool()
        {
            // 反全知门（GDD_027 #2）：只呈已发觉人才，不再裸露全部在野将。
            int y = _rt.Scenario.AnchorYear;
            var known = _rt.KnownTalents();
            if (known.Count == 0)
                return $"【已知人才】公元{y}：暂无（未闻名者不可见——反全知）。以 [discover <将id> <scout|council|bond|visit>] 发觉，[hire <将id> <待遇0-9>] 招揽。";
            var sb = new System.Text.StringBuilder($"【已知人才】公元{y} · {known.Count} 员（未闻名者隐去）：");
            foreach (KnownTalent t in known)
            {
                string stage = t.Discovery switch
                {
                    TalentKnowledge.Heard => "听闻",
                    TalentKnowledge.Located => "已定位",
                    _ => "已接触",
                };
                string status = t.LastOutcome == RecruitOutcome.Joined ? "已入伙"
                    : t.CanAttempt ? "可招" : (t.LastOutcome == RecruitOutcome.Declined ? "婉拒·可再图" : "不可招");
                sb.Append($"\n  {Name(t.GeneralId)}〔{t.DifficultyLabel}〕[{stage}·{status}]");
            }
            return sb.ToString();
        }

        private string Discover(string id, string channel)
        {
            RecruitChannel ch = channel switch
            {
                "council" => RecruitChannel.Council,
                "bond" => RecruitChannel.Bond,
                "event" => RecruitChannel.Event,
                "visit" => RecruitChannel.Visit,
                _ => RecruitChannel.Scout,
            };
            var g = new ThreeKingdom.Domain.Characters.CharacterId(id);
            _rt.DiscoverTalent(g, ch);
            TalentKnowledge d = _rt.Talents.DiscoveryOf(g);
            if (d == TalentKnowledge.Unknown) return $"× {Name(id)} 非在野人才（在职/未在世/不存），未纳入招揽视野。";
            return $"✓ 发觉 {Name(id)}——进度：{d}（{(_rt.Talents.CanAttempt(g) ? "可招" : "尚不可招，需接触")}）。";
        }

        private string Appoint(string cityId, string generalId)
        {
            var r = _rt.AppointGeneral(new ThreeKingdom.Domain.City.CityId(cityId), new ThreeKingdom.Domain.Characters.CharacterId(generalId));
            return r switch
            {
                ThreeKingdom.Application.Scenarios.AppointGate.Ok => $"✓ 调拨 {Name(generalId)} 入 {Name(cityId)}（任用簿，存档持久）。",
                ThreeKingdom.Application.Scenarios.AppointGate.NotYours => $"× {Name(generalId)} 非你麾下（须事奉本势力或已招揽）。",
                ThreeKingdom.Application.Scenarios.AppointGate.Captive => $"× {Name(generalId)} 在押，不可任用。",
                ThreeKingdom.Application.Scenarios.AppointGate.Incapacitated => $"× {Name(generalId)} 重创，须将养。",
                ThreeKingdom.Application.Scenarios.AppointGate.Absent => $"× {Name(generalId)} 不在世间（未及冠/已故）。",
                ThreeKingdom.Application.Scenarios.AppointGate.CityFull => $"× {Name(cityId)} 城册已满（≤{ThreeKingdom.Application.Scenarios.GeneralAffiliations.RosterCap}），须先撤出。",
                ThreeKingdom.Application.Scenarios.AppointGate.AlreadyThere => $"× {Name(generalId)} 已在 {Name(cityId)}。",
                _ => "× 调拨失败（非法）。",
            };
        }

        private string Unappoint(string generalId)
        {
            var r = _rt.RemoveAppointment(new ThreeKingdom.Domain.Characters.CharacterId(generalId));
            return r == ThreeKingdom.Domain.Appointment.AppointResult.Ok
                ? $"✓ 撤 {Name(generalId)} 出任用。" : $"× {Name(generalId)} 未在任用簿。";
        }

        private ThreeKingdom.Domain.Characters.GeneralState LifeOf(string id)
        {
            var gid = new ThreeKingdom.Domain.Characters.CharacterId(id);
            int y = _rt.Scenario.AnchorYear;
            return _rt.Generals.GetOrSeed(gid, x => GeneralLifeSeeding.Seed(x, y));
        }

        private string RenderLife(string id)
        {
            var s = LifeOf(id);
            string health = s.Health switch
            {
                ThreeKingdom.Domain.Characters.GeneralHealth.Hale => "康健",
                ThreeKingdom.Domain.Characters.GeneralHealth.Wounded => "负伤",
                _ => "重创",
            };
            string risk = ThreeKingdom.Domain.Characters.GeneralLifeService.RiskOf(s) switch
            {
                ThreeKingdom.Domain.Characters.DefectionRisk.Steadfast => "忠贞",
                ThreeKingdom.Domain.Characters.DefectionRisk.Settled => "安稳",
                ThreeKingdom.Domain.Characters.DefectionRisk.Wavering => "浮动",
                _ => "离心",
            };
            string master = s.Faction.HasValue ? Name(s.Faction.Value.Value) : "无主";
            return $"【{Name(id)}·人生】主君 {master}｜忠诚 {LoyaltyBand(s.Loyalty)}｜{health}｜叛离 {risk}｜记忆 {s.Memories.Count} 桩"
                 + (s.CaptiveOf.HasValue ? "｜在押" : "");
        }

        // 忠诚不呈数字（反全知），呈定性带。
        private static string LoyaltyBand(int loyalty)
            => loyalty >= 80 ? "笃厚" : loyalty >= 60 ? "安稳" : loyalty >= 40 ? "平平" : loyalty >= 20 ? "疏离" : "离德";

        private string LifeMemo(string id, ThreeKingdom.Domain.Characters.MemoryKind kind, string verb)
        {
            var s = LifeOf(id);
            s = ThreeKingdom.Domain.Characters.GeneralLifeService.Remember(s, kind, new ThreeKingdom.Domain.Characters.CharacterId("char-player-lord"), _rt.CurrentYear, weight: 1);
            _rt.Generals.Set(s);
            return $"〔{verb}〕{Name(id)} 铭记于心。\n" + RenderLife(id);
        }

        private string Hire(string id, string offer)
        {
            var g = new ThreeKingdom.Domain.Characters.CharacterId(id);
            int offerTier = ParseInt(offer, 3);
            ulong seed = 0x3151F2A9UL ^ ((ulong)(_rt.Talents.AttemptsOf(g) + 1) * 40503UL);
            RecruitAttemptResult r = _rt.RecruitGeneral(g, offerTier, seed);
            return r.Accepted ? $"〔招揽〕{r.Message}" : $"× {r.Message}";
        }

        private string RenderAssignment(string cityId)
        {
            var a = GeneralAssignmentService.Recommend(new ThreeKingdom.Domain.City.CityId(cityId), _rt.Scenario.AnchorYear, CurrentOverrides());
            string N(ThreeKingdom.Domain.Characters.CharacterId? g) => g.HasValue ? Name(g.Value.Value) : "（无）";
            return $"【{Name(cityId)}·任命荐】守将 {N(a.DefenderLead)}｜军师 {N(a.Advisor)}｜内政 {N(a.Governor)}｜先锋 {N(a.Vanguard)}";
        }

        private string RenderDiplomaticAI()
        {
            var views = _rt.PlayerStances();
            if (views.Count == 0) return "天下无对手。";
            var sb = new System.Text.StringBuilder();
            if (_rt.CoalitionAgainstPlayer()) sb.Append("⚠ 【合纵】诸侯联合，共讨于你！\n");
            sb.Append("【各势力对你的外交立场】");
            foreach (var v in views)
            {
                string stance = v.Stance switch
                {
                    ThreeKingdom.Domain.Contention.PlayerStance.Submissive => "臣服求和",
                    ThreeKingdom.Domain.Contention.PlayerStance.Neutral => "中立",
                    ThreeKingdom.Domain.Contention.PlayerStance.Wary => "警惕",
                    ThreeKingdom.Domain.Contention.PlayerStance.Hostile => "敌意",
                    _ => "合纵抗你",
                };
                sb.Append($"\n  {Name(v.Faction.Value)}：{stance}");
            }
            return sb.ToString();
        }

        private string RenderStrategies()
        {
            var views = _rt.FactionStrategies();
            if (views.Count == 0) return "天下已定或无对手。";
            var sb = new System.Text.StringBuilder("【天下大势·各势力战略】");
            foreach (var v in views)
            {
                string intent = v.Intent switch
                {
                    ThreeKingdom.Domain.Contention.StrategicIntent.Expansion => "扩张",
                    ThreeKingdom.Domain.Contention.StrategicIntent.Opportunist => "趁火打劫",
                    ThreeKingdom.Domain.Contention.StrategicIntent.Defense => "固守",
                    ThreeKingdom.Domain.Contention.StrategicIntent.Recovery => "休整恢复",
                    ThreeKingdom.Domain.Contention.StrategicIntent.Revenge => "图谋报复",
                    ThreeKingdom.Domain.Contention.StrategicIntent.Diplomacy => "求存外交",
                    _ => "濒临崩溃",
                };
                string threat = v.ThreatToPlayer switch { 3 => "·大威胁", 2 => "·威胁", 1 => "·小患", _ => "" };
                sb.Append($"\n  {Name(v.Faction.Value)}：{intent}{threat}");
            }
            return sb.ToString();
        }

        private string RenderLoreEvents()
        {
            var ctx = new LoreContext(_rt.Scenario.AnchorYear, _rt.CurrentYear, _rt.Scenario.PlayerFaction);
            var fired = LoreEvents.FiredAt(ctx);
            var ov = LoreEvents.OverridesAt(ctx);
            var sb = new System.Text.StringBuilder();

            // 当轮触发的招牌事件（叙事 + 效果）。
            if (fired.Count == 0)
                sb.AppendLine($"公元{_rt.CurrentYear}：当下无演义事件触发（事件锚定具名武将，按在世/归属/纪元触发）。");
            else
                foreach (LoreEvent e in fired)
                {
                    sb.AppendLine($"〔{e.Name}〕{e.Narrative}");
                    foreach (LoreEffect eff in e.Effects) sb.AppendLine($"    ▸ {eff.Describe(g => Name(g.Value))}");
                }

            // 开局至今累积的世界变更（演义覆盖层，可推演不入档）。
            if (!ov.IsEmpty)
            {
                sb.AppendLine($"【演义已改写天下】公元{ctx.AnchorYear}→{_rt.CurrentYear}：");
                if (ov.Slain.Count > 0) sb.AppendLine("  陨落：" + string.Join("、", NamesOf(ov.Slain)));
                if (ov.Reassigned.Count > 0)
                {
                    var moves = new List<string>();
                    foreach (var kv in ov.Reassigned) moves.Add($"{Name(kv.Key)}→{(kv.Value == null ? "在野" : kv.Value.Value.Value)}");
                    sb.AppendLine("  移籍：" + string.Join("、", moves));
                }
                if (ov.Introduced.Count > 0) sb.AppendLine("  登场：" + string.Join("、", NamesOf(ov.Introduced)));
            }
            return sb.ToString().TrimEnd();
        }

        private static IEnumerable<string> NamesOf(IEnumerable<string> ids)
        {
            foreach (string id in ids) yield return Name(id);
        }
        private static long ParseLong(string s, long def) => long.TryParse(s, out long v) ? v : def;
        private static int ParseInt(string s, int def) => int.TryParse(s, out int v) ? v : def;

        public static string Menu() =>
            "【命令】\n" +
            " 开局: starts / named <id> / cities / gov <cityId>\n" +
            " 推进: w(周) season(季) year(年) · status · map · roster [页]\n" +
            " 治理: req <n> / repair / appease · 情报: scout / council\n" +
            " 君命: mission / checkmission / tribute\n" +
            " 出征: authorize→targets→offensive→launch→(auto代打 或 bview/move/posture/round 微操)→conclude\n" +
            " 守城: defend / defauto · 施计: subvert <cityId> <1离间|2策反|3攻心>\n" +
            " 外交: pact <势力id> / breach <势力id> / diplo · 多城: theater / delegate <城> <将>\n" +
            " 人才: talents / reveal <id> / recruit <id> · 生涯: rebel(自立·无退路)\n" +
            " 被灭: fate / submit / refuse / refuge <势力id> / continue · 传承: heir\n" +
            " 存档: save / load · 帮助: ? · 退出: q";
    }
}
