using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 运行期当前<b>战役会话</b>的进程内单一来源（跨场景存活的静态状态，epic-028 story-001）。
    /// 把 Unity 壳接到完整 <c>CampaignSession</c> 脊梁（M00~M10 全 11 循环，ADR-0009）——不再指旧竖切
    /// <c>SessionService</c>/<c>GameSession</c>。生命周期逻辑在纯 C# <see cref="CampaignRuntime"/>
    /// （dotnet 已单测）；本类只是 Unity 侧薄静态壳 + <see cref="PlayerPrefsSaveMedium"/> 介质注入。
    /// <b>不持有可变 Domain 对象</b>；UI 只拿只读 <see cref="WorldStatusView"/>（ADR-0002）。
    /// 其余面板投影（账本/敌情/军议/花名册等）随 story-003/004 逐屏接入。
    /// </summary>
    public static class SessionRuntime
    {
        // 可换：选年/选城开局会以新场景重建（GDD_026）。默认汜水关太守。
        private static CampaignRuntime _runtime =
            new CampaignRuntime(new PlayerPrefsSaveMedium());

        /// <summary>开新局（MainMenu「新游戏」）：以共享场景配置（汜水关太守）开局，返回初始世界状态视图。</summary>
        public static WorldStatusView NewGame() => _runtime.NewGame();

        /// <summary>推进一个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public static WorldStatusView Advance() => _runtime.Advance(1);

        /// <summary>取当前世界状态视图（不推进；纯函数渲染恒等）。</summary>
        public static WorldStatusView Status() => _runtime.Status();

        /// <summary>本局主角人设展示视图（GDD_015：开局随机性情，给心里话着色）。</summary>
        public static PersonaView Persona() => _runtime.PersonaView();

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public static bool HasSave() => _runtime.HasSave();

        /// <summary>原子存档当前会话（统一信封；失败保留上一份有效存档）；返回是否成功。</summary>
        public static bool Save() => _runtime.Save();

        /// <summary>读取默认槽恢复会话；成功切换当前会话返回 true，失败返回 false 与原因且不动当前会话。</summary>
        public static bool Load(out string reason) => _runtime.Load(out reason);

        // --- 军议/敌情屏（story-003 / TR-ux-002/003）：UI 只拿只读展示模型，反全知只经玩家知识投影。---

        /// <summary>会话是否启用军议/情报循环（面板可用性）。</summary>
        public static bool HasIntel() => _runtime.HasIntel;

        /// <summary>召开军议（HUD「召开军议」），返回军议屏展示模型（并列建议 + 定性置信档，无成功率/唯一推荐）。</summary>
        public static CampaignCouncilView ConveneCouncil() => _runtime.ConveneCouncil();

        /// <summary>取最近军议对当前知识快照的展示模型（不重开；侦察后其 IsStale 变真）。未召开过为 null。</summary>
        public static CampaignCouncilView CurrentCouncil() => _runtime.CurrentCouncilView();

        /// <summary>取敌情面板展示模型（GDD_007，仅估计值/来源/时效，无真值）。</summary>
        public static CampaignEnemyIntelPanelView EnemyIntel() => _runtime.EnemyIntel();

        /// <summary>【story-003 最小重定向·story-004 换延迟派出】即时侦察敌军主力并入知识；返回是否成功。</summary>
        public static bool Scout() => _runtime.ScoutEnemy().Applied;

        // --- 战役主循环（story-004 / TR-ux-001/005）：治理/备战/相位/战斗，UI 只读投影 + 命令结果。---

        /// <summary>当前回合数（1 起；新手引导前 N 回合判定，story-005）。</summary>
        public static int Round() => _runtime.Round;

        /// <summary>当前相位 + 该相位合法可做动作集（AC-5）。</summary>
        public static HudPhaseView Phase() => _runtime.Phase();

        /// <summary>治理面板（多维账本 + 三动作因果说明）。</summary>
        public static GovernanceActionView Governance() => _runtime.Governance();

        /// <summary>征用军粮；返回命令结果（失败含稳定错误码）。</summary>
        public static CampaignCommandResult Requisition(long amount) => _runtime.Requisition(amount);

        /// <summary>修工事；返回命令结果。</summary>
        public static CampaignCommandResult Repair() => _runtime.Repair();

        /// <summary>安抚民心；返回命令结果。</summary>
        public static CampaignCommandResult Appease() => _runtime.Appease();

        /// <summary>备战面板（草稿 vs 已提交）。</summary>
        public static PrepPanelView Prep() => _runtime.Prep();

        /// <summary>加入设伏草稿命令。</summary>
        public static CampaignCommandResult AddAmbushOrder() => _runtime.AddAmbushOrder();

        /// <summary>移除草稿命令。</summary>
        public static CampaignCommandResult RemoveOrder(string orderId) => _runtime.RemoveOrder(orderId);

        /// <summary>提交计划（原子承诺，不可反悔）；返回是否成功。</summary>
        public static bool SubmitPlan() => _runtime.SubmitPlan();

        /// <summary>兵法条件进度（战中相位）。</summary>
        public static BattleConditionProgressView BattleConditionProgress() => _runtime.BattleConditionProgress();

        /// <summary>开战（复用脚本战斗，要求已提交计划）；返回命令结果。</summary>
        public static CampaignCommandResult StartBattle() => _runtime.StartBattle();

        /// <summary>结算战果，返回战后复盘展示模型。</summary>
        public static BattleReviewView ResolveOutcome() => _runtime.ResolveOutcome();

        // --- 出征攻城入口（GDD_019 v2 / ADR-0010/0011）：选目标 + 授权门 + 六维组装 + 发起 + 占城归属。---

        /// <summary>请君主授权出征（登记场景可攻目标）。</summary>
        public static void RequestOffensiveAuthorization() => _runtime.RequestOffensiveAuthorization();

        /// <summary>出征选目标视图（目标城 + 授权门状态）。</summary>
        public static OffensiveTargetsView OffensiveTargets() => _runtime.OffensiveTargets();

        /// <summary>以场景首个可攻目标开始组装出征草稿；返回可变草稿供 UI 修改六维。</summary>
        public static OffensivePlan BeginOffensive() => _runtime.BeginOffensiveDefault();

        /// <summary>当前出征草稿（null=未开始）。</summary>
        public static OffensivePlan CurrentOffensivePlan => _runtime.CurrentOffensivePlan;

        /// <summary>可选副将花名册。</summary>
        public static System.Collections.Generic.IReadOnlyList<ThreeKingdom.Domain.Conquest.OffensiveGeneral> DeputyRoster
            => _runtime.DeputyRoster;

        /// <summary>当前草稿的计划预览（预计战力/士气/成型条件 + 缺失提示，无胜率）。</summary>
        public static OffensivePlanView PreviewOffensive() => _runtime.PreviewOffensive();

        /// <summary>发起出征：授权门通过则进入区域战斗（多回合），返回发起结果（战斗进行中/被门拒）。</summary>
        public static OffensiveResultView LaunchOffensive() => _runtime.LaunchOffensive();

        // --- 出征区域战斗驱动（GDD_021；发起后逐回合排兵布阵 → 终局结算占城）---

        /// <summary>出征战斗进行中。</summary>
        public static bool HasOffensiveBattle => _runtime.HasOffensiveBattle;
        /// <summary>出征战斗已分胜负（待结算）。</summary>
        public static bool OffensiveBattleOver => _runtime.OffensiveBattleOver;
        /// <summary>出征战斗当前投影。</summary>
        public static ZoneBattleView OffensiveBattleView() => _runtime.OffensiveBattleView();
        /// <summary>战中调动己方支队到相邻区。</summary>
        public static bool OffensiveBattleMove(string detId, string zoneId) => _runtime.OffensiveBattleMove(detId, zoneId).Applied;
        /// <summary>推进出征战斗一回合（敌AI + 结算）。</summary>
        public static ZoneBattleView OffensiveBattleResolveRound() => _runtime.OffensiveBattleResolveRound();
        /// <summary>挂 AI 代打出征至终局（不结算）。</summary>
        public static ZoneBattleView OffensiveBattleAutoResolve() => _runtime.OffensiveBattleAutoResolve();
        /// <summary>战斗终局后结算出征后果（占城归属 / 退兵可继续）。</summary>
        public static OffensiveResultView ConcludeOffensive() => _runtime.ConcludeOffensive();

        // --- 守城区域防御战（攻守统一，玩家守方）---

        /// <summary>发起守城区域防御战，返回初始投影。</summary>
        public static ZoneBattleView StartDefenseBattle() => _runtime.StartDefenseBattle();
        /// <summary>守城战进行中。</summary>
        public static bool HasDefenseBattle => _runtime.HasDefenseBattle;
        /// <summary>守城战已分胜负。</summary>
        public static bool DefenseBattleOver => _runtime.DefenseBattleOver;
        /// <summary>守城战当前投影。</summary>
        public static ZoneBattleView DefenseBattleView() => _runtime.DefenseBattleView();
        /// <summary>守城战中调动己方守军到相邻区。</summary>
        public static bool DefenseBattleMove(string detId, string zoneId) => _runtime.DefenseBattleMove(detId, zoneId).Applied;
        /// <summary>推进守城战一回合。</summary>
        public static ZoneBattleView DefenseBattleResolveRound() => _runtime.DefenseBattleResolveRound();
        /// <summary>挂 AI 代打守城至终局（不结算）。</summary>
        public static ZoneBattleView DefenseBattleAutoResolve() => _runtime.DefenseBattleAutoResolve();
        /// <summary>守城是否守住。</summary>
        public static bool DefenseHeld => _runtime.DefenseHeld;

        // --- E3：把此前 HUD 够不到的 5 个系统接给玩家（施计/外交/多城/人才/争霸）。逻辑经 CampaignRuntime（dotnet 已单测）。---

        // 人心杠杆施计（GDD_024）。
        public static SubversionView Subvert(string cityId, ThreeKingdom.Domain.Subversion.SubversionScheme scheme, int intensityPercent = 100)
            => _runtime.AttemptSubversion(cityId, scheme, intensityPercent);
        public static bool HasPendingSubversion(string cityId) => _runtime.HasPendingSubversion(cityId);

        // 君主争霸 + 统一终局（GDD_017/018）。
        public static ThreeKingdom.Domain.Contention.ContentionState Contention => _runtime.Contention;
        public static ThreeKingdom.Domain.Contention.EndgameStatus Endgame() => _runtime.Endgame();

        // 战略外交（GDD_023）。
        public static ThreeKingdom.Domain.Diplomacy.PactResult ProposePact(
            ThreeKingdom.Domain.Map.FactionId power, ThreeKingdom.Domain.Diplomacy.DiplomaticStance target, ThreeKingdom.Domain.Diplomacy.PactFactors factors)
            => _runtime.ProposePact(power, target, factors);
        public static ThreeKingdom.Domain.Diplomacy.BreachResult BreachPact(ThreeKingdom.Domain.Map.FactionId power)
            => _runtime.BreachPact(power);

        // 多城战区委任（GDD_022）。
        public static System.Collections.Generic.IReadOnlyList<ThreeKingdom.Domain.Theater.TheaterCityReport> TheaterReports(ThreeKingdom.Domain.Theater.TheaterResources reported)
            => _runtime.TheaterReports(reported);
        public static ThreeKingdom.Application.Theater.TheaterCommandResult DelegateCity(
            ThreeKingdom.Domain.City.CityId city, ThreeKingdom.Domain.Characters.CharacterId governor)
            => _runtime.DelegateCity(city, governor);
        public static ThreeKingdom.Application.Theater.TheaterCommandResult SelfGovernCity(ThreeKingdom.Domain.City.CityId city)
            => _runtime.SelfGovernCity(city);

        // 人才招揽（GDD_020）。
        public static ThreeKingdom.Application.Talent.TalentRecruitAttempt RecruitTalent(
            ThreeKingdom.Domain.Talent.TalentId id, ThreeKingdom.Domain.Talent.RecruitmentOffer offer)
            => _runtime.RecruitTalent(id, offer);
        public static bool HasRecruited(ThreeKingdom.Domain.Talent.TalentId id) => _runtime.HasRecruited(id);

        // --- GDD_026 空降者·纪元开局与一生：选年 → 选城/剧本 → 开局；纪元/寿命；被俘续局；武将录。---

        /// <summary>可选锚点年（当前 190 讨董）。</summary>
        public static System.Collections.Generic.IReadOnlyList<AnchorYearLine> AnchorYears() => GameLauncher.AnchorYears();
        /// <summary>命名开局（汜水关太守 / 刘备·小沛 / 孙策·江东）。</summary>
        public static ScenarioChoiceView NamedStarts() => GameLauncher.NamedStarts();
        /// <summary>某锚点年可做太守的城（选城屏）。</summary>
        public static GovernorCityChoiceView GovernorCities(int anchorYear = 190) => GameLauncher.GovernorCities(anchorYear);

        /// <summary>以某命名剧本开新局（替换当前会话）。</summary>
        public static void StartNamedGame(string startId)
        {
            var start = ThreeKingdom.Application.Scenarios.PlayableStartCatalog.ById(startId)
                        ?? ThreeKingdom.Application.Scenarios.PlayableStartCatalog.Default;
            _runtime = new CampaignRuntime(new PlayerPrefsSaveMedium(), ThreeKingdom.Application.Scenarios.PlayableCampaign.ForStart(start));
            _runtime.NewGame();
        }

        /// <summary>空降为某城太守开新局（任选城；该年该城武将归你，替换当前会话）。</summary>
        public static void StartGovernorGame(string cityId)
        {
            var start = ThreeKingdom.Application.Scenarios.PlayableCampaign.GovernorStartOf(new ThreeKingdom.Domain.City.CityId(cityId));
            _runtime = new CampaignRuntime(new PlayerPrefsSaveMedium(), ThreeKingdom.Application.Scenarios.PlayableCampaign.ForStart(start));
            _runtime.NewGame();
        }

        /// <summary>HUD 顶栏当前席位目标（真实反映所选开局的治所/宗主/锋芒，取代硬编码汜水关）。</summary>
        public static string SeatObjective() => _runtime.SeatObjective;

        /// <summary>当前公元年。</summary>
        public static int CurrentYear() => _runtime.CurrentYear;
        /// <summary>当前季（春/夏/秋/冬）。</summary>
        public static string Season() => _runtime.CurrentSeasonLabel;
        /// <summary>空降者一生视图（公元年/年龄/人生阶段/是否寿终）。</summary>
        public static ArrivalLifeView Life() => _runtime.LifeView();

        /// <summary>推进一周（世界地图日常步）。</summary>
        public static WorldStatusView AdvanceWeek() => _runtime.AdvanceWeek();
        /// <summary>跳时·过一季。</summary>
        public static WorldStatusView AdvanceSeason() => _runtime.AdvanceSeason();
        /// <summary>跳时·过一年。</summary>
        public static WorldStatusView AdvanceYear() => _runtime.AdvanceYear();

        /// <summary>是否已寿终（一世自然落幕，可传承）。</summary>
        public static bool IsLifeOver() => _runtime.IsLifeOver;
        /// <summary>传承：寿终后子嗣续局（同世界同治所，新一世自当前年弱冠起）。</summary>
        public static ArrivalLifeView SucceedHeir() => _runtime.SucceedHeir();

        // 势力被灭走向（GDD_026 补：被俘→判生死→归顺?→释放?→投奔→活世界续局；唯身死才终）。
        /// <summary>玩家势力是否已覆灭。</summary>
        public static bool IsEliminated() => _runtime.IsPlayerEliminated;
        /// <summary>进入被俘流程（UI 驱动判生死/归顺/不归顺/投奔）。</summary>
        public static ThreeKingdom.Domain.Defeat.DefeatFlow BeginDefeat() => _runtime.BeginDefeat();
        /// <summary>归顺/被收留后在活世界复位为新主太守（保当前年/一生）；返回是否复位。</summary>
        public static bool ContinueUnderNewLord() => _runtime.ContinueUnderNewLord();
        /// <summary>发起自立（红线：自立后若被灭必被俘处死，无退路）。</summary>
        public static ThreeKingdom.Domain.Career.RebellionResult DeclareIndependence() => _runtime.DeclareIndependence();
        /// <summary>是否已自立（叛主，无退路）。</summary>
        public static bool HasRebelled => _runtime.HasRebelled;

        /// <summary>武将录（反全知：中文名 + 气质性情，无数值）。</summary>
        public static GeneralRosterView Roster() => GeneralRosterView.Build();

        /// <summary>战略大地图投影（城归属 + 势力 + 纪元；供 campaign map 适配器映射到 scaffold ViewModel）。</summary>
        public static CampaignMapView MapView() => _runtime.MapView();

        /// <summary>生涯视图（官阶中文头衔 + 功绩/名望 + 是否在野）。</summary>
        public static CareerView Career() => _runtime.CareerView();

        /// <summary>顶栏聚合视图（UI 单一绑定：纪元/一生/生涯/君命/手令/争霸 一次取全）。</summary>
        public static GameHudView HudSummary() => _runtime.HudSummary();

        /// <summary>可招人才录（反全知无数值：名/专长/难度定性）。</summary>
        public static TalentRecruitView TalentView() => _runtime.TalentView();

        /// <summary>外交态一览（各势力立场中文 + 可否径攻）。</summary>
        public static DiplomacyView DiplomacyView() => _runtime.DiplomacyView();

        // --- UI 便捷动作桥（把复杂参数封在这里，Unity 控制器只传 id/简单值）。---

        /// <summary>向某势力提议缔「互不侵犯」（因子取自当前名望）；返回是否达成。</summary>
        public static bool ProposeNonAggression(string factionId)
        {
            var renownNorm = ThreeKingdom.Domain.Numerics.FixedPoint.FromFraction(System.Math.Min(_runtime.CareerView().Renown, 1000), 1000);
            var factors = new ThreeKingdom.Domain.Diplomacy.PactFactors(renownNorm, ThreeKingdom.Domain.Numerics.FixedPoint.FromFraction(1, 2), ThreeKingdom.Domain.Numerics.FixedPoint.FromFraction(1, 2));
            return _runtime.ProposePact(new ThreeKingdom.Domain.Map.FactionId(factionId), ThreeKingdom.Domain.Diplomacy.DiplomaticStance.NonAggression, factors).Accepted;
        }

        /// <summary>背约于某势力（损名望·转敌对）。</summary>
        public static void Breach(string factionId) => _runtime.BreachPact(new ThreeKingdom.Domain.Map.FactionId(factionId));

        /// <summary>经侦察知晓某人才（纳入视野）。</summary>
        public static void RevealTalentScouting(string talentId)
            => _runtime.RevealTalent(new ThreeKingdom.Domain.Talent.TalentId(talentId), ThreeKingdom.Domain.Talent.TalentChannel.Scouting);

        /// <summary>招揽某人才（因子取自当前名望）；返回是否入伙。</summary>
        public static bool RecruitTalentSimple(string talentId)
        {
            var n = ThreeKingdom.Domain.Numerics.FixedPoint.FromFraction(System.Math.Min(_runtime.CareerView().Renown, 1000), 1000);
            var half = ThreeKingdom.Domain.Numerics.FixedPoint.FromFraction(1, 2);
            _runtime.RecruitTalent(new ThreeKingdom.Domain.Talent.TalentId(talentId), new ThreeKingdom.Domain.Talent.RecruitmentOffer(n, half, half, half));
            return _runtime.HasRecruited(new ThreeKingdom.Domain.Talent.TalentId(talentId));
        }

        /// <summary>结算当前君主任务（完成计功、失败撤任务并损名望）；返回进度。</summary>
        public static ThreeKingdom.Domain.Career.MissionProgress ResolveMission() => _runtime.CheckMission();
        /// <summary>献纳：从治所库存实扣所需军粮。</summary>
        public static bool PayTribute() => _runtime.PayLordTribute();
        /// <summary>多城战区态（直辖/委任）。</summary>
        public static ThreeKingdom.Domain.Theater.TheaterState TheaterState => _runtime.Theater;

        /// <summary>行动容量（手令 已用/上限，随官阶）。</summary>
        public static int ActionCapacity => _runtime.ActionCapacity;
        public static int ActionsInFlight => _runtime.ActionsInFlight;

        /// <summary>当前君主任务展示（讨伐/守土/献纳 + 目标 + 期限）。</summary>
        public static LordMissionView Mission() => _runtime.CurrentMissionView();
        /// <summary>评估并结算当前君主任务（完成计功绩、失败撤任务并损名望）；返回进度。</summary>
        public static ThreeKingdom.Domain.Career.MissionProgress CheckMission() => _runtime.CheckMission();
        /// <summary>献纳任务：从治所库存实扣所需军粮并记为已缴（库存不足返回 false）。</summary>
        public static bool PayLordTribute() => _runtime.PayLordTribute();
    }
}
