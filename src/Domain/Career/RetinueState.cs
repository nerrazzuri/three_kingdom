using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 部曲/僚属权威状态（GDD_014 §Data Model：RetinueState / TR-career-001）。
    /// 不可变聚合：核心僚属列表（含好感）+ 已任免官职位（<see cref="OfficeRole"/> → 僚属）。
    /// 构造时校验：僚属 ID 不重复；每个官职位的持有者须为列表内成员。失败即抛、无部分写入。
    /// 变更经 <see cref="CareerStateService"/> 命令路径产出新实例，<b>不</b>就地修改。
    /// <para>
    /// <b>规范遍历顺序</b>（哈希用，ADR-0004）：僚属按 <see cref="CharacterId"/> 序数升序；
    /// 官职位按 <see cref="OfficeRole"/> 枚举序数升序。故同一逻辑状态恒产同一哈希字节序。
    /// </para>
    /// </summary>
    public sealed class RetinueState
    {
        private readonly RetinueMember[] _members;          // 已按 CharacterId 序数升序
        private readonly OfficeRole[] _offices;             // 已按枚举序数升序，与 _holders 平行
        private readonly CharacterId[] _holders;

        /// <summary>核心僚属列表（按 ID 序数升序，稳定遍历）。</summary>
        public IReadOnlyList<RetinueMember> Members => _members;

        /// <summary>空部曲（无僚属、无任免）。</summary>
        public static RetinueState Empty { get; } = new RetinueState(
            Array.Empty<RetinueMember>(),
            Array.Empty<KeyValuePair<OfficeRole, CharacterId>>());

        /// <summary>
        /// 构造部曲状态。<paramref name="members"/> 不可含重复 ID；
        /// <paramref name="assignments"/> 每个官职位的持有者须在 <paramref name="members"/> 内。
        /// </summary>
        public RetinueState(
            IReadOnlyList<RetinueMember> members,
            IReadOnlyList<KeyValuePair<OfficeRole, CharacterId>> assignments)
        {
            if (members is null) throw new ArgumentNullException(nameof(members));
            if (assignments is null) throw new ArgumentNullException(nameof(assignments));

            var sortedMembers = new List<RetinueMember>(members);
            sortedMembers.Sort((a, b) => string.CompareOrdinal(a.Character.Value, b.Character.Value));
            for (int i = 1; i < sortedMembers.Count; i++)
            {
                if (sortedMembers[i].Character == sortedMembers[i - 1].Character)
                    throw new ArgumentException($"僚属 ID 重复：{sortedMembers[i].Character}。", nameof(members));
            }
            _members = sortedMembers.ToArray();

            // 官职位：每职位至多一人；持有者须为成员；按枚举序数稳定排序。
            var byRole = new SortedDictionary<OfficeRole, CharacterId>();
            foreach (KeyValuePair<OfficeRole, CharacterId> kv in assignments)
            {
                if (!IsMember(kv.Value))
                    throw new ArgumentException($"官职位 {kv.Key} 的持有者 {kv.Value} 不在僚属列表。", nameof(assignments));
                byRole[kv.Key] = kv.Value; // 同职位后者覆盖前者（构造期容错；命令期由服务控制）
            }
            _offices = new OfficeRole[byRole.Count];
            _holders = new CharacterId[byRole.Count];
            int idx = 0;
            foreach (KeyValuePair<OfficeRole, CharacterId> kv in byRole)
            {
                _offices[idx] = kv.Key;
                _holders[idx] = kv.Value;
                idx++;
            }
        }

        /// <summary>该人物是否为僚属成员。</summary>
        public bool IsMember(CharacterId character)
        {
            foreach (RetinueMember m in _members)
                if (m.Character == character) return true;
            return false;
        }

        /// <summary>查询某官职位的持有者；未任免则返回 null。</summary>
        public CharacterId? Holder(OfficeRole role)
        {
            for (int i = 0; i < _offices.Length; i++)
                if (_offices[i] == role) return _holders[i];
            return null;
        }

        /// <summary>已任免官职位（按枚举序数升序），(职位,持有者) 对。</summary>
        public IReadOnlyList<KeyValuePair<OfficeRole, CharacterId>> Assignments()
        {
            var list = new List<KeyValuePair<OfficeRole, CharacterId>>(_offices.Length);
            for (int i = 0; i < _offices.Length; i++)
                list.Add(new KeyValuePair<OfficeRole, CharacterId>(_offices[i], _holders[i]));
            return list;
        }

        /// <summary>
        /// 任免某官职位给指定僚属，产出新状态（仅供命令服务使用，维持单一写路径）。
        /// 持有者非成员则抛——调用方（服务）应先校验并返回稳定错误码，不依赖此异常做控制流。
        /// </summary>
        internal RetinueState WithOffice(OfficeRole role, CharacterId holder)
        {
            var assignments = new List<KeyValuePair<OfficeRole, CharacterId>>();
            for (int i = 0; i < _offices.Length; i++)
            {
                if (_offices[i] == role) continue; // 替换旧任免
                assignments.Add(new KeyValuePair<OfficeRole, CharacterId>(_offices[i], _holders[i]));
            }
            assignments.Add(new KeyValuePair<OfficeRole, CharacterId>(role, holder));
            return new RetinueState(_members, assignments);
        }

        /// <summary>替换某僚属好感产出新态（忠诚经营，仅供服务命令路径）。非成员则原样返回。</summary>
        internal RetinueState WithMemberAffinity(CharacterId character, FixedPoint affinity)
        {
            if (!IsMember(character)) return this;
            var members = new List<RetinueMember>();
            foreach (RetinueMember m in _members)
                members.Add(m.Character == character ? new RetinueMember(character, affinity) : m);
            return new RetinueState(members, Assignments());
        }

        /// <summary>移除某僚属（被挖角/叛离）产出新态，并撤销其所任官职（仅供服务命令路径）。非成员则原样返回。</summary>
        internal RetinueState WithoutMember(CharacterId character)
        {
            if (!IsMember(character)) return this;
            var members = new List<RetinueMember>();
            foreach (RetinueMember m in _members)
                if (m.Character != character) members.Add(m);
            var assignments = new List<KeyValuePair<OfficeRole, CharacterId>>();
            for (int i = 0; i < _offices.Length; i++)
                if (_holders[i] != character) assignments.Add(new KeyValuePair<OfficeRole, CharacterId>(_offices[i], _holders[i]));
            return new RetinueState(members, assignments);
        }

        /// <summary>撤销某官职位（保留僚属，仅去任免）产出新态（仅供服务命令路径）。无此任免则原样返回。</summary>
        internal RetinueState WithoutOffice(OfficeRole role)
        {
            if (Holder(role) == null) return this;
            var assignments = new List<KeyValuePair<OfficeRole, CharacterId>>();
            for (int i = 0; i < _offices.Length; i++)
                if (_offices[i] != role) assignments.Add(new KeyValuePair<OfficeRole, CharacterId>(_offices[i], _holders[i]));
            return new RetinueState(_members, assignments);
        }

        /// <summary>
        /// 以规范顺序追加到状态哈希（ADR-0004）。顺序：成员数 → 各成员(ID 长度+字符, 好感.Raw)
        /// → 任免数 → 各任免((int)职位, 持有者 ID 长度+字符)，全部按上述稳定排序遍历。
        /// </summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append(_members.Length);
            foreach (RetinueMember m in _members)
            {
                AppendString(hasher, m.Character.Value);
                hasher.Append(m.Affinity);
            }
            hasher.Append(_offices.Length);
            for (int i = 0; i < _offices.Length; i++)
            {
                hasher.Append((int)_offices[i]);
                AppendString(hasher, _holders[i].Value);
            }
        }

        private static void AppendString(StateHasher hasher, string value)
        {
            hasher.Append(value.Length);
            foreach (char c in value) hasher.Append((int)c);
        }
    }
}
