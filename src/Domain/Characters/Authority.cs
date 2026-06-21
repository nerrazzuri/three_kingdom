using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>需授权的命令类型（GDD_005：职责决定合法权限）。MVP 集。</summary>
    public enum CommandType
    {
        /// <summary>任命。</summary>
        Appoint = 0,

        /// <summary>派遣。</summary>
        Dispatch = 1,

        /// <summary>召回。</summary>
        Recall = 2,

        /// <summary>征调。</summary>
        Requisition = 3,

        /// <summary>外交交涉。</summary>
        Diplomacy = 4,

        /// <summary>建造。</summary>
        Construct = 5,
    }

    /// <summary>
    /// 职责授权登记（GDD_005 §Main Rules / TR-character-002）：职责 → 允许命令集（配置驱动）。
    /// 授权<b>只</b>看职责，<see cref="IsAuthorized"/> 不接受任何能力参数——<b>能力高不能绕过授权</b>（AC-1，结构性）。
    /// 不可变。
    /// </summary>
    public sealed class AuthorityRegistry
    {
        private readonly Dictionary<RoleId, HashSet<CommandType>> _allowed;

        /// <param name="allowed">职责 → 允许的命令类型集合。</param>
        public AuthorityRegistry(IReadOnlyDictionary<RoleId, IReadOnlyList<CommandType>> allowed)
        {
            if (allowed == null) throw new ArgumentNullException(nameof(allowed));
            _allowed = new Dictionary<RoleId, HashSet<CommandType>>();
            foreach (var kv in allowed)
            {
                var set = new HashSet<CommandType>();
                foreach (var cmd in kv.Value)
                {
                    if (!Enum.IsDefined(typeof(CommandType), cmd))
                        throw new ArgumentException($"未定义的命令类型：{cmd}。", nameof(allowed));
                    set.Add(cmd);
                }
                _allowed[kv.Key] = set;
            }
        }

        /// <summary>
        /// 该职责是否被授权执行该命令。仅依职责判定——无能力旁路（能力不补权限，AC-1）。
        /// 未登记职责视为无任何权限。
        /// </summary>
        public bool IsAuthorized(RoleId role, CommandType command)
            => _allowed.TryGetValue(role, out var set) && set.Contains(command);
    }
}
