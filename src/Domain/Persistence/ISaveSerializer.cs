using System;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>存档反序列化失败（格式损坏/截断/非法字段）。供加载完整性校验据此拒绝（TR-save-003）。</summary>
    public sealed class SaveFormatException : Exception
    {
        public SaveFormatException(string message) : base(message) { }
    }

    /// <summary>
    /// 存档序列化端口（ADR-0002 + ADR-0005）：Domain 定义契约，序列化为<b>显式版本化 DTO 文本</b>，
    /// <b>禁</b> Unity 序列化。量产期 Infrastructure 可提供 JSON 库实现替换；Domain 仅依赖此接口。
    /// <para>实现须满足<b>确定性</b>：同一快照 → 同一文本；<see cref="Deserialize"/>(<see cref="Serialize"/>(s)) 还原等价快照。</para>
    /// </summary>
    public interface ISaveSerializer
    {
        /// <summary>序列化为版本化 DTO 文本（确定性、规范化排序）。</summary>
        string Serialize(SaveSnapshot snapshot);

        /// <summary>从文本还原快照；格式损坏抛 <see cref="SaveFormatException"/>（不产出部分对象）。</summary>
        SaveSnapshot Deserialize(string text);
    }
}
