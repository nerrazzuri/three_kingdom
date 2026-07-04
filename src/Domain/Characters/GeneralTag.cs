namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 武将气质标签（GDD_025 R2）：<b>不是数值</b>，是"条件齐才发作"的{触发条件+效果}。
    /// 复用天时/地利/人和门（GDD_010/021），门不齐则不发作——与火攻/伏兵同理。专精化，无全能（R5）。
    /// 部分标签兼作隐秘心/负面信号（如 Fickle=反复、Wolflook=狼顾、Arrogant=傲物）。
    /// </summary>
    public enum GeneralTag
    {
        // ---- 正面/战斗气质 ----
        /// <summary>莽撞：首击威势大增，若被避则下回合自陷混乱。</summary>
        Reckless = 0,
        /// <summary>威压：对无名之将必中且增伤（关羽）。</summary>
        Awe = 1,
        /// <summary>孤胆：身边无友军时暴走（赵云）。</summary>
        LoneValor = 2,
        /// <summary>傲骨：兵优则免伤、兵劣则迟滞（关羽）。</summary>
        IronBones = 3,
        /// <summary>夜袭先手：夜间突袭有先手必中（张辽）。</summary>
        NightRaider = 4,
        /// <summary>水战：水域作战大增益、可劫粮（周瑜/甘宁）。</summary>
        Naval = 5,
        /// <summary>骑锋：平原骑兵机动翻倍，入山林受阻（马超）。</summary>
        Cavalry = 6,
        /// <summary>善守：守城/持久战顶级，野战平平（郝昭/曹仁）。</summary>
        Defender = 7,
        /// <summary>诡谋：伏击/火攻/离间等诡策门更易成型（贾诩/法正）。</summary>
        Cunning = 8,
        /// <summary>远图：长期战略/外交布局（诸葛/鲁肃）。</summary>
        Strategist = 9,
        /// <summary>仁德：民心/招揽感召（刘备）。</summary>
        Benevolent = 10,

        // ---- 负面特质（R6）----
        /// <summary>傲物：难被招揽、难容同僚（关羽/祢衡）。</summary>
        Arrogant = 20,
        /// <summary>刚愎：拒纳良谏，谋略类战法衰减（袁绍）。</summary>
        Stubborn = 21,
        /// <summary>嗜杀：占城民心流失（董卓/马超）。</summary>
        Bloodthirsty = 22,
        /// <summary>优柔：战场决策迟滞、错失战机（刘表/刘璋）。</summary>
        Hesitant = 23,
        /// <summary>反复：背主无信，极易策反（吕布/孟达）。</summary>
        Fickle = 24,
        /// <summary>狼顾：藏而不露的雄图野心（司马懿）。</summary>
        Wolflook = 25,
    }
}
