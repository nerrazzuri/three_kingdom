namespace ThreeKingdom.Domain.Characters
{
    /// <summary>忠诚倾向（GDD_025 R3，隐秘定性档，<b>不显示为数字</b>）：驱动人心杠杆策反门。</summary>
    public enum LoyaltyLeaning
    {
        /// <summary>忠义：食君之禄、几乎不可策反。</summary>
        Loyal = 0,
        /// <summary>安分：守成，无二心但也无大志。</summary>
        Content = 1,
        /// <summary>摇摆：可乘之机存在。</summary>
        Wavering = 2,
        /// <summary>怀贰：素怀异志、极易策反。</summary>
        Disloyal = 3,
    }

    /// <summary>野心（GDD_025 R3，隐秘定性档，不显示）：喂策反可乘度 + 自立倾向。</summary>
    public enum Ambition
    {
        /// <summary>无争：安于本分。</summary>
        None = 0,
        /// <summary>有志：欲有作为。</summary>
        Aspiring = 1,
        /// <summary>雄图：志在方面。</summary>
        Grand = 2,
        /// <summary>狼顾：藏而不露、伺机问鼎。</summary>
        Wolfish = 3,
    }

    /// <summary>羁绊类型（GDD_025 R4）：同场触发协同/崩解。接 GDD_006 关系。</summary>
    public enum BondType
    {
        /// <summary>血脉：宗族父子（曹氏夏侯氏）。</summary>
        Blood = 0,
        /// <summary>师徒：传承（诸葛→姜维）。</summary>
        Mentor = 1,
        /// <summary>知己：生死之交（刘关张）。</summary>
        Kindred = 2,
        /// <summary>仇怨：宿敌（曹操&吕布）。</summary>
        Feud = 3,
    }

    /// <summary>人生阶段（GDD_025 R7 时代分段）：决定当前标签集（接 GDD_015 事件切换）。</summary>
    public enum EraStage
    {
        /// <summary>青年。</summary>
        Young = 0,
        /// <summary>盛年。</summary>
        Prime = 1,
        /// <summary>晚年。</summary>
        Late = 2,
    }
}
