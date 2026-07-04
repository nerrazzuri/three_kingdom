namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 兵法复盘标签（GDD_010 §Formula 4/7/8/9 / TR-battle-002）。
    /// <b>仅用于事件后复盘识别</b>，<b>绝无</b>同名无条件执行按钮——兵法是多条件涌现结果。
    /// </summary>
    public enum TacticTag
    {
        /// <summary>诱敌/假退伏击（受控撤退保形 + 敌追击 + 伏兵突然性）。</summary>
        FeintAmbush = 0,

        /// <summary>断粮疲敌（切断补给 + 持续若干时段 + 敌士气/疲劳跨阈值）。</summary>
        SupplyExhaustion = 1,

        /// <summary>守城待变（守住 + 外援抵达 + 撑过期限）。</summary>
        HoldUntilRelief = 2,

        /// <summary>夜袭（夜间 + 隐蔽成功 + 守方未察觉 + 袭方军纪达标）——执行手段，可与其他链组合。</summary>
        NightRaid = 3,

        /// <summary>火攻（干燥天时 + 敌暴露于易燃地形 + 智将纵火）——乌巢烧粮/赤壁烧船，多条件涌现非按钮。</summary>
        FireAttack = 4,
    }

    /// <summary>
    /// 可观察条件（GDD_010 兵法条件链的必要条件原子）。复盘识别据这些<b>事后</b>事实判定，
    /// 缺任一必要条件则对应兵法不涌现、不打标签（TR-battle-002 负向不变量）。
    /// </summary>
    public enum TacticCondition
    {
        // 诱敌/假退伏击
        ControlledRetreatKeptFormation = 0,
        EnemyPursued = 1,
        AmbushSurprise = 2,

        // 断粮疲敌
        SupplyLineCut = 10,
        ShortageReachedGrace = 11,
        EnemyCohesionCrossedThreshold = 12,

        // 守城待变
        HeldPosition = 20,
        ReliefArrived = 21,
        SurvivedDeadline = 22,

        // 夜袭
        IsNight = 30,
        StealthSuccess = 31,
        DefenderUnaware = 32,
        RaiderDisciplineMet = 33,

        // 火攻
        DryField = 40,              // 干燥天时（晴，无雨无雾）——火势可燃
        EnemyExposedToFire = 41,    // 敌暴露于易燃地形（粮营/连营/林莽）
        FireIgnited = 42,           // 智将纵火（己方在场 + 智谋达标）
    }
}
