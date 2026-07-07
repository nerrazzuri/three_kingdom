using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Application.Talent;
using ThreeKingdom.Application.Theater;
using ThreeKingdom.Domain.Theater;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Defeat;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Life;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>
    /// 战役会话运行期核心（epic-028 story-001 / TR-ux-005）：Unity 壳与完整 <see cref="CampaignSession"/>
    /// 脊梁之间的<b>纯 C# 生命周期接缝</b>——新局 / 推进 / 状态投影 / 统一信封存读档。
    /// <para>
    /// 架构边界（ADR-0002 / ADR-0009）：只持 Application 会话句柄，一切变更经 <see cref="CampaignSessionService"/>；
    /// 投影为纯函数（同会话态渲染恒等，ADR-0004）；存档 I/O 经 <see cref="ISaveMedium"/> 端口注入（R-7），
    /// 原子写回沿 SaveRepository 同款「临时槽 + 原子改名」编排（失败保留上一份有效存档，ADR-0005）。
    /// 场景配置来自注入的 <see cref="PlayableCampaign"/>（与 console harness 单一同源，勿复制数值）。
    /// 本类无 UnityEngine 依赖，可 <c>dotnet test</c>（Unity 侧薄壳见 Assets/UI/SessionRuntime.cs）。
    /// </para>
    /// </summary>
    public sealed class CampaignRuntime
    {
        /// <summary>默认存档槽名（与旧竖切 "campaign" 槽区分，避免旧格式误读）。</summary>
        public const string DefaultSlot = "campaign-session";

        private readonly CampaignSessionService _service = new CampaignSessionService();
        private readonly ISaveMedium _medium;
        private readonly PlayableCampaign _scenario;
        private readonly string _slot;
        private CampaignSession? _session;
        private int _daysCrossedLastAdvance;

        /// <summary>军议/敌情屏调节项（置信档阈值等；表现态，不入会话/存档）。</summary>
        private readonly CouncilIntelTuning _councilTuning = CouncilIntelTuning.Default;

        /// <summary>最近一次召开军议的建议集（绑定召开时知识快照；用于对实时快照重判过时）。null=尚未召开。</summary>
        private CouncilAdviceSet? _lastCouncil;

        /// <summary>构造运行期核心；存档介质必须注入（端口），场景缺省为「汜水关太守」共享场景源。</summary>
        public CampaignRuntime(ISaveMedium medium, PlayableCampaign? scenario = null, string slot = DefaultSlot)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _scenario = scenario ?? PlayableCampaign.Default();
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            _slot = slot;
        }

        /// <summary>当前会话（首访自动开局，保证 HUD 单独打开也可玩）。仅供后续屏 story 经服务命令使用。</summary>
        public CampaignSession Session => _session ??= StartNew();

        /// <summary>共享场景源（不可变配置；供屏 story 取卫星配置/梯队等，勿复制数值）。</summary>
        public PlayableCampaign Scenario => _scenario;

        /// <summary>开新局（MainMenu「新游戏」）：以共享场景配置重开会话，返回初始世界状态视图。</summary>
        public WorldStatusView NewGame()
        {
            _session = StartNew();
            _daysCrossedLastAdvance = 0;
            _generation = 0;
            _lifeStartYearOverride = null;
            _life = null;
            _calendar = null;
            _defeat = null;
            _capitalOverride = null;
            _rebelled = false;
            _mission = null;
            _missionCount = 0;
            _tributeDelivered = 0;
            return Status();
        }

        /// <summary>推进 <paramref name="segments"/> 个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public WorldStatusView Advance(int segments = 1)
        {
            CampaignSession session = Session;
            int dayBefore = session.CurrentTime.Day;
            int yearBefore = Calendar.YearsElapsed(session.CurrentTime);
            _service.Advance(session, segments);
            _daysCrossedLastAdvance = session.CurrentTime.Day - dayBefore;
            int yearsCrossed = Calendar.YearsElapsed(session.CurrentTime) - yearBefore;

            // 天下大势在轨推演（GDD_015）：触发到期历史事件 → 按可达性 + 主角人设产出通报流（含心里话）。
            RefreshEventNotices(session);

            // 君主争霸自动推演（GDD_017/018）：每跨一年至多一次种子化兼并（放慢，2026-07-05：由季改年 + 缓和权重）；
            // 走向由 AI 涌现自掌、不照搬演义；强弱相当自刹车 → 三/四国鼎立可久持。终局既定则止。
            for (int y = 0; y < yearsCrossed
                 && Endgame() == ThreeKingdom.Domain.Contention.EndgameStatus.Ongoing; y++)
                AdvanceContention();
            return Status();
        }

        // --- 时间尺度（GDD_026，2026-07-05）：世界地图一步=一周；跳时按季/年，供闲时快进（不必逐周点完一生）。---

        /// <summary>一周折合的时段数（= 一"日"单元；世界步以此为原子）。</summary>
        private static int WeekSegments => WorldTime.SegmentsPerDay;

        /// <summary>推进一周（世界地图日常步）。</summary>
        public WorldStatusView AdvanceWeek() => Advance(WeekSegments);
        /// <summary>跳时·过一季（13 周；争霸推一步、大势按年落季）。</summary>
        public WorldStatusView AdvanceSeason() => Advance(WeekSegments * Calendar.WeeksPerSeason);
        /// <summary>跳时·过一年（52 周）。</summary>
        public WorldStatusView AdvanceYear() => Advance(WeekSegments * Calendar.WeeksPerYear);

        private static readonly string[] SeasonLabels = { "春", "夏", "秋", "冬" };

        /// <summary>当前季序（0春/1夏/2秋/3冬）。</summary>
        public int CurrentSeason => Calendar.SeasonOfYear(Session.CurrentTime);
        /// <summary>当前季中文。</summary>
        public string CurrentSeasonLabel => SeasonLabels[CurrentSeason];

        private readonly List<EventNoticeView> _lastNotices = new List<EventNoticeView>();
        private readonly EventReflectionService _reflection = new EventReflectionService();

        /// <summary>最近一次推进产生的天下事件通报（可述/切身；背景事件不入通报，不打扰玩家）。</summary>
        public IReadOnlyList<EventNoticeView> EventNotices() => _lastNotices;

        private void RefreshEventNotices(CampaignSession session)
        {
            _lastNotices.Clear();
            if (!session.HasHistory) return;
            ProtagonistPersona persona = Persona;
            foreach (HistoryAdvanceResult r in _service.AdvanceHistory(session))
            {
                EventReflection? refl = _reflection.Reflect(r, MonologueCatalog.Default, persona);
                if (refl != null && refl.Tier != NoticeTier.Background)
                    _lastNotices.Add(new EventNoticeView(refl));
            }
        }

        /// <summary>取当前世界状态视图（不推进；纯函数——同会话态两次调用结果恒等）。</summary>
        public WorldStatusView Status()
        {
            WorldTime t = Session.CurrentTime;
            return new WorldStatusView(new WorldStatusProjection(t.Day, t.Segment, t.AbsoluteIndex, _daysCrossedLastAdvance));
        }

        // --- 纪元与一生（GDD_026 / ADR-0015）：公元年由抽象日-段派生（纯函数）；空降者寿命由会话 id 确定性派生。皆无需新存档字段。---

        /// <summary>本局纪元日历（锚点年 + 年折算；纯函数派生公元年）。</summary>
        private EraCalendar Calendar => _calendar ??= new EraCalendar(_scenario.AnchorYear);
        private EraCalendar? _calendar;

        /// <summary>当前公元年（GDD_026：由 WorldTime 派生，存读档一致）。</summary>
        public int CurrentYear => Calendar.YearOf(Session.CurrentTime);

        /// <summary>
        /// 本局空降者的一生（入场年龄/寿命；种子化确定性派生）。传承后为子嗣一世——世代数扰动种子、起始年改当前年。
        /// </summary>
        private ArrivalLife Life => _life ??= ArrivalLife.Roll(
            PersonaSeed(Session.Id) ^ 0x11FE_5A1Dul ^ (ulong)_generation,
            _lifeStartYearOverride ?? _scenario.AnchorYear, ArrivalLifeConfig.Default);
        private ArrivalLife? _life;
        private int _generation;
        private int? _lifeStartYearOverride;

        /// <summary>当前世代（0=开局空降者，1+=历代子嗣）。</summary>
        public int Generation => _generation;

        /// <summary>当前空降者是否已寿终（一世自然落幕，可传承续局）。</summary>
        public bool IsLifeOver => Life.IsOver(CurrentYear);

        /// <summary>空降者一生视图（当前公元年/年龄/人生阶段/是否寿终；定性档，不给精确倒计时）。</summary>
        public ArrivalLifeView LifeView() => new ArrivalLifeView(CurrentYear, Life);

        /// <summary>生涯视图（GDD_014 / W5）：官阶中文头衔 + 功绩/名望 + 是否在野。</summary>
        public CareerView CareerView() => new CareerView(Session.Career.Career);

        /// <summary>战略大地图投影（城归属 + 势力 + 纪元；供 campaign map 表现层）。反全知：己方/名将/已探知势力露真名，余者未探明。</summary>
        public CampaignMapView MapView()
            => CampaignMapView.Build(Session.World, Contend, _scenario.PlayerFaction, CurrentYear, _scenario.AnchorYear, CurrentSeasonLabel,
                factionRevealed: f => HasScoutedEnemy && f == _scenario.PlayerTargetFaction);   // 派探敌情 → 发觉目标势力之将（GDD_026 R6，存读档一致）

        /// <summary>
        /// HUD 顶栏当前席位目标（真实反映所选开局，取代此前硬编码「汜水关太守」）：
        /// 太守开局（有宗主）→「{城}太守 · 奉{宗主}号令」；命名诸侯开局（无宗主）→ 用开局中文名（如「刘玄德·小沛」）。
        /// 一律附首要出征锋芒。城/宗主中文名经 <see cref="DisplayNames"/> 重标（Application 存的是稳定 id）。
        /// </summary>
        public string SeatObjective
        {
            get
            {
                var start = _scenario.Start;
                string target = DisplayNames.Of(start.OffensiveTarget.Value);
                string seat = start.Suzerain.HasValue
                    ? $"{DisplayNames.Of(start.Capital.Value)}太守 · 奉{DisplayNames.Of(start.Suzerain.Value.Value)}号令"
                    : start.DisplayName;   // 命名剧本 DisplayName 已是中文
                return $"{seat} · 守土图强，锋指{target}";
            }
        }

        /// <summary>顶栏聚合视图（UI 单一绑定对象）：纪元/一生/生涯/君命/行动容量/争霸 一次取全。</summary>
        public GameHudView HudSummary()
        {
            int rivals = 0;
            foreach (ThreeKingdom.Domain.Contention.PowerStanding p in Contend.Powers)
                if (p.Alive && p.Faction != _scenario.PlayerFaction) rivals++;
            return new GameHudView(
                CurrentYear, CurrentSeasonLabel, LifeView(), CareerView(), HasRebelled,
                CurrentMissionView().Order, ActionsInFlight, ActionCapacity,
                Contend.CitiesOf(_scenario.PlayerFaction), rivals, IsPlayerEliminated);
        }

        // --- 君主任务（GDD_014 / W5）：君主主动派讨伐/守土/献纳，完成累积功绩通往晋升。生成/评估确定性。---

        private readonly LordMissionService _missionService = new LordMissionService();
        private LordMission? _mission;
        private int _missionCount;
        private long _tributeDelivered;

        /// <summary>当前君主任务（无则种子化派发一道，按官阶+情势+当前年，确定性）。</summary>
        public LordMission CurrentMission()
        {
            if (_mission == null)
            {
                ulong seed = PersonaSeed(Session.Id) ^ 0xA55127EDUL
                    ^ ((ulong)(CurrentYear + 1) * 2654435761UL) ^ ((ulong)(_missionCount + 1) * 40503UL);
                _mission = _missionService.Generate(
                    Session.Career.Career.Rank, CurrentYear, _scenario.OffensiveTargetCities, EffectiveCapital,
                    seed, LordMissionConfig.Default);
            }
            return _mission;
        }

        /// <summary>当前君主任务展示视图（中文类型/目标/期限）。</summary>
        public LordMissionView CurrentMissionView() => new LordMissionView(CurrentMission(), CurrentYear);

        /// <summary>
        /// 评估当前任务进度并结算：完成 → 计生涯功绩（LordMissionComplete，通往晋升）并可接新任务；
        /// 失败（逾期/失守）→ 撤任务（名望罚待后续）。返回进度。
        /// </summary>
        public MissionProgress CheckMission()
        {
            LordMission m = CurrentMission();
            bool owns;
            switch (m.Type)
            {
                case MissionType.Subjugate:
                    owns = m.TargetCity.HasValue && Session.World.OwnershipOf(m.TargetCity.Value)?.Owner == _scenario.PlayerFaction;
                    break;
                case MissionType.Defend:
                    owns = Session.World.OwnershipOf(EffectiveCapital)?.Owner == _scenario.PlayerFaction;
                    break;
                default:
                    owns = false;
                    break;
            }
            MissionProgress p = _missionService.Evaluate(m, CurrentYear, new MissionContext(owns, _tributeDelivered));

            if (p == MissionProgress.Completed)
            {
                _service.ApplyCareerGain(Session, _scenario.Ladder, CareerGainSource.LordMissionComplete);
                ClearMission();
            }
            else if (p == MissionProgress.Failed)
            {
                _service.PenalizeRenown(Session, m.PenaltyRenown);   // 逾期/失守 → 损名望
                ClearMission();
            }
            return p;
        }

        private void ClearMission()
        {
            _mission = null;
            _missionCount++;
            _tributeDelivered = 0;
        }

        /// <summary>
        /// 献纳上缴（GDD_014 / W5）：当前为"献纳"任务时，从治所库存实扣所需军粮并记为已缴。库存不足则返回 false（未扣）。
        /// 缴足后 <see cref="CheckMission"/> 即判完成。非献纳任务返回 false。
        /// </summary>
        public bool PayLordTribute()
        {
            LordMission m = CurrentMission();
            if (m.Type != MissionType.Tribute) return false;
            long need = m.TributeGrain - _tributeDelivered;
            if (need <= 0) return true;
            if (!_service.LevyGrain(Session, need)) return false;   // 库存不足，未扣
            _tributeDelivered += need;
            return true;
        }

        /// <summary>
        /// 传承（GDD_026 R6）：寿终后由子嗣续局——同世界、同治所、生涯基业延续，新一世自<b>当前公元年</b>弱冠起、寿命另掷。
        /// 尚未寿终则抛（寿终方可传）。返回新一世视图。世界/生涯态不重置——只是执掌者换了一代人。
        /// </summary>
        public ArrivalLifeView SucceedHeir()
        {
            if (!IsLifeOver) throw new InvalidOperationException("尚未寿终，不能传承。");
            _generation++;
            _lifeStartYearOverride = CurrentYear;
            _life = null;   // 以新世代种子/起始年重掷一生
            return LifeView();
        }

        // --- 主角人设（GDD_015：开局随机人设，给天下事件"心里话"着色）。由会话 id 确定性派生 → 存读档一致，无需新存档字段。---

        /// <summary>本局主角人设（雄心/忠义/务实/谨慎；由会话 id 确定性生成，重开新局才变）。</summary>
        public ProtagonistPersona Persona => ProtagonistPersonas.Roll(PersonaSeed(Session.Id));

        /// <summary>主角人设展示视图（中文名 + 性情描述）。</summary>
        public PersonaView PersonaView() => new PersonaView(Persona);

        /// <summary>由会话 id 稳定散列出人设种子（FNV-1a 64；确定性、存读档一致）。</summary>
        private static ulong PersonaSeed(string id)
        {
            ulong h = 1469598103934665603UL;
            if (id != null) foreach (char c in id) { h ^= c; h *= 1099511628211UL; }
            return h;
        }

        // --- 人心杠杆施计（GDD_024）：攻城前对敌守将施离间/策反/攻心。反全知——须先侦察方能读弱点、准施计。---

        /// <summary>
        /// 对某敌城守将施一计（离间/策反/攻心）。反全知门：未侦察（无情报）则盲施大打折扣。
        /// 成功累积待生效效果（出征时削弱守备）；反噬则该城暴露、守将警觉。返回结果视图（无胜率）。
        /// </summary>
        public SubversionView AttemptSubversion(string cityId, SubversionScheme scheme, int intensityPercent = 100)
        {
            CampaignSession session = Session;
            var city = new CityId(cityId);
            bool scouted = session.HasIntel && session.PlayerKnowledge != null && session.PlayerKnowledge.Count > 0;
            FixedPoint quality = scouted ? FixedPoint.FromFraction(7, 10) : FixedPoint.Zero;
            bool exposed = session.IsSubversionExposed(city);
            FixedPoint intensity = FixedPoint.FromFraction(Math.Max(0, Math.Min(100, intensityPercent)), 100);

            // 目标为世界模型的真实守将（该城所属势力之君主）；无则回退合成守将。施计遂指向具名武将。
            FactionId? owner = session.World.OwnershipOf(city)?.Owner;
            CharacterId? lord = owner.HasValue ? session.World.FactionById(owner.Value)?.Lord : null;
            SubversionTargetProfile target = lord.HasValue
                ? SubversionTargetProfileFactory.Build(lord.Value, scouted, quality, exposed, PersonaSeed(session.Id))
                : SubversionTargetProfileFactory.Build(city, scouted, quality, exposed, PersonaSeed(session.Id));
            ulong seed = SubversionSeed(session, cityId, scheme);
            SubversionOutcome outcome = _service.AttemptSubversion(
                session, city, scheme, target, intensity, seed, SubversionConfig.Default);
            return new SubversionView(scheme, outcome);
        }

        /// <summary>某城当前是否已被施计（待生效效果存在，出征时会削弱守备）。</summary>
        public bool HasPendingSubversion(string cityId) => !Session.PendingSubversionFor(new CityId(cityId)).IsNone;

        private static ulong SubversionSeed(CampaignSession session, string cityId, SubversionScheme scheme)
        {
            ulong h = PersonaSeed(session.Id) ^ PersonaSeed(cityId);
            h ^= (ulong)(session.CurrentTime.AbsoluteIndex + 1) * 2654435761UL;
            h ^= (ulong)((int)scheme + 1) * 40503UL;
            h ^= (ulong)(session.SubversionAttemptsOn(new CityId(cityId)) + 1) * 2246822519UL;   // 每次尝试异种子
            return h;
        }

        // --- 军议/敌情屏（epic-028 story-003 / TR-ux-002/003）。只读投影 + 军议编排，反全知：UI 只经玩家知识投影。---

        /// <summary>会话是否启用情报/军议循环（军议与敌情屏可用性）。</summary>
        public bool HasIntel => Session.HasIntel;

        /// <summary>
        /// 召开军议（GDD_008）：经 <see cref="CampaignSessionService.ConveneCouncil"/> 读当前知识快照产出并列建议集，
        /// 绑定召开时快照并缓存；返回军议屏展示模型（小数置信经调节项映射为定性档，无成功率/唯一推荐）。
        /// </summary>
        public CampaignCouncilView ConveneCouncil()
        {
            _lastCouncil = _service.ConveneCouncil(Session);
            return CampaignCouncilView.FromSet(_lastCouncil, Session.CurrentKnowledgeSnapshotId!.Value, _councilTuning);
        }

        /// <summary>
        /// 取最近一次军议对<b>当前</b>知识快照的展示模型（不重开）；侦察改变知识后其 <c>IsStale</c> 变真（不静默重算）。
        /// 尚未召开过军议则返回 null。
        /// </summary>
        public CampaignCouncilView? CurrentCouncilView()
            => _lastCouncil == null
                ? null
                : CampaignCouncilView.FromSet(_lastCouncil, Session.CurrentKnowledgeSnapshotId!.Value, _councilTuning);

        /// <summary>
        /// 敌情面板展示模型（GDD_007）：从玩家阵营知识只读投影派生（结构上无真值，反全知）。
        /// 时效阈值取自场景 <see cref="IntelConfig.TtlSegments"/>（与情报评估同源，勿另立常量）。未启用情报时返回空面板。
        /// </summary>
        public CampaignEnemyIntelPanelView EnemyIntel()
        {
            if (!Session.HasIntel) return CampaignEnemyIntelPanelView.Empty;
            return CampaignEnemyIntelPanelView.FromProjection(
                Session.PlayerKnowledge!, Session.CurrentTime,
                _scenario.StartConfig.IntelConfig!.TtlSegments, Session.PendingScouts);
        }

        /// <summary>
        /// 派出侦察（GDD_007 派出→在途→返报，<b>非即时</b>）：记一支在途侦察兵，约 <see cref="PlayableCampaign.ScoutLeadSegments"/>
        /// 时段后返报——须「推进时段」到返报时刻，敌情数字才出现。返回命令结果（校验失败稳定错误码、零写入）。
        /// </summary>
        public CampaignCommandResult ScoutEnemy()
            => HasFreeAgent ? _service.DispatchScout(Session, PlayableCampaign.EnemyArmy, IntelSource.Scouting, _scenario.ScoutLeadSegments) : NoAgent();

        /// <summary>
        /// 是否已向当面之敌派探（在途侦察或已得敌情皆算）——<b>派生自持久化侦察态</b>，故存读档一致（GDD_026 R6 发觉门）。
        /// 供战略图反全知发觉目标势力之将。
        /// </summary>
        private bool HasScoutedEnemy
            => Session.PendingScouts.Count > 0 || (Session.PlayerKnowledge != null && Session.PlayerKnowledge.Entries.Count > 0);

        // --- 战役主循环（epic-028 story-004 / TR-ux-001/005 / ADR-0002/0009）。所有操作经服务命令，UI 只读投影。---

        /// <summary>当前回合数（1 起；用于新手引导前 N 回合判定，story-005）。</summary>
        public int Round => Session.CurrentTime.Day + 1;

        /// <summary>当前相位 + 该相位合法可做动作集（AC-5：任一相位都看得到下一步能做什么）。</summary>
        public HudPhaseView Phase() => HudPhaseView.ForSession(Session);

        /// <summary>治理面板（多维账本 + 三动作因果说明）。</summary>
        public GovernanceActionView Governance() => GovernanceActionView.FromSession(Session);

        // --- 行动容量节流（GDD_014 / 2026-07-05）：手下就那么几个人，同时能办的差事有限（随官阶增长）。---
        // 与"时长制"(行动需数周办成)互补：时长管"多久见效"，容量管"同时能办几件"——非体力点数（避免双重节流）。

        private const int BaseActionSlots = 2;
        /// <summary>可同时在办的差事上限（基数 + 官阶——升官则手下更多，给往上爬的实在理由）。</summary>
        public int ActionCapacity => BaseActionSlots + (int)Session.Career.Career.Rank;
        /// <summary>当前在办差事数（在途侦察 + 在办治理）。</summary>
        public int ActionsInFlight => Session.PendingScouts.Count + Session.PendingGovernance.Count;
        /// <summary>是否还有空闲人手可遣（未满容量）。</summary>
        public bool HasFreeAgent => ActionsInFlight < ActionCapacity;

        private CampaignCommandResult NoAgent()
            => CampaignCommandResult.Failure(CampaignErrorCode.NoFreeAgent,
                $"手下都在办事——同时最多 {ActionCapacity} 件（随官阶增），须待一事办完再遣。");

        /// <summary>下令征用军粮（GDD_004 派人处理→需时见效）：校验后记为在办，约 2 周后见效；超可分配量稳定错误码。占一名人手。</summary>
        public CampaignCommandResult Requisition(long amount)
            => HasFreeAgent ? _service.DispatchRequisition(Session, amount, _scenario.RequisitionLeadSegments) : NoAgent();

        /// <summary>下令修工事（GDD_004）：工事已满稳定错误码，否则记为在办，约 3 周后见效。占一名人手。</summary>
        public CampaignCommandResult Repair()
            => HasFreeAgent ? _service.DispatchRepair(Session, _scenario.RepairLeadSegments) : NoAgent();

        /// <summary>下令安抚民心（GDD_004）：记为在办，约 1 周后见效。占一名人手。</summary>
        public CampaignCommandResult Appease()
            => HasFreeAgent ? _service.DispatchAppease(Session, _scenario.AppeaseLeadSegments) : NoAgent();

        /// <summary>备战面板（草稿 vs 已提交视觉区分）。</summary>
        public PrepPanelView Prep() => PrepPanelView.FromSession(Session);

        /// <summary>加入一条设伏草稿命令（GDD_009；只改草稿不改权威态）。</summary>
        public CampaignCommandResult AddAmbushOrder() => _service.AddPlanOrder(Session, _scenario.AmbushPlan());

        /// <summary>移除一条草稿命令（只改草稿）。</summary>
        public CampaignCommandResult RemoveOrder(string orderId) => _service.RemovePlanOrder(Session, new OrderId(orderId));

        /// <summary>提交计划（原子承诺，不可反悔）；返回是否成功。</summary>
        public bool SubmitPlan() => _service.SubmitPlan(Session).Committed;

        /// <summary>兵法条件进度（战中相位显示；每链已满足/未满足 + 还差 N 条，非按钮）。</summary>
        public BattleConditionProgressView BattleConditionProgress()
            => BattleConditionProgressView.Build(_scenario.TacticChains, Session.BattleConditions);

        /// <summary>假退伏击三条件（脚本战斗满足；与 console harness 同源，兵法=条件组合非按钮）。</summary>
        private static readonly TacticCondition[] FeintAmbushConditions =
        {
            TacticCondition.ControlledRetreatKeptFormation,
            TacticCondition.EnemyPursued,
            TacticCondition.AmbushSurprise,
        };

        /// <summary>
        /// 开战（story-004 复用既有脚本战斗，替换 story-002 演示按钮）：以<b>已提交计划</b>为可执行初始条件，
        /// 建立战斗 + 解析一阶段 + 标记假退伏击条件（进入战中相位，条件进度可见）。确定性种子/夹具。
        /// 无已提交计划 → 稳定错误码；幂等：已开战则跳过。
        /// </summary>
        public CampaignCommandResult StartBattle()
        {
            if (Session.CommittedPlan == null)
                return CampaignCommandResult.Failure(CampaignErrorCode.PreparationDisabled, "开战需先提交备战计划。");
            if (!Session.HasBattle)
            {
                CampaignCommandResult started = _service.StartBattle(
                    Session, _scenario.Units(), _scenario.BattleConfig, _scenario.BattleSeed, _scenario.TacticChains);
                if (!started.Applied) return started;
                _service.ResolveBattlePhase(Session, ScriptedOrders());
                foreach (TacticCondition c in FeintAmbushConditions) _service.MarkTacticCondition(Session, c);
            }
            return CampaignCommandResult.Success();
        }

        /// <summary>
        /// 结算战果（story-004）：识别涌现兵法 + 结算胜局后果（原子写回）+ 构造复盘展示模型（进入战后相位）。
        /// 要求已开战。战役继续——败/撤退亦有续局（本脚本战胜局）。
        /// </summary>
        public BattleReviewView ResolveOutcome()
        {
            if (!Session.HasBattle) throw new InvalidOperationException("尚未开战，无战果可结算。");
            IReadOnlyList<RecognizedTactic> tactics = _service.RecognizeTactics(Session);
            var context = new OutcomeContext(_scenario.PlayerFaction, EffectiveCapital);
            OutcomeContinuation continuation = _service.ResolveBattleOutcome(
                Session, OutcomeBranch.Victory, context, _scenario.OutcomeConfig);
            CareerGain? gain = _scenario.Ladder.GainFor(CareerGainSource.CombatVictory);
            CareerState career = Session.Career.Career;
            return BattleReviewView.From(
                OutcomeBranch.Victory, continuation.Consequences.Changes, tactics, continuation.Options,
                gain, career.Merit, career.Renown, BattleReviewTuning.Default);
        }

        private static BattleOrder[] ScriptedOrders() => new[]
        {
            new BattleOrder(0, PlayableCampaign.PlayerUnit, BattleOrderType.Engage, targetUnit: PlayableCampaign.EnemyUnit),
        };

        // --- 出征攻城入口（GDD_019 v2 / ADR-0010/0011）：选目标 + 授权门 + 六维组装 + 发起 + 占城归属。---
        // 出征准备草稿为发起前临时态（ADR-0011 D7），不入存档；UI 只经此接口，权威结算在 CampaignSessionService。

        private readonly OffensiveSetupService _offensiveDerive = new OffensiveSetupService();

        /// <summary>当前出征计划草稿（null=尚未选目标开始组装）。</summary>
        public OffensivePlan? CurrentOffensivePlan => _offensivePlan;
        private OffensivePlan? _offensivePlan;

        /// <summary>可选副将花名册（GDD_014 僚属；供 UI 挑选加为副将）。</summary>
        public IReadOnlyList<OffensiveGeneral> DeputyRoster => _scenario.DeputyRoster;

        /// <summary>请君主授权出征（GDD_019 R1）：把场景可攻目标登记为授权集（受命后目标门转 Authorized）。</summary>
        public void RequestOffensiveAuthorization()
            => _service.AuthorizeOffensive(Session, _scenario.OffensiveTargetCities);

        /// <summary>
        /// 出征选目标视图（GDD_019 §7 / R1/R2）：列场景目标城 + 各自授权门（反全知只读控制权投影）。
        /// 不可攻的也列出并说明原因（AC-5）。
        /// </summary>
        public OffensiveTargetsView OffensiveTargets()
        {
            var lines = new List<OffensiveTargetLine>();
            bool authorized = false;
            foreach (CityId city in _scenario.OffensiveTargetCities)
            {
                OffensiveGateResult gate = _service.CheckOffensiveTarget(Session, city, _scenario.PlayerFaction);
                if (gate != OffensiveGateResult.NotAuthorized) authorized = true;
                lines.Add(new OffensiveTargetLine(city.Value, DisplayNames.Of(city.Value), gate));
            }
            // authorized 判据：授权集非空（任一目标不再是 NotAuthorized）。
            authorized = Session.OffensiveAuthorization.AuthorizedTargets.Count > 0;
            return new OffensiveTargetsView(lines, authorized);
        }

        /// <summary>开始组装出征计划（GDD_019 §4a）：以场景默认建草稿（主将=太守亲征、正面强攻、当前时段）。返回草稿供 UI 修改六维。</summary>
        public OffensivePlan BeginOffensive(CityId target)
        {
            _offensivePlan = new OffensivePlan(
                target, _scenario.LeadGeneral, defaultMuster: 400, defaultSupply: 200, segment: Session.CurrentTime.Segment);
            return _offensivePlan;
        }

        /// <summary>以场景首个可攻目标开始组装（Unity 壳便捷入口，等价 BeginOffensive(首目标)）。</summary>
        public OffensivePlan BeginOffensiveDefault() => BeginOffensive(_scenario.OffensiveTargetCities[0]);

        /// <summary>当前草稿的计划预览（GDD_019 R3 闭合因果可见性）：dry-run 派生战力/士气/成型条件 + 缺失提示，无胜率。未开始则抛。</summary>
        public OffensivePlanView PreviewOffensive()
        {
            if (_offensivePlan == null) throw new InvalidOperationException("尚未开始组装出征（先 BeginOffensive）。");
            bool scouted = TargetScouted();
            OffensivePreparation prep = _offensivePlan.Build(_scenario.TerrainOf(_offensivePlan.Target), scouted);
            OffensiveForce preview = _offensiveDerive.Derive(prep, _scenario.OffensiveSetup);
            return OffensivePlanView.FromPlan(_offensivePlan, preview, scouted);
        }

        private ZoneBattleRuntime? _offensiveBattle;
        private CityId _offensiveTarget;

        /// <summary>
        /// 发起出征（GDD_019 + GDD_021 端到端）：授权门通过 → <b>进入区域战斗</b>（多回合排兵布阵，替换一击结算）；
        /// 被门拒则即时返回拒绝。战斗由 <see cref="OffensiveBattleResolveRound"/> 等推进，终局后经 <see cref="ConcludeOffensive"/>
        /// 结算占城归属 C。未开始组装则抛。
        /// </summary>
        public OffensiveResultView LaunchOffensive()
        {
            if (_offensivePlan == null) throw new InvalidOperationException("尚未开始组装出征（先 BeginOffensive）。");
            CityId target = _offensivePlan.Target;
            OffensiveGateResult gate = _service.CheckOffensiveTarget(Session, target, _scenario.PlayerFaction);
            if (gate != OffensiveGateResult.Authorized)
                return OffensiveResultView.FromResult(OffensiveResult.Rejected(gate));

            // 外交战略约束门（GDD M11）：目标属盟/互不侵犯势力 → 须先背约方可攻。
            FactionId? owner = _scenario.DefendingFactionOf(target);
            if (owner != null)
            {
                WarConstraint wc = _diplomacyService.CheckWarTarget(_diplomacy, owner.Value, StrategicDiplomacyConfig.Default);
                if (wc.RequiresBreach)
                    return OffensiveResultView.Blocked($"与「{DisplayNames.Of(owner.Value.Value)}」有盟约/互不侵犯，不可径攻。");
            }

            bool scouted = TargetScouted();
            OffensivePreparation prep = _offensivePlan.Build(_scenario.TerrainOf(target), scouted);
            FixedPoint morale = _offensiveDerive.Derive(prep, _scenario.OffensiveSetup).Morale;
            int garrison = _scenario.DefenseOf(target).Garrison;
            // 守将进战斗（GDD_027 #3）：目标城武将册（应用演义覆盖层）→ 守方分区布防择位。
            var loreOv = ThreeKingdom.Application.Scenarios.LoreEvents.OverridesAt(
                new ThreeKingdom.Application.Scenarios.LoreContext(_scenario.AnchorYear, CurrentYear, _scenario.PlayerFaction));
            var defenders = PlayableCampaign.DefendersFor(target, _scenario.AnchorYear, loreOv);
            _offensiveBattle = ZoneBattleRuntime.FromOffensive(prep, morale, garrison, _scenario.OffensiveSeed, defenders: defenders);
            _offensiveTarget = target;
            return OffensiveResultView.Started();
        }

        /// <summary>出征区域战斗进行中（未分胜负）。</summary>
        public bool HasOffensiveBattle => _offensiveBattle != null && !_offensiveBattle.IsOver;
        /// <summary>出征区域战斗已分胜负（待 ConcludeOffensive 结算后果）。</summary>
        public bool OffensiveBattleOver => _offensiveBattle != null && _offensiveBattle.IsOver;

        /// <summary>出征战斗当前投影（各区态势 + 涌现 + 排兵布阵选项）。未发起则抛。</summary>
        public ZoneBattleView OffensiveBattleView() => Battle().View();
        /// <summary>战中调动己方支队到相邻区（排兵布阵）。</summary>
        public ZoneCommandResult OffensiveBattleMove(string detachmentId, string zoneId) => Battle().MoveDetachment(detachmentId, zoneId);
        /// <summary>战中改己方支队姿态。</summary>
        public ZoneCommandResult OffensiveBattleSetPosture(string detachmentId, Posture posture) => Battle().SetPosture(detachmentId, posture);
        /// <summary>推进出征战斗一回合（敌AI + 结算），返回战后投影。</summary>
        public ZoneBattleView OffensiveBattleResolveRound() => Battle().ResolveRound();
        /// <summary>挂 AI 代打出征至终局（不结算，供场景展示后再由玩家点结算），返回终局投影。</summary>
        public ZoneBattleView OffensiveBattleAutoResolve() => Battle().AutoResolve();

        private ZoneBattleRuntime Battle() => _offensiveBattle ?? throw new InvalidOperationException("尚未发起出征战斗。");

        /// <summary>
        /// 战斗终局后结算出征后果（权威）：破城 → 占城归属 C（经 <see cref="CampaignSessionService.ResolveConquest"/>：
        /// 控制权变更 + 记功 + 自立倾向）；败/超时 → 退兵可继续。清空战斗与草稿。战斗未结束则抛。
        /// </summary>
        public OffensiveResultView ConcludeOffensive()
        {
            if (_offensiveBattle == null || !_offensiveBattle.IsOver)
                throw new InvalidOperationException("战斗尚未结束，不能结算出征后果。");

            OffensiveResultView view;
            if (_offensiveBattle.Outcome == ZoneBattleOutcome.AttackerVictory)
            {
                ConquestResult conquest = _service.ResolveConquest(
                    Session, _offensiveTarget, _scenario.ConqueredGarrison, _scenario.PlayerFaction, PlayableCampaign.LordFaction,
                    FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero,
                    _scenario.OffensiveSeed, _scenario.Occupation, _scenario.Ladder, CareerGainSource.MajorBattleVictory);
                if (conquest.Verdict == OwnershipVerdict.GrantToPlayer)   // 归玩家直辖 → 入多城战区（M12）
                {
                    _theater = _theaterService.HoldConqueredCity(_theater, _offensiveTarget);
                    // 争霸领土（M13）：玩家 +1，被夺方 −1（经服务编排，持久化到会话）。
                    FactionId? loser = _scenario.DefendingFactionOf(_offensiveTarget);
                    _service.RecordPlayerConquest(Session, Contend, _scenario.PlayerFaction, loser);
                }
                view = OffensiveResultView.Victorious(conquest);
            }
            else
            {
                view = OffensiveResultView.Defeated();
            }

            _offensiveBattle = null;
            _offensivePlan = null;
            return view;
        }

        /// <summary>挂 AI 代打出征至终局并结算后果（玩家可选亲自打或代打；代打不保证赢，胜负由六维准备/对阵定）。</summary>
        public OffensiveResultView AutoResolveOffensive()
        {
            Battle().AutoResolve();
            return ConcludeOffensive();
        }

        // --- 守城区域防御战（GDD_021 R7 攻守统一：玩家=守方，攻方=敌AI）。替换脚本守城的可玩战斗。---

        private ZoneBattleRuntime? _defenseBattle;

        /// <summary>守城区域防御战进行中。</summary>
        public bool HasDefenseBattle => _defenseBattle != null && !_defenseBattle.IsOver;
        /// <summary>守城已分胜负。</summary>
        public bool DefenseBattleOver => _defenseBattle != null && _defenseBattle.IsOver;
        /// <summary>守城是否守住（守方胜=退敌）。</summary>
        public bool DefenseHeld => _defenseBattle != null && _defenseBattle.IsOver
            && _defenseBattle.Outcome == ZoneBattleOutcome.DefenderVictory;

        /// <summary>发起守城区域防御战：以守军分区布防，敌军来攻（敌AI驱动攻方）。返回初始战斗投影。</summary>
        /// <summary>上一场守城战，玩家守将是否被敌方策反/攻心（GDD_024 §13 对称威胁的预警）。</summary>
        public bool WasDefenseSubverted { get; private set; }

        public ZoneBattleView StartDefenseBattle()
        {
            var field = BattleField.Default();
            var planner = new OffensiveDeploymentPlanner();
            FixedPoint morale = FixedPoint.FromFraction(7, 10);

            // 敌方对玩家守城施人心杠杆（GDD_024 §13 对称）：种子化攻心，成功则挫玩家守方士气 + 预警（非无解，仅削弱）。
            SubversionTargetProfile ownGuard = SubversionTargetProfileFactory.Build(
                new CityId("player-defense"), scouted: true, FixedPoint.FromFraction(6, 10), false, PersonaSeed(Session.Id));
            SubversionOutcome enemyPlot = new SubversionService().Resolve(
                SubversionScheme.UnderminedMorale, ownGuard, FixedPoint.FromFraction(5, 10), 0,
                _scenario.OffensiveSeed ^ 0x5EED_D00Dul, SubversionConfig.Default);
            SubversionEffect enemyEffect = enemyPlot.Result == SubversionResult.Success ? enemyPlot.Effect : SubversionEffect.None;
            WasDefenseSubverted = enemyPlot.Result == SubversionResult.Success;

            var dets = new List<Detachment>(planner.PlanDefender(
                new SiegeDefense(_scenario.DefenseGarrison, FixedPoint.FromFraction(12, 10)), morale, field, enemyEffect));
            dets.Add(new Detachment(new DetachmentId("enemy-assault"), BattleSide.Attacker, null,
                TroopComposition.AllInfantry(_scenario.EnemyAssaultForce), _scenario.EnemyAssaultForce,
                morale, FixedPoint.FromFraction(2, 10), Posture.Assault, BattleField.Front));
            ZoneBattleState start = new ZoneBattleService().Start(field, dets, BattleSide.Defender, 6, _scenario.OffensiveSeed);
            _defenseBattle = new ZoneBattleRuntime(start, ZoneBattleContext.Default);
            return _defenseBattle.View();
        }

        /// <summary>守城战当前投影。</summary>
        public ZoneBattleView DefenseBattleView() => Defense().View();
        /// <summary>守城战中调动己方守军到相邻区。</summary>
        public ZoneCommandResult DefenseBattleMove(string detachmentId, string zoneId) => Defense().MoveDetachment(detachmentId, zoneId);
        /// <summary>守城战中改己方守军姿态。</summary>
        public ZoneCommandResult DefenseBattleSetPosture(string detachmentId, Posture posture) => Defense().SetPosture(detachmentId, posture);
        /// <summary>推进守城战一回合（敌AI + 结算），返回战后投影。</summary>
        public ZoneBattleView DefenseBattleResolveRound() => Defense().ResolveRound();
        /// <summary>挂 AI 代打守城至终局（不结算），返回终局投影。</summary>
        public ZoneBattleView DefenseBattleAutoResolve() => Defense().AutoResolve();

        /// <summary>挂 AI 代打守城至终局；返回是否守住（代打不保证守成，胜负由守备/对阵定）。</summary>
        public bool AutoResolveDefense()
        {
            Defense().AutoResolve();
            return DefenseHeld;
        }

        private ZoneBattleRuntime Defense() => _defenseBattle ?? throw new InvalidOperationException("尚未发起守城战。");

        // --- 君主争霸 + 统一终局（GDD_017/018 / epic-026/027）：群雄争霸态 + 对手扩张 + 终局判定 ---

        private readonly ThreeKingdom.Domain.Contention.EndgameService _endgameService = new ThreeKingdom.Domain.Contention.EndgameService();
        // 争霸态持久于会话（存读档一致）；未初始化则按场景初值。
        private ThreeKingdom.Domain.Contention.ContentionState Contend => Session.Contention ?? _scenario.InitialContention();

        /// <summary>当前群雄争霸态（各势力领城/存续）。</summary>
        public ThreeKingdom.Domain.Contention.ContentionState Contention => Contend;

        /// <summary>当前终局状态（继续/统一/覆灭）。</summary>
        public ThreeKingdom.Domain.Contention.EndgameStatus Endgame()
            => _endgameService.Evaluate(Contend, _scenario.PlayerFaction, ThreeKingdom.Domain.Contention.EndgameConfig.Default);

        // --- 覆灭之后（GDD_026 补）：势力被灭≠game over。被俘→判生死→归顺?→释放?→投奔他主收留?；唯身死才终。 ---

        private DefeatFlow? _defeat;
        private bool _rebelled;

        /// <summary>玩家势力是否已覆灭（领城归零）。</summary>
        public bool IsPlayerEliminated => Endgame() == ThreeKingdom.Domain.Contention.EndgameStatus.PlayerEliminated;

        /// <summary>玩家是否已自立（叛主）——若为真，日后被灭则必被俘处死（无归顺/投奔活路，GDD_026 补）。</summary>
        public bool HasRebelled => _rebelled;

        /// <summary>
        /// 发起自立（GDD_014）：资格达成则转独立新势力（叛主）。<b>红线：自立后若被灭，必被俘处死</b>——
        /// 故此步一旦成功即锁死"无退路"（<see cref="_rebelled"/>）。资格不足则不切死局（稳定返回未发起）。
        /// </summary>
        public RebellionResult DeclareIndependence()
        {
            var ctx = new RebellionContext(
                citiesOwned: Contend.CitiesOf(_scenario.PlayerFaction),
                supplyReady: true, troopsReady: true,
                lordOppression: Session.RebellionLean > 0,
                newFactionId: PlayableCampaign.RebelFaction);
            RebellionResult r = _service.LaunchRebellion(Session, _scenario.Rebellion, ctx);
            if (r.Launched) _rebelled = true;
            return r;
        }

        /// <summary>
        /// 进入被灭处境流程（GDD_026 补）：以当下最强之敌为擒获者、以生涯名声为调制、会话种子确定性。
        /// 返回状态机——由 UI 驱动 <see cref="DefeatFlow.ResolveCaptorFate"/> / 归顺 / 不归顺 / 投奔。尚未覆灭则抛。
        /// 注：复位为新主太守的<b>活世界续局</b>（保当前公元年/一生接续）为后续接线，本入口先给出决策流程与结局。
        /// </summary>
        public DefeatFlow BeginDefeat()
        {
            if (!IsPlayerEliminated) throw new InvalidOperationException("尚未覆灭，无被俘流程。");
            return _defeat ??= new DefeatFlow(
                StrongestRival(), Session.Career.Career.Renown,
                PersonaSeed(Session.Id) ^ 0x0DEFEA7EUL, CaptivityConfig.Default, rebelled: _rebelled);
        }

        /// <summary>当下最强的存续对手（覆灭时的擒获者；无对手则回退第一个已知势力）。</summary>
        private FactionId StrongestRival()
        {
            FactionId? best = null;
            int bestCities = -1;
            foreach (ThreeKingdom.Domain.Contention.PowerStanding p in Contend.Powers)
            {
                if (p.Faction == _scenario.PlayerFaction || !p.Alive) continue;
                if (p.Cities > bestCities) { bestCities = p.Cities; best = p.Faction; }
            }
            return best ?? _scenario.PlayerTargetFaction;
        }

        // 复位后新治所（活世界续局）：覆盖场景静态治所，供出征战果 OutcomeContext 等取当前所治之城。
        private CityId? _capitalOverride;
        private CityId EffectiveCapital => _capitalOverride ?? _scenario.PlayerCapital;

        /// <summary>复位后可效力的新主（归顺/投奔所定）；未复位则 null。</summary>
        public FactionId? CurrentSuzerain => _defeat?.NewLord;

        /// <summary>
        /// 东山再起·活世界续局（GDD_026 补）：势力被灭后经 <see cref="BeginDefeat"/> 流程走到可续（归顺/被收留）→
        /// 在<b>同一世界、当前公元年、这一生</b>里由新主割一城复位为太守。须先驱动流程至 <see cref="DefeatFlow.CanPlayOn"/>。
        /// 新主已无城可授（罕见）→ 返回 false（保持流亡，可另投）。成功返回 true，覆灭解除、天下照旧流转。
        /// </summary>
        public bool ContinueUnderNewLord()
        {
            if (!IsPlayerEliminated) return false;
            DefeatFlow flow = BeginDefeat();
            if (!flow.CanPlayOn || flow.NewLord == null) return false;

            FactionId lord = flow.NewLord.Value;
            CityId? grant = PickGrantCity(lord);
            if (grant == null) return false;   // 新主无城可授 → 复位失败（仍流亡）

            var economy = new CityEconomyState(grant.Value, stock: 80, reserved: 0, civMorale: 55,
                security: 45, fortificationCurrent: 15, fortificationMax: 100);
            _service.ReseatGovernor(Session, Contend, _scenario.PlayerFaction, lord, grant.Value, new Garrison(400), economy);
            _capitalOverride = grant;
            return true;
        }

        /// <summary>取新主一座可授的城（非君主治所、当前确属该主）；无则 null。</summary>
        private CityId? PickGrantCity(FactionId lord)
        {
            foreach (CityId c in PlayableCampaign.SelectableGovernorCities())
                if (Session.World.OwnershipOf(c)?.Owner == lord) return c;
            return null;
        }

        private int _contentionSteps;

        /// <summary>推进一战略步（对手种子化兼并——强吞弱）。每步种子相异，确定性、可复现。</summary>
        public void AdvanceContention()
        {
            ulong seed = new StateHasher()
                .Append(_scenario.OffensiveSeed).Append(Session.CurrentTime.AbsoluteIndex).Append(_contentionSteps++)
                .ToHash().Value;
            _service.StepRivalContention(Session, Contend, _scenario.PlayerFaction, seed, _scenario.ContentionConfig);
        }

        // --- 战略外交（GDD M11 / epic-024）：外交立场约束战争；缔约；背约代价 ---

        private readonly StrategicDiplomacyService _diplomacyService = new StrategicDiplomacyService();
        private DiplomaticStanceState _diplomacy = DiplomaticStanceState.Empty;

        /// <summary>当前外交立场态。</summary>
        public DiplomaticStanceState Diplomacy => _diplomacy;

        /// <summary>外交态一览视图（各存续势力立场中文 + 可否径攻）。</summary>
        public DiplomacyView DiplomacyView()
            => ThreeKingdom.Presentation.Screens.DiplomacyView.Build(_diplomacy, Contend, _scenario.PlayerFaction);

        /// <summary>攻打某势力的战略约束（盟约/互不侵犯须背约）。</summary>
        public WarConstraint CheckDiplomaticWarTarget(FactionId power)
            => _diplomacyService.CheckWarTarget(_diplomacy, power, StrategicDiplomacyConfig.Default);

        /// <summary>提议缔约（互不侵犯/盟约）：条件+种子判定；成则立约。返回结果。</summary>
        public PactResult ProposePact(FactionId power, DiplomaticStance target, PactFactors factors)
        {
            ulong seed = ComposeDiploSeed(power, target);
            PactResult r = _diplomacyService.ProposePact(_diplomacy, power, target, factors, seed, StrategicDiplomacyConfig.Default);
            if (r.Accepted) _diplomacy = r.State;
            return r;
        }

        /// <summary>背约（对盟/邻）：被背方转敌对 + 声誉惩罚（写回生涯名望，简化为记录）。返回背约结果。</summary>
        public BreachResult BreachPact(FactionId power)
        {
            BreachResult r = _diplomacyService.Breach(_diplomacy, power, StrategicDiplomacyConfig.Default);
            _diplomacy = r.State;
            return r;
        }

        private ulong ComposeDiploSeed(FactionId power, DiplomaticStance target)
        {
            var h = new StateHasher();
            h.Append(_scenario.OffensiveSeed).Append(Session.CurrentTime.AbsoluteIndex);
            foreach (char c in power.Value) h.Append((int)c);
            h.Append((int)target);
            return h.ToHash().Value;
        }

        // --- 多城战区（GDD_022 / M12）：占城 C 归玩家的城入战区；委任下属打理；掌管范围随官阶；反全知报告 ---

        private readonly TheaterService _theaterService = new TheaterService();
        private TheaterState _theater = TheaterState.Empty;

        /// <summary>当前多城战区态（直辖城 + 委任）。</summary>
        public TheaterState Theater => _theater;

        /// <summary>委任某直辖城给下属打理（须已持有）。</summary>
        public TheaterCommandResult DelegateCity(ThreeKingdom.Domain.City.CityId city, ThreeKingdom.Domain.Characters.CharacterId governor)
        {
            TheaterCommandResult r = _theaterService.Delegate(_theater, city, governor);
            if (r.Applied) _theater = r.State;
            return r;
        }

        /// <summary>收回某城亲管（受官阶亲管范围约束——取玩家当前官阶）。</summary>
        public TheaterCommandResult SelfGovernCity(ThreeKingdom.Domain.City.CityId city)
        {
            int rank = (int)Session.Career.Career.Rank;
            TheaterCommandResult r = _theaterService.SelfGovern(_theater, city, rank, SpanOfControlConfig.Default);
            if (r.Applied) _theater = r.State;
            return r;
        }

        /// <summary>战区报告（亲管城即时、委任城下属汇报·反全知）。</summary>
        public IReadOnlyList<TheaterCityReport> TheaterReports(TheaterResources reported)
            => new TheaterReportService().Build(_theater, reported);

        // --- 人才招揽（GDD_020）：出现随历史 · 知晓靠情报（反全知）· 入伙靠条件+种子判定 · 喂给战斗/生涯 ---

        private readonly TalentService _talentService = new TalentService();
        private ThreeKingdom.Domain.Talent.TalentState _talent = ThreeKingdom.Domain.Talent.TalentState.Empty;

        /// <summary>玩家可见人才（已登场 ∩ 已知晓；反全知，未知晓者不入）。</summary>
        public IReadOnlyList<ThreeKingdom.Domain.Talent.TalentProfile> VisibleTalents()
            => _talentService.Visible(_scenario.TalentRoster, _talent, Session.CurrentTime);

        /// <summary>可招人才录（反全知无数值：名/专长/招揽难度定性档）。</summary>
        public TalentRecruitView TalentView() => TalentRecruitView.From(VisibleTalents());

        /// <summary>经渠道知晓某人才（侦察/军师/部曲人脉/历史事件）→ 进入视野。</summary>
        public void RevealTalent(ThreeKingdom.Domain.Talent.TalentId id, ThreeKingdom.Domain.Talent.TalentChannel channel)
            => _talent = _talentService.Reveal(_talent, id, channel);

        /// <summary>发起招揽（须已登场+已知晓）：条件+种子判定出仕与否；出仕则入伙（返回为将）。返回尝试结果。</summary>
        public TalentRecruitAttempt RecruitTalent(ThreeKingdom.Domain.Talent.TalentId id, ThreeKingdom.Domain.Talent.RecruitmentOffer offer)
        {
            TalentRecruitAttempt r = _talentService.AttemptRecruit(
                _scenario.TalentRoster, _talent, id, Session.CurrentTime, offer,
                _scenario.TalentSeed, _scenario.PlayerFaction, _scenario.TalentRecruit);
            if (r.Valid) _talent = r.State;
            return r;
        }

        /// <summary>已入伙某人才。</summary>
        public bool HasRecruited(ThreeKingdom.Domain.Talent.TalentId id) => _talent.IsRecruited(id);

        /// <summary>目标是否已侦察（反全知：有非过时敌情估计 → 可得突袭类条件、免情报盲区折扣）。</summary>
        private bool TargetScouted()
        {
            if (!Session.HasIntel) return false;
            foreach (CampaignEnemyIntelView e in EnemyIntel().Entries)
                if (!e.IsStale) return true;
            return false;
        }

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public bool HasSave() => _medium.Exists(_slot);

        /// <summary>
        /// 原子存档当前会话到槽（统一信封 <see cref="CampaignSessionService.CaptureSnapshot"/>）；
        /// 先写临时槽再原子改名——任一步失败返回 false 且正式槽保留上一份有效存档（ADR-0005 guardrail）。
        /// </summary>
        public bool Save()
        {
            string content = _service.CaptureSnapshot(Session);
            string tmp = _slot + ".tmp";
            try
            {
                _medium.Write(tmp, content);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            try
            {
                _medium.Move(tmp, _slot);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取槽恢复会话（统一信封 <see cref="CampaignSessionService.Restore"/>，卫星配置由场景源提供——数据驱动）。
        /// 成功切换当前会话返回 true；失败（无存档 / 版本、指纹、格式不符）返回 false 与原因，<b>当前会话不变</b>（不部分载入）。
        /// </summary>
        public bool Load(out string reason)
        {
            string? text = _medium.Read(_slot);
            if (text == null)
            {
                reason = "槽内无存档。";
                return false;
            }

            CampaignStartConfig config = _scenario.StartConfig;
            try
            {
                CampaignSession restored = _service.Restore(
                    text, config.Fingerprint,
                    settlementConfig: config.SettlementConfig,
                    governanceConfig: config.GovernanceConfig,
                    populationPressure: config.PopulationPressure,
                    intelConfig: config.IntelConfig,
                    councilSetup: config.CouncilSetup,
                    prepConfig: config.PreparationConfig,
                    reachableRegions: config.ReachableRegions,
                    authorizedOrders: config.AuthorizedOrders,
                    battleConfig: _scenario.BattleConfig,
                    tacticChains: _scenario.TacticChains);
                _session = restored;
                _daysCrossedLastAdvance = 0;
                reason = string.Empty;
                return true;
            }
            catch (SaveFormatException ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private CampaignSession StartNew()
        {
            CampaignStartResult result = _service.StartCampaign(_scenario.StartConfig);
            if (!result.Started)
                throw new InvalidOperationException("场景开局失败（配置源已验证，此处失败属编程错误）：" + result.Error + " " + result.Detail);
            return result.Session!;
        }

        private void TryDelete(string tmp)
        {
            try { _medium.Delete(tmp); } catch { /* 清理失败不掩盖原始错误 */ }
        }
    }
}
