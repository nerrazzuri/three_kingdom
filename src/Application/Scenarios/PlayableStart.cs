using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 一个<b>可选开局</b>（#1 多剧本/势力选择，ADR-0009 装配边界）：描述"玩家扮演谁、从哪座城起家、初期锋芒指向何处"。
    /// 天下大盘（17 席世界骨架 + 城归属）由 <see cref="PlayableCampaign"/> 共用，不随开局变——变的只是<b>玩家席位</b>：
    /// 所属势力 / 君主 / 治所 / 首要出征目标。玩家可扮世界既有诸侯（吞并其席为 Self），亦可为汜水关太守（第 18 独立席）。
    /// 不可变值对象；运行期身份（<see cref="CampaignRuntime"/>）一律读此，不再硬编码单一场景。
    /// </summary>
    public sealed class PlayableStart
    {
        /// <summary>稳定开局 id（存档/选择键；如 "fanshui-governor"）。</summary>
        public string Id { get; }
        /// <summary>锚点年（公元；GDD_026：世界快照与纪元起点。当前诸开局皆 190 讨董之世）。</summary>
        public int AnchorYear { get; }
        /// <summary>中文开局名（选择屏显示，如「汜水关太守」）。</summary>
        public string DisplayName { get; }
        /// <summary>一句话开局情境（选择屏副文案）。</summary>
        public string Blurb { get; }

        /// <summary>玩家所属势力。</summary>
        public FactionId PlayerFaction { get; }
        /// <summary>玩家君主/本人。</summary>
        public CharacterId PlayerLord { get; }
        /// <summary>起家治所（内政/守城之城）。</summary>
        public CityId Capital { get; }
        /// <summary>治所守军。</summary>
        public int CapitalGarrison { get; }

        /// <summary>首要出征目标城（初盘锋芒；授权门/敌控由运行期按控制权投影判定）。</summary>
        public CityId OffensiveTarget { get; }
        /// <summary>出征目标所属势力（外交立场/破城归属承接判定）。</summary>
        public FactionId TargetFaction { get; }

        /// <summary>出征目标城地形（#3 逐城/地形战场：决定战场正面区——隘口设伏/渡口水火/坚城诈降/平原骑冲）。</summary>
        public TerrainKind TargetTerrain { get; }

        /// <summary>是否含汜水关太守专属独立席（第 18 席）：仅默认剧本为真；扮演既有诸侯时为假（占其席为 Self）。</summary>
        public bool IncludesBespokeSeat { get; }

        public PlayableStart(
            string id, string displayName, string blurb,
            FactionId playerFaction, CharacterId playerLord, CityId capital, int capitalGarrison,
            CityId offensiveTarget, FactionId targetFaction, TerrainKind targetTerrain, bool includesBespokeSeat,
            int anchorYear = 190)
        {
            Id = id;
            AnchorYear = anchorYear;
            DisplayName = displayName;
            Blurb = blurb;
            PlayerFaction = playerFaction;
            PlayerLord = playerLord;
            Capital = capital;
            CapitalGarrison = capitalGarrison;
            OffensiveTarget = offensiveTarget;
            TargetFaction = targetFaction;
            TargetTerrain = targetTerrain;
            IncludesBespokeSeat = includesBespokeSeat;
        }
    }

    /// <summary>
    /// 可选开局目录（#1）：主菜单「势力选择」的数据源。默认「汜水关太守」保持竖切原样（与既有测试/harness 单一同源），
    /// 另供数个扮演世界既有诸侯的开局——共享同一天下大盘，仅玩家席位不同 → 争霸/终局/外交自然随玩家势力重定向。
    /// 全部为不可变预设；新增开局只需在此登记一条 <see cref="PlayableStart"/>（数据驱动，运行期零改动）。
    /// </summary>
    public static class PlayableStartCatalog
    {
        /// <summary>默认：汜水关太守（第 18 独立席，锋芒指虎牢关/袁术）——竖切原局，运行期与 harness 同源。</summary>
        public static readonly PlayableStart FanshuiGovernor = new PlayableStart(
            id: "scenario-fanshui-playable", displayName: "汜水关太守",
            blurb: "受命镇守汜水关，麾下一城之地。当面袁术据虎牢关——是死守本分，还是出关问鼎？",
            playerFaction: PlayableCampaign.Player, playerLord: PlayableCampaign.Lord,
            capital: PlayableCampaign.Fanshui, capitalGarrison: 800,
            offensiveTarget: PlayableCampaign.EnemyCity, targetFaction: PlayableCampaign.Enemy,
            targetTerrain: TerrainKind.Pass, includesBespokeSeat: true);

        /// <summary>刘备·小沛：寄寓小城、志在天下，初锋芒指吕布下邳（宿敌）。</summary>
        public static readonly PlayableStart LiubeiXiaopei = new PlayableStart(
            id: "liubei-xiaopei", displayName: "刘玄德·小沛",
            blurb: "暂寄小沛，兵微将寡而志在匡扶。近有吕布据下邳虎视——先取徐州立足，方能图远。",
            playerFaction: PlayableCampaign.LiuBei, playerLord: new CharacterId("char-liubei"),
            capital: PlayableCampaign.Xiaopei, capitalGarrison: 400,
            offensiveTarget: PlayableCampaign.Xiapi, targetFaction: PlayableCampaign.LuBu,
            targetTerrain: TerrainKind.Fortified, includesBespokeSeat: false);

        /// <summary>孙策·江东：借兵起于江东，锐意开疆，初锋芒指刘表江夏。</summary>
        public static readonly PlayableStart SunceJiangdong = new PlayableStart(
            id: "sunce-jiangdong", displayName: "孙伯符·江东",
            blurb: "承父业借兵渡江，江东六郡初定，锐气正盛。西邻刘表据江夏——溯江而上，可成霸业。",
            playerFaction: PlayableCampaign.Sun, playerLord: new CharacterId("char-sunce"),
            capital: PlayableCampaign.Jianye, capitalGarrison: 700,
            offensiveTarget: PlayableCampaign.Jiangxia, targetFaction: PlayableCampaign.LiuBiao,
            targetTerrain: TerrainKind.Ford, includesBespokeSeat: false);

        /// <summary>全部可选开局（选择屏遍历序）。</summary>
        public static IReadOnlyList<PlayableStart> All { get; } = new[]
        {
            FanshuiGovernor, LiubeiXiaopei, SunceJiangdong,
        };

        /// <summary>默认开局（未选择时/harness 缺省）。</summary>
        public static PlayableStart Default => FanshuiGovernor;

        /// <summary>按 id 取开局；未知则 null。</summary>
        public static PlayableStart? ById(string id)
        {
            foreach (PlayableStart s in All) if (s.Id == id) return s;
            return null;
        }
    }
}
