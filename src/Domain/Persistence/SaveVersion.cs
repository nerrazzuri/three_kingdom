using System;
using System.Globalization;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 存档与当前运行版本的兼容关系（ADR-0005 §3/§4）。三类：
    /// <list type="bullet">
    ///   <item><see cref="Compatible"/>：版本相同，直接加载，无需迁移。</item>
    ///   <item><see cref="Migratable"/>：存档低于当前（同主版本低次版本，或更低主版本），经逆序逐版迁移链升级后加载。</item>
    ///   <item><see cref="IncompatibleNewer"/>：存档高于当前（更高主版本，或同主版本更高次版本），明确拒绝，<b>不静默降级</b>。</item>
    /// </list>
    /// </summary>
    public enum SaveCompatibility
    {
        /// <summary>版本一致，可直接加载。</summary>
        Compatible,

        /// <summary>存档较旧，可经迁移链升级后加载。</summary>
        Migratable,

        /// <summary>存档来自更新版本，不兼容（拒绝，不静默降级）。</summary>
        IncompatibleNewer,
    }

    /// <summary>
    /// 存档 schema 版本值对象（ADR-0005：SaveVersion 表达 schema 兼容关系）。
    /// 纯 Domain 值对象，<b>无任何 IO</b>（序列化/迁移链落 epic-009 / Infrastructure）。
    /// 格式 <c>"major.minor"</c>，主/次为非负整数。不可变；相等性与顺序按值。
    /// </summary>
    public readonly struct SaveVersion : IEquatable<SaveVersion>, IComparable<SaveVersion>
    {
        /// <summary>主版本号（不兼容性边界：主版本不同不可平移加载）。</summary>
        public int Major { get; }

        /// <summary>次版本号（同主版本下的可迁移演进）。</summary>
        public int Minor { get; }

        /// <summary>由主/次版本构造；任一为负抛 <see cref="ArgumentOutOfRangeException"/>。</summary>
        public SaveVersion(int major, int minor)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), "主版本不可为负。");
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), "次版本不可为负。");
            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// 解析 <c>"major.minor"</c>。非法字符串（空/格式错/含符号或空白/溢出）抛 <see cref="FormatException"/>，不产出对象。
        /// </summary>
        public static SaveVersion Parse(string text)
        {
            if (!TryParse(text, out var version))
                throw new FormatException($"非法 SaveVersion：「{text ?? "<null>"}」。要求 \"major.minor\"，主/次为非负整数。");
            return version;
        }

        /// <summary>尝试解析；非法返回 false 且不产出对象（稳定失败，无副作用）。</summary>
        public static bool TryParse(string text, out SaveVersion version)
        {
            version = default;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var parts = text.Split('.');
            if (parts.Length != 2) return false;
            if (!TryParseComponent(parts[0], out int major)) return false;
            if (!TryParseComponent(parts[1], out int minor)) return false;

            version = new SaveVersion(major, minor);
            return true;
        }

        /// <summary>纯数字非负整数解析：拒绝空、符号、空白与溢出（NumberStyles.None）。</summary>
        private static bool TryParseComponent(string s, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c < '0' || c > '9') return false; // 显式拒绝 '+'/'-'/空白/其它
            return int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// 判定本存档版本相对 <paramref name="current"/>（当前运行版本）的加载兼容关系（ADR-0005 §4）。
        /// </summary>
        public SaveCompatibility ClassifyForLoad(SaveVersion current)
        {
            if (Major > current.Major) return SaveCompatibility.IncompatibleNewer;
            if (Major < current.Major) return SaveCompatibility.Migratable;
            if (Minor > current.Minor) return SaveCompatibility.IncompatibleNewer;
            if (Minor < current.Minor) return SaveCompatibility.Migratable;
            return SaveCompatibility.Compatible;
        }

        /// <summary>是否可加载（兼容或可迁移）；更新版本的存档不可加载（TR-save-003）。</summary>
        public bool CanLoadInto(SaveVersion current) => ClassifyForLoad(current) != SaveCompatibility.IncompatibleNewer;

        public int CompareTo(SaveVersion other)
        {
            int byMajor = Major.CompareTo(other.Major);
            return byMajor != 0 ? byMajor : Minor.CompareTo(other.Minor);
        }

        public bool Equals(SaveVersion other) => Major == other.Major && Minor == other.Minor;
        public override bool Equals(object? obj) => obj is SaveVersion other && Equals(other);
        public override int GetHashCode() => (Major * 397) ^ Minor;

        public static bool operator ==(SaveVersion a, SaveVersion b) => a.Equals(b);
        public static bool operator !=(SaveVersion a, SaveVersion b) => !a.Equals(b);
        public static bool operator <(SaveVersion a, SaveVersion b) => a.CompareTo(b) < 0;
        public static bool operator >(SaveVersion a, SaveVersion b) => a.CompareTo(b) > 0;
        public static bool operator <=(SaveVersion a, SaveVersion b) => a.CompareTo(b) <= 0;
        public static bool operator >=(SaveVersion a, SaveVersion b) => a.CompareTo(b) >= 0;

        public override string ToString() => Major.ToString(CultureInfo.InvariantCulture) + "." + Minor.ToString(CultureInfo.InvariantCulture);
    }
}
