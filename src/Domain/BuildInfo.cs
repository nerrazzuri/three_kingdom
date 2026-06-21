namespace ThreeKingdom.Domain
{
    /// <summary>
    /// 框架确认占位类型（epic-001 story-001 验收：「一个示例 Domain 类型 + 其示例测试通过，确认框架可用」）。
    /// 用途仅为证明纯 C# Domain 程序集可被 <c>dotnet test</c> 在无 Unity 运行时下单元测试。
    /// 不含任何 gameplay 逻辑；待 epic-001 story-002 起的真实 Domain 类型（定点/随机流/配置/SaveVersion）落地后，
    /// 本类型可移除或替换。**禁止**在此引入 UnityEngine 引用（ADR-0002 / technical-preferences 禁则）。
    /// </summary>
    public static class BuildInfo
    {
        /// <summary>当前 Domain 程序集的占位版本标记（非 SaveVersion；后者由 story-004 定义）。</summary>
        public const string DomainMarker = "three-kingdom-domain";

        /// <summary>纯函数回声——确认 Domain 程序集可加载且确定性（同输入→同输出）。</summary>
        public static string Echo(string value) => value;
    }
}
