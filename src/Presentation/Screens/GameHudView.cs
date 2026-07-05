namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 顶栏聚合视图（UI 层接入的<b>单一绑定对象</b>）：把一屏 HUD 需要的顶层状态——纪元/一生/生涯/君命/行动容量/
    /// 争霸——聚合成一个扁平只读投影。UI 只需绑定此一个对象即得全部顶栏，无需散取十个方法。纯数据、无 Unity 依赖。
    /// 反全知：不含任何敌方真值或隐藏量级档；玩家自身生涯/寿数亦只给定性档（阶段/头衔），不给隐藏细节。
    /// </summary>
    public sealed class GameHudView
    {
        // 纪元 / 一生
        public int Year { get; }
        public string SeasonLabel { get; }
        public int Age { get; }
        public string LifePhaseLabel { get; }
        public bool IsLifeOver { get; }

        // 生涯
        public string RankTitle { get; }
        public int Merit { get; }
        public int Renown { get; }
        public bool IsUnaffiliated { get; }
        public bool HasRebelled { get; }

        // 君命（一句话）
        public string MissionOrder { get; }

        // 行动容量（手令）
        public int ActionsInFlight { get; }
        public int ActionCapacity { get; }

        // 争霸
        public int PlayerCities { get; }
        public int AliveRivals { get; }
        public bool IsEliminated { get; }

        internal GameHudView(
            int year, string seasonLabel, ArrivalLifeView life,
            CareerView career, bool hasRebelled, string missionOrder,
            int actionsInFlight, int actionCapacity,
            int playerCities, int aliveRivals, bool isEliminated)
        {
            Year = year;
            SeasonLabel = seasonLabel;
            Age = life.Age;
            LifePhaseLabel = life.PhaseLabel;
            IsLifeOver = life.IsOver;
            RankTitle = career.RankTitle;
            Merit = career.Merit;
            Renown = career.Renown;
            IsUnaffiliated = career.IsUnaffiliated;
            HasRebelled = hasRebelled;
            MissionOrder = missionOrder;
            ActionsInFlight = actionsInFlight;
            ActionCapacity = actionCapacity;
            PlayerCities = playerCities;
            AliveRivals = aliveRivals;
            IsEliminated = isEliminated;
        }
    }
}
