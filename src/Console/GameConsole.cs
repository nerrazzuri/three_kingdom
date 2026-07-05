using System;
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
            sb.Append($"  争霸：存续 {AliveCount()} 家 · 你据 {_rt.Contention.CitiesOf(PlayableCampaign.Player)} 城");
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
            int before = _rt.Contention.CitiesOf(PlayableCampaign.Player);
            _rt.ConcludeOffensive();
            int after = _rt.Contention.CitiesOf(PlayableCampaign.Player);
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
