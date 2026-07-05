using System.Collections.Generic;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Console
{
    /// <summary>控制台用内存存档介质（ISaveMedium）：字典背书，支持原子改名语义。非文件——退出即失。</summary>
    public sealed class MemorySaveMedium : ISaveMedium
    {
        private readonly Dictionary<string, string> _slots = new Dictionary<string, string>();

        public bool Exists(string name) => _slots.ContainsKey(name);
        public string? Read(string name) => _slots.TryGetValue(name, out string? v) ? v : null;
        public void Write(string name, string content) => _slots[name] = content;
        public void Delete(string name) => _slots.Remove(name);

        public void Move(string from, string to)
        {
            if (!_slots.TryGetValue(from, out string? content)) return;
            _slots[to] = content;
            _slots.Remove(from);
        }
    }
}
