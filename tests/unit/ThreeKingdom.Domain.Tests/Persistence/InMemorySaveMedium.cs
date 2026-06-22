using System.Collections.Generic;
using System.IO;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// 纯内存 <see cref="ISaveMedium"/> 测试替身（test-standards：单元测试不依赖文件系统）。
    /// 支持注入失败：在指定槽写入或原子改名时抛 <see cref="IOException"/>，用于验证
    /// 「写失败保留上一份有效存档」的原子写回保证。<see cref="Move"/> 契约性原子——失败时 to 槽不变。
    /// </summary>
    public sealed class InMemorySaveMedium : ISaveMedium
    {
        private readonly Dictionary<string, string> _slots = new Dictionary<string, string>();

        /// <summary>写入这些槽名时抛异常（模拟磁盘写满）。</summary>
        public HashSet<string> FailWriteOn { get; } = new HashSet<string>();

        /// <summary>为 true 时原子改名抛异常（模拟 rename 失败）。</summary>
        public bool FailMove { get; set; }

        public bool Exists(string name) => _slots.ContainsKey(name);

        public string? Read(string name) => _slots.TryGetValue(name, out var v) ? v : null;

        public void Write(string name, string content)
        {
            if (FailWriteOn.Contains(name)) throw new IOException($"模拟写入失败：{name}");
            _slots[name] = content;
        }

        public void Move(string from, string to)
        {
            if (FailMove) throw new IOException($"模拟改名失败：{from}->{to}");
            if (!_slots.TryGetValue(from, out var v)) throw new IOException($"临时槽不存在：{from}");
            _slots[to] = v;          // 原子覆盖
            _slots.Remove(from);
        }

        public void Delete(string name) => _slots.Remove(name);

        /// <summary>测试直读槽内容（不存在返回 null）。</summary>
        public string? Peek(string name) => Read(name);
    }
}
